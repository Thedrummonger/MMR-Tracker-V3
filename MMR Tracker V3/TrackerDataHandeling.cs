using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.LocationData;

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

        public static bool CheckSelectedItems(IEnumerable<object> Items, MiscData.CheckState checkState, MiscData.InstanceContainer instanceContainer, Func<IEnumerable<object>, LogicObjects.TrackerInstance, bool> CheckUnassignedLocations, Func<IEnumerable<object>, LogicObjects.TrackerInstance, bool> CheckUnassignedVariable, bool EnforceMarkAction = false)
        {
            bool ChangesMade = false;

            //Search for valid Object types in the list of selected Objects and sort them into lists
            IEnumerable<LocationObject> locationObjects = Items.Where(x => x is LocationObject).Select(x => x as LocationObject);
            locationObjects = locationObjects.Concat(Items.Where(x => x is LocationProxy).Select(x => (x as LocationProxy).GetReferenceLocation(instanceContainer.Instance)));
            locationObjects = locationObjects.Distinct();
            IEnumerable<EntranceData.EntranceRandoExit> ExitObjects = Items.Where(x => x is EntranceData.EntranceRandoExit).Select(x => x as EntranceData.EntranceRandoExit);
            IEnumerable<OptionData.TrackerOption> OptionObjects = Items.Where(x => x is OptionData.TrackerOption).Select(x => x as OptionData.TrackerOption);

            //If we are performing an uncheck action there should be no unchecked locations in the list and even if there are nothing will be done to them anyway
            //This check is neccessary for the "UnMark Only" action and also provides a bit more efficiency.
            IEnumerable<LocationObject> UncheckedlocationObjects = (checkState == MiscData.CheckState.Unchecked) ?
                new List<LocationObject>() :
                locationObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);
            IEnumerable<EntranceData.EntranceRandoExit> UncheckedExitObjects = (checkState == MiscData.CheckState.Unchecked) ?
                new List<EntranceData.EntranceRandoExit>() :
                ExitObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);

            //For any Locations with no randomized item, check if an item can be automatically assigned.
            foreach (LocationObject LocationObject in UncheckedlocationObjects)
            {
                LocationObject.Randomizeditem.Item = LocationObject.GetItemAtCheck(instanceContainer.Instance);
            }
            foreach (EntranceData.EntranceRandoExit ExitObject in UncheckedExitObjects)
            {
                ExitObject.DestinationExit = ExitObject.GetDestinationAtExit(instanceContainer.Instance);
            }

            //Get Entries that need a value manually assigned and pass them to given method to be assigned.
            IEnumerable<object> ManualChecks = UncheckedlocationObjects.Where(x => x.Randomizeditem.Item == null); //Locations with no item
            ManualChecks = ManualChecks.Concat(UncheckedExitObjects.Where(x => x.DestinationExit == null)); //Exits With No Destination
            ManualChecks = ManualChecks.Concat(OptionObjects.Where(x => !x.IsToggleOption())); //Non Toggle Options
            if (ManualChecks.Any())
            {
                CheckUnassignedLocations(ManualChecks, instanceContainer.Instance);
                ChangesMade = true;
            }

            //Options======================================
            foreach (var i in OptionObjects.Where(x => x.IsToggleOption())) { i.ToggleOption(); ChangesMade = true; }
            //Items======================================
            foreach (LocationObject LocationObject in locationObjects)
            {
                //When we mark a location, the action is always sent as Marked, but if the location is already marked we should instead Unchecked it unless EnforceMarkAction is true.
                var Action = (checkState == MiscData.CheckState.Marked && LocationObject.CheckState == MiscData.CheckState.Marked) && !EnforceMarkAction ? MiscData.CheckState.Unchecked : checkState;
                if (LocationObject.ToggleChecked(Action, instanceContainer.Instance)) { ChangesMade = true; }
            }
            //Exits======================================
            foreach (EntranceData.EntranceRandoExit ExitObject in ExitObjects)
            {
                var Action = (checkState == MiscData.CheckState.Marked && ExitObject.CheckState == MiscData.CheckState.Marked) && !EnforceMarkAction ? MiscData.CheckState.Unchecked : checkState;
                if (ExitObject.ToggleExitChecked(Action, instanceContainer.Instance)) { ChangesMade = true; }
            }

            //Hints======================================
            List<HintData.HintObject> HintObjects = Items.Where(x => x is HintData.HintObject).Select(x => x as HintData.HintObject).ToList();
            List<LogicDictionaryData.TrackerVariable> VariableObjects = Items.Where(x => x is LogicDictionaryData.TrackerVariable).Select(x => x as LogicDictionaryData.TrackerVariable).ToList();

            var UncheckedHintObjects = HintObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);
            foreach (var i in UncheckedHintObjects.Where(x => !string.IsNullOrWhiteSpace(x.SpoilerHintText))) { i.HintText = i.SpoilerHintText; }

            IEnumerable<object> UncheckedVariableObjects = UncheckedHintObjects.Where(x => string.IsNullOrWhiteSpace(x.HintText));
            UncheckedVariableObjects = UncheckedVariableObjects.Concat(VariableObjects.Where(x => x.GetValue(instanceContainer.Instance) is not bool));
            if (UncheckedVariableObjects.Any())
            {
                CheckUnassignedVariable(UncheckedVariableObjects, instanceContainer.Instance);
                ChangesMade = true;
            }
            foreach (var i in VariableObjects.Where(x => x.GetValue(instanceContainer.Instance) is bool)) { i.Value = !i.GetValue(instanceContainer.Instance); ChangesMade = true; }
            foreach (HintData.HintObject hintObject in HintObjects)
            {
                ChangesMade = true;
                var CheckAction = (checkState == MiscData.CheckState.Marked && hintObject.CheckState == MiscData.CheckState.Marked) && !EnforceMarkAction ? MiscData.CheckState.Unchecked : checkState;
                hintObject.CheckState = CheckAction;
                hintObject.HintText = CheckAction == MiscData.CheckState.Unchecked ? null : hintObject.HintText;
                if (instanceContainer.Instance.StaticOptions.OptionFile.CheckHintMarkItem && CheckAction == MiscData.CheckState.Checked)
                {
                    if (TryMarkHintedCheck(hintObject, instanceContainer)) { ChangesMade = true; }
                }
            }

            //Cleanup======================================

            if (ChangesMade && checkState != MiscData.CheckState.Marked)
            {
                instanceContainer.logicCalculation.CalculateLogic(checkState);
                if (checkState == MiscData.CheckState.Checked && instanceContainer.Instance.StaticOptions.AutoCheckCoupleEntrances && !instanceContainer.Instance.StaticOptions.DecoupleEntrances && instanceContainer.Instance.CheckEntrancePair())
                {
                    instanceContainer.logicCalculation.CalculateLogic(checkState);
                }
            }
            return ChangesMade;
        }

        private static bool TryMarkHintedCheck(HintData.HintObject hintObject, MiscData.InstanceContainer instanceContainer)
        {
            bool ChangesMade = false;
            foreach(var i in hintObject.ParsedHintData)
            {
                var Location = instanceContainer.Instance.GetLocationByID(i.Key);
                var Item = instanceContainer.Instance.GetItemByID(i.Value);
                if (Location is not null && Item is not null && Location.CheckState == MiscData.CheckState.Unchecked && !string.IsNullOrWhiteSpace(Location.Randomizeditem.SpoilerLogGivenItem))
                {
                    Location.Randomizeditem.Item = Location.GetItemAtCheck(instanceContainer.Instance);
                    if (Location.ToggleChecked(MiscData.CheckState.Marked, instanceContainer.Instance)) { ChangesMade = true; }
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

            var AllExits = instance.EntrancePool.AreaList.Values.SelectMany(x => x.RandomizableExits(instance).Values);
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

        public static List<object> PopulateCheckedLocationList(DataSets DataSets, MiscData.Divider Divider, LogicObjects.TrackerInstance Instance, string Filter, out int OutItemsInListBox, out int OutItemsInListBoxFiltered, bool reverse = false)
        {
            var Groups = Utility.GetCategoriesFromFile(Instance);

            List<object> DataSource = new List<object>();

            var CheckedLocations = DataSets.CheckedLocations;
            CheckedLocations = CheckedLocations
                .OrderBy(x => (Groups.ContainsKey(x.GetDictEntry(Instance).Area.ToLower().Trim()) ? Groups[x.GetDictEntry(Instance).Area.ToLower().Trim()] : DataSets.CheckedLocations.Count + 1))
                .ThenBy(x => x.GetDictEntry(Instance).Area)
                .ThenBy(x => Utility.GetLocationDisplayName(x, Instance)).ToList();

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
                    if (i.IsUnrandomized(1) && !Filter.StartsWith("^")) { continue; }
                    i.DisplayName = Utility.GetLocationDisplayName(i, Instance);

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
                List<EntranceData.EntranceRandoExit> ValidExits = new List<EntranceData.EntranceRandoExit>();
                foreach (var area in Instance.EntrancePool.AreaList)
                {
                    var CheckLoadingZoneExits = area.Value.RandomizableExits(Instance).Where(x => x.Value.CheckState == MiscData.CheckState.Checked && EntranceAppearsinListbox(x.Value, Instance));
                    var FilteredCheckedExits = CheckLoadingZoneExits.Where(x => SearchStringParser.FilterSearch(Instance, x.Value, Filter, Utility.GetEntranceDisplayName(x.Value, Instance)));

                    ItemsInListBox += CheckLoadingZoneExits.Count();
                    ItemsInListBoxFiltered += FilteredCheckedExits.Count();
                    if (!FilteredCheckedExits.Any()) { continue; }
                    foreach (var i in FilteredCheckedExits)
                    {
                        i.Value.DisplayName = Utility.GetEntranceDisplayName(i.Value, Instance);
                        ValidExits.Add(i.Value);
                    }
                }
                ValidExits = ValidExits.OrderBy(x => x.DisplayArea(Instance)).ThenBy(x => x.DisplayName).ToList();
                string CurrentArea = "";
                foreach (var i in ValidExits)
                {
                    string ItemArea =  $"{i.DisplayArea(Instance)} Exits";
                    if (CurrentArea != ItemArea)
                    {
                        CurrentArea = ItemArea;
                        if (DataSource.Count > 0) { DataSource.Add(Divider); }
                        DataSource.Add(new MiscData.Areaheader { Area = CurrentArea });
                    }
                    DataSource.Add(i);
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
                        string Display = $"{i.GetDictEntry(Instance).GetName(Instance)} X{i.AmountInStartingpool}";
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
                            string Display = $"{i.GetDictEntry(Instance).GetName(Instance)} X{j.Value}: Player {j.Key}";
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

        public static string GetLocationEntryArea(object Entry, LogicObjects.TrackerInstance Instance)
        {
            if (Entry is LocationData.LocationObject l) { return l.GetDictEntry(Instance).Area; }
            else if (Entry is LocationData.LocationProxy p) { return p.Area; }
            return "Error";
        }

        public static List<object> PopulateAvailableLocationList(DataSets DataSets, MiscData.Divider Divider, LogicObjects.TrackerInstance Instance, string Filter, bool ShowUnavailable, out int OutItemsInListBox, out int OutItemsInListBoxFiltered, bool reverse = false)
        {
            bool ShowAllLocation = ShowUnavailable || (Filter.StartsWith("^") && !Filter.StartsWith("^^")) || Filter.StartsWith("^^^");
            bool ShowInvalidLocation = Filter.StartsWith("^^");

            var Groups = Utility.GetCategoriesFromFile(Instance);
            List<object> DataSource = new List<object>();

            var AvailableProxies = DataSets.AvailableProxies;
            if (ShowAllLocation) { AvailableProxies = DataSets.AllAvailableProxies; }

            var AvailableLocations = DataSets.AvailableLocations;
            if (ShowAllLocation) { AvailableLocations = DataSets.AllAvailableLocations; }

            IEnumerable<object> AvailableLocationsEntries = AvailableLocations.Where(x => !Instance.LocationProxyData.LocationsWithProxys.ContainsKey(x.ID));
            AvailableLocationsEntries = AvailableLocationsEntries.Concat(AvailableProxies);
            AvailableLocationsEntries = AvailableLocationsEntries.OrderBy(x => (Groups.ContainsKey(GetLocationEntryArea(x, Instance).ToLower().Trim()) ? Groups[GetLocationEntryArea(x, Instance).ToLower().Trim()] : DataSets.AvailableLocations.Count() + 1))
                .ThenBy(x => GetLocationEntryArea(x, Instance))
                .ThenBy(x => Utility.GetLocationDisplayName(x, Instance)).ToList();

            IEnumerable<object> HiddenLocations = AvailableLocationsEntries.Where(x => Utility.DynamicPropertyExist(x, "Hidden") && (x as dynamic).Hidden).OrderBy(x => Utility.GetLocationDisplayName(x, Instance));
            AvailableLocationsEntries = AvailableLocationsEntries.Where(x => !Utility.DynamicPropertyExist(x, "Hidden") || !(x as dynamic).Hidden);

            var AvailableHints = DataSets.AvailableHints;
            if (ShowAllLocation) { AvailableHints = DataSets.AllAvailableHints; }

            var ItemsInListBox = 0;
            var ItemsInListBoxFiltered = 0;

            if (reverse)
            {
                AvailableLocations.Reverse();
                WriteHiddenLocations();
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
                WriteHiddenLocations();
            }

            OutItemsInListBox = ItemsInListBox;
            OutItemsInListBoxFiltered = ItemsInListBoxFiltered;
            return DataSource;

            void WriteEntrances()
            {
                if (Instance.StaticOptions.EntranceRandoFeatures) { return; }
                var Entrances = PopulateAvailableEntraceList(DataSets, Divider, Instance, Filter, ShowUnavailable, out int EntCount, out int EntCountFiltered, reverse);
                if (Entrances.Any() && DataSource.Any()) { DataSource.Add(Divider); }
                DataSource.AddRange(Entrances);
                ItemsInListBox += EntCount;
                ItemsInListBoxFiltered += EntCountFiltered;
            }

            void WriteHiddenLocations()
            {
                List<object> TempDataSource = new List<object>();
                foreach (var obj in HiddenLocations)
                {
                    var CurrentArea = "";
                    if (obj is LocationData.LocationObject i)
                    {
                        if (!i.AppearsinListbox(Instance, ShowInvalidLocation)) { continue; }
                        i.DisplayName = Utility.GetLocationDisplayName(i, Instance);
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(Instance, i, Filter, i.DisplayName)) { continue; }
                        ItemsInListBoxFiltered++;
                        CurrentArea = i.GetDictEntry(Instance).Area;
                    }
                    else if (obj is LocationData.LocationProxy p)
                    {
                        if (!p.GetReferenceLocation(Instance).AppearsinListbox(Instance, ShowInvalidLocation)) { continue; }
                        p.DisplayName = Utility.GetLocationDisplayName(p, Instance);
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(Instance, p, Filter, p.DisplayName)) { continue; }
                        ItemsInListBoxFiltered++;
                        CurrentArea = p.Area;
                    }
                    else { continue; }
                    TempDataSource.Add(obj);
                }
                if (TempDataSource.Any())
                {
                    if (DataSource.Count > 0) { DataSource.Add(Divider); }
                    DataSource.Add(new MiscData.Areaheader { Area = "Hidden Locations" });
                    DataSource.AddRange(TempDataSource);
                }
            }

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

            void WriteLocations()
            {
                string CurrentLocation = "";
                foreach (var obj in AvailableLocationsEntries)
                {
                    var CurrentArea = "";
                    if (obj is LocationData.LocationObject i)
                    {
                        if (!i.AppearsinListbox(Instance, ShowInvalidLocation)) { continue; }
                        i.DisplayName = Utility.GetLocationDisplayName(i, Instance);
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(Instance, i, Filter, i.DisplayName)) { continue; }
                        ItemsInListBoxFiltered++;
                        CurrentArea = i.GetDictEntry(Instance).Area;
                    }
                    else if (obj is LocationData.LocationProxy p)
                    {
                        if (!p.GetReferenceLocation(Instance).AppearsinListbox(Instance, ShowInvalidLocation)) { continue; }
                        p.DisplayName = Utility.GetLocationDisplayName(p, Instance);
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(Instance, p, Filter, p.DisplayName)) { continue; }
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

            void WriteHints()
            {
                if (AvailableHints.Any())
                {
                    bool DividerCreated = false;
                    foreach (var i in AvailableHints)
                    {
                        if (i.RandomizedState == MiscData.RandomizedState.ForcedJunk && !ShowInvalidLocation) { continue; }
                        i.DisplayName = (i.CheckState == MiscData.CheckState.Marked) ? $"{i.GetDictEntry(Instance).Name}: {i.HintText}" : i.GetDictEntry(Instance).Name;
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(Instance, i, Filter, i.DisplayName)) { continue; }
                        ItemsInListBoxFiltered++;
                        if (!DividerCreated)
                        {
                            if (DataSource.Count > 0) { DataSource.Add(Divider); }
                            DataSource.Add(new MiscData.Areaheader { Area = "HINTS" });
                            DividerCreated = true;
                        }
                        DataSource.Add(i);
                    }
                }
            }
        }

        public static List<object> PopulateAvailableEntraceList(DataSets DataSets, MiscData.Divider Divider, LogicObjects.TrackerInstance Instance, string Filter, bool ShowUnavailable, out int OutItemsInListBox, out int OutItemsInListBoxFiltered, bool reverse = false)
        {
            bool InLocationBox = !Instance.StaticOptions.EntranceRandoFeatures;

            var Groups = Utility.GetCategoriesFromFile(Instance);
            List<object> DataSource = new List<object>();
            OutItemsInListBox = 0;
            OutItemsInListBoxFiltered = 0;

            List<EntranceData.EntranceRandoExit> ValidExits = new List<EntranceData.EntranceRandoExit>();

            foreach (var area in Instance.EntrancePool.AreaList)
            {
                var AvailableExits = area.Value.RandomizableExits(Instance).Where(x => 
                (x.Value.Available || x.Value.CheckState == MiscData.CheckState.Marked || ShowUnavailable || Filter.StartsWith("^")) && 
                x.Value.CheckState != MiscData.CheckState.Checked && EntranceAppearsinListbox(x.Value, Instance));

                var FilteredAvailableExits = AvailableExits.Where(x => SearchStringParser.FilterSearch(Instance, x.Value, Filter, Utility.GetEntranceDisplayName(x.Value, Instance)));

                OutItemsInListBox += AvailableExits.Count();
                OutItemsInListBoxFiltered += FilteredAvailableExits.Count();
                if (!FilteredAvailableExits.Any()) { continue; }

                foreach(var i in FilteredAvailableExits)
                {
                    i.Value.DisplayName = Utility.GetEntranceDisplayName(i.Value, Instance);
                    ValidExits.Add(i.Value);

                }
            }

            ValidExits = ValidExits.OrderBy(x => x.DisplayArea(Instance)).ThenBy(x => x.DisplayName).ToList();
            string CurrentArea = "";
            foreach(var i in ValidExits)
            {
                string ItemArea = InLocationBox ? $"{i.DisplayArea(Instance)} Entrances" : i.DisplayArea(Instance);
                if (CurrentArea != ItemArea)
                {
                    CurrentArea = ItemArea;
                    if (DataSource.Count > 0) { DataSource.Add(Divider); }
                    DataSource.Add(new MiscData.Areaheader { Area = CurrentArea });
                }
                DataSource.Add(i);
            }
            return DataSource;
        }

        private static bool EntranceAppearsinListbox(EntranceData.EntranceRandoExit Location, LogicObjects.TrackerInstance Instance)
        {
            return !Location.IsJunk() && !Location.IsUnrandomized(1);
        }
    }
}
