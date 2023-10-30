using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjectExtentions
{
    public static class ItemObjectExtentions
    {
        public static bool ValidStartingItem(this ItemData.ItemObject Item, InstanceData.TrackerInstance Instance)
        {
            var ItemIndex = Instance.GetItemByID(Item.Id);
            var DictEntry = ItemIndex.GetDictEntry(Instance);
            return DictEntry.ValidStartingItem != null && (bool)DictEntry.ValidStartingItem;
        }

        public static int GetAmountPlaced(this ItemData.ItemObject Item, InstanceData.TrackerInstance Instance)
        {
            int AmountAquired = Item.GetTotalUsable(Instance);
            int AmountSetAtLocation = 0;
            foreach (var x in Instance.LocationPool.Where(x => x.Value.CheckState != CheckState.Checked))
            {
                var itemAtheck = x.Value.GetItemAtCheck(Instance) ?? "";
                if (itemAtheck == Item.Id) { AmountSetAtLocation++; }
            }

            return AmountAquired + AmountSetAtLocation;
        }

        public static int GetAmountInStartingPool(this ItemData.ItemObject Item)
        {
            return Item.AmountInStartingpool;
        }

        public static bool CanBePlaced(this ItemData.ItemObject Item, InstanceData.TrackerInstance Instance)
        {
            if (Item.GetDictEntry(Instance).GetMaxAmountInWorld(Instance) < 0) { return true; }
            return Item.GetAmountPlaced(Instance) < Item.GetDictEntry(Instance).GetMaxAmountInWorld(Instance);
        }

        public static int GetTotalUsable(this ItemData.ItemObject Item, InstanceData.TrackerInstance Instance)
        {
            return Item.AmountAquiredLocally + Item.AmountAquiredOnline.Values.Sum() + Item.GetAmountInStartingPool();
        }

        public static bool Useable(this ItemData.ItemObject Item, InstanceData.TrackerInstance Instance, int Amount = 1)
        {
            return Item.GetTotalUsable(Instance) >= Amount;
        }

        public static void ChangeLocalItemAmounts(this ItemData.ItemObject Item, InstanceData.TrackerInstance Instance, LocationData.LocationObject location, int Amount)
        {
            if (Amount == 0) { return; }
            if (location.Randomizeditem.OwningPlayer > -1)
            {
                if (!Item.AmountSentToPlayer.ContainsKey(location.Randomizeditem.OwningPlayer))
                {
                    Item.AmountSentToPlayer.Add(location.Randomizeditem.OwningPlayer, 0);
                }
                Item.AmountSentToPlayer[location.Randomizeditem.OwningPlayer] += Amount;
            }
            else
            {
                Item.AmountAquiredLocally += Amount;
            }
        }
    }
}
