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
    public class EntranceTable
    {
        public string[] CoupledEntrances { get; set; }
        public string[] OneWayEntrances { get; set; }
        public Dictionary<string, string> VanillaLocations { get; set; }
        public Dictionary<string, string> UnrandomizableLocations { get; set; }
        public string[] GossipStones { get; set; }
        public string[] ItemPool { get; set; }
        public Dictionary<string, string> LogicOverrides { get; set; }
        public Dictionary<string, string> StaticMacros { get; set; }
        public Dictionary<string, string[]> Options { get; set; }
        public Dictionary<string, string[]> StaticLocations { get; set; } = new Dictionary<string, string[]>();
        public Dictionary<string, string> StaticItems { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string[]> LogicTricks { get; set; } = new Dictionary<string, string[]>();
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

            string EntranceSeperator = " X ";

            string[] WarpExits =
            {
                $"Minuet of Forest Warp{EntranceSeperator}Sacred Forest Meadow",
                $"Bolero of Fire Warp{EntranceSeperator}DMC Central Local",
                $"Serenade of Water Warp{EntranceSeperator}Lake Hylia",
                $"Requiem of Spirit Warp{EntranceSeperator}Desert Colossus",
                $"Nocturne of Shadow Warp{EntranceSeperator}Graveyard Warp Pad Region",
                $"Prelude of Light Warp{EntranceSeperator}Temple of Time",
                $"Root Exits{EntranceSeperator}Child Spawn",
                $"Root Exits{EntranceSeperator}Adult Spawn",
            };

            foreach(var i in TestTable.Options)
            {
                OORTDict.Options.Add(new OptionData.TrackerOption
                {
                    ID = i.Key,
                    DisplayName = i.Key,
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
                    if (TestTable.GossipStones.Contains(l.Key))
                    {
                        LogicDictionaryData.DictionaryHintEntries hintEntry = new LogicDictionaryData.DictionaryHintEntries();
                        hintEntry.ID = l.Key;
                        hintEntry.Name = l.Key;
                        hintEntry.SpoilerData.SpoilerLogNames = new string[] { l.Key, "TODO" };
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

            foreach(var i in TestTable.VanillaLocations)
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

            foreach (var i in TestTable.StaticLocations)
            {
                Condesedlogic.Add(i.Key, i.Value[1]);
                LogicDictionaryData.DictionaryLocationEntries locationObject = new LogicDictionaryData.DictionaryLocationEntries();
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
                logicItem.ConditionalItems = CleanLogic( LogicStringParser.ConvertLogicStringToConditional(CurrentLogic) );
                logicCleaner.RemoveRedundantConditionals(logicItem);
                logicCleaner.MakeCommonConditionalsRequirements(logicItem);
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

                    if (MQData) { foreach (var t in tempLogic) { t.MQ = true; } }
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

    }
}
