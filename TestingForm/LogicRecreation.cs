using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;

namespace MMR_Tracker_V3
{
    public static class LogicRecreation
    {
        public static SaveState CurrentSaveState { get; set; }
        public class SaveState
        {
            public Dictionary<string, RecData> Locations { get; set; } = new Dictionary<string, RecData>();
            public Dictionary<string, RecData> Exits { get; set; } = new Dictionary<string, RecData>();
            public Dictionary<string, RecData> Hints { get; set; } = new Dictionary<string, RecData>();
            public InstanceData.SpoilerLogFileData SpoilerData { get; set; }
        }
        public class RecData
        {
            public MiscData.CheckState CheckState { get; set; }
            public dynamic randomizedEntry { get; set; }
            public dynamic SpoilerEntry { get; set; }
            public int OwningPlayer { get; set; }
            public int Price { get; set; }
        }

        public static void SaveTrackerState(MMR_Tracker_V3.TrackerObjects.InstanceData.InstanceContainer InstanceContainer)
        {
            SaveState state = new SaveState();
            state.SpoilerData = InstanceContainer.Instance.SpoilerLog is null ? null : GenericCopier<InstanceData.SpoilerLogFileData>.DeepCopy(InstanceContainer.Instance.SpoilerLog);
            foreach (var i in InstanceContainer.Instance.LocationPool)
            {
                i.Value.GetPrice(out int P, out char C);
                state.Locations.Add(i.Key, new RecData() { 
                    CheckState = i.Value.CheckState, 
                    Price = P, 
                    randomizedEntry = i.Value.Randomizeditem.Item ,
                    SpoilerEntry = i.Value.Randomizeditem.SpoilerLogGivenItem,
                    OwningPlayer = i.Value.Randomizeditem.OwningPlayer
                });
            }
            foreach (var i in InstanceContainer.Instance.GetAllRandomizableExits())
            {
                state.Exits.Add(i.ID, new RecData()
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
        public static void LoadTrackerState(MMR_Tracker_V3.TrackerObjects.InstanceData.InstanceContainer InstanceContainer)
        {
            if (CurrentSaveState is null) { return; }
            if (CurrentSaveState.SpoilerData is not null && InstanceContainer.Instance.SpoilerLog is null)
            {
                SpoilerLogHandling.Parser.ApplyGenericSpoilerLog(InstanceContainer, CurrentSaveState.SpoilerData.Log);
            }
            foreach(var i in CurrentSaveState.Locations)
            {
                if (InstanceContainer.Instance.LocationPool.ContainsKey(i.Key))
                {
                    var loc = InstanceContainer.Instance.LocationPool[i.Key];
                    loc.SetPrice(i.Value.Price);
                    loc.Randomizeditem.OwningPlayer = i.Value.OwningPlayer;
                    loc.Randomizeditem.Item = loc.GetItemAtCheck()??i.Value.randomizedEntry;
                    loc.ToggleChecked(i.Value.CheckState);
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
                if (InstanceContainer.Instance.GetExitByLogicID(i.Key) is not null)
                {
                    var loc = InstanceContainer.Instance.GetExitByLogicID(i.Key);
                    loc.DestinationExit = loc.GetDestinationAtExit() ?? i.Value.randomizedEntry;
                    loc.ToggleExitChecked(i.Value.CheckState);
                }

            }
            InstanceContainer.logicCalculation.CalculateLogic(MiscData.CheckState.Checked);
        }
    }
}
