using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MMR_Tracker_V3
{
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

        public static readonly HashSet<char> LogicChars = ['&', '|', '+', '*', '(', ')'];

        public static List<string> GetEntriesFromLogicString(string input)
        {
            List<string> BrokenString = [];
            string currentItem = "";
            bool InEscapeChar = false;
            foreach (var i in input)
            {
                if (i == '\\' && !InEscapeChar) { InEscapeChar = true; continue; }
                if (LogicChars.Contains(i) && !InEscapeChar)
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
                if (InEscapeChar) { InEscapeChar = false; }
            }
            if (currentItem != "") { BrokenString.Add(currentItem); }
            return BrokenString;
        }

        public static bool FilterSearch(InstanceData.TrackerInstance Instance, object InObject, string searchTerm, string NameToCompare)
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
            while (searchTerm.Length > 0 && GlobalModifiers.Contains(searchTerm[0]))
            {
                if (searchTerm[0] == '*') { StarredOnly = true; }
                searchTerm = searchTerm[1..];
            }
            if (StarredOnly && !searchObject.Starred) { return false; }
            if (string.IsNullOrWhiteSpace(searchTerm)) { return true; }

            List<string> ExpandedExptression = GetEntriesFromLogicString(searchTerm);

            for (var i = 0; i < ExpandedExptression.Count; i++)
            {
                ExpandedExptression[i] = PerformLogicCheck(ExpandedExptression[i], searchObject, NameToCompare);
            }

            string Expression = string.Join("", ExpandedExptression);
            //Debug.WriteLine($"Expression = {Expression}");
            try
            {
                DataTable dt = new();
                var Solution = dt.Compute(Expression, "");
                if (!int.TryParse(Solution.ToString(), out int Result)) { return true; }
                return Result > 0;
            }
            catch { CurrentStringIsError = true; return false; }
        }

        private static string PerformLogicCheck(string i, SearchObject logic, string NameToCompare)
        {
            if (i.Length == 1 && LogicChars.Contains(i[0])) { return i; }

            char[] Modifiers = new char[] { '!', '=' };

            bool Inverse = false;
            bool Perfect = false;
            var subterm = i;
            if (subterm == "") { return "1"; }
            while (subterm.Length > 0 && Modifiers.Contains(subterm[0]))
            {
                if (subterm[0] == '!') { Inverse = true; }
                if (subterm[0] == '=') { Perfect = true; }
                subterm = subterm[1..];
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
                    if (Perfect && logic.Area.Equals(subterm[1..], StringComparison.CurrentCultureIgnoreCase) == Inverse) { return "0"; }
                    else if (!Perfect && logic.Area.Contains(subterm[1..], StringComparison.CurrentCultureIgnoreCase) == Inverse) { return "0"; }
                    break;
                case '@'://Search By Item Type
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.ValidItemTypes == null) { return "0"; }
                    if (Perfect && logic.ValidItemTypes.Select(x => x.ToLower()).Contains(subterm[1..].ToLower()) == Inverse) { return "0"; }
                    else if ((!Perfect && logic.ValidItemTypes.Select(x => x.ToLower()).Any(x => x.Contains(subterm[1..], StringComparison.CurrentCultureIgnoreCase))) == Inverse) { return "0"; }
                    break;
                case '~'://Search By Dictionary Name
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.ID == null) { return "0"; }
                    if (Perfect && logic.ID.Equals(subterm[1..], StringComparison.CurrentCultureIgnoreCase) == Inverse) { return "0"; }
                    else if ((!Perfect && logic.ID.Contains(subterm[1..], StringComparison.CurrentCultureIgnoreCase)) == Inverse) { return "0"; }
                    break;
                case '$'://Search By Original Item Name
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.OriginalItem == null) { return "0"; }
                    if (Perfect && logic.OriginalItem.Equals(subterm[1..], StringComparison.CurrentCultureIgnoreCase) == Inverse) { return "0"; }
                    else if (!Perfect && logic.OriginalItem.Contains(subterm[1..], StringComparison.CurrentCultureIgnoreCase) == Inverse) { return "0"; }
                    break;
                case '%'://Search By Location Name
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.Name == null) { return "0"; }
                    if (Perfect && logic.Name.Equals(subterm[1..], StringComparison.CurrentCultureIgnoreCase) == Inverse) { return "0"; }
                    else if (!Perfect && logic.Name.Contains(subterm[1..], StringComparison.CurrentCultureIgnoreCase) == Inverse) { return "0"; }
                    break;
                default: //Search By "NameToCompare" variable
                    if (Perfect && NameToCompare.Equals(subterm, StringComparison.CurrentCultureIgnoreCase) == Inverse) { return "0"; }
                    else if (!Perfect && (NameToCompare.Contains(subterm, StringComparison.CurrentCultureIgnoreCase) == Inverse)) { return "0"; }
                    break;
            }
            return "1";
        }

        public static SearchObject CreateSearchableObject(object Object, InstanceData.TrackerInstance instance)
        {
            SearchObject OutObject = new();
            if (Object is LocationData.LocationObject locationObject)
            {
                var DictData = locationObject.GetDictEntry();
                OutObject.ID = locationObject.ID;
                OutObject.Area = DictData.Area;
                OutObject.Name = DictData.GetName();
                OutObject.OriginalItem = DictData.OriginalItem;
                OutObject.Randomizeditem = locationObject.Randomizeditem.Item;
                OutObject.Starred = locationObject.Starred;
                OutObject.ValidItemTypes = DictData.ValidItemTypes;
            }
            else if (Object is LocationData.LocationProxy locationProxy)
            {
                var LocReference = instance.GetLocationByID(locationProxy.ReferenceID);
                var DictData = LocReference.GetDictEntry();
                OutObject.ID = locationProxy.ID;
                OutObject.Area = locationProxy.GetDictEntry().Area;
                OutObject.Name = locationProxy.GetDictEntry().Name;
                OutObject.OriginalItem = DictData.OriginalItem;
                OutObject.Randomizeditem = LocReference?.Randomizeditem.Item;
                OutObject.Starred = LocReference?.Starred ?? false;
                OutObject.ValidItemTypes = DictData.ValidItemTypes;
            }
            else if (Object is ItemData.ItemObject ItemObject)
            {
                var DictData = ItemObject.GetDictEntry();
                OutObject.ID = ItemObject.ID;
                OutObject.Area = "";
                if (ItemObject.AmountInStartingpool > 0) { OutObject.Area += "starting "; }
                if (ItemObject.AmountAquiredOnline.Any(x => x.Value > 0)) { OutObject.Area += "online "; }
                OutObject.Area += "item";
                OutObject.Name = DictData.GetName();
                OutObject.OriginalItem = DictData.GetName();
                OutObject.Randomizeditem = DictData.GetName();
                OutObject.Starred = false;
                OutObject.ValidItemTypes = DictData.ItemTypes;
            }
            else if (Object is HintData.HintObject HintObject)
            {
                var DictData = HintObject.GetDictEntry();
                OutObject.ID = HintObject.ID;
                OutObject.Area = "hints";
                OutObject.Name = DictData.Name;
                OutObject.OriginalItem = DictData.Name;
                OutObject.Randomizeditem = DictData.Name;
                OutObject.Starred = false;
                OutObject.ValidItemTypes = ["hint"];
            }
            else if (Object is MacroObject MacroObject)
            {
                bool Istrick = MacroObject.isTrick();
                var DictData = MacroObject.GetDictEntry();
                var LogicData = instance.GetLogic(MacroObject.ID, false);
                OutObject.ID = MacroObject.ID;
                OutObject.Area = Istrick ? (LogicData.TrickCategory ?? "misc") : "macro";
                OutObject.Name = DictData.Name ?? MacroObject.ID;
                OutObject.OriginalItem = MacroObject.ID;
                OutObject.Randomizeditem = MacroObject.ID;
                OutObject.Starred = Istrick;
                List<string> ItemTypes = ["macro"];
                if (Istrick) { ItemTypes.Add("trick"); }
                OutObject.ValidItemTypes = [.. ItemTypes];
            }
            else if (Object is EntranceData.EntranceRandoExit ExitObject)
            {
                OutObject.ID = ExitObject.ID;
                OutObject.Area = ExitObject.DisplayArea();
                OutObject.Name = ExitObject.ExitID;
                OutObject.OriginalItem = ExitObject.EntrancePair == null ? "One Way" : $"{ExitObject.EntrancePair.Area} To {ExitObject.EntrancePair.Exit}";
                OutObject.Randomizeditem = ExitObject.DestinationExit == null ? null : $"{ExitObject.DestinationExit.region} From {ExitObject.DestinationExit.from}";
                OutObject.Starred = ExitObject.Starred;
                List<string> ItemTypes = ["exit"];
                if (ExitObject.EntrancePair == null) { ItemTypes.Add("One Way"); }
                OutObject.ValidItemTypes = [.. ItemTypes];
            }
            else if (Object is OptionData.ChoiceOption ChoiceOptionObject)
            {
                OutObject.ID = ChoiceOptionObject.ID;
                OutObject.Area = "Options";
                OutObject.Name = ChoiceOptionObject.Name;
                OutObject.OriginalItem = ChoiceOptionObject.Value;
                OutObject.Randomizeditem = ChoiceOptionObject.Value;
                OutObject.Starred = true;
                List<string> ItemTypes = ["Option", "Choice"];
                OutObject.ValidItemTypes = [.. ItemTypes];
            }
            else if (Object is OptionData.ToggleOption ToggleOptionObject)
            {
                OutObject.ID = ToggleOptionObject.ID;
                OutObject.Area = "Options";
                OutObject.Name = ToggleOptionObject.Name;
                OutObject.OriginalItem = ToggleOptionObject.GetValue().ID;
                OutObject.Randomizeditem = ToggleOptionObject.GetValue().ID;
                OutObject.Starred = true;
                List<string> ItemTypes = ["Option", "Toggle"];
                OutObject.ValidItemTypes = [.. ItemTypes];
            }
            else if (Object is OptionData.IntOption IntOptionObject)
            {
                OutObject.ID = IntOptionObject.ID;
                OutObject.Area = "Options";
                OutObject.Name = IntOptionObject.Name;
                OutObject.OriginalItem = IntOptionObject.Value.ToString();
                OutObject.Randomizeditem = IntOptionObject.Value.ToString();
                OutObject.Starred = true;
                List<string> ItemTypes = ["Option", "int"];
                OutObject.ValidItemTypes = [.. ItemTypes];
            }
            else if (Object is EntranceData.EntranceRandoDestination DestinationObject)
            {
                OutObject.ID = DestinationObject.region;
                OutObject.Area = DestinationObject.from;
                OutObject.Name = DestinationObject.region;
                OutObject.OriginalItem = DestinationObject.from;
                OutObject.Randomizeditem = DestinationObject.from;
                OutObject.Starred = true;
                List<string> ItemTypes = ["Destination"];
                OutObject.ValidItemTypes = [.. ItemTypes];
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
                OutObject.ValidItemTypes = [ObjectString];
            }
            return OutObject;
        }
    }
}
