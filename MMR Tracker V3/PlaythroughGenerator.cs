using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static MMR_Tracker_V3.PlaythroughGenerator;
using static MMR_Tracker_V3.TrackerObjects.EntranceData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public class PlaythroughGenerator
    {
        public InstanceData.InstanceContainer container;
        public List<string> ignoredChecks;
        public Dictionary<string, PlaythroughObject> Playthrough = [];
        public PlaythroughGenerator(InstanceData.TrackerInstance instance, List<string> ignoredChecks = null)
        {
            container = new();
            container.CopyAndLoadInstance(instance);
            this.ignoredChecks = ignoredChecks ?? [];
        }

        public void GeneratePlaythrough()
        {
            PlaythroughData playthroughData = new();
            int Sphere = 0;
            ResetInstance();
            Playthrough.Clear();
            container.logicCalculation.CompileOptionActionEdits();
            container.logicCalculation.CalculateLogic(MiscData.CheckState.Checked);
            ScanLocationsThisSphere(playthroughData);

            while (playthroughData.NewLocations().Count > 0 || playthroughData.NewMacros().Count > 0 || playthroughData.NewExits().Count > 0)
            {
                IEnumerable<CheckableLocation> LocationsToCheck = [
                    ..playthroughData.NewLocations().Where(x => x.CheckState == CheckState.Unchecked),
                    ..playthroughData.NewExits().Where(x => x.CheckState == CheckState.Unchecked && x.IsRandomized())];

                AddNewItemsToPlaythrough(playthroughData, Sphere);

                foreach (var i in playthroughData.NewLocations()) { playthroughData.CheckedLocations.Add(i); }
                foreach (var i in playthroughData.NewExits()) { playthroughData.CheckedExits.Add(i); }
                foreach (var i in playthroughData.NewMacros()) { playthroughData.CheckedMacros.Add(i); }

                LocationChecker.CheckSelectedItems(LocationsToCheck, container, new CheckItemSetting(CheckState.Checked).SetCheckEntrancePairs(false));

                ScanLocationsThisSphere(playthroughData);
                Sphere++;
            }

            foreach(var i in Playthrough)
            {
                i.Value.advancedUnlockData = PlaythroughTools.GetAdvancedUnlockData(i.Key, container.logicCalculation.UnlockData, container.Instance);
            }
        }

        private void AddNewItemsToPlaythrough(PlaythroughData playthroughData, int Sphere)
        {
            foreach (var item in playthroughData.NewLocations())
            {
                Playthrough.Add(item.ID, new PlaythroughObject()
                {
                    id = item.ID,
                    CheckType = CheckableLocationTypes.location,
                    sphere = Sphere,
                    Check = item
                });
            }
            foreach (var item in playthroughData.NewExits())
            {
                Playthrough.Add(item.ID, new PlaythroughObject()
                {
                    id = item.ID,
                    CheckType = CheckableLocationTypes.Exit,
                    sphere = Sphere,
                    Check = item
                });
            }
            foreach (var item in playthroughData.NewMacros())
            {
                Playthrough.Add(item.ID, new PlaythroughObject()
                {
                    id = item.ID,
                    CheckType = CheckableLocationTypes.macro,
                    sphere = Sphere,
                    Check = item
                });
            }
        }

        private void ScanLocationsThisSphere(PlaythroughData playthroughData)
        {
            playthroughData.LocationsThisSphere = container.Instance.LocationPool.Values.Where(x =>
                x.Available && !x.IsJunk()).ToHashSet();
            playthroughData.ExitsThisSphere = container.Instance.ExitPool.Values.Where(x =>
                x.Available && !x.IsJunk()).ToHashSet();
            playthroughData.MacrosThisSphere = container.Instance.MacroPool.Values.Where(x =>
                x.Available).ToHashSet();
        }

        private void ResetInstance()
        {
            foreach (var i in container.Instance.AreaPool.Values) { i.ExitsAcessibleFrom = 0; }
            foreach (var i in container.Instance.ItemPool.Values) { i.AmountAquiredLocally = 0; }
            foreach (var i in container.Instance.LocationPool.Values) 
            {
                i.Available = false;
                i.CheckState = MiscData.CheckState.Unchecked;
                if (ignoredChecks.Contains(i.ID) || i.GetItemAtCheck() == null) { i.RandomizedState = MiscData.RandomizedState.ForcedJunk; }
                else
                {
                    i.Randomizeditem.Item = i.GetItemAtCheck();
                    if (i.IsUnrandomized()) { i.RandomizedState = MiscData.RandomizedState.Randomized; }
                }
            }
            foreach (var i in container.Instance.GetAllRandomizableExits())
            {
                i.Available = false;
                i.CheckState = MiscData.CheckState.Unchecked;
                if (ignoredChecks.Contains(i.ID) || i.GetDestinationAtExit() == null) { i.RandomizedState = MiscData.RandomizedState.ForcedJunk; }
                else
                {
                    i.DestinationExit = i.GetDestinationAtExit();
                    if (i.RandomizedState == MiscData.RandomizedState.UnrandomizedManual) { i.RandomizedState = MiscData.RandomizedState.Randomized; }
                }
            }
            foreach (var i in container.Instance.GetMacroExits())
            {
                i.Available = false;
                i.CheckState = MiscData.CheckState.Unchecked;
                i.DestinationExit = i.GetDestinationAtExit();
            }
            foreach (var i in container.Instance.MacroPool.Values)
            {
                i.Available = false;
            }
            container.logicCalculation.UnlockData.Clear();
        }

        public bool FilterImportantPlaythrough(object WinCon)
        {
            PlaythroughObject WinConLoc = SetWinConImportant(WinCon);
            if (WinConLoc is null) {  return false; }

            List<PlaythroughObject> HandledImportantChecks = new List<PlaythroughObject>();
            List<PlaythroughObject> ImportantChecks() { return Playthrough.Values.Where(x => x.Important).ToList(); }
            List<PlaythroughObject> UnhandledImportantChecks() { return ImportantChecks().Where(x => !HandledImportantChecks.Contains(x)).ToList(); }

            while (UnhandledImportantChecks().Any())
            {
                foreach (var i in UnhandledImportantChecks())
                {
                    MarkCheckwithRequiredItems(i);
                }
            }

            void MarkCheckwithRequiredItems(PlaythroughObject ImportantCheck)
            {
                foreach (var i in ImportantCheck.advancedUnlockData.RealItemsUsed)
                {
                    int StartingAmount = container.Instance.GetItemByID(i.Key)?.AmountInStartingpool ?? 0;
                    int ItemsNeeded = i.Value;
                    ItemsNeeded -= StartingAmount;
                    if (ItemsNeeded < 1) { continue; }
                    foreach(var playobj in Playthrough.Where(x => x.Value.CheckType == CheckableLocationTypes.location))
                    {
                        var loc = playobj.Value.Check as LocationObject;
                        if (loc.Randomizeditem.Item == i.Key)
                        {
                            playobj.Value.Important = true;
                            ItemsNeeded--;
                        }
                        if (ItemsNeeded < 1) { break; }
                    }
                }
                foreach (var i in ImportantCheck.advancedUnlockData.AreasAccessed)
                {
                    foreach(var playobj in Playthrough.Where(x => x.Value.CheckType == CheckableLocationTypes.Exit))
                    {
                        var exit = playobj.Value.Check as EntranceRandoExit;
                        if (exit.IsRandomizableEntrance() && exit.IsRandomized()) 
                        { 
                            playobj.Value.Important = true;
                            break;
                        }
                    }
                }
                foreach (var i in ImportantCheck.advancedUnlockData.OptionsUsed)
                {
                    LogicFunctions.IsLogicFunction(i, out string func, out string[] Params);
                    if (func == "renewable")
                    {
                        bool IsNegated = Params.Length > 1 && bool.TryParse(Params[1], out bool NegateBool) && !NegateBool;
                        foreach(var playobj in Playthrough.Where(x => x.Value.CheckType == CheckableLocationTypes.location))
                        {
                            var loc = playobj.Value.Check as LocationObject;
                            if (loc.GetDictEntry().Repeatable == !IsNegated && loc.Randomizeditem.Item == Params[0])
                            {
                                playobj.Value.Important = true;
                                break;
                            }
                        }
                    }
                }
                HandledImportantChecks.Add(ImportantCheck);
            }

            return true;

        }

        private PlaythroughObject SetWinConImportant(object WinObj)
        {
            if (WinObj is ItemData.ItemObject itemWin)
            {
                foreach (var p in Playthrough.Where(x => x.Value.CheckType == CheckableLocationTypes.location))
                {
                    LocationData.LocationObject locationObject = p.Value.Check as LocationData.LocationObject;
                    if (locationObject.Randomizeditem.Item == itemWin.ID) { p.Value.Important = true; return p.Value; }
                }
            }
            if (WinObj is EntranceData.EntranceRandoArea AreaWin)
            {
                foreach (var p in Playthrough.Where(x => x.Value.CheckType == CheckableLocationTypes.Exit))
                {
                    EntranceData.EntranceRandoExit EntranceRandoExit = p.Value.Check as EntranceData.EntranceRandoExit;
                    if (EntranceRandoExit.DestinationExit.region == AreaWin.ID) { p.Value.Important = true; return p.Value; }
                }
            }
            if (WinObj is MacroObject MacroWin)
            {
                var MacroPlaythrough = Playthrough.Where(x => x.Value.CheckType == CheckableLocationTypes.macro).ToDictionary();
                if (Playthrough.TryGetValue(MacroWin.ID, out PlaythroughObject po)) { po.Important = true; return po; }
            }
            if (WinObj is LocationData.LocationObject LocWin)
            {
                var LocPlaythrough = Playthrough.Where(x => x.Value.CheckType == CheckableLocationTypes.location).ToDictionary();
                if (Playthrough.TryGetValue(LocWin.ID, out PlaythroughObject po)) { po.Important = true; return po; }
            }
            if (WinObj is EntranceRandoExit ExitWin)
            {
                var MacroPlaythrough = Playthrough.Where(x => x.Value.CheckType == CheckableLocationTypes.Exit).ToDictionary();
                if (Playthrough.TryGetValue(ExitWin.ID, out PlaythroughObject po)) { po.Important = true; return po; }
            }
            return null;
        }

        public IEnumerable<string> CreateReadablePlaythrough(Dictionary<string, PlaythroughObject> result)
        {
            throw new NotImplementedException();
        }

        public class PlaythroughObject
        {
            public string id { get; set; }
            public int sphere { get; set; }
            public MiscData.CheckableLocationTypes CheckType { get; set; }
            public CheckableLocation Check { get; set; }
            public List<string> UsedItems { get; set; }
            public bool Important { get; set; }
            public PlaythroughTools.AdvancedUnlockData advancedUnlockData { get; set; }
        }

        public class PlaythroughData()
        {
            public HashSet<LocationData.LocationObject> CheckedLocations = [];
            public HashSet<LocationData.LocationObject> LocationsThisSphere = [];
            public HashSet<LocationData.LocationObject> NewLocations() => LocationsThisSphere.Where(x => !CheckedLocations.Contains(x)).ToHashSet();
            public HashSet<EntranceData.EntranceRandoExit> CheckedExits = [];
            public HashSet<EntranceData.EntranceRandoExit> ExitsThisSphere = [];
            public HashSet<EntranceData.EntranceRandoExit> NewExits() => ExitsThisSphere.Where(x => !CheckedExits.Contains(x)).ToHashSet();
            public HashSet<MacroObject> CheckedMacros = [];
            public HashSet<MacroObject> MacrosThisSphere = [];
            public HashSet<MacroObject> NewMacros() => MacrosThisSphere.Where(x => !CheckedMacros.Contains(x)).ToHashSet();
        }

    }
    public static class PlaythroughTools
    {

        public static object GetDefaultWincon(InstanceData.TrackerInstance instance)
        {
            var WinConAsItem = instance.GetLogicItemData(instance.LogicDictionary.WinCondition);
            if (WinConAsItem.Type.In(LogicItemTypes.item, LogicItemTypes.Area, LogicItemTypes.macro)) { return WinConAsItem.Object; }

            var WinConAsLocation = instance.GetCheckableLocationByID(instance.LogicDictionary.WinCondition, false);
            if (WinConAsLocation is not null) { return WinConAsLocation; }

            return null;
        }
        public static IEnumerable<object> FormatAdvancedUnlockData(object advancedUnlockData, Dictionary<string, List<string>> logicUnlockData)
        {
            throw new NotImplementedException();
        }

        public static AdvancedUnlockData GetAdvancedUnlockData(string ID, Dictionary<string, Dictionary<string, LogicItemData>> logicUnlockData, InstanceData.TrackerInstance instance)
        {
            if (!logicUnlockData.ContainsKey(ID)) { return null; }
            Dictionary<CheckableLocation, Dictionary<string, LogicItemData>> logicUnlockDataWithLocation =
                logicUnlockData.ToDictionary(x => instance.GetCheckableLocationByID(x.Key, false), x => x.Value);
            Dictionary<string, LogicItemData> SelectedItemUnlockData = new Dictionary<string, LogicItemData>(logicUnlockData[ID]);
            AdvancedUnlockData AdvUnlockData = new();

            AdvUnlockData.LogicUsed = logicUnlockData[ID].Select(x => x.Value.Type == LogicItemTypes.item ? $"{x.Key}, {x.Value.Amount}" : x.Key).ToList();

            LogicCalculation.CommitUnlockData(SelectedItemUnlockData, ParseDownMacros(ID, logicUnlockData, instance, out HashSet<string> ExitsTaken));

            foreach (var i in SelectedItemUnlockData)
            {
                switch (i.Value.Type)
                {
                    case LogicItemTypes.item:
                        AdvUnlockData.RealItemsUsed.Add(i.Key, i.Value.Amount);
                        break;
                    case LogicItemTypes.Area:
                        AdvUnlockData.AreasAccessed.Add(i.Key);
                        break;
                    case LogicItemTypes.macro:
                        string[] UsedMacroLogicString = logicUnlockData.TryGetValue(i.Key, out Dictionary<string, LogicItemData> UsedMacroUnlockData) ?
                            GetUsedLogicList(UsedMacroUnlockData) : [];
                        AdvUnlockData.MacrosUsed.Add(i.Key, UsedMacroLogicString);
                        break;
                    case LogicItemTypes.function:
                        AdvUnlockData.OptionsUsed.Add(i.Key);
                        break;
                    case LogicItemTypes.LogicEntryCollection:
                    case LogicItemTypes.Boolean:
                    case LogicItemTypes.error:
                        break;
                }
            }
            foreach(var i in ExitsTaken)
            {
                string[] UsedExitLogicString = logicUnlockData.TryGetValue(i, out Dictionary<string, LogicItemData> UsedExitUnlockData) ?
                            GetUsedLogicList(UsedExitUnlockData) : [];
                AdvUnlockData.ExitsTaken.Add(i, UsedExitLogicString);
            }

            return AdvUnlockData;
        }

        private static string[] GetUsedLogicList(Dictionary<string, LogicItemData> UnlockData)
        {
            return UnlockData.Select(x => x.Value.Type == LogicItemTypes.item ? $"{x.Key}, {x.Value.Amount}" : x.Key).ToArray();
        }

        private static Dictionary<string, LogicItemData> ParseDownMacros(string ID, Dictionary<string, Dictionary<string, LogicItemData>> logicUnlockDataWithLocation, InstanceData.TrackerInstance instance, out HashSet<string> TakenExits)
        {
            Dictionary<string, LogicItemData> ExtraEntries = [];
            HashSet<string> ExitsTaken = [];
            HashSet<string> MacrosScanned = [ID];
            HashSet<string> AreasScanned = [ID];
            HashSet<string> ExitsScanned = [ID];
            ScanEntryForMacros(ID, logicUnlockDataWithLocation);
            void ScanEntryForMacros(string ID, Dictionary<string, Dictionary<string, LogicItemData>> logicUnlockDataWithLocation)
            {
                var CurrentEntry = logicUnlockDataWithLocation[ID];
                LogicCalculation.CommitUnlockData(ExtraEntries, CurrentEntry);
                foreach (var i in CurrentEntry)
                {
                    if (i.Value.Type == LogicItemTypes.macro && !MacrosScanned.Contains(i.Key) && logicUnlockDataWithLocation.ContainsKey(i.Key))
                    {
                        MacrosScanned.Add(i.Key);
                        ScanEntryForMacros(i.Key, logicUnlockDataWithLocation);
                    }
                    else if (i.Value.Type == LogicItemTypes.Area && !AreasScanned.Contains(i.Key))
                    {
                        AreasScanned.Add(i.Key);
                        var Exit = GetFirstExitToThisArea(i.Key);
                        if (Exit is not null && logicUnlockDataWithLocation.ContainsKey(Exit)) 
                        {
                            ExitsTaken.Add(Exit);
                            ScanEntryForMacros(Exit, logicUnlockDataWithLocation); 
                        } 
                    }
                    else if (i.Value.Type == LogicItemTypes.function)
                    {
                        LogicFunctions.IsLogicFunction(i.Key, out string Func, out string[] Params);
                        if (Func == "available")
                        {
                            bool IsNegated = Params.Length > 1 && bool.TryParse(Params[1], out bool NegBool) && !NegBool;
                            if (!IsNegated && logicUnlockDataWithLocation.ContainsKey(Params[0]))
                            {
                                ScanEntryForMacros(Params[0], logicUnlockDataWithLocation);
                            }
                        }
                    }
                }
            }
            TakenExits = ExitsTaken;
            return ExtraEntries;

            string GetFirstExitToThisArea(string Area)
            {
                var AreaObj = instance.GetAreaByLogicID(Area);
                if (AreaObj is null || AreaObj.IsRoot) { return null; }
                foreach(var i in logicUnlockDataWithLocation)
                {
                    var Entry = instance.GetCheckableLocationByID(i.Key, false);
                    if (Entry.LocationType() != CheckableLocationTypes.Exit) { continue; }
                    EntranceData.EntranceRandoExit exit = Entry as EntranceData.EntranceRandoExit;
                    if (exit.DestinationExit is null || exit.DestinationExit.region != Area) { continue; }
                    if (exit.IsRandomizableEntrance() && exit.IsRandomized()) { return null; }
                    if (ExitsScanned.Contains(exit.ID)) { return null; }
                    ExitsScanned.Add(exit.ID);
                    return exit.ID;
                }
                return null;
            }
        }

        [Serializable]
        public class AdvancedUnlockData
        {
            public string Name { get; set; }
            public List<string> LogicUsed { get; set; } = new List<string>();
            public Dictionary<string, int> RealItemsUsed { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, string[]> MacrosUsed { get; set; } = new Dictionary<string, string[]>();
            public List<string> AreasAccessed { get; set; } = new List<string>();
            public List<string> Rootareas { get; set; } = new List<string>();
            public Dictionary<string, string[]> ExitsTaken { get; set; } = new Dictionary<string, string[]>();
            public List<string> OptionsUsed { get; set; } = new List<string>();
            public List<string> Unknown { get; set; } = new List<string>();
        }
    }
}
