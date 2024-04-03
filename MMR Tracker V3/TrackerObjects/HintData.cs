using System;
using System.Collections.Generic;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class HintData
    {

        [Serializable]
        public class HintObject(InstanceData.TrackerInstance Parent) : CheckableLocation(Parent)
        {
            private InstanceData.TrackerInstance _parent = Parent;
            public new RandomizedState RandomizedState
            {
                get { return _RandomizedState; }
                set { _RandomizedState = value == RandomizedState.Randomized ? RandomizedState.Randomized : RandomizedState.ForcedJunk; }
            }
            private RandomizedState _RandomizedState = RandomizedState.Randomized;
            public string HintText { get; set; }
            public Dictionary<string, string> ParsedHintData { get; set; } = [];
            public List<string> FoolishLocations { get; set; } = [];
            public string SpoilerHintText { get; set; }
            public override string ToString()
            {
                return DisplayName ?? ID;
            }
            public LogicDictionaryData.DictionaryHintEntries GetDictEntry()
            {
                return GetParent().LogicDictionary.HintSpots[ID];
            }

            public override string GetName()
            {
                return GetDictEntry().Name ?? ID;
            }

            public override LogicDictionaryData.DictionaryCheckableLocationEntry GetAbstractDictEntry() => GetDictEntry();

            public override CheckableLocationTypes LocationType() => CheckableLocationTypes.Hint;
        }
    }
}
