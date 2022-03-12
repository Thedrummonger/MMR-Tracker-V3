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

        public static List<List<string>> ConvertLogicStringToConditional(string InLogic)
        {
            //This should probably not happen here.
            InLogic = LogicStringParser.HandleOOTRandoBadEntries(InLogic);

            InLogic = LogicStringParser.ReplaceLogicOperatorsWithMathOperators(InLogic);
            var LogicEntries = LogicStringParser.SplitLogicString(InLogic);
            var PrepedLogic = LogicStringParser.ReplaceEntryWithLetter(LogicEntries, out Dictionary<string, string> ReplacementDict);
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
            string LastChar = "";
            string currentItem = "";
            int InIgnoredComma = 0;
            List<string> BrokenString = new List<string>();
            foreach(var i in LogicString)
            {
                if (InIgnoredComma > 0 || (i == '(' && !LastChar[0].ISLogicChar()))
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

        public static string ReplaceLogicOperatorsWithMathOperators(string Input)
        {
            string[] parts = Input.Split('\'');
            for (int i = 0; i < parts.Length; i += 2)
            {
                foreach (var j in OROperators) { parts[i] = parts[i].Replace(j, " + "); }
                foreach (var j in AndOperators) { parts[i] = parts[i].Replace(j, " * "); }
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

        public static string HandleOOTRandoBadEntries(string InLogic)
        {
            string RebuiltString = "";
            int CommaDepth = 0;
            string CurrentBadActor = "";
            string CurrentReplacementActor = "";
            Dictionary<string, string> Replacements = new Dictionary<string, string>();
            foreach (var i in InLogic)
            {
                RebuiltString += i;
                if (RebuiltString.EndsWith(" at("))
                {
                    CommaDepth++;
                    CurrentBadActor = " at";
                    CurrentReplacementActor = " ";
                }
                if (CommaDepth > 0)
                {
                    CurrentBadActor += i;
                    if (i == ',') { CurrentReplacementActor += " and ("; }
                    else { CurrentReplacementActor += i; }
                    if (i == '(') { CommaDepth++; }
                    if (i == ')') { CommaDepth--; }
                    if (CommaDepth == 0)
                    {
                        CurrentReplacementActor += ")";
                        Replacements.Add(CurrentBadActor, CurrentReplacementActor);
                    }
                }
            }
            foreach(var i in Replacements)
            {
                RebuiltString = RebuiltString.Replace(i.Key, i.Value);
            }
            return RebuiltString;
        }
    }
}
