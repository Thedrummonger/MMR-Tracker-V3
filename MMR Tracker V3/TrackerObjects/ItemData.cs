using MMR_Tracker_V3.TrackerObjectExtentions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.MMRData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class ItemData
    {
        [Serializable]
        public class ItemObject
        {
            public string Id { get; set; }
            public int AmountAquiredLocally { get; set; } = 0;
            public int AmountInStartingpool { get; set; } = 0;
            public Dictionary<int, int> AmountAquiredOnline { get; set; } = new Dictionary<int, int>();
            public Dictionary<int, int> AmountSentToPlayer { get; set; } = new Dictionary<int, int>();
            public string DisplayName { get; set; }
            public InstanceData.ReferenceData referenceData { get; set; } = new InstanceData.ReferenceData();

            public LogicDictionaryData.DictionaryItemEntries GetDictEntry(InstanceData.TrackerInstance Instance)
            {
                return Instance.LogicDictionary.ItemList[Id];
            }
            public override string ToString()
            {
                return DisplayName ?? Id;
            }
        }
    }
}
