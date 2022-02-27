using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public static class ObjectExtentions
    {
        //ItemData
        public static ItemObject GetItemByString(this ItemPool itemPool, string item)
        {
            var Item = itemPool.CurrentPool.First(x => item == x.Id || item == x.ItemName || x.AltItemNames.Contains(item));
            return Item;
        }

        public static int GetAmountPlaced(this ItemObject itemObject, LogicObjects.TrackerInstance Instance)
        {
            return Instance.LocationPool.Locations.Where(x => x.TrackerData.RandomizedItem != null && Instance.ItemPool.GetItemByString(x.TrackerData.RandomizedItem) == itemObject).Count();
        }

        public static bool CanBePlaced(this ItemObject itemObject, LogicObjects.TrackerInstance Instance)
        {
            if (itemObject.MaxAmountInPool < 0) { return true; }
            return itemObject.GetAmountPlaced(Instance) < itemObject.MaxAmountInPool;
        }

        public static int GetTotalUsable(this ItemObject itemObject)
        {
            return itemObject.AmountAquiredLocally + itemObject.AmountAquiredOnline + itemObject.AmountInStartingpool;
        }

        public static bool Useable(this ItemObject itemObject, int Amount = 1)
        {
            return itemObject.GetTotalUsable() >= Amount;
        }

        public static void ChangeLocalItemAmounts(this ItemObject itemObject, LocationData.LocationObjectData location, int Amount)
        {
            if (Amount != 0)
            {
                if (location.ItemBelongedToPlayer != -1)
                {
                    if (!itemObject.AmountSentToPlayer.ContainsKey(location.ItemBelongedToPlayer)) 
                    { 
                        itemObject.AmountSentToPlayer.Add(location.ItemBelongedToPlayer, 0); 
                    }
                    itemObject.AmountSentToPlayer[location.ItemBelongedToPlayer] += Amount;
                }
                else
                {
                    itemObject.AmountAquiredLocally += Amount;
                }
            }
        }

        //Location Data

        public static void ToggleChecked(this LocationData.LocationObjectData data, CheckState NewState, LogicObjects.TrackerInstance Instance)
        {
            CheckState CurrentState = data.CheckState;
            if (CurrentState == NewState) 
            {
                return; 
            }
            else if (CurrentState == CheckState.Checked)
            {
                UncheckItem(data, NewState, Instance);
            }
            else if (NewState == CheckState.Checked)
            {
                if (!CheckItem(data, NewState, Instance))
                {
                    return;
                }
            }
            else
            {
                ToggleMarked(data, NewState, Instance);
            }
            data.CheckState = NewState;
        }
        public static bool UncheckItem(this LocationData.LocationObjectData data, CheckState NewState, LogicObjects.TrackerInstance Instance)
        {
            var ItemObjectToAltar = Instance.ItemPool.GetItemByString(data.AlteredItem);
            ItemObjectToAltar.ChangeLocalItemAmounts(data, -1);

            if (NewState == CheckState.Unchecked)
            {
                data.RandomizedItem = null;
            }

            return true;

        }
        public static bool CheckItem(this LocationData.LocationObjectData data, CheckState NewState, LogicObjects.TrackerInstance Instance)
        {
            var ItemToAlter = data.GetItemAtCheck();

            var ItemObjectToAltar = Instance.ItemPool.GetItemByString(ItemToAlter);

            if (ItemObjectToAltar == null) { return false; }

            ItemObjectToAltar.ChangeLocalItemAmounts(data, 1);
            data.AlteredItem = ItemToAlter;

            return true;
        }
        public static bool ToggleMarked(this LocationData.LocationObjectData data, CheckState NewState, LogicObjects.TrackerInstance Instance)
        {
            if (NewState == CheckState.Marked)
            {
                data.RandomizedItem = data.GetItemAtCheck();
            }
            else if (NewState == CheckState.Unchecked)
            {
                data.RandomizedItem = null;
            }
            return true;
        }

        public static string GetItemAtCheck(this LocationData.LocationObjectData data)
        {
            var ItemAtCheck = data.RandomizedItem;
            if (data.SpoilerLogGivenItem != null)
            {
                ItemAtCheck = data.SpoilerLogGivenItem;
            }
            if ((data.RandomizedState == RandomizedState.Unrandomized || data.RandomizedState == RandomizedState.UnrandomizedManual) && data.VanillaItem != null)
            {
                ItemAtCheck = data.VanillaItem;
            }
            if (data.AlteredItem != null)
            {
                ItemAtCheck = data.AlteredItem;
            }
            return ItemAtCheck;
        }
    }
}
