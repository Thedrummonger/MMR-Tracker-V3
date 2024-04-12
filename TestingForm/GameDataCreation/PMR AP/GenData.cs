using Microsoft.VisualBasic.Logging;
using MMR_Tracker_V3;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingForm.GameDataCreation.PMR_AP
{
    public class GenData
    {
        public static string GetItemType(List<object> e) { return Utility.SerializeConvert<string>(e[0]); }
        public static string GetItemProgression(List<object> e) { return Utility.SerializeConvert<string>(e[1]); }
        public static int GetItemID(List<object> e) { return Utility.SerializeConvert<int>(e[2]); }
        public static int GetItemBasePrice(List<object> e) { return Utility.SerializeConvert<int>(e[3]); }
        public static bool IsItemUnused(List<object> e) { return Utility.SerializeConvert<bool>(e[4]); }
        public static bool IsItemDupe(List<object> e) { return Utility.SerializeConvert<bool>(e[5]); }
        public static bool IsItemNotPlaced(List<object> e) { return Utility.SerializeConvert<bool>(e[6]); }
        public static string GetLocationFullID(List<object> e) { return Utility.SerializeConvert<string>(e[0]); }
        public static string GetLocationType(List<object> e) { return Utility.SerializeConvert<string>(e[1]); }
        public static int GetLocationAreaID(List<object> e) { return Utility.SerializeConvert<int>(e[2]); }
        public static int GetLocationMapID(List<object> e) { return Utility.SerializeConvert<int>(e[3]); }
        public static int GetLocationMapAreaID(List<object> e) { return Utility.SerializeConvert<int>(e[4]); }
        public static string GetLocationVanillaItem(List<object> e) { return Utility.SerializeConvert<string>(e[6]); }
        public static string GetLocationKeyName(List<object> e) { return Utility.SerializeConvert<string>(e[7]); }
        public static void ReadAndGenData(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary)
        {
            //need to set | as the 'Quote' char because the area names don't follow standard logic convention and
            //break the parser a standard ' also can't be used because some area names use it
            //All areas need to be wrapped in | and that needs to be stripped off later.
            LogicStringParser logicStringParser = new LogicStringParser(LogicStringParser.OperatorType.PyStyle, quotes: '|');
            var PMRRegionsFolder = Path.Combine(TestingReferences.GetDevTestingPath(), "PMR", "PMR_APWorld-main", "data", "regions");

            var Items = TestingUtility.DeserializeJsonFile<Dictionary<string, List<object>>>
                (Path.Combine(TestingReferences.GetOtherGameDataPath("PMR AP"), "Items.json"));
            var Locations = TestingUtility.DeserializeJsonFile<Dictionary<string, List<object>>>
                (Path.Combine(TestingReferences.GetOtherGameDataPath("PMR AP"), "Locations.json"));
            var Macros = TestingUtility.DeserializeJsonFile<Dictionary<string, string>>
                (Path.Combine(TestingReferences.GetDevTestingPath(), "PMR", "PMR_APWorld-main", "data", "LogicHelpers.json"));

            dictionary = new LogicDictionaryData.LogicDictionary()
            {
                GameCode = "PMRAP",
                RootAreas = ["Menu"],
                GameName = "Paper Mario",
                LogicVersion = 1,
                NetPlaySupported = true,
            };
            Logic = new MMRData.LogicFile() { GameCode = "PMRAP", Version = 1, Logic = [] };
            foreach (var Loc in Locations)
            {
                var locationData = new LogicDictionaryData.DictionaryLocationEntries()
                {
                    ID = Loc.Key,
                    Area = $"{GetLocationAreaID(Loc.Value)}|{GetLocationMapID(Loc.Value)}",
                    Name = Loc.Key,
                    OriginalItem = GetLocationVanillaItem(Loc.Value),
                    ValidItemTypes = ["item"],
                    Repeatable = GetLocationType(Loc.Value) == "Shop Item",
                    SpoilerData = new() { NetIDs = [Loc.Key], Tags = [GetLocationType(Loc.Value), GetLocationFullID(Loc.Value)] }
                };
                dictionary.LocationList.Add(Loc.Key, locationData);
            }
            foreach (var Item in Items)
            {
                if (IsItemNotPlaced(Item.Value) || IsItemDupe(Item.Value)) { continue; }
                int Max = GetItemProgression(Item.Value) == "filler" && GetItemType(Item.Value) == "ITEM" ? -1 : 1;
                var ItemData = new LogicDictionaryData.DictionaryItemEntries()
                {
                    ID = Item.Key.Replace(" ", "_"),
                    MaxAmountInWorld = Max,
                    Name = Item.Key,
                    ItemTypes = ["item"],
                    ValidStartingItem = true,
                    SpoilerData = new() { NetIDs = [Item.Key, Item.Key.Replace(" ", "_")], Tags = [GetItemProgression(Item.Value), GetItemType(Item.Value)] }
                };
                dictionary.ItemList.Add(Item.Key.Replace(" ", "_"), ItemData);
            }

            Dictionary<string, string> RegionsDict = [];
            foreach(var File in Directory.GetFiles(PMRRegionsFolder))
            {
                if (Path.GetFileNameWithoutExtension(File).In("bowser's_castle_boss_rush", "bowser's_castle_shortened")) { continue; }
                var RegionFile = TestingUtility.DeserializeJsonFile<List<PMRRegion>>(File);
                foreach (var Region in RegionFile)
                {
                    RegionsDict[$"{Region.area_id}|{Region.map_id}"] = Path.GetFileNameWithoutExtension(File).Replace("_", " ").ConvertToCamelCase();
                    foreach(var Location in Region.locations)
                    {
                        var ID = Location.Key;
                        if (!dictionary.LocationList.ContainsKey(ID)) { Debug.WriteLine($"Bad Logic Line? {ID}"); }
                        AddLocationLogicEntry(Logic, Region, ID, Location.Value);
                    }
                    foreach(var Event in Region.events)
                    {
                        AddLocationLogicEntry(Logic, Region, Event.Key, Event.Value);
                    }
                    foreach (var Exits in Region.exits)
                    {
                        string ID = $"{Region.region_name} => {Exits.Key}";
                        Logic.Logic.Add(new MMRData.JsonFormatLogicItem
                        {
                            Id = ID,
                            ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(logicStringParser, Exits.Value, ID)
                        });
                        dictionary.EntranceList.Add(ID, new LogicDictionaryData.DictionaryEntranceEntries()
                        {
                            ID = ID,
                            Area = Region.region_name,
                            Exit = Exits.Key,
                            RandomizableEntrance = false
                        });
                    }
                }
            }
            foreach (var i in Macros)
            {
                Logic.Logic.Add(new MMRData.JsonFormatLogicItem
                {
                    Id = i.Key,
                    ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(logicStringParser, i.Value, i.Key)
                });
            }
            Logic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = "RF_BrokenVeranda", RequiredItems = [$"setting{{starting_map, goomba_village}}"] });

            dictionary.LogicEntryCollections.Add("Letter", new OptionData.LogicEntryCollection()
            {
                ID = "Letter",
                Entries = dictionary.ItemList.Keys.Where(x => x.StartsWith("Letter_to_")).ToList()
            });
            dictionary.LogicEntryCollections.Add("Magical_Seeds", new OptionData.LogicEntryCollection()
            {
                ID = "Magical_Seeds",
                Entries = dictionary.ItemList.Keys.Where(x => x.StartsWith("Magical_Seed") || x.StartsWith("MagicalSeed")).ToList()
            });
            dictionary.LogicEntryCollections.Add("STARSPIRITS", new OptionData.LogicEntryCollection()
            {
                ID = "STARSPIRITS",
                Entries = Enumerable.Range(1, 7).Select(x => $"STARSPIRIT_{x}").ToList()
            });

            dictionary.ChoiceOptions.Add("bowser_castle_mode", new OptionData.ChoiceOption(null)
            {
                ID = "bowser_castle_mode",
                Name = "Bowsers Castle Mode",
                Value = "vanilla"
            }.CreateSimpleValues(("vanilla", "Vanilla"), ("shortened", "Shortened"), ("boss_rush", "Boss Rush")));

            dictionary.ChoiceOptions.Add("gear_shuffle_mode", new OptionData.ChoiceOption(null)
            {
                ID = "gear_shuffle_mode",
                Name = "Gear Shuffle Mode",
                Value = "full_shuffle"
            }.CreateSimpleValues(("vanilla", "Vanilla"), ("gear_location_shuffle", "Location Shuffle"), ("full_shuffle", "Full Shuffle")));

            dictionary.ChoiceOptions.Add("hidden_block_mode", new OptionData.ChoiceOption(null)
            {
                ID = "hidden_block_mode",
                Name = "Hidden Block Mode",
                Value = "always_visible"
            }.CreateSimpleValues(("vanilla", "Vanilla"), ("watt_out", "Watt Out"), ("watt_obtained", "Watt Obtained"), ("always_visible", "Always Visible")));

            dictionary.ChoiceOptions.Add("starting_map", new OptionData.ChoiceOption(null)
            {
                ID = "starting_map",
                Name = "Starting Map",
                Value = "toad_town"
            }.CreateSimpleValues(("toad_town", "Toad Town"), ("goomba_village", "Goomba Village"), ("dry_dry_outpost", "Dry Dry Outpost"), ("yoshi_village", "Yoshi Village")));

            List<(string, string)> ToggleOptions = [
                ("ch7_bridge_visible", "Ch.7 Bridge Visible"),
                ("open_blue_house", "Open Blue House"),
                ("open_forest", "Open Forest"),
                ("open_mt_rugged", "Open Mt. Rugged"),
                ("open_prologue", "Open Prologue"),
                ("open_toybox", "Open Toy Box"),
                ("open_whale", "Open Whale"),
                ("partners_always_usable", "Partners Always Usable"),
                ("cook_without_frying_pan", "Cook Without Frying Pan")
                ];
            foreach(var i in ToggleOptions)
            {
                dictionary.ToggleOptions.Add(i.Item1, new OptionData.ToggleOption(null)
                {
                    ID = i.Item1,
                    Name = i.Item2,
                    Value = false
                }.CreateSimpleValues());
            }

            dictionary.IntOptions.Add("magical_seeds", new OptionData.IntOption(null)
            {
                ID = "magical_seeds",
                Max = 4,
                Min = 1,
                Value = 4,
                Name = "Magical Seeds Required"
            });
            dictionary.IntOptions.Add("star_spirits_required", new OptionData.IntOption(null)
            {
                ID = "star_spirits_required",
                Max = 7,
                Min = 1,
                Value = 7,
                Name = "Star Spirits Required"
            });

            var DictRef = dictionary;

            List<(string, string)> StartingMaps = [
                ("TT Gate District Mario's House Pipe", "toad_town"),
                ("GR Goomba Village", "goomba_village"),
                ("DDO Outpost 2", "dry_dry_outpost"),
                ("JJ Village Buildings", "yoshi_village")
                ];

            foreach(var i in StartingMaps)
            {
                dictionary.EntranceList.Add($"Menu => {i.Item1}", new()
                {
                    ID = $"Menu => {i.Item1}",
                    Area = "Menu",
                    Exit = i.Item1,
                });
                Logic.Logic.Add(new MMRData.JsonFormatLogicItem()
                {
                    Id = $"Menu => {i.Item1}",
                    RequiredItems = [$"setting{{starting_map, {i.Item2}}}"]
                });
            }

            foreach (var l in Logic.Logic)
            {
                LogicUtilities.TransformLogicItems(l, x =>
                {
                    if (x.StartsWith('|') && x.EndsWith('|')) { return x[1..^1]; }
                    if (x.StartsWith('\'') && x.EndsWith('\'')) { return x[1..^1]; }
                    if (x == "Magical_Seed, magical_seeds") {  return "Magical_Seeds, magical_seeds"; }
                    if (x == "'STARSPIRIT', star_spirits_required") { return "STARSPIRITS, star_spirits_required"; }
                    if (x.Contains(" < "))
                    {
                        var Data = x.TrimSplit("<");
                        var Option = DictRef.ChoiceOptions[Data[0]];
                        var Value = Option.ValueList.Keys.ToArray()[int.Parse(Data[1])];
                        return $"setting{{{Data[0]}, {Value}, false}}";
                    }
                    if (x.Contains(" == "))
                    {
                        var Data = x.TrimSplit("==");
                        var Option = DictRef.ChoiceOptions[Data[0]];
                        var Value = Option.ValueList.Keys.ToArray()[int.Parse(Data[1])];
                        return $"setting{{{Data[0]}, {Value}}}";
                    }
                    if (x.StartsWith("not ") && DictRef.ToggleOptions.TryGetValue(x.SplitOnce(' ').Item2, out OptionData.ToggleOption TOF))
                    {
                        return $"setting{{{TOF.ID}, false}}";
                    }
                    if (DictRef.ToggleOptions.TryGetValue(x, out OptionData.ToggleOption TO))
                    {
                        return $"setting{{{TO.ID}}}";
                    }
                    if (x == "RedKey")
                    {
                        return "Red_Key";
                    }
                    if (x == "VolcanoVase")
                    {
                        return "Volcano_Vase";
                    }
                    if (x == "can_reenter_pipes")
                    {
                        return "can_reenter_vertical_pipes";
                    }
                    return x;
                });
                LogicUtilities.RemoveRedundantConditionals(l);
                LogicUtilities.MakeCommonConditionalsRequirements(l);
                LogicUtilities.SortConditionals(l);
            }

            void AddLocationLogicEntry(MMRData.LogicFile logic, PMRRegion Region, string ID, string Logic)
            {
                var LogicEntry = logic.Logic.FirstOrDefault(x => x.Id == ID);
                string LogicString = $"(({Logic}) and (|{Region.region_name}|))";
                if (LogicEntry != null)
                {
                    LogicString += $" or ({LogicStringConverter.ConvertConditionalToLogicString(logicStringParser, LogicEntry.ConditionalItems)})";
                    LogicEntry.ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(logicStringParser, LogicString, ID);
                }
                else
                {
                    logic.Logic.Add(new MMRData.JsonFormatLogicItem
                    {
                        Id = ID,
                        ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(logicStringParser, LogicString, ID)
                    });
                }
            }

            foreach(var i in dictionary.LocationList)
            {
                i.Value.Area = RegionsDict[i.Value.Area];
            }

            string FinalDictFile = Path.Combine(TestingReferences.GetTestingDictionaryPath(), @"PMR V1.json");
            string FinalLogicFile = Path.Combine(TestingReferences.GetTestingLogicPresetsPath(), @"DEV-PMR Casual.json");
            File.WriteAllText(FinalLogicFile, JsonConvert.SerializeObject(Logic, Utility.DefaultSerializerSettings));
            File.WriteAllText(FinalDictFile, JsonConvert.SerializeObject(dictionary, Utility.DefaultSerializerSettings));
        }
        public class PMRRegion
        {
            public string region_name { get; set; }
            public string area_id { get; set; }
            public string map_id { get; set; }
            public string map_name { get; set; }
            public Dictionary<string, string> locations { get; set; } = [];
            public Dictionary<string, string> events { get; set; } = [];
            public Dictionary<string, string> exits { get; set; } = [];
        }
    }
}
