using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.datamodel;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.OOTMMUtil;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.FunctionParsing;
using System.Xml.Schema;

namespace MMR_Tracker_V3.OtherGames.OOTMMV2
{
    public static class GenData
    {
        public static LogicStringParser OOTMMLogicStringParser = new LogicStringParser();
        public static OOTMMParserData OTTMMPaths = new OOTMMParserData();
        public static void ReadData(out MMRData.LogicFile OutLogic, out LogicDictionaryData.LogicDictionary outDict)
        {
            //CodePaths

            OTTMMPaths.MMRTrackerCodePath = Path.Combine(References.TestingPaths.GetDevCodePath(), "MMR Tracker V3");
            OTTMMPaths.OOTMMV2CodeFolder = Path.Combine(OTTMMPaths.MMRTrackerCodePath, "OtherGames", "OOTMMV2");
            OTTMMPaths.SettingsFile = Path.Combine(OTTMMPaths.OOTMMV2CodeFolder, "settings.json");
            OTTMMPaths.TricksFile = Path.Combine(OTTMMPaths.OOTMMV2CodeFolder, "tricks.json");
            OTTMMPaths.ItemsFile = Path.Combine(OTTMMPaths.OOTMMV2CodeFolder, "items.json");
            OTTMMPaths.ItemNamesFile = Path.Combine(OTTMMPaths.OOTMMV2CodeFolder, "ItemNames.json");
            
            //Shared Data
            OTTMMPaths.OOTMMCorePath = Path.Combine(References.TestingPaths.GetDevTestingPath(), "OoTMM-develop", "packages", "core");
            OTTMMPaths.OOTMMTestingFolder = Path.Combine(References.TestingPaths.GetDevTestingPath(), "OOTMMTesting");
            OTTMMPaths.ExampleSpoiler = Path.Combine(OTTMMPaths.OOTMMTestingFolder, "AreaRefSpoiler.txt");
            OTTMMPaths.ExampleSpoilerMQ = Path.Combine(OTTMMPaths.OOTMMTestingFolder, "AreaRefSpoilerMQ.txt");


            //OOT Data Paths
            OTTMMPaths.OOTData = Path.Combine(OTTMMPaths.OOTMMCorePath, "data", "oot");
            OTTMMPaths.OOTWorld = Path.Combine(OTTMMPaths.OOTData, "world");
            OTTMMPaths.OOTMQWorld = Path.Combine(OTTMMPaths.OOTData, "world_mq");

            //OOT Data Files
            OTTMMPaths.OOTEntrancesFile = Path.Combine(OTTMMPaths.OOTData, "entrances.csv");
            OTTMMPaths.OOTMacroFile = Path.Combine(OTTMMPaths.OOTData, "macros.yml");
            OTTMMPaths.OOTPoolFile = Path.Combine(OTTMMPaths.OOTData, "pool.csv");
            OTTMMPaths.OOTHintFile = Path.Combine(OTTMMPaths.OOTData, "hints.csv");

            //MM Data
            OTTMMPaths.MMData = Path.Combine(OTTMMPaths.OOTMMCorePath, "data", "mm");
            OTTMMPaths.MMWorld = Path.Combine(OTTMMPaths.MMData, "world");

            //MM Data Files
            OTTMMPaths.MMEntrancesFile = Path.Combine(OTTMMPaths.MMData, "entrances.csv");
            OTTMMPaths.MMMacroFile = Path.Combine(OTTMMPaths.MMData, "macros.yml");
            OTTMMPaths.MMPoolFile = Path.Combine(OTTMMPaths.MMData, "pool.csv");
            OTTMMPaths.MMHintFile = Path.Combine(OTTMMPaths.MMData, "hints.csv");

            //Shared Data Files
            OTTMMPaths.SHAREDMacroFile = Path.Combine(OTTMMPaths.OOTMMCorePath, "data", "macros.yml");

            MMRData.LogicFile LogicFile = LogicFileCreation.ReadAndParseLogicFile(OTTMMPaths);
            LogicDictionaryData.LogicDictionary DictionaryFile = LogicDictionaryCreation.CreateDictionary(OTTMMPaths);

            AssignLocationAreas(DictionaryFile);

            SettingsCreation.CreateSettings(DictionaryFile, OTTMMPaths);

            AddTingleProxies(LogicFile, DictionaryFile);

            AddWalletMactros(LogicFile, DictionaryFile);

            AddAdditionalData(LogicFile, DictionaryFile, OTTMMPaths);

            AddGameClearChecks(LogicFile, DictionaryFile);

            ParseLogicFunctions(LogicFile);

            FixAreaClearLogic(LogicFile);

            AddLinkToGlobal(LogicFile, DictionaryFile);

            foreach (var i in LogicFile.Logic)
            {
                if (!OTTMMPaths.MMLogicEntries.Contains(i.Id) || 
                    OTTMMPaths.MMSOTSafeLogicEntries.Contains(i.Id) || 
                    i.RequiredItems.Contains("MM_can_reset_time") || 
                    i.RequiredItems.Contains("MM_can_reset_time_on_moon")) { continue; }
                i.RequiredItems.Add("MM_can_reset_time");
            }

            RemoveNonLogicAlteringSettings(LogicFile, DictionaryFile);

            File.WriteAllText(Path.Combine(OTTMMPaths.OOTMMTestingFolder,"LogicFile.json"), JsonConvert.SerializeObject(LogicFile, Testing._NewtonsoftJsonSerializerOptions));
            File.WriteAllText(Path.Combine(OTTMMPaths.OOTMMTestingFolder, "DictionaryFile.json"), JsonConvert.SerializeObject(DictionaryFile, Testing._NewtonsoftJsonSerializerOptions));

            OutLogic = LogicFile;
            outDict = DictionaryFile;
        }

