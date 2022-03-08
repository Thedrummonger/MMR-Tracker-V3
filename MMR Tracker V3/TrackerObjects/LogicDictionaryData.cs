using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.OptionData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class LogicDictionaryData
    {
        [Serializable]
        public class LogicDictionary
        {
            public int LogicVersion { get; set; }
            public string LogicFormat { get; set; }
            public string GameCode { get; set; }
            public int DefaultWalletCapacity { get; set; } = 99;
            public List<DictionaryLocationEntries> LocationList { get; set; } = new List<DictionaryLocationEntries>();
            public List<DictionaryItemEntries> ItemList { get; set; } = new List<DictionaryItemEntries>();
            public List<DictionaryMacroEntry> MacroList { get; set; } = new List<DictionaryMacroEntry>(); 
            public List<TrackerOption> Options { get; set; } = new List<TrackerOption>();
            public List<DictionaryHintEntries> HintSpots { get; set; } = new List<DictionaryHintEntries>();

            public static LogicDictionary FromJson(string json)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<LogicDictionary>(json, _NewtonsoftJsonSerializerOptions);
                //return JsonSerializer.Deserialize<LogicFile>(json, _jsonSerializerOptions);
            }
            public override string ToString()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this, _NewtonsoftJsonSerializerOptions);
                //return JsonSerializer.Serialize(this, _jsonSerializerOptions);
            }
            private readonly static Newtonsoft.Json.JsonSerializerSettings _NewtonsoftJsonSerializerOptions = new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };

        }

        [Serializable]
        public class DictionaryLocationEntries
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string OriginalItem { get; set; }
            public string[] AltNames { get; set; } = Array.Empty<string>();
            public string Area { get; set; }
            public string[] ValidItemTypes { get; set; } = Array.Empty<string>();
            public bool? IgnoreForSettingString { get; set; } = null;
        }

        [Serializable]
        public class DictionaryItemEntries
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public int? WalletCapacity { get; set; } = null;
            public int? MaxAmountInWorld { get; set; } = null;
            public bool? ValidStartingItem { get; set; } = null;
            public string[] ItemTypes { get; set; } = Array.Empty<string>();

        }

        [Serializable]
        public class DictionaryMacroEntry
        {
            public string ID { get; set; }
            public bool Static { get; set; } = false;
            public int? WalletCapacity { get; set; } = null;
            public List<string> RequiredItemsOverride { get; set; } = null;
            public List<List<string>> ConditionalItemsOverride { get; set; } = null;
            public dynamicLogicData DynamicLogicData { get; set; } = null;
        }

        [Serializable]
        public class DictionaryHintEntries
        {
            public string ID { get; set; }
            public string Name { get; set; }

        }
    }
}
