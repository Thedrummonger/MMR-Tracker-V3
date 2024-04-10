using MMR_Tracker_V3.TrackerObjects;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.EntranceData;
using MMR_Tracker_V3.TrackerObjectExtentions;

namespace MMR_Tracker_V3.TrackerObjectExtensions
{
    public static class EntranceObjectExtensions
    {
        public static string DisplayArea(this EntranceRandoExit exit)
        {
            return exit.GetDictEntry()?.DisplayArea ?? exit.GetParentArea().ID;
        }
        public static string DisplayExit(this EntranceRandoExit exit)
        {
            return exit.GetDictEntry()?.DisplayExit ?? exit.ExitID;
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
            return new EntranceRandoDestination { region = exit.ExitID, from = exit.GetParentArea().ID };
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
                DestinationAtCheck = new EntranceRandoDestination { region = exit.ExitID, from = exit.GetParentArea().ID };
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
                exit.UncheckExit(NewState);
            }
            else if (NewState == CheckState.Checked)
            {
                if (!exit.CheckExit(NewState)) { return false; }
            }
            else
            {
                if (!exit.ToggleMarked(NewState)) { return false; }
            }
            exit.CheckState = NewState;
            return true;
        }
        public static bool UncheckExit(this EntranceData.EntranceRandoExit loc, CheckState NewState)
        {
            var Area = loc.GetParent().GetAreaByLogicID(loc.DestinationExit.region);

            Area?.ChangeLocalItemAmounts(loc, -1);
            if (NewState == CheckState.Unchecked)
            {
                loc.DestinationExit = null;
            }
            return true;

        }

        public static bool CheckExit(this EntranceData.EntranceRandoExit loc, CheckState NewState)
        {
            if (loc.DestinationExit is null) { return false; }

            if (loc.GetParent().AreaPool.TryGetValue(loc.DestinationExit.region, out EntranceRandoArea Area))
            {
                Area.ChangeLocalItemAmounts(loc, 1);
            }
            return true;
        }
        public static bool ToggleMarked(this EntranceRandoExit loc, CheckState NewState)
        {
            if (NewState == CheckState.Marked && loc.DestinationExit is null)
            {
                return false;
            }
            else if (NewState == CheckState.Unchecked)
            {
                loc.DestinationExit = null;
            }
            return true;
        }

        public static EntranceRandoDestination AsDestination(this EntranceRandoExit exit)
        {
            return new EntranceRandoDestination { from = exit.GetParentArea().ID, region = exit.ExitID };
        }

        public static EntranceRandoDestination AsDestination(this EntranceAreaPair Pair)
        {
            return new EntranceRandoDestination { from = Pair.Area, region = Pair.Exit };
        }

        public static EntranceRandoExit AsExit(this EntranceRandoDestination destination, InstanceData.TrackerInstance ParentInstance)
        {
            return ParentInstance.GetExitByAreaIDAndExitID(destination.from, destination.region);
        }

        public static EntranceRandoExit AsExit(this EntranceAreaPair Pair, InstanceData.TrackerInstance ParentInstance)
        {
            return ParentInstance.GetExitByAreaIDAndExitID(Pair.Area, Pair.Exit);
        }
    }
}
