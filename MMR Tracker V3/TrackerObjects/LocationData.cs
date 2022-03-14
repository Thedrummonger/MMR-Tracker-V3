using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.MMRData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class LocationData
    {
        [Serializable]
        public class LocationObject
        {
            public string ID { get; set; }
            public RandomizeditemData Randomizeditem { get; set; } = new RandomizeditemData();
            public bool Available { get; set; } = false;
            public CheckState CheckState { get; set; } = CheckState.Unchecked;
            public bool Starred { get; set; }
            public int CheckPrice { get; set; } = -1;
            public string DisplayName { get; set; }
            public RandomizedState RandomizedState { get; set; } = RandomizedState.Randomized;
            public string SingleValidItem { get; set; } = null;
            public override string ToString()
            {
                return DisplayName ?? ID;
            }

            public void SetRandomizedState(RandomizedState Newstate, LogicObjects.TrackerInstance Instance)
            {
                if (Newstate == RandomizedState) { return; }
                RandomizedState = Newstate;
            }

            public bool IsUnrandomized(int Include = 0)
            {
                if ((Include == 0 || Include == 1) && RandomizedState == RandomizedState.Unrandomized) { return true; }
                if ((Include == 0 || Include == 2) && RandomizedState == RandomizedState.UnrandomizedManual) { return true; }
                return false;
            }
            public bool IsRandomized()
            {
                return RandomizedState == RandomizedState.Randomized;
            }
            public bool IsJunk()
            {
                return RandomizedState == RandomizedState.ForcedJunk;
            }
            public bool AutoMarked()
            {
                return IsUnrandomized(1) || SingleValidItem != null;
            }
            public LogicDictionaryData.DictionaryLocationEntries GetDictEntry(LogicObjects.TrackerInstance Instance)
            {
                return Instance.LogicDictionary.LocationList[Instance.InstanceReference.LocationDictionaryMapping[ID]];
            }

        }
        [Serializable]
        public class RandomizeditemData
        {
            public int OwningPlayer { get; set; } = -1;
            public string Item { get; set; } = null;
            public string SpoilerLogGivenItem { get; set; } = null;
            public string[] ValidItemTypes { get; set; } = Array.Empty<string>();
        }
    }
}
