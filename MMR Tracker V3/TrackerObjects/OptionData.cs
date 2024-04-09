using System;
using System.Collections.Generic;
using System.Linq;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class OptionData
    {
        public abstract class LogicOption(InstanceData.TrackerInstance Parent)
        {
            private InstanceData.TrackerInstance _parent = Parent;
            public InstanceData.TrackerInstance GetParent() { return _parent; }
            public void SetParent(InstanceData.TrackerInstance parent) { _parent = parent; }

            public string ID { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string SubCategory { get; set; }
            public int Priority { get; set; } = 0;
            public List<List<string>> Conditionals { get; set; } = [];

            public abstract string getOptionName();
            public abstract OptionValue GetValue(string _Value = null);
            public abstract MiscData.OptionTypes OptionType();
        }
        public class ChoiceOption(InstanceData.TrackerInstance Parent) : LogicOption(Parent)
        {
            public string Value { get; set; }
            public Dictionary<string, OptionValue> ValueList { get; set; } = new Dictionary<string, OptionValue>();
            public override OptionValue GetValue(string _Value = null)
            {
                if (!ValueList.ContainsKey(_Value ?? Value)) { throw new Exception($"{_Value ?? Value} was not a valid value for Option {ID}"); }
                return ValueList[_Value ?? Value];
            }
            public void SetValue(string _Value)
            {
                if (!ValueList.ContainsKey(_Value)) { throw new Exception($"{_Value} was not a valid value for Option {ID}"); }
                Value = _Value;
            }
            public void SetDynValue(object _Value)
            {
                if (_Value is Int64 Int64BoolVal) { SetValue(ValueList.Keys.ToArray()[Int64BoolVal]); }
                else if (_Value is int IntBoolVal) { SetValue(ValueList.Keys.ToArray()[IntBoolVal]); }
                else if (_Value is string StringBoolVal) { SetValue(StringBoolVal); }
                else { throw new Exception($"{_Value.GetType().Name} {_Value} Could not be applied to a choice option"); }
            }
            public ChoiceOption CreateSimpleValues(params string[] Values)
            {
                ValueList = Values.ToDictionary(x => x, x => new OptionValue { ID = x, Name = x });
                return this;
            }
            public ChoiceOption CreateSimpleValues(params (string id, string name)[] Values)
            {
                ValueList = Values.ToDictionary(x => x.id, x => new OptionValue { ID = x.id, Name = x.name });
                return this;
            }
            public override string ToString()
            {
                return $"{getOptionName()}: {getValueName()}";
            }
            public override string getOptionName()
            {
                return string.IsNullOrWhiteSpace(Name) ? ID : Name;
            }
            public string getValueName(string _val = null)
            {
                if (!ValueList.ContainsKey(_val ?? Value)) { return _val; }
                return string.IsNullOrWhiteSpace(ValueList[_val ?? Value].Name) ? ValueList[_val ?? Value].ID : ValueList[_val ?? Value].Name;
            }

            public override MiscData.OptionTypes OptionType() => MiscData.OptionTypes.ChoiceOption;
        }

        public class MultiSelectOption(InstanceData.TrackerInstance Parent) : LogicOption(Parent)
        {
            public HashSet<string> EnabledValues { get; set; }
            public Dictionary<string, OptionValue> ValueList { get; set; } = new Dictionary<string, OptionValue>();
            public override OptionValue GetValue(string _Value)
            {
                if (!ValueList.ContainsKey(_Value)) { throw new Exception($"{_Value} was not a valid value for Option {ID}"); }
                return ValueList[_Value];
            }
            public OptionValue[] GetEnabledValues()
            {
                return ValueList.Where(x => EnabledValues.Contains(x.Key)).Select(x => x.Value).ToArray();
            }
            public OptionValue[] GetDisabledValues()
            {
                return ValueList.Where(x => !EnabledValues.Contains(x.Key)).Select(x => x.Value).ToArray();
            }
            public void ToggleValue(string _Value)
            {
                if (!ValueList.ContainsKey(_Value)) { throw new Exception($"{_Value} was not a valid value for Option {ID}"); }

                if (!EnabledValues.Contains(_Value)) { EnabledValues.Add(_Value); }
                else if (EnabledValues.Contains(_Value)) { EnabledValues.Remove(_Value); }
            }
            public void SetValue(string _Value, bool State)
            {
                if (!ValueList.ContainsKey(_Value)) { throw new Exception($"{_Value} was not a valid value for Option {ID}"); }

                if (State) { EnabledValues.Add(_Value); }
                else { EnabledValues.Remove(_Value); }
            }
            public void SetValues(IEnumerable<string> _Values)
            {
                EnabledValues.Clear();
                foreach (var _Value in _Values)
                {
                    if (!ValueList.ContainsKey(_Value)) { throw new Exception($"{_Value} was not a valid value for Option {ID}"); }
                    EnabledValues.Add(_Value);
                }
            }
            public MultiSelectOption CreateSimpleValues(params string[] Values)
            {
                ValueList = Values.ToDictionary(x => x, x => new OptionValue { ID = x, Name = x });
                return this;
            }
            public override string ToString()
            {
                return $"{getOptionName()}";
            }
            public override string getOptionName()
            {
                return string.IsNullOrWhiteSpace(Name) ? ID : Name;
            }
            public string getValueName(string _val)
            {
                if (!ValueList.ContainsKey(_val)) { return _val; }
                return string.IsNullOrWhiteSpace(ValueList[_val].Name) ? ValueList[_val].ID : ValueList[_val].Name;
            }
            public override MiscData.OptionTypes OptionType() => MiscData.OptionTypes.MultiSelectOption;
        }
        public class MultiSelectValueListDisplay
        {
            public MultiSelectOption Parent { get; set; }
            public OptionValue Value { get; set; }
            public override string ToString()
            {
                return $"-{Value}: {Parent.EnabledValues.Contains(Value.ID)}";
            }
        }
        public class ToggleOption(InstanceData.TrackerInstance Parent) : LogicOption(Parent)
        {
            public bool Value { get; set; }
            public OptionValue Enabled { get; set; }
            public OptionValue Disabled { get; set; }
            public override OptionValue GetValue(string _Value = null)
            {
                if (_Value is null) { return GetValue(Value); }
                if (!bool.TryParse(_Value, out bool val)) { throw new Exception($"{_Value} was not a valid value for Option {ID}"); }
                return GetValue(val);
            }
            public OptionValue GetValue(bool _Value) => _Value ? Enabled : Disabled;
            public void SetValue(bool _Value)
            {
                Value = _Value;
            }
            public void SetValue(string _Value)
            {
                if (!bool.TryParse(_Value, out bool val)) { throw new Exception($"{_Value} was not a valid value for Option {ID}"); }
                Value = val;
            }
            public void SetDynValue(object _Value)
            {
                if (_Value is bool boolVal) { SetValue(boolVal); }
                else if (_Value is int IntBoolVal) { SetValue(IntBoolVal > 0); }
                else if (_Value is Int64 Int64BoolVal) { SetValue(Int64BoolVal > 0); }
                else if (_Value is string StringBoolVal && bool.TryParse(StringBoolVal, out bool rb)) { SetValue(rb); }
                else { throw new Exception($"{_Value.GetType().Name} {_Value} Could not be applied to a toggle option"); }
            }
            public void ToggleValue()
            {
                Value = !Value;
            }
            public bool IsEnabled() => Value;
            public ToggleOption CreateSimpleValues(bool Lower = false)
            {
                string EID = Lower ? true.ToString().ToLower() : true.ToString();
                string FID = Lower ? false.ToString().ToLower() : false.ToString();
                Enabled = new OptionValue { ID = EID, Name = EID };
                Disabled = new OptionValue { ID = FID, Name = FID };
                return this;
            }
            public override string ToString()
            {
                return $"{getOptionName()}: {getValueName()}";
            }
            public override string getOptionName()
            {
                return string.IsNullOrWhiteSpace(Name) ? ID : Name;
            }
            public string getValueName()
            {
                OptionValue selectedVal = GetValue();
                return string.IsNullOrWhiteSpace(selectedVal.Name) ? selectedVal.ID : selectedVal.Name;
            }
            public override MiscData.OptionTypes OptionType() => MiscData.OptionTypes.ToggleOption;

        }
        public class IntOption(InstanceData.TrackerInstance Parent) : LogicOption(Parent)
        {
            public int Value { get; set; }
            public int Min { get; set; } = int.MinValue;
            public int Max { get; set; } = int.MaxValue;
            public void SetValue(int _Value)
            {
                if (_Value > Max || _Value < Min) { throw new Exception($"{_Value} was not a valid value for Option {ID}"); }
                Value = _Value;
            }
            public void SetDynValue(object _Value)
            {
                if (_Value is int i1) { SetValue(i1); }
                else if (_Value is Int64 i64) { SetValue(Convert.ToInt32(i64)); }
                else if (_Value is string str && int.TryParse(str, out int istr)) { SetValue(istr); }
                else { throw new Exception($"{_Value.GetType().Name} {_Value} Could not be applied to an int option"); }
            }
            public override string ToString()
            {
                return $"{getOptionName()}: {getValueName()}";
            }
            public override string getOptionName()
            {
                return string.IsNullOrWhiteSpace(Name) ? ID : Name;
            }
            public string getValueName()
            {
                return Value.ToString();
            }
            public override OptionValue GetValue(string _Value)
            {
                return new OptionValue { ID = Value.ToString(), Name = Value.ToString(), Actions = new Action() };
            }
            public override MiscData.OptionTypes OptionType() => MiscData.OptionTypes.IntOption;

        }

        public class LogicEntryCollection
        {
            public string ID { get; set; }
            public List<string> Entries { get; set; } = new List<string> { };

            public List<string> GetOptionEditDefinedValue(IEnumerable<Action> Edits)
            {
                var ReturnList = new List<string>();
                ReturnList.AddRange(Entries);
                foreach (var edit in Edits.Select(x => x.VariableEdit))
                {
                    if (!edit.ContainsKey(ID)) { continue; }
                    switch (edit[ID].action)
                    {
                        case MiscData.MathOP.add:
                            ReturnList.AddRange(edit[ID].Values);
                            break;
                        case MiscData.MathOP.subtract:
                            foreach (var value in edit[ID].Values) { ReturnList.Remove(value); }
                            break;
                        case MiscData.MathOP.set:
                            ReturnList.Clear();
                            ReturnList.AddRange(edit[ID].Values);
                            break;
                    }
                }
                return ReturnList;
            }

            public List<string> GetValue(InstanceData.TrackerInstance instance)
            {
                return instance.InstanceReference.OptionActionCollectionEdits[ID];
            }
        }

        public class OptionValue
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public Action Actions { get; set; } = new Action();
            public override string ToString()
            {
                string Dis = string.IsNullOrWhiteSpace(Name) ? ID : Name;
                return Dis;
            }
        }

        public class Action
        {
            public LogicReplacement[] LogicReplacements { get; set; } = Array.Empty<LogicReplacement>();
            public AdditionalLogic[] AdditionalLogic { get; set; } = Array.Empty<AdditionalLogic>();
            public Dictionary<string, string> ItemNameOverride { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, MaxAmountSetData> ItemMaxAmountEdit { get; set; } = new Dictionary<string, MaxAmountSetData>();
            public Dictionary<string, CollectionEditData> VariableEdit { get; set; } = new Dictionary<string, CollectionEditData>();
            public bool LocationValid(string ID)
            {
                return LogicReplacements.Any(x => x.LocationValid(ID)) || AdditionalLogic.Any(x => x.LocationValid(ID));
            }

            public void AddMaxAmountEdit(string Item, MiscData.MathOP mathOP, int amount)
            {
                ItemMaxAmountEdit.Add(Item, new MaxAmountSetData { action = mathOP, amount = amount });
            }
        }

        public class ActionItemEdit
        {
            public string Name;
            public int MaxAmount;
        }

        public class MaxAmountSetData
        {
            public MiscData.MathOP action { get; set; } = MiscData.MathOP.add;
            public int amount { get; set; } = 0;
        }

        public class CollectionEditData
        {
            public MiscData.MathOP action { get; set; } = MiscData.MathOP.add;
            public List<string> Values { get; set; } = new List<string>();
        }

        [Serializable]
        public class AdditionalLogic
        {
            public string[] LocationWhitelist { get; set; } = Array.Empty<string>();
            public string[] LocationBlacklist { get; set; } = Array.Empty<string>();
            public string[] AdditionalRequirements { get; set; } = Array.Empty<string>();
            public List<string>[] AdditionalConditionals { get; set; } = Array.Empty<List<string>>();
            public bool LocationValid(string ID)
            {
                return (!LocationWhitelist.Any() || LocationWhitelist.Contains(ID)) && !LocationBlacklist.Contains(ID);
            }
        }

        [Serializable]
        public class LogicReplacement
        {
            public string[] LocationWhitelist { get; set; } = Array.Empty<string>();
            public string[] LocationBlacklist { get; set; } = Array.Empty<string>();
            public Dictionary<string, string> ReplacementList { get; set; } = new Dictionary<string, string>();
            public bool LocationValid(string ID)
            {
                return (!LocationWhitelist.Any() || LocationWhitelist.Contains(ID)) && !LocationBlacklist.Contains(ID);
            }
        }
    }
}
