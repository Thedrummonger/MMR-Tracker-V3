using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static MMR_Tracker_V3.TrackerObjects.HintData;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.LogicDictionaryData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.MMRData;
using static MMR_Tracker_V3.TrackerObjects.OptionData;

namespace MMR_Tracker_V3
{
    public class InstanceData
    {
        [Serializable]
        public class TrackerInstance
        {
            public Dictionary<string, LocationObject> LocationPool { get; set; } = new Dictionary<string, LocationObject>();
            public Dictionary<string, HintObject> HintPool { get; set; } = new Dictionary<string, HintObject>();
            public Dictionary<string, MacroObject> MacroPool { get; set; } = new Dictionary<string, MacroObject>();
            public Dictionary<string, ItemObject> ItemPool { get; set; } = new Dictionary<string, ItemObject>();
            public Dictionary<string, ChoiceOption> ChoiceOptions { get; set; } = new Dictionary<string, ChoiceOption>();
            public Dictionary<string, ToggleOption> ToggleOptions { get; set; } = new Dictionary<string, ToggleOption>();
            public Dictionary<string, IntOption> IntOptions { get; set; } = new Dictionary<string, IntOption>();
            public Dictionary<string, LogicEntryCollection> LogicEntryCollections { get; set; } = new Dictionary<string, LogicEntryCollection>();
            public EntranceData.EntrancePool EntrancePool { get; set; } = new EntranceData.EntrancePool();
            public LogicDictionary LogicDictionary { get; set; } = new LogicDictionary();
            public LogicFile LogicFile { get; set; } = new MMRData.LogicFile();
            public SpoilerLogFileData SpoilerLog { get; set; } = null;
            public Dictionary<string, JsonFormatLogicItem> RuntimeLogic { get; set; } = new Dictionary<string, JsonFormatLogicItem>();
            public LocationProxyData LocationProxyData { get; set; } = new LocationProxyData();
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
            public string ToJson(JSONType type)
            {
                return type switch
                {
                    JSONType.Newtonsoft => Newtonsoft.Json.JsonConvert.SerializeObject(this, _NewtonsoftJsonSerializerOptions),
                    JSONType.UTF8 => Utf8Json.JsonSerializer.ToJsonString(this),
                    JSONType.DotNet => System.Text.Json.JsonSerializer.Serialize(this),
                    _ => throw new NotImplementedException(),
                };
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
            //A table mapping Logic names to an entrance area pair
            public Dictionary<string, EntranceData.EntranceAreaPair> EntranceLogicNameToEntryData { get; set; } = new Dictionary<string, EntranceData.EntranceAreaPair>();
            //A table Mapping an Exit to its logic name
            public Dictionary<string, string> ExitLogicMap { get; set; } = new Dictionary<string, string>();
        }

        [Serializable]
        public class Options
        {
            public string ShowOptionsInListBox { get; set; } = OptionData.DisplayListBoxes[0];

            public Dictionary<string, bool> MinimizedHeader { get; set; } = new Dictionary<string, bool>();
            public OptionFile OptionFile { get; set; } = new OptionFile();
        }

        [Serializable]
        public class OptionFile
        {
            public bool CheckForUpdate { get; set; } = true;
            public bool CheckHintMarkItem { get; set; } = true;
            public bool CompressSave { get; set; } = true;
            public bool ShowMacroExitsPathfinder { get; set; } = false;
            public bool ShowRedundantPathfinder { get; set; } = false;
            public bool AutoCheckCoupleEntrances { get { return _AutoCheckCoupleEntrances; } set { _AutoCheckCoupleEntrances = value; } }
            private bool _AutoCheckCoupleEntrances = true;
            public bool EntranceRandoFeatures { get; set; } = true;
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
            public Dictionary<char, Dictionary<int, string>>  CapacityMap { get; set; } = new Dictionary<char, Dictionary<int, string>>();
            public Dictionary<int, string> GetCapacityMap(char Currency)
            {
                if (CapacityMap.ContainsKey(Currency)) { return CapacityMap[Currency]; }
                return new Dictionary<int, string>();
            }
        }

        public class ReferenceData
        {
            public LogicFileType LogicList { get; set; }
            public int LogicIndex { get; set; }
        }
        [Serializable]
        public class SpoilerLogFileData
        {
            public string FileName { get; set; }
            public string[] Log { get; set; }
            public Dictionary<string, PlaythroughGenerator.PlaythroughObject> Playthrough { get; set; }
            public void GetStaticPlaythrough(TrackerInstance instance)
            {
                PlaythroughGenerator generator = new(instance);
                generator.GeneratePlaythrough();
                if (instance.LogicDictionary.WinCondition != null)
                {
                    var wincon = instance.LogicDictionary.WinCondition;
                    bool Literal = wincon.IsLiteralID(out string ParsedWinCon);
                    instance.GetItemEntryType(ParsedWinCon, Literal, out object ItemOut);
                    instance.GetItemEntryType(ParsedWinCon, Literal, out object LocationOut);
                    var outitem = ItemOut??LocationOut??null;
                    if (outitem is not null) { generator.FilterImportantPlaythrough(outitem); }
                    Debug.WriteLine($"Seed Beatable: {generator.Playthrough.ContainsKey(instance.LogicDictionary.WinCondition)}");
                }
                Playthrough = generator.Playthrough;
            }
        }

    }
}
