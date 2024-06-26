﻿using MathNet.Symbolics;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TDMUtils;

namespace MMR_Tracker_V3.Logic
{
    public static class LogicUtilities
    {
        public class LogicFunction
        {
            public string Funtion;
            public string Parameters;
            public string ParametersTrimmed
            {
                get
                {
                    if (Parameters.StartsWith("(") && Parameters.EndsWith(")")) { return Parameters[1..(Parameters.Length - 1)]; }
                    else { return Parameters; }
                }
            }
            public override string ToString()
            {
                return Funtion + Parameters;
            }
            public LogicFunction(string func, string parm)
            {
                Funtion = func;
                Parameters = parm;
            }
            public LogicFunction(string LogicLine)
            {
                int paramStart = LogicLine.IndexOf('(');
                Funtion = LogicLine[..paramStart];
                Parameters = LogicLine[paramStart..];
            }
        }
        public static void MoveRequirementsToConditionals(MMRData.JsonFormatLogicItem entry)
        {
            if (!entry.ConditionalItems.Any()) { entry.ConditionalItems = new List<List<string>> { entry.RequiredItems }; }
            else
            {
                var NewConditionals = entry.ConditionalItems.Select(x => x.ToList()).ToList();
                foreach (var i in NewConditionals)
                {
                    i.AddRange(entry.RequiredItems.ToList());
                }
                entry.ConditionalItems = NewConditionals.Select(x => x.ToList()).ToList();
            }
            entry.RequiredItems = new List<string>(); ;
        }
        public static void RemoveRedundantConditionals(MMRData.JsonFormatLogicItem entry)
        {
            var cleanedConditionals = entry.ConditionalItems.Select(x => x.Distinct().ToList()).ToList();

            bool Clear = false;
            while (!Clear)
            {
                var test = cleanedConditionals.Where(i => IsRedundant(i, cleanedConditionals)).ToList();
                if (test.Any())
                {
                    var TempCond = cleanedConditionals;
                    TempCond.Remove(test[0]);
                    cleanedConditionals = TempCond;
                }
                else { Clear = true; }
            }

            List<List<string>> TempConditionals = cleanedConditionals;

            if (TempConditionals.Any(SetContainedFalseBool) && !TempConditionals.All(SetContainedFalseBool))
            {
                TempConditionals.RemoveAll(SetContainedFalseBool);
            }

            if (entry.RequiredItems.Any())
            {
                var NewConditionals = cleanedConditionals;
                foreach (var i in NewConditionals)
                {
                    i.RemoveAll(x => entry.RequiredItems.Contains(x));
                }
                TempConditionals = NewConditionals.ToList();
                TempConditionals.RemoveAll(x => !x.Any());
            }

            foreach (var i in TempConditionals)
            {
                if (i.Any(x => bool.TryParse(x, out bool TestResult) && TestResult == true) && !i.All(x => bool.TryParse(x, out bool TestResult) && TestResult == true))
                {
                    i.RemoveAll(x => bool.TryParse(x, out bool Result) && Result == true);
                }
            }

            TempConditionals.RemoveAll(x => !x.Any());

            entry.ConditionalItems = TempConditionals;

            bool SetContainedFalseBool(IEnumerable<string> set)
            {
                return set.Any(x => bool.TryParse(x, out bool PV) && !PV);
            }

            bool IsRedundant(List<string> FocusedList, List<List<string>> CheckingList)
            {
                foreach (var i in CheckingList)
                {
                    if (!i.Equals(FocusedList) && i.All(j => FocusedList.Contains(j)))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public static void MakeCommonConditionalsRequirements(MMRData.JsonFormatLogicItem entry)
        {
            List<string> ConsistantConditionals =
                entry.ConditionalItems.SelectMany(x => x).Distinct().Where(i => entry.ConditionalItems.All(x => x.Contains(i))).ToList();

            bool changesMade = ConsistantConditionals.Any();

            var NewRequirements = (entry.RequiredItems ?? new List<string>()).ToList();
            NewRequirements.AddRange(ConsistantConditionals.Where(x => !bool.TryParse(x, out bool BoolEntry) || !BoolEntry));
            entry.RequiredItems = NewRequirements.Distinct().ToList();

            var NewConditionals = entry.ConditionalItems.Select(x => x.ToList()).ToList();
            foreach (var i in NewConditionals)
            {
                i.RemoveAll(x => ConsistantConditionals.Contains(x));
            }
            NewConditionals.RemoveAll(x => !x.Any());
            entry.ConditionalItems = NewConditionals;
        }

        public static void SortConditionals(MMRData.JsonFormatLogicItem entry)
        {
            for (var ind = 0; ind < entry.ConditionalItems.Count; ind++)
            {
                entry.ConditionalItems[ind] = [.. entry.ConditionalItems[ind].OrderBy(x => !LogicFunctions.IsLogicFunction(x)).ThenBy(x => x)];
            }
            entry.ConditionalItems = [.. entry.ConditionalItems.OrderBy(x => x.Count).ThenBy(x => string.Join(" ", x))];
        }

        public static MMRData.JsonFormatLogicItem CreateInaccessibleLogic(string ID) => new()
        {
            Id = ID,
            RequiredItems = ["false"],
            ConditionalItems = [],
            IsTrick = false
        };

        public static MMRData.JsonFormatLogicItem CreateLogicEntryFromLogicLine(string ID, LogicStringParser Parser, string LogicLine) => new()
        {
            Id = ID,
            RequiredItems = [],
            ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(Parser, LogicLine, ID),
            IsTrick = false
        };

        public static void TransformLogicItems(MMRData.JsonFormatLogicItem entry, Func<string, string> Transformer)
        {
            List<List<string>> NewConditional = [];
            List<string> NewReq = [];
            foreach(var Cond in entry.ConditionalItems)
            {
                List<string> NewConditionalSet = [];
                foreach (var item in Cond)
                {
                    NewConditionalSet.Add(Transformer(item));
                }
                NewConditional.Add(NewConditionalSet);
            }
            foreach (var Req in entry.RequiredItems)
            {
                NewReq.Add(Transformer(Req));
            }
            entry.RequiredItems = NewReq;
            entry.ConditionalItems = NewConditional;
        }
        /// <summary>
        /// Deep Clones the Requirements and Conditionals from a MMR JsonFormattedLogicObject
        /// </summary>
        /// <param name="Requirements">Source Requirements</param>
        /// <param name="Conditionals">Source Conditionals</param>
        /// <param name="NewRequirements">Cloned Requirements</param>
        /// <param name="NewConditionals">Cloned Conditionals</param>
        public static void DeepCloneLogic(List<string> Requirements, List<List<string>> Conditionals, out List<string> NewRequirements, out List<List<string>> NewConditionals)
        {

            NewRequirements = Requirements.ConvertAll(o => (string)o.Clone());
            NewConditionals = Conditionals.ConvertAll(p => p.ConvertAll(o => (string)o.Clone()));
        }
    }
    public static class LogicStringConverter
    {
        public static List<List<string>> ConvertLogicStringToConditional(LogicStringParser Parser, string LogicLine, string LogicID, bool Invert = false)
        {
            var ParsedLogic = new List<LogicStringParser.LogicItem>();
            var MathString = "";
            var ExpandedMathString = "";
            var MathLogicArray = new List<List<string>>();
            if (string.IsNullOrWhiteSpace(LogicLine)) { return new List<List<string>>(); }
            try
            {
                ParsedLogic = Parser.ParseLogicString(LogicLine);
                MathString = ConvertLogicParserObjectToMathString(ParsedLogic, out Dictionary<string, string> RefMap, out _);
                ExpandedMathString = ExpandMathExpressionString(MathString);
                MathLogicArray = ConvertMathStringToArray(ExpandedMathString, Invert);
                return RestorelogicValues(MathLogicArray, RefMap);
            }
            catch (Exception e)
            {
                MiscUtilities.PrintObjectToConsole(ParsedLogic);
                MiscUtilities.PrintObjectToConsole(MathString);
                MiscUtilities.PrintObjectToConsole(ExpandedMathString);
                MiscUtilities.PrintObjectToConsole(MathLogicArray);
                throw new Exception($"Error Parsing Logic Line for {LogicID}\n{LogicLine}\n{ParsedLogic}\n{MathString}\n{ExpandedMathString}\n{MathLogicArray}\n{e.Message}");
            }
        }

        public static string ConvertConditionalToLogicString(LogicStringParser Parser, List<List<string>> Conditional)
        {
            if (!Conditional.SelectMany(x => x).Any()) { return "(true)"; }
            return $"(({string.Join($") {Parser._OROP} (", Conditional.Select(x => string.Join($" {Parser._ANDOP} ", x)))}))";
        }

        public static List<List<string>> ReParseConditional(LogicStringParser Parser, List<List<string>> Conditional, string LogicID)
        {
            var Stringify = ConvertConditionalToLogicString(Parser, Conditional);
            return ConvertLogicStringToConditional(Parser, Stringify, LogicID);
        }

        private static string ConvertLogicParserObjectToMathString(List<LogicStringParser.LogicItem> Logic, out Dictionary<string, string> Ref, out Dictionary<char, string> OperatorMap)
        {
            int CurrentLetterIndex = 1;
            string Mathstring = "";
            var TextToLetter = new Dictionary<string, string>();
            Ref = new Dictionary<string, string>();
            OperatorMap = new Dictionary<char, string>();
            foreach (var i in Logic)
            {
                if (i.Type == LogicStringParser.EntryType.OpenPar)
                {
                    OperatorMap['('] = i.Text;
                    Mathstring += '(';
                }
                else if (i.Type == LogicStringParser.EntryType.ClosePar)
                {
                    OperatorMap[')'] = i.Text;
                    Mathstring += ')';
                }
                else if (i.Type == LogicStringParser.EntryType.AndOp)
                {
                    OperatorMap['*'] = i.Text;
                    Mathstring += '*';
                }
                else if (i.Type == LogicStringParser.EntryType.OrOp)
                {
                    OperatorMap['+'] = i.Text;
                    Mathstring += '+';
                }
                else
                {
                    if (!TextToLetter.ContainsKey(i.Text))
                    {
                        TextToLetter.Add(i.Text, IndexToLetter(CurrentLetterIndex));
                        Ref.Add(IndexToLetter(CurrentLetterIndex), i.Text);
                        CurrentLetterIndex++;
                    }
                    Mathstring += TextToLetter[i.Text];
                }
            }
            return Mathstring;
        }

        public static string IndexToLetter(int index)
        {
            int ColumnBase = 26;
            int DigitMax = 7; // ceil(log26(Int32.Max))
            string Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (index <= 0)
                throw new IndexOutOfRangeException("index must be a positive number");

            if (index <= ColumnBase)
                return Digits[index - 1].ToString();

            var sb = new StringBuilder().Append(' ', DigitMax);
            var current = index;
            var offset = DigitMax;
            while (current > 0)
            {
                sb[--offset] = Digits[--current % ColumnBase];
                current /= ColumnBase;
            }
            return sb.ToString(offset, DigitMax - offset);
        }

        private static string ExpandMathExpressionString(string MathLogic)
        {
            Expression LogicSet = Infix.ParseOrThrow(MathLogic);
            var Output = Algebraic.Expand(LogicSet);
            return Infix.Format(Output).Replace(" ", "");
        }

        private static List<List<string>> ConvertMathStringToArray(string MathString, bool invert = false)
        {
            if (invert)
            {
                MathString = MathString.Replace("+", "++").Replace("*", "**");
                MathString = MathString.Replace("++", "*").Replace("**", "+");
            }
            var MathArray = MathString.Split('+').Select(x => x.Split('*').ToList()).ToList();
            List<List<string>> CleanedArray = new List<List<string>>();
            foreach (var set in MathArray)
            {
                List<string> CleanedSet = new List<string>();
                foreach (var item in set)
                {
                    if (int.TryParse(item, out _)) { continue; }
                    CleanedSet.Add(item);
                }
                if (CleanedSet.Any()) { CleanedArray.Add(CleanedSet); }
            }
            return CleanedArray;
        }

        private static List<List<string>> RestorelogicValues(List<List<string>> MathLogicArray, Dictionary<string, string> logicObjectMap)
        {
            return MathLogicArray.Select(x => x.Select(y => Convert(y)).ToList()).ToList();
            string Convert(string entry)
            {
                if (entry.Contains('^')) { entry = entry[..entry.IndexOf("^")]; }
                return logicObjectMap[entry];
            }
        }

    }
    public class LogicStringParser
    {
        public string _ANDOP;
        public string _OROP;
        public char _OpenContainer;
        public char _CloseContainer;
        private bool _AllowSpaces;
        private char _Quotes = default;
        public LogicStringParser(OperatorType operatorType = OperatorType.CStyle, ContainerType containerType = ContainerType.parentheses, bool AllowSpaces = true, char quotes = default)
        {
            Initialize(GetAndOP(operatorType), GetOrOP(operatorType), GetOpenContainer(containerType), GetCloseContainer(containerType), AllowSpaces, quotes);
        }
        public LogicStringParser(string AndOP, string OrOP, ContainerType containerType, bool AllowSpaces = true, char quotes = default)
        {
            Initialize(AndOP, OrOP, GetOpenContainer(containerType), GetCloseContainer(containerType), AllowSpaces, quotes);
        }
        public LogicStringParser(string AndOP, string OrOP, char OpenContainer, char CloseContainer, bool AllowSpaces = true, char quotes = default)
        {
            Initialize(AndOP, OrOP, OpenContainer, CloseContainer, AllowSpaces, quotes);
        }
        public LogicStringParser(OperatorType operatorType, char OpenContainer, char CloseContainer, bool AllowSpaces = true, char quotes = default)
        {
            Initialize(GetAndOP(operatorType), GetOrOP(operatorType), OpenContainer, CloseContainer, AllowSpaces, quotes);
        }

        private void Initialize(string AndOP, string OrOP, char OpenContainer, char CloseContainer, bool AllowSpaces, char quotes = default)
        {
            _ANDOP = AndOP;
            _OROP = OrOP;
            _OpenContainer = OpenContainer;
            _CloseContainer = CloseContainer;
            _Quotes = quotes;
            _AllowSpaces = AllowSpaces;
            Debug.WriteLine($"{_ANDOP} {_OROP} {_OpenContainer} {_CloseContainer}");
        }

        private string GetAndOP(OperatorType type)
        {
            return type switch
            {
                OperatorType.CStyle => "&&",
                OperatorType.CStlyeSingle => "&",
                OperatorType.PyStyle => "and",
                _ => "&&",
            };
        }

        private string GetOrOP(OperatorType type)
        {
            return type switch
            {
                OperatorType.CStyle => "||",
                OperatorType.CStlyeSingle => "|",
                OperatorType.PyStyle => "or",
                _ => "||",
            };
        }

        private char GetOpenContainer(ContainerType type)
        {
            return type switch
            {
                ContainerType.parentheses => '(',
                ContainerType.bracket => '[',
                ContainerType.braces => '{',
                _ => '(',
            };
        }

        private char GetCloseContainer(ContainerType type)
        {
            return type switch
            {
                ContainerType.parentheses => ')',
                ContainerType.bracket => ']',
                ContainerType.braces => '}',
                _ => ')',
            };
        }

        public class LogicItem
        {
            public string Text { get; set; }
            public EntryType Type { get; set; }
        }


        public enum EntryType
        {
            LogicItem,
            AndOp,
            OrOp,
            OpenPar,
            ClosePar,
            function,
            none
        }
        public enum ContainerType
        {
            parentheses,
            bracket,
            braces
        }
        public enum OperatorType
        {
            CStyle,
            CStlyeSingle,
            PyStyle
        }

        public List<LogicItem> ParseLogicString(string logicString)
        {
            List<LogicItem> ParsedLogic = new();
            string CurrentEntry = "";
            int currentIndex = -1;
            bool InQuotes = false;
            char QuoteChar = _Quotes == default ? '\'' : _Quotes;
            foreach (char c in logicString)
            {
                currentIndex++;
                CurrentEntry += c;
                if (c == QuoteChar) { InQuotes = !InQuotes; }
                if (IsEndOfExpression(CurrentEntry, logicString, currentIndex, InQuotes))
                {
                    string TrimmedEntry = CurrentEntry.Trim();
                    if (!string.IsNullOrWhiteSpace(TrimmedEntry))
                    {
                        EntryType CurrType = GetEntryType(TrimmedEntry);
                        ParsedLogic.Add(new LogicItem { Text = TrimmedEntry, Type = CurrType });
                    }
                    CurrentEntry = "";
                }
            }
            return ParsedLogic;
        }

        private EntryType GetEntryType(string trimmedEntry)
        {
            if (entryIsFunction(trimmedEntry)) { return EntryType.function; }
            else if (trimmedEntry.Trim() == _ANDOP) { return EntryType.AndOp; }
            else if (trimmedEntry.Trim() == _OROP) { return EntryType.OrOp; }
            else if (trimmedEntry.Trim() == $"{_OpenContainer}") { return EntryType.OpenPar; }
            else if (trimmedEntry.Trim() == $"{_CloseContainer}") { return EntryType.ClosePar; }
            else { return EntryType.LogicItem; }
        }

        private bool IsEndOfExpression(string currentEntry, string LogicString, int CurrentIndex, bool inQuotes)
        {
            if (_Quotes != default && inQuotes) { return false; }
            if (CurrentIndex + 1 >= LogicString.Length) { return true; } //Return true if this is the last char in the line
            //If the expression is a function, return whether or not the function has escaped parenthese
            //To test this, check if the number of closing Parenthese is equal to the number of open Parenthese
            if (entryIsFunction(currentEntry.Trim())) { return GetFunctionParDepth(currentEntry) == 0; }
            if (currentEntry == $"{_ANDOP} ") { return true; } //Return true if the expression is the AND Operator
            if (currentEntry == $"{_OROP} ") { return true; } //Return true if the expression is the OR Operator
            if (currentEntry == $"{_OpenContainer}") { return true; } //Return true if the expression is a Open Parenthese
            if (currentEntry == $"{_CloseContainer}") { return true; } //Return true if the expression is a Close Parenthese

            //Return true if next character of the string contains an operator or Parenthese
            //Any other than ')' should always be preceded by a space and operators should be followed by a space
            if (GetNeighborChars(LogicString, CurrentIndex, 1) == $"{_CloseContainer}") { return true; }
            if (char.IsWhiteSpace(LogicString[CurrentIndex])) //Current char is whitespace
            {
                if (!_AllowSpaces && !inQuotes) { return true; }
                if (GetNeighborChars(LogicString, CurrentIndex, 1) == $"{_OpenContainer}") { return true; }
                if (GetNeighborChars(LogicString, CurrentIndex, _ANDOP.Length + 1) == $"{_ANDOP} ") { return true; }
                if (GetNeighborChars(LogicString, CurrentIndex, _OROP.Length + 1) == $"{_OROP} ") { return true; }
            }
            return false;
        }

        private bool entryIsFunction(string currentEntry)
        {
            int currentIndex = 0;
            foreach (var i in currentEntry)
            {
                //check for an open parenthese immediately preceded by a non whitespace char. This should only happen in a function
                char PrevChar = CharRelPos(currentEntry, currentIndex, -1);
                if (i == _OpenContainer && !char.IsWhiteSpace(PrevChar)) { return true; }
                currentIndex++;
            }
            return false;
        }

        private int GetFunctionParDepth(string currentEntry)
        {
            return currentEntry.Count(x => x == _OpenContainer) - currentEntry.Count(x => x == _CloseContainer);
        }

        private char CharRelPos(string line, int CurrenIndex, int move)
        {
            int Target = CurrenIndex + move;
            if (Target < 0 || Target >= line.Length) { return ' '; }
            return line[Target];
        }

        private string GetNeighborChars(string line, int CurrenIndex, int Count)
        {
            string SubString = "";
            for (var i = 1; i <= Count; i++)
            {
                SubString += CharRelPos(line, CurrenIndex, i);
            }
            return SubString;
        }

    }
}
