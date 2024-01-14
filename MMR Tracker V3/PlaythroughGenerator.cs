using MathNet.Numerics;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
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
        public InstanceData.InstanceContainer Container = new InstanceData.InstanceContainer();
        public Dictionary<string, PlaythroughObject> Playthrough = new Dictionary<string, PlaythroughObject>();
        public List<string> _IngoredChecks;
        public Dictionary<string, List<Tuple<object, PlaythroughObject>>> FirstObtainedDict = new Dictionary<string, List<Tuple<object, PlaythroughObject>>>();
        public PlaythroughGenerator(InstanceData.TrackerInstance instance, List<string> IngoredChecks = null)
        {
            Container.CopyAndLoadInstance(instance);
            _IngoredChecks = IngoredChecks??new List<string>();
            Container.logicCalculation = new LogicCalculation(Container);
        }

        [Serializable]
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
            Container.logicCalculation.CompileOptionActionEdits();
            Container.logicCalculation.CalculateLogic(MiscData.CheckState.Checked);

            var AvailableLocations = Container.Instance.LocationPool.Values.Where(x => x.Available && x.CheckState == MiscData.CheckState.Unchecked && x.IsRandomized());
            var AvailableEntrances = getAllAvailableEntrances(Container.Instance, Container.logicCalculation.AutoObtainedObjects);
            var AquiredMacros = Container.Instance.MacroPool.Values.Where(x => x.Aquired && !Playthrough.ContainsKey(x.ID));

            while (AvailableLocations.Any() || AvailableEntrances.Any() || AquiredMacros.Any())
            {
                //Debug.WriteLine($"Sphere: {Sphere} ============================================");
                foreach (var i in AvailableEntrances)
                {
                    //Debug.WriteLine($"Checking: {$"{i.ParentAreaID} X {i.ID}"}");
                    if (i.GetDestinationAtExit() == null)
                    {
                        throw new Exception($"{$"{i.ParentAreaID} X {i.ID}"} had no item");
                    }
                    var RandomizableExits = Container.Instance.EntrancePool.AreaList[i.ParentAreaID].RandomizableExits();
                    if (RandomizableExits.ContainsKey(i.ID) && RandomizableExits[i.ID].IsRandomized())
                    {
                        i.ToggleExitChecked(MiscData.CheckState.Checked);
                    }
                    i.CheckState = MiscData.CheckState.Checked;
                    PlaythroughObject playthroughObject = new PlaythroughObject
                    {
                        id = Container.Instance.GetLogicNameFromExit(i),
                        ItemObtained = i.GetDestinationAtExit().region,
                        UsedItems = Container.logicCalculation.LogicUnlockData[Container.Instance.GetLogicNameFromExit(i)],
                        CheckType = MiscData.LogicEntryType.Exit,
                        sphere = Sphere
                    };
                    if (!FirstObtainedDict.ContainsKey(playthroughObject.ItemObtained)) { FirstObtainedDict.Add(playthroughObject.ItemObtained, new List<Tuple<object, PlaythroughObject>>()); }
                    FirstObtainedDict[playthroughObject.ItemObtained].Add(new Tuple<object, PlaythroughObject>(i, playthroughObject));
                    Playthrough.Add(Container.Instance.GetLogicNameFromExit(i), playthroughObject);
                }
                foreach (var i in AvailableLocations)
                {
                    //Debug.WriteLine($"Checking: {i.ID}");
                    i.ToggleChecked(MiscData.CheckState.Checked);
                    i.CheckState = MiscData.CheckState.Checked;
                    if (i.GetItemAtCheck() == null)
                    {
                        throw new Exception($"{i.ID} had no item");
                    }
                    PlaythroughObject playthroughObject = new PlaythroughObject
                    {
                        id = i.ID,
                        ItemObtained = i.GetItemAtCheck(),
                        UsedItems = Container.logicCalculation.LogicUnlockData[i.ID],
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
                        UsedItems = Container.logicCalculation.LogicUnlockData[i.ID],
                        CheckType = MiscData.LogicEntryType.macro,
                        sphere = Sphere
                    };
                    if (!FirstObtainedDict.ContainsKey(playthroughObject.ItemObtained)) { FirstObtainedDict.Add(playthroughObject.ItemObtained, new List<Tuple<object, PlaythroughObject>>()); }
                    FirstObtainedDict[playthroughObject.ItemObtained].Add(new Tuple<object, PlaythroughObject> ( i, playthroughObject));
                    Playthrough.Add(i.ID, playthroughObject);
                }

                Container.logicCalculation.CalculateLogic(MiscData.CheckState.Checked);
                AvailableLocations = Container.Instance.LocationPool.Values.Where(x => x.Available && x.CheckState == MiscData.CheckState.Unchecked && x.IsRandomized());
                AvailableEntrances = getAllAvailableEntrances(Container.Instance, Container.logicCalculation.AutoObtainedObjects);
                AquiredMacros = Container.Instance.MacroPool.Values.Where(x => x.Aquired && !Playthrough.ContainsKey(x.ID));


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
            else if (WinObj is ItemData.ItemObject itemWin && FirstObtainedDict.ContainsKey(itemWin.ID))
            {
                WinCon = FirstObtainedDict[itemWin.ID].First().Item2;
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
                    int StartingAmount = Container.Instance.GetItemByID(i.Key)?.AmountInStartingpool??0;
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
                    if (FirstObtainedDict.ContainsKey(i) && FirstObtainedDict[i].First().Item1 is EntranceData.EntranceRandoExit ere && ere.IsRandomizableEntrance() && (ere.IsRandomized() || ere.IsUnrandomized(MiscData.UnrandState.Manual)))
                    {
                        FirstObtainedDict[i].First().Item2.Important = true;
                    }
                }
                foreach (var i in ImportantCheck.advancedUnlockData.OptionsUsed.Where(x => x.StartsWith("renewable")))
                {
                    var item = i.Replace("renewable{", "").Replace("}", "");
                    if (FirstObtainedDict.ContainsKey(item))
                    {
                        foreach(var j in FirstObtainedDict[item])
                        {
                            if (j.Item1 is LocationData.LocationObject Loc && Loc.IsRepeatable())
                            {
                                j.Item2.Important = true;
                                break;
                            }
                        }
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
                i.Value.advancedUnlockData = PlaythroughTools.GetAdvancedUnlockData(i.Key, Container.logicCalculation.LogicUnlockData, Container.Instance, this);
            }
        }

        private List<EntranceData.EntranceRandoExit> getAllAvailableEntrances(InstanceData.TrackerInstance instance, Dictionary<object, int> AutoObtainedObjects)
        {
            var AvailableEntrances = instance.EntrancePool.AreaList.Values.SelectMany(x => x.RandomizableExits().Values.Where(x => x.Available && x.CheckState == MiscData.CheckState.Unchecked && x.IsRandomized()));
            var AutoObtainedEntrance = AutoObtainedObjects.Keys.Where(x => x is EntranceData.EntranceRandoExit && !Playthrough.ContainsKey(Container.Instance.GetLogicNameFromExit(x as EntranceData.EntranceRandoExit))).Select(x => x as EntranceData.EntranceRandoExit);
            //var AquiredEntranceMacros = instance.EntrancePool.AreaList.Values.SelectMany(x => x.MacroExits.Values.Where(x => x.CheckState == MiscData.CheckState.Checked && !Playthrough.ContainsKey(GetEntId(x))));
            //var UnrandEntranceMacros = instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values.Where(x => x.CheckState == MiscData.CheckState.Checked && !Playthrough.ContainsKey(GetEntId(x)) && x.IsUnrandomized()));
            //var AllEntrances = AvailableEntrances.Concat(AquiredEntranceMacros).Concat(UnrandEntranceMacros).ToList();
            var AllEntrances = AvailableEntrances.Concat(AutoObtainedEntrance).ToList();
            //Debug.WriteLine(AutoObtainedEntrance.Count());
            return AllEntrances;
        }

        public void PrepareInstance()
        {
            foreach(var i in Container.Instance.ItemPool.Values)
            {
                i.AmountAquiredLocally = 0;
            }
            foreach (var i in Container.Instance.EntrancePool.AreaList)
            {
                i.Value.ExitsAcessibleFrom = 0;
            }
            foreach (var i in Container.Instance.LocationPool.Values)
            {
                i.Available = false;
                i.CheckState = TrackerObjects.MiscData.CheckState.Unchecked;
                if (i.GetItemAtCheck() == null) { i.RandomizedState = TrackerObjects.MiscData.RandomizedState.ForcedJunk; }
                else
                {
                    i.Randomizeditem.Item = i.GetItemAtCheck();
                    i.RandomizedState = TrackerObjects.MiscData.RandomizedState.Randomized;
                }
                if (_IngoredChecks.Contains(i.ID)) { i.RandomizedState = TrackerObjects.MiscData.RandomizedState.ForcedJunk; }
            }
            foreach (var i in Container.Instance.EntrancePool.AreaList.Values.SelectMany(x => x.RandomizableExits().Values))
            {
                i.Available = false;
                i.CheckState = TrackerObjects.MiscData.CheckState.Unchecked;
                if (i.GetDestinationAtExit() == null) { i.RandomizedState = TrackerObjects.MiscData.RandomizedState.ForcedJunk; }
                else
                {
                    i.DestinationExit = i.GetDestinationAtExit();
                    if (i.RandomizedState == MiscData.RandomizedState.UnrandomizedManual) { i.RandomizedState = MiscData.RandomizedState.Randomized; }
                    //i.RandomizedState = TrackerObjects.MiscData.RandomizedState.Randomized;
                }
                if (_IngoredChecks.Contains(Container.Instance.GetLogicNameFromExit(i))) { i.RandomizedState = TrackerObjects.MiscData.RandomizedState.ForcedJunk; }
            }
            foreach (var i in Container.Instance.EntrancePool.AreaList.Values.SelectMany(x => x.NonRandomizableExits().Values))
            {
                i.Available = false;
                i.CheckState = TrackerObjects.MiscData.CheckState.Unchecked;
            }
            foreach (var i in Container.Instance.MacroPool.Values)
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
                string AreaName = (Container.Instance.GetLocationByID(p.Key)?.GetDictEntry()?.Area);
                string LocationName = Container.Instance.GetLocationByID(p.Key)?.GetDictEntry()?.GetName()??p.Key;
                string LocationDisplay = AreaName is null ? LocationName : $"{AreaName} - {LocationName}";
                string ItemName = Container.Instance.GetItemByID(p.Value.ItemObtained)?.GetDictEntry()?.GetName()??p.Value.ItemObtained;
                List<string> RealItems = new List<string>();

                foreach(var i in p.Value.advancedUnlockData.RealItemsUsed)
                {
                    string display = Container.Instance.GetItemByID(i.Key)?.GetDictEntry()?.GetName()??i.Key;
                    if (i.Value > 1) { display += $" X{i.Value}"; }
                    RealItems.Add(display);
                }

                readablePlaythrough.Add($"Check [{LocationDisplay.ToUpper()}] to obtain [{ItemName.ToUpper()}]");
                if (RealItems.Any()) { readablePlaythrough.Add($"   Using: [{string.Join(", ", RealItems)}]"); }
            }
            return readablePlaythrough;
        }
    }
    public static class PlaythroughTools
    {
        [Serializable]
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
            public List<string> Unknown { get; set; } = new List<string>();
        }
        public class ShpereCompletionData
        {
            public int ChecksInSphere { get; set; }
            public double PercentageAvailable { get; set; }
            public double PercentageAquired { get; set; }
        }
        public static AdvancedUnlockData GetAdvancedUnlockData(string ID, Dictionary<string, List<string>> UnlockData, InstanceData.TrackerInstance instance, PlaythroughGenerator playthroughObject = null)
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
                    var logicItem = instance.GetLogicItemData(i);

                    if (logicItem.Type == MiscData.LogicEntryType.macro && !Data.MacrosUsed.Contains(logicItem.CleanID))
                    {
                        Data.MacrosUsed.Add(logicItem.CleanID);
                        ParseRequirements(logicItem.CleanID);
                    }
                    else if (logicItem.Type == MiscData.LogicEntryType.function && !Data.OptionsUsed.Contains(logicItem.CleanID))
                    {
                        Data.OptionsUsed.Add(logicItem.CleanID);
                    }
                    else if (logicItem.Type == MiscData.LogicEntryType.item)
                    {
                        if (Data.RealItemsUsed.ContainsKey(logicItem.CleanID)) { if (logicItem.Amount > Data.RealItemsUsed[logicItem.CleanID]) { Data.RealItemsUsed[logicItem.CleanID] = logicItem.Amount; } }
                        else { Data.RealItemsUsed.Add(logicItem.CleanID, logicItem.Amount); }
                    }
                    else if (logicItem.Type == MiscData.LogicEntryType.Area && !Data.AreasAccessed.Contains(logicItem.CleanID))
                    {
                        GetAreaData(logicItem.CleanID, playthroughObject, instance, out List<EntranceData.EntranceAreaPair> path, out List<string> areasVisited);
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
                    else if (!Data.AreasAccessed.Contains(logicItem.CleanID) && !Data.OptionsUsed.Contains(logicItem.CleanID) && !Data.MacrosUsed.Contains(logicItem.CleanID) && !Data.Unknown.Contains(logicItem.CleanID))
                    {
                        Data.Unknown.Add(logicItem.CleanID);
                    }
                }
            }
        }

        public static void GetAreaData(string area,  PlaythroughGenerator playthroughObject, InstanceData.TrackerInstance instance, out List<EntranceData.EntranceAreaPair> outPath, out List<string> outAreasVisited)
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

                    var RandomizableExits = instance.EntrancePool.AreaList[eo.ParentAreaID].RandomizableExits();
                    bool ExitIsRandom = RandomizableExits.ContainsKey(eo.ID) && RandomizableExits[eo.ID].IsRandomized();
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

        private static List<string> GetUncheckedLocations(InstanceData.TrackerInstance instance)
        {
            List<string> Uncheckedlocations = new List<string>();
            foreach (var i in instance.LocationPool.Values)
            {
                if (i.CheckState != MiscData.CheckState.Checked) { Uncheckedlocations.Add(i.ID); }
            }
            foreach (var i in instance.EntrancePool.AreaList.Values.SelectMany(x => x.RandomizableExits().Values))
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
            Dictionary<string, int> TempRealItems = new Dictionary<string, int>();
            foreach (var i in ADVUnlockData.RealItemsUsed)
            {
                if (!TempRealItems.ContainsKey(i.Key)) { TempRealItems.Add(i.Key, 0); }
                TempRealItems[i.Key] += i.Value;
            }
            foreach (var i in ADVUnlockData.OptionsUsed.Where(x => x.StartsWith("renewable")))
            {
                string RenewableItemName = i.Replace("renewable{", "").TrimEnd('}');
                if (!TempRealItems.ContainsKey(RenewableItemName)) { TempRealItems.Add(RenewableItemName, 1); }
            }
            foreach(var i in TempRealItems)
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
            ReturnList.Add(new MiscData.Divider() { Display = "FUNCTIONS USED:" });
            foreach (var i in ADVUnlockData.OptionsUsed)
            {
                ReturnList.Add(i);
            }
            ReturnList.Add(new MiscData.Divider() { Display = Divider });
            ReturnList.Add(new MiscData.Divider() { Display = "EXITS TAKEN:" });
            foreach (var i in ADVUnlockData.ExitsTaken)
            {
                ReturnList.Add(i);
                if (!UnlockData.ContainsKey(i) || !UnlockData[i].Any()) { continue; }
                ReturnList.Add($"    Unlocked With: {string.Join(" | ", UnlockData[i])}");
            }
            if (ADVUnlockData.Unknown.Any())
            {
                ReturnList.Add(new MiscData.Divider() { Display = Divider });
                ReturnList.Add(new MiscData.Divider() { Display = "UNKNOWN:" });
                foreach (var i in ADVUnlockData.Unknown)
                {
                    ReturnList.Add(i);
                }
            }
            return ReturnList;
        }

        public static List<string> GetMissingItems(string logicID, InstanceData.TrackerInstance Instance)
        {
            Dictionary<string, PlaythroughGenerator.PlaythroughObject> Playthrough = Instance.SpoilerLog?.Playthrough;
            if (Playthrough is null)
            {
                PlaythroughGenerator playthroughGenerator = new PlaythroughGenerator(Instance);
                playthroughGenerator.GeneratePlaythrough();
                Playthrough = playthroughGenerator.Playthrough;
            }
            if (!Playthrough.ContainsKey(logicID)) { return null; }
            var AdvancedUnlockData = Playthrough[logicID].advancedUnlockData;
            //var AdvancedUnlockData = GetAdvancedUnlockData(logicID, playthroughGenerator._Instance.logicCalculation.LogicUnlockData, playthroughGenerator._Instance.Instance, playthroughGenerator);
            if (AdvancedUnlockData == null) { return null; }
            List<string> NeededItems = new List<string>();
            foreach (var item in AdvancedUnlockData.RealItemsUsed)
            {
                var ItemObj = Instance.GetItemByID(item.Key);
                if (ItemObj is null || ItemObj.Useable(item.Value)) { continue; }
                int MissingAmunt = item.Value - ItemObj.GetTotalUsable();
                NeededItems.Add($"{ItemObj.GetDictEntry().GetName()} x{MissingAmunt}");
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

        public static Dictionary<int, ShpereCompletionData> GetShpereCompletionData(InstanceData.TrackerInstance Instance)
        {
            Dictionary<int, ShpereCompletionData> Results = new();
            PlaythroughGenerator playthroughGenerator = new(Instance);
            playthroughGenerator.GeneratePlaythrough();

            Dictionary<int, List<LocationData.LocationObject>> ShereBreakdown = new();

            foreach (var i in playthroughGenerator.Playthrough)
            {
                var Location = Instance.GetLocationEntryType(i.Value.id, false, out object OBJ);
                if (OBJ is null || OBJ is not LocationData.LocationObject loc) { continue; }
                if (!ShereBreakdown.ContainsKey(i.Value.sphere)) { ShereBreakdown[i.Value.sphere] = new List<LocationData.LocationObject>(); }
                ShereBreakdown[i.Value.sphere].Add(loc);
            }

            foreach (var i in ShereBreakdown.Keys)
            {
                if (!ShereBreakdown[i].Any(x => x.Available) && !ShereBreakdown[i].Any(x => x.CheckState == MiscData.CheckState.Checked)) { break; }

                double PrecentUnlocked = (double)ShereBreakdown[i].Where(x => x.Available).Count() / (double)ShereBreakdown[i].Count();
                double PrecentAquired = (double)ShereBreakdown[i].Where(x => x.CheckState == MiscData.CheckState.Checked).Count() / (double)ShereBreakdown[i].Count();
                Results[i] = new ShpereCompletionData { PercentageAquired= Math.Round(PrecentAquired*100, 2), PercentageAvailable = Math.Round(PrecentUnlocked*100, 2), ChecksInSphere = i };
            }
            return Results;
        }
    }
}
