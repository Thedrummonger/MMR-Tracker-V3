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


            public override string ToString()
            {
                return DisplayName ?? Id;
            }
            public LogicDictionaryData.DictionaryItemEntries GetDictEntry(InstanceData.TrackerInstance Instance)
            {
                return Instance.LogicDictionary.ItemList[Id];
            }
            public bool ValidStartingItem(InstanceData.TrackerInstance Instance)
            {
                var ItemIndex = Instance.GetItemByID(Id);
                var DictEntry = ItemIndex.GetDictEntry(Instance);
                return DictEntry.ValidStartingItem != null && (bool)DictEntry.ValidStartingItem;
            }

            public int GetAmountPlaced(InstanceData.TrackerInstance Instance)
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

            public bool CanBePlaced(InstanceData.TrackerInstance Instance)
            {
                if (GetDictEntry(Instance).GetMaxAmountInWorld(Instance) < 0) { return true; }
                return GetAmountPlaced(Instance) < GetDictEntry(Instance).GetMaxAmountInWorld(Instance);
            }

            public int GetTotalUsable(InstanceData.TrackerInstance Instance)
            {
                return AmountAquiredLocally + AmountAquiredOnline.Values.Sum() + GetAmountInStartingPool();
            }

            public bool Useable(InstanceData.TrackerInstance Instance, int Amount = 1)
            {
                return GetTotalUsable(Instance) >= Amount;
            }

            public void ChangeLocalItemAmounts(InstanceData.TrackerInstance Instance, LocationData.LocationObject location, int Amount)
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
