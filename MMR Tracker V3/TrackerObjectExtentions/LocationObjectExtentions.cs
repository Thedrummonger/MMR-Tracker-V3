using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjectExtentions
{
    public static class LocationObjectExtentions
    {
        public static bool CanContainItem(this LocationData.LocationObject loc, ItemData.ItemObject item, TrackerInstance instance, bool EmptyIsWildcard = true)
        {
            var LocTypes = loc?.GetDictEntry(instance)?.ValidItemTypes;
            var itemTypes = item?.GetDictEntry(instance)?.ItemTypes;
            if (LocTypes is null || itemTypes is null) { return EmptyIsWildcard; }
            return LocTypes.Intersect(itemTypes).Any();
        }

        public static bool IsRepeatable(this LocationData.LocationObject loc, InstanceData.TrackerInstance Instance)
        {
            return loc.GetDictEntry(Instance).Repeatable is not null && (bool)loc.GetDictEntry(Instance).Repeatable;
        }

        public static void SetRandomizedState(this LocationData.LocationObject loc, RandomizedState Newstate, InstanceData.TrackerInstance Instance)
        {
            if (Newstate == loc.RandomizedState) { return; }
            loc.RandomizedState = Newstate;
        }
        public static bool IsUnrandomized(this LocationData.LocationObject loc, UnrandState Include = UnrandState.Any)
        {
            if ((Include == UnrandState.Any || Include == UnrandState.Unrand) && loc.RandomizedState == RandomizedState.Unrandomized) { return true; }
            if ((Include == UnrandState.Any || Include == UnrandState.Manual) && loc.RandomizedState == RandomizedState.UnrandomizedManual) { return true; }
            return false;
        }
        public static bool IsRandomized(this LocationData.LocationObject loc)
        {
            return loc.RandomizedState == RandomizedState.Randomized;
        }
        public static bool IsJunk(this LocationData.LocationObject loc)
        {
            return loc.RandomizedState == RandomizedState.ForcedJunk;
        }
        public static bool CanBeUnrandomized(this LocationData.LocationObject loc, InstanceData.TrackerInstance instance)
        {
            //If it's already unrandomized let it continue to be
            if (loc.IsUnrandomized()) { return true; }
            string OriginalItem = loc.GetDictEntry(instance).OriginalItem ?? "";
            var OriginalItemObject = instance.GetItemByID(OriginalItem);
            //IF the original item is not a valid item or is blank it can't be unrandomized
            if (OriginalItemObject == null) { return false; }
            //If the check was already given it's vanilla item through the spoiler log or manually it can be unrandomized
            //This gets around a quirk caused by "CanBePlaced" not being smart enough to know to ignore the item 
            //assigned to this check when checking to see if the max amount has been placed
            if (loc.GetItemAtCheck(instance) is not null && loc.GetItemAtCheck(instance) == OriginalItemObject.ID) { return true; }
            //If the max amount of this object has been placed return false, otherwise true
            return OriginalItemObject.CanBePlaced(instance);
        }
        public static bool AppearsinListbox(this LocationData.LocationObject loc, InstanceData.TrackerInstance Instance, bool ShowJunkUnrand = false)
        {
            return (!loc.IsJunk() || ShowJunkUnrand) && (!loc.IsUnrandomized(MiscData.UnrandState.Unrand) || ShowJunkUnrand) && !string.IsNullOrWhiteSpace(loc.GetDictEntry(Instance).GetName(Instance));
        }

        public static bool ToggleChecked(this LocationData.LocationObject loc, CheckState NewState, InstanceData.TrackerInstance Instance)
        {
            CheckState CurrentState = loc.CheckState;
            if (CurrentState == NewState)
            {
                return false;
            }
            else if (CurrentState == CheckState.Checked)
            {
                loc.UncheckItem(NewState, Instance);
            }
            else if (NewState == CheckState.Checked)
            {
                if (!loc.CheckItem(NewState, Instance)) { return false; }
            }
            else
            {
                if (!loc.ToggleMarked(NewState, Instance)) { return false; }
            }
            loc.CheckState = NewState;
            return true;
        }

        public static bool UncheckItem(this LocationData.LocationObject loc, CheckState NewState, InstanceData.TrackerInstance Instance)
        {
            var ItemAtCheck = Instance.GetItemByID(loc.Randomizeditem.Item);

            if (ItemAtCheck != null)
            {
                ItemAtCheck.ChangeLocalItemAmounts(Instance, loc, -1);
            }
            if (NewState == CheckState.Unchecked)
            {
                loc.Randomizeditem.Item = null;
            }
            return true;

        }

        public static bool CheckItem(this LocationData.LocationObject loc, CheckState NewState, InstanceData.TrackerInstance Instance)
        {
            if (string.IsNullOrWhiteSpace(loc.Randomizeditem.Item)) { return false; }

            var ItemAtCheck = Instance.GetItemByID(loc.Randomizeditem.Item);
            if (ItemAtCheck != null)
            {
                ItemAtCheck.ChangeLocalItemAmounts(Instance, loc, 1);
            }
            return true;
        }

        public static bool ToggleMarked(this LocationData.LocationObject loc, CheckState NewState, InstanceData.TrackerInstance Instance)
        {
            if (NewState == CheckState.Marked && string.IsNullOrWhiteSpace(loc.Randomizeditem.Item))
            {
                return false;
            }
            else if (NewState == CheckState.Unchecked)
            {
                loc.Randomizeditem.Item = null;
            }
            return true;
        }

        public static string GetItemAtCheck(this LocationData.LocationObject loc, InstanceData.TrackerInstance Instance)
        {
            var ItemAtCheck = loc.Randomizeditem.Item;
            if (loc.SingleValidItem != null)
            {
                ItemAtCheck = loc.SingleValidItem;
            }
            if (loc.Randomizeditem.SpoilerLogGivenItem != null)
            {
                ItemAtCheck = loc.Randomizeditem.SpoilerLogGivenItem;
            }
            if ((loc.IsUnrandomized()) && loc.GetDictEntry(Instance).OriginalItem != null)
            {
                ItemAtCheck = loc.GetDictEntry(Instance).OriginalItem;
            }
            return ItemAtCheck;
        }
    }
}
