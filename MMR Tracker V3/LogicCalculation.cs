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
        public static bool RequirementsMet(List<string> Requirements, LogicObjects.TrackerInstance instance)
        {
            foreach(var i in Requirements)
            {
                if (!instance.InstanceReference.LogicDataMappings[i].GetMappedEntryUsable(instance))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool ConditionalsMet(List<List<string>> Conditionals, LogicObjects.TrackerInstance instance)
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
                if (!CalculateMacros(instance) && !CheckUrandomizedLocations(instance))
                {
                    break;
                }
            }
            foreach (var i in instance.LocationPool.Locations.Where(x => x.TrackerData.RandomizedState != RandomizedState.Unrandomized))
            {
                i.TrackerData.Available = RequirementsMet(i.LogicData.RequiredItems, instance) && ConditionalsMet(i.LogicData.ConditionalItems, instance);
            }

        }
        public static bool CalculateMacros(LogicObjects.TrackerInstance instance)
        {
            bool MacroStateChanged = false;
            foreach(var i in instance.Macros.MacroList)
            {
                bool MacroValid = RequirementsMet(i.LogicData.RequiredItems, instance) && ConditionalsMet(i.LogicData.ConditionalItems, instance);
                if (MacroValid != i.Aquired)
                {
                    MacroStateChanged = true;
                    i.Aquired = MacroValid;
                }
            }
            return MacroStateChanged;
        }
        public static bool CheckUrandomizedLocations(LogicObjects.TrackerInstance instance)
        {
            bool ItemStateChanged = false;
            foreach (var i in instance.LocationPool.Locations.Where(x => x.TrackerData.RandomizedState == RandomizedState.Unrandomized))
            {
                bool LocationAvailable = RequirementsMet(i.LogicData.RequiredItems, instance) && ConditionalsMet(i.LogicData.ConditionalItems, instance);
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
    }
}
