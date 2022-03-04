using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.LogicObjects;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public static class ObjectExtentions
    {
        //ItemData
        public static ItemObject SearchPoolForMatchingItem(this ItemPool itemPool, string item)
        {
            var Item = itemPool.CurrentPool.FirstOrDefault(x => item == x.Id || x.AltItemNames.Contains(item));
            return Item;
        }
        public static LogicMapping GetLogicItemMapping(this TrackerInstance Instance, string entry)
        {
            if (Instance.InstanceReference.LogicItemMappings.ContainsKey(entry))
            {
                return Instance.InstanceReference.LogicItemMappings[entry];
            }
            else if(entry.StartsWith("'") && entry.EndsWith("'"))
            {
                string CleanedEntry = entry[1..^1];
                if (Instance.InstanceReference.LogicItemMappings.ContainsKey(CleanedEntry))
                {
                    return Instance.InstanceReference.LogicItemMappings[CleanedEntry];
                }
            }
            return null;
        }

        public static int GetAmountPlaced(this ItemObject itemObject, LogicObjects.TrackerInstance Instance)
        {
            int AmountAquired = itemObject.GetTotalUsable();
            int AmountSetAtLocation = 0;
            foreach(var x in Instance.LocationPool.Locations.Where(x => x.TrackerData.CheckState != CheckState.Checked))
            {
                var SetItem = x.TrackerData.GetItemAtCheck();
                if (SetItem != null && SetItem == itemObject.Id) { AmountSetAtLocation++; }
            }

            return AmountAquired + AmountSetAtLocation;
        }

        public static bool CanBePlaced(this ItemObject itemObject, LogicObjects.TrackerInstance Instance)
        {
            if (itemObject.MaxAmountInPool < 0) { return true; }
            return itemObject.GetAmountPlaced(Instance) < itemObject.MaxAmountInPool;
        }

        public static int GetTotalUsable(this ItemObject itemObject)
        {
            var AmountAquiredOnline = 0;
            foreach(var i in itemObject.AmountAquiredOnline)
            {
                AmountAquiredOnline += i.Value;
            }

            return itemObject.AmountAquiredLocally + AmountAquiredOnline + itemObject.AmountInStartingpool;
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
        public static LogicMapping GetLogicLocationMapping(this TrackerInstance Instance, string entry)
        {
            if (Instance.InstanceReference.LogicLocationMappings.ContainsKey(entry))
            {
                return Instance.InstanceReference.LogicLocationMappings[entry];
            }
            return null;
        }

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
                if (!CheckItem(data, NewState, Instance)) { return; }
            }
            else
            {
                if (!ToggleMarked(data, NewState, Instance)) { return; }
            }
            data.CheckState = NewState;
        }
        public static bool UncheckItem(this LocationData.LocationObjectData data, CheckState NewState, LogicObjects.TrackerInstance Instance)
        {
            var ItemObjectToAltar = Instance.ItemPool.SearchPoolForMatchingItem(data.AlteredItem);
            data.AlteredItem = null;

            if (ItemObjectToAltar != null)
            {
                ItemObjectToAltar.ChangeLocalItemAmounts(data, -1);
            }
            if (NewState == CheckState.Unchecked)
            {
                data.RandomizedItem = null;
            }


            return true;

        }
        public static bool CheckItem(this LocationData.LocationObjectData data, CheckState NewState, LogicObjects.TrackerInstance Instance)
        {
            if (string.IsNullOrWhiteSpace(data.RandomizedItem)) { return false; }

            var ItemObjectToAltar = Instance.ItemPool.SearchPoolForMatchingItem(data.RandomizedItem);

            if (ItemObjectToAltar != null)
            {
                ItemObjectToAltar.ChangeLocalItemAmounts(data, 1);
                data.AlteredItem = data.RandomizedItem;
            }

            return true;
        }
        public static bool ToggleMarked(this LocationData.LocationObjectData data, CheckState NewState, LogicObjects.TrackerInstance Instance)
        {
            if (NewState == CheckState.Marked && string.IsNullOrWhiteSpace(data.RandomizedItem))
            {
                return false;
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

        //Logic Map

        public static bool GetMappedEntryUsable(this LogicMapping mapping, TrackerInstance instance, int AmmountNeeded = 1)
        {
            if (mapping.logicEntryType == LogicEntryType.item)
            {
                var Item = instance.ItemPool.CurrentPool[mapping.IndexInList];
                return Item.Useable(AmmountNeeded);
            }
            else if (mapping.logicEntryType == LogicEntryType.macro)
            {
                var Macro = instance.Macros.MacroList[mapping.IndexInList];
                return Macro.Aquired;
            }
            else
            {
                return false;
            }
        }
        public static bool GetMappedEntryAvailable(this LogicMapping mapping, TrackerInstance instance, int AmmountNeeded = 1)
        {
            if (mapping.logicEntryType == LogicEntryType.location)
            {
                var Item = instance.LocationPool.Locations[mapping.IndexInList];
                return Item.TrackerData.Available;
            }
            else
            {
                return false;
            }
        }
        public static string GetMappedEntryID(this LogicMapping mapping, TrackerInstance instance, int AmmountNeeded = 1)
        {
            if (mapping.logicEntryType == LogicEntryType.item)
            {
                return instance.ItemPool.CurrentPool[mapping.IndexInList].Id;
            }
            else if (mapping.logicEntryType == LogicEntryType.location)
            {
                return instance.LocationPool.Locations[mapping.IndexInList].LogicData.Id;
            }
            else if (mapping.logicEntryType == LogicEntryType.macro)
            {
                return instance.Macros.MacroList[mapping.IndexInList].LogicData.Id;
            }
            else
            {
                return null;
            }
        }

        public static object GetMappedEntry(this LogicMapping mapping, TrackerInstance instance)
        {
            if (mapping.logicEntryType == LogicEntryType.item)
            {
                return instance.ItemPool.CurrentPool[mapping.IndexInList];
            }
            else if (mapping.logicEntryType == LogicEntryType.location)
            {
                return instance.LocationPool.Locations[mapping.IndexInList];
            }
            else if (mapping.logicEntryType == LogicEntryType.macro)
            {
                return instance.Macros.MacroList[mapping.IndexInList];
            }
            else if (mapping.logicEntryType == LogicEntryType.Hint)
            {
                return instance.HintPool.Hints[mapping.IndexInList];
            }
            else
            {
                return false;
            }
        }
    }
}
