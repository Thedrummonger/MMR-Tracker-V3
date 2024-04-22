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

namespace TestingForm.GameDataCreation.BanjoTooie
{
    internal class DataGenerator
    {
        public static void GenData(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary)
        {
            var Locations = TestingUtility.DeserializeJsonFile<Dictionary<string, string>>(
               Path.Join(TestingReferences.GetOtherGameDataPath("BanjoTooie"), "Locations.json"));
            var Items = TestingUtility.DeserializeJsonFile<Dictionary<string, string>>(
                Path.Join(TestingReferences.GetOtherGameDataPath("BanjoTooie"), "Items.json"));
            var Regions = TestingUtility.DeserializeJsonFile<Dictionary<string, string>>(
               Path.Join(TestingReferences.GetOtherGameDataPath("BanjoTooie"), "Regions.json"));
            var AreaMap = TestingUtility.DeserializeJsonFile<Dictionary<string, string[]>>(
                Path.Join(TestingReferences.GetOtherGameDataPath("BanjoTooie"), "LocationAreaMap.json"));

            dictionary = new()
            {
                GameCode = "BT",
                GameName = "Banjo Tooie",
                LogicVersion = 1,
                NetPlaySupported = true,
                WinCondition = "HAG1"
            };
            Logic = new()
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
                    OriginalItem = GetVanillaItems(location.Key),
                    Area = GetArea(Regions, AreaMap, location.Key),
                    ValidItemTypes = ["item"],
                    SpoilerData = new MMRData.SpoilerlogReference { NetIDs = [location.Value] }
                };
                dictionary.LocationList.Add(location.Key, LocationData);

                Logic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = location.Key });
            }
            foreach(var item in Items)
            {
                var itemData = new LogicDictionaryData.DictionaryItemEntries()
                {
                    ID = item.Key,
                    Name = item.Value,
                    ItemTypes = ["item"],
                    SpoilerData = new MMRData.SpoilerlogReference { NetIDs = [item.Value] }
                };
                dictionary.ItemList.Add(item.Key, itemData);
            }

            File.WriteAllText(Path.Combine(TestingReferences.GetDevTestingPath(), "Banjo", "BTLogic.json"), Logic.ToFormattedJson());
            File.WriteAllText(Path.Combine(TestingReferences.GetDevTestingPath(), "Banjo", "BTDict.json"), dictionary.ToFormattedJson());
        }

        public static string GetVanillaItems(string Key)
        {
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
