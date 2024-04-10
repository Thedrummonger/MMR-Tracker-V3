using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

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
            { "price", (instance, Function, Params, SubUnlockData) => { return CheckPriceFunction(instance, Function, Params); } },
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
        public static bool IsLogicFunction(string i, out string Func, out string[] Params, Tuple<char, char> functionCasing = null, string ParamSeparator = ",")
        {
            bool Check = IsLogicFunction(i, out Func, out string Param, functionCasing);
            Params = Param.TrimSplit(ParamSeparator);
            return Check;
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
                x.Randomizeditem.OwningPlayer == instance.GetParentContainer().netConnection.PlayerID &&
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
            var CLO = instance.GetCheckableLocationByID(CleanedID, IsLitteral);
            if (CLO is null) { Debug.WriteLine($"{Parameters[0]} is not a randomizable entry"); return false; }
            return CLO.RandomizedState == RandomizedState.Randomized != Inverse;
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

            var CO = instance.GetCheckableLocationByID(paramClean, litteral);
            if (CO is null) { Debug.WriteLine($"{Parameters[0]} is not a valid Checkable Location"); return false; }

            if (Function == "check" && CO.RandomizedState == RandomizedState.ForcedJunk) { Function = "available"; }

            if (CO is MacroObject MO) { return MO.Available != Inverted; }
            else if (Function == "check") { return CO.CheckState == CheckState.Checked != Inverted; }
            else if (Function == "available") { return CO.Available != Inverted; }
            else { Debug.WriteLine($"{paramClean} could not be checked"); }
            return false;
        }

        private static bool CheckOptionFunction(InstanceData.TrackerInstance instance, string Function, string[] Parameters)
        {
            if (Parameters.Length < 1) { return false; } //No Values Pased

            var OptionType = instance.GetLogicOptionByID(Parameters[0]);
            bool Inverse = Parameters.Length > 2 && bool.TryParse(Parameters[2], out bool inverseValue) && !inverseValue;

            if (OptionType is OptionData.ToggleOption TogOpt)
            {
                if (Parameters.Length < 2 || TogOpt.Enabled.ID == Parameters[1]) { return TogOpt.GetValue() == TogOpt.Enabled != Inverse; }
                else if (TogOpt.Disabled.ID == Parameters[1]) { return TogOpt.GetValue() == TogOpt.Disabled != Inverse; }
                else if (TogOpt.Enabled.ID == Parameters[1]) { return TogOpt.GetValue() == TogOpt.Enabled != Inverse; }
                else if (bool.TryParse(Parameters[1], out bool ParamBool))
                {
                    if (ParamBool) { return TogOpt.GetValue() == TogOpt.Enabled != Inverse; }
                    else { return TogOpt.GetValue() == TogOpt.Disabled != Inverse; }
                }
                else { return false; }
            }
            else if (OptionType is OptionData.ChoiceOption ChoiceOpt)
            {
                if (Parameters.Length < 2) { return false; } //Not enough values passed
                if (!ChoiceOpt.ValueList.ContainsKey(Parameters[1])) { Debug.WriteLine($"{Parameters[1]} was not a valid Value for option {ChoiceOpt.ID}"); }
                return ChoiceOpt.GetValue().ID == Parameters[1] != Inverse;
            }
            else if (OptionType is OptionData.MultiSelectOption MultiOpt)
            {
                if (Parameters.Length < 2) { return false; } //Not enough values passed
                if (!MultiOpt.ValueList.ContainsKey(Parameters[1])) { Debug.WriteLine($"{Parameters[1]} was not a valid Value for option {MultiOpt.ID}"); }
                return MultiOpt.EnabledValues.Contains(Parameters[1]) != Inverse;
            }
            else if (OptionType is OptionData.IntOption IntOpt)
            {
                if (Parameters.Length < 2) { return false; } //Not enough values passed
                if (!int.TryParse(Parameters[1], out int ParamInt)) { return false; }
                return IntOpt.Value == ParamInt != Inverse;
            }
            Debug.WriteLine($"Setting {Parameters[0]} was not found in setting list");
            return false;
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

            List<CheckableLocation> ValidEntries = [];
            foreach (var currentOption in OptionList)
            {
                var Location = instance.GetCheckableLocationByID(currentOption, false);
                if (Location is LocationData.LocationObject LO && LO.GetItemAtCheck() is not null)
                {
                    ValidEntries.Add(LO);
                }
                else if (Location is EntranceData.EntranceRandoExit EO && EO.GetDestinationAtExit() is not null)
                {
                    ValidEntries.Add(EO);
                }
            }
            if (ValidEntries.Count == 0) { return false; }
            foreach(var Location in ValidEntries)
            {
                foreach(var Item in ValueList)
                {
                    if (Location is LocationData.LocationObject LO && LO.GetItemAtCheck() == Item) { return !inverse; }
                    else if (Location is EntranceData.EntranceRandoExit EO && EO.LeadsToArea(Item)) {  return !inverse; }
                }
            }
            return inverse;
        }

        private static bool CheckPriceFunction(InstanceData.TrackerInstance instance, string function, string[] Parameters)
        {
            if (Parameters.Length < 2) { return false; } //Not enough Values Pased
            bool inverse = Parameters.Length > 2 && bool.TryParse(Parameters[2], out bool inverseValue) && !inverseValue; //If a third param is passed and its a false bool, invert the result

            bool litteral = Parameters[0].IsLiteralID(out string paramClean);
            var CL = instance.GetCheckableLocationByID(paramClean, litteral);
            if (CL is null) { return false; }
            if (!int.TryParse(Parameters[1], out int Cost)) { return false; }
            int LocationCost = CL.GetPrice().Item1;
            return LocationCost <= Cost != inverse;
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
