using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.MMRData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class HintData
    {

        [Serializable]
        public class HintObject
        {
            private InstanceData.TrackerInstance _parent;
            public InstanceData.TrackerInstance GetParent() { return _parent; }
            public void SetParent(InstanceData.TrackerInstance parent) { _parent = parent; }
            public HintObject(InstanceData.TrackerInstance Parent)
            {
                _parent = Parent;
            }
            public string ID { get; set; }
            public CheckState CheckState { get; set; } = CheckState.Unchecked;
            public RandomizedState RandomizedState { 
                get { return _RandomizedState; } 
                set { _RandomizedState = value == RandomizedState.Randomized ? RandomizedState.Randomized : RandomizedState.ForcedJunk; }
            }
            private RandomizedState _RandomizedState = RandomizedState.Randomized;
            public bool Available { get; set; }
            public bool Starred { get; set; }
            public string HintText { get; set; }
            public Dictionary<string, string> ParsedHintData { get; set; } = new Dictionary<string, string>();
            public List<string> FoolishLocations { get; set; } = new List<string>();
            public string SpoilerHintText { get; set; }
            public string DisplayName { get; set; }
            public InstanceData.ReferenceData referenceData { get; set; } = new InstanceData.ReferenceData();
            public override string ToString()
            {
                return DisplayName ?? ID;
            }
            public LogicDictionaryData.DictionaryHintEntries GetDictEntry()
            {
                return GetParent().LogicDictionary.HintSpots[ID];
            }
        }
    }
}
