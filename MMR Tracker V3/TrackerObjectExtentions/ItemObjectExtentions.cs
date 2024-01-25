using MMR_Tracker_V3.TrackerObjects;
using System.Linq;
using static MMR_Tracker_V3.DataStructure.MiscData;

namespace MMR_Tracker_V3.TrackerObjectExtentions
{
    public static class ItemObjectExtentions
    {
        public static bool ValidStartingItem(this ItemData.ItemObject Item)
        {
            var ItemIndex = Item.GetParent().GetItemByID(Item.ID);
            var DictEntry = ItemIndex.GetDictEntry();
            return DictEntry.ValidStartingItem != null && (bool)DictEntry.ValidStartingItem;
        }

        public static int GetAmountPlaced(this ItemData.ItemObject Item)
        {
            int AmountAquired = Item.GetTotalUsable();
            int AmountSetAtLocation = 0;
            foreach (var x in Item.GetParent().LocationPool.Where(x => x.Value.CheckState != CheckState.Checked))
            {
                bool OwnedByLocalPlayer = x.Value.Randomizeditem.OwningPlayer < 0 || Item.GetParent().GetParentContainer().netConnection.PlayerID == x.Value.Randomizeditem.OwningPlayer;
                var itemAtheck = x.Value.GetItemAtCheck() ?? "";
                if (itemAtheck == Item.ID && OwnedByLocalPlayer) { AmountSetAtLocation++; }
            }

            return AmountAquired + AmountSetAtLocation;
        }

        public static int GetAmountInStartingPool(this ItemData.ItemObject Item)
        {
            return Item.AmountInStartingpool;
        }

        public static bool CanBePlaced(this ItemData.ItemObject Item)
        {
            if (Item.GetDictEntry().GetMaxAmountInWorld() < 0) { return true; }
            return Item.GetAmountPlaced() < Item.GetDictEntry().GetMaxAmountInWorld();
        }

        public static int GetTotalUsable(this ItemData.ItemObject Item)
        {
            return Item.AmountAquiredLocally + Item.AmountAquiredOnline.Values.Sum() + Item.GetAmountInStartingPool();
        }

        public static bool Useable(this ItemData.ItemObject Item, int Amount = 1)
        {
            return Item.GetTotalUsable() >= Amount;
        }

        public static void ChangeLocalItemAmounts(this ItemData.ItemObject Item, LocationData.LocationObject location, int Amount)
        {
            if (Amount == 0) { return; }
            bool OwnedByLocalPlayer = location.Randomizeditem.OwningPlayer < 0 || Item.GetParent().GetParentContainer().netConnection.PlayerID == location.Randomizeditem.OwningPlayer;
            if (!OwnedByLocalPlayer)
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
