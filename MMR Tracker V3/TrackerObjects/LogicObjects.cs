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
            public Dictionary<string, TrackerVariable> Variables { get; set; } = new Dictionary<string, TrackerVariable>();
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
            private readonly static Newtonsoft.Json.JsonSerializerSettings _NewtonsoftJsonSerializerOptions = new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };
            public ItemObject GetItemByID(string item)
            {
                if (item is null) { return null; }
                if (!ItemPool.ContainsKey(item)) { return null; }
                return ItemPool[item];
            }
            public LocationObject GetLocationByID(string item)
            {
                if (item is null) { return null; }
                if (!LocationPool.ContainsKey(item)) { return null; }
                return LocationPool[item];
            }

            public MacroObject GetMacroByID(string item)
            {
                if (item is null) { return null; }
                if (!MacroPool.ContainsKey(item)) { return null; }
                return MacroPool[item];
            }

            public TrackerOption GetTrackerOptionByID(string item)
            {
                if (item is null) { return null; }
                if (!UserOptions.ContainsKey(item)) { return null; }
                return UserOptions[item];
            }

            public LogicEntryType GetLocationEntryType(string ID, bool literal, out object Obj)
            {
                if (literal && LocationPool.ContainsKey(ID)) { Obj = LocationPool[ID]; return LogicEntryType.location; }
                if (literal && InstanceReference.EntranceLogicNameToEntryData.ContainsKey(ID)) 
                {
                    var EA = InstanceReference.EntranceLogicNameToEntryData[ID];
                    Obj = EntrancePool.AreaList[EA.Area].Exits[EA.Exit]; 
                    return LogicEntryType.Exit; 
                }
                if (literal && HintPool.ContainsKey(ID)) { Obj = HintPool[ID]; return LogicEntryType.Hint; }
                if (MacroPool.ContainsKey(ID)) { Obj = MacroPool[ID]; return LogicEntryType.macro; }
                if (!literal && LocationPool.ContainsKey(ID)) { Obj = LocationPool[ID]; return LogicEntryType.location; }
                if (!literal && InstanceReference.EntranceLogicNameToEntryData.ContainsKey(ID))
                {
                    var EA = InstanceReference.EntranceLogicNameToEntryData[ID];
                    Obj = EntrancePool.AreaList[EA.Area].Exits[EA.Exit];
                    return LogicEntryType.Exit; 
                }
                if (!literal && HintPool.ContainsKey(ID)) { Obj = HintPool[ID]; return LogicEntryType.Hint; }
                //if (UserOptions.ContainsKey(ID)) { Obj = UserOptions[ID]; return LogicEntryType.Option; }
                if (Variables.ContainsKey(ID))
                {
                    Obj = Variables[ID];
                    if (Variables[ID].GetValue(this) is string) { return LogicEntryType.variableString; }
                    if (Variables[ID].GetValue(this) is bool) { return LogicEntryType.variableBool; }
                    if (Variables[ID].GetValue(this) is Int64) { return LogicEntryType.variableInt; }
                    if (Variables[ID].GetValue(this) is Newtonsoft.Json.Linq.JArray || Variables[ID].GetValue(this) is List<string>) { return LogicEntryType.variableList; }
                }
                Obj = null;
                return LogicEntryType.error;
            }

            public LogicEntryType GetItemEntryType(string OriginalID, bool literal, out object obj)
            {
                this.MultipleItemEntry(OriginalID, out string ID, out _);
                if (literal && ItemPool.ContainsKey(ID)) { obj = ItemPool[ID]; return LogicEntryType.item; }
                if (literal && EntrancePool.AreaList.ContainsKey(ID)) { obj = EntrancePool.AreaList[ID]; return LogicEntryType.Area; }
                if (MacroPool.ContainsKey(ID)) { obj = MacroPool[ID]; return LogicEntryType.macro; }
                if (!literal && ItemPool.ContainsKey(ID)) { obj = ItemPool[ID]; return LogicEntryType.item; }
                if (!literal && EntrancePool.AreaList.ContainsKey(ID)) { obj = EntrancePool.AreaList[ID]; return LogicEntryType.Area; }
                if (Variables.ContainsKey(ID))
                {
                    obj = Variables[ID];
                    if (Variables[ID].GetValue(this) is string) { return LogicEntryType.variableString; }
                    if (Variables[ID].GetValue(this) is bool) { return LogicEntryType.variableBool; }
                    if (Variables[ID].GetValue(this) is Int64) { return LogicEntryType.variableInt; }
                    if (Variables[ID].GetValue(this) is Newtonsoft.Json.Linq.JArray || Variables[ID].GetValue(this) is List<string>) { return LogicEntryType.variableList; }
                }
                if (bool.TryParse(ID, out bool result)) { obj = result; return LogicEntryType.Bool; }
                obj = null;
                if (LogicEditing.CheckLogicFunction(this, OriginalID, new List<string>(), out _, false)) { return LogicEntryType.function; }
                return LogicEntryType.error;
            }

            public ItemObject GetItemToPlace(string Item, bool SpoilerName = true, bool Starting = false)
            {
                ItemObject ValidItem;
                if (SpoilerName)
                {
                    ValidItem = ItemPool.Values.FirstOrDefault(x =>
                        x.GetDictEntry(this)?.SpoilerData?.SpoilerLogNames != null &&
                        x.GetDictEntry(this).SpoilerData.SpoilerLogNames.Contains(Item) &&
                        x.CanBePlaced(this) && (x.ValidStartingItem(this) || !Starting));
                }
                else
                {
                    ValidItem = ItemPool.Values.FirstOrDefault(x =>
                        x.GetDictEntry(this)?.Name != null && x.GetDictEntry(this).Name == Item &&
                        x.CanBePlaced(this) && (x.ValidStartingItem(this) || !Starting));
                }
                return ValidItem;
            }

            public void ToggleAllTricks(bool? state)
            {
                if (state is null) { foreach (var i in MacroPool.Values.Where(x => x.isTrick(this))) { i.TrickEnabled = !i.TrickEnabled; } }
                else { foreach (var i in MacroPool.Values.Where(x => x.isTrick(this))) { i.TrickEnabled = (bool)state; } }
            }

            public List<char> GetAllCurrencies(bool all = false)
            {
                return PriceData.CapacityMap.Keys.Where(x => x != '*' || all).ToList();
            }
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
            public bool DecoupleEntrances { get; set; } = false;
            public bool ShowMacroExitsPathfinder { get; set; } = false;
            public bool AutoCheckCoupleEntrances { get { return !DecoupleEntrances && _AutoCheckCoupleEntrances; } set { _AutoCheckCoupleEntrances = value; } }
            private bool _AutoCheckCoupleEntrances = true;
            public bool EntranceRandoFeatures { get; set; } = true;

            public Dictionary<string, bool> MinimizedHeader { get; set; } = new Dictionary<string, bool>();
            public OptionFile OptionFile { get; set; } = new OptionFile();
        }

        [Serializable]
        public class OptionFile
        {
            public bool CheckForUpdate { get; set; } = true;
            public bool CheckHintMarkItem { get; set; } = true;
            public bool CompressSave { get; set; } = true;
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
        }

    }
}