        private static void AssignLocationAreas(LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            Dictionary<string, string> LocationAreas = new Dictionary<string, string>();

            ReadFile(OTTMMPaths.ExampleSpoiler);
            ReadFile(OTTMMPaths.ExampleSpoilerMQ);
            void ReadFile(string Path)
            {
                bool AtLocations = false;
                string CurrentArea = "";
                foreach (var line in File.ReadAllLines(Path))
                {
                    if (line.StartsWith("Location List (")) { AtLocations = true; continue; }
                    if (!AtLocations || string.IsNullOrWhiteSpace(line)) { continue; }

                    if (line.EndsWith("):")) { CurrentArea = line[..(line.IndexOf("("))].Trim(); continue; }

                    string Location = line[..(line.IndexOf(":"))].Trim();
                    if (!LocationAreas.ContainsKey(Location)) { LocationAreas.Add(Location, CurrentArea); }
                }
            }
            foreach(var i in dictionaryFile.LocationList)
            {
                if (LocationAreas.ContainsKey(i.Key)) { i.Value.Area = LocationAreas[i.Key]; }
            }

            var UnknownAreas = dictionaryFile.LocationList.Where(x => x.Value.Area == "UNKNOWN").Select(x => x.Key);
            if (UnknownAreas.Any()) { Debug.WriteLine($"The following checks had no area\n{string.Join('\n', UnknownAreas)}"); }

            Debug.WriteLine(string.Join('\n', LocationAreas.Values.Distinct().ToList()));

        }

        private static void AddLinkToGlobal(MMRData.LogicFile logicFile, LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            dictionaryFile.EntranceList["OOT SPAWN => OOT SPAWN CHILD"].AlwaysAccessable = true;
            dictionaryFile.EntranceList["OOT SPAWN => OOT SPAWN ADULT"].AlwaysAccessable = true;
        }

