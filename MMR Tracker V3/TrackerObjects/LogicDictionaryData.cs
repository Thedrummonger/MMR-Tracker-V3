using MMR_Tracker_V3.TrackerObjectExtentions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.MMRData;
using static MMR_Tracker_V3.TrackerObjects.OptionData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class LogicDictionaryData
    {
        [Serializable]
        public class LogicDictionary
        {
            private TrackerInstance Parent;
            public TrackerInstance GetParentInstancer() { return Parent; }
            public void SetParentContainer(TrackerInstance P) { Parent = P; }
            public int LogicVersion { get; set; }
            public string GameCode { get; set; }
            public string RootArea { get; set; }
            public string WinCondition { get; set; }
            public DefaultSettings DefaultSettings { get; set; }
            public Dictionary<string, DictionaryLocationEntries> LocationList { get; set; } = [];
            public Dictionary<string, DictionaryItemEntries> ItemList { get; set; } = [];
            public Dictionary<string, DictionaryEntranceEntries> EntranceList { get; set; } = [];
            public Dictionary<string, DictionaryHintEntries> HintSpots { get; set; } = [];
            public Dictionary<string, DictionaryMacroEntry> MacroList { get; set; } = [];
            public Dictionary<string, ChoiceOption> ChoiceOptions { get; set; } = [];
            public Dictionary<string, MultiSelectOption> MultiSelectOptions { get; set; } = [];
            public Dictionary<string, ToggleOption> ToggleOptions { get; set; } = [];
            public Dictionary<string, IntOption> IntOptions { get; set; } = [];
            public Dictionary<string, LogicEntryCollection> LogicEntryCollections { get; set; } = [];
            public List<JsonFormatLogicItem> AdditionalLogic { get; set; } = [];
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
            private readonly static Newtonsoft.Json.JsonSerializerSettings _NewtonsoftJsonSerializerOptions = new()
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };
            public List<string> GetAreas()
            {
                return EntranceList.Values.Select(x => x.Area).Concat(EntranceList.Values.Select(x => x.Exit)).Distinct().ToList();
            }
        }

        public class DefaultSettings
        {
            public string CustomItemListString { get; set; }
            public string CustomStartingItemListString { get; set; }
            public string CustomJunkLocationsString { get; set; }
            public Dictionary<string, RandomizedState> ManualRandomizationState { get; set; }
            public List<string> EnabledTricks { get; set; }
        }

        [Serializable]
        public class DictionaryLocationEntries
        {
            private LogicDictionary _parent;
            public LogicDictionary GetParent() { return _parent; }
            public void SetParent(LogicDictionary parent) { _parent = parent; }
            public string ID { get; set; }
            public string Name { get; set; }
            public string OriginalItem { get; set; }
            public char? WalletCurrency { get; set; } = null;
            public string Area { get; set; }
            public string[] ValidItemTypes { get; set; } = [];
            public bool? IgnoreForSettingString { get; set; } = null;
            public bool? Repeatable { get; set; } = null;
            public SpoilerlogReference SpoilerData { get; set; } = new SpoilerlogReference();
            public List<DictLocationProxy> LocationProxys { get; set; } = [];
            public string GetName()
            {
                return Name ?? ID;
            }
        }

        public class DictLocationProxy
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Area { get; set; }
            public string LogicInheritance { get; set; } = null;
            public LocationData.LocationProxy ToInstanceData(DictionaryLocationEntries Parent, InstanceData.TrackerInstance instance)
            {
                return new LocationData.LocationProxy(instance)
                {
                    ID = ID,
                    ReferenceID = Parent.ID,
                    DictInd = Parent.LocationProxys.IndexOf(this)
                };
            }

        }

        [Serializable]
        public class DictionaryItemEntries
        {
            private LogicDictionary _parent;
            public LogicDictionary GetParent() { return _parent; }
            public void SetParent(LogicDictionary parent) { _parent = parent; }
            public string ID { get; set; }
            public string Name { get; set; }
            public int? WalletCapacity { get; set; } = null;
            public char? WalletCurrency { get; set; } = null;
            public int? MaxAmountInWorld { get; set; } = null;
            public bool? ValidStartingItem { get; set; } = null;
            public string[] ItemTypes { get; set; } = [];
            public bool? IgnoreForSettingString { get; set; } = null;
            public SpoilerlogReference SpoilerData { get; set; } = new SpoilerlogReference();

            public string GetName(bool DoEdits = true)
            {
                return DoEdits switch
                {
                    true => _parent.GetParentInstancer().InstanceReference.OptionActionItemEdits[ID].Name,
                    _ => Name??ID,
                };
            }
            public string GetOptionEditDefinedName(List<OptionData.Action> Actions)
            {
                var NameReplaceOption = Actions.FirstOrDefault(x => x.ItemNameOverride.ContainsKey(ID));
                if (NameReplaceOption != null) { return NameReplaceOption.ItemNameOverride[ID]; }
                return Name ?? ID;
            }

            public int GetMaxAmountInWorld()
            {
                if (!_parent.GetParentInstancer().InstanceReference.OptionActionItemEdits.TryGetValue(ID, out ActionItemEdit value)) { return -1; }
                return value.MaxAmount;
            }

            public int GetOptionEditDefinedMaxAmountInWorld(List<OptionData.Action> Actions)
            {
                var OptionsEffectingThisItem = Actions.Where(x => x.ItemMaxAmountEdit.ContainsKey(ID));
                if (!OptionsEffectingThisItem.Any()) { return MaxAmountInWorld ?? -1; }
                int FinalValue = MaxAmountInWorld is null ? -1 : (int)MaxAmountInWorld;
                foreach(var i in OptionsEffectingThisItem)
                {
                    var EditData = i.ItemMaxAmountEdit[ID];
                    switch (EditData.action){
                        case MathOP.add:
                            if (FinalValue < 0) { break; }
                            FinalValue += EditData.amount;
                            break;
                        case MathOP.subtract:
                            if (FinalValue < 0) { break; }
                            if (FinalValue - EditData.amount < 0) { FinalValue = 0; break; }
                            FinalValue -= EditData.amount;
                            break;
                        case MathOP.set:
                            FinalValue = EditData.amount;
                            break;
                    }
                }
                return FinalValue;
            }

        }

        [Serializable]
        public class DictionaryMacroEntry
        {
            private LogicDictionary _parent;
            public LogicDictionary GetParent() { return _parent; }
            public void SetParent(LogicDictionary parent) { _parent = parent; }
            public string ID { get; set; }
            public string Name { get; set; }
            public int? WalletCapacity { get; set; } = null;
            public char? WalletCurrency { get; set; } = null;
            public SpoilerlogReference SpoilerData { get; set; } = new SpoilerlogReference();
        }

        [Serializable]
        public class DictionaryHintEntries
        {
            private LogicDictionary _parent;
            public LogicDictionary GetParent() { return _parent; }
            public void SetParent(LogicDictionary parent) { _parent = parent; }
            public string ID { get; set; }
            public string Name { get; set; }
            public SpoilerlogReference SpoilerData { get; set; } = new SpoilerlogReference();

        }

        [Serializable]
        public class DictionaryEntranceEntries
        {
            private LogicDictionary _parent;
            public LogicDictionary GetParent() { return _parent; }
            public void SetParent(LogicDictionary parent) { _parent = parent; }
            public string ID { get; set; }
            public string Area { get; set; }
            public string Exit { get; set; }
            public string DisplayArea { get; set; } = null;
            public string DisplayExit { get; set; } = null;
            public EntranceData.EntranceAreaPair EntrancePairID { get; set; }
            public bool RandomizableEntrance { get; set; }
            public bool AlwaysAccessable { get; set; } = false;
            public bool DestinationHasSingleEntrance { get; set; } = false;
            public SpoilerlogReference SpoilerData { get; set; } = new SpoilerlogReference();
        }
    }
}
