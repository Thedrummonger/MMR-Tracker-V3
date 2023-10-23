using MMR_Tracker_V3.TrackerObjectExtentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            public Dictionary<string, DictionaryLocationEntries> LocationList { get; set; } = new Dictionary<string, DictionaryLocationEntries>();
            public Dictionary<string, DictionaryItemEntries> ItemList { get; set; } = new Dictionary<string, DictionaryItemEntries>();
            public Dictionary<string, DictionaryEntranceEntries> EntranceList { get; set; } = new Dictionary<string, DictionaryEntranceEntries>();
            public Dictionary<string, DictionaryHintEntries> HintSpots { get; set; } = new Dictionary<string, DictionaryHintEntries>();
            public Dictionary<string, DictionaryMacroEntry> MacroList { get; set; } = new Dictionary<string, DictionaryMacroEntry>();
            public Dictionary<string, ChoiceOption> ChoiceOptions { get; set; } = new Dictionary<string, ChoiceOption>();
            public Dictionary<string, ToggleOption> ToggleOptions { get; set; } = new Dictionary<string, ToggleOption>();
            public Dictionary<string, IntOption> IntOptions { get; set; } = new Dictionary<string, IntOption>();
            public Dictionary<string, LogicEntryCollection> LogicEntryCollections { get; set; } = new Dictionary<string, LogicEntryCollection>();
            public List<JsonFormatLogicItem> AdditionalLogic { get; set; } = new List<JsonFormatLogicItem>();
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
            public List<string> GetAreas()
            {
                return EntranceList.Values.Select(x => x.Area).Concat(EntranceList.Values.Select(x => x.Exit)).Distinct().ToList();
            }
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
            public bool? Repeatable { get; set; } = null;
            public SpoilerlogReference SpoilerData { get; set; } = new SpoilerlogReference();
            public List<DictLocationProxy> LocationProxys { get; set; } = new List<DictLocationProxy>();
            public string GetName(InstanceData.TrackerInstance instance)
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

            public string GetName(InstanceData.TrackerInstance instance)
            {
                var NameReplaceOption = instance.GetOptionActions().FirstOrDefault(x => x.ItemNameOverride.ContainsKey(ID));
                if (NameReplaceOption != null) { return NameReplaceOption.ItemNameOverride[ID]; }
                return Name ?? ID;
            }

            public int GetMaxAmountInWorld(InstanceData.TrackerInstance instance)
            {
                var OptionsEffectingThisItem = instance.GetOptionActions().Where(x => x.ItemMaxAmountEdit.ContainsKey(ID));
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
            public string DisplayExit { get; set; } = null;
            public EntranceData.EntranceAreaPair EntrancePairID { get; set; }
            public bool RandomizableEntrance { get; set; }
            public bool AlwaysAccessable { get; set; } = false;
            public bool DestinationHasSingleEntrance { get; set; } = false;
            public SpoilerlogReference SpoilerData { get; set; } = new SpoilerlogReference();
        }
    }
}
