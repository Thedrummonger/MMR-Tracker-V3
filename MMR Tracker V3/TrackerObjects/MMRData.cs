using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class MMRData
    {

        [Serializable]
        public class JsonFormatLogicItem
        {
            public string Id { get; set; }
            public List<string> RequiredItems { get; set; } = new List<string>();
            public List<List<string>> ConditionalItems { get; set; } = new List<List<string>>();
            public TimeOfDay TimeNeeded { get; set; }
            public TimeOfDay TimeAvailable { get; set; }
            public TimeOfDay TimeSetup { get; set; }
            public bool IsTrick { get; set; }

            private string _trickTooltip;
            private string _trickCategory;
            public string TrickTooltip
            {
                get { return IsTrick ? _trickTooltip : null; }
                set { _trickTooltip = value; }
            }
            public string TrickCategory
            {
                get { return IsTrick ? _trickCategory : null; }
                set { _trickCategory = value; }
            }
        }

        [Serializable]
        public class SpoilerLogData
        {
            public GameplaySettings GameplaySettings { get; set; }
            public Dictionary<string, string> LocationLog { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> GossipLog { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, int> PriceLog { get; set; } = new Dictionary<string, int>();
        }

        [Serializable]
        public class GameplaySettings
        {
            public bool RandomizeDungeonEntrances { get; set; } = false;
            public string StartingItemMode { get; set; } = "";
            public string Logic { get; set; } = "";
            public bool AddSongs { get; set; } = false;
            public bool ProgressiveUpgrades { get; set; } = false;
            public bool ByoAmmo { get; set; } = false;
            public string SmallKeyMode { get; set; } = "Default";
            public string BossKeyMode { get; set; } = "Default";
            public string StrayFairyMode { get; set; } = "Default";
            public string LogicMode { get; set; } = "Casual";
            public string UserLogicFileName { get; set; } = "";
            public string CustomItemListString { get; set; } = "";
            public string CustomJunkLocationsString { get; set; } = "";
            public string CustomStartingItemListString { get; set; } = "";
            public string GossipHintStyle { get; set; } = "Default";
            public string GaroHintStyle { get; set; } = "Default";
            public bool FreeHints { get; set; } = false;
            public bool FreeGaroHints { get; set; } = false;
            public List<string> EnabledTricks { get; set; } = new List<string>();
        }

        [Serializable]
        public class LogicFile
        {
            public int Version { get; set; } = -1;

            public string GameCode 
            { 
                get { return _GameCode ?? "MMR"; } 
                set { _GameCode = value == "MMR" ? null : value; }
            }
            private string _GameCode = null;

            public List<JsonFormatLogicItem> Logic { get; set; }

            public override string ToString()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this, _NewtonsoftJsonSerializerOptions);
                //return JsonSerializer.Serialize(this, _jsonSerializerOptions);
            }

            public static LogicFile FromJson(string json)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<LogicFile>(json, _NewtonsoftJsonSerializerOptions);
                //return JsonSerializer.Deserialize<LogicFile>(json, _jsonSerializerOptions);
            }

            private readonly static Newtonsoft.Json.JsonSerializerSettings _NewtonsoftJsonSerializerOptions = new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };

            public IEnumerable<string> GetAllItemsUsedInLogic()
            {
                var AllReq = Logic.Select(x => x.RequiredItems).SelectMany(x => x).Distinct();
                var allCond = Logic.Select(x => x.ConditionalItems.SelectMany(x => x).Distinct()).SelectMany(x => x).Distinct();
                return AllReq.Concat(allCond).Distinct();
            }
        }

        public class SpoilerlogReference
        {
            public string[] SpoilerLogNames { get; set; } = Array.Empty<string>();
            public string[] GossipHintNames { get; set; } = Array.Empty<string>();
            public string[] PriceDataNames { get; set; } = Array.Empty<string>();
        }
    }
}
