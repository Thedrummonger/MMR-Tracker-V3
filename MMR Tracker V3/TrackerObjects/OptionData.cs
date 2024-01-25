using System;
using System.Collections.Generic;
using System.Linq;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class OptionData
    {
        public class ChoiceOption
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public string Description { get; set; }
            public string SubCategory { get; set; }
            public int Priority { get; set; } = 0;
            public List<List<string>> Conditionals { get; set; } = new List<List<string>>();
            public Dictionary<string, OptionValue> ValueList { get; set; } = new Dictionary<string, OptionValue>();
            public OptionValue GetValue(string _Value = null)
            {
                if (!ValueList.ContainsKey(_Value ?? Value)) { throw new Exception($"{_Value ?? Value} was not a valid value for Option {ID}"); }
                return ValueList[_Value ?? Value];
            }
            public void SetValue(string _Value)
            {
                if (!ValueList.ContainsKey(_Value)) { throw new Exception($"{_Value} was not a valid value for Option {ID}"); }
                Value = _Value;
            }
            public ChoiceOption CreateSimpleValues(params string[] Values)
            {
                ValueList = Values.ToDictionary(x => x, x => new OptionValue { ID = x, Name = x });
                return this;
            }
            public override string ToString()
            {
                return $"{getOptionName()}: {getValueName()}";
            }
            public string getOptionName()
            {
                return string.IsNullOrWhiteSpace(Name) ? ID : Name;
            }
            public string getValueName(string _val = null)
            {
                if (!ValueList.ContainsKey(_val ?? Value)) { return _val; }
                return string.IsNullOrWhiteSpace(ValueList[_val ?? Value].Name) ? ValueList[_val ?? Value].ID : ValueList[_val ?? Value].Name;
            }
        }

        public class MultiSelectOption
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public HashSet<string> EnabledValues { get; set; }
            public string Description { get; set; }
            public string SubCategory { get; set; }
            public int Priority { get; set; } = 0;
            public List<List<string>> Conditionals { get; set; } = new List<List<string>>();
            public Dictionary<string, OptionValue> ValueList { get; set; } = new Dictionary<string, OptionValue>();
            public OptionValue GetValue(string _Value)
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
            public MultiSelectOption CreateSimpleValues(params string[] Values)
            {
                ValueList = Values.ToDictionary(x => x, x => new OptionValue { ID = x, Name = x });
                return this;
            }
            public override string ToString()
            {
                return $"{getOptionName()}";
            }
            public string getOptionName()
            {
                return string.IsNullOrWhiteSpace(Name) ? ID : Name;
            }
            public string getValueName(string _val)
            {
                if (!ValueList.ContainsKey(_val)) { return _val; }
                return string.IsNullOrWhiteSpace(ValueList[_val].Name) ? ValueList[_val].ID : ValueList[_val].Name;
            }
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
        public class ToggleOption
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public string Description { get; set; }
            public string SubCategory { get; set; }
            public int Priority { get; set; }
            public List<List<string>> Conditionals { get; set; } = new List<List<string>>();
            public OptionValue Enabled { get; set; }
            public OptionValue Disabled { get; set; }
            public OptionValue GetValue(string _Value = null)
            {
                string v = _Value ?? Value;
                OptionValue vo;
                if (Enabled.ID == v) { vo = Enabled; }
                else if (Disabled.ID == v) { vo = Disabled; }
                else { vo = null; }

                if (vo is null) { throw new Exception($"{_Value ?? Value} was not a valid value for Option {ID}"); }
                return vo;
            }
            public void SetValue(bool _Value)
            {
                Value = _Value ? Enabled.ID : Disabled.ID;
            }
            public void SetValue(string _Value)
            {
                string v = _Value;
                OptionValue vo;
                if (Enabled.ID == v) { vo = Enabled; }
                else if (Disabled.ID == v) { vo = Disabled; }
                else { vo = null; }

                if (vo is null) { throw new Exception($"{_Value ?? Value} was not a valid value for Option {ID}"); }
                Value = vo.ID;
            }
            public void ToggleValue()
            {
                if (Value == Disabled.ID) { Value = Enabled.ID; }
                else { Value = Disabled.ID; }
            }
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
            public string getOptionName()
            {
                return string.IsNullOrWhiteSpace(Name) ? ID : Name;
            }
            public string getValueName()
            {
                OptionValue selectedVal = Enabled.ID == Value ? Enabled : Disabled;
                return string.IsNullOrWhiteSpace(selectedVal.Name) ? selectedVal.ID : selectedVal.Name;
            }

        }
        public class IntOption
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public int Value { get; set; }
            public string Description { get; set; }
            public string SubCategory { get; set; }
            public int Priority { get; set; }
            public List<List<string>> Conditionals { get; set; } = new List<List<string>>();
            public int Min { get; set; } = int.MinValue;
            public int Max { get; set; } = int.MaxValue;
            public void SetValue(int _Value)
            {
                if (_Value > Max || _Value < Min) { throw new Exception($"{_Value} was not a valid value for Option {ID}"); }
                Value = _Value;
            }
            public override string ToString()
            {
                return $"{getOptionName()}: {getValueName()}";
            }
            public string getOptionName()
            {
                return string.IsNullOrWhiteSpace(Name) ? ID : Name;
            }
            public string getValueName()
            {
                return Value.ToString();
            }

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
