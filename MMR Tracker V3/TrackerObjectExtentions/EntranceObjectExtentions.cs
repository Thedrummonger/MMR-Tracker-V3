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
        public static string DisplayArea(this EntranceRandoExit exit)
        {
            return exit.GetDictEntry()?.DisplayArea??exit.ParentAreaID;
        }
        public static string DisplayExit(this EntranceRandoExit exit)
        {
            return exit.GetDictEntry()?.DisplayExit??exit.ID;
        }
        public static string GetEntranceDisplayName(this EntranceRandoExit ExitObjectObject)
        {
            var Destination = ExitObjectObject.GetDestinationAtExit();
            string StarredDisplay = ExitObjectObject.Starred ? "*" : "";
            string RandomizedExitDisplay = Destination is null ? "" : $"{Destination.region} <- {Destination.from}";

            return ExitObjectObject.CheckState switch
            {
                MiscData.CheckState.Marked => $"{ExitObjectObject.DisplayExit()}: {RandomizedExitDisplay}{StarredDisplay}",
                MiscData.CheckState.Unchecked => $"{ExitObjectObject.DisplayExit()}{StarredDisplay}",
                MiscData.CheckState.Checked => $"{RandomizedExitDisplay}: {ExitObjectObject.DisplayExit()}{StarredDisplay}",
                _ => ExitObjectObject.ToString(),
            };
        }
        public static bool IsRandomizableEntrance(this EntranceRandoExit exit)
        {
            return exit.GetDictEntry().RandomizableEntrance;
        }
        /// <summary>
        /// Returns true if the Entrances randomized destination is the given area.
        /// </summary>
        /// <param name="Entrance"></param>
        /// <param name="Area"></param>
        /// <returns></returns>
        public static bool LeadsToArea(this EntranceRandoExit Entrance, string Area)
        {
            if (Entrance.DestinationExit is not null && Entrance.DestinationExit.region == Area) { return true; }
            return false;
        }

        public static EntranceRandoDestination GetVanillaDestination(this EntranceRandoExit exit)
        {
            return new EntranceRandoDestination { region = exit.ID, from = exit.ParentAreaID };
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
        /// <summary>
        /// Determines which destination should be applied to this exit when it's checked
        /// It will get this data from one of three places in the following priority
        /// 1. It's vanilla exit if the check is unrandomized
        /// 2. It's Spoiler Defined Destination if one has been set by the spoiler log
        /// 3. It's currently defined DestinationExit. If this is null the result of this function will be null
        /// </summary>
        /// <param name="exit"></param>
        /// <param name="currentTrackerInstance"></param>
        /// <returns></returns>
        public static EntranceRandoDestination GetDestinationAtExit(this EntranceRandoExit exit)
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
        /// <summary>
        /// Use this function to set the new checkstate of the Exit
        /// </summary>
        /// <param name="exit"></param>
        /// <param name="NewState">The new Randomized State</param>
        /// <param name="Instance"></param>
        /// <returns></returns>
        public static bool ToggleExitChecked(this EntranceRandoExit exit, CheckState NewState)
        {
            CheckState CurrentState = exit.CheckState;
            if (CurrentState == NewState)
            {
                return false;
            }
            else if (CurrentState == CheckState.Checked)
            {
                var Destination = exit.GetParent().EntrancePool.AreaList[exit.DestinationExit.region];
                Destination.ExitsAcessibleFrom--;
            }
            else if (NewState == CheckState.Checked)
            {
                if (exit.DestinationExit == null) { return false; }
                var Destination = exit.GetParent().EntrancePool.AreaList[exit.DestinationExit.region];
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

        public static EntranceRandoDestination AsDestination(this EntranceRandoExit exit)
        {
            return new EntranceRandoDestination { from = exit.ParentAreaID, region = exit.ID };
        }

        public static EntranceRandoDestination AsDestination(this EntranceAreaPair Pair)
        {
            return new EntranceRandoDestination { from = Pair.Area, region = Pair.Exit };
        }

        public static EntranceRandoExit AsExit(this EntranceRandoDestination destination, InstanceData.TrackerInstance ParentInstance)
        {
            return ParentInstance.EntrancePool.AreaList[destination.from].GetExit(destination.region);
        }

        public static EntranceRandoExit AsExit(this EntranceAreaPair Pair, InstanceData.TrackerInstance ParentInstance)
        {
            return ParentInstance.EntrancePool.AreaList[Pair.Area].GetExit(Pair.Exit);
        }
    }
}
