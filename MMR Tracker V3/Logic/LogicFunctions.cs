using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MMR_Tracker_V3.DataStructure;
using static MMR_Tracker_V3.DataStructure.MiscData;

namespace MMR_Tracker_V3.Logic
{
    public class LogicFunctions
    {
        public static readonly Dictionary<string, Func<InstanceData.TrackerInstance, string, string[], List<string>, bool>> Functions = new()
        {
            { "check", (instance, Function, Params, SubUnlockData) => { return CheckAvailableFunction(instance, Function, Params); } },
            { "available", (instance, Function, Params, SubUnlockData) => { return CheckAvailableFunction(instance, Function, Params); } },
            { "contains", (instance, Function, Params, SubUnlockData) => { return CheckContainsFunction(instance, Function, Params); } },
            { "renewable", (instance, Function, Params, SubUnlockData) => { return CheckRenewableFunction(instance, Function, Params); } },
            { "rand", (instance, Function, Params, SubUnlockData) => { return CheckIsRandomizedFunction(instance, Function, Params); } },
            { "randomized", (instance, Function, Params, SubUnlockData) => { return CheckIsRandomizedFunction(instance, Function, Params); } },
            { "trick", (instance, Function, Params, SubUnlockData) => { return CheckTrickEnabledFunction(instance, Function, Params); } },
            { "setting", (instance, Function, Params, SubUnlockData) => { return CheckOptionFunction(instance, Function, Params); } },
            { "option", (instance, Function, Params, SubUnlockData) => { return CheckOptionFunction(instance, Function, Params); } },
            { "comment", (instance, Function, Params, SubUnlockData) => { return true; } }, //Comment Style Check. Will always return true but can state why its returning true and what it's expecting
            { "cmnt", (instance, Function, Params, SubUnlockData) => { return true; } },
            { "cc", (instance, Function, Params, SubUnlockData) => { return true; } },
            { "time", (instance, Function, Params, SubUnlockData) => { return true; } },//The tracker currently doesn't track time, but this can at least display the data for logic 
        };
        public static string[] GetParamList(string Param)
        {
            return Param.Split(',').Select(x => x.Trim()).ToArray();
        }
        public static bool IsLogicFunction(string i, Tuple<char, char> functionCasing = null)
        {
            return IsLogicFunction(i, out _, out _, functionCasing);
        }
        public static bool IsLogicFunction(string i, out string Func, out string Param, Tuple<char, char> functionCasing = null)
        {
            functionCasing ??= new('{', '}');
            Func = null;
            Param = null;
            if (i.IsLiteralID(out _)) { return false; }
            bool squirFunc = i.EndsWith(functionCasing.Item2) && i.Contains(functionCasing.Item1);

            if (!squirFunc) { return false; }

            int funcEnd = i.IndexOf(functionCasing.Item1);
            int paramStart = i.IndexOf(functionCasing.Item1) + 1;
            int paramEnd = i.LastIndexOf(functionCasing.Item2);
            Func = i[..funcEnd].Trim().ToLower();
            Param = i[paramStart..paramEnd].Trim();

            if (!Functions.ContainsKey(Func)) { return false; }
            return true;
        }

        public static bool LogicFunctionAquired(InstanceData.TrackerInstance instance, string i, List<string> SubUnlockData = null)
        {
            SubUnlockData ??= new List<string>();
            if (!IsLogicFunction(i, out string Function, out string Parameters)) { return false; }
            return Functions[Function](instance, Function, GetParamList(Parameters), SubUnlockData);
        }

        private static bool CheckRenewableFunction(InstanceData.TrackerInstance instance, string Function, string[] Parameters)
        {
            var ShouldBeRepeatable = true;
            if (Parameters.Length > 1 && bool.TryParse(Parameters[1], out bool Mod))
            {
                ShouldBeRepeatable = Mod;
            }

            var item = instance.GetItemByID(Parameters[0]);
            if (item is null) { Debug.WriteLine($"{Parameters[0]} Was not a valid Renewable Item Entry"); return false; }
            if (item.GetTotalUsable() < 1) { return false; }
            return instance.LocationPool.Values.Any(x =>
                x.CheckState == CheckState.Checked &&
                IsAtProperCheck(x, ShouldBeRepeatable) &&
                x.Randomizeditem.OwningPlayer == -1 &&
                x.Randomizeditem.Item == item.ID);
            bool IsAtProperCheck(LocationData.LocationObject x, bool ShouldBeRepeatable)
            {
                if (ShouldBeRepeatable) { return x.GetDictEntry().Repeatable is not null && (bool)x.GetDictEntry().Repeatable; }
                else { return x.GetDictEntry().Repeatable is not null && !(bool)x.GetDictEntry().Repeatable; }
            }
        }

