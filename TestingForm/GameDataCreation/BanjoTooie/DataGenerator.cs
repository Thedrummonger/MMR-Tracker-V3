using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.LogicDictionaryData;
using MMR_Tracker_V3;
using Microsoft.VisualBasic.Logging;
using MMR_Tracker_V3.Logic;

namespace TestingForm.GameDataCreation.BanjoTooie
{
    internal class DataGenerator
    {
        static LogicStringParser logicparser = new LogicStringParser();
        public class BTLogicArea
        {
            public Dictionary<string, BTLogicEntry> locations = [];
            public Dictionary<string, BTLogicEntry> exits = [];
            public Dictionary<string, BTLogicEntry> events = [];
        }
        public class BTLogicEntry
        {
            public string beginnerLogic = "";
            public string normalLogic = "";
            public string advancedLogic = "";
        }
        public static void GenData(out MMRData.LogicFile FinalLogic, out LogicDictionaryData.LogicDictionary Finaldictionary)
        {
            var Locations = TestingUtility.DeserializeJsonFile<Dictionary<string, string>>(
               Path.Join(TestingReferences.GetOtherGameDataPath("BanjoTooie"), "Locations.json"));
            var Items = TestingUtility.DeserializeJsonFile<Dictionary<string, string>>(
                Path.Join(TestingReferences.GetOtherGameDataPath("BanjoTooie"), "Items.json"));
            var Regions = TestingUtility.DeserializeJsonFile<Dictionary<string, string>>(
               Path.Join(TestingReferences.GetOtherGameDataPath("BanjoTooie"), "Regions.json"));
            var AreaMap = TestingUtility.DeserializeJsonFile<Dictionary<string, string[]>>(
                Path.Join(TestingReferences.GetOtherGameDataPath("BanjoTooie"), "LocationAreaMap.json"));
            var OriginalItems = TestingUtility.DeserializeJsonFile<Dictionary<string, string>>(
                Path.Join(TestingReferences.GetOtherGameDataPath("BanjoTooie"), "OriginalItem.json"));

            var WorldGraphFolder = Path.Join(TestingReferences.GetOtherGameDataPath("BanjoTooie"), "World");

            LogicDictionary dictionary = new()
            {
                GameCode = "BT",
                GameName = "Banjo Tooie",
                LogicVersion = 1,
                NetPlaySupported = true,
                WinCondition = "VICTORY",
                AreaOrder = AreaOrder(),
                RootAreas = ["Menu"]
            };
            MMRData.LogicFile Logic = new()
            {
                Logic = [],
                GameCode = "BT",
                Version = 1,
            };
            foreach(var location in Locations) 
            {
                var LocationData = new LogicDictionaryData.DictionaryLocationEntries()
                {
                    ID = location.Key,
                    Name = location.Value,
                    OriginalItem = GetVanillaItems(OriginalItems, location.Key),
                    Area = GetArea(Regions, AreaMap, location.Key),
                    ValidItemTypes = GetLocationTypes(location.Key),
                    SpoilerData = new MMRData.SpoilerlogReference { NetIDs = [location.Value] }
                };
                dictionary.LocationList.Add(location.Key, LocationData);
            }
            foreach(var item in Items)
            {
                var itemData = new LogicDictionaryData.DictionaryItemEntries()
                {
                    ID = item.Key,
                    Name = item.Value,
                    ItemTypes = GetItemTypes(item.Key),
                    SpoilerData = new MMRData.SpoilerlogReference { NetIDs = [item.Value] }
                };
                dictionary.ItemList.Add(item.Key, itemData);
            }

            var JinjoCounts = UsableJinjos(Items);
            foreach (var item in dictionary.LocationList.Values.Where(x => x.OriginalItem == "JINJO"))
            {
                string Jinjo = JinjoCounts.First(x => x.Value > 0).Key;
                JinjoCounts[Jinjo]--;
                item.OriginalItem = Jinjo;
            }

            foreach(var i in Directory.GetFiles(WorldGraphFolder))
            {
                var WorldFile = TestingUtility.DeserializeYAMLFile<Dictionary<string, BTLogicArea>>(i);
                foreach(var area in WorldFile)
                {
                    foreach(var loc in area.Value.locations)
                    {
                        AddLogicEntry(loc.Key, loc.Value.beginnerLogic, area.Key);
                    }
                    foreach (var loc in area.Value.events)
                    {
                        AddLogicEntry(loc.Key, loc.Value.beginnerLogic, area.Key);
                    }
                    foreach (var loc in area.Value.exits)
                    {
                        string ExitID = $"{area.Key} => {loc.Key}";
                        AddLogicEntry(ExitID, loc.Value.beginnerLogic, area.Key);
                        AddEntrance(ExitID, area.Key, loc.Key);
                    }
                }
            }

            dictionary.LogicEntryCollections.Add("check_notes", new OptionData.LogicEntryCollection
            {
                ID = "check_notes",
                Entries = [.. Enumerable.Repeat("NOTE", 5), .. Enumerable.Repeat("TREBLE", 20)]
            });

            dictionary.ToggleOptions.Add("open_hag1", new OptionData.ToggleOption(null)
            {
                ID = "open_hag1",
                Name = "HAG 1 Open",
                Description = "HAG 1 boss fight is opened when Cauldron Keep is. Only 55 jiggies are needed to win.",
                Value = true
            }.CreateSimpleValues());

            dictionary.ToggleOptions.Add("randomize_chuffy", new OptionData.ToggleOption(null)
            {
                ID = "randomize_chuffy",
                Name = "Chuffy as a randomized AP Item.",
                Description = "Chuffy is lost across the MultiWorld.",
                Value = true
            }.CreateSimpleValues());

            foreach(var l in Logic.Logic)
            {
                LogicUtilities.RemoveRedundantConditionals(l);
                LogicUtilities.MakeCommonConditionalsRequirements(l);
                LogicUtilities.SortConditionals(l);
            }

            void AddEntrance(string ID, string Area, string Exit)
            {
                var itemData = new LogicDictionaryData.DictionaryEntranceEntries()
                {
                    ID = ID,
                    Area = Area,
                    Exit = Exit,
                    RandomizableEntrance = false
                };
                dictionary.EntranceList.Add(ID, itemData);
            }

            void AddLogicEntry(string ID, string LogicString, string ParentArea)
            {
                MMRData.JsonFormatLogicItem logicItem = new()
                {
                    Id = ID,
                    RequiredItems = [ParentArea],
                    ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(logicparser, LogicString, ID)
                };
                Logic.Logic.Add(logicItem);
            }

            File.WriteAllText(Path.Combine(TestingReferences.GetDevTestingPath(), "Banjo", "BTLogic.json"), Logic.ToFormattedJson());
            File.WriteAllText(Path.Combine(TestingReferences.GetDevTestingPath(), "Banjo", "BTDict.json"), dictionary.ToFormattedJson());
            FinalLogic = Logic;
            Finaldictionary = dictionary;
        }

