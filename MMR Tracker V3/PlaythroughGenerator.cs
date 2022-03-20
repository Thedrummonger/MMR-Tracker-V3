using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
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
}
