using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net;
using YamlDotNet.Serialization.NamingConventions;
using System.Text.RegularExpressions;
using MMR_Tracker_V3.TrackerObjects;

namespace MMR_Tracker_V3.OtherGames.OOTMMRCOMBO
{

    public static class ReadAndParseData
    {
        public class MMROOTLocation
        {
            public string location;
            public string scene;
            public string item;
        }

        public class MMROOTLogicEntry
        {
            public string dungeon;
            public string boss;
            public string region;
            public Dictionary<string, string> exits =  new Dictionary<string, string>();
            public Dictionary<string, string> locations = new Dictionary<string, string>();
            public Dictionary<string, string> events = new Dictionary<string, string>();
            public Dictionary<string, string> gossip = new Dictionary<string, string>();
        }

        public static void CreateFiles(out TrackerObjects.MMRData.LogicFile Logic, out TrackerObjects.LogicDictionaryData.LogicDictionary dictionary)
        {
            string TestFolder = References.TestingPaths.GetDevTestingPath();
            string FinalLogicFile = Path.Combine(TestFolder, @"OOTMMLogic.json");
            string FinalDictFile = Path.Combine(TestFolder, @"OOTMMDict.json");

            AddEntriesFromItemPools(out TrackerObjects.MMRData.LogicFile LogicFile, out TrackerObjects.LogicDictionaryData.LogicDictionary dictionaryFile);
            AddEntriesFromLogicFiles(LogicFile, dictionaryFile);

            CleanLogicAndParse(LogicFile);

            AddVariablesandOptions(dictionaryFile);

            Logic = LogicFile;
            dictionary = dictionaryFile;
            File.WriteAllText(FinalLogicFile, JsonConvert.SerializeObject(LogicFile, Testing._NewtonsoftJsonSerializerOptions));
            File.WriteAllText(FinalDictFile, JsonConvert.SerializeObject(dictionaryFile, Testing._NewtonsoftJsonSerializerOptions));


        }

        private static void AddVariablesandOptions(LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            LogicDictionaryData.TrackerVariable MM_Masks = new LogicDictionaryData.TrackerVariable();
            MM_Masks.Static = true;
            MM_Masks.Value =new List<string> {
                "MM_MASK_POSTMAN",
                "MM_MASK_ALL_NIGHT",
                "MM_MASK_BLAST",
                "MM_MASK_STONE",
                "MM_MASK_GREAT_FAIRY",
                "MM_MASK_KEATON",
                "MM_MASK_BREMEN",
                "MM_MASK_BUNNY",
                "MM_MASK_DON_GERO",
                "MM_MASK_SCENTS",
                "MM_MASK_ROMANI",
                "MM_MASK_TROUPE_LEADER",
                "MM_MASK_KAFEI",
                "MM_MASK_COUPLE",
                "MM_MASK_TRUTH",
                "MM_MASK_KAMARO",
                "MM_MASK_GIBDO",
                "MM_MASK_GARO",
                "MM_MASK_CAPTAIN",
                "MM_MASK_GIANT"
            };
            MM_Masks.Name = "MM Masks";
            MM_Masks.ID = "MM_MASKS";
            dictionaryFile.Variables.Add(MM_Masks);

            OptionData.TrackerOption ageFilter = new OptionData.TrackerOption();
            ageFilter.ID = "age_filter";
            ageFilter.DisplayName = "Age Filter";
            ageFilter.CurrentValue = "both";
            ageFilter.CreateSimpleValues(new string[] { "both", "adult", "child" });
            dictionaryFile.Options.Add(ageFilter);

            OptionData.TrackerOption DoorOfTime = new OptionData.TrackerOption();
            DoorOfTime.ID = "doorOfTime";
            DoorOfTime.DisplayName = "Door Of Time";
            DoorOfTime.CurrentValue = "closed";
            DoorOfTime.CreateSimpleValues(new string[] { "open", "closed" });
            dictionaryFile.Options.Add(DoorOfTime);

            OptionData.TrackerOption GanonBossKey = new OptionData.TrackerOption();
            GanonBossKey.ID = "ganonBossKey";
            GanonBossKey.DisplayName = "Ganon's Boss Key";
            GanonBossKey.CurrentValue = "ganon";
            GanonBossKey.CreateSimpleValues(new string[] { "removed", "vanilla", "ganon", "anywhere" });
            dictionaryFile.Options.Add(GanonBossKey);

            OptionData.TrackerOption SmallKey = new OptionData.TrackerOption();
            SmallKey.ID = "smallKeyShuffle";
            SmallKey.DisplayName = "Small Key Shuffle";
            SmallKey.CurrentValue = "anywhere";
            SmallKey.CreateSimpleValues(new string[] { "ownDungeon", "anywhere" });
            dictionaryFile.Options.Add(SmallKey);

            //Game Clear
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "Game_Clear", RequiredItems = new List<string> { "OOT_GANON", "MM_MAJORA" } });
            dictionaryFile.MacroList.Add(new LogicDictionaryData.DictionaryMacroEntry { ID = "Game_Clear", Name = "Both Games Cleared" });
            dictionaryFile.WinCondition = "Game_Clear";

