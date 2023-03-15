using MathNet.Symbolics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MMR_Tracker_V3
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
        public static void MoveRequirementsToConditionals(TrackerObjects.MMRData.JsonFormatLogicItem entry)
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
        public static void RemoveRedundantConditionals(TrackerObjects.MMRData.JsonFormatLogicItem entry)
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
            entry.ConditionalItems = TempConditionals;

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
        public static void MakeCommonConditionalsRequirements(TrackerObjects.MMRData.JsonFormatLogicItem entry)
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
    }
    public static class LogicStringConverter
    {
        public static List<List<string>> ConvertLogicStringToConditional(LogicStringParser Parser, string LogicLine)
        {
            var ParsedLogic = Parser.ParseLogicString(LogicLine);
            var MathString = ConvertLogicParserObjectToMathString(ParsedLogic, out Dictionary<string, string> RefMap, out _);
            var ExpandedMathString = ExpandMathExpressionString(MathString);
            var MathLogicArray = ConvertMathStringToArray(ExpandedMathString);
            return RestorelogicValues(MathLogicArray, RefMap);
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
            MathNet.Symbolics.Expression LogicSet = Infix.ParseOrThrow(MathLogic);
            var Output = Algebraic.Expand(LogicSet);
            return Infix.Format(Output).Replace(" ", "");
        }

        private static List<List<string>> ConvertMathStringToArray(string MathString)
        {
            return MathString.Split('+').Select(x => x.Split('*').ToList()).ToList();
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
        private string _ANDOP;
        private string _OROP;
        private char _OpenContainer;
        private char _CloseContainer;
        public LogicStringParser(OperatorType operatorType = OperatorType.CStyle, ContainerType containerType = ContainerType.parentheses)
        {
            Initialize(GetAndOP(operatorType), GetOrOP(operatorType), GetOpenContainer(containerType), GetCloseContainer(containerType));
        }
        public LogicStringParser(string AndOP, string OrOP, ContainerType containerType)
        {
            Initialize(AndOP, OrOP, GetOpenContainer(containerType), GetCloseContainer(containerType));
        }
        public LogicStringParser(string AndOP, string OrOP, char OpenContainer, char CloseContainer)
        {
            Initialize(AndOP, OrOP, OpenContainer, CloseContainer);
        }
        public LogicStringParser(OperatorType operatorType, char OpenContainer, char CloseContainer)
        {
            Initialize(GetAndOP(operatorType), GetOrOP(operatorType), OpenContainer, CloseContainer);
        }

        private void Initialize(string AndOP, string OrOP, char OpenContainer, char CloseContainer)
        {
            _ANDOP = AndOP;
            _OROP = OrOP;
            _OpenContainer = OpenContainer;
            _CloseContainer = CloseContainer;
            Debug.WriteLine($"{_ANDOP} {_OROP} {_OpenContainer} {_CloseContainer}");
        }

        private string GetAndOP(OperatorType type)
        {
            return type switch
            {
                OperatorType.CStyle => "&&",
                OperatorType.PyStyle => "and",
                _ => "&&",
            };
        }

        private string GetOrOP(OperatorType type)
        {
            return type switch
            {
                OperatorType.CStyle => "||",
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
                _ => '(',
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
            PyStyle
        }

        public List<LogicItem> ParseLogicString(string logicString)
        {
            List<LogicItem> ParsedLogic = new();
            string CurrentEntry = "";
            int currentIndex = -1;
            foreach (char c in logicString)
            {
                currentIndex++;
                CurrentEntry += c;
                if (IsEndOfExpression(CurrentEntry, logicString, currentIndex))
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

        private bool IsEndOfExpression(string currentEntry, string LogicString, int CurrentIndex)
        {
            char CurrentChar = LogicString[CurrentIndex];
            //If the expression is a function, return whether or not the function has ended
            //To test this, check if the number of closing Parenthese is equal to the number of open Parenthese
            if (entryIsFunction(currentEntry.Trim())) { return GetFunctionParDepth(currentEntry) == 0; }
            if (currentEntry == $"{_ANDOP} ") { return true; } //Return true if the expression is the AND Operator
            if (currentEntry == $"{_OROP} ") { return true; } //Return true if the expression is the OR Operator
            if (currentEntry == $"{_OpenContainer}") { return true; } //Return true if the expression is a Open Parenthese
            if (currentEntry == $"{_CloseContainer}") { return true; } //Return true if the expression is a Close Parenthese

            //Return true if next character of the string contains an operator or Parenthese
            //Any Operator other than ')' should always be preceded by a space
            if (GetNextChar(LogicString, CurrentIndex, 1) == $"{_CloseContainer}") { return true; }
            if (char.IsWhiteSpace(CurrentChar))
            {
                if (GetNextChar(LogicString, CurrentIndex, 1) == $"{_OpenContainer}") { return true; }
                if (GetNextChar(LogicString, CurrentIndex, _ANDOP.Length) == _ANDOP) { return true; }
                if (GetNextChar(LogicString, CurrentIndex, _OROP.Length) == _OROP) { return true; }
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
            int ParDepth = 0;
            foreach (var i in currentEntry)
            {
                if (i == _OpenContainer) { ParDepth++; }
                if (i == _CloseContainer) { ParDepth--; }
            }
            return ParDepth;
        }

        private char CharRelPos(string line, int CurrenIndex, int move)
        {
            int Target = CurrenIndex + move;
            if (Target < 0 || Target >= line.Length) { return ' '; }
            return line[Target];
        }

        private string GetNextChar(string line, int CurrenIndex, int Count)
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
