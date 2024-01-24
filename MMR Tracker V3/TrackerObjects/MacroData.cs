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
    public class MacroObject(InstanceData.TrackerInstance Parent) : CheckableLocation(Parent)
    {
        public bool Aquired { get; set; } = false;
        public bool TrickEnabled { get; set; } = true;

        public LogicDictionaryData.DictionaryMacroEntry GetDictEntry()
        {
            if (GetParent().LogicDictionary.MacroList.TryGetValue(ID, out LogicDictionaryData.DictionaryMacroEntry value))
            {
                return value;
            }
            return new LogicDictionaryData.DictionaryMacroEntry()
            {
                ID = ID,
                Name = null,
            };
        }

        public bool isTrick()
        {
            return GetParent().GetLogic(ID, false).IsTrick;
        }
    }
}