            //Temp Workaround for a typo in logic
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "MM_ZORA", RequiredItems = new List<string> { "MM_MASK_ZORA" } });

        }

        public static void CleanLogicAndParse(TrackerObjects.MMRData.LogicFile LogicFile)
        {
            string MMLogic = Path.Combine(References.TestingPaths.GetDevTestingPath(), "core-develop", "data", "mm", "world");
            string OOTLogic = Path.Combine(References.TestingPaths.GetDevTestingPath(), "core-develop", "data", "oot", "world");
            string MMOOTCodeMMMacros = Path.Combine(References.TestingPaths.GetDevCodePath(), @"MMR Tracker V3", "OtherGames", "OOTMMRCOMBO", @"MMMacroOverride.json");
            string MMOOTCodeOOTMacros = Path.Combine(References.TestingPaths.GetDevCodePath(), @"MMR Tracker V3", "OtherGames", "OOTMMRCOMBO", @"OOTMacroOverride.json");
            Dictionary<string, string> MMMacros = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(MMOOTCodeMMMacros));
            Dictionary<string, string> OOTMacros = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(MMOOTCodeOOTMacros));
            var files = Directory.GetFiles(MMLogic).Concat(Directory.GetFiles(OOTLogic)).ToList();

            Dictionary<string, List<string>> FoundFunctions = new Dictionary<string, List<string>>();
            foreach(var file in files)
            {
                string Game = "OOT";
                string OpositeGame = Game == "OOT" ? "MM" : "OOT";
                if (file.Contains(@"\mm\")) { Game = "MM"; }
                var FileOBJ = JsonConvert.DeserializeObject<Dictionary<string, MMROOTLogicEntry>>(LogicStringParser.ConvertYamlStringToJsonString(File.ReadAllText(file)));
                foreach(var key in FileOBJ.Keys)
                {
                    foreach(var i in FileOBJ[key].locations?.Keys.ToArray()??new string[0])
                    {
                        var LogicFileEntry = LogicFile.Logic.Find(x => x.Id == i || x.Id == $"{Game} {i}" || x.Id == $"{Game}_{i}");
                        if (LogicFileEntry is not null)
                        {
                            string Logic = $"({FileOBJ[key].locations[i]}) && {Game} {key}";
                            LogicFileEntry.ConditionalItems = ParselogicLine(Logic, Game);
                            logicCleaner.RemoveRedundantConditionals(LogicFileEntry);
                            logicCleaner.MakeCommonConditionalsRequirements(LogicFileEntry);
                        }
                    }
                    foreach (var i in FileOBJ[key].exits?.Keys.ToArray()??new string[0])
                    {
                        string TrueAreaName = $"{Game} {key}";
                        string TrueExitName = i.StartsWith($"{OpositeGame} ") || i.StartsWith($"{Game} ") ? $"{i}" : $"{Game} {i}";
                        string FullexitName = $"{TrueAreaName} => {TrueExitName}";
                        var LogicFileEntry = LogicFile.Logic.Find(x => x.Id == FullexitName);
                        if (LogicFileEntry is not null)
                        {
                            string Logic = $"({FileOBJ[key].exits[i]}) && {Game} {key}";
                            if (FullexitName == "OOT SPAWN => OOT Link's House") { Logic = FileOBJ[key].exits[i]; }
                            LogicFileEntry.ConditionalItems = ParselogicLine(Logic, Game);
                            logicCleaner.RemoveRedundantConditionals(LogicFileEntry);
                            logicCleaner.MakeCommonConditionalsRequirements(LogicFileEntry);
                        }

                    }
                    foreach (var i in FileOBJ[key].events?.Keys.ToArray()??new string[0])
                    {
                        var LogicFileEntry = LogicFile.Logic.Find(x => x.Id == i || x.Id == $"{Game} {i}" || x.Id == $"{Game}_{i}");
                        if (LogicFileEntry is not null)
                        {
                            string Logic = $"(({FileOBJ[key].events[i]}) && {Game} {key})";
                            if (LogicFileEntry.ConditionalItems.Any() || LogicFileEntry.RequiredItems.Any())
                            {
                                Debug.WriteLine($"{key} {i} Duplicate Logic Entry");
                                logicCleaner.MoveRequirementsToConditionals(LogicFileEntry);
                                string CurrentLogic = $" || ({string.Join(" || ", LogicFileEntry.ConditionalItems.Select(x => string.Join(" && ", x)))})";
                                Logic += CurrentLogic;
                            }
                            LogicFileEntry.ConditionalItems = ParselogicLine(Logic, Game);
                            logicCleaner.RemoveRedundantConditionals(LogicFileEntry);
                            logicCleaner.MakeCommonConditionalsRequirements(LogicFileEntry);
                        }
                    }
                    foreach (var i in FileOBJ[key].gossip?.Keys.ToArray()??new string[0])
                    {
                        var LogicFileEntry = LogicFile.Logic.Find(x => x.Id == i || x.Id == $"{Game} {i}" || x.Id == $"{Game}_{i}");
                        if (LogicFileEntry is not null)
                        {
                            string Logic = $"({FileOBJ[key].gossip[i]}) && {Game} {key}";
                            LogicFileEntry.ConditionalItems = ParselogicLine(Logic, Game);
                            logicCleaner.RemoveRedundantConditionals(LogicFileEntry);
                            logicCleaner.MakeCommonConditionalsRequirements(LogicFileEntry);
                        }
                    }
                }
            }
            foreach (var key in MMMacros.Keys)
            {
                var LogicFileEntry = LogicFile.Logic.Find(x => x.Id == key || x.Id == $"MM {key}" || x.Id == $"MM_{key}");
                if (LogicFileEntry is not null)
                {
                    string Logic = $"({MMMacros[key]})";
                    LogicFileEntry.ConditionalItems = ParselogicLine(Logic, "MM");
                    logicCleaner.RemoveRedundantConditionals(LogicFileEntry);
                    logicCleaner.MakeCommonConditionalsRequirements(LogicFileEntry);
                }
            }
            foreach (var key in OOTMacros.Keys)
            {
                var LogicFileEntry = LogicFile.Logic.Find(x => x.Id == key || x.Id == $"OOT {key}" || x.Id == $"OOT_{key}");
                if (LogicFileEntry is not null)
                {
                    string Logic = $"({OOTMacros[key]})";
                    LogicFileEntry.ConditionalItems = ParselogicLine(Logic, "OOT");
                    logicCleaner.RemoveRedundantConditionals(LogicFileEntry);
                    logicCleaner.MakeCommonConditionalsRequirements(LogicFileEntry);
                }
            }
        }

        public static List<List<string>> ParselogicLine(string Line, string Game)
        {
            string MMOOTCodeMMMacros = Path.Combine(References.TestingPaths.GetDevCodePath(), @"MMR Tracker V3", "OtherGames", "OOTMMRCOMBO", @"MMMacroOverride.json");
            string MMOOTCodeOOTMacros = Path.Combine(References.TestingPaths.GetDevCodePath(), @"MMR Tracker V3", "OtherGames", "OOTMMRCOMBO", @"OOTMacroOverride.json");
            Dictionary<string, string> MMMacros = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(MMOOTCodeMMMacros));
            Dictionary<string, string> OOTMacros = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(MMOOTCodeOOTMacros));

            LogicStringParser.OperatorList operators = new LogicStringParser.OperatorList(new string[] { "&&" }, new string[] { "||" });
            string CleanLine = ParseFunction(Line, Game);
            List<List<string>> logic = LogicStringParser.ConvertLogicStringToConditional(CleanLine, operators);

            List<List<string>> finallogic = new List<List<string>>();

            foreach(var CondSet in logic)
            {
                List<string> newCondSet = new List<string>();
                foreach (var con in CondSet)
                {
                    if (Game == "MM")
                    {
                        if (!con.StartsWith("MM") && MMMacros.ContainsKey($"{con}")) { newCondSet.Add($"MM_{con}"); }
                        else { newCondSet.Add(con); }
                    }
                    else
                    {
                        if (!con.StartsWith("OOT") && OOTMacros.ContainsKey($"{con}")) { newCondSet.Add($"OOT_{con}"); }
                        else { newCondSet.Add(con); }
                    }
                }
                finallogic.Add(newCondSet);
            }

            return finallogic;
        }

        public static string ParseFunction(string LogicLine, string Game)
        {
            string line = LogicLine;
            var FunctionsFound = LogicStringParser.GetFunctionsFromLogicString(line);

            while (FunctionsFound.Any())
            {
                var f = FunctionsFound[0];
                switch (f.Funtion)
                {
                    case "trick":
                        line = line.Replace(f.ToString(), $"{f.ParametersTrimmed}");
                        break;
                    case "has":
                    case "event":
                        line = line.Replace(f.ToString(), $"{Game}_{f.ParametersTrimmed}");
                        break;
                    case "adult_trade":
                    case "can_ride_bean":
                        line = line.Replace(f.ToString(), $"{Game}_is_adult && {Game}_{f.ParametersTrimmed}");
                        break;
                    case "masks":
                        line = line.Replace(f.ToString(), $"MM_MASKS, {f.ParametersTrimmed}");
                        break;
                    case "can_play":
                        line = line.Replace(f.ToString(), $"has(OCARINA) && has({f.ParametersTrimmed})");
                        break;
                    case "has_small_keys_fire":
                        int Keys = int.Parse(f.ParametersTrimmed);
                        line = line.Replace(f.ToString(), $"(smallKeyShuffle == anywhere && has(SMALL_KEY_FIRE, {Keys + 1})) || (smallKeyShuffle != anywhere && has(SMALL_KEY_FIRE, {Keys}))");
                        break;
                    case "setting":
                        line = line.Replace(f.ToString(), $"{f.ParametersTrimmed.Replace(",", " ==")}");
                        break;
                    case "age":
                        if (f.ParametersTrimmed == "child") { line = line.Replace(f.ToString(), $"age_filter != adult"); }
                        else if (f.ParametersTrimmed == "adult") { line = line.Replace(f.ToString(), $"age_filter != child && OOT_TIME_TRAVEL"); }
                        break;
                    case "cond":
                        string CleanedInput = f.ParametersTrimmed.Replace("setting(", "(");
                        var splitArray = Regex.Split(CleanedInput, @"(?<!,[^(]+\([^)]+),");
                        line = line.Replace(f.ToString(), $"({splitArray[0]} == {splitArray[1]} && {splitArray[2]}) || ({splitArray[0]} != {splitArray[1]} && {splitArray[3]})");
                        break;
                    default:
                        line = line.Replace(f.ToString(), $"ERROR UNHANDLED FUNTION {f.Funtion}");
                        break;
                }
                FunctionsFound = LogicStringParser.GetFunctionsFromLogicString(line);
            }
            return line;
        }

        public static void AddEntriesFromLogicFiles(TrackerObjects.MMRData.LogicFile LogicFile, TrackerObjects.LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            string TestFolder = References.TestingPaths.GetDevTestingPath();
            string CodeFolder = References.TestingPaths.GetDevCodePath();
            string MMOOTCodeDir = Path.Combine(TestFolder, @"core-develop");
            string MMOOTCodeData = Path.Combine(MMOOTCodeDir, @"data");
            string MMOOTCodeMM = Path.Combine(MMOOTCodeData, @"mm");
            string MMOOTCodeOOT = Path.Combine(MMOOTCodeData, @"oot");
            string OOTMMCode = Path.Combine(CodeFolder, @"MMR Tracker V3", "OtherGames", "OOTMMRCOMBO");
            string MMOOTCodeMMMacros = Path.Combine(OOTMMCode, @"MMMacroOverride.json");
            string MMOOTCodeOOTMacros = Path.Combine(OOTMMCode, @"OOTMacroOverride.json");
            string MMOOTCodeMMWorld = Path.Combine(MMOOTCodeMM, @"world");
            string MMOOTCodeOOTWorld = Path.Combine(MMOOTCodeOOT, @"world");
            string[] MMOOTCodeMMWorldFiles = Directory.GetFiles(MMOOTCodeMMWorld);
            string[] MMOOTCodeOOTWorldFiles = Directory.GetFiles(MMOOTCodeOOTWorld);
            string FinalLogicFile = Path.Combine(TestFolder, @"OOTMMLogic.json");
            string FinalDictFile = Path.Combine(TestFolder, @"OOTMMDict.json");
            string OOTMMRandoEntrances = Path.Combine(OOTMMCode, @"RandomizableEntrances.json");
            string MMMacrosfile = File.ReadAllText(MMOOTCodeMMMacros);
            string OOTMacrosfile = File.ReadAllText(MMOOTCodeOOTMacros);

            Dictionary<string, string> RandoEntrances = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(OOTMMRandoEntrances));

            Dictionary<string, string> MMMacros = JsonConvert.DeserializeObject<Dictionary<string, string>>(MMMacrosfile);
            Dictionary<string, string> OOTMacros = JsonConvert.DeserializeObject<Dictionary<string, string>>(OOTMacrosfile);

            Dictionary<string, MMROOTLogicEntry> MMLogicEntries = new Dictionary<string, MMROOTLogicEntry>();
            Dictionary<string, MMROOTLogicEntry> OOTLogicEntries = new Dictionary<string, MMROOTLogicEntry>();
            addEntranceandEventData("OOT", MMOOTCodeOOTWorldFiles);
            addEntranceandEventData("MM", MMOOTCodeMMWorldFiles);
            addMacros("OOT", OOTMacros);
            addMacros("MM", MMMacros);

            void addMacros(string Game, Dictionary<string, string> MacroFiles)
            {
                foreach(var i in MacroFiles.Keys)
                {
                    string Truename = $"{Game}_{i}";
                    if (LogicFile.Logic.Find(x => x.Id == Truename) == null)
                    {


                        LogicFile.Logic.Add(new TrackerObjects.MMRData.JsonFormatLogicItem { Id = Truename });
                    }
                }
            }

            void addEntranceandEventData(string Game, string[] WorldFiles)
            {
                string OpositeGame = Game == "OOT" ? "MM" : "OOT";
                foreach (var i in WorldFiles)
                {
                    var text = LogicStringParser.ConvertYamlStringToJsonString(File.ReadAllText(i));
                    var LogicObject = JsonConvert.DeserializeObject<Dictionary<string, MMROOTLogicEntry>>(text);
                    foreach (var l in LogicObject.Keys)
                    {
                        string TrueAreaName = $"{Game} {l}";
                        if (!dictionaryFile.AreaList.Contains(TrueAreaName))
                            dictionaryFile.AreaList.Add(TrueAreaName);
                        foreach (var exit in LogicObject[l]?.exits?.Keys?.ToList()??new List<string>())
                        {
                            string TrueExitName = exit.StartsWith($"{OpositeGame} ") || exit.StartsWith($"{Game} ") ? $"{exit}" : $"{Game} {exit}";
                            if (TrueExitName == "OOT MM SPAWN") { TrueExitName = "MM SPAWN"; }
                            string FullexitName = $"{TrueAreaName} => {TrueExitName}";

                            if (LogicFile.Logic.Find(x => x.Id == FullexitName) == null)
                            {
                                TrackerObjects.MMRData.JsonFormatLogicItem EntranceEntry = new TrackerObjects.MMRData.JsonFormatLogicItem();
                                EntranceEntry.Id = FullexitName;
                                LogicFile.Logic.Add(EntranceEntry);
                            }

                            if (dictionaryFile.EntranceList.Find(x => x.ID == FullexitName) == null)
                            {
                                TrackerObjects.LogicDictionaryData.DictionaryEntranceEntries entranceEntry = new TrackerObjects.LogicDictionaryData.DictionaryEntranceEntries();
                                entranceEntry.ID = FullexitName;
                                entranceEntry.RandomizableEntrance = false;
                                entranceEntry.Area = TrueAreaName;
                                entranceEntry.Exit = TrueExitName;
                                entranceEntry.AlwaysAccessable = false;
                                if (FullexitName.StartsWith("OOT SPAWN => ")) { entranceEntry.AlwaysAccessable = true; }
                                if (RandoEntrances.ContainsKey(exit) && RandoEntrances[exit] == l) { entranceEntry.RandomizableEntrance = true; }

                                dictionaryFile.EntranceList.Add(entranceEntry);
                            }
                        }
                        foreach (var Event in LogicObject[l]?.events?.Keys?.ToList()??new List<string>())
                        {
                            string TrueMacroName = $"{Game}_{Event}";
                            if (LogicFile.Logic.Find(x => x.Id == TrueMacroName) == null)
                            {
                                LogicFile.Logic.Add(new TrackerObjects.MMRData.JsonFormatLogicItem { Id = TrueMacroName });
                            }
                        }
                        foreach (var hint in LogicObject[l]?.gossip?.Keys?.ToList()??new List<string>())
                        {
                            string TrueHintName = $"{Game} {hint}";
                            if (LogicFile.Logic.Find(x => x.Id == TrueHintName) == null)
                            {
                                LogicFile.Logic.Add(new TrackerObjects.MMRData.JsonFormatLogicItem { Id = TrueHintName });
                            }
                            if (dictionaryFile.HintSpots.Find(x => x.ID == TrueHintName) == null)
                            {
                                LogicDictionaryData.DictionaryHintEntries dictionaryHint = new LogicDictionaryData.DictionaryHintEntries();
                                dictionaryHint.ID = TrueHintName;
                                dictionaryHint.Name = TrueHintName;
                                dictionaryFile.HintSpots.Add(dictionaryHint);
                            }
                        }
                    }
                }
            }

        }

        public static void readAndApplySpoilerLog(LogicObjects.TrackerInstance Instance)
        {
            Instance.ToggleAllTricks(false);
            foreach (var i in Instance.EntrancePool.AreaList.SelectMany(x => x.Value.LoadingZoneExits))
            {
                i.Value.RandomizedState = MiscData.RandomizedState.Unrandomized;
            }
            Dictionary<string, bool> spoilerFileLocation =
                new Dictionary<string, bool> { { "Settings", false }, { "Tricks", false }, { "Starting Items", false }, { "Entrances", false }, { "Hints", false } };
            foreach(var l in Instance.SpoilerLog.Log)
            {
                if (string.IsNullOrWhiteSpace(l)){ continue; }
                if (l == "Settings") {
                    foreach (var k in spoilerFileLocation.Keys) { spoilerFileLocation[k] = false; }
                    spoilerFileLocation["Settings"] = true;
                    continue;
                }
                if (l == "Tricks")
                {
                    foreach (var k in spoilerFileLocation.Keys) { spoilerFileLocation[k] = false; }
                    spoilerFileLocation["Tricks"] = true;
                    continue;
                }
                if (l == "Starting Items") {
                    foreach (var k in spoilerFileLocation.Keys) { spoilerFileLocation[k] = false; }
                    spoilerFileLocation["Starting Items"] = true;
                    continue;
                }
                if (l == "Entrances")
                {
                    foreach (var k in spoilerFileLocation.Keys) { spoilerFileLocation[k] = false; }
                    spoilerFileLocation["Entrances"] = true;
                    continue;
                }
                if (l == "Foolish Regions")
                {
                    foreach (var k in spoilerFileLocation.Keys) { spoilerFileLocation[k] = false; }
                    continue;
                }
                if (l == "Hints")
                {
                    foreach (var k in spoilerFileLocation.Keys) { spoilerFileLocation[k] = false; }
                    spoilerFileLocation["Hints"] = true;
                    continue;
                }
                if (l == "Sphere 0") { break; }
                string Line = l.Trim();
                if (spoilerFileLocation["Settings"])
                {
                    var SettingLineData = Line.Split(":").Select(x => x.Trim()).ToArray();
                    if (SettingLineData.Length < 2) { continue; }
                    if (Instance.UserOptions.ContainsKey(SettingLineData[0]))
                    {
                        Instance.UserOptions[SettingLineData[0]].CurrentValue = SettingLineData[1];
                    }
                    else if (SettingLineData[0] == "erBoss" && SettingLineData[1] != "none")
                    {
                        foreach(var i in Instance.EntrancePool.AreaList.SelectMany(x => x.Value.LoadingZoneExits))
                        {
                            i.Value.RandomizedState = MiscData.RandomizedState.Randomized;
                        }
                    }
                }
                if (spoilerFileLocation["Tricks"])
                {
                    var trick = Instance.MacroPool.Values.FirstOrDefault(x => x.isTrick(Instance) && x.ID == Line);
                    if (trick is not null) { trick.TrickEnabled = true; }
                    else { Debug.WriteLine($"{Line} Could notbe found in the trick list!"); }
                }
                if (spoilerFileLocation["Starting Items"])
                {
                    var startingItemData = Line.Split(":").Select(x => x.Trim()).ToArray();
                    if (startingItemData.Length < 2 || !int.TryParse(startingItemData[1], out int tr)) { continue; }
                    if (tr < 1) { continue; }
                    for (var i = 0; i < tr; i++)
                    {
                        var ValidItem = Instance.GetItemToPlace(startingItemData[0], true, true);
                        if (ValidItem is not null) { ValidItem.AmountInStartingpool++; }
                        else { Debug.WriteLine($"{startingItemData[1]} Could not be made a starting item!"); }
                    }
                }
                if (spoilerFileLocation["Hints"])
                {
                    var pieces = Line.Split(new[] { ':' }, 2).Select(x => x.Trim()).ToArray();
                    var GossipStone = pieces[0];
                    if (!Instance.HintPool.ContainsKey(GossipStone)) { continue; }
                    var GossipStoneEntry = Instance.HintPool[GossipStone];
                    var Data = pieces[1].Split(new[] { ',' }, 2).Select(x => x.Trim()).ToArray();
                    var HintType = Data[0];
                    var HintData = Data[1].Replace(")", "").Split("(").Select(x => x.Trim()).ToArray();
                    string LocationName = HintData[0].ToLower().Replace("_", " ");
                    LocationName = Regex.Replace(LocationName, @"(^\w)|(\s\w)", m => m.Value.ToUpper()).Replace("Mm", "MM").Replace("Oot", "OOT");
                    switch (HintType)
                    {
                        case "Item-Exact":
                        case "Item-Region":
                            Func<ItemData.ItemObject, bool> predicate = x => x.GetDictEntry(Instance).SpoilerData.GossipHintNames.Contains(HintData[1]) || x.Id == HintData[1];
                            string ItemName = Instance.ItemPool.Values.FirstOrDefault(predicate)?.GetDictEntry(Instance)?.GetName(Instance)??HintData[1];
                            GossipStoneEntry.SpoilerHintText = $"{LocationName}: {ItemName}";
                            break;
                        case "Hero":
                            GossipStoneEntry.SpoilerHintText = $"{LocationName}: Hero";
                            break;
                        case "Foolish":
                            GossipStoneEntry.SpoilerHintText = $"{LocationName}: foolish";
                            break;
                    }
                }
                if (spoilerFileLocation["Entrances"])
                {
                    var EntranceData = Line.Split(new string[] { "->" }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
                    var Source = EntranceData[0].Split('/');
                    var Destination = EntranceData[1].Split('/');
                    if (Instance.EntrancePool.AreaList.ContainsKey(Source[0]) && 
                        Instance.EntrancePool.AreaList[Source[0]].LoadingZoneExits.ContainsKey(Source[1]) &&
                        Instance.EntrancePool.AreaList.ContainsKey(Destination[0]) &&
                        Instance.EntrancePool.AreaList[Destination[0]].LoadingZoneExits.ContainsKey(Destination[1]))
                    {
                        Instance.EntrancePool.AreaList[Source[0]].LoadingZoneExits[Source[1]].SpoilerDefinedDestinationExit = new EntranceData.EntranceRandoDestination
                        {
                            region = Destination[1],
                            from = Destination[0]
                        };
                    }
                    else { Debug.WriteLine($"{Line} Could not be mapped to an entrance!"); }
                }
            }

            var ReleventData = GetReleventSpoilerLines(Instance.SpoilerLog.Log);
            foreach (var i in ReleventData)
            {
                var entryData = i.Split(':').Select(x => x.Trim()).ToArray();

                TrackerObjects.LocationData.LocationObject location = null;
                TrackerObjects.ItemData.ItemObject item = Instance.GetItemToPlace(entryData[1]);

                if (Instance.LocationPool.ContainsKey(entryData[0])) { location = Instance.LocationPool[entryData[0]]; }
                else { Debug.WriteLine($"{entryData[0]} was not a valid location!"); }
                if (item is null) { Debug.WriteLine($"{entryData[1]} was not a valid Item or no more of this could be placed!"); }
                if (location is not null) { location.Randomizeditem.SpoilerLogGivenItem = item?.Id ?? entryData[1]; }

            }
        }


        public static void GetSpoilerLog()
        {
            Dictionary<string, string> CheckList = new Dictionary<string, string>();
            Dictionary<string, string> ItemList = new Dictionary<string, string>(); 
            string TestFolder = References.TestingPaths.GetDevTestingPath();
            string SpoilerLogs = Path.Combine(TestFolder, @"MM-OOT");
            string[] AllSpoilerLogs = Directory.GetFiles(SpoilerLogs);

            foreach (var LogicFile in AllSpoilerLogs)
            {
                Debug.WriteLine(LogicFile);
                if (!Path.GetFileNameWithoutExtension(LogicFile).StartsWith("spoiler-")) { continue; }
                var Data = File.ReadAllLines(LogicFile);
                var ReleventData = GetReleventSpoilerLines(Data);
                foreach (var i in ReleventData)
                {
                    var entryData = i.Split(':').Select(x => x.Trim()).ToArray();
                    CheckList[entryData[0]] = "";
                    ItemList[entryData[1]] = "";
                }
            }
            Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(CheckList.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value), Testing._NewtonsoftJsonSerializerOptions));
            Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(ItemList.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value), Testing._NewtonsoftJsonSerializerOptions));
        }

        public static List<string> GetReleventSpoilerLines(string[] Data)
        {
            var ReleventSpoilerlines = new List<string>();
            bool AtSphereData = false;
            bool CurrentLineEmpty = false;
            bool AtLocationData = false;
            foreach (var x in Data)
            {
                bool SphereLine;
                if (x.StartsWith("Sphere ")) { AtSphereData = true; SphereLine = true; }
                else { SphereLine = false; }
                bool PreviousLineEmpty = CurrentLineEmpty;
                if (string.IsNullOrWhiteSpace(x)) { CurrentLineEmpty = true; }
                else { CurrentLineEmpty = false; }
                if (!CurrentLineEmpty && !SphereLine && PreviousLineEmpty && AtSphereData) { AtLocationData = true; }
                if (AtLocationData) { ReleventSpoilerlines.Add(x); }
            }
            return ReleventSpoilerlines;
        }

        public static void AddEntriesFromItemPools(out TrackerObjects.MMRData.LogicFile Logic, out TrackerObjects.LogicDictionaryData.LogicDictionary dictionary)
        {
            string CodeFolder = References.TestingPaths.GetDevCodePath();
            string OOTMMCode = Path.Combine(CodeFolder, @"MMR Tracker V3", "OtherGames", "OOTMMRCOMBO");
            string MMpool = Path.Combine(References.TestingPaths.GetDevTestingPath(), @"core-develop", "data", "mm", "pool.csv");
            string OOTpool = Path.Combine(References.TestingPaths.GetDevTestingPath(), @"core-develop", "data", "oot", "pool.csv");
            string OOTMMChecks = Path.Combine(OOTMMCode, @"checks.json");
            string OOTMMItems = Path.Combine(OOTMMCode, @"items.json");
            string OOTMMTricks = Path.Combine(OOTMMCode, @"tricks.json");
            string OOTMMArea = Path.Combine(OOTMMCode, @"AreaNames.json");

            Dictionary<string, dynamic> OOTRCheckDict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(File.ReadAllText(OOTMMChecks));
            Dictionary<string, dynamic> OOTRItemsDict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(File.ReadAllText(OOTMMItems));
            Dictionary<string, string> OOTRTricksDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(OOTMMTricks));
            Dictionary<string, string> OOTRAreaDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(OOTMMArea));

            string[] MMPoolWebData = File.ReadAllLines(MMpool);
            var mmPool = ConvertCsvFileToJsonObject(MMPoolWebData.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
            var mmPoolObj = JsonConvert.DeserializeObject<List<MMROOTLocation>>(mmPool);

            string[] OOTPoolWebData = File.ReadAllLines(OOTpool);
            var ootPool = ConvertCsvFileToJsonObject(OOTPoolWebData.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
            var ootPoolObj = JsonConvert.DeserializeObject<List<MMROOTLocation>>(ootPool);

            foreach(var i in OOTRCheckDict.Keys)
            {
                var OOTCheck = ootPoolObj.Find(x => $"OOT {x.location}" == i);
                var MMRCheck = mmPoolObj.Find(x => $"MM {x.location}" == i);
                if (OOTCheck is not null)
                    OOTRCheckDict[i] = new string[] { OOTCheck.scene, $"OOT_{OOTCheck.item}" };
                else if (MMRCheck is not null)
                    OOTRCheckDict[i] = new string[] { MMRCheck.scene, $"MM_{MMRCheck.item}" };
            }

            TrackerObjects.MMRData.LogicFile logicFile = new TrackerObjects.MMRData.LogicFile();
            logicFile.Version = 1;
            logicFile.GameCode = "OOTMM";
            logicFile.Logic = new List<TrackerObjects.MMRData.JsonFormatLogicItem>();

            TrackerObjects.LogicDictionaryData.LogicDictionary logicDictionary = new TrackerObjects.LogicDictionaryData.LogicDictionary();
            logicDictionary.LogicVersion = 1;
            logicDictionary.GameCode = "OOTMM";
            logicDictionary.RootArea = "OOT SPAWN";
            logicDictionary.LocationList = new List<TrackerObjects.LogicDictionaryData.DictionaryLocationEntries>();
            logicDictionary.ItemList = new List<TrackerObjects.LogicDictionaryData.DictionaryItemEntries>();

            foreach (var i in OOTRCheckDict.Keys)
            {
                string[] DictValue = OOTRCheckDict[i] as string[];
                logicFile.Logic.Add(new TrackerObjects.MMRData.JsonFormatLogicItem() { Id = i });
                TrackerObjects.LogicDictionaryData.DictionaryLocationEntries dictEntry = new TrackerObjects.LogicDictionaryData.DictionaryLocationEntries();
                dictEntry.ID = i;
                dictEntry.SpoilerData = new TrackerObjects.MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { i }, GossipHintNames = new string[] { i } };
                dictEntry.Name = i;
                dictEntry.ValidItemTypes = new string[] { "item" };
                if (OOTRAreaDict.ContainsKey(i)) { dictEntry.Area = OOTRAreaDict[i]; }
                else if (OOTRAreaDict.ContainsKey(DictValue[0])) { dictEntry.Area = OOTRAreaDict[DictValue[0]]; }
                else { dictEntry.Area = DictValue[0]; }
                dictEntry.OriginalItem = DictValue[1];
                logicDictionary.LocationList.Add(dictEntry);
            }
            foreach(var i in OOTRItemsDict.Keys)
            {
                if (OOTRItemsDict[i] is bool ib && !ib) { continue; }
                TrackerObjects.LogicDictionaryData.DictionaryItemEntries dictItem = new TrackerObjects.LogicDictionaryData.DictionaryItemEntries();
                string ItemName = i;
                List<string> ExtraSpoilerNames = new List<string>();
                if (OOTRItemsDict[i] is System.String) { ItemName = (string)OOTRItemsDict[i]; }
                else 
                { 
                    ItemName = OOTRItemsDict[i][0];
                    ExtraSpoilerNames = OOTRItemsDict[i][1].ToObject<List<string>>();
                }

                if (i.StartsWith("OOT_"))
                {
                    ItemName = $"OOT {ItemName}";
                }
                else if (i.StartsWith("MM_"))
                {
                    ItemName = $"MM {ItemName}";
                }


                dictItem.Name = ItemName;
                dictItem.MaxAmountInWorld = -1;
                dictItem.ValidStartingItem = true;
                dictItem.ID = i;
                dictItem.SpoilerData = new TrackerObjects.MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { i }, GossipHintNames =  new string[] { i, ItemName } };
                ExtraSpoilerNames.ForEach(x => dictItem.SpoilerData.SpoilerLogNames = dictItem.SpoilerData.SpoilerLogNames.Append(x).ToArray());
                dictItem.ItemTypes = new string[] { "item" };
                logicDictionary.ItemList.Add(dictItem);
            }

            foreach(var i in OOTRTricksDict.Keys)
            {
                TrackerObjects.LogicDictionaryData.DictionaryMacroEntry TrickObject = new TrackerObjects.LogicDictionaryData.DictionaryMacroEntry();
                TrickObject.ID = i;
                TrickObject.Name = OOTRTricksDict[i].Split(":")[1];
                logicDictionary.MacroList.Add(TrickObject);

                logicFile.Logic.Add(new TrackerObjects.MMRData.JsonFormatLogicItem() { Id = i, IsTrick = true, TrickCategory = OOTRTricksDict[i].Split(":")[0] });
            }

            Logic = logicFile;
            dictionary = logicDictionary;

        }

        public static string ConvertCsvFileToJsonObject(string[] lines)
        {
            var csv = new List<string[]>();

            var properties = lines[0].Split(',');

            foreach (string line in lines)
            {
                var LineData = line.Split(',');
                csv.Add(LineData);
            }

            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                    objResult.Add(properties[j].Trim(), csv[i][j].Trim());

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult, Testing._NewtonsoftJsonSerializerOptions);
        }
    }
}
