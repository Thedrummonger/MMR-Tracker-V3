using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
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