        public static void FixAreaClearLogic(MMRData.LogicFile Logic)
        {
            var MM_BOSS_GREAT_BAY = Logic.Logic.First(x => x.Id == "MM_EVENT_BOSS_GREAT_BAY");
            var MM_BOSS_SNOWHEAD = Logic.Logic.First(x => x.Id == "MM_EVENT_BOSS_SNOWHEAD");
            var MM_CLEAN_SWAMP = Logic.Logic.First(x => x.Id == "MM_EVENT_CLEAN_SWAMP");
            var OOT_EVENT_WATER_TEMPLE_CLEARED = Logic.Logic.First(x => x.Id == "OOT_EVENT_WATER_TEMPLE_CLEARED");

            string CodeFolder = References.TestingPaths.GetDevCodePath();

            Dictionary<string, string> RandoAreaClear = new(){
                  {"OOT Deku Tree Boss", "OOT Deku Tree Boss"},
                  {"OOT Dodongo Cavern Boss", "OOT Dodongo Cavern Boss"},
                  {"OOT Jabu-Jabu Boss", "OOT Jabu-Jabu Boss"},
                  {"OOT Forest Temple Boss", "OOT Forest Temple Boss"},
                  {"OOT Fire Temple Boss", "OOT Fire Temple Boss"},
                  {"OOT Water Temple Boss", "OOT Water Temple Boss"},
                  {"OOT Spirit Temple Boss", "OOT Spirit Temple Boss"},
                  {"OOT Shadow Temple Boss", "OOT Shadow Temple Boss"},
                  {"MM Woodfall Temple Boss", "MM Woodfall Temple Boss"},
                  {"MM Great Bay Temple Boss", "MM Great Bay Temple Boss"},
                  {"MM Snowhead Temple Boss", "MM Snowhead Temple Boss"},
                  {"MM Stone Tower Temple Boss", "MM Stone Tower Boss"}
            };

            CreateLogic(MM_BOSS_GREAT_BAY, "MM Great Bay Temple Boss Access", "MM Great Bay Temple Boss");
            CreateLogic(MM_BOSS_SNOWHEAD, "MM Snowhead Temple Boss Access", "MM Snowhead Temple Boss");
            CreateLogic(MM_CLEAN_SWAMP, "MM Woodfall Temple Boss Access", "MM Woodfall Temple Boss");
            CreateLogic(OOT_EVENT_WATER_TEMPLE_CLEARED, "OOT Water Temple Antichamber", "OOT Water Temple Boss");

            void CreateLogic(MMRData.JsonFormatLogicItem Item, string DungeonArea, string BossDoor)
            {
                Item.RequiredItems = new List<string>();
                Item.ConditionalItems = new List<List<string>>();
                foreach (var i in RandoAreaClear)
                {
                    Item.ConditionalItems.Add(new() { $"contains{{{DungeonArea} => {BossDoor}, {i.Key}}}", $"check{{{i.Value}}}" });
                }
            }
        }

        private static void RemoveNonLogicAlteringSettings(MMRData.LogicFile LogicFile, LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            List<string> settingIDs = new List<string>();

            var AllLogicItems = LogicFile.Logic.SelectMany(x => x.ConditionalItems.SelectMany(x => x))
                .Concat(LogicFile.Logic.SelectMany(x => x.RequiredItems));
            AllLogicItems = AllLogicItems.Concat(dictionaryFile.AdditionalLogic.SelectMany(x => x.ConditionalItems.SelectMany(x => x))
                .Concat(dictionaryFile.AdditionalLogic.SelectMany(x => x.RequiredItems)));

            foreach (string Entry in AllLogicItems)
            {
                if (LogicEditing.IsLogicFunction(Entry, out string Func, out string Param, new('{', '}')))
                {
                    if (Func == "setting" || Func == "var")
                    {
                        var Params = Param.Split(',').Select(x => x.Trim()).ToList();
                        settingIDs.Add(Params[0]);
                        if (Params.Count > 1 && dictionaryFile.Variables.ContainsKey(Params[1])) { settingIDs.Add(Params[1]); }
                        if (dictionaryFile.Variables.ContainsKey(Params[0]) && dictionaryFile.Variables[Params[0]].Value is Newtonsoft.Json.Linq.JArray) { settingIDs.Add(Params[0]); }
                    }
                }
                else if (Entry.Contains(','))
                {
                    var Params = Entry.Split(',').Select(x => x.Trim()).ToArray();
                    if (dictionaryFile.Variables.ContainsKey(Params[0])) { settingIDs.Add(Params[0]); }
                    if (Params.Length > 1 && dictionaryFile.Variables.ContainsKey(Params[1])) { settingIDs.Add(Params[1]); }
                }
                 
            }

            foreach(var i in dictionaryFile.Options)
            {
                foreach(var j in i.Value.Values)
                {
                    if (j.Value.VariableEdit.Any())
                    {
                        settingIDs.Add(i.Key);
                        foreach (var k in j.Value.VariableEdit)
                        {
                            settingIDs.Add(k.Key);
                        }
                    }
                }
            }


            foreach (var i in dictionaryFile.Options.Keys) 
            { 
                if (!settingIDs.Contains(i))
                    dictionaryFile.Options.Remove(i);
            }
            foreach (var i in dictionaryFile.Variables.Keys)
            {
                if (!settingIDs.Contains(i))
                    dictionaryFile.Variables.Remove(i);
            }

        }

