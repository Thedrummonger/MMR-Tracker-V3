using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using static MMR_Tracker_V3.TrackerObjects.HintData;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.LogicDictionaryData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.MMRData;
using static MMR_Tracker_V3.TrackerObjects.OptionData;

namespace MMR_Tracker_V3
{
    public class LogicObjects
    {
        [Serializable]
        public class TrackerInstance
        {
            public Dictionary<string, LocationObject> LocationPool { get; set; } = new Dictionary<string, LocationObject>();
            public Dictionary<string, HintObject> HintPool { get; set; } = new Dictionary<string, HintObject>();
            public Dictionary<string, MacroObject> MacroPool { get; set; } = new Dictionary<string, MacroObject>();
            public Dictionary<string, ItemObject> ItemPool { get; set; } = new Dictionary<string, ItemObject>();
            public Dictionary<string, TrackerOption> UserOptions { get; set; } = new Dictionary<string, TrackerOption>();
            public EntranceData.EntrancePool EntrancePool { get; set; } = new EntranceData.EntrancePool();
            public LogicDictionary LogicDictionary { get; set; } = new LogicDictionary();
            public LogicFile LogicFile { get; set; } = new MMRData.LogicFile();
            public MMRData.SpoilerLogData SpoilerLog { get; set; } = null;
            public Dictionary<string, JsonFormatLogicItem> LogicOverride { get; set; } = new Dictionary<string, JsonFormatLogicItem>();
            public Options StaticOptions { get; set; } = new Options();
            public PriceData PriceData { get; set; } = new PriceData();
            public InstanceReference InstanceReference { get; set; } = new InstanceReference();
            public static TrackerInstance FromJson(string json)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<TrackerInstance>(json, _NewtonsoftJsonSerializerOptions);
            }
            public override string ToString()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this, _NewtonsoftJsonSerializerOptions);
            }
            private readonly static Newtonsoft.Json.JsonSerializerSettings _NewtonsoftJsonSerializerOptions = new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };
        }

        [Serializable]
        public class InstanceReference
        {
            //Dict References
            public Dictionary<string, int> LocationDictionaryMapping { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, int> ItemDictionaryMapping { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, int> MacroDictionaryMapping { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, int> HintDictionaryMapping { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, EntranceData.EntranceAreaPair> EntranceLogicNameToEntryData { get; set; } = new Dictionary<string, EntranceData.EntranceAreaPair>();

            //Logic File References
            public Dictionary<string, int> LogicFileMapping { get; set; } = new Dictionary<string, int>();
        }

        [Serializable]
        public class Options
        {
            public string ShowOptionsInListBox { get; set; } = OptionData.DisplayListBoxes[0];
            public bool DecoupleEntrances { get; set; } = false;
            public bool AutoCheckCoupleEntrances { get { return !DecoupleEntrances && _AutoCheckCoupleEntrances; } set { _AutoCheckCoupleEntrances = value; } }
            private bool _AutoCheckCoupleEntrances = true;
            public bool EntranceRandoFeatures { get; set; } = true;
            public OptionFile OptionFile { get; set; } = new OptionFile();
        }

        [Serializable]
        public class OptionFile
        {
            public bool CheckForUpdate { get; set; } = true;
            public bool MoveMarkedToBottom { get; set; } = false;
            public WinformData WinformData { get; set; } = new WinformData();
        }

        [Serializable]
        public class WinformData
        {
            public string FormFont { get; set; } = string.Empty;
            public bool HorizontalLayout { get; set; } = false;
            public bool ShowEntryNameTooltip { get; set; } = true;
        }

        [Serializable]
        public class PriceData
        {
            public bool Initialized { get; set; } = false;
            public List<string> WalletEntries { get; set; } = new List<string>();
            public Dictionary<string, int> Wallets { get; set; } = new Dictionary<string, int>();
            public Dictionary<int,string> CapacityMap { get; set; } = new Dictionary<int, string>();
        }

    }
}
