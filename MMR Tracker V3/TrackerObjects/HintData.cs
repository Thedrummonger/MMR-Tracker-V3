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
            public string ID { get; set; }
            public CheckState CheckState { get; set; } = CheckState.Unchecked;
            public RandomizedState RandomizedState { get; set; } = RandomizedState.Randomized;
            public bool Available { get; set; }
            public bool Starred { get; set; }
            public string HintText { get; set; }
            public Dictionary<string, string> ParsedHintData { get; set; } = new Dictionary<string, string>();
            public List<string> FoolishLocations { get; set; } = new List<string>();
            public string SpoilerHintText { get; set; }
            public string DisplayName { get; set; }
            public LogicObjects.ReferenceData referenceData { get; set; } = new LogicObjects.ReferenceData();
            public override string ToString()
            {
                return DisplayName ?? ID;
            }
            public LogicDictionaryData.DictionaryHintEntries GetDictEntry(LogicObjects.TrackerInstance Instance)
            {
                return Instance.LogicDictionary.HintSpots[referenceData.DictIndex];
            }
        }
    }
}
