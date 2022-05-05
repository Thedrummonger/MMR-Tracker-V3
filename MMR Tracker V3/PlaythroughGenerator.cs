using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public class PlaythroughGenerator
    {
        public LogicObjects.TrackerInstance _Instance;
        public Dictionary<string, PlaythroughObject> Playthrough = new Dictionary<string, PlaythroughObject>();
        public List<string> _IngoredChecks;
        public Dictionary<string, MMRData.JsonFormatLogicItem> LogicMap = new Dictionary<string, MMRData.JsonFormatLogicItem>();
        public Dictionary<string, List<string>> PlaythroughUnlockData = new Dictionary<string, List<string>>();
        public Dictionary<string, List<Tuple<object, PlaythroughObject>>> FirstObtainedDict = new Dictionary<string, List<Tuple<object, PlaythroughObject>>>();
        public PlaythroughGenerator(LogicObjects.TrackerInstance instance, List<string> IngoredChecks = null)
        {
            _Instance = Newtonsoft.Json.JsonConvert.DeserializeObject<LogicObjects.TrackerInstance>(Newtonsoft.Json.JsonConvert.SerializeObject(instance));
            _IngoredChecks = IngoredChecks??new List<string>();
        }

        public class PlaythroughObject
        {
            public string id { get; set; }
            public int sphere { get; set; }
            public MiscData.LogicEntryType CheckType { get; set; }
            public string ItemObtained { get; set; }
            public List<string> UsedItems { get; set; }
            public bool Important { get; set; }
            public PlaythroughTools.AdvancedUnlockData advancedUnlockData { get; set; }
        }

        public void GeneratePlaythrough()
        {
            Stopwatch stopwatch = new Stopwatch();
            Utility.TimeCodeExecution(stopwatch);
            int Sphere = 0;
            PrepareInstance();
            Playthrough.Clear();
            LogicCalculation.FillLogicMap(_Instance, LogicMap);
            Dictionary<object, int> AutoObtainedObjects = new Dictionary<object, int>();
            LogicCalculation.CalculateLogic(_Instance, PlaythroughUnlockData, AutoObtainedObjects);

            var AvailableLocations = _Instance.LocationPool.Values.Where(x => x.Available && x.CheckState == MiscData.CheckState.Unchecked && x.IsRandomized());
            var AvailableEntrances = getAllAvailableEntrances(_Instance, AutoObtainedObjects);
            var AquiredMacros = _Instance.MacroPool.Values.Where(x => x.Aquired && !Playthrough.ContainsKey(x.ID));

            while (AvailableLocations.Any() || AvailableEntrances.Any())
            {
                //Debug.WriteLine($"Sphere: {Sphere} ============================================");
                foreach (var i in AvailableEntrances)
                {
                    //Debug.WriteLine($"Checking: {$"{i.ParentAreaID} X {i.ID}"}");
                    if (i.GetDestinationAtExit(_Instance) == null)
                    {
                        throw new Exception($"{$"{i.ParentAreaID} X {i.ID}"} had no item");
                    }
                    if (_Instance.EntrancePool.AreaList[i.ParentAreaID].LoadingZoneExits.ContainsKey(i.ID) && _Instance.EntrancePool.AreaList[i.ParentAreaID].LoadingZoneExits[i.ID].IsRandomized())
                    {
                        i.ToggleExitChecked(MiscData.CheckState.Checked, _Instance);
                    }
                    i.CheckState = MiscData.CheckState.Checked;
                    PlaythroughObject playthroughObject = new PlaythroughObject
                    {
                        id = _Instance.GetLogicNameFromExit(i),
                        ItemObtained = i.GetDestinationAtExit(_Instance).region,
                        UsedItems = PlaythroughUnlockData[_Instance.GetLogicNameFromExit(i)],
                        CheckType = MiscData.LogicEntryType.Exit,
                        sphere = Sphere
                    };
                    if (!FirstObtainedDict.ContainsKey(playthroughObject.ItemObtained)) { FirstObtainedDict.Add(playthroughObject.ItemObtained, new List<Tuple<object, PlaythroughObject>>()); }
                    FirstObtainedDict[playthroughObject.ItemObtained].Add(new Tuple<object, PlaythroughObject>(i, playthroughObject));
                    Playthrough.Add(_Instance.GetLogicNameFromExit(i), playthroughObject);
                }
                foreach (var i in AvailableLocations)
                {
                    //Debug.WriteLine($"Checking: {i.ID}");
                    i.ToggleChecked(MiscData.CheckState.Checked, _Instance);
                    i.CheckState = MiscData.CheckState.Checked;
                    if (i.GetItemAtCheck(_Instance) == null)
                    {
                        throw new Exception($"{i.ID} had no item");
                    }
                    PlaythroughObject playthroughObject = new PlaythroughObject
                    {
                        id = i.ID,
                        ItemObtained = i.GetItemAtCheck(_Instance),
                        UsedItems = PlaythroughUnlockData[i.ID],
                        CheckType = MiscData.LogicEntryType.location,
                        sphere = Sphere
                    };
                    if (!FirstObtainedDict.ContainsKey(playthroughObject.ItemObtained)) { FirstObtainedDict.Add(playthroughObject.ItemObtained, new List<Tuple<object, PlaythroughObject>>()); }
                    FirstObtainedDict[playthroughObject.ItemObtained].Add(new Tuple<object, PlaythroughObject>(i, playthroughObject));
                    Playthrough.Add(i.ID, playthroughObject);
                }
                foreach (var i in AquiredMacros)
                {
                    PlaythroughObject playthroughObject = new PlaythroughObject
                    {
                        id = i.ID,
                        ItemObtained = i.ID,
                        UsedItems = PlaythroughUnlockData[i.ID],
                        CheckType = MiscData.LogicEntryType.macro,
                        sphere = Sphere
                    };
                    if (!FirstObtainedDict.ContainsKey(playthroughObject.ItemObtained)) { FirstObtainedDict.Add(playthroughObject.ItemObtained, new List<Tuple<object, PlaythroughObject>>()); }
                    FirstObtainedDict[playthroughObject.ItemObtained].Add(new Tuple<object, PlaythroughObject> ( i, playthroughObject));
                    Playthrough.Add(i.ID, playthroughObject);
                }

                LogicCalculation.CalculateLogic(_Instance, PlaythroughUnlockData, AutoObtainedObjects);
                AvailableLocations = _Instance.LocationPool.Values.Where(x => x.Available && x.CheckState == MiscData.CheckState.Unchecked && x.IsRandomized());
                AvailableEntrances = getAllAvailableEntrances(_Instance, AutoObtainedObjects);
                AquiredMacros = _Instance.MacroPool.Values.Where(x => x.Aquired && !Playthrough.ContainsKey(x.ID));


                Sphere++;
            }
            Utility.TimeCodeExecution(stopwatch, "Generate Playthough", -1);
            GetRealItemData();
            //File.WriteAllText(@"D:\Testing\Playtrhough.txt", Newtonsoft.Json.JsonConvert.SerializeObject(Playthrough, Testing._NewtonsoftJsonSerializerOptions));
        }

        public bool FilterImportantPlaythrough(object WinObj)
        {
            PlaythroughObject WinCon = null;
            Debug.WriteLine(WinObj is LocationData.LocationObject);
            Debug.WriteLine(WinObj is MacroObject);
            Debug.WriteLine(WinObj is ItemData.ItemObject);
            Debug.WriteLine(WinObj is EntranceData.EntranceRandoArea);
            if (WinObj is LocationData.LocationObject LocWin && Playthrough.ContainsKey(LocWin.ID))
            {
                WinCon = Playthrough[LocWin.ID];
            }
            else if (WinObj is MacroObject MacroWin && Playthrough.ContainsKey(MacroWin.ID))
            {
                WinCon = Playthrough[MacroWin.ID];
            }
            else if (WinObj is ItemData.ItemObject itemWin && FirstObtainedDict.ContainsKey(itemWin.Id))
            {
                WinCon = FirstObtainedDict[itemWin.Id].First().Item2;
            }
            else if (WinObj is EntranceData.EntranceRandoArea AreaWin && FirstObtainedDict.ContainsKey(AreaWin.ID))
            {
                WinCon = FirstObtainedDict[AreaWin.ID].First().Item2;
            }
            else
            {
                return false;
            }

            Debug.WriteLine($"Wincon is {WinCon.id}");

            foreach(var i in Playthrough.Values)
            {
                i.Important = false;
            }

            WinCon.Important = true;

            List<PlaythroughObject> HandledImportantChecks = new List<PlaythroughObject>();
            List<PlaythroughObject> ImportantChecks() { return Playthrough.Values.Where(x => x.Important).ToList(); }
            List<PlaythroughObject> UnhandledImportantChecks() { return ImportantChecks().Where(x => !HandledImportantChecks.Contains(x)).ToList(); }

            while (UnhandledImportantChecks().Any())
            {
                foreach(var i in UnhandledImportantChecks())
                {
                    MarkCheckwithRequiredItems(i);
                }
            }

            void MarkCheckwithRequiredItems(PlaythroughObject ImportantCheck)
            {
                foreach(var i in ImportantCheck.advancedUnlockData.RealItemsUsed)
                {
                    int StartingAmount = _Instance.GetItemByID(i.Key)?.AmountInStartingpool??0;
                    if (FirstObtainedDict.ContainsKey(i.Key))
                    {
                        for(var j = 0; j < i.Value; j++)
                        {
                            if (StartingAmount > 0) { StartingAmount--; continue; }
                            FirstObtainedDict[i.Key][j].Item2.Important = true;
                        }
                    }
                }
                foreach (var i in ImportantCheck.advancedUnlockData.AreasAccessed)
                {
                    if (FirstObtainedDict.ContainsKey(i))
                    {
                        FirstObtainedDict[i].First().Item2.Important = true;
                    }
                }
                HandledImportantChecks.Add(ImportantCheck);
            }

            return true;

        }

        private void GetRealItemData()
        {
            foreach(var i in Playthrough)
            {
                i.Value.advancedUnlockData = PlaythroughTools.GetAdvancedUnlockData(i.Key, PlaythroughUnlockData, _Instance, this);
            }
        }

        private List<EntranceData.EntranceRandoExit> getAllAvailableEntrances(LogicObjects.TrackerInstance instance, Dictionary<object, int> AutoObtainedObjects)
        {
            var AvailableEntrances = instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values.Where(x => x.Available && x.CheckState == MiscData.CheckState.Unchecked && x.IsRandomized()));
            var AutoObtainedEntrance = AutoObtainedObjects.Keys.Where(x => x is EntranceData.EntranceRandoExit && !Playthrough.ContainsKey(_Instance.GetLogicNameFromExit(x as EntranceData.EntranceRandoExit))).Select(x => x as EntranceData.EntranceRandoExit);
            //var AquiredEntranceMacros = instance.EntrancePool.AreaList.Values.SelectMany(x => x.MacroExits.Values.Where(x => x.CheckState == MiscData.CheckState.Checked && !Playthrough.ContainsKey(GetEntId(x))));
            //var UnrandEntranceMacros = instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values.Where(x => x.CheckState == MiscData.CheckState.Checked && !Playthrough.ContainsKey(GetEntId(x)) && x.IsUnrandomized()));
            //var AllEntrances = AvailableEntrances.Concat(AquiredEntranceMacros).Concat(UnrandEntranceMacros).ToList();
            var AllEntrances = AvailableEntrances.Concat(AutoObtainedEntrance).ToList();
            Debug.WriteLine(AutoObtainedEntrance.Count());
            return AllEntrances;
        }

        public void PrepareInstance()
        {
            foreach(var i in _Instance.ItemPool.Values)
            {
                i.AmountAquiredLocally = 0;
            }
            foreach (var i in _Instance.EntrancePool.AreaList)
            {
                i.Value.ExitsAcessibleFrom = 0;
            }
            foreach (var i in _Instance.LocationPool.Values)
            {
                i.Available = false;
                i.CheckState = TrackerObjects.MiscData.CheckState.Unchecked;
                if (i.GetItemAtCheck(_Instance) == null) { i.RandomizedState = TrackerObjects.MiscData.RandomizedState.ForcedJunk; }
                else
                {
                    i.Randomizeditem.Item = i.GetItemAtCheck(_Instance);
                    i.RandomizedState = TrackerObjects.MiscData.RandomizedState.Randomized;
                }
                if (_IngoredChecks.Contains(i.ID)) { i.RandomizedState = TrackerObjects.MiscData.RandomizedState.ForcedJunk; }
            }
            foreach (var i in _Instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values))
            {
                i.Available = false;
                i.CheckState = TrackerObjects.MiscData.CheckState.Unchecked;
                if (i.GetDestinationAtExit(_Instance) == null) { i.RandomizedState = TrackerObjects.MiscData.RandomizedState.ForcedJunk; }
                else
                {
                    i.DestinationExit = i.GetDestinationAtExit(_Instance);
                    if (i.RandomizedState == MiscData.RandomizedState.UnrandomizedManual) { i.RandomizedState = MiscData.RandomizedState.Randomized; }
                    //i.RandomizedState = TrackerObjects.MiscData.RandomizedState.Randomized;
                }
                if (_IngoredChecks.Contains(_Instance.GetLogicNameFromExit(i))) { i.RandomizedState = TrackerObjects.MiscData.RandomizedState.ForcedJunk; }
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

        public List<string> CreateReadablePlaythrough(Dictionary<string, PlaythroughObject> FormatPlaythrough)
        {
            List<string> readablePlaythrough = new List<string>();
            int sphere = -1;
            foreach(var p in FormatPlaythrough)
            {
                if (sphere != p.Value.sphere) { 
                    if (sphere > -1) { readablePlaythrough.Add(String.Empty); }
                    readablePlaythrough.Add($"SPHERE: {p.Value.sphere}");
                    sphere = p.Value.sphere;
                }
                string AreaName = (_Instance.GetLocationByID(p.Key)?.GetDictEntry(_Instance)?.Area);
                string LocationName = _Instance.GetLocationByID(p.Key)?.GetDictEntry(_Instance)?.Name??p.Key;
                string LocationDisplay = AreaName is null ? LocationName : $"{AreaName} - {LocationName}";
                string ItemName = _Instance.GetItemByID(p.Value.ItemObtained)?.GetDictEntry(_Instance)?.GetItemName(_Instance)??p.Value.ItemObtained;
                List<string> RealItems = new List<string>();

                foreach(var i in p.Value.advancedUnlockData.RealItemsUsed)
                {
                    string display = _Instance.GetItemByID(i.Key)?.GetDictEntry(_Instance)?.GetItemName(_Instance)??i.Key;
                    if (i.Value > 1) { display += $" X{i.Value}"; }
                    RealItems.Add(display);
                }

                readablePlaythrough.Add($"Check [{LocationDisplay.ToUpper()}] to obtain [{ItemName.ToUpper()}]");
                if (RealItems.Any()) { readablePlaythrough.Add($"Using: [{string.Join(", ", RealItems)}]"); }
            }
            return readablePlaythrough;
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
        public static AdvancedUnlockData GetAdvancedUnlockData(string ID, Dictionary<string, List<string>> UnlockData, LogicObjects.TrackerInstance instance, PlaythroughGenerator playthroughObject = null)
        {
            if (playthroughObject == null)
            {
                playthroughObject = new PlaythroughGenerator(instance);
                playthroughObject.GeneratePlaythrough();
            }

            var MacroExits = instance.EntrancePool.AreaList.Values.SelectMany(x => x.MacroExits.Values).Where(x => x.CheckState == MiscData.CheckState.Checked);
            MacroExits = MacroExits.Concat(instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values).Where(x => x.CheckState == MiscData.CheckState.Checked && x.IsUnrandomized()));
            var LoadingZoneExits = instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values).Where(x => x.CheckState == MiscData.CheckState.Checked && x.RandomizedState == MiscData.RandomizedState.Randomized);

            var AreasParsed = new List<string>();

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
                    if (UnlockData.ContainsKey(LogicItem))
                    {
                        foreach (var i in UnlockData[LogicItem]) { ParseItem(i, false); }
                    }
                }
                else if (type == MiscData.LogicEntryType.Option && !Data.OptionsUsed.Contains(LogicItem))
                {
                    Data.OptionsUsed.Add(LogicItem);
                }
                else if (type == MiscData.LogicEntryType.Area)
                {
                    if (AreasParsed.Contains(LogicItem)) { return; }
                    AreasParsed.Add(LogicItem);
                    //Debug.WriteLine($"Geting Path to {LogicItem}");
                    var PathToUnrandomizedExit = GetPathFromRandomizedEntrance(LogicItem, playthroughObject, instance);
                    if (!PathToUnrandomizedExit.Any())
                    {
                        var ReqArea = GetClosestRandomizedArea(LogicItem, playthroughObject, instance);
                        //Debug.WriteLine($"Path Was Empty Closet Unrand Area was {(ReqArea == null ? "root" : $"{Newtonsoft.Json.JsonConvert.SerializeObject(ReqArea)}")}");
                        if (ReqArea != null && !Data.AreasAccessed.Contains(ReqArea.DestinationExit.region)) { Data.AreasAccessed.Add(ReqArea.DestinationExit.region); }
                    }
                    else
                    {
                        //Debug.WriteLine($"Path Contained {PathToUnrandomizedExit.Count} Steps");
                        foreach (var i in PathToUnrandomizedExit)
                        {
                            if (UnlockData.ContainsKey(instance.GetLogicNameFromExit(i)))
                            {
                                //Debug.WriteLine($"Parsing {$"{i.Area} X {i.Exit}"}");
                                foreach (var j in UnlockData[instance.GetLogicNameFromExit(i)]) { ParseItem(j, false); }
                            }
                        }
                    }
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

        public static List<EntranceData.EntranceAreaPair> GetPathFromRoot(string area, PlaythroughGenerator playthroughObject)
        {
            List<EntranceData.EntranceAreaPair> path = new List<EntranceData.EntranceAreaPair>();
            List<string> areasViited = new List<string>();
            while (true)
            {
                if (playthroughObject.FirstObtainedDict.ContainsKey(area))
                {
                    bool NewAreaFound = false;
                    foreach(var i in playthroughObject.FirstObtainedDict[area])
                    {
                        if (i.Item1 is EntranceData.EntranceRandoExit eo && !areasViited.Contains(eo.ParentAreaID))
                        {
                            areasViited.Add(eo.ParentAreaID);
                            area = eo.ParentAreaID;
                            path.Add(new EntranceData.EntranceAreaPair() { Area = eo.ParentAreaID, Exit = eo.ID });
                            NewAreaFound = true;
                            break;
                        }
                    }
                    if (!NewAreaFound) { break; }
                }
                else { break; }
            }
            path.Reverse();
            return path;
        }
        public static List<EntranceData.EntranceAreaPair> GetPathFromRandomizedEntrance(string area, PlaythroughGenerator playthroughObject, LogicObjects.TrackerInstance instance)
        {
            List<EntranceData.EntranceAreaPair> NewPath = new List<EntranceData.EntranceAreaPair>();
            var path = GetPathFromRoot(area, playthroughObject);
            path.Reverse();
            foreach(var i in path)
            {
                if (instance.EntrancePool.AreaList[i.Area].LoadingZoneExits.ContainsKey(i.Exit) && 
                    (instance.EntrancePool.AreaList[i.Area].LoadingZoneExits[i.Exit].IsRandomized())) { break; }
                else { NewPath.Add(i); }
            }
            NewPath.Reverse();
            return NewPath;
        }
        public static EntranceData.EntranceRandoExit GetClosestRandomizedArea(string area, PlaythroughGenerator playthroughObject, LogicObjects.TrackerInstance instance)
        {
            List<EntranceData.EntranceAreaPair> NewPath = new List<EntranceData.EntranceAreaPair>();
            var path = GetPathFromRoot(area, playthroughObject);
            path.Reverse();
            foreach (var i in path)
            {
                if (instance.EntrancePool.AreaList[i.Area].LoadingZoneExits.ContainsKey(i.Exit) &&
                    (instance.EntrancePool.AreaList[i.Area].LoadingZoneExits[i.Exit].IsRandomized()))
                { return instance.EntrancePool.AreaList[i.Area].LoadingZoneExits[i.Exit]; }
            }
            return null;
        }
    }
}
