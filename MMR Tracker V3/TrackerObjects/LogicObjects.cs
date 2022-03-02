using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.LogicDictionaryData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public class LogicObjects
    {
        public class TrackerInstance
        {
            public LocationPool LocationPool { get; set; } = new LocationPool();
            public ItemPool ItemPool { get; set; } = new ItemPool();
            public MacroData Macros { get; set; } = new MacroData();
            public LogicDictionary LogicDictionary { get; set; } = new LogicDictionary();
            public MMRData.LogicFile LogicFile { get; set; } = new MMRData.LogicFile();
            public Options Options { get; set; } = new Options();
            public InstanceReference InstanceReference { get; set; } = new InstanceReference(); 
            public static TrackerInstance FromJson(string json)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<TrackerInstance>(json, _NewtonsoftJsonSerializerOptions);
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

        public class InstanceReference
        {
            public Dictionary<string, LogicMapping> LogicDataMappings { get; set; } = new Dictionary<string, LogicMapping>();
            public Keydata Keydata { get; set; } = new Keydata();
        }

        public class Keydata
        {
            public List<string> SmallKeys { get; set; } = new List<string>();
            public List<string> BossKeys { get; set; } = new List<string>();
            public List<string> LocationWithKeysInLogic { get; set; } = new List<string>();
        }

        public class LogicMapping
        {
            public LogicEntryType logicEntryType { get; set; }
            public int IndexInList { get; set; }

        }

        public class Options
        {
            public bool EntranceRadnoEnabled { get; set; } = false;
            public bool CoupleEntrances { get; set; } = true;
            public bool ShowAdditionalStats { get; set; } = false;
            public bool CheckForUpdate { get; set; } = true;
            public WinformData WinformData { get; set; } = new WinformData();
        }

        public class WinformData
        {
            public string FormFont { get; set; } = string.Empty;
            public bool HorizontalLayout { get; set; } = false;
            public bool MoveMarkedToBottom { get; set; } = false;
            public MiddleClickFunction MiddleClickFunction { get; set; } = MiddleClickFunction.set;
            public bool ShowEntryNameTooltip { get; set; } = true;
        }

    }
}
