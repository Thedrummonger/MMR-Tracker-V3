using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.LogicDictionaryData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public class LogicObjects
    {
        #region TrackerInstance
        public class TrackerInstance
        {
            public LocationPool LocationPool { get; set; } = new LocationPool();
            public ItemPool ItemPool { get; set; } = new ItemPool();
            public MacroData Macros { get; set; } = new MacroData();
            public LogicDictionary LogicDictionary { get; set; } = new LogicDictionary();
            public Options Options { get; set; } = new Options();
            public InstanceReference InstanceReference { get; set; } = new InstanceReference();
        }

        public class InstanceReference
        {
            public Dictionary<string, string> EntrancePairs { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, int> IDtoIndex { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, string> EntranceAreaDic { get; set; } = new Dictionary<string, string>();
            public Keydata Keydata { get; set; } = new Keydata();
        }

        public class Keydata
        {
            public List<string> SmallKeys { get; set; } = new List<string>();
            public List<string> BossKeys { get; set; } = new List<string>();
            public List<string> LocationWithKeysInLogic { get; set; } = new List<string>();
        }

        public class Options
        {
            public bool EntranceRadnoEnabled { get; set; } = false;
            public bool CoupleEntrances { get; set; } = true;
            public bool ShowAdditionalStats { get; set; } = false;
            public bool CheckForUpdate { get; set; } = true;
            public WinformData WinformData { get; set; } = new WinformData();
        }

        public class WinformData
        {
            public string FormFont { get; set; } = string.Empty;
            public bool HorizontalLayout { get; set; } = false;
            public bool MoveMarkedToBottom { get; set; } = false;
            public MiddleClickFunction MiddleClickFunction { get; set; } = MiddleClickFunction.set;
            public bool ShowEntryNameTooltip { get; set; } = true;
        }
        #endregion TrackerInstance

    }
}
