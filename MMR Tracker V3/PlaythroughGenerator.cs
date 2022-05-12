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
        public MiscData.InstanceContainer _Instance = new MiscData.InstanceContainer();
        public Dictionary<string, PlaythroughObject> Playthrough = new Dictionary<string, PlaythroughObject>();
        public List<string> _IngoredChecks;
        public Dictionary<string, List<Tuple<object, PlaythroughObject>>> FirstObtainedDict = new Dictionary<string, List<Tuple<object, PlaythroughObject>>>();
        public PlaythroughGenerator(LogicObjects.TrackerInstance instance, List<string> IngoredChecks = null)
        {
            _Instance.Instance = Newtonsoft.Json.JsonConvert.DeserializeObject<LogicObjects.TrackerInstance>(Newtonsoft.Json.JsonConvert.SerializeObject(instance));
            _IngoredChecks = IngoredChecks??new List<string>();
            _Instance.logicCalculation = new LogicCalculation(_Instance);
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
            _Instance.logicCalculation.CalculateLogic(MiscData.CheckState.Checked);

            var AvailableLocations = _Instance.Instance.LocationPool.Values.Where(x => x.Available && x.CheckState == MiscData.CheckState.Unchecked && x.IsRandomized());
            var AvailableEntrances = getAllAvailableEntrances(_Instance.Instance, _Instance.logicCalculation.AutoObtainedObjects);
            var AquiredMacros = _Instance.Instance.MacroPool.Values.Where(x => x.Aquired && !Playthrough.ContainsKey(x.ID));

            while (AvailableLocations.Any() || AvailableEntrances.Any())
            {
                //Debug.WriteLine($"Sphere: {Sphere} ============================================");
                foreach (var i in AvailableEntrances)
                {
                    //Debug.WriteLine($"Checking: {$"{i.ParentAreaID} X {i.ID}"}");
                    if (i.GetDestinationAtExit(_Instance.Instance) == null)
                    {
                        throw new Exception($"{$"{i.ParentAreaID} X {i.ID}"} had no item");
                    }
                    if (_Instance.Instance.EntrancePool.AreaList[i.ParentAreaID].LoadingZoneExits.ContainsKey(i.ID) && _Instance.Instance.EntrancePool.AreaList[i.ParentAreaID].LoadingZoneExits[i.ID].IsRandomized())
                    {
                        i.ToggleExitChecked(MiscData.CheckState.Checked, _Instance.Instance);
                    }
                    i.CheckState = MiscData.CheckState.Checked;
                    PlaythroughObject playthroughObject = new PlaythroughObject
                    {
                        id = _Instance.Instance.GetLogicNameFromExit(i),
                        ItemObtained = i.GetDestinationAtExit(_Instance.Instance).region,
                        UsedItems = _Instance.logicCalculation.LogicUnlockData[_Instance.Instance.GetLogicNameFromExit(i)],
                        CheckType = MiscData.LogicEntryType.Exit,
                        sphere = Sphere
                    };
                    if (!FirstObtainedDict.ContainsKey(playthroughObject.ItemObtained)) { FirstObtainedDict.Add(playthroughObject.ItemObtained, new List<Tuple<object, PlaythroughObject>>()); }
                    FirstObtainedDict[playthroughObject.ItemObtained].Add(new Tuple<object, PlaythroughObject>(i, playthroughObject));
                    Playthrough.Add(_Instance.Instance.GetLogicNameFromExit(i), playthroughObject);
                }
                foreach (var i in AvailableLocations)
                {
                    //Debug.WriteLine($"Checking: {i.ID}");
                    i.ToggleChecked(MiscData.CheckState.Checked, _Instance.Instance);
                    i.CheckState = MiscData.CheckState.Checked;
                    if (i.GetItemAtCheck(_Instance.Instance) == null)
                    {
                        throw new Exception($"{i.ID} had no item");
                    }
                    PlaythroughObject playthroughObject = new PlaythroughObject
                    {
                        id = i.ID,
                        ItemObtained = i.GetItemAtCheck(_Instance.Instance),
                        UsedItems = _Instance.logicCalculation.LogicUnlockData[i.ID],
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
                        UsedItems = _Instance.logicCalculation.LogicUnlockData[i.ID],
                        CheckType = MiscData.LogicEntryType.macro,
                        sphere = Sphere
                    };
                    if (!FirstObtainedDict.ContainsKey(playthroughObject.ItemObtained)) { FirstObtainedDict.Add(playthroughObject.ItemObtained, new List<Tuple<object, PlaythroughObject>>()); }
                    FirstObtainedDict[playthroughObject.ItemObtained].Add(new Tuple<object, PlaythroughObject> ( i, playthroughObject));
                    Playthrough.Add(i.ID, playthroughObject);
                }

                _Instance.logicCalculation.CalculateLogic(MiscData.CheckState.Checked);
                AvailableLocations = _Instance.Instance.LocationPool.Values.Where(x => x.Available && x.CheckState == MiscData.CheckState.Unchecked && x.IsRandomized());
                AvailableEntrances = getAllAvailableEntrances(_Instance.Instance, _Instance.logicCalculation.AutoObtainedObjects);
                AquiredMacros = _Instance.Instance.MacroPool.Values.Where(x => x.Aquired && !Playthrough.ContainsKey(x.ID));


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
                    int StartingAmount = _Instance.Instance.GetItemByID(i.Key)?.AmountInStartingpool??0;
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
                    if (FirstObtainedDict.ContainsKey(i) && FirstObtainedDict[i].First().Item1 is EntranceData.EntranceRandoExit ere && !ere.isMacroExit(_Instance.Instance) && (ere.IsRandomized() || ere.IsUnrandomized(2)))
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
                i.Value.advancedUnlockData = PlaythroughTools.GetAdvancedUnlockData(i.Key, _Instance.logicCalculation.LogicUnlockData, _Instance.Instance, this);
            }
        }

        private List<EntranceData.EntranceRandoExit> getAllAvailableEntrances(LogicObjects.TrackerInstance instance, Dictionary<object, int> AutoObtainedObjects)
        {
            var AvailableEntrances = instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values.Where(x => x.Available && x.CheckState == MiscData.CheckState.Unchecked && x.IsRandomized()));
            var AutoObtainedEntrance = AutoObtainedObjects.Keys.Where(x => x is EntranceData.EntranceRandoExit && !Playthrough.ContainsKey(_Instance.Instance.GetLogicNameFromExit(x as EntranceData.EntranceRandoExit))).Select(x => x as EntranceData.EntranceRandoExit);
            //var AquiredEntranceMacros = instance.EntrancePool.AreaList.Values.SelectMany(x => x.MacroExits.Values.Where(x => x.CheckState == MiscData.CheckState.Checked && !Playthrough.ContainsKey(GetEntId(x))));
            //var UnrandEntranceMacros = instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values.Where(x => x.CheckState == MiscData.CheckState.Checked && !Playthrough.ContainsKey(GetEntId(x)) && x.IsUnrandomized()));
            //var AllEntrances = AvailableEntrances.Concat(AquiredEntranceMacros).Concat(UnrandEntranceMacros).ToList();
            var AllEntrances = AvailableEntrances.Concat(AutoObtainedEntrance).ToList();
            Debug.WriteLine(AutoObtainedEntrance.Count());
            return AllEntrances;
        }

        public void PrepareInstance()
        {
            foreach(var i in _Instance.Instance.ItemPool.Values)
            {
                i.AmountAquiredLocally = 0;
            }
            foreach (var i in _Instance.Instance.EntrancePool.AreaList)
            {
                i.Value.ExitsAcessibleFrom = 0;
            }
            foreach (var i in _Instance.Instance.LocationPool.Values)
            {
                i.Available = false;
                i.CheckState = TrackerObjects.MiscData.CheckState.Unchecked;
                if (i.GetItemAtCheck(_Instance.Instance) == null) { i.RandomizedState = TrackerObjects.MiscData.RandomizedState.ForcedJunk; }
                else
                {
                    i.Randomizeditem.Item = i.GetItemAtCheck(_Instance.Instance);
                    i.RandomizedState = TrackerObjects.MiscData.RandomizedState.Randomized;
                }
                if (_IngoredChecks.Contains(i.ID)) { i.RandomizedState = TrackerObjects.MiscData.RandomizedState.ForcedJunk; }
            }
            foreach (var i in _Instance.Instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values))
            {
                i.Available = false;
                i.CheckState = TrackerObjects.MiscData.CheckState.Unchecked;
                if (i.GetDestinationAtExit(_Instance.Instance) == null) { i.RandomizedState = TrackerObjects.MiscData.RandomizedState.ForcedJunk; }
                else
                {
                    i.DestinationExit = i.GetDestinationAtExit(_Instance.Instance);
                    if (i.RandomizedState == MiscData.RandomizedState.UnrandomizedManual) { i.RandomizedState = MiscData.RandomizedState.Randomized; }
                    //i.RandomizedState = TrackerObjects.MiscData.RandomizedState.Randomized;
                }
                if (_IngoredChecks.Contains(_Instance.Instance.GetLogicNameFromExit(i))) { i.RandomizedState = TrackerObjects.MiscData.RandomizedState.ForcedJunk; }
            }
            foreach (var i in _Instance.Instance.EntrancePool.AreaList.Values.SelectMany(x => x.MacroExits.Values))
            {
                i.Available = false;
                i.CheckState = TrackerObjects.MiscData.CheckState.Unchecked;
            }
            foreach (var i in _Instance.Instance.MacroPool.Values)
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
                string AreaName = (_Instance.Instance.GetLocationByID(p.Key)?.GetDictEntry(_Instance.Instance)?.Area);
                string LocationName = _Instance.Instance.GetLocationByID(p.Key)?.GetDictEntry(_Instance.Instance)?.GetName(_Instance.Instance)??p.Key;
                string LocationDisplay = AreaName is null ? LocationName : $"{AreaName} - {LocationName}";
                string ItemName = _Instance.Instance.GetItemByID(p.Value.ItemObtained)?.GetDictEntry(_Instance.Instance)?.GetName(_Instance.Instance)??p.Value.ItemObtained;
                List<string> RealItems = new List<string>();

                foreach(var i in p.Value.advancedUnlockData.RealItemsUsed)
                {
                    string display = _Instance.Instance.GetItemByID(i.Key)?.GetDictEntry(_Instance.Instance)?.GetName(_Instance.Instance)??i.Key;
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
        public class AdvancedUnlockData
        {
            public string Name { get; set; }
            public List<string> LogicUsed { get; set; } = new List<string>();
            public Dictionary<string, int> RealItemsUsed { get; set; } = new Dictionary<string, int>();
            public List<string> MacrosUsed { get; set; } = new List<string>();
            public List<string> AreasAccessed { get; set; } = new List<string>();
            public List<string> Rootareas { get; set; } = new List<string>();
            public List<string> ExitsTaken { get; set; } = new List<string>();
            public List<string> OptionsUsed { get; set; } = new List<string>();
        }
        public static AdvancedUnlockData GetAdvancedUnlockData(string ID, Dictionary<string, List<string>> UnlockData, LogicObjects.TrackerInstance instance, PlaythroughGenerator playthroughObject = null)
        {
            if (playthroughObject == null)
            {
                var UncheckedLocations = GetUncheckedLocations(instance);
                UncheckedLocations.Remove(ID);
                playthroughObject = new PlaythroughGenerator(instance, UncheckedLocations);
                playthroughObject.GeneratePlaythrough();
            }
            AdvancedUnlockData Data = new AdvancedUnlockData() { Name = ID };
            if (!UnlockData.ContainsKey(ID)) { return Data; }
            Data.LogicUsed = UnlockData[ID];
            ParseRequirements(ID);
            return Data;

            void ParseRequirements(string SubID)
            {
                if (!UnlockData.ContainsKey(SubID)) { return; }
                foreach (var i in UnlockData[SubID])
                {
                    instance.MultipleItemEntry(i, out string LogicItem, out int Amount);
                    bool Literal = LogicItem.IsLiteralID(out LogicItem);
                    var type = instance.GetItemEntryType(LogicItem, Literal, out _);
                    if (type == MiscData.LogicEntryType.macro && !Data.MacrosUsed.Contains(LogicItem))
                    {
                        Data.MacrosUsed.Add(LogicItem);
                        ParseRequirements(LogicItem);
                    }
                    else if (type == MiscData.LogicEntryType.Option && !Data.OptionsUsed.Contains(LogicItem))
                    {
                        Data.OptionsUsed.Add(LogicItem);
                    }
                    else if (type == MiscData.LogicEntryType.item)
                    {
                        if (Data.RealItemsUsed.ContainsKey(LogicItem)) { if (Amount > Data.RealItemsUsed[LogicItem]) { Data.RealItemsUsed[LogicItem] = Amount; } }
                        else { Data.RealItemsUsed.Add(LogicItem, Amount); }
                    }
                    else if (type == MiscData.LogicEntryType.Area && !Data.AreasAccessed.Contains(LogicItem))
                    {
                        GetAreaData(LogicItem, playthroughObject, instance, out List<EntranceData.EntranceAreaPair> path, out List<string> areasVisited);
                        Data.AreasAccessed.AddRange(areasVisited);
                        foreach(var p in path)
                        {
                            string PathID = instance.GetLogicNameFromExit(p);
                            if (!Data.ExitsTaken.Contains(PathID))
                            {
                                Data.ExitsTaken.Add(PathID);
                                ParseRequirements(PathID);
                            }
                        }
                    }
                }
            }
        }

        public static void GetAreaData(string area,  PlaythroughGenerator playthroughObject, LogicObjects.TrackerInstance instance, out List<EntranceData.EntranceAreaPair> outPath, out List<string> outAreasVisited)
        {
            List<EntranceData.EntranceAreaPair> path = new List<EntranceData.EntranceAreaPair>();
            List<string> areasVisited = new List<string>();
            CheckArea(area);

            void CheckArea(string area)
            {
                areasVisited.Add(area);
                if (!playthroughObject.FirstObtainedDict.ContainsKey(area) || instance.EntrancePool.RootArea == area) { return; }
                foreach (var i in playthroughObject.FirstObtainedDict[area])
                {
                    if (i.Item1 is not EntranceData.EntranceRandoExit eo || areasVisited.Contains(eo.ParentAreaID) || !instance.EntrancePool.AreaList.ContainsKey(eo.ParentAreaID)) { continue; }
                    
                    bool ExitIsRandom = instance.EntrancePool.AreaList[eo.ParentAreaID].LoadingZoneExits.ContainsKey(eo.ID) && instance.EntrancePool.AreaList[eo.ParentAreaID].LoadingZoneExits[eo.ID].IsRandomized();
                    if (ExitIsRandom) { return; }
                    path.Add(new EntranceData.EntranceAreaPair() { Area = eo.ParentAreaID, Exit = eo.ID });
                    CheckArea(eo.ParentAreaID);
                    break;
                }
            }
            outPath = path;
            outAreasVisited = areasVisited;
            outPath.Reverse();
            outAreasVisited.Reverse();
        }

        private static List<string> GetUncheckedLocations(LogicObjects.TrackerInstance instance)
        {
            List<string> Uncheckedlocations = new List<string>();
            foreach (var i in instance.LocationPool.Values)
            {
                if (i.CheckState != MiscData.CheckState.Checked) { Uncheckedlocations.Add(i.ID); }
            }
            foreach (var i in instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values))
            {
                if (i.CheckState != MiscData.CheckState.Checked) { Uncheckedlocations.Add(instance.GetLogicNameFromExit(i)); }
            }
            return Uncheckedlocations;
        }

        public static List<dynamic> FormatAdvancedUnlockData(AdvancedUnlockData ADVUnlockData, Dictionary<string, List<string>> UnlockData, string Divider = "")
        {
            List<dynamic> ReturnList = new List<dynamic>();
            ReturnList.Add(ADVUnlockData.Name);
            ReturnList.Add(new MiscData.Divider() { Display = Divider });
            ReturnList.Add(new MiscData.Divider(){ Display = "LOGIC USED:" });
            foreach (var i in ADVUnlockData.LogicUsed)
            {
                ReturnList.Add(i);
            }
            ReturnList.Add(new MiscData.Divider() { Display = Divider });
            ReturnList.Add(new MiscData.Divider() { Display = "REAL ITEMS USED:" });
            foreach (var i in ADVUnlockData.RealItemsUsed)
            {
                ReturnList.Add(i);
            }
            ReturnList.Add(new MiscData.Divider() { Display = Divider });
            ReturnList.Add(new MiscData.Divider() { Display = "MACROS USED:" });
            foreach (var i in ADVUnlockData.MacrosUsed)
            {
                ReturnList.Add(i);
                if (!UnlockData.ContainsKey(i) || !UnlockData[i].Any()) { continue; }
                ReturnList.Add($"    Unlocked With: {string.Join(" | ", UnlockData[i])}");
            }
            ReturnList.Add(new MiscData.Divider() { Display = Divider });
            ReturnList.Add(new MiscData.Divider() { Display = "EXITS TAKEN:" });
            foreach (var i in ADVUnlockData.ExitsTaken)
            {
                ReturnList.Add(i);
                if (!UnlockData.ContainsKey(i) || !UnlockData[i].Any()) { continue; }
                ReturnList.Add($"    Unlocked With: {string.Join(" | ", UnlockData[i])}");
            }
            return ReturnList;
        }

        public static List<string> GetMissingItems(string logicID, LogicObjects.TrackerInstance Instance)
        {
            PlaythroughGenerator playthroughGenerator = new PlaythroughGenerator(Instance);
            playthroughGenerator.GeneratePlaythrough();
            if (!playthroughGenerator.Playthrough.ContainsKey(logicID)) { return null; }
            var AdvancedUnlockData = GetAdvancedUnlockData(logicID, playthroughGenerator._Instance.logicCalculation.LogicUnlockData, playthroughGenerator._Instance.Instance, playthroughGenerator);
            if (AdvancedUnlockData == null) { return null; }
            List<string> NeededItems = new List<string>();
            foreach (var item in AdvancedUnlockData.RealItemsUsed)
            {
                var ItemObj = Instance.GetItemByID(item.Key);
                if (ItemObj is null || ItemObj.Useable(Instance, item.Value)) { continue; }
                int MissingAmunt = item.Value - ItemObj.GetTotalUsable(Instance);
                NeededItems.Add($"{ItemObj.GetDictEntry(Instance).GetName(Instance)} x{MissingAmunt}");
            }
            foreach (var item in AdvancedUnlockData.Rootareas)
            {
                if (!Instance.EntrancePool.IsEntranceRando) { break; }
                if (Instance.EntrancePool.AreaList.ContainsKey(item) && Instance.EntrancePool.AreaList[item].ExitsAcessibleFrom < 1)
                {
                    NeededItems.Add(item);
                }
            }
            return NeededItems;
        }
    }
}
