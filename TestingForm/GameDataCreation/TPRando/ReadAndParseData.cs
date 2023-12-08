using MathNet.Symbolics;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.SpoilerLogImporter;
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
using TestingForm;

namespace MMR_Tracker_V3.GameDataCreation.TPRando
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

        private static string[] StartingItems = new string[]
        {
            "Shadow Crystal",
            "Progressive Sword",
            "Boomerang",
            "Lantern",
            "Slingshot",
            "Progressive Fishing Rod",
            "Iron Boots",
            "Progressive Bow",
            "Filled Bomb Bag",
            "Zora Armor",
            "Progressive Clawshot",
            "Aurus Memo",
            "Asheis Sketch",
            "Spinner",
            "Ball and Chain",
            "Progressive Dominion Rod",
            "Progressive Sky Book",
            "Horse Call",
            "Gate Keys",
            "Empty Bottle",
            "Ordon Shield",
            "Hylian Shield"
        };
        private static string[] JunkItems = new string[] {
            "Bombs_5",
            "Bombs_10",
            "Bombs_20",
            "Bombs_30",
            "Arrows_10",
            "Arrows_20",
            "Arrows_30",
            "Seeds_50",
            "Water_Bombs_5",
            "Water_Bombs_10",
            "Water_Bombs_15",
            "Bomblings_5",
            "Bomblings_10",
            "Blue_Rupee",
            "Yellow_Rupee",
            "Red_Rupee",
            "Purple_Rupee",
            "Orange_Rupee"
        };

        private static string[] AdditionalItems = new string[] { "Lantern", "Gate_Keys", "Shadow_Crystal", "Foolish_Item" };

        private static LogicStringParser TPLogicParser = new LogicStringParser(LogicStringParser.OperatorType.PyStyle, quotes: '\'');

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
            CreateAdditionalData(TRPLogic, TRPDictionary);
            AlterSpecificLogic(TRPLogic);

            CreateHowlingStoneChecks(TRPLogic, TRPDictionary);

            string FinalDictFile = Path.Combine(TestingReferences.GetTestingDictionaryPath(), @"TPR V1.json");
            string FinalLogicFile = Path.Combine(TestingReferences.GetTestingLogicPresetsPath(), @"DEV-TPR Casual.json");
            File.WriteAllText(FinalLogicFile, JsonConvert.SerializeObject(TRPLogic, Utility._NewtonsoftJsonSerializerOptions));
            File.WriteAllText(FinalDictFile, JsonConvert.SerializeObject(TRPDictionary, Utility._NewtonsoftJsonSerializerOptions));
        }

        private static void CreateHowlingStoneChecks(MMRData.LogicFile tRPLogic, LogicDictionaryData.LogicDictionary tRPDictionary)
        {
            CreateHowlingStone("Snowpeak Blizzard", "Kakariko Graveyard Golden Wolf", "Snowpeak Province");
            CreateHowlingStone("Lake Hylia", "Gerudo Desert Golden Wolf", "Lake Hylia");
            CreateHowlingStone("Hidden Village", "North Castle Town Golden Wolf", "Hidden Village");
            CreateHowlingStone("North Faron Woods", "Outside South Castle Town Golden Wolf", "Faron Woods");
            CreateHowlingStone("Upper Zoras River", "West Hyrule Field Golden Wolf", "Zoras Domain");
            CreateHowlingStone("Death Mountain", "Ordon Spring Golden Wolf", "Death Mountain");

            void CreateHowlingStone(string v, string WolfCheck, string Area)
            {
                string ID = $"{v} Howling Stone";
                tRPDictionary.LocationList.Add(ID, new LogicDictionaryData.DictionaryLocationEntries { Area = Area, ID = ID, Name = ID, OriginalItem = ID, ValidItemTypes = new string[] { ID } });
                tRPDictionary.ItemList.Add(ID, new LogicDictionaryData.DictionaryItemEntries { ID = ID, Name = ID, ItemTypes = new string[] { ID }, MaxAmountInWorld = 1, ValidStartingItem = false });

                var GoldenWolfLogic = tRPLogic.Logic.Find(x => x.Id == WolfCheck);

                tRPLogic.Logic.Add(new MMRData.JsonFormatLogicItem
                {
                    Id= ID,
                    ConditionalItems = GoldenWolfLogic.ConditionalItems.Select(x => x).ToList(),
                    RequiredItems = GoldenWolfLogic.RequiredItems.Where(x => !x.Contains('\'') && x != "CanCompleteMDH").ToList(),
                });

                GoldenWolfLogic.ConditionalItems.Clear();
                GoldenWolfLogic.RequiredItems = GoldenWolfLogic.RequiredItems.Where(x => x.Contains('\'') || x == "CanCompleteMDH").ToList();
                GoldenWolfLogic.RequiredItems.Add(ID);
            }

        }

        private static void AlterSpecificLogic(MMRData.LogicFile tRPLogic)
        {
            List<Tuple<string, string>> LogicAlterations = new List<Tuple<string, string>>()
            {
               //new("canCompleteAllDungeons", "Progressive_Mirror_Shard, 3 and Progressive_Fused_Shadow, 3"),
               //new("canCompleteForestTemple", "check{Forest Temple Dungeon Reward}"),
               //new("canCompleteGoronMines", "check{Goron Mines Dungeon Reward}"),
               //new("canCompleteLakebedTemple", "check{Lakebed Temple Dungeon Reward}"),
               //new("canCompleteSnowpeakRuins", "check{Snowpeak Ruins Dungeon Reward}"),
               //new("canCompleteArbitersGrounds", "check{Arbiters Grounds Stallord Heart Container}"),
               //new("canCompleteTempleofTime", "check{Temple of Time Dungeon Reward}"),
               //new("canCompleteCityinTheSky", "check{City in The Sky Dungeon Reward}"),
               //new("canCompletePalaceofTwilight", "check{Palace of Twilight Zant Heart Container}"),
            };
            foreach(var item in LogicAlterations)
            {
                var Target = tRPLogic.Logic.First(x => x.Id == item.Item1);
                Target.RequiredItems.Clear();
                Target.ConditionalItems.Clear();
                Target.ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(TPLogicParser, item.Item2, item.Item1);
                LogicUtilities.RemoveRedundantConditionals(Target);
                LogicUtilities.MakeCommonConditionalsRequirements(Target);
            }
        }

        private static void CreateAdditionalData(MMRData.LogicFile tRPLogic, LogicDictionaryData.LogicDictionary tRPDictionary)
        {
            string[] ItemWheelItems = new string[] { "Progressive_Clawshot", "Progressive_Dominion_Rod", "Ball_and_Chain", "Spinner", "Progressive_Bow", "Iron_Boots",
                "Boomerang", "Lantern", "Slingshot", "Progressive_Fishing_Rod", "Hawkeye", "Filled_Bomb_Bag", "Empty_Bottle",
                "Jovani_Bottle", "Sera_Bottle", "Coro_Bottle", "Aurus_Memo", "Renados_Letter", "Horse_Call" };

            string[] Bugs = new string[] { "Female_Phasmid", "Male_Phasmid", "Female_Grasshopper", "Male_Grasshopper", "Female_Pill_Bug", "Male_Pill_Bug",
                "Male_Ant", "Female_Ant", "Female_Beetle", "Male_Beetle", "Female_Snail", "Male_Snail", "Female_Dayfly", "Male_Dayfly", "Female_Mantis", "Male_Mantis",
                "Female_Stag_Beetle", "Male_Stag_Beetle", "Female_Ladybug", "Male_Ladybug", "Female_Dragonfly", "Female_Butterfly", "Male_Butterfly", "Male_Dragonfly" };

            tRPDictionary.LogicEntryCollections.Add("ItemWheelItems", new OptionData.LogicEntryCollection { ID = "ItemWheelItems", Entries = ItemWheelItems.ToList() });
            tRPDictionary.LogicEntryCollections.Add("AllBugs", new OptionData.LogicEntryCollection { ID = "AllBugs", Entries = Bugs.ToList() });

            tRPLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = "HasBug", RequiredItems = new List<string> { "AllBugs, 1" } });
        }

        private static void CreateOptions(LogicDictionaryData.LogicDictionary tRPDictionary)
        {
            Dictionary<string, string> OptionPrettyName = new Dictionary<string, string>
            {
                { "Fused_Shadows", "Fused Shadows" },
                { "Mirror_Shards","Mirror Shards"},
                { "All_Dungeons","All Dungeons"},
                { "vanilla","Vanilla"},
                { "ownDungeon","Own Dungeon"},
                { "anyDungeon", "Any Dungeon"},
                { "anywhere","Anywhere"},
                { "NoWrestling","No Wrestling"},
                { "OpenGrove","Open Grove"}
            };

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
                if (Values is null) 
                {
                    tRPDictionary.ToggleOptions.Add(ID, new OptionData.ToggleOption
                    {
                        Name = Name,
                        SubCategory = Category,
                        Enabled = new OptionData.OptionValue { ID = true.ToString(), Name = true.ToString() },
                        Disabled = new OptionData.OptionValue { ID = false.ToString(), Name = false.ToString() },
                        ID= ID,
                        Value = false.ToString()
                    });
                }
                else
                {
                    var Option = new OptionData.ChoiceOption
                    {
                        ID = ID,
                        Name= Name,
                        Value = Values[0],
                        SubCategory= Category,
                    };
                    Option.CreateSimpleValues(Values);
                    foreach (var Value in Option.ValueList)
                    {
                        if (OptionPrettyName.ContainsKey(Value.Key)) { Value.Value.Name = OptionPrettyName[Value.Key]; }
                    }
                    tRPDictionary.ChoiceOptions.Add(ID, Option);
                }
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

        private static Dictionary<string, int> GetItemCOunts(List<CheckData> checkData)
        {
            Dictionary<string, int> ExtraItems = new Dictionary<string, int>()
            {
                { "Progressive_Sword", 1 },         //Sword from master sword pedestal
                { "Progressive_Dominion_Rod", 1  },  //Restored Dominion Rod, The valilla check seems to be removed and having all the sky books just moves the canon?
                //These staring Items are odd so just increase the max amount until I have a better Solution
                { "Asheis_Sketch", 1  },
                { "Aurus_Memo", 1  },
                { "Filled_Bomb_Bag", 1  },
                { "Horse_Call", 1  },
                { "Progressive_Fishing_Rod", 1  },
                { "Progressive_Sky_Book", 7  },
                { "Zora_Armor", 1  },
            };
            Dictionary<string, int> ItemCounts = new Dictionary<string, int>();
            foreach (var check in checkData)
            {
                if (!ItemCounts.ContainsKey(check.itemId)) { ItemCounts.Add(check.itemId, 1); }
                else { ItemCounts[check.itemId]++; }
                if (JunkItems.Contains(check.itemId)) { ItemCounts[check.itemId] = -1; }
            }
            foreach(var check in AdditionalItems) 
            {
                int Amount = check == "Foolish_Item" ? -1 : 1;
                ItemCounts.Add(check, Amount);
            }
            foreach (var check in ExtraItems)
            {
                ItemCounts[check.Key] += check.Value;
            }
            return ItemCounts;
        }

        private static void CreateDictionaryAndLogicFile(Dictionary<string, WorldGraph> worldGRaph, List<CheckData> checkData, out MMRData.LogicFile TRPLogic, out LogicDictionaryData.LogicDictionary TRPDictionary)
        {
            Dictionary<string, int> ItemCounts = GetItemCOunts(checkData);
            TRPLogic = new() { GameCode = "TPR", Logic = new List<MMRData.JsonFormatLogicItem>(), Version = 1 };
            TRPDictionary = new() { LogicVersion = 1, GameCode = "TPR", RootArea = "Ordon Province", WinCondition = "Triforce" };
            foreach (var graph in worldGRaph)
            {
                foreach(var loc in graph.Value.Locations)
                {
                    var CheckData = checkData.Find(x => x.name == loc.Key);
                    //graph.key is the area and needs '' around it for other hacky stuff in the logic praser
                    string Logic = $"('{graph.Key}') and ({loc.Value})";
                    MMRData.JsonFormatLogicItem logicItem = new MMRData.JsonFormatLogicItem()
                    {
                        Id = loc.Key,
                        ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(TPLogicParser, Logic, loc.Key)
                    };
                    LogicDictionaryData.DictionaryLocationEntries locationEntry = new()
                    {
                        ID = loc.Key,
                        Area = graph.Value.Region,
                        Name = loc.Key,
                        OriginalItem = CheckData.itemId,
                        SpoilerData = new MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { loc.Key } },
                        ValidItemTypes = new string[] { "item" }.Concat(CheckData.category).ToArray()
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
                        ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(TPLogicParser, Exit.Value, ExitID)
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
            foreach(var i in checkData.Select(x => x.itemId).Concat(AdditionalItems))
            {
                if (TRPDictionary.ItemList.Any(x => x.Key == i)) { continue; }
                LogicDictionaryData.DictionaryItemEntries itemEntry = new()
                {
                    ID = i,
                    Name = i.Replace("_", " "),
                    SpoilerData = new MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { i, i.Replace("_", " ") } },
                    MaxAmountInWorld = ItemCounts[i],
                    ItemTypes = new string[] { "item" },
                };
                itemEntry.ValidStartingItem = itemEntry.SpoilerData.SpoilerLogNames.Intersect(StartingItems).Any();
                TRPDictionary.ItemList.Add(i, itemEntry);
            }

            TRPLogic.Logic.AddRange(ParseMacrosFromCodeV2.ReadLines());


            TRPLogic.Logic.Add(new MMRData.JsonFormatLogicItem()
            {
                Id = "DefeatGanon",
                ConditionalItems = new List<List<string>> { new List<string> { "Ganondorf Castle" } }
            });
            TRPDictionary.LocationList.Add("DefeatGanon", new()
            {
                ID = "DefeatGanon",
                Area = "Hyrule Castle",
                Name = "Ganondorf",
                OriginalItem = "Triforce",
                SpoilerData = new MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { "DefeatGanon", "Ganondorf" } },
                ValidItemTypes = new string[] { "GameClear" }
            });
            TRPDictionary.ItemList.Add("Triforce", new()
            {
                ID = "Triforce",
                Name = "Triforce",
                SpoilerData = new MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { "Triforce" } },
                MaxAmountInWorld = 1,
                ItemTypes = new string[] { "GameClear" },
                ValidStartingItem= false,
            });
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
            string WorldFiles = Path.Combine(TestingReferences.GetDevTestingPath(), "TPRando", "Randomizer-Web-Generator-development", "Generator", "World");
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
            string WorldFiles = Path.Combine(TestingReferences.GetDevTestingPath(), "TPRando", "Randomizer-Web-Generator-development", "Generator", "World");
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
