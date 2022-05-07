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
            public LogicObjects.ReferenceData referenceData { get; set; } = new LogicObjects.ReferenceData();


            public override string ToString()
            {
                return DisplayName ?? Id;
            }
            public LogicDictionaryData.DictionaryItemEntries GetDictEntry(LogicObjects.TrackerInstance Instance)
            {
                return Instance.LogicDictionary.ItemList[referenceData.DictIndex];
            }
            public bool ValidStartingItem(LogicObjects.TrackerInstance Instance)
            {
                var ItemIndex = Instance.GetItemByID(Id);
                var DictEntry = ItemIndex.GetDictEntry(Instance);
                return DictEntry.ValidStartingItem != null && (bool)DictEntry.ValidStartingItem;
            }

            public int GetAmountPlaced(LogicObjects.TrackerInstance Instance)
            {
                int AmountAquired = GetTotalUsable(Instance);
                int AmountSetAtLocation = 0;
                foreach (var x in Instance.LocationPool.Where(x => x.Value.CheckState != CheckState.Checked))
                {
                    var itemAtheck = x.Value.GetItemAtCheck(Instance) ?? "";
                    if (itemAtheck == Id) { AmountSetAtLocation++; }
                }

                return AmountAquired + AmountSetAtLocation;
            }

            public int GetAmountInStartingPool()
            {
                return AmountInStartingpool;
            }

            public bool CanBePlaced(LogicObjects.TrackerInstance Instance)
            {
                if (GetDictEntry(Instance).MaxAmountInWorld < 0) { return true; }
                return GetAmountPlaced(Instance) < GetDictEntry(Instance).MaxAmountInWorld;
            }

            public int GetTotalUsable(LogicObjects.TrackerInstance Instance)
            {
                return AmountAquiredLocally + AmountAquiredOnline.Values.Sum() + GetAmountInStartingPool();
            }

            public bool Useable(LogicObjects.TrackerInstance Instance, int Amount = 1)
            {
                return GetTotalUsable(Instance) >= Amount;
            }

            public void ChangeLocalItemAmounts(LogicObjects.TrackerInstance Instance, LocationData.LocationObject location, int Amount)
            {
                if (Amount == 0) { return; }
                if (location.Randomizeditem.OwningPlayer != -1)
                {
                    if (!AmountSentToPlayer.ContainsKey(location.Randomizeditem.OwningPlayer))
                    {
                        AmountSentToPlayer.Add(location.Randomizeditem.OwningPlayer, 0);
                    }
                    AmountSentToPlayer[location.Randomizeditem.OwningPlayer] += Amount;
                }
                else
                {
                    AmountAquiredLocally += Amount;
                }
            }
        }
    }
}
