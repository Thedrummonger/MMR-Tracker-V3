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

        public class Configuration
        {
            public GameplaySettings GameplaySettings { get; set; }
        }

        public class GameplaySettings
        {
            public bool UseCustomItemList { get; set; } = true;
            public string[] CategoriesRandomized { get; set; } = null;
            public bool AddDungeonItems { get; set; } = false;
            public bool AddShopItems { get; set; } = false;
            public bool AddMoonItems { get; set; } = false;
            public bool AddFairyRewards { get; set; } = false;
            public bool AddOther { get; set; } = false;
            public bool AddNutChest { get; set; } = false;
            public bool CrazyStartingItems { get; set; } = false;
            public bool AddCowMilk { get; set; } = false;
            public bool AddSkulltulaTokens { get; set; } = false;
            public bool AddStrayFairies { get; set; } = false;
            public bool AddMundaneRewards { get; set; } = false;
            public bool RandomizeBottleCatchContents { get; set; } = false;
            public bool ExcludeSongOfSoaring { get; set; } = false;
            public bool RandomizeDungeonEntrances { get; set; } = false;
            public bool NoStartingItems { get; set; } = false;
            public string StartingItemMode { get; set; } = "";
            public string Logic { get; set; } = "";
            public bool AddSongs { get; set; } = false;
            public bool ProgressiveUpgrades { get; set; } = false;
            public bool ByoAmmo { get; set; } = false;
            public bool DecoupleEntrances { get; set; } = false;
            public string SmallKeyMode { get; set; } = "Default";
            public string BossKeyMode { get; set; } = "Default";
            public string LogicMode { get; set; } = "Casual";
            public string UserLogicFileName { get; set; } = "";
            public string CustomItemListString { get; set; } = "";
            public string RandomizedEntrancesString { get; set; } = "";
            public string CustomJunkLocationsString { get; set; } = "";
            public string CustomStartingItemListString { get; set; } = "";
            public string GossipHintStyle { get; set; } = "Default";
            public List<string> EnabledTricks { get; set; } = new List<string>();
        }

        public class LogicFile
        {
            public int Version { get; set; } = -1;

            private string _GameCode = null;

            public string GameCode 
            { 
                get { return _GameCode == null ? "MMR" : _GameCode; } 
                set { _GameCode = value == "MMR" ? null : value; } 
            }

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
        }
    }
}
