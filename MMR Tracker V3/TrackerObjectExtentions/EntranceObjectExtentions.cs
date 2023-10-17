using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.EntranceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjectExtentions
{
    public static class EntranceObjectExtentions
    {
        public static string DisplayArea(this EntranceRandoExit exit, InstanceData.TrackerInstance Instance)
        {
            return exit.GetDictEntry(Instance)?.DisplayArea??exit.ParentAreaID;
        }
        public static string DisplayExit(this EntranceRandoExit exit, InstanceData.TrackerInstance Instance)
        {
            return exit.GetDictEntry(Instance)?.DisplayExit??exit.ID;
        }
        public static bool IsRandomizableEntrance(this EntranceRandoExit exit, InstanceData.TrackerInstance currentTrackerInstance)
        {
            return exit.GetDictEntry(currentTrackerInstance).RandomizableEntrance;
        }

        public static LogicDictionaryData.DictionaryEntranceEntries GetDictEntry(this EntranceRandoExit exit, InstanceData.TrackerInstance Instance)
        {
            return Instance.LogicDictionary.EntranceList[Instance.GetLogicNameFromExit(exit)];
        }

        public static EntranceRandoDestination GetVanillaDestination(this EntranceRandoExit exit)
        {
            return new EntranceRandoDestination { region = exit.ID, from = exit.ParentAreaID };
        }
        public static EntranceRandoDestination GetDestnationFromEntrancePair(this EntranceRandoExit exit)
        {
            return new EntranceRandoDestination { region = exit.EntrancePair.Exit, from = exit.EntrancePair.Area };
        }
        public static bool IsUnrandomized(this EntranceRandoExit exit, UnrandState Include = UnrandState.Any)
        {
            if ((Include == UnrandState.Any || Include == UnrandState.Unrand) && exit.RandomizedState == RandomizedState.Unrandomized) { return true; }
            if ((Include == UnrandState.Any || Include == UnrandState.Manual) && exit.RandomizedState == RandomizedState.UnrandomizedManual) { return true; }
            return false;
        }
        public static bool IsRandomized(this EntranceRandoExit exit)
        {
            return exit.RandomizedState == RandomizedState.Randomized;
        }
        public static bool IsJunk(this EntranceRandoExit exit)
        {
            return exit.RandomizedState == RandomizedState.ForcedJunk;
        }

        public static EntranceRandoDestination GetDestinationAtExit(this EntranceRandoExit exit, InstanceData.TrackerInstance currentTrackerInstance)
        {
            var DestinationAtCheck = exit.DestinationExit;
            if (exit.SpoilerDefinedDestinationExit != null)
            {
                DestinationAtCheck = exit.SpoilerDefinedDestinationExit;
            }
            if ((exit.IsUnrandomized()))
            {
                DestinationAtCheck = new EntranceRandoDestination { region = exit.ID, from = exit.ParentAreaID };
            }
            return DestinationAtCheck;
        }

        public static bool ToggleExitChecked(this EntranceRandoExit exit, CheckState NewState, InstanceData.TrackerInstance Instance)
        {
            CheckState CurrentState = exit.CheckState;
            if (CurrentState == NewState)
            {
                return false;
            }
            else if (CurrentState == CheckState.Checked)
            {
                var Destination = Instance.EntrancePool.AreaList[exit.DestinationExit.region];
                Destination.ExitsAcessibleFrom--;
            }
            else if (NewState == CheckState.Checked)
            {
                if (exit.DestinationExit == null) { return false; }
                var Destination = Instance.EntrancePool.AreaList[exit.DestinationExit.region];
                Destination.ExitsAcessibleFrom++;
            }
            else if (CurrentState == CheckState.Unchecked && NewState == CheckState.Marked)
            {
                if (exit.DestinationExit == null) { return false; }
            }

            if (NewState == CheckState.Unchecked) { exit.DestinationExit = null; }
            exit.CheckState = NewState;
            return true;
        }
    }
}
