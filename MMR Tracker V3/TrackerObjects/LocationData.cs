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
            public bool CanBeRandomized { get; set; } = true;
            public string DisplayName { get; set; }
            private RandomizedState _RandomizedState = RandomizedState.Randomized;
            public RandomizedState RandomizedState
            {
                get
                {
                    if (!CanBeRandomized)
                    {
                        if (_RandomizedState == RandomizedState.Randomized) { return RandomizedState.UnrandomizedManual; }
                        if (_RandomizedState == RandomizedState.ForcedJunk) { return RandomizedState.Unrandomized; }
                    }
                    return _RandomizedState;
                }
                set
                {
                    _RandomizedState = value;
                }
            }
            public override string ToString()
            {
                return DisplayName ?? ID;
            }
            public bool IsUnrandomized(int Include = 0)
            {
                if ((Include == 0 || Include == 1) && RandomizedState == RandomizedState.Unrandomized) { return true; }
                if ((Include == 0 || Include == 2) && RandomizedState == RandomizedState.UnrandomizedManual) { return true; }
                return false;
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
