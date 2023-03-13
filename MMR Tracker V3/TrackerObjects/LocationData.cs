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
            public bool Hidden { get; set; } = false;
            public int? Price { get; set; } = null;
            public char? Currency { get; set; } = null;
            public string DisplayName { get; set; }
            public RandomizedState RandomizedState { get; set; } = RandomizedState.Randomized;
            public string SingleValidItem { get; set; } = null;
            public LogicObjects.ReferenceData referenceData { get; set; } = new LogicObjects.ReferenceData();

            public void GetPrice(out int outPrice, out char outCurrency)
            {
                outPrice = Price??-1;
                outCurrency = Currency??'$';
                return;
            }
            public void SetPrice(int inPrice, char inCurrency = '$')
            {
                Price = inPrice < 0 ? null : inPrice;
                Currency = inCurrency;
                return;
            }

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
                return Instance.LogicDictionary.LocationList[referenceData.DictIndex];
            }
            public bool CanBeUnrandomized(LogicObjects.TrackerInstance instance)
            {
                if (IsUnrandomized()) { return true; }
                string OriginalItem = GetDictEntry(instance).OriginalItem ?? "";
                var OriginalItemObject = instance.GetItemByID(OriginalItem);
                if (OriginalItemObject == null) { return false; }
                return OriginalItemObject.CanBePlaced(instance);
            }
            public bool AppearsinListbox(LogicObjects.TrackerInstance Instance, bool ShowJunkUnrand = false)
            {
                return (!IsJunk() || ShowJunkUnrand) && (!IsUnrandomized(1) || ShowJunkUnrand) && !string.IsNullOrWhiteSpace(GetDictEntry(Instance).GetName(Instance));
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
        }

        public class LocationProxyData
        {
            public Dictionary<string, List<string>> LocationsWithProxys { get; set; } = new Dictionary<string, List<string>>();
            public Dictionary<string, LocationProxy> LocationProxies { get; set; } = new Dictionary<string, LocationProxy>();

        }

        public class LocationProxy
        {
            public string ReferenceID { get; set; }
            public string ID { get; set; }
            public string Name { get; set; }
            public bool Starred { get; set; }
            public bool Hidden { get; set; } = false;
            public string Area { get; set; }
            public string LogicInheritance { get; set; } = null;
            public string DisplayName { get; set; }
            public override string ToString()
            {
                return DisplayName??Name;
            }
            public bool ProxyAvailable(LogicObjects.TrackerInstance instance)
            {
                var LogicId = LogicInheritance ?? ReferenceID;
                bool Literal = LogicId.IsLiteralID(out LogicId);
                var type = instance.GetLocationEntryType(LogicId, Literal, out _);
                return type switch
                {
                    LogicEntryType.location => instance.GetLocationByID(LogicId).Available,
                    LogicEntryType.macro => instance.GetMacroByID(LogicId).Aquired,
                    _ => false,
                };
            }
            public LocationObject GetReferenceLocation(LogicObjects.TrackerInstance instance)
            {
                return (instance.GetLocationByID(ReferenceID));
            }
            public object GetLogicInheritance(LogicObjects.TrackerInstance instance)
            {
                var LogicId = LogicInheritance ?? ReferenceID;
                bool Literal = LogicId.IsLiteralID(out LogicId);
                instance.GetLocationEntryType(LogicId, Literal, out object Result);
                return Result;
            }
        }
    }
}
