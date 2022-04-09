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

        public static string GetDisplayName(int Function, object obj, LogicObjects.TrackerInstance instance)
        {
            string Displayname = "";
            if (obj is LocationData.LocationObject i)
            {
                if (Function == 0) //Available Locations
                {
                    Displayname = i.GetDictEntry(instance).Name ?? i.ID;
                    if (i.CheckState == MiscData.CheckState.Marked)
                    {
                        string RandomizedItemDisplay = i.Randomizeditem.Item;
                        var RandomizedItem = instance.GetItemByID(i.Randomizeditem.Item);
                        if (RandomizedItem != null) { RandomizedItemDisplay = RandomizedItem.GetDictEntry(instance).GetItemName(instance) ?? RandomizedItem.Id; }
                        Displayname += $": {RandomizedItemDisplay}";
                        if (i.Price > -1) { Displayname += $" [${i.Price}]"; }
                    }
                }
                else if (Function == 1) //Checked Locations
                {
                    Displayname = i.Randomizeditem.Item;
                    var RandomizedItem = instance.GetItemByID(i.Randomizeditem.Item);
                    if (RandomizedItem != null) { Displayname = RandomizedItem.GetDictEntry(instance).GetItemName(instance) ?? RandomizedItem.Id; }
                    Displayname = $"{Displayname}: {i.GetDictEntry(instance).Name ?? i.ID}";
                    if (i.Price > -1) { Displayname += $" [${i.Price}]"; }
                }
                return Displayname;
            }
            else if (obj is LocationData.LocationProxy p)
            {
                var refLoc = instance.LocationPool[p.ReferenceID];
                if (Function == 0) //Available Locations
                {
                    Displayname = p.Name??p.ID;
                    if (refLoc.CheckState == MiscData.CheckState.Marked)
                    {
                        string RandomizedItemDisplay = refLoc.Randomizeditem.Item;
                        var RandomizedItem = instance.GetItemByID(refLoc.Randomizeditem.Item);
                        if (RandomizedItem != null) { RandomizedItemDisplay = RandomizedItem.GetDictEntry(instance).GetItemName(instance) ?? RandomizedItem.Id; }
                        Displayname += $": {RandomizedItemDisplay}";
                        if (refLoc.Price > -1) { Displayname += $" [${refLoc.Price}]"; }
                        else if (p.LogicInheritance != null && instance.MacroPool.ContainsKey(p.LogicInheritance) && instance.MacroPool[p.LogicInheritance].Price > -1) 
                        { Displayname += $" [${instance.MacroPool[p.LogicInheritance].Price}]"; }
                    }
                }
                return Displayname;
            }
            else { return obj.ToString(); }
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

        public class SearchObject
        {
            public bool Starred { get; set; } = false;
            public string ID { get; set; } = "";
            public string Name { get; set; } = "";
            public string OriginalItem { get; set; } = "";
            public string Randomizeditem { get; set; } = "";
            public string Area { get; set; } = "";
            public string[] ValidItemTypes { get; set; } = null;
        }

        public static readonly char[] LogicChars = new char[] { '&', '|', '+', '*', '(', ')' };

        public static List<string> GetEntriesFromLogicString(string input)
        {
            List<string> BrokenString = new List<string>();
            string currentItem = "";
            foreach (var i in input)
            {
                if (LogicChars.Contains(i))
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

        public static bool FilterSearch(LogicObjects.TrackerInstance Instance, object InObject, string searchTerm, string NameToCompare)
        {
            var searchObject = CreateSearchableObject(InObject, Instance);
            if (searchObject == null) { return false; }
            //Debug.WriteLine($"{searchObject.ID}: {InObject.GetType()}" );

            //Since filter search is usually called a large number of times at once, we can cut down on lag by checking first if we've already compared against the given string
            //If we have and that string was a malformed term, skip all subsequent searches until the text changes.
            if (searchTerm != CurrentString)
            {
                CurrentString = searchTerm;
                CurrentStringIsError = false;
            }
            else if (CurrentStringIsError) { return false; ; }

            if (string.IsNullOrWhiteSpace(searchTerm)) { return true; }

            bool StarredOnly = false;
            char[] GlobalModifiers = new char[] { '^', '*' };
            while (searchTerm.Count() > 0 && GlobalModifiers.Contains(searchTerm[0]))
            {
                if (searchTerm[0] == '*') { StarredOnly = true; }
                searchTerm = searchTerm.Substring(1);
            }
            if (StarredOnly && !searchObject.Starred) { return false; }
            if (string.IsNullOrWhiteSpace(searchTerm)) { return true; }

            List<string> ExpandedExptression = GetEntriesFromLogicString(searchTerm);

            for (var i = 0; i < ExpandedExptression.Count; i++)
            {
                ExpandedExptression[i] = PerformLogicCheck(ExpandedExptression[i], searchObject, NameToCompare);
            }

            string Expression = string.Join("", ExpandedExptression);
            //Console.WriteLine($"Expression = {Expression}");
            try
            {
                DataTable dt = new();
                var Solution = dt.Compute(Expression, "");
                if (!int.TryParse(Solution.ToString(), out int Result)) { return true; }
                return Result > 0;
            }
            catch { CurrentStringIsError = true; return true; }
        }

        private static string PerformLogicCheck(string i, SearchObject logic, string NameToCompare)
        {
            if (LogicChars.Contains(i[0])) { return i; }

            char[] Modifiers = new char[] { '!', '=' };

            bool Inverse = false;
            bool Perfect = false;
            var subterm = i;
            if (subterm == "") { return "1"; }
            while (subterm.Length > 0 && Modifiers.Contains(subterm[0]))
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
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.Randomizeditem == null) { return "0"; }
                    if (Perfect && logic.Randomizeditem.ToLower() == subterm[1..].ToLower() == Inverse) { return "0"; }
                    else if (!Perfect && logic.Randomizeditem.ToLower().Contains(subterm[1..].ToLower()) == Inverse) { return "0"; }
                    break;
                case '#'://Search By Location Area
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.Area == null) { return "0"; }
                    if (Perfect && logic.Area.ToLower() == subterm[1..].ToLower() == Inverse) { return "0"; }
                    else if (!Perfect && logic.Area.ToLower().Contains(subterm[1..].ToLower()) == Inverse) { return "0"; }
                    break;
                case '@'://Search By Item Type
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.ValidItemTypes == null) { return "0"; }
                    if (Perfect && logic.ValidItemTypes.Select(x => x.ToLower()).Contains(subterm[1..].ToLower()) == Inverse) { return "0"; }
                    else if ((!Perfect && logic.ValidItemTypes.Select(x => x.ToLower()).Any(x => x.Contains(subterm[1..].ToLower()))) == Inverse) { return "0"; }
                    break;
                case '~'://Search By Dictionary Name
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.ID == null) { return "0"; }
                    if (Perfect && logic.ID.ToLower() == subterm[1..].ToLower() == Inverse) { return "0"; }
                    else if ((!Perfect && logic.ID.ToLower().Contains(subterm[1..].ToLower())) == Inverse) { return "0"; }
                    break;
                case '$'://Search By Original Item Name
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.OriginalItem == null) { return "0"; }
                    if (Perfect && logic.OriginalItem.ToLower() == subterm[1..].ToLower() == Inverse) { return "0"; }
                    else if (!Perfect && logic.OriginalItem.ToLower().Contains(subterm[1..].ToLower()) == Inverse) { return "0"; }
                    break;
                case '%'://Search By Location Name
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.Name == null) { return "0"; }
                    if (Perfect && logic.Name.ToLower() == subterm[1..].ToLower() == Inverse) { return "0"; }
                    else if (!Perfect && logic.Name.ToLower().Contains(subterm[1..].ToLower()) == Inverse) { return "0"; }
                    break;
                default: //Search By "NameToCompare" variable
                    if (Perfect && NameToCompare.ToLower() == subterm.ToLower() == Inverse) { return "0"; }
                    else if (!Perfect && (NameToCompare.ToLower().Contains(subterm.ToLower()) == Inverse)) { return "0"; }
                    break;
            }
            return "1";
        }

        public static SearchObject CreateSearchableObject(object Object, LogicObjects.TrackerInstance instance)
        {
            SearchObject OutObject = new();
            if (Object is LocationData.LocationObject locationObject)
            {
                var DictData = locationObject.GetDictEntry(instance);
                OutObject.ID = locationObject.ID;
                OutObject.Area = DictData.Area;
                OutObject.Name = DictData.Name;
                OutObject.OriginalItem = DictData.OriginalItem;
                OutObject.Randomizeditem = locationObject.Randomizeditem.Item;
                OutObject.Starred = locationObject.Starred;
                OutObject.ValidItemTypes = DictData.ValidItemTypes;
            }
            else if (Object is LocationData.LocationProxy locationProxy)
            {
                var LocReference = instance.LocationPool[locationProxy.ReferenceID];
                var DictData = LocReference.GetDictEntry(instance);
                OutObject.ID = locationProxy.ID;
                OutObject.Area = locationProxy.Area;
                OutObject.Name = locationProxy.Name;
                OutObject.OriginalItem = DictData.OriginalItem;
                OutObject.Randomizeditem = LocReference.Randomizeditem.Item;
                OutObject.Starred = LocReference.Starred;
                OutObject.ValidItemTypes = DictData.ValidItemTypes;
            }
            else if (Object is ItemData.ItemObject ItemObject)
            {
                var DictData = ItemObject.GetDictEntry(instance);
                OutObject.ID = ItemObject.Id;
                OutObject.Area = "";
                if (ItemObject.AmountInStartingpool > 0) { OutObject.Area += "starting "; }
                if (ItemObject.AmountAquiredOnline.Any(x => x.Value > 0)) { OutObject.Area += "online "; }
                OutObject.Area += "item";
                OutObject.Name = DictData.Name;
                OutObject.OriginalItem = DictData.Name;
                OutObject.Randomizeditem = DictData.Name;
                OutObject.Starred = false;
                OutObject.ValidItemTypes = DictData.ItemTypes;
            }
            else if (Object is HintData.HintObject HintObject)
            {
                var DictData = HintObject.GetDictEntry(instance);
                OutObject.ID = HintObject.ID;
                OutObject.Area = "hints";
                OutObject.Name = DictData.Name;
                OutObject.OriginalItem = DictData.Name;
                OutObject.Randomizeditem = DictData.Name;
                OutObject.Starred = false;
                OutObject.ValidItemTypes = new string[] { "hint" };
            }
            else if (Object is MacroObject MacroObject)
            {
                bool Istrick = MacroObject.isTrick(instance);
                var DictData = MacroObject.GetDictEntry(instance);
                var LogicData = instance.GetLogic(MacroObject.ID, false);
                OutObject.ID = MacroObject.ID;
                OutObject.Area = Istrick ? (LogicData.TrickCategory??"misc") : "macro";
                OutObject.Name = DictData.Name ?? MacroObject.ID;
                OutObject.OriginalItem = MacroObject.ID;
                OutObject.Randomizeditem = MacroObject.ID;
                OutObject.Starred = Istrick;
                List<string> ItemTypes = new() { "macro" };
                if (Istrick) { ItemTypes.Add("trick"); }
                OutObject.ValidItemTypes = ItemTypes.ToArray();
            }
            else if (Object is EntranceData.EntranceRandoExit ExitObject)
            {
                OutObject.ID = instance.EntrancePool.GetLogicNameFromExit(ExitObject);
                OutObject.Area = ExitObject.ParentAreaID;
                OutObject.Name = ExitObject.ID;
                OutObject.OriginalItem = ExitObject.EntrancePair == null ? "One Way" : $"{ExitObject.EntrancePair.Area} To {ExitObject.EntrancePair.Exit}";
                OutObject.Randomizeditem = ExitObject.DestinationExit == null ? null : $"{ExitObject.DestinationExit.region} From {ExitObject.DestinationExit.from}";
                OutObject.Starred = ExitObject.Starred;
                List<string> ItemTypes = new() { "exit" };
                if (ExitObject.EntrancePair == null) { ItemTypes.Add("One Way"); }
                OutObject.ValidItemTypes = ItemTypes.ToArray();
            }
            else if (Object is OptionData.TrackerOption OptionObject)
            {
                OutObject.ID = OptionObject.ID;
                OutObject.Area = "Options";
                OutObject.Name = OptionObject.DisplayName;
                OutObject.OriginalItem = OptionObject.CurrentValue;
                OutObject.Randomizeditem = OptionObject.CurrentValue;
                OutObject.Starred = OptionObject.IsToggleOption();
                List<string> ItemTypes = new() { "Option" };
                if (OptionObject.IsToggleOption()) { ItemTypes.Add("Toggle"); }
                OutObject.ValidItemTypes = ItemTypes.ToArray();
            }
            else if (Object is LogicDictionaryData.TrackerVariable VariableObject)
            {
                OutObject.ID = VariableObject.ID;
                OutObject.Area = "Variable";
                OutObject.Name = VariableObject.Name;
                OutObject.OriginalItem = VariableObject.ValueToString();
                OutObject.Randomizeditem = VariableObject.ValueToString();
                OutObject.Starred = !VariableObject.Static;
                List<string> ItemTypes = new() { "Variable" };
                if (VariableObject.Value is string) { ItemTypes.Add("string"); }
                if (VariableObject.Value is Int64) { ItemTypes.Add("Number"); }
                if (VariableObject.Value is bool) { ItemTypes.Add("Bool"); }
                if (VariableObject.Value is List<string>) { ItemTypes.Add("List"); }
                OutObject.ValidItemTypes = ItemTypes.ToArray();
            }
            else if (Object is EntranceData.EntranceRandoDestination DestinationObject)
            {
                OutObject.ID = DestinationObject.region;
                OutObject.Area = DestinationObject.from;
                OutObject.Name = DestinationObject.region;
                OutObject.OriginalItem = DestinationObject.from;
                OutObject.Randomizeditem = DestinationObject.from;
                OutObject.Starred = true;
                List<string> ItemTypes = new() { "Destination" };
                OutObject.ValidItemTypes = ItemTypes.ToArray();
            }
            else
            {
                string ObjectString = Object.ToString();
                OutObject.ID = ObjectString;
                OutObject.Area = ObjectString;
                OutObject.Name = ObjectString;
                OutObject.OriginalItem = ObjectString;
                OutObject.Randomizeditem = ObjectString;
                OutObject.Starred = true;
                OutObject.ValidItemTypes = new string[] { ObjectString };
            }
            return OutObject;
        }
    }
}
