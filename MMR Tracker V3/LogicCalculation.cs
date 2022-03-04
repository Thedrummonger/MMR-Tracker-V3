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

                string LogicItem = i;
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

                var Mapping = instance.GetLogicItemMapping(LogicItem);
                if (Mapping == null) { return false; }
                if (!Mapping.GetMappedEntryUsable(instance, NeededAmount)) { return false; }
            }
            if (LogItem) { Debug.WriteLine($"Entry was valid"); }
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
            while (true)
            {
                bool MacroChanged = CalculateMacros(instance);
                bool UnrandomizedItemAquired = CheckUrandomizedLocations(instance);
                if (!MacroChanged && !UnrandomizedItemAquired) { break; }
            }
            foreach (var i in instance.LocationPool.Locations.Where(x => x.TrackerData.RandomizedState != RandomizedState.Unrandomized))
            {
                i.TrackerData.Available = CheckRequirementAndConditionals(instance, i.LogicData.ConditionalItems, i.LogicData.RequiredItems, i.LogicData.Id);
            }
            foreach(var i in instance.HintPool.Hints)
            {
                i.Available = CheckRequirementAndConditionals(instance, i.LogicData.ConditionalItems, i.LogicData.RequiredItems, i.LogicData.Id);
            }

        }

        public static bool CheckRequirementAndConditionals(LogicObjects.TrackerInstance instance, List<List<string>> Conditionals, List<string> Requirements, string ID)
        {
            var CurrentObject = instance.GetLogicLocationMapping(ID);

            Utility.DeepCloneLogic(Requirements, Conditionals, out List<string> newRequirements, out List<List<string>> newConditionals);

            if (CurrentObject.logicEntryType == LogicEntryType.macro)
            {
                var MacroObject = (MacroObject)CurrentObject.GetMappedEntry(instance);
                if (MacroObject.DynamicLogic != null)
                {
                    var ReplacementMacro = HandleDynamicLogic(instance, MacroObject);
                    if (ReplacementMacro == null) { return false; }
                    Utility.DeepCloneLogic(ReplacementMacro.LogicData.RequiredItems, ReplacementMacro.LogicData.ConditionalItems, out newRequirements, out newConditionals);
                }
            }

            if (LogItem)
            {
                Debug.WriteLine(string.Join(",", newRequirements));
            }

            var ValidOptions = instance.TrackerOptions.Options.Where(x => x.GetActions().LocationValid(ID));

            if (ValidOptions.Any())
            {
                foreach (var option in ValidOptions)
                {
                    if (LogItem) { Debug.WriteLine($"Checking Option {option.ID}"); }
                    foreach (var replacements in option.GetActions().LogicReplacements)
                    {
                        if (!replacements.LocationValid(ID)) { continue; }
                        if (LogItem) { Debug.WriteLine($"Applying {replacements.ReplacementList.Length} Replacements"); }
                        foreach (var i in replacements.ReplacementList)
                        {
                            if (LogItem) { Debug.WriteLine($"Replacing {i.Target} With {i.Replacement}"); }
                            newRequirements = newRequirements
                                .Select(x => x == i.Target ? x.Replace(i.Target, i.Replacement) : x.Replace(" ", " ")).ToList();
                            for (var p = 0; p < newConditionals.Count; p++)
                            {
                                newConditionals[p] = newConditionals[p]
                                    .Select(x => x == i.Target ? x.Replace(i.Target, i.Replacement) : x.Replace(" ", " ")).ToList();
                            }
                        }
                    }
                    foreach(var additionalSet in option.GetActions().AdditionalLogic)
                    {
                        if (!additionalSet.LocationValid(ID)) { continue; }
                        foreach(var i in additionalSet.AdditionalRequirements)
                        {
                            newRequirements.Add(i);
                        }
                        foreach (var i in additionalSet.AdditionalConditionals)
                        {
                            newConditionals.Add(i);
                        }
                    }
                }
            }

            if (LogItem)
            {
                Debug.WriteLine(string.Join(",", newRequirements));
            }

            return RequirementsMet(newRequirements, instance) && ConditionalsMet(newConditionals, instance);
        }

        public static bool CalculateMacros(LogicObjects.TrackerInstance instance)
        {
            bool MacroStateChanged = false;
            foreach(var i in instance.Macros.MacroList)
            {
                var MacroData = i;

                bool MacroValid = CheckRequirementAndConditionals(instance, MacroData.LogicData.ConditionalItems, MacroData.LogicData.RequiredItems, i.LogicData.Id);

                if (!i.TrickEnabled) { MacroValid = false; }

                if (MacroValid != i.Aquired)
                {
                    MacroStateChanged = true;
                    i.Aquired = MacroValid;
                }
            }
            return MacroStateChanged;
        }

        public static TrackerObjects.MacroObject HandleDynamicLogic(LogicObjects.TrackerInstance instance, TrackerObjects.MacroObject i)
        {
            var LocationToCompare = instance.GetLogicLocationMapping(i.DynamicLogic.LocationToCompare);
            if (LocationToCompare == null || LocationToCompare.logicEntryType != LogicEntryType.location) { return null; }
            foreach (var arg in i.DynamicLogic.Arguments)
            {
                if (((LocationObject)LocationToCompare.GetMappedEntry(instance)).TrackerData.RandomizedItem == arg.ItemAtLocation)
                {
                    return (TrackerObjects.MacroObject)instance.GetLogicItemMapping(arg.LogicToUse).GetMappedEntry(instance);
                }
            }
            return null;
        }

        public static bool CheckUrandomizedLocations(LogicObjects.TrackerInstance instance)
        {
            bool ItemStateChanged = false;
            foreach (var i in instance.LocationPool.Locations.Where(x => x.TrackerData.RandomizedState == RandomizedState.Unrandomized))
            {
                Debug.WriteLine($"Setting {i.LogicData.Id}");
                bool LocationAvailable = CheckRequirementAndConditionals(instance, i.LogicData.ConditionalItems, i.LogicData.RequiredItems, i.LogicData.Id);

                bool ShouldBeChecked = LocationAvailable && i.TrackerData.CheckState != CheckState.Checked;
                bool ShouldBeUnChecked = !LocationAvailable && i.TrackerData.CheckState != CheckState.Unchecked;

                if (ShouldBeChecked || ShouldBeUnChecked)
                {
                    ItemStateChanged = true;
                    i.TrackerData.Available = LocationAvailable;
                    CheckState checkState = i.TrackerData.Available ? CheckState.Checked : CheckState.Unchecked;
                    if (checkState == CheckState.Unchecked) { i.TrackerData.RandomizedItem = null; }
                    if (checkState == CheckState.Checked) { i.TrackerData.RandomizedItem = i.TrackerData.GetItemAtCheck(); }

                    i.TrackerData.ToggleChecked(checkState, instance);
                }
            }
            return ItemStateChanged;
        }

        public static bool MultipleItemEntry(string Entry, out string Item, out int Amount)
        {
            Item = null;
            Amount = -1;
            if (!Entry.Contains(",")) { return false; }
            var data = Entry.Split(',');
            Item = data[0];
            if(!int.TryParse(data[1].Trim(), out Amount)) { return false; }
            return true;
        }

        private static bool LogicOptionEntry(LogicObjects.TrackerInstance instance, string i, out bool optionEntryValid)
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
            //todo
            return false;
        }
    }
}
