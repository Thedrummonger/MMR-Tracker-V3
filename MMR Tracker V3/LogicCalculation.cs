using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public static class LogicCalculation
    {
        static bool LogItem = false;

        private static bool RequirementsMet(List<string> Requirements, LogicObjects.TrackerInstance instance)
        {
            foreach(string i in Requirements)
            {
                if (LogItem) { Debug.WriteLine($"Checking Requirement {i}"); }

                bool Literal = i.IsLiteralID(out string LogicItem);
                int NeededAmount = 1;

                if (bool.TryParse(LogicItem, out bool BoolEntry)) 
                { 
                    if (BoolEntry) { continue; }
                    else { return false; }
                }

                if (LogicOptionEntry(instance, LogicItem, out bool OptionEntryValid))
                {
                    if (OptionEntryValid) { continue; }
                    else { return false; }
                }

                if (MultipleItemEntry(i, out string MultiItem, out int Amount))
                {
                    LogicItem = MultiItem;
                    NeededAmount = Amount;
                }

                var type = instance.GetItemEntryType(LogicItem, Literal);
                if (type == LogicEntryType.item) 
                {
                    var ItemObject = instance.GetItemByID(LogicItem);
                    if (!ItemObject.Useable(NeededAmount)) { return false; }
                }
                else if (type == LogicEntryType.macro)
                {
                    var MacroObject = instance.GetMacroByID(LogicItem);
                    if (!MacroObject.Aquired) { return false; }
                }
                else
                {
                    Debug.WriteLine($"{LogicItem}: {type}");
                    return false;
                }
            }
            return true;
        }

        private static bool ConditionalsMet(List<List<string>> Conditionals, LogicObjects.TrackerInstance instance)
        {
            if (!Conditionals.Any()) { return true; }
            foreach (var i in Conditionals)
            {
                if (RequirementsMet(i, instance)) { return true; }
            }
            return false;
        }
        public static void CalculateLogic(LogicObjects.TrackerInstance instance)
        {
            Stopwatch stopwatch = new Stopwatch();

            Utility.TimeCodeExecution(stopwatch);
            while (true)
            {
                bool MacroChanged = CalculateMacros(instance);
                bool UnrandomizedItemAquired = CheckUrandomizedLocations(instance);
                if (!MacroChanged && !UnrandomizedItemAquired) { break; }
            }
            Utility.TimeCodeExecution(stopwatch, "Macro and Unrandomized Item Calculation", 1);
            foreach (var i in instance.LocationPool.Where(x => x.RandomizedState != RandomizedState.Unrandomized))
            {
                var Logic = instance.GetLogic(i.ID);
                i.Available = RequirementsMet(Logic.RequiredItems, instance) && ConditionalsMet(Logic.ConditionalItems, instance);
            }
            Utility.TimeCodeExecution(stopwatch, "Location Calculation", 1);
            foreach (var i in instance.HintPool)
            {
                var Logic = instance.GetLogic(i.ID);
                i.Available = RequirementsMet(Logic.RequiredItems, instance) && ConditionalsMet(Logic.ConditionalItems, instance);
            }
            Utility.TimeCodeExecution(stopwatch, "Hint Location Calculation", 1);

        }

        public static bool CalculateMacros(LogicObjects.TrackerInstance instance)
        {
            bool MacroStateChanged = false;
            foreach(var i in instance.MacroPool)
            {
                var Logic = instance.GetLogic(i.ID);
                var Available = RequirementsMet(Logic.RequiredItems, instance) && ConditionalsMet(Logic.ConditionalItems, instance);

                if (Logic.IsTrick && !i.TrickEnabled) { Available = false; }

                if (Available != i.Aquired)
                {
                    MacroStateChanged = true;
                    i.Aquired = Available;
                }
            }
            return MacroStateChanged;
        }


        public static bool CheckUrandomizedLocations(LogicObjects.TrackerInstance instance)
        {
            bool ItemStateChanged = false;
            foreach (var i in instance.LocationPool.Where(x => x.RandomizedState == RandomizedState.Unrandomized))
            {
                var Logic = instance.GetLogic(i.ID);
                var Available = RequirementsMet(Logic.RequiredItems, instance) && ConditionalsMet(Logic.ConditionalItems, instance);

                bool ShouldBeChecked = Available && i.CheckState != CheckState.Checked;
                bool ShouldBeUnChecked = !Available && i.CheckState != CheckState.Unchecked;

                if (ShouldBeChecked || ShouldBeUnChecked)
                {
                    ItemStateChanged = true;
                    i.Available = Available;
                    CheckState checkState = i.Available ? CheckState.Checked : CheckState.Unchecked;
                    if (checkState == CheckState.Unchecked) { i.Randomizeditem.Item = null; }
                    if (checkState == CheckState.Checked) { i.Randomizeditem.Item = i.GetItemAtCheck(instance); }

                    i.ToggleChecked(checkState, instance);
                }
            }
            return ItemStateChanged;
        }

        public static bool MultipleItemEntry(string Entry, out string Item, out int Amount)
        {
            Item = Entry;
            Amount = 1;
            if (!Entry.Contains(",")) { return false; }
            var data = Entry.Split(',');
            Item = data[0];
            if(!int.TryParse(data[1].Trim(), out Amount)) { return false; }
            return true;
        }

        public static bool LogicOptionEntry(LogicObjects.TrackerInstance instance, string i, out bool optionEntryValid)
        {
            optionEntryValid = false;
            if (!i.Contains("==") && !i.Contains("!=")) { return false; }

            bool inverse = i.Contains("!=");
            string[] data = inverse ? i.Split("!=") : i.Split("==");
            optionEntryValid = checkOptionEntry(instance, data, inverse);
            return true;
        }

        private static bool checkOptionEntry(LogicObjects.TrackerInstance instance, string[] data, bool inverse)
        {
            string CleanedOptionName = data[0].Trim().Replace("'", "");
            string CleanedOptionValue = data[1].Trim().Replace("'", "");

            var Option = instance.TrackerOptions.Find(x => x.ID == CleanedOptionName);
            if (Option == null) { return false; }
            return (Option.CurrentValue == CleanedOptionValue) == !inverse;
        }
    }
}
