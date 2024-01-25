using MMR_Tracker_V3.TrackerObjectExtentions;
using System;
using System.Collections.Generic;
using System.Linq;
using static MMR_Tracker_V3.DataStructure.MiscData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class LocationData
    {
        [Serializable]
        public class LocationObject(InstanceData.TrackerInstance Parent) : CheckableLocation(Parent)
        {
            public RandomizeditemData Randomizeditem { get; set; } = new RandomizeditemData();
            public string SingleValidItem { get; set; } = null;
            public LogicDictionaryData.DictionaryLocationEntries GetDictEntry()
            {
                return GetParent().LogicDictionary.LocationList[ID];
            }

            public override string ToString()
            {
                return DisplayName ?? ID;
            }

            public List<ItemData.ItemObject> GetValidItems(string Filter = "", bool ForOtherPlayers = false)
            {
                return GetParent().GetValidItemsForLocation(this, Filter, ForOtherPlayers);
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
            public Dictionary<string, List<string>> LocationsWithProxys { get; set; } = [];
            public Dictionary<string, LocationProxy> LocationProxies { get; set; } = [];

        }

        public class LocationProxy(InstanceData.TrackerInstance Parent) : CheckableLocation(Parent)
        {
            public string ReferenceID { get; set; }
            public int DictInd { get; set; } = -1;
            public override string ToString()
            {
                return DisplayName ?? GetDictEntry().Name ?? ID;
            }
            public bool ProxyAvailable()
            {
                var LogicId = GetDictEntry().LogicInheritance ?? ReferenceID;
                bool Literal = LogicId.IsLiteralID(out LogicId);
                var type = GetParent().GetLocationEntryType(LogicId, Literal, out _);
                return type switch
                {
                    LogicEntryType.location => GetParent().GetLocationByID(LogicId).Available,
                    LogicEntryType.macro => GetParent().GetMacroByID(LogicId).Aquired,
                    _ => false,
                };
            }
            public LocationObject GetReferenceLocation()
            {
                return (GetParent().GetLocationByID(ReferenceID));
            }
            public object GetLogicInheritance()
            {
                var LogicId = GetDictEntry().LogicInheritance ?? ReferenceID;
                bool Literal = LogicId.IsLiteralID(out LogicId);
                GetParent().GetLocationEntryType(LogicId, Literal, out object Result);
                return Result;
            }
            public LogicDictionaryData.DictLocationProxy GetDictEntry()
            {
                var RefLocDict = GetReferenceLocation().GetDictEntry();
                if (DictInd > -1) { return RefLocDict.LocationProxys[DictInd]; }
                var Entry = RefLocDict.LocationProxys.First(x => x.ID == this.ID);
                return Entry;
            }
            public new void GetPrice(out int outPrice, out char outCurrency)
            {
                dynamic Target = GetLogicInheritance();
                Target.GetPrice(out outPrice, out outCurrency);
            }
            public new void SetPrice(int inPrice, char inCurrency = '\0')
            {
                dynamic Target = GetLogicInheritance();
                Target.SetPrice(inPrice, inCurrency);
            }
        }
    }
}
