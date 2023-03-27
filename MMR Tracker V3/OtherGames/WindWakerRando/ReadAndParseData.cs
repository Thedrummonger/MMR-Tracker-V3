using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static void CreateFiles()
        {
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
            Json = Utility.ConvertYamlStringToJsonString(MacroWebData, true);
            Debug.WriteLine(Json);
        }
    }
}
