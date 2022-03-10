using System;
using System.Collections.Generic;
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
            public SpoilerlogReference SpoilerData { get; set; } = new SpoilerlogReference();
            public string DisplayName { get; set; }

            public override string ToString()
            {
                return DisplayName ?? Id;
            }
            public LogicDictionaryData.DictionaryItemEntries GetDictEntry(LogicObjects.TrackerInstance Instance)
            {
                return Instance.LogicDictionary.ItemList[Instance.InstanceReference.ItemDictionaryMapping[Id]];
            }
            public bool ValidStartingItem(LogicObjects.TrackerInstance Instance)
            {
                var ItemIndex = Instance.InstanceReference.ItemDictionaryMapping[Id];
                var DictEntry = Instance.LogicDictionary.ItemList[ItemIndex];
                return DictEntry.ValidStartingItem != null && (bool)DictEntry.ValidStartingItem;
            }
        }
    }
}
