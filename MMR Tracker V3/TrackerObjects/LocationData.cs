using MMR_Tracker_V3.TrackerObjectExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

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

            public override string GetName()
            {
                return GetDictEntry().Name;
            }

            public override object GetAbstractDictEntry() => GetDictEntry();

            public override CheckableLocationTypes LocationType() => CheckableLocationTypes.location;
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
                return GetLogicInheritance().Available;
            }
            public LocationObject GetReferenceLocation()
            {
                return (GetParent().GetLocationByID(ReferenceID));
            }
            public CheckableLocation GetLogicInheritance()
            {
                var LogicId = GetDictEntry().LogicInheritance ?? ReferenceID;
                bool Literal = LogicId.IsLiteralID(out LogicId);
                return GetParent().GetCheckableLocationByID(LogicId, Literal);
            }
            public LogicDictionaryData.DictLocationProxy GetDictEntry()
            {
                var RefLocDict = GetReferenceLocation().GetDictEntry();
                if (DictInd > -1) { return RefLocDict.LocationProxys[DictInd]; }
                var Entry = RefLocDict.LocationProxys.First(x => x.ID == this.ID);
                return Entry;
            }
            public override void GetPrice(out int outPrice, out char outCurrency)
            {
                CheckableLocation Target = GetLogicInheritance();
                Target.GetPrice(out outPrice, out outCurrency);
            }
            public override void SetPrice(int inPrice, char inCurrency = '\0')
            {
                CheckableLocation Target = GetLogicInheritance();
                Target.SetPrice(inPrice, inCurrency);
            }
            public override (int, char) GetPrice()
            {
                CheckableLocation Target = GetLogicInheritance();
                Target.GetPrice(out int outPrice, out char outCurrency);
                return (outPrice, outCurrency);
            }

            public override string GetName()
            {
                return GetDictEntry().Name??ID;
            }

            public override object GetAbstractDictEntry() => GetDictEntry();

            public override CheckableLocationTypes LocationType() => GetLogicInheritance().LocationType();
        }
    }
}
