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

        public LogicDictionaryData.DictionaryMacroEntry GetDictEntry(InstanceData.TrackerInstance Instance)
        {
            if (Instance.LogicDictionary.MacroList.ContainsKey(ID))
            {
                return Instance.LogicDictionary.MacroList[ID];
            }
            return new LogicDictionaryData.DictionaryMacroEntry
            {
                ID = ID,
                Name = null,
            };
        }

        public bool isTrick(InstanceData.TrackerInstance Instance)
        {
            return Instance.GetLogic(ID, false).IsTrick;
        }
    }
}