        public static string[] AreaOrder()
        {
            return [
                "Spiral Mountain",
                "Isle O Hags - Jinjo Village",
                "Isle O Hags - Wooded Hollow",
                "Mayahem Temple",
                "Isle O Hags - Plateau",
                "Glitter Gulch Mine",
                "Glitter Gulch Mine Station",
                "Chuffy's Cabin",
                "Isle O Hags - Pine Grove",
                "Witchyworld",
                "Isle O Hags - Cliff Top",
                "Jolly Roger's Lagoon",
                "Isle O Hags - Wasteland",
                "Terrydactyland",
                "Terrydactyland - Hatch Cave",
                "Outside Grunty's Industries",
                "Grunty's Industries Station",
                "Grunty's Industries 1st Floor",
                "Grunty's Industries 2nd Floor",
                "Grunty's Industries 3rd Floor Onwords",
                "Isle O Hags - Cliff Top (Across Bridge)",
                "Hailfire Peaks",
                "Hailfire Peaks Lava Station",
                "Hailfire Peaks Icy Station",
                "Cloud Cuckooland",
                "HAG 1",
                "Misc",
                "Multiworld",
                "Starting Items"
            ];
        }

        public static Dictionary<string, int> UsableJinjos(Dictionary<string, string> ItemList)
        {
            Dictionary<string, int> UsableJinjos = [];
            foreach(var item in ItemList.Keys.Where(x => x.EndsWith("JINJO")))
            {
                UsableJinjos[item] = UsableJinjos.Keys.Count + 1;
            }
            return UsableJinjos;
        }

        public static string[] GetLocationTypes(string Key)
        {
            if (Key == "HAG1") { return ["VICTORY"]; }
            return ["item"];
        }
        public static string[] GetItemTypes(string Key)
        {
            if (Key == "VICTORY") { return ["VICTORY"]; }
            return ["item"];
        }
        public static string GetVanillaItems(Dictionary<string, string> OriginalItemDict, string Key)
        {
            if (OriginalItemDict.TryGetValue(Key, out string item)) { return item == "*" ? Key : item; }
            return "Unknown"; //TODO Figure this out
        }

        public static string GetArea(Dictionary<string, string> Regions, Dictionary<string, string[]> AreaMap, string Key)
        {
            foreach (var area in AreaMap)
            {
                if (area.Value.Contains(Key) && Regions.TryGetValue(area.Key, out string AreaName)) { return AreaName; }
            }
            Debug.WriteLine($"Location {Key} could not be assigned an area");
            return ("Misc");
        }
    }
}
