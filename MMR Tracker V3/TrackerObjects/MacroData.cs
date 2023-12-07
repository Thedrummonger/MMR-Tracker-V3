using MMR_Tracker_V3.TrackerObjectExtentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.MMRData;

namespace MMR_Tracker_V3.TrackerObjects
{
    [Serializable]
    public class MacroObject
    {
        private InstanceData.TrackerInstance _parent;
        public InstanceData.TrackerInstance GetParent() { return _parent; }
        public void SetParent(InstanceData.TrackerInstance parent) { _parent = parent; }
        public MacroObject(InstanceData.TrackerInstance Parent)
        {
            _parent = Parent;
        }
        public string ID { get; set; }
        public bool Aquired { get; set; } = false;
        public bool TrickEnabled { get; set; } = true;
        public int? Price { get; set; } = null;
        public char? Currency { get; set; } = null;
        public InstanceData.ReferenceData referenceData { get; set; } = new InstanceData.ReferenceData();


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

        public LogicDictionaryData.DictionaryMacroEntry GetDictEntry()
        {
            if (GetParent().LogicDictionary.MacroList.ContainsKey(ID))
            {
                return GetParent().LogicDictionary.MacroList[ID];
            }
            return new LogicDictionaryData.DictionaryMacroEntry
            {
                ID = ID,
                Name = null,
            };
        }

        public bool isTrick()
        {
            return _parent.GetLogic(ID, false).IsTrick;
        }
    }
}
