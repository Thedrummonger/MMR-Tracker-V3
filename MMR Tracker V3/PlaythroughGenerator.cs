using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public class PlaythroughGenerator
    {
        private InstanceData.TrackerInstance instance;
        public List<string> _IngoredChecks;
        public Dictionary<string, List<Tuple<object, PlaythroughObject>>> FirstObtainedDict = new Dictionary<string, List<Tuple<object, PlaythroughObject>>>();
        public Dictionary<string, PlaythroughObject> Playthrough = new Dictionary<string, PlaythroughObject>();
        public PlaythroughGenerator(InstanceData.TrackerInstance instance, List<string> ignoredChecks = null)
        {
            this.instance = instance;
            _IngoredChecks = ignoredChecks ?? [];
        }

        public bool FilterImportantPlaythrough(object tag)
        {
            throw new NotImplementedException();
        }

        public void GeneratePlaythrough()
        {
            throw new NotImplementedException();
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
            public string ItemObtained { get; set; }
            public List<string> UsedItems { get; set; }
            public bool Important { get; set; }
            public PlaythroughTools.AdvancedUnlockData advancedUnlockData { get; set; }
        }

    }
    public static class PlaythroughTools
    {
        public static IEnumerable<object> FormatAdvancedUnlockData(object advancedUnlockData, Dictionary<string, List<string>> logicUnlockData)
        {
            throw new NotImplementedException();
        }

        public static object GetAdvancedUnlockData(string ID, Dictionary<string, Dictionary<string, LogicItemData>> logicUnlockData, InstanceData.TrackerInstance instance)
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
                        AdvUnlockData.RealItemsUsed.Add(((ItemData.ItemObject)i.Value.Object).GetDictEntry().GetName(), i.Value.Amount);
                        break;
                    case LogicItemTypes.Area:
                        AdvUnlockData.AreasAccessed.Add(((EntranceData.EntranceRandoArea)i.Value.Object).ID);
                        break;
                    case LogicItemTypes.macro:
                        string[] UsedMacroLogicString = logicUnlockData.TryGetValue(i.Key, out Dictionary<string, LogicItemData> UsedMacroUnlockData) ?
                            GetUsedLogicList(UsedMacroUnlockData) : [];
                        AdvUnlockData.MacrosUsed.Add(((MacroObject)i.Value.Object).GetName(), UsedMacroLogicString);
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