        private static void AddWalletMactros(MMRData.LogicFile logicFile, LogicDictionaryData.LogicDictionary DictionaryFile)
        {
            var Costs = new List<int> { 0, 99, 200, 500, 999, 9999 };
            foreach (var Cost in Costs)
            {
                AddCostMacro(Cost, "MM");
                AddCostMacro(Cost, "OOT");
            }
            void AddCostMacro(int Cost, string Gamecode)
            {
                string ID = $"{Gamecode}_COST_{Cost}";
                var Logic = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, $"can_use_wallet({Costs.IndexOf(Cost)})", ID);
                logicFile.Logic.Add(new MMRData.JsonFormatLogicItem { Id = ID, ConditionalItems = Logic });
                DictionaryFile.MacroList.Add(ID, new LogicDictionaryData.DictionaryMacroEntry
                {
                    ID = ID,
                    WalletCapacity = Cost,
                    WalletCurrency = Gamecode[0]
                });
            }
        }

        private static void AddTingleProxies(MMRData.LogicFile logicFile, LogicDictionaryData.LogicDictionary DictionaryFile)
        {
            List<Tuple<string, string, string>> TingleLocations = new()
            {
                new("Town", "Clock Town", DictionaryFile.LocationList["MM Clock Town Blast Mask"].Area ),
                new("Swamp", "Woodfall", DictionaryFile.LocationList["MM Road to Southern Swamp Grotto Grass 01"].Area ),
                new("Mountain", "Snowhead", DictionaryFile.LocationList["MM Twin Islands Underwater Chest 1"].Area ),
                new("Ranch", "Ranch", DictionaryFile.LocationList["MM Milk Road Grass 1"].Area ),
                new("Great Bay", "Great Bay", DictionaryFile.LocationList["MM Great Bay Coast Zora Mask"].Area ),
                new("Ikana", "Ikana", DictionaryFile.LocationList["MM Ikana Canyon Grass 1"].Area )
            };

            foreach (var Location in TingleLocations)
            {
                int PrevIndex = TingleLocations.IndexOf(Location) - 1;
                if (PrevIndex < 0) { PrevIndex = TingleLocations.Count - 1; }
                Tuple<string, string, string> AltLocation = TingleLocations[PrevIndex];
                MMRData.JsonFormatLogicItem TinglePurchaseMacro = new() { Id = $"MM Tingle Purchase {Location.Item1} at {Location.Item1}", 
                    ConditionalItems = new List<List<string>> { new List<string> { $"MM Tingle {Location.Item1}", $"MM_COST_99" } } };
                MMRData.JsonFormatLogicItem TinglePurchaseMacro2 = new() { Id = $"MM Tingle Purchase {Location.Item1} at {AltLocation.Item1}",
                    ConditionalItems = new List<List<string>> { new List<string> { $"MM Tingle {AltLocation.Item1}", $"MM_COST_99" } }
                };
                logicFile.Logic.Add(TinglePurchaseMacro);
                logicFile.Logic.Add(TinglePurchaseMacro2);

                DictionaryFile.MacroList.Add(TinglePurchaseMacro.Id, new() { ID = TinglePurchaseMacro.Id, WalletCurrency = 'M' });
                DictionaryFile.MacroList.Add(TinglePurchaseMacro2.Id, new() { ID = TinglePurchaseMacro2.Id, WalletCurrency = 'M' });

                var MapLocationCheckDict = DictionaryFile.LocationList[$"MM Tingle Map {Location.Item2}"];
                var MapLocationCheckDictAlt = DictionaryFile.LocationList[$"MM Tingle Map {AltLocation.Item2}"];
                MapLocationCheckDict.LocationProxys.Add(new LogicDictionaryData.DictLocationProxy
                {
                    ID = $"MM Tingle Purchase {Location.Item1} at {Location.Item1} Proxy",
                    Area = Location.Item3,
                    LogicInheritance = TinglePurchaseMacro.Id,
                    Name = MapLocationCheckDict.Name + $" ({Location.Item1})"
                });
                MapLocationCheckDict.LocationProxys.Add(new LogicDictionaryData.DictLocationProxy
                {
                    ID = $"MM Tingle Purchase {Location.Item1} at {AltLocation.Item1} Proxy",
                    Area = AltLocation.Item3,
                    LogicInheritance = TinglePurchaseMacro2.Id,
                    Name = MapLocationCheckDict.Name + $" ({AltLocation.Item1})"
                });

                var MapLocationCheckLogic = logicFile.Logic.First(x => x.Id == $"MM Tingle Map {Location.Item2}");
                MapLocationCheckLogic.RequiredItems.Clear();
                MapLocationCheckLogic.ConditionalItems = new List<List<string>> { new List<string>() { TinglePurchaseMacro.Id }, new List<string>() { TinglePurchaseMacro2.Id } };

            }
        }

