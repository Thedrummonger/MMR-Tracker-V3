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
        public int Price { get; set; } = -1;
        public LogicObjects.ReferenceData referenceData { get; set; } = new LogicObjects.ReferenceData();

        public LogicDictionaryData.DictionaryMacroEntry GetDictEntry(LogicObjects.TrackerInstance Instance)
        {
            if (referenceData.DictIndex > -1)
            {
                return Instance.LogicDictionary.MacroList[referenceData.DictIndex];
            }
            return new LogicDictionaryData.DictionaryMacroEntry
            {
                ID = ID,
                Name = null,
            };
        }

        public bool isTrick(LogicObjects.TrackerInstance Instance)
        {
            return Instance.GetLogic(ID, false).IsTrick;
        }
    }
}
