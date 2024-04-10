using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using System.Linq;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjectExtensions
{
    public static class LocationObjectExtensions
    {
        public static bool CanContainItem(this LocationData.LocationObject loc, ItemData.ItemObject item, bool EmptyIsWildcard = true)
        {
            var LocTypes = loc?.GetDictEntry()?.ValidItemTypes;
            var itemTypes = item?.GetDictEntry()?.ItemTypes;
            if (LocTypes is null || itemTypes is null) { return EmptyIsWildcard; }
            return LocTypes.Intersect(itemTypes).Any();
        }

        public static bool IsRepeatable(this LocationData.LocationObject loc)
        {
            return loc.GetDictEntry().Repeatable is not null && (bool)loc.GetDictEntry().Repeatable;
        }
        public static bool CanBeUnrandomized(this LocationData.LocationObject loc)
        {
            //If it's already unrandomized let it continue to be
            if (loc.IsUnrandomized()) { return true; }
            string OriginalItem = loc.GetDictEntry().OriginalItem ?? "";
            var OriginalItemObject = loc.GetParent().GetItemByID(OriginalItem);
            //IF the original item is not a valid item or is blank it can't be unrandomized
            if (OriginalItemObject == null) { return false; }
            //If the check was already given it's vanilla item through the spoiler log or manually it can be unrandomized
            //This gets around a quirk caused by "CanBePlaced" not being smart enough to know to ignore the item 
            //assigned to this check when checking to see if the max amount has been placed
            if (loc.GetItemAtCheck() is not null && loc.GetItemAtCheck() == OriginalItemObject.ID) { return true; }
            //If the max amount of this object has been placed return false, otherwise true
            return OriginalItemObject.CanBePlaced();
        }

        public static bool ToggleChecked(this LocationData.LocationObject loc, CheckState NewState)
        {
            CheckState CurrentState = loc.CheckState;
            if (CurrentState == NewState)
            {
                return false;
            }
            else if (CurrentState == CheckState.Checked)
            {
                loc.UncheckItem(NewState);
            }
            else if (NewState == CheckState.Checked)
            {
                if (!loc.CheckItem(NewState)) { return false; }
            }
            else
            {
                if (!loc.ToggleMarked(NewState)) { return false; }
            }
            loc.CheckState = NewState;
            return true;
        }

        public static bool UncheckItem(this LocationData.LocationObject loc, CheckState NewState)
        {
            var ItemAtCheck = loc.GetParent().GetItemByID(loc.Randomizeditem.Item);

            ItemAtCheck?.ChangeLocalItemAmounts(loc, -1);
            if (NewState == CheckState.Unchecked)
            {
                loc.Randomizeditem.Item = null;
            }
            return true;

        }

        public static bool CheckItem(this LocationData.LocationObject loc, CheckState NewState)
        {
            if (string.IsNullOrWhiteSpace(loc.Randomizeditem.Item)) { return false; }

            var ItemAtCheck = loc.GetParent().GetItemByID(loc.Randomizeditem.Item);
            if (ItemAtCheck != null)
            {
                ItemAtCheck.ChangeLocalItemAmounts(loc, 1);
            }
            return true;
        }

        public static bool ToggleMarked(this LocationData.LocationObject loc, CheckState NewState)
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

        public static string GetItemAtCheck(this LocationData.LocationObject loc)
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
            if ((loc.IsUnrandomized()) && loc.GetDictEntry().OriginalItem != null)
            {
                ItemAtCheck = loc.GetDictEntry().OriginalItem;
            }
            return ItemAtCheck;
        }
    }
}