        private static void AddAdditionalData(MMRData.LogicFile LogicFile, LogicDictionaryData.LogicDictionary DictionaryFile, OOTMMParserData OTTMMPaths)
        {
            //This event doesn't exist but logic for magic is asking for it. Just make it always false.
            LogicFile.Logic.Add(new MMRData.JsonFormatLogicItem { Id = "MM_EVENT_CHATEAU", RequiredItems = new List<string>() { "false" } });
            LogicFile.Logic.Add(new MMRData.JsonFormatLogicItem { Id = "MM_is_child", RequiredItems = new List<string>() { "OOT_is_child" } });
            LogicFile.Logic.Add(new MMRData.JsonFormatLogicItem { Id = "MM_!is_goal_triforce", RequiredItems = new List<string>() { "setting{goal, triforce, false}", "setting{goal, triforce3, false}" } });
            LogicFile.Logic.Add(new MMRData.JsonFormatLogicItem { Id = "OOT_!is_goal_triforce", RequiredItems = new List<string>() { "setting{goal, triforce, false}", "setting{goal, triforce3, false}" } });

            var MASKS_REGULAR = new string[] {
              "MM_MASK_CAPTAIN",
              "MM_MASK_GIANT",
              "MM_MASK_ALL_NIGHT",
              "MM_MASK_BUNNY",
              "MM_MASK_KEATON",
              "MM_MASK_GARO",
              "MM_MASK_ROMANI",
              "MM_MASK_TROUPE_LEADER",
              "MM_MASK_POSTMAN",
              "MM_MASK_COUPLE",
              "MM_MASK_GREAT_FAIRY",
              "MM_MASK_GIBDO",
              "MM_MASK_DON_GERO",
              "MM_MASK_KAMARO",
              "MM_MASK_TRUTH",
              "MM_MASK_STONE",
              "MM_MASK_BREMEN",
              "MM_MASK_BLAST",
              "MM_MASK_SCENTS",
              "MM_MASK_KAFEI",
              "SHARED_MASK_TRUTH",
              "SHARED_MASK_BUNNY",
              "SHARED_MASK_KEATON",
            };

            OptionData.TrackerVar MMMaskVar = new OptionData.TrackerVar
            {
                ID = "MM_MASKS",
                Static = true,
                Value = MASKS_REGULAR.Select(x => $"{x}, 1").ToList(),
            };
            DictionaryFile.Variables.Add("MM_MASKS", MMMaskVar);

            MMRData.JsonFormatLogicItem BothGameSouls = new() { Id = "BothGameSouls", RequiredItems = new List<string> { "var{enemySoulsOot}", "var{enemySoulsMm}" } };
            MMRData.JsonFormatLogicItem BothGameSkeletonKey = new() { Id = "BothGameSkeletonKey", RequiredItems = new List<string> { "var{skeletonKeyOot}", "var{skeletonKeyMm}" } };
            DictionaryFile.AdditionalLogic.Add(BothGameSouls);
            DictionaryFile.AdditionalLogic.Add(BothGameSkeletonKey);

            Dictionary<string, string> SettingLogic = new()
            {
                { "players", "mode, multi" },
                { "distinctWorlds", "mode, multi" },
                { "triforceGoal", "goal, triforce" },
                { "triforcePieces", "goal, triforce" },
                { "ganonBossKey", "goal, triforce, false" },
                { "majoraChild", "goal, triforce, false" },
                { "csmcHearts", "csmc, never, false" },
                { "bottomlessWallets", "colossalWallets" },
                { "sharedSouls", BothGameSouls.Id },
                { "sharedSkeletonKey", BothGameSkeletonKey.Id },
                { "sharedSongSun", "sunSongMm" }
            };

            foreach(var i in SettingLogic)
            {
                string[] LogicParts = i.Value.Split(',').Select(x => x.Trim()).ToArray();
                string LogicString;
                if (DictionaryFile.Options.ContainsKey(LogicParts[0])) { LogicString = $"setting{{{i.Value}}}"; }
                else if (DictionaryFile.Variables.ContainsKey(LogicParts[0])) { LogicString = $"var{{{i.Value}}}"; }
                else { LogicString = i.Value; }
                if (DictionaryFile.Options.ContainsKey(i.Key)) { DictionaryFile.Options[i.Key].Logic = LogicString; }
                else if (DictionaryFile.Variables.ContainsKey(i.Key)) { DictionaryFile.Variables[i.Key].Logic = LogicString; }
                else { throw new Exception($"{i.Key} is not a valid option or variable"); }
            }

        }
        private static void AddGameClearChecks(MMRData.LogicFile logicFile, LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            dictionaryFile.LocationList.Add("MM_BOSS_MAJORA", new LogicDictionaryData.DictionaryLocationEntries
            {
                ID = "MM_BOSS_MAJORA",
                Name = "Majora",
                Area = "The Moon",
                OriginalItem = "MM_MASK_MAJORA",
                ValidItemTypes = new string[] { "Majora" },
                SpoilerData = new MMRData.SpoilerlogReference()
            });
            logicFile.Logic.Add(new MMRData.JsonFormatLogicItem
            {
                Id = "MM_BOSS_MAJORA",
                RequiredItems = new List<string> { "MM_EVENT_MAJORA" }
            });

            dictionaryFile.ItemList.Add("MM_MASK_MAJORA", new LogicDictionaryData.DictionaryItemEntries
            {
                ID = "MM_MASK_MAJORA",
                Name = "Majoras Mask",
                ValidStartingItem = false,
                MaxAmountInWorld = 1,
                ItemTypes= new string[] { "Majora" },
                SpoilerData = new MMRData.SpoilerlogReference()
            });

            dictionaryFile.LocationList.Add("OOT_BOSS_GANON", new LogicDictionaryData.DictionaryLocationEntries
            {
                ID = "OOT_BOSS_GANON",
                Name = "Ganon",
                Area = "Ganon's Castle",
                OriginalItem = "OOT_TRIFORCE",
                ValidItemTypes = new string[] { "Ganon" },
                SpoilerData = new MMRData.SpoilerlogReference()
            });

            logicFile.Logic.Add(new MMRData.JsonFormatLogicItem
            {
                Id = "OOT_BOSS_GANON",
                RequiredItems = new List<string> { "OOT_EVENT_GANON" }
            });

            dictionaryFile.ItemList.Add("OOT_TRIFORCE", new LogicDictionaryData.DictionaryItemEntries
            {
                ID = "OOT_TRIFORCE",
                Name = "Triforce",
                ValidStartingItem = false,
                MaxAmountInWorld = 1,
                ItemTypes= new string[] { "Ganon" },
                SpoilerData = new MMRData.SpoilerlogReference()
            });

            //Game Clear

            string GameClearLogic =
                "(setting{goal, any} && (OOT_TRIFORCE || MM_MASK_MAJORA)) || " +
                "(setting{goal, both} && OOT_TRIFORCE && MM_MASK_MAJORA) || " +
                "(setting{goal, ganon} && OOT_TRIFORCE) || " +
                "(setting{goal, majora} && MM_MASK_MAJORA) || " +
                "(setting{goal, triforce} && SHARED_TRIFORCE, triforceGoal) || " +
                "(setting{goal, triforce3} && SHARED_TRIFORCE_POWER && SHARED_TRIFORCE_COURAGE && SHARED_TRIFORCE_WISDOM)";

            logicFile.Logic.Add(new MMRData.JsonFormatLogicItem { Id = "Game_Clear", ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, GameClearLogic, "Game_Clear") });
            dictionaryFile.MacroList.Add("Game_Clear", new LogicDictionaryData.DictionaryMacroEntry { ID = "Game_Clear", Name = "Both Games Cleared" });
            dictionaryFile.WinCondition = "Game_Clear";
        }
    }
}
