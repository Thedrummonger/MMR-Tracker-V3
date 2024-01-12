using MathNet.Numerics;
using MMR_Tracker_V3.TrackerObjectExtentions;
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
        public class LocationObject(InstanceData.TrackerInstance Parent)
        {
            private InstanceData.TrackerInstance _parent = Parent;
            public InstanceData.TrackerInstance GetParent() { return _parent; }
            public void SetParent(InstanceData.TrackerInstance parent) { _parent = parent; }

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
            public InstanceData.ReferenceData referenceData { get; set; } = new InstanceData.ReferenceData();
            public LogicDictionaryData.DictionaryLocationEntries GetDictEntry()
            {
                return GetParent().LogicDictionary.LocationList[ID];
            }

            public override string ToString()
            {
                return DisplayName ?? ID;
            }
            public void GetPrice(out int outPrice, out char outCurrency)
            {
                outPrice = Price??-1;
                outCurrency = Currency??'$';
                return;
            }
            public void SetPrice(int inPrice, char inCurrency = '\0')
            {
                if (inCurrency == '\0') { inCurrency = Currency??'$'; }
                Price = inPrice < 0 ? null : inPrice;
                Currency = inCurrency;
                return;
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

        public class LocationProxy(InstanceData.TrackerInstance Parent)
        {
            private InstanceData.TrackerInstance _parent = Parent;
            public InstanceData.TrackerInstance GetParent() { return _parent; }
            public void SetParent(InstanceData.TrackerInstance parent) { _parent = parent; }

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
            public bool ProxyAvailable()
            {
                var LogicId = LogicInheritance ?? ReferenceID;
                bool Literal = LogicId.IsLiteralID(out LogicId);
                var type = _parent.GetLocationEntryType(LogicId, Literal, out _);
                return type switch
                {
                    LogicEntryType.location => _parent.GetLocationByID(LogicId).Available,
                    LogicEntryType.macro => _parent.GetMacroByID(LogicId).Aquired,
                    _ => false,
                };
            }
            public LocationObject GetReferenceLocation()
            {
                return (_parent.GetLocationByID(ReferenceID));
            }
            public object GetLogicInheritance()
            {
                var LogicId = LogicInheritance ?? ReferenceID;
                bool Literal = LogicId.IsLiteralID(out LogicId);
                _parent.GetLocationEntryType(LogicId, Literal, out object Result);
                return Result;
            }
        }
    }
}
