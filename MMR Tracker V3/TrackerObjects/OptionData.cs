using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class OptionData
    {
        private static readonly Dictionary<string, string> ToggleValues = new()
        {
            {"yes", "no" },
            {"enabled", "disabled" },
            {"true", "false" },
            {"on", "off" },
            {"open", "closed" },
        };

        public static Dictionary<string, string> GetToggleValues()
        {
            return ToggleValues;
        }

        public static readonly string[] DisplayListBoxes = { "None", "Available Locations", "Checked Locations" };

        [Serializable]
        public class TrackerOption
        {
            public string ID { get; set; }
            public string DisplayName { get; set; }
            public string CurrentValue { get; set; } = "";
            public string SubCategory { get; set; } = null;
            public Dictionary<string, actions> Values { get; set; } = new Dictionary<string, actions>();
            public actions GetActions()
            {
                return Values[CurrentValue];
            }
            public bool IsToggleOption()
            {
                return 
                    Values.Count == 2 &&
                    Values.Keys.Select(x => x.ToLower()).Intersect(ToggleValues.Keys).Any() && 
                    Values.Keys.Select(x => x.ToLower()).Intersect(ToggleValues.Values).Any();
            }
            public void ToggleOption()
            {
                if (!this.IsToggleOption()) { return; }
                CurrentValue = Values.Keys.First(x => x != CurrentValue);
            }

            public void CreateSimpleValues(string[] SimpleValues) { Values = SimpleValues.ToDictionary(x => x, x => new actions()); }

            public override string ToString()
            {
                return DisplayName + ": " + CurrentValue;
            }
        }

        [Serializable]
        public class actions
        {
            public string Name { get; set; } = null;
            public LogicReplacement[] LogicReplacements { get; set; } = Array.Empty<LogicReplacement>();
            public AdditionalLogic[] AdditionalLogic { get; set; } = Array.Empty<AdditionalLogic>();
            public Dictionary<string, string> ItemNameOverride { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, MaxAmountSetData> ItemMaxAmountEdit { get; set; } = new Dictionary<string, MaxAmountSetData>();
            public Dictionary<string, VariableEditData> VariableEdit { get; set; } = new Dictionary<string, VariableEditData>();
            public bool LocationValid(string ID)
            {
                return LogicReplacements.Any(x => x.LocationValid(ID)) || AdditionalLogic.Any(x => x.LocationValid(ID));
            }

            public void AddMaxAmountEdit(string Item, MiscData.MathOP mathOP, int amount)
            {
                ItemMaxAmountEdit.Add(Item, new MaxAmountSetData { action = mathOP, amount = amount});
            }
        }

        public class MaxAmountSetData
        {
            public MiscData.MathOP action { get; set; } = MiscData.MathOP.add;
            public int amount { get; set; } = 0;
        }

        public class VariableEditData
        {
            public MiscData.MathOP action { get; set; } = MiscData.MathOP.add;
            public dynamic EditValue { get; set; } = 0;
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


        public class TrackerVar
        {
            private dynamic _value;
            public string ID { get; set; }
            public string Name { get; set; }
            public bool Static { get; set; } = true;
            public dynamic Value { set { _value = ParseInput(value); } get { return _value; } }

            private static dynamic ParseInput(dynamic value)
            {
                if (value is string @string) { return @string; }
                if (value is Int64 || value is Int32 || value is Int16 || value is int) { return (int)value; }
                if (value is bool boolean) { return boolean; }
                if (value is List<string> || value is Array || value is Newtonsoft.Json.Linq.JArray) 
                { 
                    List<string> list = new();
                    foreach(object i in value) { list.Add(i.ToString()); } 
                    return list;
                }
                return value.ToString();
            }

            public string ValueToString()
            {
                string DisplayValue = null;
                if (_value is string valString) { DisplayValue = valString; }
                if (_value is int valint) { DisplayValue = valint.ToString(); }
                if (_value is bool valbool) { DisplayValue = valbool.ToString(); }
                if (_value is List<string> valListString) { DisplayValue = string.Join(", ", valListString); }
                DisplayValue ??= $"{_value.ToString()}";
                return DisplayValue;
            }

            public override string ToString()
            {
                return (Name??ID) + ": " + ValueToString();
            }

            public new MiscData.VariableEntryType GetType()
            {
                if (_value is string) { return MiscData.VariableEntryType.varstring; }
                if (_value is int) { return MiscData.VariableEntryType.varint; }
                if (_value is bool) { return MiscData.VariableEntryType.varbool; }
                if (_value is List<string>) { return MiscData.VariableEntryType.varlist; }
                return MiscData.VariableEntryType.error;
            }

            public dynamic GetValue(LogicObjects.TrackerInstance instance) 
            { 
                return GetActualValue(instance);
            }

            private dynamic GetActualValue(LogicObjects.TrackerInstance instance)
            {
                string ReturnString = GetType() == MiscData.VariableEntryType.varstring ? _value : string.Empty;
                bool ReturnBool = GetType() == MiscData.VariableEntryType.varbool ? _value : false;
                int ReturnInt = GetType() == MiscData.VariableEntryType.varint ? _value : 0;
                List<string> ReturnList = new List<string>();
                if (GetType() == MiscData.VariableEntryType.varlist) { ReturnList.AddRange(_value); }

                foreach (var i in instance.UserOptions.Values)
                {
                    var edits = i.GetActions().VariableEdit;
                    if (edits.ContainsKey(ID))
                    {
                        TrackerVar EditValue = new() { Value = edits[ID].EditValue };
                        switch (GetType())
                        {
                            case MiscData.VariableEntryType.varlist:
                                switch (edits[ID].action)
                                {
                                    case MiscData.MathOP.add:   
                                        if (EditValue.GetType() == MiscData.VariableEntryType.varlist) { ReturnList.AddRange(EditValue.Value); }
                                        else { ReturnList.Add(((object)EditValue.Value).ToString()); }
                                        break;
                                    case MiscData.MathOP.subtract:
                                        if (EditValue.GetType() == MiscData.VariableEntryType.varlist) { ReturnList.RemoveAll(x => ((List<string>)EditValue.Value).Contains(x)); }
                                        else { ReturnList.Remove(((object)EditValue.Value).ToString()); }
                                        break;
                                    case MiscData.MathOP.set:
                                        if (EditValue.GetType() == MiscData.VariableEntryType.varlist) { ReturnList = (List<string>)EditValue.Value; }
                                        else { ReturnList =  new() { ((object)EditValue.Value).ToString() }; }
                                        break;
                                }
                                break;
                            case MiscData.VariableEntryType.varint:
                                if (EditValue.GetType() != MiscData.VariableEntryType.varint) { continue; }
                                switch (edits[ID].action)
                                {
                                    case MiscData.MathOP.add:
                                        ReturnInt += (int)EditValue.Value;
                                        break;
                                    case MiscData.MathOP.subtract:
                                        ReturnInt -= (int)EditValue.Value;
                                        break;
                                    case MiscData.MathOP.multiply:
                                        ReturnInt *= (int)EditValue.Value;
                                        break;
                                    case MiscData.MathOP.divide:
                                        ReturnInt /= (int)EditValue.Value;
                                        break;
                                    case MiscData.MathOP.set:
                                        ReturnInt = (int)EditValue.Value;
                                        break;
                                }
                                break;
                        }
                    }
                }

                return GetType() switch
                {
                    MiscData.VariableEntryType.varstring => ReturnString,
                    MiscData.VariableEntryType.varint => ReturnInt,
                    MiscData.VariableEntryType.varbool => ReturnBool,
                    MiscData.VariableEntryType.varlist => ReturnList,
                    _ => _value,
                };
            }
        }
    }
}
