using MathNet.Symbolics;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames.TPRando
{
    public class ReadAndParseData
    {
        private class CheckData
        {
            public string name { get; set; }
            public string requirements { get; set; }
            public string[] category { get; set; }
            public string itemId { get; set; }
        }
        private class RoomData
        {
            public string Area { get; set; }
            public string[] Neighbours { get; set; }
            public string[] NeighbourRequirements { get; set; }
            public string[] Checks { get; set; }
            public string Region { get; set; }
        }
        private class WorldGraph
        {
            public string Region { get; set; }
            public Dictionary<string, string> Locations { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> Exits { get; set; } = new Dictionary<string, string>();
        }

        public class TPRSpoilerLog
        {
            public string[] requiredDungeons { get; set; }
            public Dictionary<string, string> itemPlacements { get; set; }
            public Dictionary<string, object> settings { get; set; }
        }

        private static LogicStringParser TPLogicParser = new LogicStringParser(LogicStringParser.OperatorType.PyStyle);

        public static void CreateFiles(out MMRData.LogicFile TRPLogic, out LogicDictionaryData.LogicDictionary TRPDictionary)
        {
            var CheckData = GetCheckData();
            var RoomData = GetRoomData();
            var WorldGRaph = CreateWorldGraph(CheckData, RoomData);
            var AllItems = CheckData.Select(x => x.itemId).Distinct().ToList();
            //CompareDataToSpoilerLog(CheckData, AllItems);
            CreateDictionaryAndLogicFile(WorldGRaph, CheckData, out TRPLogic, out TRPDictionary);
            FormatLogic(TRPLogic);
            CreateOptions(TRPDictionary);
            string TestFolder = Path.Combine(References.TestingPaths.GetDevTestingPath(), "TPRando");
            string FinalDictFile = Path.Combine(TestFolder,  @"TPR V1.json");
            string FinalLogicFile = Path.Combine(TestFolder, @"DEV-TPR Casual.json");
            File.WriteAllText(FinalLogicFile, JsonConvert.SerializeObject(TRPLogic, Testing._NewtonsoftJsonSerializerOptions));
            File.WriteAllText(FinalDictFile, JsonConvert.SerializeObject(TRPDictionary, Testing._NewtonsoftJsonSerializerOptions));
        }

        private static void CreateOptions(LogicDictionaryData.LogicDictionary tRPDictionary)
        {
            CreateOption("castleRequirements", "Hyrule Castle Requirements", "Access Options", new string[] { "Vanilla", "Open", "Fused_Shadows", "Mirror_Shards", "All_Dungeons" });
            CreateOption("palaceRequirements", "Palace of Twilight Requirements", "Access Options", new string[] { "Vanilla", "Open", "Fused_Shadows", "Mirror_Shards" });
            CreateOption("faronWoodsLogic", "Open Faron Woods", "Access Options", new string[] { "Closed", "Open" });

            CreateOption("shuffleNpcItems", "Shuffle Gifts From NPCs", "Misc");

            CreateOption("smallKeySettings", "Small Keys", "Dungeon Items", new string[] { "vanilla", "ownDungeon", "anyDungeon", "anywhere", "Keysy" });
            CreateOption("bigKeySettings", "Big Keys", "Dungeon Items", new string[] { "vanilla", "ownDungeon", "anyDungeon", "anywhere", "Keysy" });

            CreateOption("skipPrologue", "Skip Prologue", "Timesavers");
            CreateOption("faronTwilightCleared", "Faron Twilight Cleared", "Timesavers");
            CreateOption("eldinTwilightCleared", "Eldin Twilight Cleared", "Timesavers");
            CreateOption("lanayruTwilightCleared", "Lanayru Twilight Cleared", "Timesavers");
            CreateOption("skipMdh", "Skip Midna's Desperate Hour", "Timesavers");
            CreateOption("barrenDungeons", "Unrequired Dungeons Are Barren", "Timesavers");
            CreateOption("openDot", "Open Door of Time", "Timesavers");

            CreateOption("skipLakebedEntrance", "Lakebed Does Not Require Water Bombs", "Dungeon Entrance Settings");
            CreateOption("skipArbitersEntrance", "Arbiters Does Not Require Bulblin Camp", "Dungeon Entrance Settings");
            CreateOption("skipSnowpeakEntrance", "Snowpeak Does Not Require Reekfish Scent", "Dungeon Entrance Settings");
            CreateOption("skipCityEntrance", "City Does Not Require Filled Skybook", "Dungeon Entrance Settings");
            CreateOption("goronMinesEntrance", "Goron Mines Entrance", "Dungeon Entrance Settings", new string[] { "Closed", "NoWrestling", "Open" });
            CreateOption("totEntrance", "Temple of Time Entrance", "Dungeon Entrance Settings", new string[] { "Closed", "OpenGrove", "Open" });

            CreateOption("transformAnywhere", "Transform Anywhere", "Additional Settings");
            CreateOption("increaseWallet", "Increase Wallet Capacity", "Additional Settings");

            CreateOption("logicRules", "logicRules", "static", new string[] { "Glitchless" }); //Always true for this logic
            CreateOption("skipMinesEntrance", "", "Dungeon Entrance Settings", new string[] { "False" }); //Leftover?

            void CreateOption(string ID, string Name, string Category, string[] Values = null)
            {
                if (Values is null) { Values = new string[] { "False", "True" }; }
                var Option = new OptionData.TrackerOption
                {
                    ID = ID,
                    DisplayName= Name,
                    CurrentValue = Values[0],
                    SubCategory= Category,
                };
                Option.CreateSimpleValues(Values);
                tRPDictionary.Options.Add(ID, Option);
            }
        }

        private static void FormatLogic(MMRData.LogicFile tRPLogic)
        {
            foreach(var logicEntry in tRPLogic.Logic)
            {
                List<List<string>> NewConditionalSet = new List<List<string>>();
                foreach(var Set in logicEntry.ConditionalItems)
                {
                    List<string> NewCond = new List<string>();
                    foreach(var c in Set)
                    {
                        NewCond.Add(ParseLogicItem(c));
                    }
                    NewConditionalSet.Add(NewCond);
                }
                logicEntry.ConditionalItems = NewConditionalSet;
                LogicUtilities.RemoveRedundantConditionals(logicEntry);
                LogicUtilities.MakeCommonConditionalsRequirements(logicEntry);
            }
        }

        private static string ParseLogicItem(string c)
        {
            if (c.StartsWith($"Setting."))
            {
                string SettingLine = c.Replace("Setting.", "");
                SettingLine = SettingLine.Replace(" equals ", ", ");
                SettingLine = $"option{{{SettingLine}}}";
                return SettingLine;
            }
            else if (c.StartsWith($"Room."))
            {
                string AreaAccessLine = c.Replace("Room.", "");
                AreaAccessLine = AreaAccessLine.Replace("_", " ");
                return AreaAccessLine;
            }
            return c;
        }

        private static void CreateDictionaryAndLogicFile(Dictionary<string, WorldGraph> worldGRaph, List<CheckData> checkData, out MMRData.LogicFile TRPLogic, out LogicDictionaryData.LogicDictionary TRPDictionary)
        {
            string Macros = Path.Combine(References.TestingPaths.GetDevCodePath(), @"MMR Tracker V3", "OtherGames", "TPRando", @"Macros.json");
            TRPLogic = new() { GameCode = "TPR", Logic = new List<MMRData.JsonFormatLogicItem>(), Version = 1 };
            TRPDictionary = new() { LogicVersion = 1, GameCode = "TPR", RootArea = "Ordon Province" };
            foreach (var graph in worldGRaph)
            {
                foreach(var loc in graph.Value.Locations)
                {
                    var CheckData = checkData.Find(x => x.name == loc.Key);
                    string Logic = $"('{graph.Key}') and ({loc.Value})";
                    MMRData.JsonFormatLogicItem logicItem = new MMRData.JsonFormatLogicItem()
                    {
                        Id = loc.Key,
                        ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(TPLogicParser, Logic)
                    };
                    LogicDictionaryData.DictionaryLocationEntries locationEntry = new()
                    {
                        ID = loc.Key,
                        Area = graph.Value.Region,
                        Name = loc.Key,
                        OriginalItem = CheckData.itemId,
                        SpoilerData = new MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { loc.Key } },
                        ValidItemTypes = new string[] { "item" }
                    };
                    TRPLogic.Logic.Add(logicItem);
                    TRPDictionary.LocationList.Add(loc.Key, locationEntry);
                }
                foreach(var Exit in graph.Value.Exits)
                {
                    string ExitID = $"{graph.Key} => {Exit.Key}";
                    MMRData.JsonFormatLogicItem logicItem = new MMRData.JsonFormatLogicItem()
                    {
                        Id = ExitID,
                        ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(TPLogicParser, Exit.Value)
                    };
                    LogicDictionaryData.DictionaryEntranceEntries exitEntry = new()
                    {
                        ID = ExitID,
                        Area = graph.Key,
                        Exit = Exit.Key,
                        RandomizableEntrance = false
                    };
                    TRPLogic.Logic.Add(logicItem);
                    TRPDictionary.EntranceList.Add(ExitID, exitEntry);
                }
            }
            var AdditionalItems = new string[] { "Lantern", "Gate_Keys", "Shadow_Crystal" };
            foreach(var i in checkData.Select(x => x.itemId).Concat(AdditionalItems))
            {
                if (TRPDictionary.ItemList.Any(x => x.Key == i)) { continue; }
                LogicDictionaryData.DictionaryItemEntries itemEntry = new()
                {
                    ID = i,
                    Name = i.Replace("_", " "),
                    SpoilerData = new MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { i, i.Replace("_", " ") } },
                    MaxAmountInWorld = -1,
                    ItemTypes = new string[] { "item" },
                    ValidStartingItem= true,
                };
                TRPDictionary.ItemList.Add(i, itemEntry);
            }
            foreach(var i in JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Macros)))
            {
                MMRData.JsonFormatLogicItem logicItem = new MMRData.JsonFormatLogicItem()
                {
                    Id = i.Key,
                    ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(TPLogicParser, i.Value)
                };
                TRPLogic.Logic.Add(logicItem);
            }
        }

        private static void CompareDataToSpoilerLog(List<CheckData> checkData, List<string> AllItems)
        {
            List<string> allSpoilerlogItems = new List<string>();
            string TestSpoilerLog = Path.Combine(References.TestingPaths.GetDevTestingPath(), "TPRando", "TestSpoiler.json");
            var SpoilerData = Newtonsoft.Json.JsonConvert.DeserializeObject<TPRSpoilerLog>(File.ReadAllText(TestSpoilerLog));

            Debug.WriteLine("\nLocations in spoiler log not found in Pool");
            foreach(var i in SpoilerData.itemPlacements)
            {
                if (!allSpoilerlogItems.Contains(i.Value)) { allSpoilerlogItems.Add(i.Value); }
                if (!checkData.Any(x => x.name == i.Key)) { Debug.WriteLine(i); }
            }
            Debug.WriteLine("\n\nLocations in Pool not found in Spoiler Log");
            foreach (var i in checkData.Select(x => x.name))
            {
                if (!SpoilerData.itemPlacements.ContainsKey(i)) { Debug.WriteLine(i); }
            }
            Debug.WriteLine("\n\nItems in spoiler log not found in Pool");
            foreach (var i in allSpoilerlogItems)
            {
                if (!AllItems.Contains(i)) { Debug.WriteLine(i); }
            }

            Debug.WriteLine("\n\nItems in Pool not found in Spoiler Log");
            foreach (var i in AllItems)
            {
                if (!allSpoilerlogItems.Contains(i)) { Debug.WriteLine(i); }
            }

        }

        private static Dictionary<string, WorldGraph> CreateWorldGraph(List<CheckData> checkData, List<RoomData> roomData)
        {
            Dictionary<string, WorldGraph> worldGraphs = new Dictionary<string, WorldGraph>();
            foreach(var room in roomData)
            {
                Debug.WriteLine($"Adding {room.Area} to graph");
                var graph = new WorldGraph();
                graph.Region = room.Region;
                foreach(var i in room.Checks)
                {
                    if (string.IsNullOrWhiteSpace(i)) { continue; }
                    var check = checkData.FirstOrDefault(x => x.name == i);
                    if (check == null)
                    {
                        throw new Exception($"Check Data did not contain check named {i}");
                    }
                    graph.Locations.Add(check.name, check.requirements);
                }
                graph.Exits = room.Neighbours.Zip(room.NeighbourRequirements, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
                worldGraphs.Add(room.Area, graph);
            }
            return worldGraphs;
        }

        private static List<RoomData> GetRoomData()
        {
            string WorldFiles = Path.Combine(References.TestingPaths.GetDevTestingPath(), "TPRando", "Randomizer-Web-Generator-main", "Generator", "World");
            string Rooms = Path.Combine(WorldFiles, "Rooms");

            List<RoomData> AllRooms = new List<RoomData>();
            ScanChecksDir(Rooms);
            return AllRooms;

            void ScanChecksDir(string path)
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    ScanChecksDir(dir);
                }
                foreach (var file in Directory.GetFiles(path))
                {
                    string AreaName = Path.GetFileNameWithoutExtension(file);
                    Debug.WriteLine($"Scanning Room {AreaName}");
                    RoomData roomData = Newtonsoft.Json.JsonConvert.DeserializeObject<RoomData>(File.ReadAllText(file));
                    roomData.Area = AreaName;
                    AllRooms.Add(roomData);
                }
            }
        }

        private static List<CheckData> GetCheckData()
        {
            string WorldFiles = Path.Combine(References.TestingPaths.GetDevTestingPath(), "TPRando", "Randomizer-Web-Generator-main", "Generator", "World");
            string Checks = Path.Combine(WorldFiles, "Checks");
            string Rooms = Path.Combine(WorldFiles, "Rooms");

            List<CheckData> AllChecks = new List<CheckData>();
            ScanChecksDir(Checks);
            return AllChecks;

            void ScanChecksDir(string path)
            {
                foreach(var dir in Directory.GetDirectories(path))
                {
                    ScanChecksDir(dir);
                }
                foreach(var file in Directory.GetFiles(path))
                {
                    string CheckName = Path.GetFileNameWithoutExtension(file);
                    Debug.WriteLine($"Scanning Check {CheckName}");
                    CheckData checkData = Newtonsoft.Json.JsonConvert.DeserializeObject<CheckData>(File.ReadAllText(file));
                    checkData.name = CheckName;
                    AllChecks.Add(checkData);
                }
            }

        }
    }
}
