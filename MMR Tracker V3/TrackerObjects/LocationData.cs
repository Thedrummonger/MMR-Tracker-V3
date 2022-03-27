using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.MMRData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class LocationData
    {
        [Serializable]
        public class LocationObject
        {
            public string ID { get; set; }
            public RandomizeditemData Randomizeditem { get; set; } = new RandomizeditemData();
            public bool Available { get; set; } = false;
            public CheckState CheckState { get; set; } = CheckState.Unchecked;
            public bool Starred { get; set; }
            public int CheckPrice { get; set; } = -1;
            public string DisplayName { get; set; }
            public RandomizedState RandomizedState { get; set; } = RandomizedState.Randomized;
            public string SingleValidItem { get; set; } = null;


            public override string ToString()
            {
                return DisplayName ?? ID;
            }

            public void SetRandomizedState(RandomizedState Newstate, LogicObjects.TrackerInstance Instance)
            {
                if (Newstate == RandomizedState) { return; }
                RandomizedState = Newstate;
            }

            public bool IsUnrandomized(int Include = 0)
            {
                if ((Include == 0 || Include == 1) && RandomizedState == RandomizedState.Unrandomized) { return true; }
                if ((Include == 0 || Include == 2) && RandomizedState == RandomizedState.UnrandomizedManual) { return true; }
                return false;
            }
            public bool IsRandomized()
            {
                return RandomizedState == RandomizedState.Randomized;
            }
            public bool IsJunk()
            {
                return RandomizedState == RandomizedState.ForcedJunk;
            }
            public LogicDictionaryData.DictionaryLocationEntries GetDictEntry(LogicObjects.TrackerInstance Instance)
            {
                return Instance.LogicDictionary.LocationList[Instance.InstanceReference.LocationDictionaryMapping[ID]];
            }
            public bool CanBeUnrandomized(LogicObjects.TrackerInstance instance)
            {
                if (IsUnrandomized()) { return true; }
                string OriginalItem = GetDictEntry(instance).OriginalItem ?? "";
                var OriginalItemObject = instance.GetItemByID(OriginalItem);
                if (OriginalItemObject == null) { return false; }
                return OriginalItemObject.CanBePlaced(instance);
            }

            public bool ToggleChecked(CheckState NewState, LogicObjects.TrackerInstance Instance)
            {
                CheckState CurrentState = CheckState;
                if (CurrentState == NewState)
                {
                    return false;
                }
                else if (CurrentState == CheckState.Checked)
                {
                    UncheckItem(NewState, Instance);
                }
                else if (NewState == CheckState.Checked)
                {
                    if (!CheckItem(NewState, Instance)) { return false; }
                }
                else
                {
                    if (!ToggleMarked(NewState, Instance)) { return false; }
                }
                CheckState = NewState;
                return true;
            }

            public bool UncheckItem(CheckState NewState, LogicObjects.TrackerInstance Instance)
            {
                var ItemAtCheck = Instance.GetItemByID(Randomizeditem.Item);

                if (ItemAtCheck != null)
                {
                    ItemAtCheck.ChangeLocalItemAmounts(Instance, this, -1);
                }
                if (NewState == CheckState.Unchecked)
                {
                    Randomizeditem.Item = null;
                }
                return true;

            }

            public bool CheckItem(CheckState NewState, LogicObjects.TrackerInstance Instance)
            {
                if (string.IsNullOrWhiteSpace(Randomizeditem.Item)) { return false; }

                var ItemAtCheck = Instance.GetItemByID(Randomizeditem.Item);
                if (ItemAtCheck != null)
                {
                    ItemAtCheck.ChangeLocalItemAmounts(Instance, this, 1);
                }
                return true;
            }

            public bool ToggleMarked(CheckState NewState, LogicObjects.TrackerInstance Instance)
            {
                if (NewState == CheckState.Marked && string.IsNullOrWhiteSpace(Randomizeditem.Item))
                {
                    return false;
                }
                else if (NewState == CheckState.Unchecked)
                {
                    Randomizeditem.Item = null;
                }
                return true;
            }

            public string GetItemAtCheck(LogicObjects.TrackerInstance Instance)
            {
                var ItemAtCheck = Randomizeditem.Item;
                if (SingleValidItem != null)
                {
                    ItemAtCheck = SingleValidItem;
                }
                if (Randomizeditem.SpoilerLogGivenItem != null)
                {
                    ItemAtCheck = Randomizeditem.SpoilerLogGivenItem;
                }
                if ((IsUnrandomized()) && GetDictEntry(Instance).OriginalItem != null)
                {
                    ItemAtCheck = GetDictEntry(Instance).OriginalItem;
                }
                return ItemAtCheck;
            }

        }
        [Serializable]
        public class RandomizeditemData
        {
            public int OwningPlayer { get; set; } = -1;
            public string Item { get; set; } = null;
            public string SpoilerLogGivenItem { get; set; } = null;
            public string[] ValidItemTypes { get; set; } = Array.Empty<string>();
        }
    }
}
