using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public class PlaythroughGenerator
    {
        LogicObjects.TrackerInstance _Instance;
        Dictionary<string, PlaythroughObject> Playthrough;
        Dictionary<string, MMRData.JsonFormatLogicItem> LogicMap = new Dictionary<string, MMRData.JsonFormatLogicItem>();
        public PlaythroughGenerator(LogicObjects.TrackerInstance instance)
        {
            _Instance = Newtonsoft.Json.JsonConvert.DeserializeObject<LogicObjects.TrackerInstance>(Newtonsoft.Json.JsonConvert.SerializeObject(instance));
        }

        public class PlaythroughObject
        {
            public string id { get; set; }
            public int sphere { get; set; }
            public List<string> UsedItems { get; set; }
        }

        public void PrepareInstance()
        {
            foreach(var i in _Instance.LocationPool.Values)
            {
                i.Available = false;
                i.CheckState = TrackerObjects.MiscData.CheckState.Unchecked;
                if (i.GetItemAtCheck(_Instance) == null) { i.RandomizedState = TrackerObjects.MiscData.RandomizedState.ForcedJunk; }
                else { i.RandomizedState = TrackerObjects.MiscData.RandomizedState.Randomized; }
            }
            foreach (var i in _Instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values))
            {
                i.Available = false;
                i.CheckState = TrackerObjects.MiscData.CheckState.Unchecked;
                if (i.GetDestinationAtExit(_Instance) == null) { i.RandomizedState = TrackerObjects.MiscData.RandomizedState.ForcedJunk; }
                else { i.RandomizedState = TrackerObjects.MiscData.RandomizedState.Randomized; }
            }
            foreach (var i in _Instance.EntrancePool.AreaList.Values.SelectMany(x => x.MacroExits.Values))
            {
                i.Available = false;
                i.CheckState = TrackerObjects.MiscData.CheckState.Unchecked;
            }
            foreach (var i in _Instance.MacroPool.Values)
            {
                i.Aquired = false;
            }
        }

        public void GetLogic()
        {
            foreach (var i in _Instance.MacroPool) { LogicMap[i.Key] = _Instance.GetLogic(i.Key); }
            foreach (var i in _Instance.LocationPool) { LogicMap[i.Key] = _Instance.GetLogic(i.Key); }
        }
    }
    public static class PlaythroughTools
    {
        public static Dictionary<string, List<EntranceData.EntranceAreaPair>> PAthingMap = new Dictionary<string, List<EntranceData.EntranceAreaPair>>();
        public class AdvancedUnlockData
        {
            public List<string> LogicUsed { get; set; } = new List<string>();
            public Dictionary<string, int> RealItemsUsed { get; set; } = new Dictionary<string, int>();
            public List<string> MacrosUsed { get; set; } = new List<string>();
            public List<string> AreasAccessed { get; set; } = new List<string>();
            public List<string> OptionsUsed { get; set; } = new List<string>();
        }
        public static AdvancedUnlockData GetAdvancedUnlockData(string ID, Dictionary<string, List<string>> UnlockData, LogicObjects.TrackerInstance instance)
        {
            var MacroExits = instance.EntrancePool.AreaList.Values.SelectMany(x => x.MacroExits.Values).Where(x => x.CheckState == MiscData.CheckState.Checked);
            MacroExits = MacroExits.Concat(instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values).Where(x => x.CheckState == MiscData.CheckState.Checked && x.IsUnrandomized()));
            var LoadingZoneExits = instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values).Where(x => x.CheckState == MiscData.CheckState.Checked && x.RandomizedState == MiscData.RandomizedState.Randomized);

            List<string> Areas = new List<string>();

            AdvancedUnlockData Data = new AdvancedUnlockData();
            if (!UnlockData.ContainsKey(ID)) { return Data; }
            foreach (var Entry in UnlockData[ID])
            {
                Data.LogicUsed.Add(Entry);
                ParseItem(Entry);
            }

            return Data;

            void ParseItem(string SubID, bool root = true)
            {
                LogicCalculation.MultipleItemEntry(instance, SubID, out string LogicItem, out int Amount);
                bool Literal = LogicItem.IsLiteralID(out LogicItem);
                var type = instance.GetItemEntryType(LogicItem, Literal, out _);

                if (type == MiscData.LogicEntryType.macro && !Data.MacrosUsed.Contains(LogicItem))
                {
                    Data.MacrosUsed.Add(LogicItem);
                    if (UnlockData.ContainsKey(SubID))
                    {
                        foreach(var i in UnlockData[SubID]) { ParseItem(i, false); }
                    }
                }
                else if (type == MiscData.LogicEntryType.Option && !Data.OptionsUsed.Contains(LogicItem))
                {
                    Data.OptionsUsed.Add(LogicItem);
                }
                else if (type == MiscData.LogicEntryType.Area && !Areas.Contains(LogicItem))
                {
                    Data.AreasAccessed.Add(LogicItem);
                }
                else if (type == MiscData.LogicEntryType.item)
                {
                    if (Data.RealItemsUsed.ContainsKey(LogicItem))
                    {
                        if (Amount > Data.RealItemsUsed[LogicItem]) { Data.RealItemsUsed[LogicItem] = Amount; }
                    }
                    else
                    {
                        Data.RealItemsUsed.Add(LogicItem, Amount);
                    }
                }

            }

        }

    }
}
