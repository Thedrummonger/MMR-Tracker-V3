using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            public int LogicVersion { get; set; }
            public string GameCode { get; set; }
            public string RootArea { get; set; }
            public string WinCondition { get; set; }
            public List<DictionaryLocationEntries> LocationList { get; set; } = new List<DictionaryLocationEntries>();
            public List<DictionaryItemEntries> ItemList { get; set; } = new List<DictionaryItemEntries>();
            public List<DictionaryEntranceEntries> EntranceList { get; set; } = new List<DictionaryEntranceEntries>();
            public List<string> AreaList { get; set; } = new List<string>();
            public List<DictionaryMacroEntry> MacroList { get; set; } = new List<DictionaryMacroEntry>();
            public List<JsonFormatLogicItem> AdditionalLogic { get; set; } = new List<JsonFormatLogicItem>();
            public List<TrackerOption> Options { get; set; } = new List<TrackerOption>();
            public List<TrackerVariable> Variables { get; set; } = new List<TrackerVariable>();
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
            public char? WalletCurrency { get; set; } = null;
            public string Area { get; set; }
            public string[] ValidItemTypes { get; set; } = Array.Empty<string>();
            public bool? IgnoreForSettingString { get; set; } = null;
            public SpoilerlogReference SpoilerData { get; set; } = new SpoilerlogReference();
            public List<DictLocationProxy> LocationProxys { get; set; } = new List<DictLocationProxy>();
            public string GetName(LogicObjects.TrackerInstance instance)
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
            public LocationData.LocationProxy ToInstanceData(DictionaryLocationEntries Parent)
            {
                return new LocationData.LocationProxy
                {
                    ID = ID,
                    ReferenceID = Parent.ID,
                    Area = Area,
                    LogicInheritance = LogicInheritance,
                    Name = Name
                };
            }

        }

        [Serializable]
        public class DictionaryItemEntries
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public int? WalletCapacity { get; set; } = null;
            public char? WalletCurrency { get; set; } = null;
            public int? MaxAmountInWorld { get; set; } = null;
            public bool? ValidStartingItem { get; set; } = null;
            public string[] ItemTypes { get; set; } = Array.Empty<string>();
            public SpoilerlogReference SpoilerData { get; set; } = new SpoilerlogReference();

            public string GetName(LogicObjects.TrackerInstance instance)
            {
                var NameReplaceOption = instance.UserOptions.Values.FirstOrDefault(x => x.GetActions().ItemNameOverride.ContainsKey(ID));
                if (NameReplaceOption != null) { return NameReplaceOption.GetActions().ItemNameOverride[ID]; }
                return Name ?? ID;
            }

            public int GetMaxAmountInWorld(LogicObjects.TrackerInstance instance)
            {
                var OptionsEffectingThisItem = instance.UserOptions.Values.Where(x => x.GetActions().ItemMaxAmountEdit.ContainsKey(ID));
                if (!OptionsEffectingThisItem.Any()) { return MaxAmountInWorld ?? -1; }
                int FinalValue = MaxAmountInWorld is null ? -1 : (int)MaxAmountInWorld;
                foreach(var i in OptionsEffectingThisItem)
                {
                    var EditData = i.GetActions().ItemMaxAmountEdit[ID];
                    switch (EditData.action){
                        case MathOP.add:
                            FinalValue += EditData.amount;
                            break;
                        case MathOP.subtract:
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
            public string ID { get; set; }
            public string Name { get; set; }
            public int? WalletCapacity { get; set; } = null;
            public char? WalletCurrency { get; set; } = null;
            public SpoilerlogReference SpoilerData { get; set; } = new SpoilerlogReference();
        }

        [Serializable]
        public class DictionaryHintEntries
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public SpoilerlogReference SpoilerData { get; set; } = new SpoilerlogReference();

        }

        [Serializable]
        public class DictionaryEntranceEntries
        {
            public string ID { get; set; }
            public string Area { get; set; }
            public string Exit { get; set; }
            public string DisplayArea { get; set; } = null;
            public EntranceData.EntranceAreaPair EntrancePairID { get; set; }
            public bool RandomizableEntrance { get; set; }
            public bool AlwaysAccessable { get; set; } = false;
            public bool DestinationHasSingleEntrance { get; set; } = false;
        }

        public class TrackerVariable
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public bool Static  { get; set; } = true;
            private dynamic _value;
            public dynamic Value
            {
                get
                {
                    List<string> JsonList = new List<string>();
                    if ((_value is Newtonsoft.Json.Linq.JArray))
                    {
                        foreach(string i in _value) { JsonList.Add(i); }
                        return JsonList;
                    }
                    return _value;
                }
                set => _value = value;
            }
            public override string ToString()
            {
                return (Name??ID) + ": " + ValueToString();
            }
            public string ValueToString()
            {
                string DisplayValue = null;
                if (Value is string valString) { DisplayValue = valString; }
                if (Value is Int64 valint) { DisplayValue = valint.ToString(); }
                if (Value is bool valbool) { DisplayValue = valbool.ToString(); }
                if (Value is List<string> valListString) { DisplayValue = string.Join(", ", valListString); }
                if (DisplayValue == null) { DisplayValue = $"{Value.GetType().ToString()}"; }
                return DisplayValue;
            }
        }
    }
}
