using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.Logic
{
    public class LogicFunctions
    {
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
            return true;
        }

        public static bool CheckLogicFunction(TrackerInstance instance, string i, List<string> SubUnlockData, out bool LogicFuntionValid, bool DoCheck = true)
        {
            LogicFuntionValid = false;

            if (!IsLogicFunction(i, out string Func, out string Param)) { return false; }

            switch (Func)
            {
                case "check":
                case "available":
                    if (!DoCheck) { return true; }
                    LogicFuntionValid = CheckAvailableFunction(instance, Func, Param);
                    break;
                case "contains":
                    if (!DoCheck) { return true; }
                    LogicFuntionValid = CheckContainsFunction(instance, Param.Split(",").Select(x => x.Trim()).ToArray());
                    break;
                case "renewable":
                    if (!DoCheck) { return true; }
                    LogicFuntionValid = CheckRenewableFunction(instance, Param.Split(",").Select(x => x.Trim()).ToArray());
                    break;
                case "rand":
                case "randomized":
                    if (!DoCheck) { return true; }
                    LogicFuntionValid = CheckIsRandomizedFunction(instance, Param.Split(",").Select(x => x.Trim()).ToArray());
                    break;
                case "trick":
                    if (!DoCheck) { return true; }
                    LogicFuntionValid = CheckTrickEnabledFunction(instance, Param.Split(",").Select(x => x.Trim()).ToArray());
                    break;
                case "setting":
                case "option":
                    if (!DoCheck) { return true; }
                    LogicFuntionValid = CheckOptionFunction(instance, Param.Split(",").Select(x => x.Trim()).ToArray());
                    break;
                case "comment":
                case "cmnt":
                case "cc":
                    LogicFuntionValid = true; //Comment Style Check. Will always return true but can state why its returning true and what it's expecting
                    break;
                case "time":
                    LogicFuntionValid = true; //The tracker currently doesn't track time, but this can at least display the data for logic 
                    break;
                default:
                    return false;
            }

            return true;
        }

        private static bool CheckRenewableFunction(TrackerInstance instance, string[] strings)
        {
            var ShouldBeRepeatable = true;
            if (strings.Length > 1 && bool.TryParse(strings[1], out bool Mod))
            {
                ShouldBeRepeatable = Mod;
            }

            var item = instance.GetItemByID(strings[0]);
            if (item is null) { Debug.WriteLine($"{strings[0]} Was not a valid Renewable Item Entry"); return false; }
            if (item.GetTotalUsable() < 1) { return false; }
            return instance.LocationPool.Values.Any(x =>
                x.CheckState == CheckState.Checked &&
                IsAtProperCheck(x, ShouldBeRepeatable) &&
                x.Randomizeditem.OwningPlayer == -1 &&
                x.Randomizeditem.Item == item.ID);
            bool IsAtProperCheck(LocationObject x, bool ShouldBeRepeatable)
            {
                if (ShouldBeRepeatable) { return x.GetDictEntry().Repeatable is not null && (bool)x.GetDictEntry().Repeatable; }
                else { return x.GetDictEntry().Repeatable is not null && !(bool)x.GetDictEntry().Repeatable; }
            }
        }

        private static bool CheckIsRandomizedFunction(TrackerInstance instance, string[] strings)
        {
            bool Inverse = strings.Length > 1 && bool.TryParse(strings[1], out bool InverseParam) && !InverseParam;
            bool IsLitteral = strings[0].IsLiteralID(out string CleanedID);
            instance.GetLocationEntryType(CleanedID, IsLitteral, out dynamic OBJ);
            if (OBJ is null) { Debug.WriteLine($"{strings[0]} is not a valid logic Entry"); return false; }
            if (!Utility.DynamicPropertyExist(OBJ, "RandomizedState")) { Debug.WriteLine($"{strings[0]} is not a randomizable entry"); return false; }
            RandomizedState randomizedState = OBJ.RandomizedState;
            return randomizedState == RandomizedState.Randomized != Inverse;
        }

        private static bool CheckTrickEnabledFunction(TrackerInstance instance, string[] strings)
        {
            bool Inverse = strings.Length > 1 && bool.TryParse(strings[1], out bool InverseParam) && !InverseParam;
            if (!instance.MacroPool.ContainsKey(strings[0])) { Debug.WriteLine($"{strings[0]} Was not a valid Trick Macro"); return false; }
            if (!instance.MacroPool[strings[0]].isTrick()) { Debug.WriteLine($"{strings[0]} Was a valid macro but was not a trick"); return false; }
            return instance.MacroPool[strings[0]].TrickEnabled != Inverse;
        }

        private static bool CheckAvailableFunction(TrackerInstance instance, string func, string paramString)
        {
            string[] Params = paramString.Split(',').Select(x => x.Trim()).ToArray();
            bool Inverted = Params.Length > 1 && bool.TryParse(Params[1], out bool IsInverted) && !IsInverted;
            bool litteral = Params[0].IsLiteralID(out string paramClean);

            var type = instance.GetLocationEntryType(paramClean, litteral, out dynamic obj);
            if (obj is null) { Debug.WriteLine($"{Params[0]} is not a valid logic Entry"); return false; }

            if (func == "check" && Utility.DynamicPropertyExist(obj, "RandomizedState") && obj.RandomizedState == RandomizedState.ForcedJunk) { func = "available"; }

            if (func == "check" && Utility.DynamicPropertyExist(obj, "CheckState")) { return obj.CheckState == CheckState.Checked != Inverted; }
            else if (func == "available" && Utility.DynamicPropertyExist(obj, "Available")) { return obj.Available != Inverted; }
            else if (func == "available" && Utility.DynamicPropertyExist(obj, "Aquired")) { return obj.Aquired != Inverted; }
            else { Debug.WriteLine($"{paramClean} could not be checked. Type: {type}"); }
            return false;
        }

        public static bool CheckOptionFunction(TrackerInstance instance, string[] param)
        {
            if (param.Length < 1) { return false; } //No Values Pased

            var OptionType = instance.GetLocationEntryType(param[0], false, out _);
            bool Inverse = param.Length > 2 && bool.TryParse(param[2], out bool inverseValue) && !inverseValue;

            if (OptionType == LogicEntryType.ToggleOption)
            {
                var TogOpt = instance.ToggleOptions[param[0]];
                if (param.Length < 2 || TogOpt.Enabled.ID == param[1]) { return TogOpt.GetValue() == TogOpt.Enabled != Inverse; }
                else if (TogOpt.Disabled.ID == param[1]) { return TogOpt.GetValue() == TogOpt.Disabled != Inverse; }
                else if (bool.TryParse(param[1], out bool ParamBool))
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
                var ChoiceOpt = instance.ChoiceOptions[param[0]];
                if (param.Length < 2) { return false; } //Not enough values passed
                if (!ChoiceOpt.ValueList.ContainsKey(param[1])) { Debug.WriteLine($"{param[1]} was not a valid Value for option {ChoiceOpt.ID}"); }
                return ChoiceOpt.GetValue().ID == param[1] != Inverse;
            }
            else if (OptionType == LogicEntryType.MultiSelectOption)
            {
                var ChoiceOpt = instance.MultiSelectOptions[param[0]];
                if (param.Length < 2) { return false; } //Not enough values passed
                if (!ChoiceOpt.ValueList.ContainsKey(param[1])) { Debug.WriteLine($"{param[1]} was not a valid Value for option {ChoiceOpt.ID}"); }
                return ChoiceOpt.EnabledValues.Contains(param[1]) != Inverse;
            }
            else if (OptionType == LogicEntryType.IntOption)
            {
                var IntOpt = instance.IntOptions[param[0]];
                if (param.Length < 2) { return false; } //Not enough values passed
                if (!int.TryParse(param[1], out int ParamInt)) { return false; }
                return IntOpt.Value == ParamInt != Inverse;
            }
            else
            {
                return false;
            }
        }

        private static bool CheckContainsFunction(TrackerInstance instance, string[] param)
        {
            if (param.Length < 2) { return false; } //Not enough Values Pased

            string FunctionValue = param[0];
            string ParamValue = param[1];
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

            bool inverse = param.Length > 2 && bool.TryParse(param[2], out bool inverseValue) && !inverseValue; //If a third param is passed and its a false bool, invert the result

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
