using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public class Utility
    {
        private static string CurrentString = "";
        private static bool CurrentStringIsError = false;
        public static bool FilterSearch(LogicObjects.TrackerInstance Instance, LocationData.LocationObject logic, string searchTerm, string NameToCompare)
        {
            //Since filter search is usually called a large number of times at once, we can cut down on lag by checking first if we've already compared against the given string
            //If we have and that string was a malformed term, skip all subsequent searches until the text changes.
            if (searchTerm != CurrentString)
            {
                CurrentString = searchTerm;
                CurrentStringIsError = false;
            }
            else if (CurrentStringIsError) { return true; }

            if (string.IsNullOrWhiteSpace(searchTerm)) { return true; }

            bool StarredOnly = false;
            char[] GlobalModifiers = new char[] { '^', '*' };
            while (searchTerm.Count() > 0 && GlobalModifiers.Contains(searchTerm[0]))
            {
                if (searchTerm[0] == '*') { StarredOnly = true; }
                searchTerm = searchTerm.Substring(1);
            }
            if (StarredOnly && !logic.Starred) { return false; }
            if (string.IsNullOrWhiteSpace(searchTerm)) { return true; }

            List<string> ExpandedExptression = GetEntries(searchTerm);

            for (var i = 0; i < ExpandedExptression.Count(); i++)
            {
                ExpandedExptression[i] = PerformLogicCheck(ExpandedExptression[i]);
            }

            string Expression = string.Join("", ExpandedExptression);
            //Console.WriteLine($"Expression = {Expression}");
            try
            {
                int Result;
                DataTable dt = new DataTable();
                var Solution = dt.Compute(Expression, "");
                if (!int.TryParse(Solution.ToString(), out Result)) { return true; }
                return Result > 0;
            }
            catch { CurrentStringIsError = true; return true; }

            string PerformLogicCheck(string i)
            {
                if (ISLogicChar(i[0])) { return i; }

                char[] Modifiers = new char[] { '!', '=' };

                bool Inverse = false;
                bool Perfect = false;
                var subterm = i;
                if (subterm == "") { return "1"; }
                while (subterm.Count() > 0 && Modifiers.Contains(subterm[0]))
                {
                    if (subterm[0] == '!') { Inverse = true; }
                    if (subterm[0] == '=') { Perfect = true; }
                    subterm = subterm.Substring(1);
                }
                if (subterm == "") { return "1"; }
                if (string.IsNullOrWhiteSpace(subterm)) { return ""; }

                subterm = subterm.Trim();

                switch (subterm[0])
                {
                    case '_': //Search By Randomized Item
                        if (subterm.Substring(1) == "") { return "1"; }
                        if (logic.Randomizeditem.Item == null) { return "0"; }
                        if (Perfect && logic.Randomizeditem.Item.ToLower() == subterm.Substring(1).ToLower() == Inverse) { return "0"; }
                        else if (!Perfect && logic.Randomizeditem.Item.ToLower().Contains(subterm.Substring(1).ToLower()) == Inverse) { return "0"; }
                        break;
                    case '#'://Search By Location Area
                        if (subterm.Substring(1) == "") { return "1"; }
                        if (logic.GetDictEntry(Instance).Area == null) { return "0"; }
                        if (Perfect && logic.GetDictEntry(Instance).Area.ToLower() == subterm.Substring(1).ToLower() == Inverse) { return "0"; }
                        else if (!Perfect && logic.GetDictEntry(Instance).Area.ToLower().Contains(subterm.Substring(1).ToLower()) == Inverse) { return "0"; }
                        break;
                    case '@'://Search By Item Type
                        if (subterm.Substring(1) == "") { return "1"; }
                        if (logic.GetDictEntry(Instance).ValidItemTypes == null) { return "0"; }
                        if (Perfect && logic.GetDictEntry(Instance).ValidItemTypes.Select(x => x.ToLower()).Contains(subterm.Substring(1).ToLower()) == Inverse) { return "0"; }
                        else if ((!Perfect && logic.GetDictEntry(Instance).ValidItemTypes.Select(x => x.ToLower()).Any(x => x.Contains(subterm.Substring(1).ToLower()))) == Inverse) { return "0"; }
                        break;
                    case '~'://Search By Dictionary Name
                        if (subterm.Substring(1) == "") { return "1"; }
                        if (logic.ID == null) { return "0"; }
                        if (Perfect && logic.ID.ToLower() == subterm.Substring(1).ToLower() == Inverse) { return "0"; }
                        else if ((!Perfect && logic.ID.ToLower().Contains(subterm.Substring(1).ToLower())) == Inverse) { return "0"; }
                        break;
                    case '$'://Search By Original Item Name
                        if (subterm.Substring(1) == "") { return "1"; }
                        if (logic.GetDictEntry(Instance).OriginalItem == null) { return "0"; }
                        if (Perfect && logic.GetDictEntry(Instance).OriginalItem.ToLower() == subterm.Substring(1).ToLower() == Inverse) { return "0"; }
                        else if (!Perfect && logic.GetDictEntry(Instance).OriginalItem.ToLower().Contains(subterm.Substring(1).ToLower()) == Inverse) { return "0"; }
                        break;
                    case '%'://Search By Location Name
                        if (subterm.Substring(1) == "") { return "1"; }
                        if (logic.GetDictEntry(Instance).Name == null) { return "0"; }
                        if (Perfect && logic.GetDictEntry(Instance).Name.ToLower() == subterm.Substring(1).ToLower() == Inverse) { return "0"; }
                        else if (!Perfect && logic.GetDictEntry(Instance).Name.ToLower().Contains(subterm.Substring(1).ToLower()) == Inverse) { return "0"; }
                        break;
                    default: //Search By "NameToCompare" variable
                        if (Perfect && NameToCompare.ToLower() == subterm.ToLower() == Inverse) { return "0"; }
                        else if (!Perfect && (NameToCompare.ToLower().Contains(subterm.ToLower()) == Inverse)) { return "0"; }
                        break;
                }
                return "1";
            }
            List<string> GetEntries(string input)
            {
                List<string> BrokenString = new List<string>();
                string currentItem = "";
                foreach (var i in input)
                {
                    if (ISLogicChar(i))
                    {
                        if (currentItem != "")
                        {
                            BrokenString.Add(currentItem);
                            currentItem = "";
                        }
                        if (i == '&') { BrokenString.Add("*"); }
                        else if (i == '|') { BrokenString.Add("+"); }
                        else { BrokenString.Add(i.ToString()); }
                    }
                    else { currentItem += i.ToString(); }
                }
                if (currentItem != "") { BrokenString.Add(currentItem); }
                return BrokenString;
            }

            bool ISLogicChar(char i)
            {
                switch (i)
                {
                    case '&':
                    case '|':
                    case '+':
                    case '*':
                    case '(':
                    case ')':
                        return true;
                    default:
                        return false;
                }

            }
        }

        public static Dictionary<string, int> GetCategoriesFromFile(LogicObjects.TrackerInstance Instance)
        {
            Dictionary<string, int> Groups = new Dictionary<string, int>();
            if (File.Exists(References.Globalpaths.CategoryTextFile))
            {
                //Groups = File.ReadAllLines(@"Recources\Other Files\Categories.txt")
                //    .Select(x => x.ToLower().Trim()).Distinct()
                //    .Select((value, index) => new { value, index })
                //    .ToDictionary(pair => pair.value, pair => pair.index);

                bool AtGame = true;
                foreach (var i in File.ReadAllLines(References.Globalpaths.CategoryTextFile))
                {
                    var x = i.ToLower().Trim();
                    if (string.IsNullOrWhiteSpace(x) || x.StartsWith("//")) { continue; }
                    if (x.StartsWith("#gamecodestart:"))
                    {
                        AtGame = x.Replace("#gamecodestart:", "").Trim().Split(',')
                            .Select(y => y.Trim()).Contains(Instance.LogicFile.GameCode.ToLower());
                        continue;
                    }
                    if (x.StartsWith("#gamecodeend:")) { AtGame = true; continue; }

                    //Console.WriteLine($"{x} Is Valid {AtGame}");

                    if (!Groups.ContainsKey(x) && AtGame)
                    {
                        Groups.Add(x, Groups.Count());
                    }
                }
                return Groups;
            }
            else { return new Dictionary<string, int>(); }
        }

        public static void DeepCloneLogic(List<string> Requirements, List<List<string>> Conditionals, out List<string> NewRequirements, out List<List<string>> NewConditionals)
        {
            NewRequirements = Requirements.ConvertAll(o => (string)o.Clone());
            NewConditionals = Conditionals.ConvertAll(p => p.ConvertAll(o => (string)o.Clone()));
        }

        public static bool CheckforSpoilerLog(LogicObjects.TrackerInstance logic)
        {
            return logic.LocationPool.Any(x => !string.IsNullOrWhiteSpace(x.Randomizeditem.SpoilerLogGivenItem));
        }

        public static void TimeCodeExecution(Stopwatch stopwatch, string CodeTimed = "", int Action = 0)
        {
            if (Action == 0)
            {
                stopwatch.Start();
            }
            else
            {
                Debug.WriteLine($"{CodeTimed} took {stopwatch.ElapsedMilliseconds} m/s");
                stopwatch.Stop();
                stopwatch.Reset();
                if (Action == 1) { stopwatch.Start(); }
            }
        }

        public static MMRData.JsonFormatLogicItem CreateInaccessableLogic(string ID)
        {
            return new MMRData.JsonFormatLogicItem()
            {
                Id = ID,
                RequiredItems = new List<string> { "false" },
                ConditionalItems = new List<List<string>>(),
                IsTrick = false
            };
        }
    }
    public static class GenericCopier<T>
    {
        public static T DeepCopy(object objectToCopy)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, objectToCopy);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}
