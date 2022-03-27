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

            public int GetAmountInStartingPool(LogicObjects.TrackerInstance Instance)
            {
                int AmountWorth = GetDictEntry(Instance).AmountWorth ?? 1;
                return AmountInStartingpool * AmountWorth;
            }

            public bool CanBePlaced(LogicObjects.TrackerInstance Instance)
            {
                if (GetDictEntry(Instance).MaxAmountInWorld < 0) { return true; }
                return GetAmountPlaced(Instance) < GetDictEntry(Instance).MaxAmountInWorld;
            }

            public int GetTotalUsable(LogicObjects.TrackerInstance Instance)
            {
                return AmountAquiredLocally + AmountAquiredOnline.Values.Sum() + GetAmountInStartingPool(Instance);
            }

            public bool Useable(LogicObjects.TrackerInstance Instance, int Amount = 1)
            {
                return GetTotalUsable(Instance) >= Amount;
            }

            public void ChangeLocalItemAmounts(LogicObjects.TrackerInstance Instance, LocationData.LocationObject location, int Amount)
            {
                if (Amount == 0) { return; }
                ParseItemRefernce(Amount, Instance, out ItemObject ItemToAlter, out int AmountToAlter);
                if (location.Randomizeditem.OwningPlayer != -1)
                {
                    if (!ItemToAlter.AmountSentToPlayer.ContainsKey(location.Randomizeditem.OwningPlayer))
                    {
                        ItemToAlter.AmountSentToPlayer.Add(location.Randomizeditem.OwningPlayer, 0);
                    }
                    ItemToAlter.AmountSentToPlayer[location.Randomizeditem.OwningPlayer] += AmountToAlter;
                }
                else
                {
                    //Debug.WriteLine($"{location.ID} Was checked for {AmountToAlter} and contained {this.Id}");
                    //Debug.WriteLine($"{ItemToAlter.Id} Will be altered. Current Amount {ItemToAlter.AmountAquiredLocally}");
                    ItemToAlter.AmountAquiredLocally += AmountToAlter;
                    //Debug.WriteLine($"{ItemToAlter.Id} altered. New Amount {ItemToAlter.AmountAquiredLocally}");
                }
            }

            public void ParseItemRefernce(int BaseAmount, LogicObjects.TrackerInstance Instance, out ItemObject RefItemObject, out int Amount)
            {
                var dictEntry = GetDictEntry(Instance);
                RefItemObject = this;
                Amount = BaseAmount;
                if (dictEntry.ItemReference != null && Instance.GetItemByID(dictEntry.ItemReference) != null)
                {
                    RefItemObject = Instance.GetItemByID(dictEntry.ItemReference);
                }
                if (dictEntry.AmountWorth != null)
                {
                    Amount = (int)dictEntry.AmountWorth * BaseAmount;
                }
            }
        }
    }
}
