using System;
using System.Collections.Generic;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class ItemData
    {
        [Serializable]
        public class ItemObject(InstanceData.TrackerInstance Parent) : ObtainableObject(Parent)
        {
            public LogicDictionaryData.DictionaryItemEntries GetDictEntry()
            {
                return GetParent().LogicDictionary.ItemList[ID];
            }
            public override string ToString()
            {
                return DisplayName ?? ID;
            }
        }
    }
}
