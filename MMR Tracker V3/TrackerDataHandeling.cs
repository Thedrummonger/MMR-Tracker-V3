using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public class TrackerDataHandeling
    {
        public class DataSets
        {
            public List<LocationData.LocationObject> UncheckedLocations { get; set; } = new List<LocationData.LocationObject>();
            public List<LocationData.LocationObject> AvailableLocations { get; set; } = new List<LocationData.LocationObject>();
            public List<LocationData.LocationObject> AllAvailableLocations { get; set; } = new List<LocationData.LocationObject>();
            public List<LocationData.LocationObject> MarkedLocations { get; set; } = new List<LocationData.LocationObject>();
            public List<LocationData.LocationObject> CheckedLocations { get; set; } = new List<LocationData.LocationObject>();

            public List<LocationData.LocationProxy> AvailableProxies { get; set; } = new List<LocationData.LocationProxy>();
            public List<LocationData.LocationProxy> AllAvailableProxies { get; set; } = new List<LocationData.LocationProxy>();
            public List<LocationData.LocationProxy> MarkedProxies { get; set; } = new List<LocationData.LocationProxy>();

            public List<EntranceData.EntranceRandoExit> UncheckedEntrances { get; set; } = new List<EntranceData.EntranceRandoExit>();
            public List<EntranceData.EntranceRandoExit> AvailableEntrances { get; set; } = new List<EntranceData.EntranceRandoExit>();
            public List<EntranceData.EntranceRandoExit> AllAvailableEntrances { get; set; } = new List<EntranceData.EntranceRandoExit>();
            public List<EntranceData.EntranceRandoExit> MarkedEntrances { get; set; } = new List<EntranceData.EntranceRandoExit>();
            public List<EntranceData.EntranceRandoExit> CheckedEntrances { get; set; } = new List<EntranceData.EntranceRandoExit>();

            public List<HintData.HintObject> AvailableHints { get; set; } = new List<HintData.HintObject>();
            public List<HintData.HintObject> AllAvailableHints { get; set; } = new List<HintData.HintObject>();
            public List<HintData.HintObject> UnheckedHints { get; set; } = new List<HintData.HintObject>();
            public List<HintData.HintObject> CheckedHints { get; set; } = new List<HintData.HintObject>();
            public List<HintData.HintObject> MarkedHints { get; set; } = new List<HintData.HintObject>();

            public List<MacroObject> Tricks { get; set; } = new List<MacroObject>();
            public List<ItemData.ItemObject> AvailableStartingItems { get; set; } = new List<ItemData.ItemObject>();

            public List<ItemData.ItemObject> LocalObtainedItems { get; set; } = new List<ItemData.ItemObject>();
            public List<ItemData.ItemObject> CurrentStartingItems { get; set; } = new List<ItemData.ItemObject>();
            public List<ItemData.ItemObject> OnlineObtainedItems { get; set; } = new List<ItemData.ItemObject>();
        }

        public static bool CheckSelectedItems(IEnumerable<object> Items, MiscData.CheckState checkState, LogicObjects.TrackerInstance Instance, Func<IEnumerable<object>, LogicObjects.TrackerInstance, bool> CheckUnassignedLocations, Func<IEnumerable<object>, LogicObjects.TrackerInstance, bool> CheckUnassignedVariable, bool EnforceMarkAction = false, Stopwatch CodeTimer = null)
        {
            Debug.WriteLine("Checking Item-----------------------------");
            Stopwatch FunctionTime = new Stopwatch();
            Utility.TimeCodeExecution(FunctionTime);

            bool ChangesMade = false;
            Utility.TimeCodeExecution(FunctionTime, "Saving Current State", 1);

            //Search for valid Object types in the list of selected Objects and sort them into lists
            IEnumerable<LocationData.LocationObject> locationObjects = Items.Where(x => x is LocationData.LocationObject).Select(x => x as LocationData.LocationObject);
            locationObjects = locationObjects.Concat(Items.Where(x => x is LocationData.LocationProxy).Select(x => Instance.LocationPool[(x as LocationData.LocationProxy).ReferenceID]));
            IEnumerable<EntranceData.EntranceRandoExit> ExitObjects = Items.Where(x => x is EntranceData.EntranceRandoExit).Select(x => x as EntranceData.EntranceRandoExit);
            IEnumerable<OptionData.TrackerOption> OptionObjects = Items.Where(x => x is OptionData.TrackerOption).Select(x => x as OptionData.TrackerOption);
            Utility.TimeCodeExecution(FunctionTime, "Sorting Selected Items", 1);

            //If we are performing an uncheck action there should be no unchecked locations in the list and even if there are nothing will be done to them anyway
            //This check is neccessary for the "UnMark Only" action and also provides a bit more efficiency.
            IEnumerable<LocationData.LocationObject> UncheckedlocationObjects = (checkState == MiscData.CheckState.Unchecked) ?
                new List<LocationData.LocationObject>() :
                locationObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);
            IEnumerable<EntranceData.EntranceRandoExit> UncheckedExitObjects = (checkState == MiscData.CheckState.Unchecked) ?
                new List<EntranceData.EntranceRandoExit>() :
                ExitObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);
            Utility.TimeCodeExecution(FunctionTime, "Getting Unchecked Entries", 1);

            //For any Locations with no randomized item, check if an item can be automatically assigned.
            foreach (LocationData.LocationObject LocationObject in UncheckedlocationObjects)
            {
                LocationObject.Randomizeditem.Item = LocationObject.GetItemAtCheck(Instance);
            }
            foreach (EntranceData.EntranceRandoExit ExitObject in UncheckedExitObjects)
            {
                ExitObject.DestinationExit = ExitObject.GetDestinationAtExit(Instance);
            }
            Utility.TimeCodeExecution(FunctionTime, "Checking for Randmomized item data", 1);

            //Get Entries that need a value manually assigned and pass them to given method to be assigned.
            IEnumerable<object> ManualChecks = UncheckedlocationObjects.Where(x => x.Randomizeditem.Item == null); //Locations with no item
            ManualChecks = ManualChecks.Concat(UncheckedExitObjects.Where(x => x.DestinationExit == null)); //Exits With No Destination
            ManualChecks = ManualChecks.Concat(OptionObjects.Where(x => !x.IsToggleOption())); //Non Toggle Options
            if (ManualChecks.Any())
            {
                if (CodeTimer != null) { CodeTimer.Stop(); }
                CheckUnassignedLocations(ManualChecks, Instance);
                ChangesMade = true;
                if (CodeTimer != null) { CodeTimer.Start(); }
            }
            Utility.TimeCodeExecution(FunctionTime, "Manual Check Form", 1);

            //Options======================================
            foreach (var i in OptionObjects.Where(x => x.IsToggleOption())) { i.ToggleOption(); ChangesMade = true; }
            Utility.TimeCodeExecution(FunctionTime, "Toggling Options", 1);
            //Items======================================
            foreach (LocationData.LocationObject LocationObject in locationObjects)
            {
                //When we mark a location, the action is always sent as Marked, but if the location is already marked we should instead Unchecked it unless EnforceMarkAction is true.
                var Action = (checkState == MiscData.CheckState.Marked && LocationObject.CheckState == MiscData.CheckState.Marked) && !EnforceMarkAction ? MiscData.CheckState.Unchecked : checkState;
                if (LocationObject.ToggleChecked(Action, Instance)) { ChangesMade = true; }
            }
            Utility.TimeCodeExecution(FunctionTime, "Checking Locations", 1);
            //Exits======================================
            foreach (EntranceData.EntranceRandoExit ExitObject in ExitObjects)
            {
                var Action = (checkState == MiscData.CheckState.Marked && ExitObject.CheckState == MiscData.CheckState.Marked) && !EnforceMarkAction ? MiscData.CheckState.Unchecked : checkState;
                if (ExitObject.ToggleExitChecked(Action, Instance)) { ChangesMade = true; }
            }
            Utility.TimeCodeExecution(FunctionTime, "Checking Entrances", 1);

            //Hints======================================
            List<HintData.HintObject> HintObjects = Items.Where(x => x is HintData.HintObject).Select(x => x as HintData.HintObject).ToList();
            List<LogicDictionaryData.TrackerVariable> VariableObjects = Items.Where(x => x is LogicDictionaryData.TrackerVariable).Select(x => x as LogicDictionaryData.TrackerVariable).ToList();

            var UncheckedHintObjects = HintObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);
            foreach (var i in UncheckedHintObjects.Where(x => !string.IsNullOrWhiteSpace(x.SpoilerHintText))) { i.HintText = i.SpoilerHintText; }

            IEnumerable<object> UncheckedVariableObjects = UncheckedHintObjects.Where(x => string.IsNullOrWhiteSpace(x.HintText));
            UncheckedVariableObjects = UncheckedVariableObjects.Concat(VariableObjects.Where(x => x.Value is not bool));
            if (UncheckedVariableObjects.Any())
            {
                if (CodeTimer != null) { CodeTimer.Stop(); }
                CheckUnassignedVariable(UncheckedVariableObjects, Instance);
                ChangesMade = true;
                if (CodeTimer != null) { CodeTimer.Start(); }
            }
            foreach (var i in VariableObjects.Where(x => x.Value is bool)) { i.Value = !i.Value; ChangesMade = true; }
            foreach (HintData.HintObject hintObject in HintObjects)
            {
                ChangesMade = true;
                var CheckAction = (checkState == MiscData.CheckState.Marked && hintObject.CheckState == MiscData.CheckState.Marked) && !EnforceMarkAction ? MiscData.CheckState.Unchecked : checkState;
                hintObject.CheckState = CheckAction;
                hintObject.HintText = CheckAction == MiscData.CheckState.Unchecked ? null : hintObject.HintText;
            }
            Utility.TimeCodeExecution(FunctionTime, "Setting Hints", 1);

            //Cleanup======================================

            if (ChangesMade)
            {
                LogicCalculation.CalculateLogic(Instance);
                Utility.TimeCodeExecution(FunctionTime, "---TOTAL Calculating Logic", 1);
                if (checkState == MiscData.CheckState.Checked && Instance.StaticOptions.AutoCheckCoupleEntrances && !Instance.StaticOptions.DecoupleEntrances && LogicCalculation.CheckEntrancePair(Instance))
                {
                    LogicCalculation.CalculateLogic(Instance);
                    Utility.TimeCodeExecution(FunctionTime, "Chcking Entrance Pairs", 1);
                }
            }
            return ChangesMade;
        }

        public static DataSets PopulateDataSets(LogicObjects.TrackerInstance instance)
        {
            DataSets dataSets = new DataSets();

            dataSets.UncheckedLocations = instance.LocationPool.Values.Where(x => x.CheckState == MiscData.CheckState.Unchecked).ToList();
            dataSets.MarkedLocations = instance.LocationPool.Values.Where(x => x.CheckState == MiscData.CheckState.Marked).ToList();
            dataSets.CheckedLocations = instance.LocationPool.Values.Where(x => x.CheckState == MiscData.CheckState.Checked).ToList();
            dataSets.AllAvailableLocations = instance.LocationPool.Values.Where(x => x.CheckState != MiscData.CheckState.Checked).ToList();
            dataSets.AvailableLocations = dataSets.AllAvailableLocations.Where(x => x.Available || x.CheckState == MiscData.CheckState.Marked).ToList();

            dataSets.MarkedProxies = instance.LocationProxyData.LocationProxies.Values.Where(x => x.GetReferenceLocation(instance).CheckState == MiscData.CheckState.Marked).ToList();
            dataSets.AllAvailableProxies = instance.LocationProxyData.LocationProxies.Values.Where(x => x.GetReferenceLocation(instance).CheckState != MiscData.CheckState.Checked).ToList();
            dataSets.AvailableProxies = dataSets.AllAvailableProxies.Where(x => x.ProxyAvailable(instance) || x.GetReferenceLocation(instance).CheckState == MiscData.CheckState.Marked).ToList();

            var AllExits = instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values);
            dataSets.UncheckedEntrances = AllExits.Where(x => x.CheckState == MiscData.CheckState.Unchecked).ToList();
            dataSets.MarkedEntrances = AllExits.Where(x => x.CheckState == MiscData.CheckState.Checked).ToList();
            dataSets.CheckedEntrances = AllExits.Where(x => x.CheckState == MiscData.CheckState.Marked).ToList();
            dataSets.AllAvailableEntrances = AllExits.Where(x => x.CheckState != MiscData.CheckState.Checked).ToList();
            dataSets.AvailableEntrances = AllExits.Where(x => x.Available || x.CheckState == MiscData.CheckState.Marked).ToList();

            dataSets.UnheckedHints = instance.HintPool.Values.Where(x => x.CheckState == MiscData.CheckState.Unchecked).ToList();
            dataSets.MarkedHints = instance.HintPool.Values.Where(x => x.CheckState == MiscData.CheckState.Marked).ToList();
            dataSets.CheckedHints = instance.HintPool.Values.Where(x => x.CheckState == MiscData.CheckState.Checked).ToList();
            dataSets.AllAvailableHints = instance.HintPool.Values.Where(x => x.CheckState != MiscData.CheckState.Checked).ToList();
            dataSets.AvailableHints = dataSets.AllAvailableHints.Where(x => x.Available || x.CheckState == MiscData.CheckState.Marked).ToList();

            dataSets.Tricks = instance.MacroPool.Values.Where(x => x.isTrick(instance)).ToList();
            dataSets.AvailableStartingItems = instance.ItemPool.Values.Where(x => x.ValidStartingItem(instance)).ToList();

            dataSets.LocalObtainedItems = instance.ItemPool.Values.Where(x => x.AmountAquiredLocally > 0).ToList();
            dataSets.CurrentStartingItems = instance.ItemPool.Values.Where(x => x.AmountInStartingpool > 0).ToList();
            dataSets.OnlineObtainedItems = instance.ItemPool.Values.Where(x => x.AmountAquiredOnline.Any(x => x.Value > 0)).ToList();
            return dataSets;
        }

        public static List<object> PopulateCheckedLocationList(MiscData.Divider Divider, LogicObjects.TrackerInstance Instance, string Filter, out int OutItemsInListBox, out int OutItemsInListBoxFiltered, bool reverse = false)
        {
            var Groups = Utility.GetCategoriesFromFile(Instance);
            var DataSets = PopulateDataSets(Instance);

            List<object> DataSource = new List<object>();

            var CheckedLocations = DataSets.CheckedLocations;
            CheckedLocations = CheckedLocations
                .OrderBy(x => (Groups.ContainsKey(x.GetDictEntry(Instance).Area.ToLower().Trim()) ? Groups[x.GetDictEntry(Instance).Area.ToLower().Trim()] : DataSets.CheckedLocations.Count + 1))
                .ThenBy(x => x.GetDictEntry(Instance).Area)
                .ThenBy(x => Utility.GetDisplayName(1, x, Instance)).ToList();

            var ItemsInListBox = 0;
            var ItemsInListBoxFiltered = 0;

            if (reverse)
            {
                CheckedLocations.Reverse();
                WriteStartingAndOnlineItems();
                WriteOptions();
                WriteHints();
                WriteEntrances();
                WriteLocations();
            }
            else
            {
                WriteLocations();
                WriteEntrances();
                WriteHints();
                WriteOptions();
                WriteStartingAndOnlineItems();
            }


            OutItemsInListBox = ItemsInListBox;
            OutItemsInListBoxFiltered = ItemsInListBoxFiltered;
            return DataSource;

            void WriteOptions()
            {
                if (Instance.StaticOptions.ShowOptionsInListBox == null || Instance.StaticOptions.ShowOptionsInListBox != OptionData.DisplayListBoxes[2]) { return; }
                bool DividerCreated = false;
                foreach (var i in Instance.UserOptions.Where(x => x.Value.Values.Count > 1))
                {
                    ItemsInListBox++;
                    if (!SearchStringParser.FilterSearch(Instance, i.Value, Filter, i.Value.DisplayName)) { continue; }
                    if (!DividerCreated)
                    {
                        if (DataSource.Count > 0) { DataSource.Add(Divider); }
                        DataSource.Add(new MiscData.Areaheader { Area = "Options" });
                        DividerCreated = true;
                    }
                    ItemsInListBoxFiltered++;
                    DataSource.Add(i.Value);
                }
                foreach (var i in Instance.Variables.Values.Where(x => !x.Static))
                {
                    ItemsInListBox++;
                    if (!SearchStringParser.FilterSearch(Instance, i, Filter, i.ToString())) { continue; }
                    if (!DividerCreated)
                    {
                        if (DataSource.Count > 0) { DataSource.Add(Divider); }
                        DataSource.Add(new MiscData.Areaheader { Area = "Options" });
                        DividerCreated = true;
                    }
                    ItemsInListBoxFiltered++;
                    DataSource.Add(i);
                }
            }

            void WriteLocations()
            {
                string CurrentLocation = "";
                foreach (var i in CheckedLocations)
                {
                    if (!LocationAppearsinListbox(i, Instance)) { continue; }
                    i.DisplayName = Utility.GetDisplayName(1, i, Instance);

                    ItemsInListBox++;
                    if (!SearchStringParser.FilterSearch(Instance, i, Filter, i.DisplayName)) { continue; }
                    ItemsInListBoxFiltered++;
                    if (CurrentLocation != i.GetDictEntry(Instance).Area)
                    {
                        if (DataSource.Count > 0) { DataSource.Add(Divider); }
                        DataSource.Add(new MiscData.Areaheader { Area = i.GetDictEntry(Instance).Area });
                        CurrentLocation = i.GetDictEntry(Instance).Area;
                    }
                    DataSource.Add(i);
                }
            }

            void WriteEntrances()
            {
                foreach (var area in Instance.EntrancePool.AreaList)
                {
                    var CheckLoadingZoneExits = area.Value.LoadingZoneExits.Where(x => x.Value.CheckState == MiscData.CheckState.Checked && EntranceAppearsinListbox(x.Value, Instance));
                    var FilteredCheckedExits = CheckLoadingZoneExits.Where(x => SearchStringParser.FilterSearch(Instance, x.Value, Filter, ExitDisplayName(x.Value)));

                    ItemsInListBox += CheckLoadingZoneExits.Count();
                    ItemsInListBoxFiltered += FilteredCheckedExits.Count();
                    if (!FilteredCheckedExits.Any()) { continue; }
                    if (DataSource.Count > 0) { DataSource.Add(Divider); }
                    DataSource.Add(new MiscData.Areaheader { Area = $"{area.Key} Exits" });
                    foreach (var i in FilteredCheckedExits)
                    {
                        i.Value.DisplayName = ExitDisplayName(i.Value);
                        DataSource.Add(i.Value);
                    }
                }
                string ExitDisplayName(EntranceData.EntranceRandoExit Exit)
                {
                    return $"{Exit.DestinationExit.region} From {Exit.DestinationExit.from} : {Exit.ID} Exit";
                }
            }

            void WriteHints()
            {
                if (DataSets.CheckedHints.Any())
                {
                    bool DividerCreated = false;
                    foreach (var i in DataSets.CheckedHints)
                    {
                        i.DisplayName = $"{i.GetDictEntry(Instance).Name}: {i.HintText}";
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(Instance, i, Filter, i.DisplayName)) { continue; }
                        ItemsInListBoxFiltered++;
                        if (!DividerCreated)
                        {
                            if (DataSource.Count > 0) { DataSource.Add(Divider); }
                            DataSource.Add("HINTS:");
                            DividerCreated = true;
                        }
                        DataSource.Add(i);
                    }
                }
            }

            void WriteStartingAndOnlineItems()
            {
                if (DataSets.CurrentStartingItems.Any())
                {
                    bool DividerCreated = false;
                    foreach (var i in DataSets.CurrentStartingItems)
                    {
                        string Display = $"{i.GetDictEntry(Instance).GetItemName(Instance)} X{i.AmountInStartingpool}";
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(Instance, i, Filter, Display)) { continue; }
                        if (!DividerCreated)
                        {
                            if (DataSource.Count > 0) { DataSource.Add(Divider); }
                            DataSource.Add(new MiscData.Areaheader { Area = "Starting Items" });
                            DividerCreated = true;
                        }
                        ItemsInListBoxFiltered++;
                        DataSource.Add(Display);
                    }
                }

                if (DataSets.OnlineObtainedItems.Any())
                {
                    bool DividerCreated = false;
                    foreach (var i in DataSets.OnlineObtainedItems)
                    {
                        foreach (var j in i.AmountAquiredOnline)
                        {
                            string Display = $"{i.GetDictEntry(Instance).GetItemName(Instance)} X{j.Value}: Player {j.Key}";
                            ItemsInListBox++;
                            if (!SearchStringParser.FilterSearch(Instance, i, Filter, Display)) { continue; }
                            if (!DividerCreated)
                            {
                                if (DataSource.Count > 0) { DataSource.Add(Divider); }
                                DataSource.Add(new MiscData.Areaheader { Area = "MultiWorld Items" });
                                DividerCreated = true;
                            }
                            ItemsInListBoxFiltered++;
                            DataSource.Add(Display);
                        }
                    }
                }
            }
        }

        public static List<object> PopulateAvailableLocationList(MiscData.Divider Divider, LogicObjects.TrackerInstance Instance, string Filter, bool ShowUnavailable, out int OutItemsInListBox, out int OutItemsInListBoxFiltered, bool reverse = false)
        {
            var Groups = Utility.GetCategoriesFromFile(Instance);
            var DataSets = PopulateDataSets(Instance);
            List<object> DataSource = new List<object>();

            var AvailableProxies = DataSets.AvailableProxies;
            if (Filter.StartsWith("^") || ShowUnavailable) { AvailableProxies = DataSets.AllAvailableProxies; }

            var AvailableLocations = DataSets.AvailableLocations;
            if (Filter.StartsWith("^") || ShowUnavailable) { AvailableLocations = DataSets.AllAvailableLocations; }

            AvailableLocations = AvailableLocations
                .OrderBy(x => (Groups.ContainsKey(x.GetDictEntry(Instance).Area.ToLower().Trim()) ? Groups[x.GetDictEntry(Instance).Area.ToLower().Trim()] : DataSets.AvailableLocations.Count() + 1))
                .ThenBy(x => x.GetDictEntry(Instance).Area)
                .ThenBy(x => Utility.GetDisplayName(0, x, Instance)).ToList();

            var AvailableHints = DataSets.AvailableHints;
            if (Filter.StartsWith("^") || ShowUnavailable)
            {
                AvailableHints = DataSets.AllAvailableHints;
            }

            var ItemsInListBox = 0;
            var ItemsInListBoxFiltered = 0;

            if (reverse)
            {
                AvailableLocations.Reverse();
                WriteOptions();
                WriteHints(DataSets);
                WriteLocations(AvailableLocations, AvailableProxies);
            }
            else
            {
                WriteLocations(AvailableLocations, AvailableProxies);
                WriteHints(DataSets);
                WriteOptions();
            }

            OutItemsInListBox = ItemsInListBox;
            OutItemsInListBoxFiltered = ItemsInListBoxFiltered;
            return DataSource;

            void WriteOptions()
            {
                if (Instance.StaticOptions.ShowOptionsInListBox == null || Instance.StaticOptions.ShowOptionsInListBox != OptionData.DisplayListBoxes[1]) { return; }
                bool DividerCreated = false;
                foreach (var i in Instance.UserOptions.Where(x => x.Value.Values.Count > 1))
                {
                    ItemsInListBox++;
                    i.Value.DisplayName = i.Value.DisplayName;
                    if (!SearchStringParser.FilterSearch(Instance, i.Value, Filter, i.Value.DisplayName)) { continue; }
                    if (!DividerCreated)
                    {
                        if (DataSource.Count > 0) { DataSource.Add(Divider); }
                        DataSource.Add(new MiscData.Areaheader { Area = "Options" });
                        DividerCreated = true;
                    }
                    ItemsInListBoxFiltered++;
                    DataSource.Add(i.Value);
                }
                foreach (var i in Instance.Variables.Values.Where(x => !x.Static))
                {
                    ItemsInListBox++;
                    if (!SearchStringParser.FilterSearch(Instance, i, Filter, i.ToString())) { continue; }
                    if (!DividerCreated)
                    {
                        if (DataSource.Count > 0) { DataSource.Add(Divider); }
                        DataSource.Add(new MiscData.Areaheader { Area = "Options" });
                        DividerCreated = true;
                    }
                    ItemsInListBoxFiltered++;
                    DataSource.Add(i);
                }
            }

            void WriteLocations(List<LocationData.LocationObject> AvailableLocations, List<LocationData.LocationProxy> AvailableProxies)
            {
                var AllAvailable = new List<object>().Concat(AvailableLocations.Where(x => !x.GetDictEntry(Instance).LocationProxys.Any())).Concat(AvailableProxies);
                string CurrentLocation = "";
                foreach (var obj in AllAvailable)
                {
                    var CurrentArea = "";
                    if (obj is LocationData.LocationObject i)
                    {
                        if (!LocationAppearsinListbox(i, Instance)) { continue; }
                        i.DisplayName = Utility.GetDisplayName(0, i, Instance);
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(Instance, i, Filter, i.DisplayName)) { continue; }
                        ItemsInListBoxFiltered++;
                        CurrentArea = i.GetDictEntry(Instance).Area;
                    }
                    else if (obj is LocationData.LocationProxy p)
                    {
                        var ProxyLoc = Instance.LocationPool[p.ReferenceID];
                        if (!LocationAppearsinListbox(ProxyLoc, Instance)) { continue; }
                        p.DisplayName = p.Name;
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(Instance, ProxyLoc, Filter, p.DisplayName)) { continue; }
                        ItemsInListBoxFiltered++;
                        CurrentArea = p.Area;
                    }
                    else { continue; }
                    if (CurrentLocation != CurrentArea)
                    {
                        if (DataSource.Count > 0) { DataSource.Add(Divider); }
                        DataSource.Add(new MiscData.Areaheader { Area = CurrentArea });
                        CurrentLocation = CurrentArea;
                    }
                    DataSource.Add(obj);
                }
            }

            void WriteHints(DataSets DataSets)
            {
                if (AvailableHints.Any())
                {
                    bool DividerCreated = false;
                    foreach (var i in AvailableHints)
                    {
                        i.DisplayName = (i.CheckState == MiscData.CheckState.Marked) ? $"{i.GetDictEntry(Instance).Name}: {i.HintText}" : i.GetDictEntry(Instance).Name;
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(Instance, i, Filter, i.DisplayName)) { continue; }
                        ItemsInListBoxFiltered++;
                        if (!DividerCreated)
                        {
                            if (DataSource.Count > 0) { DataSource.Add(Divider); }
                            DataSource.Add(new MiscData.Areaheader { Area = "HINTS:" });
                            DividerCreated = true;
                        }
                        DataSource.Add(i);
                    }
                }
            }
        }

        public static List<object> PopulateAvailableEntraceList(MiscData.Divider Divider, LogicObjects.TrackerInstance Instance, string Filter, bool ShowUnavailable, out int OutItemsInListBox, out int OutItemsInListBoxFiltered, bool reverse = false)
        {
            var Groups = Utility.GetCategoriesFromFile(Instance);
            var DataSets = PopulateDataSets(Instance);
            List<object> DataSource = new List<object>();
            OutItemsInListBox = 0;
            OutItemsInListBoxFiltered = 0;
            foreach (var area in Instance.EntrancePool.AreaList)
            {
                var AvailableExits = area.Value.LoadingZoneExits.Where(x => 
                (x.Value.Available || x.Value.CheckState == MiscData.CheckState.Marked || ShowUnavailable || Filter.StartsWith("^")) && 
                x.Value.CheckState != MiscData.CheckState.Checked && EntranceAppearsinListbox(x.Value, Instance));

                var FilteredAvailableExits = AvailableExits.Where(x => SearchStringParser.FilterSearch(Instance, x.Value, Filter, ExitDisplayName(x.Value)));

                OutItemsInListBox += AvailableExits.Count();
                OutItemsInListBoxFiltered += FilteredAvailableExits.Count();
                if (!FilteredAvailableExits.Any()) { continue; }

                if (DataSource.Count > 0) { DataSource.Add(Divider); }
                DataSource.Add(new MiscData.Areaheader { Area = area.Key });
                foreach(var i in FilteredAvailableExits)
                {
                    i.Value.DisplayName = ExitDisplayName(i.Value);
                    DataSource.Add(i.Value);
                }
            }
            return DataSource;

            string ExitDisplayName(EntranceData.EntranceRandoExit Exit)
            {
                var Name = $"{Exit.ID}";
                if (Exit.CheckState == MiscData.CheckState.Marked) { Name += $": {Exit.DestinationExit.region} <= {Exit.DestinationExit.from}"; }
                return Name;
            }
        }

        private static bool LocationAppearsinListbox(LocationData.LocationObject Location, LogicObjects.TrackerInstance Instance)
        {
            return !Location.IsJunk() && !Location.IsUnrandomized(1) && !string.IsNullOrWhiteSpace(Location.GetDictEntry(Instance).Name);
        }
        private static bool EntranceAppearsinListbox(EntranceData.EntranceRandoExit Location, LogicObjects.TrackerInstance Instance)
        {
            return !Location.IsJunk() && !Location.IsUnrandomized(1);
        }
    }
}