        private static bool CheckIsRandomizedFunction(InstanceData.TrackerInstance instance, string Function, string[] Parameters)
        {
            bool Inverse = Parameters.Length > 1 && bool.TryParse(Parameters[1], out bool InverseParam) && !InverseParam;
            bool IsLitteral = Parameters[0].IsLiteralID(out string CleanedID);
            instance.GetLocationEntryType(CleanedID, IsLitteral, out dynamic OBJ);
            if (OBJ is null) { Debug.WriteLine($"{Parameters[0]} is not a valid logic Entry"); return false; }
            if (!Utility.DynamicPropertyExist(OBJ, "RandomizedState")) { Debug.WriteLine($"{Parameters[0]} is not a randomizable entry"); return false; }
            RandomizedState randomizedState = OBJ.RandomizedState;
            return randomizedState == RandomizedState.Randomized != Inverse;
        }

        private static bool CheckTrickEnabledFunction(InstanceData.TrackerInstance instance, string Function, string[] Parameters)
        {
            bool Inverse = Parameters.Length > 1 && bool.TryParse(Parameters[1], out bool InverseParam) && !InverseParam;
            if (!instance.MacroPool.ContainsKey(Parameters[0])) { Debug.WriteLine($"{Parameters[0]} Was not a valid Trick Macro"); return false; }
            if (!instance.MacroPool[Parameters[0]].isTrick()) { Debug.WriteLine($"{Parameters[0]} Was a valid macro but was not a trick"); return false; }
            return instance.MacroPool[Parameters[0]].TrickEnabled != Inverse;
        }

        private static bool CheckAvailableFunction(InstanceData.TrackerInstance instance, string Function, string[] Parameters)
        {
            bool Inverted = Parameters.Length > 1 && bool.TryParse(Parameters[1], out bool IsInverted) && !IsInverted;
            bool litteral = Parameters[0].IsLiteralID(out string paramClean);

            var type = instance.GetLocationEntryType(paramClean, litteral, out dynamic obj);
            if (obj is null) { Debug.WriteLine($"{Parameters[0]} is not a valid logic Entry"); return false; }

            if (Function == "check" && Utility.DynamicPropertyExist(obj, "RandomizedState") && obj.RandomizedState == RandomizedState.ForcedJunk) { Function = "available"; }

            if (Function == "check" && Utility.DynamicPropertyExist(obj, "CheckState")) { return obj.CheckState == CheckState.Checked != Inverted; }
            else if (Function == "available" && Utility.DynamicPropertyExist(obj, "Available")) { return obj.Available != Inverted; }
            else if (Function == "available" && Utility.DynamicPropertyExist(obj, "Aquired")) { return obj.Aquired != Inverted; }
            else { Debug.WriteLine($"{paramClean} could not be checked. Type: {type}"); }
            return false;
        }

        private static bool CheckOptionFunction(InstanceData.TrackerInstance instance, string Function, string[] Parameters)
        {
            if (Parameters.Length < 1) { return false; } //No Values Pased

            var OptionType = instance.GetLocationEntryType(Parameters[0], false, out _);
            bool Inverse = Parameters.Length > 2 && bool.TryParse(Parameters[2], out bool inverseValue) && !inverseValue;

            if (OptionType == LogicEntryType.ToggleOption)
            {
                var TogOpt = instance.ToggleOptions[Parameters[0]];
                if (Parameters.Length < 2 || TogOpt.Enabled.ID == Parameters[1]) { return TogOpt.GetValue() == TogOpt.Enabled != Inverse; }
                else if (TogOpt.Disabled.ID == Parameters[1]) { return TogOpt.GetValue() == TogOpt.Disabled != Inverse; }
                else if (bool.TryParse(Parameters[1], out bool ParamBool))
                {
                    if (ParamBool) { return TogOpt.GetValue() == TogOpt.Enabled != Inverse; }
                    else { return TogOpt.GetValue() == TogOpt.Disabled != Inverse; }
                }
                else
                {
                    return false;
                }
            }
            else if (OptionType == LogicEntryType.ChoiceOption)
            {
                var ChoiceOpt = instance.ChoiceOptions[Parameters[0]];
                if (Parameters.Length < 2) { return false; } //Not enough values passed
                if (!ChoiceOpt.ValueList.ContainsKey(Parameters[1])) { Debug.WriteLine($"{Parameters[1]} was not a valid Value for option {ChoiceOpt.ID}"); }
                return ChoiceOpt.GetValue().ID == Parameters[1] != Inverse;
            }
            else if (OptionType == LogicEntryType.MultiSelectOption)
            {
                var ChoiceOpt = instance.MultiSelectOptions[Parameters[0]];
                if (Parameters.Length < 2) { return false; } //Not enough values passed
                if (!ChoiceOpt.ValueList.ContainsKey(Parameters[1])) { Debug.WriteLine($"{Parameters[1]} was not a valid Value for option {ChoiceOpt.ID}"); }
                return ChoiceOpt.EnabledValues.Contains(Parameters[1]) != Inverse;
            }
            else if (OptionType == LogicEntryType.IntOption)
            {
                var IntOpt = instance.IntOptions[Parameters[0]];
                if (Parameters.Length < 2) { return false; } //Not enough values passed
                if (!int.TryParse(Parameters[1], out int ParamInt)) { return false; }
                return IntOpt.Value == ParamInt != Inverse;
            }
            else
            {
                return false;
            }
        }

