using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
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
        private static bool RequirementsMet(List<string> Requirements, LogicObjects.TrackerInstance instance, IEnumerable<OptionData.TrackerOption> OptionData)
        {
            foreach(string i in Requirements)
            {
                string LogicItem = i;
                int NeededAmount = 1;

                if (bool.TryParse(LogicItem, out bool BoolEntry) && !BoolEntry) { return false; }

                if (MultipleItemEntry(i, out string MultiItem, out int Amount))
                {
                    LogicItem = MultiItem;
                    NeededAmount = Amount;
                }

                var Mapping = instance.GetLogicItemMapping(LogicItem);
                if (Mapping == null) { return false; }
                if (!Mapping.GetMappedEntryUsable(instance, NeededAmount)) { return false; }
            }
            return true;
        }
        private static bool ConditionalsMet(List<List<string>> Conditionals, LogicObjects.TrackerInstance instance, IEnumerable<OptionData.TrackerOption> OptionData)
        {
            if (!Conditionals.Any()) { return true; }
            foreach (var i in Conditionals)
            {
                if (RequirementsMet(i, instance, OptionData)) { return true; }
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

            var ValidOptions = instance.TrackerOptions.Options.Where(x => x.LocationValid(ID) && x.Enabled);

            if (ValidOptions.Any())
            {
                foreach (var i in ValidOptions)
                {
                    //Do Option Edits
                }
            }

            return RequirementsMet(newRequirements, instance, ValidOptions) && ConditionalsMet(newConditionals, instance, ValidOptions);
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
                bool LocationAvailable = CheckRequirementAndConditionals(instance, i.LogicData.ConditionalItems, i.LogicData.RequiredItems, i.LogicData.Id);
                if (LocationAvailable != i.TrackerData.Available)
                {
                    ItemStateChanged = true;
                    i.TrackerData.Available = LocationAvailable;
                    CheckState checkState = i.TrackerData.Available ? CheckState.Checked : CheckState.Unchecked;
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
    }
}
