using MathNet.Symbolics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public static class LogicStringParser
    {
        public static readonly string[] OROperators = { "||", "|", " or ", " OR ", " Or " };
        public static readonly string[] AndOperators = { "&&", "&", " and ", " AND ", " And " };
        public static readonly string[] StrictOROperators = { "||", "|" };
        public static readonly string[] StrictAndOperators = { "&&", "&"};

        public static List<List<string>> ConvertLogicStringToConditional(string InLogic, bool StrictOperators = false, bool Logging = false)
        {
            //This should probably not happen here.
            if (Logging) { Debug.WriteLine(InLogic); }
            //string InLogic = LogicStringParser.HandleOOTRandoBadEntries(InLogic2, Logging);
            if (Logging) { Debug.WriteLine(InLogic); }
            InLogic = LogicStringParser.ReplaceLogicOperatorsWithMathOperators(InLogic, StrictOperators);
            if (Logging) { Debug.WriteLine(InLogic); }
            var LogicEntries = LogicStringParser.SplitLogicString(InLogic);
            if (Logging) { Debug.WriteLine(string.Join("\n", LogicEntries)); }
            var PrepedLogic = LogicStringParser.ReplaceEntryWithLetter(LogicEntries, out Dictionary<string, string> ReplacementDict);
            if (Logging) { Debug.WriteLine(PrepedLogic); }
            var Conditional = LogicStringParser.ExpandLogicString(PrepedLogic);
            return Conditional.Select(y => y.Select(x => ReplacementDict[x]).ToList()).ToList();
        }

        public static List<List<string>> ExpandLogicString(string InLogic)
        {
            Expression LogicSet = Infix.ParseOrThrow(InLogic);
            var Output = Algebraic.Expand(LogicSet);
            string ExpandedLogic = Infix.Format(Output).Replace(" ", "");
            List<string> ConditionalSets = ExpandedLogic.Split("+").ToList();
            List<List<string>> Conditionals = ConditionalSets
              .Select(x => x.Split("*")
                .Select(y => y.Trim())
                .Select(y => Regex.Replace(y, "[^a-zA-Z]", String.Empty))
                .Where(y => !string.IsNullOrWhiteSpace(y))
                .ToList()
              ).Where(x => x.Any()).ToList();
            return Conditionals;
        }

        public static string ReplaceEntryWithLetter(List<string> BrokenLogic, out Dictionary<string, string> ReplacementMap)
        {
            List<string> ReplacementBrokenLogic = new List<string>();
            Dictionary<string, string> DictionaryStringMap = new Dictionary<string, string>();
            int Index = 1;
            foreach (var i in BrokenLogic)
            {
                if (i.Length == 1 && i[0].ISLogicChar()) { ReplacementBrokenLogic.Add(i); }
                else
                {
                    var Letter = IndexToColumn(Index);
                    ReplacementBrokenLogic.Add(Letter);
                    DictionaryStringMap.Add(Letter, i);
                    Index++;
                }
            }
            ReplacementMap = DictionaryStringMap;
            return string.Join("", ReplacementBrokenLogic);
        }

        public static List<string> SplitLogicString(string LogicString)
        {
            string LastChar = null;
            string currentItem = "";
            int InIgnoredComma = 0;
            List<string> BrokenString = new List<string>();
            foreach(var i in LogicString)
            {
                if (InIgnoredComma > 0 || (i == '(' && LastChar != null && !LastChar[0].ISLogicChar()))
                {
                    currentItem += i.ToString();
                    if (i == ')') { InIgnoredComma--; }
                    if (i == '(') { InIgnoredComma++; }
                    continue;
                }

                if (ISLogicChar(i))
                {
                    if (!string.IsNullOrWhiteSpace(currentItem))
                    {
                        BrokenString.Add(currentItem.Trim());
                        currentItem = "";
                    }
                    BrokenString.Add(i.ToString());
                }
                else { currentItem += i.ToString(); }

                if (!char.IsWhiteSpace(i))
                {
                    LastChar = i.ToString();
                }
            }
            if (!string.IsNullOrWhiteSpace(currentItem)) { BrokenString.Add(currentItem.Trim()); }
            return BrokenString;
        }

        public static string ReplaceLogicOperatorsWithMathOperators(string Input, bool strict)
        {
            string[] parts = Input.Split('\'');
            for (int i = 0; i < parts.Length; i += 2)
            {
                if (strict)
                {
                    foreach (var j in StrictOROperators) { parts[i] = parts[i].Replace(j, " + "); }
                    foreach (var j in StrictAndOperators) { parts[i] = parts[i].Replace(j, " * "); }
                }
                else
                {
                    foreach (var j in OROperators) { parts[i] = parts[i].Replace(j, " + "); }
                    foreach (var j in AndOperators) { parts[i] = parts[i].Replace(j, " * "); }
                }
            }
            return string.Join("'", parts);
        }

        public static bool ISLogicChar(this char i, bool ConsiderNewline = true)
        {
            return i switch
            {
                '+' or '*' or '(' or ')' => true,
                _ => false,
            };
        }

        public static string IndexToColumn(int index)
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
    }

    public static class logicCleaner
    {
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
            NewRequirements.AddRange(ConsistantConditionals);
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
}
