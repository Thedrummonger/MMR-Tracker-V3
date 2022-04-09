using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames
{
    public class OOTRLogicObject
    {
        public string region_name { get; set; } = "";
        public string Dungeon { get; set; } = "";
        public bool MQ { get; set; } = false;
        public Dictionary<string, string> events { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> locations { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> exits { get; set; } = new Dictionary<string, string>();
    }
    public class OOTRGossip
    {
        public string text = "";
        public string[] colors = null;
    }
    public class SpoilerLogPriceData
    {
        public string item = "";
        public int price = 0;
    }
    public class RegionExit
    {
        public string region = "";
        public string from = "";
    }
    public class SpoilerLog
    {
        public Dictionary<string, dynamic> settings = new Dictionary<string, dynamic>();
        public Dictionary<string, dynamic> randomized_settings = new Dictionary<string, dynamic>();
        public Dictionary<string, string> dungeons = new Dictionary<string, string>();
        public Dictionary<string, dynamic> entrances = new Dictionary<string, dynamic>();
        public Dictionary<string, dynamic> locations = new Dictionary<string, dynamic>();
        public Dictionary<string, int> item_pool = new Dictionary<string, int>();
        public Dictionary<string, string> trials = new Dictionary<string, string>();
        public Dictionary<string, int> starting_items = new Dictionary<string, int>();
        public Dictionary<string, OOTRGossip> gossip_stones = new Dictionary<string, OOTRGossip>();
    }
    public class EntranceTable
    {
        public string[] CoupledEntrances { get; set; }
        public string[] OneWayEntrances { get; set; }
        public Dictionary<string, string> VanillaLocations { get; set; }
        public Dictionary<string, string> UnrandomizableLocations { get; set; }
        public Dictionary<string, string> GossipStones { get; set; }
        public string[] ItemPool { get; set; }
        public Dictionary<string, string> LogicOverrides { get; set; }
        public Dictionary<string, string> StaticMacros { get; set; }
        public Dictionary<string, string[]> Options { get; set; }
        public Dictionary<string, string[]> StaticLocations { get; set; } = new Dictionary<string, string[]>();
        public Dictionary<string, string> StaticItems { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string[]> LogicTricks { get; set; } = new Dictionary<string, string[]>();
        public Dictionary<string, dynamic[]> Wallets { get; set; } = new Dictionary<string, dynamic[]>();
        public List<LogicDictionaryData.TrackerVariable> Variables { get; set; } = new List<LogicDictionaryData.TrackerVariable>();
    }
    public class OOTRTools
    { 
        public static void ReadEntranceRefFile(out string OutLogic, out string OutDict)
        {
            var Logic = GetDataFromWeb("Roman971/OoT-Randomizer/Dev-R");

            EntranceTable TestTable = new EntranceTable();

            string OOTRHelperFile = @"D:\Visual Studio Code Stuff\MMR-Tracker-V3\MMR Tracker V3\OtherGames\OOTREntranceReference.json";

            if (File.Exists(OOTRHelperFile))
            {
                TestTable = JsonConvert.DeserializeObject<EntranceTable>(File.ReadAllText(OOTRHelperFile));
            }
            else
            {
                System.Net.WebClient wc = new System.Net.WebClient();
                TestTable = JsonConvert.DeserializeObject<EntranceTable>(wc.DownloadString(@"https://raw.githubusercontent.com/Thedrummonger/MMR-Tracker-V3/master/MMR%20Tracker%20V3/OtherGames/OOTREntranceReference.json"));
            }


            LogicDictionaryData.LogicDictionary OORTDict = new LogicDictionaryData.LogicDictionary();

            OORTDict.GameCode = "OOTR";
            OORTDict.LogicVersion = 1;
            OORTDict.Variables = TestTable.Variables;

            string EntranceSeperator = " X ";

            string[] WarpExits =
            {
                $"Minuet of Forest Warp{EntranceSeperator}Sacred Forest Meadow",
                $"Bolero of Fire Warp{EntranceSeperator}DMC Central Local",
                $"Serenade of Water Warp{EntranceSeperator}Lake Hylia",
                $"Requiem of Spirit Warp{EntranceSeperator}Desert Colossus",
                $"Nocturne of Shadow Warp{EntranceSeperator}Graveyard Warp Pad Region",
                $"Prelude of Light Warp{EntranceSeperator}Temple of Time",
                $"Child Spawn{EntranceSeperator}KF Links House",
                $"Adult Spawn{EntranceSeperator}Temple of Time",
            };

            foreach(var i in TestTable.Options)
            {
                OORTDict.Options.Add(new OptionData.TrackerOption
                {
                    ID = i.Key,
                    DisplayName = Regex.Replace(i.Key.Replace("_", " "), @"(^\w)|(\s\w)", m => m.Value.ToUpper()),
                    CurrentValue = i.Value[0],
                    Values = i.Value.ToDictionary(x => x, x => new OptionData.actions())
                });
            }

            for(var i = 0; i < TestTable.CoupledEntrances.Length; i += 2)
            {
                var Line1 = TestTable.CoupledEntrances[i];
                var Line2 = TestTable.CoupledEntrances[i+1];
                LogicDictionaryData.DictionaryEntranceEntries Exit = new LogicDictionaryData.DictionaryEntranceEntries();
                LogicDictionaryData.DictionaryEntranceEntries CoupledExit = new LogicDictionaryData.DictionaryEntranceEntries();
                var data1 = Line1.Split(new string[] { "->" }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
                var data2 = Line2.Split(new string[] { "->" }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();

                Exit.ID = data1[0] + EntranceSeperator + data1[1];
                Exit.Area = data1[0];
                Exit.Exit = data1[1];
                Exit.EntrancePairID = new EntranceData.EntranceAreaPair { Area = data2[0], Exit = data2[1] };
                Exit.RandomizableEntrance = true;
                CoupledExit.ID = data2[0] + EntranceSeperator + data2[1];
                CoupledExit.Area = data2[0];
                CoupledExit.Exit = data2[1];
                CoupledExit.EntrancePairID = new EntranceData.EntranceAreaPair { Area = data1[0], Exit = data1[1] };
                CoupledExit.RandomizableEntrance = true;

                OORTDict.EntranceList.Add(Exit);
                OORTDict.EntranceList.Add(CoupledExit);
            }
            foreach (var i in OORTDict.EntranceList)
            {
                i.DestinationHasSingleEntrance = OORTDict.EntranceList.Where(x => x.ID == i.ID).Count() < 2;
            }
            foreach (var i in TestTable.OneWayEntrances)
            {
                LogicDictionaryData.DictionaryEntranceEntries Exit = new LogicDictionaryData.DictionaryEntranceEntries();
                var data1 = i.Split(new string[] { "->" }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
                Exit.ID = data1[0] + EntranceSeperator + data1[1];
                Exit.Area = data1[0];
                Exit.Exit = data1[1];
                Exit.EntrancePairID = null;
                Exit.AlwaysAccessable = WarpExits.Contains(Exit.ID);
                Exit.RandomizableEntrance = true;
                OORTDict.EntranceList.Add(Exit);
            }
            foreach (var i in Logic)
            {
                OORTDict.AreaList.Add(i.region_name);
                foreach (var e in i.exits)
                {
                    string ID = i.region_name + EntranceSeperator + e.Key;
                    if (OORTDict.EntranceList.Where(x => x.ID == ID).Any()) { continue; }
                    LogicDictionaryData.DictionaryEntranceEntries Exit = new LogicDictionaryData.DictionaryEntranceEntries();
                    Exit.ID = ID;
                    Exit.Area = i.region_name;
                    Exit.Exit = e.Key;
                    Exit.EntrancePairID = null;
                    Exit.RandomizableEntrance = false;
                    OORTDict.EntranceList.Add(Exit);
                }
                foreach(var l in i.locations)
                {
                    if (TestTable.GossipStones.ContainsKey(l.Key))
                    {
                        LogicDictionaryData.DictionaryHintEntries hintEntry = new LogicDictionaryData.DictionaryHintEntries();
                        hintEntry.ID = l.Key;
                        hintEntry.Name = TestTable.GossipStones[l.Key];
                        hintEntry.SpoilerData.SpoilerLogNames = new string[] { l.Key, TestTable.GossipStones[l.Key] };
                        OORTDict.HintSpots.Add(hintEntry);
                    }
                    else
                    {
                        if (OORTDict.LocationList.Where(x => x.ID == l.Key).Any())
                        {
                            var DuplicateEntry = OORTDict.LocationList.First(x => x.ID == l.Key);
                            if (string.IsNullOrEmpty(i.Dungeon)) { DuplicateEntry.Area = "Misc"; }
                            continue;
                        }
                        LogicDictionaryData.DictionaryLocationEntries locationObject = new LogicDictionaryData.DictionaryLocationEntries();
                        locationObject.Area = (string.IsNullOrEmpty(i.Dungeon) ? i.region_name : i.Dungeon) + (l.Key.Contains(" GS ") ? " Gold Skulltulas" : "");
                        locationObject.ID = l.Key;
                        locationObject.Name = l.Key;
                        locationObject.SpoilerData.SpoilerLogNames = new string[] { l.Key };
                        locationObject.SpoilerData.PriceDataNames = new string[] { l.Key };
                        locationObject.ValidItemTypes = new string[] { "Item" };
                        OORTDict.LocationList.Add(locationObject);
                    }
                }
                if (i.region_name == "Logic Helper")
                {
                    foreach (var l in i.events)
                    {
                        LogicDictionaryData.DictionaryMacroEntry MacroObject = new LogicDictionaryData.DictionaryMacroEntry();
                        MacroObject.ID = l.Key;
                        OORTDict.MacroList.Add(MacroObject);
                    }
                }
            }

            foreach (var i in TestTable.VanillaLocations)
            {
                //Debug.WriteLine($"Setting {i.Value} at {i.Key}");
                var location = OORTDict.LocationList.Find(x => x.ID == i.Key);
                location.OriginalItem = CleanItem(i.Value);
            }
            foreach (var i in TestTable.UnrandomizableLocations)
            {
                //Debug.WriteLine($"Setting {i.Value} at {i.Key}");
                var location = OORTDict.LocationList.Find(x => x.ID == i.Key);
                location.OriginalItem = CleanItem(i.Value);
                location.ValidItemTypes = new string[] { $"{i.Value}" };
            }

            foreach(var i in TestTable.ItemPool)
            {
                if (OORTDict.ItemList.Any(x => x.ID == CleanItem(i))) { continue; }
                LogicDictionaryData.DictionaryItemEntries itemEntries = new LogicDictionaryData.DictionaryItemEntries();
                itemEntries.ID = CleanItem(i);
                itemEntries.ItemTypes = new string[] { "Item", i };
                itemEntries.MaxAmountInWorld = -1;
                itemEntries.Name = i;
                itemEntries.SpoilerData.SpoilerLogNames = new string[] { CleanItem(i), i };
                itemEntries.ValidStartingItem = true;
                OORTDict.ItemList.Add(itemEntries);
                
            }

            MMRData.LogicFile OORTLogic = new MMRData.LogicFile();
            OORTLogic.GameCode = "OOTR";
            OORTLogic.Version = 1;
            OORTLogic.Logic = new List<MMRData.JsonFormatLogicItem>();

            Dictionary<string, string> Condesedlogic = new();

            foreach (var i in Logic)
            {
                foreach (var l in i.locations)
                {
                    addEntry(l.Key, l.Value);
                }
                foreach (var l in i.events)
                {
                    addEntry(l.Key, l.Value);
                }
                foreach (var l in i.exits)
                {
                    addEntry($"{i.region_name}{EntranceSeperator}{l.Key}", l.Value);
                }

                void addEntry(string loc, string Logic)
                {
                    string LogicString = "";
                    string LayoutType = i.MQ ? "Master Quest" : "Vanilla";
                    string Dungeon = string.IsNullOrWhiteSpace(i.Dungeon) ? null : $"{i.Dungeon} Layout == {LayoutType}";

                    LogicString = $"({Logic})" + (Dungeon != null ? $" & ({Dungeon})" : "") + (i.region_name != "Logic Helper" ? $" & ({i.region_name})" : "");

                    if (Condesedlogic.ContainsKey(loc))
                    {
                        Condesedlogic[loc] += $" or ({LogicString})";
                    }
                    else
                    {
                        Condesedlogic.Add(loc, $"({LogicString})");
                    }
                }
            }

            foreach (var i in TestTable.StaticMacros)
            {
                Condesedlogic.Add(i.Key, i.Value);
            }

            foreach(var i in TestTable.Wallets)
            {
                var ItemEntry = OORTDict.ItemList.Find(x => x.ID == CleanItem(i.Key));
                if (ItemEntry == null)
                {
                    Condesedlogic.Add(i.Key, i.Value[0]);
                    LogicDictionaryData.DictionaryMacroEntry WalletMacro = new LogicDictionaryData.DictionaryMacroEntry();
                    WalletMacro.ID = i.Key;
                    WalletMacro.WalletCapacity = (int)i.Value[1];
                    OORTDict.MacroList.Add(WalletMacro);
                }
                else
                {
                    ItemEntry.WalletCapacity = (int)i.Value[1];
                }
            }

            foreach (var i in TestTable.StaticLocations)
            {
                Condesedlogic.Add(i.Key, i.Value[1]);
                LogicDictionaryData.DictionaryLocationEntries locationObject = new LogicDictionaryData.DictionaryLocationEntries();
                locationObject.OriginalItem = i.Value[2];
                locationObject.Area = i.Value[0];
                locationObject.ID = i.Key;
                locationObject.Name = i.Key;
                locationObject.ValidItemTypes = new string[] { i.Value[0] };
                OORTDict.LocationList.Add(locationObject);
            }
            foreach (var i in TestTable.StaticItems)
            {
                LogicDictionaryData.DictionaryItemEntries itemEntries = new LogicDictionaryData.DictionaryItemEntries();
                itemEntries.ID = i.Key;
                itemEntries.ItemTypes = new string[] { i.Value };
                itemEntries.MaxAmountInWorld = -1;
                itemEntries.Name = i.Key;
                itemEntries.SpoilerData.SpoilerLogNames = new string[] { i.Key };
                itemEntries.ValidStartingItem = false;
                OORTDict.ItemList.Add(itemEntries);
            }

            foreach (var i in Condesedlogic)
            {
                string CurrentLogic = i.Value;
                if (TestTable.LogicOverrides.ContainsKey(i.Key))
                {
                    CurrentLogic = TestTable.LogicOverrides[i.Key];
                }
                MMRData.JsonFormatLogicItem logicItem = new MMRData.JsonFormatLogicItem();
                logicItem.Id = i.Key;
                logicItem.ConditionalItems = CleanLogic(LogicStringParser.ConvertLogicStringToConditional(CurrentLogic));
                logicItem.ConditionalItems = AddKeyRingLogic(logicItem.ConditionalItems);
                logicCleaner.RemoveRedundantConditionals(logicItem);
                logicCleaner.MakeCommonConditionalsRequirements(logicItem);
                logicItem.RequiredItems = logicItem.RequiredItems.Where(x => x.ToLower() != "true").ToList();
                OORTLogic.Logic.Add(logicItem);
            }
            foreach (var i in TestTable.LogicTricks)
            {
                MMRData.JsonFormatLogicItem logicItem = new MMRData.JsonFormatLogicItem();
                logicItem.Id = i.Key;
                logicItem.IsTrick = true;
                logicItem.TrickTooltip = i.Value[1];
                OORTLogic.Logic.Add(logicItem);
                LogicDictionaryData.DictionaryMacroEntry macroEntry = new LogicDictionaryData.DictionaryMacroEntry
                {
                    ID = i.Key,
                    Name = i.Value[0]
                };
                OORTDict.MacroList.Add(macroEntry);
            }

            Debug.WriteLine("===============================");
            List<string> Found = new List<string>();
            foreach(var i in OORTDict.LocationList)
            {
                if (OORTDict.LocationList.Where(x => x.ID == i.ID).Count() > 1 && !Found.Contains(i.ID))
                {
                    Debug.WriteLine(i.ID + $" Exists in Location Dict {OORTDict.LocationList.Where(x => x.ID == i.ID).Count()} Times");
                    Found.Add(i.ID);
                }
            }

            OutLogic = JsonConvert.SerializeObject(OORTLogic, Testing._NewtonsoftJsonSerializerOptions);
            OutDict = JsonConvert.SerializeObject(OORTDict, Testing._NewtonsoftJsonSerializerOptions);
            LogicObjects.TrackerInstance OOTRInstance = new LogicObjects.TrackerInstance();
            TrackerInstanceCreation.ApplyLogicAndDict(OOTRInstance, JsonConvert.SerializeObject(OORTLogic, Testing._NewtonsoftJsonSerializerOptions), JsonConvert.SerializeObject(OORTDict, Testing._NewtonsoftJsonSerializerOptions));
            TrackerInstanceCreation.PopulateTrackerObject(OOTRInstance);

            try
            {
                File.WriteAllText(@"D:\Testing\OOTRDict.json", OutDict);
                File.WriteAllText(@"D:\Testing\OOTRLogic.json", OutLogic);
                File.WriteAllText(@"D:\Testing\OOTRSaveFile.json", JsonConvert.SerializeObject(OOTRInstance, Testing._NewtonsoftJsonSerializerOptions));
            }
            catch { }
            //return;


            Debug.WriteLine("===============================");

            foreach(var i in OOTRInstance.LogicFile.GetAllItemsUsedInLogic().ToList())
            {
                LogicCalculation.RequirementsMet(new List<string> { i }, OOTRInstance);
            }

        }

        private static List<List<string>> AddKeyRingLogic(List<List<string>> conditionalItems)
        {
            List<List<string>> NewConditionals = new List<List<string>>();
            foreach (var cond in conditionalItems)
            {
                List<string> NewConditional = new List<string>();
                foreach (var i in cond)
                {
                    if (i.Contains("Small_Key") && i.Contains(","))
                    {
                        string NewLogicLine = i + " | ";
                        string KeyRingEntry = i.Replace("Small_Key_", "Small_Key_Ring_");
                        KeyRingEntry = KeyRingEntry.Substring(0, KeyRingEntry.IndexOf(","));
                        NewLogicLine += KeyRingEntry;
                        NewConditional.Add($"({NewLogicLine})");
                    }
                    else { NewConditional.Add(i); }
                }
                NewConditionals.Add(NewConditional);
            }
            string NewLogicString = $"({string.Join(") | (", NewConditionals.Select(x => string.Join(" & ", x)))})";
            return LogicStringParser.ConvertLogicStringToConditional(NewLogicString);
        }

        public static List<OOTRLogicObject> GetDataFromWeb(string Branch = "TestRunnerSRL/OoT-Randomizer/Dev")
        {
            List<OOTRLogicObject> Logic = new List<OOTRLogicObject>();
            System.Net.WebClient wc = new System.Net.WebClient();
            List<string> LogicFiles = new List<string>
            {
                $"https://raw.githubusercontent.com/{Branch}/data/World/Overworld.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Bottom%20of%20the%20Well.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Deku%20Tree.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Dodongos%20Cavern.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Fire%20Temple.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Forest%20Temple.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Ganons%20Castle.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Gerudo%20Training%20Ground.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Ice%20Cavern.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Jabu%20Jabus%20Belly.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Shadow%20Temple.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Spirit%20Temple.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Water%20Temple.json"

            };

            List<string> MQLogicFiles = new List<string>
            {
                $"https://raw.githubusercontent.com/{Branch}/data/World/Bottom%20of%20the%20Well%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Deku%20Tree%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Dodongos%20Cavern%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Fire%20Temple%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Forest%20Temple%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Ganons%20Castle%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Gerudo%20Training%20Ground%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Ice%20Cavern%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Jabu%20Jabus%20Belly%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Shadow%20Temple%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Spirit%20Temple%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Water%20Temple%20MQ.json"

            };


            GetLogicDataFromFile(LogicFiles, false);
            GetLogicDataFromFile(MQLogicFiles, true);

            void GetLogicDataFromFile(List<string> LogicFileUrls, bool MQData)
            {
                foreach (var i in LogicFileUrls)
                {
                    Console.WriteLine(i);
                    string ItemData = wc.DownloadString(i);
                    string[] ItemDataLines = ItemData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    var CleanedLogic = RemoveCommentsFromJSON(ItemDataLines);
                    var tempLogic = JsonConvert.DeserializeObject<List<OOTRLogicObject>>(CleanedLogic, new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Ignore });

                    foreach (var t in tempLogic) 
                    { 
                        t.MQ = MQData; 
                        if (t.region_name == "Ganons Castle Tower") { t.Dungeon = null; }
                    }
                    Logic.AddRange(tempLogic);
                }
            }

            string RawHelperLogic = wc.DownloadString($"https://raw.githubusercontent.com/{Branch}/data/LogicHelpers.json");
            string[] RawHelperLogicLines = RawHelperLogic.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var CleanedHelperLogic = RemoveCommentsFromJSON(RawHelperLogicLines);

            OOTRLogicObject HelperLogic = new OOTRLogicObject() { region_name = "Logic Helper", events = JsonConvert.DeserializeObject<Dictionary<string, string>>(CleanedHelperLogic) };


            Logic.Add(HelperLogic);
            return Logic;
        }

        public static string RemoveCommentsFromJSON(string[] ItemDataLines)
        {
            string CleanedLogic = "";
            foreach (var i in ItemDataLines)
            {
                string[] lines = i.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                string[] LinesNew = lines.Where(i => !i.Trim().StartsWith("#")).Select(i => (i.Contains("#")) ? i.Substring(0, i.IndexOf("#")) : i).ToArray();
                var CleanedLine = string.Join(Environment.NewLine, LinesNew);
                if (!string.IsNullOrWhiteSpace(CleanedLine)) { CleanedLogic += CleanedLine; }
            }
            return CleanedLogic;
        }

        public static List<List<string>> CleanLogic(List<List<string>> Conditionals)
        {
            List<List<string>> NewCond = new List<List<string>>();
            foreach(var cond in Conditionals)
            {
                List<string> newSet = new List<string>();
                foreach(var i in cond)
                {
                    var NewString = i;
                    if (i.StartsWith("at("))
                    {
                        NewString = i[2..]; 
                        var regex = new Regex(Regex.Escape(","));
                        NewString = regex.Replace(NewString, " & (", 1);
                        NewString += ")";
                    }
                    else if (i.StartsWith("here("))
                    {
                        NewString = i[4..];
                    }
                    newSet.Add(NewString);
                }
                NewCond.Add(newSet);
            }
            return LogicStringParser.ConvertLogicStringToConditional(string.Join(" + ", NewCond.Select(x => string.Join(" * ", x))));
        }

        public static string CleanItem(string Item)
        {
            string input = Regex.Replace(Item, @"\[.*?\]", String.Empty).Trim();
            input = input.Replace(" ", "_").Replace("(", "").Replace(")", "").Replace("'", "");
            return input;
        }

        public static void HandleOOTRSpoilerLog(string SpoilerLog, LogicObjects.TrackerInstance Instance)
        {
            var Log = JsonConvert.DeserializeObject<SpoilerLog>(SpoilerLog, Testing._NewtonsoftJsonSerializerOptions);

            Instance.StaticOptions.DecoupleEntrances = Log.settings.ContainsKey("decouple_entrances") && Log.settings["decouple_entrances"];

            Instance.LocationPool["Ganon"].Randomizeditem.SpoilerLogGivenItem = Instance.LocationPool["Ganon"].GetDictEntry(Instance).OriginalItem;

            string[] SpoilerLogSettings = new string[] { "Starting_age", "skip_child_zelda", "free_scarecrow", "open_door_of_time", "complete_mask_quest", "bombchus_in_logic", "plant_beans", 
                "hints", "damage_multiplier", "gerudo_fortress", "open_forest", "zora_fountain", "open_kakariko", "bridge", "shuffle_ganon_bosskey" };

            foreach(var setting in SpoilerLogSettings)
            {
                if (Log.settings.ContainsKey(setting) && Instance.UserOptions.ContainsKey(setting))
                {
                    Debug.WriteLine($"Settng {Instance.UserOptions[setting].ID} To {Log.settings[setting].ToString()}");
                    Instance.UserOptions[setting].CurrentValue = Log.settings[setting].ToString();
                }
            }

            string[] BridgeCount = new string[] { "bridge_stones", "bridge_medallions", "bridge_rewards", "bridge_tokens" };
            string[] GanonCount = new string[] { "ganon_bosskey_stones", "ganon_bosskey_medallions", "ganon_bosskey_rewards", "ganon_bosskey_tokens" };
            foreach(var setting in BridgeCount)
            {
                if (Log.settings.ContainsKey(setting) && Instance.Variables.ContainsKey("bridge_amount"))
                {
                    Debug.WriteLine($"Settng {Instance.Variables["bridge_amount"].ID} To {Log.settings[setting]}");
                    Instance.Variables["bridge_amount"].Value = Log.settings[setting];
                }
            }
            foreach (var setting in GanonCount)
            {
                if (Log.settings.ContainsKey(setting) && Instance.Variables.ContainsKey("ganon_key_amount"))
                {
                    Debug.WriteLine($"Settng {Instance.Variables["ganon_key_amount"].ID} To {Log.settings[setting]}");
                    Instance.Variables["ganon_key_amount"].Value = Log.settings[setting];
                }
            }

            string[] SageGiftActive = new string[] { "stones", "medallions", "dungeons", "tokens" };
            if (SageGiftActive.Contains(Instance.UserOptions["shuffle_ganon_bosskey"].CurrentValue))
            {
                Instance.LocationPool["Gift from Sages"].SetRandomizedState(MiscData.RandomizedState.ForcedJunk, Instance);
            }

            Dictionary<string, string> DungeonLayouts = new Dictionary<string, string>
            {
                { "Ganons Castle Layout", "Ganons Castle" },
                { "Bottom of the Well Layout", "Bottom of the Well" },
                { "Deku Tree Layout", "Deku Tree" },
                { "Dodongos Cavern Layout", "Dodongos Cavern" },
                { "Fire Temple Layout", "Fire Temple" },
                { "Forest Temple Layout", "Forest Temple" },
                { "Gerudo Training Ground Layout", "Gerudo Training Ground" },
                { "Ice Cavern Layout", "Ice Cavern" },
                { "Jabu Jabus Belly Layout", "Jabu Jabus Belly" },
                { "Shadow Temple Layout", "Shadow Temple" },
                { "Spirit Temple Layout", "Spirit Temple" },
                { "Water Temple Layout", "Water Temple" },
            };
            Dictionary<string, string> TrialStatus = new Dictionary<string, string>
            {
                { "Forest Trial Status", "Forest" },
                { "Fire Trial Status", "Fire" },
                { "Water Trial Status", "Water" },
                { "Spirit Trial Status", "Spirit" },
                { "Shadow Trial Status", "Shadow" },
                { "Light Trial Status", "Light" },
            };
            foreach(var i in TrialStatus)
            {
                if (Log.settings.ContainsKey("trials_random") && !Log.settings["trials_random"] && Log.settings["trials"] == 6)
                {
                    Instance.LocationPool[i.Key].RandomizedState = MiscData.RandomizedState.Unrandomized;
                }
                else
                {
                    Instance.LocationPool[i.Key].Randomizeditem.SpoilerLogGivenItem = char.ToUpper(Log.trials[i.Value][0]) + Log.trials[i.Value][1..];
                }
            }
            foreach (var i in DungeonLayouts)
            {
                if (Log.settings.ContainsKey("mq_dungeons_mode") && (Log.settings["mq_dungeons_mode"] == "vanilla" || (Log.settings["mq_dungeons_mode"] == "count" && Log.settings["mq_dungeons_count"] == 0))) 
                { 
                    Instance.LocationPool[i.Key].RandomizedState = MiscData.RandomizedState.Unrandomized; 
                }
                else
                {
                    Instance.LocationPool[i.Key].Randomizeditem.SpoilerLogGivenItem = Log.dungeons[i.Value] == "mq" ? "Master Quest" : "Vanilla";
                }
            }

            string LightArrowLocation = "";
            foreach (var i in Log.locations)
            {
                string Locations = i.Key;
                string Item = "";
                int Price = -1;

                Type T = i.Value.GetType();
                if (T == typeof(string)) { Item = CleanItem(i.Value.ToString()); }
                else
                {
                    SpoilerLogPriceData Data = i.Value.ToObject<SpoilerLogPriceData>();
                    Item = CleanItem(Data.item);
                    Price = Data.price;
                }
                if (Instance.LocationPool.ContainsKey(Locations))
                {
                    if (!Instance.ItemPool.ContainsKey(Item)) { Debug.WriteLine(Item + " Was an unknown item."); }
                    Instance.LocationPool[Locations].Randomizeditem.SpoilerLogGivenItem = Item;
                    if (Price > 0) { Instance.LocationPool[Locations].Price = Price; }
                    if (Item == "Light_Arrows") { LightArrowLocation = Instance.LocationPool[Locations].GetDictEntry(Instance).Area; }
                }
                else
                {
                    Debug.WriteLine(Locations + " Was an unknown Locations.");
                }
            }
            foreach (var i in Log.gossip_stones)
            {
                var GossipLocation = Instance.HintPool.Values.FirstOrDefault(x => x.GetDictEntry(Instance).SpoilerData.SpoilerLogNames.Contains(i.Key));
                if (GossipLocation == null)
                {
                    Debug.WriteLine(i.Key + " Was an unknown Hint Location.");
                    continue;
                }
                GossipLocation.SpoilerHintText = i.Value.text;
            }
            
            Instance.HintPool["Ganondorf Hint"].SpoilerHintText = $"Ha ha ha... You'll never beat me by reflecting my lightning bolts and unleashing the arrows" + ((bool)Log.settings["misc_hints"] ? $" from {LightArrowLocation}."  : ".");

            foreach (var i in Log.entrances)
            {
                Debug.WriteLine($"==================================================");
                var EntranceData = i.Key.Split(new string[] { " -> " }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
                EntranceData.EntranceAreaPair Entrance = new() { Area = EntranceData[0], Exit = EntranceData[1] };
                var Exit = Instance.EntrancePool.AreaList[Entrance.Area].LoadingZoneExits[Entrance.Exit];
                EntranceData.EntranceRandoDestination destination = new EntranceData.EntranceRandoDestination();
                Debug.WriteLine($"Assigning Spoiler Data to {Exit.ParentAreaID} -> {Exit.ID}");
                string sValue = i.Value as string;
                if (sValue != null)
                {
                    destination.region = sValue;
                    var AllExits = Instance.EntrancePool.AreaList.SelectMany(x => x.Value.LoadingZoneExits.Values).First(x => x.ID == sValue && x.EntrancePair != null);
                    Debug.WriteLine($"Region {sValue} Did not have a From Region. Assuming full exit to be {AllExits.ID} <= {AllExits.ParentAreaID}");
                    destination.from = AllExits.ParentAreaID;
                }
                else
                {
                    RegionExit R = i.Value.ToObject<RegionExit>();
                    destination.region = R.region;
                    destination.from = R.from;
                }
                Exit.SpoilerDefinedDestinationExit = destination;
                Debug.WriteLine($"{Exit.ParentAreaID} > {Exit.ID} Was Set To {destination.region} < {destination.from}");
                if (!Instance.StaticOptions.DecoupleEntrances)
                {
                    Debug.WriteLine($"Settng Paired Entrance of {Exit.ParentAreaID} -> {Exit.ID} = {destination.region} <- {destination.from}");
                    Debug.WriteLine($"Destination Was exit {destination.region} > {destination.from}");
                    var DestinationPairedExit = Instance.EntrancePool.GetEntrancePairOfDestination(destination);
                    if (DestinationPairedExit == null) { continue; }
                    Debug.WriteLine($"Entrance Pair of Destnation was {DestinationPairedExit.ParentAreaID} -> {DestinationPairedExit.ID}");
                    var OriginalEntrancePairedExit = Exit.EntrancePair;
                    if (OriginalEntrancePairedExit == null) { continue; }
                    Debug.WriteLine($"Entrance Pair of Original Exit was {OriginalEntrancePairedExit.Area} -> {OriginalEntrancePairedExit.Exit}");
                    var OriginalEntrancePairedExitAsDestination = new EntranceData.EntranceRandoDestination { from = OriginalEntrancePairedExit.Area, region = OriginalEntrancePairedExit.Exit };
                    DestinationPairedExit.SpoilerDefinedDestinationExit = OriginalEntrancePairedExitAsDestination;
                    Debug.WriteLine($"{DestinationPairedExit.ParentAreaID} -> {DestinationPairedExit.ID} Was Set To {OriginalEntrancePairedExitAsDestination.region} <- {OriginalEntrancePairedExitAsDestination.from} ");
                }
            }
            foreach (var i in Log.starting_items)
            {
                var item = CleanItem(i.Key);
                if (Instance.ItemPool.ContainsKey(item))
                {
                    Instance.ItemPool[item].AmountInStartingpool = i.Value;
                }
            }
            foreach (var i in Log.settings)
            {
                if (i.Key == "disabled_locations")
                {
                    foreach(string junk in i.Value)
                    {
                        if (Instance.LocationPool.ContainsKey(junk)) { Instance.LocationPool[junk].SetRandomizedState(MiscData.RandomizedState.ForcedJunk, Instance); }
                    }
                }
                if (i.Key == "allowed_tricks")
                {
                    List<string> EnabledTricks = new List<string>();
                    foreach(string val in i.Value) { EnabledTricks.Add(val); }
                    foreach(var trick in Instance.MacroPool.Where(x => x.Value.isTrick(Instance))) 
                    { 
                        trick.Value.TrickEnabled = EnabledTricks.Contains(trick.Key); 
                    }
                }
                if (i.Key == "dungeon_shortcuts" || (i.Key == "dungeon_shortcuts_choice" && i.Value == "all"))
                {
                    bool AllShortcuts = (i.Key == "dungeon_shortcuts_choice" && i.Value == "all");
                    List<string> DungeonShortcuts = new List<string>();
                    if (!AllShortcuts) { foreach (string val in i.Value) { DungeonShortcuts.Add(val); } }
                    Instance.UserOptions["Deku_tree_Shortcuts"].CurrentValue = DungeonShortcuts.Contains("deku_tree") || AllShortcuts ? "Enabled" : "Disabled";
                    Instance.UserOptions["Dodongos_Cavern_Shortcuts"].CurrentValue = DungeonShortcuts.Contains("dodongos_cavern") || AllShortcuts ? "Enabled" : "Disabled";
                    Instance.UserOptions["Jabu_Jabus_Belly_Shortcuts"].CurrentValue = DungeonShortcuts.Contains("jabu_jabus_belly") || AllShortcuts ? "Enabled" : "Disabled";
                    Instance.UserOptions["Forest_Temple_Shortcuts"].CurrentValue = DungeonShortcuts.Contains("forest_temple") || AllShortcuts ? "Enabled" : "Disabled";
                    Instance.UserOptions["Fire_Temple_Shortcuts"].CurrentValue = DungeonShortcuts.Contains("fire_temple") || AllShortcuts ? "Enabled" : "Disabled";
                    Instance.UserOptions["Water_Temple_Shortcuts"].CurrentValue = DungeonShortcuts.Contains("water_temple") || AllShortcuts ? "Enabled" : "Disabled";
                    Instance.UserOptions["Shadow_Temple_Shortcuts"].CurrentValue = DungeonShortcuts.Contains("shadow_temple") || AllShortcuts ? "Enabled" : "Disabled";
                    Instance.UserOptions["Spirit_Temple_Shortcuts"].CurrentValue = DungeonShortcuts.Contains("spirit_temple") || AllShortcuts ? "Enabled" : "Disabled";
                }
            }
            Debug.WriteLine("Locations ================================");
            foreach (var i in Instance.LocationPool.Values)
            {
                if (i.Randomizeditem.SpoilerLogGivenItem == null)
                {
                    Debug.WriteLine($"{i.ID} Was not found in spoiler log, Unrandomizing.");
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized, Instance);
                }
            }
            Debug.WriteLine("Locations ================================");
            foreach (var i in Instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values))
            {
                if (i.SpoilerDefinedDestinationExit == null)
                {
                    Debug.WriteLine($"{i.ParentAreaID} -> {i.ID} Was not found in spoiler log, Unrandomizing.");
                    i.RandomizedState = MiscData.RandomizedState.Unrandomized;
                }
            }
        }

    }
}
