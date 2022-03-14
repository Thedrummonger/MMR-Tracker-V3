using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.LogicObjects;
using static MMR_Tracker_V3.TrackerObjects.EntranceData;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public static class ObjectExtentions
    {
        //ItemData
        public static ItemObject GetItemByID(this LogicObjects.TrackerInstance instance, string item, string Source = "")
        {
            if (item == null) { Debug.WriteLine(Source); }
            if (!instance.ItemPool.ContainsKey(item)) { return null; }
            return instance.ItemPool[item];
        }

        public static LocationData.LocationObject GetLocationByID(this LogicObjects.TrackerInstance instance, string item)
        {
            if (!instance.LocationPool.ContainsKey(item)) { return null; }
            return instance.LocationPool[item];
        }

        public static MacroObject GetMacroByID(this TrackerInstance instance, string item)
        {
            if (!instance.MacroPool.ContainsKey(item)) { return null; }
            return instance.MacroPool[item];
        }

        public static OptionData.TrackerOption GetTrackerOptionByID(this LogicObjects.TrackerInstance instance, string item)
        {
            if (!instance.UserOptions.ContainsKey(item)) { return null; }
            return instance.UserOptions[item];
        }

        public static bool CanBeUnrandomized(this LocationData.LocationObject locationObject, LogicObjects.TrackerInstance instance)
        {
            if (locationObject.IsUnrandomized()) { return true; }
            string OriginalItem = locationObject.GetDictEntry(instance).OriginalItem ?? "";
            var OriginalItemObject = instance.GetItemByID(OriginalItem);
            if (OriginalItemObject == null) { return false; }
            return OriginalItemObject.CanBePlaced(instance);
        }

        public static bool IsLiteralID(this string ID, out string CleanedID)
        {
            bool Literal = false;
            CleanedID = ID;
            if (ID.StartsWith("'") && ID.EndsWith("'"))
            {
                Literal = true;
                CleanedID = ID.Replace("'", "");
            }
            return Literal;
        }

        public static LogicEntryType GetLocationEntryType(this LogicObjects.TrackerInstance instance, string ID, bool literal = false)
        {
            if (literal && instance.LocationPool.ContainsKey(ID)) { return LogicEntryType.location; }
            if (literal && instance.InstanceReference.EntranceLogicNameToEntryData.ContainsKey(ID)) { return LogicEntryType.Exit; }
            if (literal && instance.HintPool.ContainsKey(ID)) { return LogicEntryType.Hint; }
            if (instance.MacroPool.ContainsKey(ID)) { return LogicEntryType.macro; }
            if (!literal && instance.LocationPool.ContainsKey(ID)) { return LogicEntryType.location; }
            if (!literal && instance.InstanceReference.EntranceLogicNameToEntryData.ContainsKey(ID)) { return LogicEntryType.Exit; }
            if (!literal && instance.HintPool.ContainsKey(ID)) { return LogicEntryType.Hint; }
            if (instance.UserOptions.ContainsKey(ID)) { return LogicEntryType.Option; }
            if (instance.Variables.ContainsKey(ID))
            {
                if (instance.Variables[ID].Value is string) { return LogicEntryType.variableString; }
                if (instance.Variables[ID].Value is bool) { return LogicEntryType.variableBool; }
                if (instance.Variables[ID].Value is Int64) { return LogicEntryType.variableInt; }
                if (instance.Variables[ID].Value is Newtonsoft.Json.Linq.JArray) { return LogicEntryType.variableList; }
            }
            return LogicEntryType.error;
        }

        public static LogicEntryType GetOptionEntryType(this LogicObjects.TrackerInstance instance, string ID, bool literal = false)
        {
            if (literal && instance.UserOptions.ContainsKey(ID)) { return LogicEntryType.Option; }
            if (instance.LocationPool.ContainsKey(ID)) { return LogicEntryType.location; }
            if (!literal && instance.UserOptions.ContainsKey(ID)) { return LogicEntryType.Option; }
            return LogicEntryType.error;
        }

        public static LogicEntryType GetItemEntryType(this LogicObjects.TrackerInstance instance, string OriginalID, bool literal = false)
        {
            LogicCalculation.MultipleItemEntry(instance, OriginalID, out string ID, out _);
            if (literal && instance.ItemPool.ContainsKey(ID)) { return LogicEntryType.item; }
            if (literal && instance.EntrancePool.AreaList.ContainsKey(ID)) { return LogicEntryType.Area; }
            if (instance.MacroPool.ContainsKey(ID)) { return LogicEntryType.macro; }
            if (!literal && instance.ItemPool.ContainsKey(ID)) { return LogicEntryType.item; }
            if (!literal && instance.EntrancePool.AreaList.ContainsKey(ID)) { return LogicEntryType.Area; }
            if (instance.Variables.ContainsKey(ID))
            {
                if (instance.Variables[ID].Value is string) { return LogicEntryType.variableString; }
                if (instance.Variables[ID].Value is bool) { return LogicEntryType.variableBool; }
                if (instance.Variables[ID].Value is Int64) { return LogicEntryType.variableInt; }
                if (instance.Variables[ID].Value is List<string>) { return LogicEntryType.variableList; }
            }
            if (bool.TryParse(ID, out _)) { return LogicEntryType.Bool; }
            if (LogicCalculation.LogicOptionEntry(instance, ID, out _)) { return LogicEntryType.Option; }
            return LogicEntryType.error;
        }

        public static int GetAmountPlaced(this ItemObject itemObject, LogicObjects.TrackerInstance Instance)
        {
            int AmountAquired = itemObject.GetTotalUsable();
            int AmountSetAtLocation = 0;
            foreach(var x in Instance.LocationPool.Where(x => x.Value.CheckState != CheckState.Checked))
            {
                var itemAtheck = x.Value.GetItemAtCheck(Instance)??"";
                if (itemAtheck == itemObject.Id) { AmountSetAtLocation++; }
            }

            return AmountAquired + AmountSetAtLocation;
        }

        public static bool CanBePlaced(this ItemObject itemObject, LogicObjects.TrackerInstance Instance)
        {
            if (itemObject.GetDictEntry(Instance).MaxAmountInWorld < 0) { return true; }
            return itemObject.GetAmountPlaced(Instance) < itemObject.GetDictEntry(Instance).MaxAmountInWorld;
        }

        public static int GetTotalUsable(this ItemObject itemObject)
        {
            var AmountAquiredOnline = itemObject.AmountAquiredOnline.Values.Sum();
            return itemObject.AmountAquiredLocally + AmountAquiredOnline + itemObject.AmountInStartingpool;
        }

        public static bool Useable(this ItemObject itemObject, int Amount = 1)
        {
            return itemObject.GetTotalUsable() >= Amount;
        }

        public static void ChangeLocalItemAmounts(this ItemObject itemObject, LocationData.LocationObject location, int Amount)
        {
            if (Amount != 0)
            {
                if (location.Randomizeditem.OwningPlayer != -1)
                {
                    if (!itemObject.AmountSentToPlayer.ContainsKey(location.Randomizeditem.OwningPlayer)) 
                    { 
                        itemObject.AmountSentToPlayer.Add(location.Randomizeditem.OwningPlayer, 0); 
                    }
                    itemObject.AmountSentToPlayer[location.Randomizeditem.OwningPlayer] += Amount;
                }
                else
                {
                    itemObject.AmountAquiredLocally += Amount;
                }
            }
        }

        public static bool ToggleExitChecked(this EntranceData.EntranceRandoExit data, CheckState NewState, LogicObjects.TrackerInstance Instance)
        {
            CheckState CurrentState = data.CheckState;
            if (CurrentState == NewState)
            {
                return false;
            }
            else if (CurrentState == CheckState.Checked)
            {
                var Destination = Instance.EntrancePool.AreaList[data.DestinationExit.region];
                Destination.ExitsAcessibleFrom--;
            }
            else if (NewState == CheckState.Checked)
            {
                if (data.DestinationExit == null) { return false; }
                var Destination = Instance.EntrancePool.AreaList[data.DestinationExit.region];
                Destination.ExitsAcessibleFrom++;
            }
            else if (CurrentState == CheckState.Unchecked && NewState == CheckState.Marked)
            {
                if (data.DestinationExit == null) { return false; }
            }

            if (NewState == CheckState.Unchecked) { data.DestinationExit = null; }
            data.CheckState = NewState;
            return true;
        }

        public static bool ToggleChecked(this LocationData.LocationObject data, CheckState NewState, LogicObjects.TrackerInstance Instance)
        {
            CheckState CurrentState = data.CheckState;
            if (CurrentState == NewState) 
            {
                return false; 
            }
            else if (CurrentState == CheckState.Checked)
            {
                UncheckItem(data, NewState, Instance);
            }
            else if (NewState == CheckState.Checked)
            {
                if (!CheckItem(data, NewState, Instance)) { return false; }
            }
            else
            {
                if (!ToggleMarked(data, NewState, Instance)) { return false; }
            }
            data.CheckState = NewState;
            return true;
        }

        public static bool UncheckItem(this LocationData.LocationObject data, CheckState NewState, LogicObjects.TrackerInstance Instance)
        {
            var ItemObjectToAltar = Instance.GetItemByID(data.Randomizeditem.Item);

            if (ItemObjectToAltar != null)
            {
                ItemObjectToAltar.ChangeLocalItemAmounts(data, -1);
            }
            if (NewState == CheckState.Unchecked)
            {
                data.Randomizeditem.Item = null;
            }


            return true;

        }

        public static bool CheckItem(this LocationData.LocationObject data, CheckState NewState, LogicObjects.TrackerInstance Instance)
        {
            if (string.IsNullOrWhiteSpace(data.Randomizeditem.Item)) { return false; }

            var ItemObjectToAltar = Instance.GetItemByID(data.Randomizeditem.Item);
            if (ItemObjectToAltar != null) { ItemObjectToAltar.ChangeLocalItemAmounts(data, 1); }

            return true;
        }

        public static bool ToggleMarked(this LocationData.LocationObject data, CheckState NewState, LogicObjects.TrackerInstance Instance)
        {
            if (NewState == CheckState.Marked && string.IsNullOrWhiteSpace(data.Randomizeditem.Item))
            {
                return false;
            }
            else if (NewState == CheckState.Unchecked)
            {
                data.Randomizeditem.Item = null;
            }
            return true;
        }

        public static string GetItemAtCheck(this LocationData.LocationObject data, LogicObjects.TrackerInstance Instance)
        {
            var ItemAtCheck = data.Randomizeditem.Item;
            if (data.SingleValidItem != null)
            {
                ItemAtCheck = data.SingleValidItem;
            }
            if (data.Randomizeditem.SpoilerLogGivenItem != null)
            {
                ItemAtCheck = data.Randomizeditem.SpoilerLogGivenItem;
            }
            if ((data.IsUnrandomized()) && data.GetDictEntry(Instance).OriginalItem != null)
            {
                ItemAtCheck = data.GetDictEntry(Instance).OriginalItem;
            }
            return ItemAtCheck;
        }

        public static EntranceRandoDestination GetDestinationAtExit(this EntranceRandoExit Exit, LogicObjects.TrackerInstance currentTrackerInstance)
        {
            var DestinationAtCheck = Exit.DestinationExit;
            if (Exit.SpoilerDefinedDestinationExit != null)
            {
                DestinationAtCheck = Exit.SpoilerDefinedDestinationExit;
            }
            if ((Exit.IsUnrandomized()))
            {
                DestinationAtCheck = new EntranceRandoDestination { region = Exit.ID, from = Exit.ParentAreaID };
            }
            return DestinationAtCheck;
        }
    }
}
