using MMR_Tracker_V3.TrackerObjects;
using System.Linq;
using TDMUtils;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjectExtensions
{
    public static class ItemObjectExtensions
    {
        public static bool ValidStartingItem(this ItemData.ItemObject Item)
        {
            var DictEntry = Item.GetDictEntry();
            return DictEntry.ValidStartingItem != null && (bool)DictEntry.ValidStartingItem;
        }

        public static int GetAmountPlaced(this ItemData.ItemObject Item)
        {
            int AmountAquired = Item.GetTotalUsable();
            int AmountSetAtLocation = 0;
            foreach (var x in Item.GetParent().LocationPool.Where(x => x.Value.CheckState != CheckState.Checked))
            {
                var itemAtheck = x.Value.GetItemAtCheck() ?? "";
                if (itemAtheck == Item.ID && x.Value.IsOwnedByLocalPlayer()) { AmountSetAtLocation++; }
            }

            return AmountAquired + AmountSetAtLocation;
        }

        public static int GetAmountInStartingPool(this ObtainableObject Item)
        {
            return Item.AmountInStartingpool;
        }

        public static bool CanBePlaced(this ItemData.ItemObject Item)
        {
            if (Item.GetDictEntry().GetMaxAmountInWorld() < 0) { return true; }
            return Item.GetAmountPlaced() < Item.GetDictEntry().GetMaxAmountInWorld();
        }

        public static int GetAmountLeftToPlace(this ItemData.ItemObject Item)
        {
            if (Item.GetDictEntry().GetMaxAmountInWorld() < 0) { return int.MaxValue; }
            return Item.GetDictEntry().GetMaxAmountInWorld() - Item.GetAmountPlaced();
        }

        public static int GetTotalUsable(this ObtainableObject Item)
        {
            return Item.AmountAquiredLocally + Item.AmountAquiredOnline.Values.Sum() + Item.GetAmountInStartingPool();
        }

        public static bool Useable(this ObtainableObject Item, int Amount = 1)
        {
            return Item.GetTotalUsable() >= Amount;
        }

        public static void ChangeLocalItemAmounts(this ObtainableObject Item, CheckableLocation location, int Amount)
        {
            if (Amount == 0) { return; }
            if (Item is ItemData.ItemObject IO && location is LocationData.LocationObject LO && !LO.IsOwnedByLocalPlayer())
            {
                IO.AmountSentToPlayer.SetIfEmpty(LO.Randomizeditem.OwningPlayer, 0);
                IO.AmountSentToPlayer[LO.Randomizeditem.OwningPlayer] += Amount;
                return;
            }
            Item.AmountAquiredLocally += Amount;
        }
    }
}
