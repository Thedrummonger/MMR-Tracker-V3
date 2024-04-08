using MMR_Tracker_V3;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.FSharp.Core.ByRefKinds;

namespace TestingForm.GameDataCreation.WindWakerRando
{
    internal class Generator
    {
        public class WWRLocation
        {
            public string Need;
            [JsonProperty("Original item")]
            public string OriginalItem;
            public string Types;
        }
        public static void GenData(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary)
        {
            LogicStringParser WWRParser = new(operatorType: LogicStringParser.OperatorType.CStlyeSingle);

            var WWRCodeFolder = Path.Combine(TestingReferences.GetOtherGameDataPath("WindWakerRando"));
            var EntranceListFile = Path.Combine(WWRCodeFolder, "EntranceList.json");
            var ItemListFile = Path.Combine(WWRCodeFolder, "ItemList.json");
            var Entrances = TestingUtility.DeserializeJsonFile<string[]>(EntranceListFile);
            var Items = TestingUtility.DeserializeJsonFile<string[]>(ItemListFile);

            var WWRtestingFolder = Path.Combine(TestingReferences.GetDevTestingPath(), "WW");
            var LogicFolderPath = Path.Combine(WWRtestingFolder, "wwrando-master", "logic");
            var ItemLocationsFile = Path.Combine(LogicFolderPath, "item_locations.txt");
            var MacroFile = Path.Combine(LogicFolderPath, "macros.txt");
            var Locations = TestingUtility.DeserializeYAMLFile<Dictionary<string, WWRLocation>>(ItemLocationsFile);
            var Macros = TestingUtility.DeserializeYAMLFile<Dictionary<string, string>>(MacroFile);
            Debug.WriteLine(Locations.ToFormattedJson());
            Debug.WriteLine(Macros.ToFormattedJson());

            MMRData.LogicFile logicFile = new() { Logic = [], GameCode = "WWR", Version = 2 };
            LogicDictionaryData.LogicDictionary dictFile = new() { 
                GameCode = "WWR",
                GameName = "The Wind Waker",
                LogicVersion = 2,
                NetPlaySupported = true,
                WinCondition = "Can Reach and Defeat Ganondorf"
            };
            foreach (var Location in Locations) { AddLogicEntry(Location.Key, Location.Value.Need); }
            foreach (var Location in Macros) { AddLogicEntry(Location.Key, Location.Value); }
            void AddLogicEntry(string ID, string Logic)
            {
                MMRData.JsonFormatLogicItem LogicItem = new()
                {
                    Id = ID,
                    ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(WWRParser, Logic, ID)
                };
                logicFile.Logic.Add(LogicItem);
            }
            CleanLogic(logicFile);
            TransformCounts(logicFile);

            foreach (var location in Locations) 
            {
                var Entry = new LogicDictionaryData.DictionaryLocationEntries()
                {
                    ID = location.Key,
                    Name = location.Key,
                    Area = location.Key.TrimSplit("-")[0],
                    OriginalItem = location.Value.OriginalItem,
                    ValidItemTypes = ["item"],
                    SpoilerData = new MMRData.SpoilerlogReference { NetID = location.Key, Tags = [location.Value.Types] },
                };
                dictFile.LocationList.Add(location.Key, Entry);
            }

            foreach(var entrance in Entrances)
            {
                var EntranceID = logicFile.Logic.First(x => x.Id == $"Can Access {entrance}");
                var LocationName = EntranceID.ConditionalItems[0][0]["Can Access ".Length..];
                var LocationEntry = new LogicDictionaryData.DictionaryLocationEntries()
                {
                    ID = EntranceID.Id,
                    Name = LocationName,
                    Area = GetEntArea(entrance),
                    OriginalItem = EntranceID.Id,
                    ValidItemTypes = ["Entrances"],
                };
                var ItemEntry = new LogicDictionaryData.DictionaryItemEntries()
                {
                    ID = EntranceID.Id,
                    Name = entrance,
                    ItemTypes = ["Entrances"],
                    MaxAmountInWorld = 1,
                    ValidStartingItem = false,
                };
                dictFile.LocationList.Add(EntranceID.Id, LocationEntry);
                dictFile.ItemList.Add(EntranceID.Id, ItemEntry);
            }
            foreach (var item in Items.Distinct())
            {
                var ItemEntry = new LogicDictionaryData.DictionaryItemEntries()
                {
                    ID = item,
                    Name = item,
                    ItemTypes = ["item"],
                    ValidStartingItem = true,
                    SpoilerData = new MMRData.SpoilerlogReference() { NetID = item }
                };
                dictFile.ItemList.Add(item, ItemEntry);
            }
            Logic = logicFile;
            dictionary = dictFile;
        }

        public static string GetEntArea(string ID)
        {
            if (ID.EndsWith("Miniboss Arena")) { return "MiniBoss Entrance"; }
            else if (ID.EndsWith("Boss Arena")) { return "Boss Entrance"; }
            else if (ID.EndsWith("Secret Cave")) { return "Secret Cave Entrance"; }
            else if (ID.EndsWith("Inner Cave")) { return "Inner Cave Entrance"; }
            else if (ID.EndsWith("Fairy Fountain")) { return "Fairy Fountain Entrance"; }
            return "Dungeon Entrance";
        }

        public static void CleanLogic(MMRData.LogicFile logicFile)
        {

            foreach (var Entry in logicFile.Logic)
            {
                LogicUtilities.TransformLogicItems(Entry, x =>
                {
                    if (x.StartsWith("Option \""))
                    {
                        var Data = x.TrimSplit("\"");
                        if (Data.Length == 3)
                        {
                            bool Enabled = Data[2] == "Enabled";
                            return $"setting{{{Data[1]}, {Enabled}}}";
                        }
                        else if (Data.Length > 3)
                        {
                            bool Enabled = Data[2] == "Is";
                            return $"setting{{{Data[1]}, {Data[3]}, {Enabled}}}";
                        }
                    }
                    else if (x.StartsWith("Can Access Item Location \""))
                    {
                        var Data = x.TrimSplit("\"");
                        return $"available{{{Data[1]}}}";
                    }
                    else if (x == "Impossible") { return false.ToString(); }
                    else if (x == "Nothing") { return true.ToString(); }
                    return x;
                });
                LogicUtilities.RemoveRedundantConditionals(Entry);
                LogicUtilities.SortConditionals(Entry);
            }
        }

        public static void TransformCounts(MMRData.LogicFile logicFile)
        {
            foreach (var Entry in logicFile.Logic)
            {
                LogicUtilities.TransformLogicItems(Entry, input =>
                {
                    string pattern = @" (x)(\d+)";
                    // Check if the pattern matches in the input string
                    if (Regex.IsMatch(input, pattern))
                    {
                        // Replace occurrences of the pattern with ",$2", where $2 is the matched number
                        string result = Regex.Replace(input, pattern, " ,$2");
                        return result;
                    }
                    return input;
                });
                LogicUtilities.RemoveRedundantConditionals(Entry);
                LogicUtilities.SortConditionals(Entry);
            }
        }
    }
}
