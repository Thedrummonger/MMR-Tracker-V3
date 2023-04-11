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


            string OGItemMapPath = Path.Combine(References.TestingPaths.GetDevCodePath(), @"MMR Tracker V3", "OtherGames", "WindWakerRando", $"OriginalItemMap.json");
            string OGItemMapContent = File.ReadAllText(OGItemMapPath);
            var OGItemMapData = JsonConvert.DeserializeObject<Dictionary<string, string>>(OGItemMapContent);

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

            foreach(var i in Pool)
            {
                dictionary.LocationList.Add(i.Key, new TrackerObjects.LogicDictionaryData.DictionaryLocationEntries
                {
                    ID = i.Key,
                    Area = i.Key.StringSplit(" - ")[0].Trim(),
                    Name= i.Key,
                    OriginalItem = OGItemMapData.ContainsKey(i.Value.OriginalItem) ? OGItemMapData[i.Value.OriginalItem] : i.Value.OriginalItem,
                    ValidItemTypes = i.Value.Types.Split(',').Select(x => x.Trim()).Concat(new string[] { "Item" }).ToArray(),
                    SpoilerData = new TrackerObjects.MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { i.Key } }
                });
                Logic.Logic.Add(new TrackerObjects.MMRData.JsonFormatLogicItem
                {
                    Id = i.Key,
                });
            }

            string ItemPoolPath = Path.Combine(References.TestingPaths.GetDevCodePath(), @"MMR Tracker V3", "OtherGames", "WindWakerRando", $"ItemPool.json");
            string ItemPoolContent = File.ReadAllText(ItemPoolPath);
            var ItemPoolData = JsonConvert.DeserializeObject<List<string>>(ItemPoolContent);

            string TreasureChartsPath = Path.Combine(References.TestingPaths.GetDevCodePath(), @"MMR Tracker V3", "OtherGames", "WindWakerRando", $"TreasureCharts.json");
            string TreasureChartsContent = File.ReadAllText(TreasureChartsPath);
            var TreasureChartsData = JsonConvert.DeserializeObject<Dictionary<string, int>>(TreasureChartsContent);

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
        }
    }
}
