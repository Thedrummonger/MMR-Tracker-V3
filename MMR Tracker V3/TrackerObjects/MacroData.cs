using MMR_Tracker_V3.TrackerObjectExtensions;
using System;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjects
{
    [Serializable]
    public class MacroObject(InstanceData.TrackerInstance Parent) : CheckableLocation(Parent)
    {
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

        public override string GetName()
        {
            return GetDictEntry().Name??ID;
        }

        public override LogicDictionaryData.DictionaryCheckableLocationEntry GetAbstractDictEntry() => GetDictEntry();

        public override CheckableLocationTypes LocationType() => CheckableLocationTypes.macro;
    }
}
