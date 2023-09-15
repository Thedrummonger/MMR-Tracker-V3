using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MMR_Tracker_V3.OtherGames.WindWakerRando
{
    public class ReadAndParseData
    {
        public class WWRPool
        {
            public string ID { get; set; }
            public string Need { get; set; }
            [JsonProperty("Original item")]
            public string OriginalItem { get; set; }
            public string Types { get; set; }
        }
        public class CodeFileReader<T>
        {
            public T ReadCodeFile(string FileName)
            {
                string Name;
                string Extention = ".json";
                if (Path.HasExtension(FileName))
                {
                    Name = Path.GetFileNameWithoutExtension(FileName);
                    Extention = Path.GetExtension(FileName);
                }
                else
                {
                    Name = FileName;
                }
                string Target = Path.Combine(References.TestingPaths.GetDevCodePath(), @"MMR Tracker V3", "OtherGames", "WindWakerRando", $"{Name}{Extention}");
                string Content = File.ReadAllText(Target);
                return JsonConvert.DeserializeObject<T>(Content);
            }
        }

        public static LogicStringParser WWRLogicConverter = new LogicStringParser("&", "|", LogicStringParser.ContainerType.parentheses);

        public static void CreateFiles(out TrackerObjects.MMRData.LogicFile Logic, out TrackerObjects.LogicDictionaryData.LogicDictionary dictionary)
        {
            dictionary = new TrackerObjects.LogicDictionaryData.LogicDictionary
            {
                GameCode = "WWR",
                LogicVersion = 1,
            };
            Logic = new TrackerObjects.MMRData.LogicFile
            {
                GameCode = "WWR",
                Logic = new List<TrackerObjects.MMRData.JsonFormatLogicItem>(),
                Version = 1,
            };

            var OGItemMapData = new CodeFileReader<Dictionary<string, string>>().ReadCodeFile("OriginalItemMap");
            var ItemPoolData = new CodeFileReader<List<string>>().ReadCodeFile("ItemPool");
            var TreasureChartsData = new CodeFileReader<Dictionary<string, int>>().ReadCodeFile("TreasureCharts");
            var DeungeonCodeData = new CodeFileReader<Dictionary<string, string>>().ReadCodeFile("DungeonCodes");
            var EntranceData = new CodeFileReader<Dictionary<string, string>>().ReadCodeFile("Entrances");

            string LocationPoolWebData;
            string MacroWebData;
            using (var wc = new System.Net.WebClient())
            {
                LocationPoolWebData = wc.DownloadString("https://raw.githubusercontent.com/LagoLunatic/wwrando/master/logic/item_locations.txt");
                MacroWebData = wc.DownloadString("https://raw.githubusercontent.com/LagoLunatic/wwrando/master/logic/macros.txt");
            }
            string Json = Utility.ConvertYamlStringToJsonString(LocationPoolWebData, true);
            Dictionary<string, WWRPool> Pool = JsonConvert.DeserializeObject<Dictionary<string, WWRPool>>(Json);
            foreach(var i in Pool) { i.Value.ID = i.Key; }
            string MacroJson = Utility.ConvertYamlStringToJsonString(MacroWebData, true);
            Dictionary<string, string> MacroPool = JsonConvert.DeserializeObject<Dictionary<string, string>>(MacroJson);

            foreach (var i in Pool)
            {
                dictionary.LocationList.Add(i.Key, new TrackerObjects.LogicDictionaryData.DictionaryLocationEntries
                {
                    ID = i.Key,
                    Area = i.Key.StringSplit(" - ")[0].Trim(),
                    Name = $"{string.Join(" - ", i.Key.StringSplit(" - ")[1..])} ({i.Key.StringSplit(" - ")[0].Trim()})",
                    OriginalItem = ParseOriginalItem(i.Value.OriginalItem, i.Key.StringSplit(" - ")[0].Trim()),
                    ValidItemTypes = i.Value.Types.Split(',').Select(x => x.Trim()).Concat(new string[] { "Item" }).ToArray(),
                    SpoilerData = new TrackerObjects.MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { i.Key } }
                });
                Logic.Logic.Add(new TrackerObjects.MMRData.JsonFormatLogicItem
                {
                    Id = i.Key,
                    ConditionalItems = i.Value.Need == "Nothing" ? new List<List<string>>() : LogicStringConverter.ConvertLogicStringToConditional(WWRLogicConverter, i.Value.Need, i.Key)
                });
            }

            foreach(var macro in MacroPool)
            {
                if (EntranceData.ContainsKey(macro.Key)) { continue; }
                Logic.Logic.Add(new TrackerObjects.MMRData.JsonFormatLogicItem
                {
                    Id = macro.Key,
                    ConditionalItems = macro.Value == "Nothing" ? new List<List<string>>() : LogicStringConverter.ConvertLogicStringToConditional(WWRLogicConverter, macro.Value, macro.Key)
                });
            }

            foreach (var Entrance in EntranceData)
            {
                dictionary.LocationList.Add(Entrance.Value, new LogicDictionaryData.DictionaryLocationEntries
                {
                    ID = Entrance.Value,
                    Area = Entrance.Value.Contains("Dungeon Entrance") ? "Dugneon Entrances" : "Cave Entrances",
                    Name = Entrance.Value.Replace("Can Access ", ""),
                    OriginalItem = Entrance.Key,
                    ValidItemTypes = new string[] { "Entrances" }
                });
                dictionary.ItemList.Add(Entrance.Key, new LogicDictionaryData.DictionaryItemEntries
                {
                    ID = Entrance.Key,
                    Name = Entrance.Key.Replace("Can Access ", ""),
                    ValidStartingItem = false,
                    MaxAmountInWorld = 1,
                    ItemTypes = new string[] { "Entrances" }
                });
            }

            foreach(var LogicEntry in Logic.Logic)
            {
                List<List<string>> NewConditional = new List<List<string>>();
                foreach(var Set in LogicEntry.ConditionalItems)
                {
                    List<string> NewSet = new List<string>();
                    foreach(var condtional in Set)
                    {
                        NewSet.Add(CleanConditional(condtional));
                    }
                    NewConditional.Add(NewSet);
                }
                LogicEntry.ConditionalItems = NewConditional;
            }

            string ParseOriginalItem(string originalItem, string Area)
            {
                if (OGItemMapData.ContainsKey(originalItem)) { return OGItemMapData[originalItem]; }
                else if (originalItem == "Big Key" || originalItem == "Small Key" || originalItem == "Dungeon Map" || originalItem == "Compass")
                {
                    return $"{DeungeonCodeData[Area]} {originalItem}";
                }
                return originalItem;
            }


            foreach (var i in ItemPoolData.Concat(TreasureChartsData.Keys))
            {
                int Max = -1;
                var Data = i.StringSplit("x ");
                string ItemName = i;
                if(Data.Length > 1)
                {
                    Max = int.Parse(Data[0]);
                    ItemName = Data[1];
                }
                dictionary.ItemList.Add(ItemName, new TrackerObjects.LogicDictionaryData.DictionaryItemEntries
                {
                    ID = ItemName,
                    Name = ItemName,
                    MaxAmountInWorld = Max,
                    ItemTypes = new string[] {"Item"},
                    ValidStartingItem = true,
                    SpoilerData = new TrackerObjects.MMRData.SpoilerlogReference { SpoilerLogNames = new string[] {ItemName} }
                });
            }

            OptionData.TrackerOption skip_rematch_bosses = new OptionData.TrackerOption();
            skip_rematch_bosses.ID = "skip_rematch_bosses";
            skip_rematch_bosses.DisplayName = "Skip Rematch Bosses";
            skip_rematch_bosses.CurrentValue = "Enabled";
            skip_rematch_bosses.CreateSimpleValues(new string[] { "Enabled", "Disabled" });
            dictionary.Options.Add(skip_rematch_bosses.ID, skip_rematch_bosses);

            OptionData.TrackerOption sword_mode = new OptionData.TrackerOption();
            sword_mode.ID = "sword_mode";
            sword_mode.DisplayName = "Sword Mode";
            sword_mode.CurrentValue = "Start with Hero's Sword";
            sword_mode.CreateSimpleValues(new string[] { "Swordless", "No Starting Sword", "Start with Hero's Sword" });
            dictionary.Options.Add(sword_mode.ID, sword_mode);

        }

        private static string CleanConditional(string condtional)
        {
            var MultiItemTest = condtional.StringSplit(" x");
            if(MultiItemTest.Length == 2 && int.TryParse(MultiItemTest[1], out _))
            {
                return $"{MultiItemTest[0]}, {MultiItemTest[1]}";
            }
            if (condtional == "Impossible") { return "false"; }
            if (condtional.Contains('"'))
            {
                var Segments = condtional.Split('"').Select(x => x.Trim()).ToArray();
                if (Segments[0] == "Option")
                {
                    string OptionName = Segments[1];
                    string OptionMod = Segments.Length > 3 ? Segments[2] : "Is";
                    string OptionValue = Segments.Length > 3 ? Segments[3] : Segments[2];
                    Debug.WriteLine(string.Join("|", Segments));
                    string OptionString = $"option{{{OptionName}, {OptionValue}" + (OptionMod.ToLower() == "is not" ? ", false" : "") + "}";
                    return OptionString;
                }
                else if (Segments[0] == "Can Access Other Location")
                {
                    return $"available{{{Segments[1]}}}";
                }
            }
            return condtional;
        }
    }
}
