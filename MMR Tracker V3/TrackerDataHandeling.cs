using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        public static bool CheckSelectedItems(IEnumerable<object> SelectedObjects, MiscData.CheckState checkState, MiscData.InstanceContainer instanceContainer, Func<IEnumerable<object>, InstanceData.TrackerInstance, bool> CheckUnassignedLocations, Func<IEnumerable<object>, InstanceData.TrackerInstance, bool> CheckUnassignedVariable, bool EnforceMarkAction = false)
        {
            bool ChangesMade = false;

            //Handle Options
            IEnumerable<OptionData.ChoiceOption> choiceOptions = SelectedObjects.Where(x => x is OptionData.ChoiceOption).Select(x => x as OptionData.ChoiceOption);
            IEnumerable<OptionData.IntOption> IntOptions = SelectedObjects.Where(x => x is OptionData.IntOption).Select(x => x as OptionData.IntOption);
            IEnumerable<OptionData.ToggleOption> ToggleOptions = SelectedObjects.Where(x => x is OptionData.ToggleOption).Select(x => x as OptionData.ToggleOption);

            foreach (var i in ToggleOptions) 
            { 
                i.ToggleValue(); 
                ChangesMade = true; 
            }
            if (choiceOptions.Any()) 
            { 
                CheckUnassignedLocations(choiceOptions, instanceContainer.Instance);
                ChangesMade = true;
            }
            if (IntOptions.Any())
            {
                CheckUnassignedVariable(IntOptions, instanceContainer.Instance);
                ChangesMade = true;
            }

            //Handle Locations
            IEnumerable<LocationObject> locationObjects = SelectedObjects.Where(x => x is LocationObject).Select(x => x as LocationObject);
            locationObjects = locationObjects.Concat(SelectedObjects.Where(x => x is LocationProxy).Select(x => (x as LocationProxy).GetReferenceLocation(instanceContainer.Instance)));
            locationObjects = locationObjects.Distinct();
            //If we are performing an uncheck action there should be no unchecked locations in the list and even if there are nothing will be done to them anyway
            //This check is neccessary for the "UnMark Only" action.
            IEnumerable<LocationObject> UncheckedlocationObjects = (checkState == MiscData.CheckState.Unchecked) ?
                new List<LocationObject>() :
                locationObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);

            foreach (LocationObject LocationObject in UncheckedlocationObjects)
            {
                LocationObject.Randomizeditem.Item = LocationObject.GetItemAtCheck(instanceContainer.Instance);
            }
            //Get Entries that need a value manually assigned and pass them to given method to be assigned.
            IEnumerable<object> ManualLocationChecks = UncheckedlocationObjects.Where(x => x.Randomizeditem.Item == null); //Locations with no item
            if (ManualLocationChecks.Any())
            {
                CheckUnassignedLocations(ManualLocationChecks, instanceContainer.Instance);
                ChangesMade = true;
            }
            foreach (LocationObject LocationObject in locationObjects)
            {
                //When we mark a location, the action is always sent as Marked, but if the location is already marked we should instead Unchecked it unless EnforceMarkAction is true.
                var Action = (checkState == MiscData.CheckState.Marked && LocationObject.CheckState == MiscData.CheckState.Marked) && !EnforceMarkAction ? MiscData.CheckState.Unchecked : checkState;
                if (LocationObject.ToggleChecked(Action, instanceContainer.Instance)) { ChangesMade = true; }
            }

            //Handle Exits
            IEnumerable<EntranceData.EntranceRandoExit> ExitObjects = SelectedObjects.Where(x => x is EntranceData.EntranceRandoExit).Select(x => x as EntranceData.EntranceRandoExit);
            IEnumerable<EntranceData.EntranceRandoExit> UncheckedExitObjects = (checkState == MiscData.CheckState.Unchecked) ?
                new List<EntranceData.EntranceRandoExit>() :
                ExitObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);
            foreach (EntranceData.EntranceRandoExit ExitObject in UncheckedExitObjects)
            {
                ExitObject.DestinationExit = ExitObject.GetDestinationAtExit(instanceContainer.Instance);
            }
            IEnumerable<object> ManualExitChecks = UncheckedExitObjects.Where(x => x.DestinationExit == null); //Exits With No Destination
            if (ManualExitChecks.Any())
            {
                CheckUnassignedLocations(ManualExitChecks, instanceContainer.Instance);
                ChangesMade = true;
            }
            foreach (EntranceData.EntranceRandoExit ExitObject in ExitObjects)
            {
                var Action = (checkState == MiscData.CheckState.Marked && ExitObject.CheckState == MiscData.CheckState.Marked) && !EnforceMarkAction ? MiscData.CheckState.Unchecked : checkState;
                if (ExitObject.ToggleExitChecked(Action, instanceContainer.Instance)) { ChangesMade = true; }
            }

            //Hints======================================
            List<HintData.HintObject> HintObjects = SelectedObjects.Where(x => x is HintData.HintObject).Select(x => x as HintData.HintObject).ToList();

            var UncheckedHintObjects = HintObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);
            foreach (var i in UncheckedHintObjects.Where(x => !string.IsNullOrWhiteSpace(x.SpoilerHintText))) 
            { 
                i.HintText = i.SpoilerHintText; 
            }
            IEnumerable<object> UncheckedVariableObjects = UncheckedHintObjects.Where(x => string.IsNullOrWhiteSpace(x.HintText));
            if (UncheckedVariableObjects.Any())
            {
                CheckUnassignedVariable(UncheckedVariableObjects, instanceContainer.Instance);
                ChangesMade = true;
            }
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
                if (checkState == MiscData.CheckState.Checked && instanceContainer.Instance.StaticOptions.OptionFile.AutoCheckCoupleEntrances && instanceContainer.Instance.CheckEntrancePair())
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

        public static DataSets PopulateDataSets(InstanceData.TrackerInstance instance)
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

        public static List<object> PopulateCheckedLocationList(DataSets DataSets, MiscData.Divider Divider, MiscData.InstanceContainer IC, string Filter, out int OutItemsInListBox, out int OutItemsInListBoxFiltered, bool reverse = false)
        {
            var Groups = Utility.GetCategoriesFromFile(IC.Instance);

            List<object> DataSource = new List<object>();

            var CheckedLocations = DataSets.CheckedLocations;
            CheckedLocations = CheckedLocations
                .OrderBy(x => (Groups.ContainsKey(x.GetDictEntry(IC.Instance).Area.ToLower().Trim()) ? Groups[x.GetDictEntry(IC.Instance).Area.ToLower().Trim()] : DataSets.CheckedLocations.Count + 1))
                .ThenBy(x => x.GetDictEntry(IC.Instance).Area)
                .ThenBy(x => Utility.GetLocationDisplayName(x, IC.Instance)).ToList();

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
                if (IC.Instance.StaticOptions.ShowOptionsInListBox == null || IC.Instance.StaticOptions.ShowOptionsInListBox != OptionData.DisplayListBoxes[2]) { return; }

                List<dynamic> ChoiceOptions = IC.Instance.ChoiceOptions.Values.Where(x => x.ValueList.Count > 1).Cast<dynamic>().ToList();
                List<dynamic> ToggleOptions = IC.Instance.ToggleOptions.Values.Cast<dynamic>().ToList();
                List<dynamic> IntOptions = IC.Instance.IntOptions.Values.Cast<dynamic>().ToList();
                List<dynamic> All = ChoiceOptions.Concat(ToggleOptions).Concat(IntOptions).ToList();

                Dictionary<string, List<dynamic>> Categorized = new Dictionary<string, List<dynamic>>();
                foreach (var item in All)
                {
                    if (!IC.logicCalculation.ConditionalsMet(item.Conditionals, new List<string>())) { continue; }
                    string Sub = item.SubCategory;
                    Sub ??= "";
                    if (!Categorized.ContainsKey(Sub)) { Categorized.Add(Sub, new List<dynamic>()); }
                    Categorized[Sub].Add(item);
                }

                string CurrentCategory = null;
                foreach(var i in Categorized)
                {
                    foreach(var c in i.Value)
                    {
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, c, Filter, c.ToString())) { continue; }
                        if (CurrentCategory is null || CurrentCategory != i.Key)
                        {
                            if (DataSource.Count > 0) { DataSource.Add(Divider); }
                            DataSource.Add(new MiscData.Areaheader { Area = i.Key == "" ? "Options" : $"Options: {i.Key}" });
                            CurrentCategory = i.Key;
                        }
                        ItemsInListBoxFiltered++;
                        DataSource.Add(c);
                    }
                }
            }

            void WriteLocations()
            {
                string CurrentLocation = "";
                foreach (var i in CheckedLocations)
                {
                    if (i.IsUnrandomized(MiscData.UnrandState.Unrand) && !Filter.StartsWith("^")) { continue; }
                    i.DisplayName = Utility.GetLocationDisplayName(i, IC.Instance);

                    ItemsInListBox++;
                    if (!SearchStringParser.FilterSearch(IC.Instance, i, Filter, i.DisplayName)) { continue; }
                    ItemsInListBoxFiltered++;
                    if (CurrentLocation != i.GetDictEntry(IC.Instance).Area)
                    {
                        if (DataSource.Count > 0) { DataSource.Add(Divider); }
                        DataSource.Add(new MiscData.Areaheader { Area = i.GetDictEntry(IC.Instance).Area });
                        CurrentLocation = i.GetDictEntry(IC.Instance).Area;
                    }
                    DataSource.Add(i);
                }
            }

            void WriteEntrances()
            {
                List<EntranceData.EntranceRandoExit> ValidExits = new List<EntranceData.EntranceRandoExit>();
                foreach (var area in IC.Instance.EntrancePool.AreaList)
                {
                    var CheckLoadingZoneExits = area.Value.RandomizableExits(IC.Instance).Where(x => x.Value.CheckState == MiscData.CheckState.Checked && EntranceAppearsinListbox(x.Value, IC.Instance));
                    var FilteredCheckedExits = CheckLoadingZoneExits.Where(x => SearchStringParser.FilterSearch(IC.Instance, x.Value, Filter, Utility.GetEntranceDisplayName(x.Value, IC.Instance)));

                    ItemsInListBox += CheckLoadingZoneExits.Count();
                    ItemsInListBoxFiltered += FilteredCheckedExits.Count();
                    if (!FilteredCheckedExits.Any()) { continue; }
                    foreach (var i in FilteredCheckedExits)
                    {
                        i.Value.DisplayName = Utility.GetEntranceDisplayName(i.Value, IC.Instance);
                        ValidExits.Add(i.Value);
                    }
                }
                ValidExits = ValidExits.OrderBy(x => x.DisplayArea(IC.Instance)).ThenBy(x => x.DisplayName).ToList();
                string CurrentArea = "";
                foreach (var i in ValidExits)
                {
                    string ItemArea =  $"{i.DisplayArea(IC.Instance)} Exits";
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
                        i.DisplayName = $"{i.GetDictEntry(IC.Instance).Name}: {i.HintText}";
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, i, Filter, i.DisplayName)) { continue; }
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
                        string Display = $"{i.GetDictEntry(IC.Instance).GetName(IC.Instance)} X{i.AmountInStartingpool}";
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, i, Filter, Display)) { continue; }
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
                            string Display = $"{i.GetDictEntry(IC.Instance).GetName(IC.Instance)} X{j.Value}: Player {j.Key}";
                            ItemsInListBox++;
                            if (!SearchStringParser.FilterSearch(IC.Instance, i, Filter, Display)) { continue; }
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

        public static string GetLocationEntryArea(object Entry, InstanceData.TrackerInstance Instance)
        {
            if (Entry is LocationData.LocationObject l) { return l.GetDictEntry(Instance).Area; }
            else if (Entry is LocationData.LocationProxy p) { return p.Area; }
            return "Error";
        }

        public static List<object> PopulateAvailableLocationList(DataSets DataSets, MiscData.Divider Divider, MiscData.InstanceContainer IC, string Filter, bool ShowUnavailable, out int OutItemsInListBox, out int OutItemsInListBoxFiltered, bool reverse = false)
        {
            bool ShowAllLocation = ShowUnavailable || (Filter.StartsWith("^") && !Filter.StartsWith("^^")) || Filter.StartsWith("^^^");
            bool ShowInvalidLocation = Filter.StartsWith("^^");

            var Groups = Utility.GetCategoriesFromFile(IC.Instance);
            List<object> DataSource = new List<object>();

            var AvailableProxies = DataSets.AvailableProxies;
            if (ShowAllLocation) { AvailableProxies = DataSets.AllAvailableProxies; }

            var AvailableLocations = DataSets.AvailableLocations;
            if (ShowAllLocation) { AvailableLocations = DataSets.AllAvailableLocations; }

            IEnumerable<object> AvailableLocationsEntries = AvailableLocations.Where(x => !IC.Instance.LocationProxyData.LocationsWithProxys.ContainsKey(x.ID));
            AvailableLocationsEntries = AvailableLocationsEntries.Concat(AvailableProxies);
            AvailableLocationsEntries = AvailableLocationsEntries.OrderBy(x => (Groups.ContainsKey(GetLocationEntryArea(x, IC.Instance).ToLower().Trim()) ? Groups[GetLocationEntryArea(x, IC.Instance).ToLower().Trim()] : DataSets.AvailableLocations.Count() + 1))
                .ThenBy(x => GetLocationEntryArea(x, IC.Instance))
                .ThenBy(x => Utility.GetLocationDisplayName(x, IC.Instance)).ToList();

            IEnumerable<object> HiddenLocations = AvailableLocationsEntries.Where(x => Utility.DynamicPropertyExist(x, "Hidden") && (x as dynamic).Hidden).OrderBy(x => Utility.GetLocationDisplayName(x, IC.Instance));
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
                if (IC.Instance.StaticOptions.OptionFile.EntranceRandoFeatures) { return; }
                var Entrances = PopulateAvailableEntraceList(DataSets, Divider, IC, Filter, ShowUnavailable, out int EntCount, out int EntCountFiltered, reverse);
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
                        if (!i.AppearsinListbox(IC.Instance, ShowInvalidLocation)) { continue; }
                        i.DisplayName = Utility.GetLocationDisplayName(i, IC.Instance);
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, i, Filter, i.DisplayName)) { continue; }
                        ItemsInListBoxFiltered++;
                        CurrentArea = i.GetDictEntry(IC.Instance).Area;
                    }
                    else if (obj is LocationData.LocationProxy p)
                    {
                        if (!p.GetReferenceLocation(IC.Instance).AppearsinListbox(IC.Instance, ShowInvalidLocation)) { continue; }
                        p.DisplayName = Utility.GetLocationDisplayName(p, IC.Instance);
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, p, Filter, p.DisplayName)) { continue; }
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
                if (IC.Instance.StaticOptions.ShowOptionsInListBox == null || IC.Instance.StaticOptions.ShowOptionsInListBox != OptionData.DisplayListBoxes[1]) { return; }

                List<dynamic> ChoiceOptions = IC.Instance.ChoiceOptions.Values.Where(x => x.ValueList.Count > 1).Cast<dynamic>().ToList();
                List<dynamic> ToggleOptions = IC.Instance.ToggleOptions.Values.Cast<dynamic>().ToList();
                List<dynamic> IntOptions = IC.Instance.IntOptions.Values.Cast<dynamic>().ToList();
                List<dynamic> All = ChoiceOptions.Concat(ToggleOptions).Concat(IntOptions).ToList();

                Dictionary<string, List<dynamic>> Categorized = new Dictionary<string, List<dynamic>>();
                foreach (var item in All)
                {
                    if (!IC.logicCalculation.ConditionalsMet(item.Conditionals, new List<string>())) { continue; }
                    string Sub = item.SubCategory;
                    Sub ??= "";
                    if (!Categorized.ContainsKey(Sub)) { Categorized.Add(Sub, new List<dynamic>()); }
                    Categorized[Sub].Add(item);
                }

                string CurrentCategory = null;
                foreach (var i in Categorized)
                {
                    foreach (var c in i.Value)
                    {
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, c, Filter, c.ToString())) { continue; }
                        if (CurrentCategory is null || CurrentCategory != i.Key)
                        {
                            if (DataSource.Count > 0) { DataSource.Add(Divider); }
                            DataSource.Add(new MiscData.Areaheader { Area = i.Key == "" ? "Options" : $"Options: {i.Key}" });
                            CurrentCategory = i.Key;
                        }
                        ItemsInListBoxFiltered++;
                        DataSource.Add(c);
                    }
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
                        if (!i.AppearsinListbox(IC.Instance, ShowInvalidLocation)) { continue; }
                        i.DisplayName = Utility.GetLocationDisplayName(i, IC.Instance);
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, i, Filter, i.DisplayName)) { continue; }
                        ItemsInListBoxFiltered++;
                        CurrentArea = i.GetDictEntry(IC.Instance).Area;
                    }
                    else if (obj is LocationData.LocationProxy p)
                    {
                        if (!p.GetReferenceLocation(IC.Instance).AppearsinListbox(IC.Instance, ShowInvalidLocation)) { continue; }
                        p.DisplayName = Utility.GetLocationDisplayName(p, IC.Instance);
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, p, Filter, p.DisplayName)) { continue; }
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
                        i.DisplayName = (i.CheckState == MiscData.CheckState.Marked) ? $"{i.GetDictEntry(IC.Instance).Name}: {i.HintText}" : i.GetDictEntry(IC.Instance).Name;
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, i, Filter, i.DisplayName)) { continue; }
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

        public static List<object> PopulateAvailableEntraceList(DataSets DataSets, MiscData.Divider Divider, MiscData.InstanceContainer IC, string Filter, bool ShowUnavailable, out int OutItemsInListBox, out int OutItemsInListBoxFiltered, bool reverse = false)
        {
            bool InLocationBox = !IC.Instance.StaticOptions.OptionFile.EntranceRandoFeatures;

            var Groups = Utility.GetCategoriesFromFile(IC.Instance);
            List<object> DataSource = new List<object>();
            OutItemsInListBox = 0;
            OutItemsInListBoxFiltered = 0;

            List<EntranceData.EntranceRandoExit> ValidExits = new List<EntranceData.EntranceRandoExit>();

            foreach (var area in IC.Instance.EntrancePool.AreaList)
            {
                var AvailableExits = area.Value.RandomizableExits(IC.Instance).Where(x => 
                (x.Value.Available || x.Value.CheckState == MiscData.CheckState.Marked || ShowUnavailable || Filter.StartsWith("^")) && 
                x.Value.CheckState != MiscData.CheckState.Checked && EntranceAppearsinListbox(x.Value, IC.Instance));

                var FilteredAvailableExits = AvailableExits.Where(x => SearchStringParser.FilterSearch(IC.Instance, x.Value, Filter, Utility.GetEntranceDisplayName(x.Value, IC.Instance)));

                OutItemsInListBox += AvailableExits.Count();
                OutItemsInListBoxFiltered += FilteredAvailableExits.Count();
                if (!FilteredAvailableExits.Any()) { continue; }

                foreach(var i in FilteredAvailableExits)
                {
                    i.Value.DisplayName = Utility.GetEntranceDisplayName(i.Value, IC.Instance);
                    ValidExits.Add(i.Value);

                }
            }

            ValidExits = ValidExits.OrderBy(x => x.DisplayArea(IC.Instance)).ThenBy(x => x.DisplayName).ToList();
            string CurrentArea = "";
            foreach(var i in ValidExits)
            {
                string ItemArea = InLocationBox ? $"{i.DisplayArea(IC.Instance)} Entrances" : i.DisplayArea(IC.Instance);
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

        private static bool EntranceAppearsinListbox(EntranceData.EntranceRandoExit Location, InstanceData.TrackerInstance Instance)
        {
            return !Location.IsJunk() && !Location.IsUnrandomized(MiscData.UnrandState.Unrand);
        }
    }
}
