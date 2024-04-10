using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjectExtentions
{
    public static class CheckableLocationsObjectExtentions
    {
        public static void SetRandomizedState(this CheckableLocation loc, RandomizedState Newstate)
        {
            if (Newstate == loc.RandomizedState) { return; }
            loc.RandomizedState = Newstate;
        }
        public static bool IsUnrandomized(this CheckableLocation loc, UnrandState Include = UnrandState.Any)
        {
            if ((Include == UnrandState.Any || Include == UnrandState.Unrand) && loc.RandomizedState == RandomizedState.Unrandomized) { return true; }
            if ((Include == UnrandState.Any || Include == UnrandState.Manual) && loc.RandomizedState == RandomizedState.UnrandomizedManual) { return true; }
            return false;
        }
        public static bool IsRandomized(this CheckableLocation loc)
        {
            return loc.RandomizedState == RandomizedState.Randomized;
        }
        public static bool IsJunk(this CheckableLocation loc)
        {
            return loc.RandomizedState == RandomizedState.ForcedJunk;
        }
        public static bool AppearsinListbox(this CheckableLocation loc, bool ShowJunkUnrand = false)
        {
            return (!loc.IsJunk() || ShowJunkUnrand) && (!loc.IsUnrandomized(MiscData.UnrandState.Unrand) || ShowJunkUnrand) && !string.IsNullOrWhiteSpace(loc.GetAbstractDictEntry().GetName());
        }
    }
}
