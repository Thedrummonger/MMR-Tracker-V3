using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
            if (string.IsNullOrWhiteSpace(LogicLine)) { return []; }
            try
            {
                List<LogicStringParser.LogicItem> items = Parser.ParseLogicString(LogicLine);
                IBoolExpr ast = Parse(items);
                IBoolExpr dnfAst = ast.ToDNF();
                List<List<string>> clauses = FlattenDnf(dnfAst);
                return clauses;
            }
            catch (Exception e)
            {
                throw new Exception($"Error Parsing Logic Line for {LogicID}\n{LogicLine}\n{e.Message}");
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

        private interface IBoolExpr { }

        private class VarExpr(string name) : IBoolExpr
        {
            public string Name { get; } = name;
        }

        private class AndExpr(IBoolExpr left, IBoolExpr right) : IBoolExpr
        {
            public IBoolExpr Left { get; } = left;
            public IBoolExpr Right { get; } = right;
        }

        private class OrExpr(IBoolExpr left, IBoolExpr right) : IBoolExpr
        {
            public IBoolExpr Left { get; } = left;
            public IBoolExpr Right { get; } = right;
        }

        private static IBoolExpr Parse(List<LogicStringParser.LogicItem> items)
        {
            int index = 0;
            return ParseOrExpr(items, ref index);
        }

        // Grammar with precedence:
        // OR-level:   ParseOrExpr -> ParseAndExpr { OR_OP ParseAndExpr }
        // AND-level:  ParseAndExpr -> ParsePrimary { AND_OP ParsePrimary }
        // Primary:    variable | "(" ParseOrExpr ")"

        private static IBoolExpr ParseOrExpr(List<LogicStringParser.LogicItem> items, ref int index)
        {
            IBoolExpr left = ParseAndExpr(items, ref index);

            while (index < items.Count && items[index].Type == LogicStringParser.EntryType.OrOp)
            {
                index++; // skip the OR token
                IBoolExpr right = ParseAndExpr(items, ref index);
                left = new OrExpr(left, right);
            }
            return left;
        }

        private static IBoolExpr ParseAndExpr(List<LogicStringParser.LogicItem> items, ref int index)
        {
            IBoolExpr left = ParsePrimary(items, ref index);

            while (index < items.Count && items[index].Type == LogicStringParser.EntryType.AndOp)
            {
                index++; // skip the AND token
                IBoolExpr right = ParsePrimary(items, ref index);
                left = new AndExpr(left, right);
            }
            return left;
        }

        private static IBoolExpr ParsePrimary(List<LogicStringParser.LogicItem> items, ref int index)
        {
            if (index >= items.Count)
            {
                throw new Exception("Unexpected end of tokens while parsing.");
            }

            var token = items[index];

            if (token.Type == LogicStringParser.EntryType.OpenPar)
            {
                index++; // skip '('
                IBoolExpr expr = ParseOrExpr(items, ref index);

                if (index >= items.Count || items[index].Type != LogicStringParser.EntryType.ClosePar)
                {
                    throw new Exception("Missing closing parenthesis");
                }

                index++; // skip ')'
                return expr;
            }
            else
            {
                // It's a variable, e.g. "Item1"
                index++;
                return new VarExpr(token.Text);
            }
        }

        private static IBoolExpr ToDNF(this IBoolExpr expr)
        {
            switch (expr)
            {
                case VarExpr v:
                    // Single variable => already simplest form
                    return v;

                case AndExpr andExpr:
                    // Convert children first
                    var leftAnd = andExpr.Left.ToDNF();
                    var rightAnd = andExpr.Right.ToDNF();
                    return DistributeAnd(leftAnd, rightAnd);

                case OrExpr orExpr:
                    // Convert children first
                    var leftOr = orExpr.Left.ToDNF();
                    var rightOr = orExpr.Right.ToDNF();
                    return new OrExpr(leftOr, rightOr);

                default:
                    throw new InvalidOperationException("Unknown expression type in DNF converter");
            }
        }
        private static IBoolExpr DistributeAnd(IBoolExpr left, IBoolExpr right)
        {
            if (left is OrExpr leftOr)
            {
                var leftDistributed = DistributeAnd(leftOr.Left, right);
                var rightDistributed = DistributeAnd(leftOr.Right, right);
                return new OrExpr(leftDistributed, rightDistributed);
            }
            else if (right is OrExpr rightOr)
            {
                var leftDistributed = DistributeAnd(left, rightOr.Left);
                var rightDistributed = DistributeAnd(left, rightOr.Right);
                return new OrExpr(leftDistributed, rightDistributed);
            }
            else
            {
                // Both sides are not OR => just And them
                return new AndExpr(left, right);
            }
        }

        private static List<List<string>> FlattenDnf(IBoolExpr expr)
        {
            switch (expr)
            {
                case VarExpr v:
                    // Single variable => single clause with one item
                    return [[v.Name]];

                case AndExpr andExpr:
                    // Cross product of left and right
                    var leftAndClauses = FlattenDnf(andExpr.Left);
                    var rightAndClauses = FlattenDnf(andExpr.Right);

                    var result = new List<List<string>>();
                    foreach (var lc in leftAndClauses)
                    {
                        foreach (var rc in rightAndClauses)
                        {
                            // Combine the two sub-lists
                            var combined = new List<string>(lc);
                            combined.AddRange(rc);
                            result.Add(combined);
                        }
                    }
                    return result;

                case OrExpr orExpr:
                    // Union of left and right
                    var leftOrClauses = FlattenDnf(orExpr.Left);
                    var rightOrClauses = FlattenDnf(orExpr.Right);
                    leftOrClauses.AddRange(rightOrClauses);
                    return leftOrClauses;

                default:
                    throw new InvalidOperationException("Unknown expression type in FlattenDnf");
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