        private static bool CheckContainsFunction(InstanceData.TrackerInstance instance, string Function, string[] Parameters)
        {
            if (Parameters.Length < 2) { return false; } //Not enough Values Pased

            string FunctionValue = Parameters[0];
            string ParamValue = Parameters[1];
            List<string> OptionList = new() { FunctionValue };
            List<string> ValueList = new() { ParamValue };

            if (instance.LogicEntryCollections.ContainsKey(FunctionValue))
            {
                OptionList = instance.LogicEntryCollections[FunctionValue].GetValue(instance);
            }

            if (instance.LogicEntryCollections.ContainsKey(ParamValue))
            {
                ValueList = instance.LogicEntryCollections[ParamValue].GetValue(instance);
            }

            bool inverse = Parameters.Length > 2 && bool.TryParse(Parameters[2], out bool inverseValue) && !inverseValue; //If a third param is passed and its a false bool, invert the result

            bool RequiremntMet = false;
            foreach (var currentOption in OptionList)
            {
                var CurrentOptionType = instance.GetLocationEntryType(currentOption, false, out object CurrentOptionEntry);
                foreach (var CurrentValue in ValueList)
                {
                    //This should always return whatever value will result in false if the item is unknown. This is because checking a location should never 
                    //result in another location becoming unavilable otherwise we could enter an infinite loop if both those location are checked automatically
                    if (CurrentOptionType == LogicEntryType.location && instance.GetLocationByID(currentOption)?.GetItemAtCheck() == null) { RequiremntMet = inverse; }
                    else if (CurrentOptionType == LogicEntryType.location && instance.GetLocationByID(currentOption)?.GetItemAtCheck() == CurrentValue) { RequiremntMet = true; }
                    else if (CurrentOptionType == LogicEntryType.Exit && (CurrentOptionEntry as EntranceData.EntranceRandoExit)?.DestinationExit == null) { RequiremntMet = inverse; }
                    else if (CurrentOptionType == LogicEntryType.Exit && (CurrentOptionEntry as EntranceData.EntranceRandoExit).LeadsToArea(CurrentValue)) { RequiremntMet = true; }
                }
            }
            return RequiremntMet != inverse;

        }
    }
    internal class MMRSettingExpressionParser
    {
        private static LogicStringParser SettingExpressionParser = new LogicStringParser();
        public static bool ConvertSettingExpressionToLogic(MMRData.JsonFormatLogicItem LogicItem, LogicDictionaryData.LogicDictionary LogicDict)
        {
            var ParsedExpression = LogicStringConverter.ConvertLogicStringToConditional(SettingExpressionParser, LogicItem.SettingExpression, LogicItem.Id);
            ParsedExpression = ParsedExpression.Select(x => x.Select(y => ParseSegment(y, LogicDict)).ToList()).ToList();

            LogicUtilities.MoveRequirementsToConditionals(LogicItem);
            string CurrentCondString = LogicStringConverter.ConvertConditionalToLogicString(SettingExpressionParser, LogicItem.ConditionalItems);
            string SettingCondString = LogicStringConverter.ConvertConditionalToLogicString(SettingExpressionParser, ParsedExpression);
            string NewCondString = $"({CurrentCondString}) && ({SettingCondString})";

            LogicItem.ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(SettingExpressionParser, NewCondString, LogicItem.Id);

            LogicUtilities.RemoveRedundantConditionals(LogicItem);
            LogicUtilities.MakeCommonConditionalsRequirements(LogicItem);

            return true;
        }

        private static string ParseSegment(string X, LogicDictionaryData.LogicDictionary LogicDict)
        {
            bool Inverse = false;
            string Segment = X.Trim();
            if (Segment.StartsWith("!"))
            {
                Inverse = true;
                Segment = Segment[1..];
            }

            Segment = Segment.SplitOnce('.').Item2;

            if (LogicDict.ToggleOptions.ContainsKey(Segment))
            {
                return $"setting{{{Segment}, {(!Inverse).ToString().ToLower()}}}";
            }

            if (Segment.StartsWith("CustomItemList.Contains"))
            {
                string Item = Segment["CustomItemList.Contains(".Length..^1];
                Item = Item["Item.".Length..];
                return $"randomized{{{Item}, {(!Inverse).ToString().ToLower()}}}";
            }

            if (Segment.Contains(".HasFlag"))
            {
                string[] Data = Segment.Split(".HasFlag");
                string Option = Data[0];
                string Value = Data[1][1..^1].Split('.').Last();
                return $"setting{{{Option}, {Value}, {(!Inverse).ToString().ToLower()}}}";
            }

            if (Segment.Contains(" == "))
            {
                string[] Data = Segment.Split(" == ");
                string Option = Data[0];
                string Value = Data[1].Split('.').Last();
                return $"setting{{{Option}, {Value}}}";
            }

            if (Segment.Contains(" != "))
            {
                string[] Data = Segment.Split(" != ");
                string Option = Data[0];
                string Value = Data[1].Split('.').Last();
                return $"setting{{{Option}, {Value}, false}}";
            }

            return Segment;
        }
    }
}
