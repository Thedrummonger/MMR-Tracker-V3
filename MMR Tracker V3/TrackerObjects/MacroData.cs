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
        public int MacroPrice { get; set; } = -1;

        public LogicDictionaryData.DictionaryMacroEntry GetDictEntry(LogicObjects.TrackerInstance Instance)
        {
            if (Instance.InstanceReference.MacroDictionaryMapping.ContainsKey(ID))
            {
                return Instance.LogicDictionary.MacroList[Instance.InstanceReference.MacroDictionaryMapping[ID]];
            }
            return new LogicDictionaryData.DictionaryMacroEntry
            {
                ID = ID,
                DynamicLogicData = null,
                ConditionalItemsOverride = null,
                RequiredItemsOverride = null,
                Static = false
            };
        }

        public bool isTrick(LogicObjects.TrackerInstance Instance)
        {
            if (Instance.LogicOverride.ContainsKey(ID))
            {
                return Instance.LogicOverride[ID].IsTrick;
            }
            if (Instance.InstanceReference.LogicFileMapping.ContainsKey(ID))
            {
                int Index = Instance.InstanceReference.LogicFileMapping[ID];
                return Instance.LogicFile.Logic[Index].IsTrick;
            }
            return false;
        }
    }
    [Serializable]

    public class dynamicLogicData
    {
        //If "LocationToCompare" has the randomized Item "ItemAtLocation" Use the logic of Macro "LogicToUse"
        public string LocationToCompare { get; set; }
        public List<DynamicLogicArguments> Arguments { get; set; } = new List<DynamicLogicArguments>();

    }
    [Serializable]
    public class DynamicLogicArguments
    {
        public string ItemAtLocation { get; set; }
        public string LogicToUse { get; set; }
    }
}
