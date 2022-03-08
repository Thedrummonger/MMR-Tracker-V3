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
        public static Dictionary<string, int> GetCategoriesFromFile(LogicObjects.TrackerInstance Instance)
        {
            Dictionary<string, int> Groups = new();
            if (File.Exists(References.Globalpaths.CategoryTextFile))
            {
                bool AtGame = true;
                foreach (var i in File.ReadAllLines(References.Globalpaths.CategoryTextFile))
                {
                    var Line = i.ToLower().Trim();
                    if (string.IsNullOrWhiteSpace(Line) || Line.StartsWith("//")) { continue; }
                    if (Line.StartsWith("#gamecodestart:"))
                    {
                        AtGame = Line.Replace("#gamecodestart:", "").Trim().Split(',')
                            .Select(y => y.Trim()).Contains(Instance.LogicFile.GameCode.ToLower());
                        continue;
                    }
                    if (Line.StartsWith("#gamecodeend:")) { AtGame = true; continue; }

                    if (!Groups.ContainsKey(Line) && AtGame)
                    {
                        Groups.Add(Line, Groups.Count);
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
            return logic.LocationPool.Values.Any(x => !string.IsNullOrWhiteSpace(x.Randomizeditem.SpoilerLogGivenItem));
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

        public static string GetDisplayName(int Function, LocationData.LocationObject i, LogicObjects.TrackerInstance instance)
        {
            string Displayname = "";
            if (Function == 0) //Available Locations
            { 
                Displayname = i.GetDictEntry(instance).Name ?? i.ID;
                if (i.CheckState == MiscData.CheckState.Marked)
                {
                    string RandomizedItemDisplay = i.Randomizeditem.Item;
                    var RandomizedItem = instance.GetItemByID(i.Randomizeditem.Item);
                    if (RandomizedItem != null) { RandomizedItemDisplay = RandomizedItem.GetDictEntry(instance).Name ?? RandomizedItem.Id; }
                    Displayname += $": {RandomizedItemDisplay}";
                }
            }
            else if (Function == 1) //Checked Locations
            {
                Displayname = i.Randomizeditem.Item;
                var RandomizedItem = instance.GetItemByID(i.Randomizeditem.Item);
                if (RandomizedItem != null) { Displayname = RandomizedItem.GetDictEntry(instance).Name ?? RandomizedItem.Id; }
                Displayname = $"{Displayname}: {i.GetDictEntry(instance).Name ?? i.ID}";
            }
            return Displayname;
        }

        public static List<int> ParseLocationAndJunkSettingString(string c, int ItemCount, string ParseType)
        {
            var result = new List<int>();
            if (string.IsNullOrWhiteSpace(c))
            {
                return result;
            }

            result.Clear();
            string[] Sections = c.Split('-');
            int[] NewSections = new int[ItemCount];
            if (Sections.Length != NewSections.Length) 
            { 
                Debug.WriteLine($"{ParseType} String Didin't match {Sections.Length}, {NewSections.Length}"); 
                return null; 
            }

            try
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    if (Sections[(ItemCount - 1) - i] != "") { NewSections[i] = Convert.ToInt32(Sections[(ItemCount - 1) - i], 16); }
                }
                for (int i = 0; i < 32 * ItemCount; i++)
                {
                    int j = i / 32;
                    int k = i % 32;
                    if (((NewSections[j] >> k) & 1) > 0) { result.Add(i); }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"It broke {e.Message}");
                return null;
            }
            return result;
        }

        public static List<string> GetAllWalletLogicEntries(LogicObjects.TrackerInstance instance)
        {
            bool CurrentInitializedState = instance.PriceData.Initialized;
            instance.PriceData.Initialized = false;

            List<string> WalletEntries = new List<string>();
            foreach (var i in instance.ItemPool)
            {
                if (i.Value.GetDictEntry(instance).WalletCapacity != null)
                {
                    WalletEntries.Add(i.Key);
                }
            }

            while (true)
            {
                if (!ScanMacros()) { break; }
            }

            instance.PriceData.Initialized = CurrentInitializedState;

            return WalletEntries;

            bool ScanMacros()
            {
                bool NewWalletFound = false;
                foreach (var i in instance.MacroPool)
                {
                    if (WalletEntries.Contains(i.Key)) { continue; }
                    var Logic = instance.GetLogic(i.Key);
                    if (IsWalletmacro(Logic))
                    {
                        WalletEntries.Add(i.Key);
                        NewWalletFound = true;
                    }
                }
                return NewWalletFound;
            }

            bool IsWalletmacro(MMRData.JsonFormatLogicItem Logic)
            {
                if (!Logic.RequiredItems.Any() && !Logic.ConditionalItems.Any()) { return false; }
                foreach (var i in Logic.RequiredItems)
                {
                    if (!WalletEntries.Contains(i)) { return false; }
                }
                foreach (var cond in Logic.ConditionalItems)
                {
                    foreach (var i in cond)
                    {
                        if (!WalletEntries.Contains(i)) { return false; }
                    }
                }
                return true;
            }

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

    public static class SearchStringParser
    {
        private static string CurrentString = "";
        private static bool CurrentStringIsError = false;

        public static bool ISLogicChar(char i)
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

        public static List<string> GetEntriesFromLogicString(string input)
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

            List<string> ExpandedExptression = GetEntriesFromLogicString(searchTerm);

            for (var i = 0; i < ExpandedExptression.Count(); i++)
            {
                ExpandedExptression[i] = PerformLogicCheck(ExpandedExptression[i], Instance, logic, NameToCompare);
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
        }

        private static string PerformLogicCheck(string i, LogicObjects.TrackerInstance Instance, LocationData.LocationObject logic, string NameToCompare)
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
    }
}
