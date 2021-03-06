using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public class LogicRecreation
    {
        public SaveState CurrentSaveState { get; set; }
        public class SaveState
        {
            public Dictionary<string, RecData> Locations { get; set; } = new Dictionary<string, RecData>();
            public Dictionary<string, RecData> Exits { get; set; } = new Dictionary<string, RecData>();
            public Dictionary<string, RecData> Hints { get; set; } = new Dictionary<string, RecData>();
            public LogicObjects.SpoilerLogFileData SpoilerData { get; set; }
        }
        public class RecData
        {
            public MiscData.CheckState CheckState { get; set; }
            public dynamic randomizedEntry { get; set; }
            public dynamic SpoilerEntry { get; set; }
            public int OwningPlayer { get; set; }
            public int Price { get; set; }
        }

        public void SaveTrackerState(MiscData.InstanceContainer InstanceContainer)
        {
            SaveState state = new SaveState();
            state.SpoilerData = InstanceContainer.Instance.SpoilerLog is null ? null : GenericCopier<LogicObjects.SpoilerLogFileData>.DeepCopy(InstanceContainer.Instance.SpoilerLog);
            foreach (var i in InstanceContainer.Instance.LocationPool)
            {
                state.Locations.Add(i.Key, new RecData() { 
                    CheckState = i.Value.CheckState, 
                    Price = i.Value.Price, 
                    randomizedEntry = i.Value.Randomizeditem.Item ,
                    SpoilerEntry = i.Value.Randomizeditem.SpoilerLogGivenItem,
                    OwningPlayer = i.Value.Randomizeditem.OwningPlayer
                });
            }
            foreach (var i in InstanceContainer.Instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values))
            {
                state.Exits.Add(InstanceContainer.Instance.GetLogicNameFromExit(i), new RecData()
                {
                    CheckState = i.CheckState,
                    randomizedEntry = i.DestinationExit,
                    SpoilerEntry = i.SpoilerDefinedDestinationExit
                });
            }
            foreach (var i in InstanceContainer.Instance.HintPool)
            {
                state.Hints.Add(i.Key, new RecData()
                {
                    CheckState = i.Value.CheckState,
                    randomizedEntry = i.Value.HintText,
                    SpoilerEntry = i.Value.SpoilerHintText
                });
            }
            CurrentSaveState = state;
        }
        public void LoadTrackerState(MiscData.InstanceContainer InstanceContainer)
        {
            if (CurrentSaveState is null) { return; }
            if (CurrentSaveState.SpoilerData is not null)
            {
                SpoilerLogTools.ImportSpoilerLog(CurrentSaveState.SpoilerData.Log, CurrentSaveState.SpoilerData.FileName, InstanceContainer.Instance);
            }
            foreach(var i in CurrentSaveState.Locations)
            {
                if (InstanceContainer.Instance.LocationPool.ContainsKey(i.Key))
                {
                    var loc = InstanceContainer.Instance.LocationPool[i.Key];
                    loc.Price = i.Value.Price;
                    loc.Randomizeditem.OwningPlayer = i.Value.OwningPlayer;
                    loc.Randomizeditem.Item = loc.GetItemAtCheck(InstanceContainer.Instance)??i.Value.randomizedEntry;
                    loc.ToggleChecked(i.Value.CheckState, InstanceContainer.Instance);
                }
            }
            foreach (var i in CurrentSaveState.Hints)
            {
                if (InstanceContainer.Instance.HintPool.ContainsKey(i.Key))
                {
                    var loc = InstanceContainer.Instance.HintPool[i.Key];
                    loc.SpoilerHintText = i.Value.SpoilerEntry;
                    loc.HintText = loc.SpoilerHintText;
                    loc.CheckState = i.Value.CheckState;
                }
            }
            foreach (var i in CurrentSaveState.Exits)
            {
                var EntPool = InstanceContainer.Instance.EntrancePool;
                var entDict = InstanceContainer.Instance.InstanceReference.EntranceLogicNameToEntryData;
                if (entDict.ContainsKey(i.Key) && EntPool.AreaList.ContainsKey(entDict[i.Key].Area) && EntPool.AreaList[entDict[i.Key].Area].LoadingZoneExits.ContainsKey(entDict[i.Key].Exit))
                {
                    var loc = EntPool.AreaList[entDict[i.Key].Area].LoadingZoneExits[entDict[i.Key].Exit];
                    loc.DestinationExit = loc.GetDestinationAtExit(InstanceContainer.Instance)??i.Value.randomizedEntry;
                    loc.ToggleExitChecked(i.Value.CheckState, InstanceContainer.Instance);
                }
            }
            InstanceContainer.logicCalculation.CalculateLogic(MiscData.CheckState.Checked);
        }
    }
}
