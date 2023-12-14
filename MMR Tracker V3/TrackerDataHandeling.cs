using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using static MMR_Tracker_V3.TrackerObjects.EntranceData;
using static MMR_Tracker_V3.TrackerObjects.HintData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;

namespace MMR_Tracker_V3
{
    public class TrackerDataHandeling
    {
        public class DataSets
        {
            public List<LocationData.LocationObject> LocationStateIsUnchecked { get; set; } = new List<LocationData.LocationObject>();
            public List<LocationData.LocationObject> LocationISMarkedOrISAvailableAndUnchecked { get; set; } = new List<LocationData.LocationObject>();
            public List<LocationData.LocationObject> LocationStateIsNOTChecked { get; set; } = new List<LocationData.LocationObject>();
            public List<LocationData.LocationObject> LocationStateIsMarked { get; set; } = new List<LocationData.LocationObject>();
            public List<LocationData.LocationObject> LocationStateIsChecked { get; set; } = new List<LocationData.LocationObject>();

            public List<LocationData.LocationProxy> ProxyISMarkedOrISAvailableAndUnchecked { get; set; } = new List<LocationData.LocationProxy>();
            public List<LocationData.LocationProxy> ProxyStateIsNOTChecked { get; set; } = new List<LocationData.LocationProxy>();
            public List<LocationData.LocationProxy> ProxyStateIsMarked { get; set; } = new List<LocationData.LocationProxy>();
            public List<LocationData.LocationProxy> ProxyStateIsChecked { get; set; } = new List<LocationData.LocationProxy>();

            public List<EntranceData.EntranceRandoExit> ExitStateIsUnchecked { get; set; } = new List<EntranceData.EntranceRandoExit>();
            public List<EntranceData.EntranceRandoExit> ExitISMarkedOrISAvailableAndUnchecked { get; set; } = new List<EntranceData.EntranceRandoExit>();
            public List<EntranceData.EntranceRandoExit> ExitStateIsNOTChecked { get; set; } = new List<EntranceData.EntranceRandoExit>();
            public List<EntranceData.EntranceRandoExit> ExitStateIsChecked { get; set; } = new List<EntranceData.EntranceRandoExit>();
            public List<EntranceData.EntranceRandoExit> ExitStateIsMarked { get; set; } = new List<EntranceData.EntranceRandoExit>();

            public List<HintData.HintObject> HIntISMarkedOrISAvailableAndUnchecked { get; set; } = new List<HintData.HintObject>();
            public List<HintData.HintObject> HintStateIsNOTChecked { get; set; } = new List<HintData.HintObject>();
            public List<HintData.HintObject> HintStateIsUnchecked { get; set; } = new List<HintData.HintObject>();
            public List<HintData.HintObject> HintStateIsChecked { get; set; } = new List<HintData.HintObject>();
            public List<HintData.HintObject> HistStateIsMarked { get; set; } = new List<HintData.HintObject>();

            public List<MacroObject> Tricks { get; set; } = new List<MacroObject>();
            public List<ItemData.ItemObject> AvailableStartingItems { get; set; } = new List<ItemData.ItemObject>();

            public List<ItemData.ItemObject> LocalObtainedItems { get; set; } = new List<ItemData.ItemObject>();
            public List<ItemData.ItemObject> CurrentStartingItems { get; set; } = new List<ItemData.ItemObject>();
            public List<ItemData.ItemObject> OnlineObtainedItems { get; set; } = new List<ItemData.ItemObject>();
        }

        public static DataSets PopulateDataSets(InstanceData.TrackerInstance instance)
        {
            DataSets dataSets = new DataSets();

            bool ShowUnavailableMarkedLocations = instance.StaticOptions.OptionFile.ShowUnavailableMarkedLocations;

            foreach (var i in instance.LocationPool.Values)
            {
                switch (i.CheckState)
                {
                    case CheckState.Checked:
                        dataSets.LocationStateIsChecked.Add(i);
                        break;
                    case CheckState.Marked:
                        dataSets.LocationStateIsMarked.Add(i);
                        dataSets.LocationStateIsNOTChecked.Add(i);
                        if (ShowUnavailableMarkedLocations) { dataSets.LocationISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                    case CheckState.Unchecked:
                        dataSets.LocationStateIsUnchecked.Add(i);
                        dataSets.LocationStateIsNOTChecked.Add(i);
                        if (i.Available) { dataSets.LocationISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                }
            }

            //dataSets.UncheckedLocations = instance.LocationPool.Values.Where(x => x.CheckState == MiscData.CheckState.Unchecked).ToList();
            //dataSets.MarkedLocations = instance.LocationPool.Values.Where(x => x.CheckState == MiscData.CheckState.Marked).ToList();
            //dataSets.CheckedLocations = instance.LocationPool.Values.Where(x => x.CheckState == MiscData.CheckState.Checked).ToList();
            //dataSets.UncheckedOrMarkedLocations = instance.LocationPool.Values.Where(x => x.CheckState != MiscData.CheckState.Checked).ToList();
            //dataSets.AvailableLocations = dataSets.UncheckedOrMarkedLocations.Where(x => x.Available || x.CheckState == MiscData.CheckState.Marked).ToList();

            foreach(var i in instance.LocationProxyData.LocationProxies.Values)
            {
                var CheckState = i.GetReferenceLocation().CheckState;
                switch (CheckState)
                {
                    case MiscData.CheckState.Checked:
                        dataSets.ProxyStateIsChecked.Add(i);
                        break;
                    case MiscData.CheckState.Marked:
                        dataSets.ProxyStateIsNOTChecked.Add(i);
                        dataSets.ProxyStateIsMarked.Add(i);
                        if (ShowUnavailableMarkedLocations) { dataSets.ProxyISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                    case MiscData.CheckState.Unchecked:
                        dataSets.ProxyStateIsNOTChecked.Add(i);
                        if (i.ProxyAvailable()) { dataSets.ProxyISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                }
            }

            //dataSets.ProxyStateIsMarked = instance.LocationProxyData.LocationProxies.Values.Where(x => x.GetReferenceLocation(instance).CheckState == MiscData.CheckState.Marked).ToList();
            //dataSets.ProxyStateIsNOTChecked = instance.LocationProxyData.LocationProxies.Values.Where(x => x.GetReferenceLocation(instance).CheckState != MiscData.CheckState.Checked).ToList();
            //dataSets.ProxyStateIsMarkedAndAvailable = dataSets.ProxyStateIsNOTChecked.Where(x => x.ProxyAvailable(instance) || x.GetReferenceLocation(instance).CheckState == MiscData.CheckState.Marked).ToList();

            var AllExits = instance.EntrancePool.AreaList.Values.SelectMany(x => x.RandomizableExits().Values);
            foreach (var i in AllExits)
            {
                switch (i.CheckState)
                {
                    case CheckState.Checked:
                        dataSets.ExitStateIsChecked.Add(i);
                        break;
                    case CheckState.Marked:
                        dataSets.ExitStateIsMarked.Add(i);
                        dataSets.ExitStateIsNOTChecked.Add(i);
                        if (ShowUnavailableMarkedLocations) { dataSets.ExitISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                    case CheckState.Unchecked:
                        dataSets.ExitStateIsUnchecked.Add(i);
                        dataSets.ExitStateIsNOTChecked.Add(i);
                        if(i.Available) { dataSets.ExitISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                }
            }
            //dataSets.ExitStateIsUnchecked = AllExits.Where(x => x.CheckState == MiscData.CheckState.Unchecked).ToList();
            //dataSets.ExitStateIsChecked = AllExits.Where(x => x.CheckState == MiscData.CheckState.Checked).ToList();
            //dataSets.ExitStateIsMarked = AllExits.Where(x => x.CheckState == MiscData.CheckState.Marked).ToList();
            //dataSets.ExitStateIsNOTChecked = AllExits.Where(x => x.CheckState != MiscData.CheckState.Checked).ToList();
            //dataSets.ExitISMarkedOrISAvailableAndUnchecked = AllExits.Where(x => x.Available || x.CheckState == MiscData.CheckState.Marked).ToList();

            foreach (var i in instance.HintPool.Values)
            {
                switch (i.CheckState)
                {
                    case CheckState.Checked:
                        dataSets.HintStateIsChecked.Add(i);
                        break;
                    case CheckState.Marked:
                        dataSets.HistStateIsMarked.Add(i);
                        dataSets.HintStateIsNOTChecked.Add(i);
                        if (ShowUnavailableMarkedLocations) { dataSets.HIntISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                    case CheckState.Unchecked:
                        dataSets.HintStateIsUnchecked.Add(i);
                        dataSets.HintStateIsNOTChecked.Add(i);
                        if (i.Available) { dataSets.HIntISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                }
            }

            //dataSets.HintStateIsUnchecked = instance.HintPool.Values.Where(x => x.CheckState == MiscData.CheckState.Unchecked).ToList();
            //dataSets.HistStateIsMarked = instance.HintPool.Values.Where(x => x.CheckState == MiscData.CheckState.Marked).ToList();
            //dataSets.HintStateIsChecked = instance.HintPool.Values.Where(x => x.CheckState == MiscData.CheckState.Checked).ToList();
            //dataSets.HintStateIsNOTChecked = instance.HintPool.Values.Where(x => x.CheckState != MiscData.CheckState.Checked).ToList();
            //dataSets.HIntISMarkedOrISAvailableAndUnchecked = dataSets.HintStateIsNOTChecked.Where(x => x.Available || x.CheckState == MiscData.CheckState.Marked).ToList();

            dataSets.Tricks = instance.MacroPool.Values.Where(x => x.isTrick()).ToList();
            dataSets.AvailableStartingItems = instance.ItemPool.Values.Where(x => x.ValidStartingItem()).ToList();

            dataSets.LocalObtainedItems = instance.ItemPool.Values.Where(x => x.AmountAquiredLocally > 0).ToList();
            dataSets.CurrentStartingItems = instance.ItemPool.Values.Where(x => x.AmountInStartingpool > 0).ToList();
            dataSets.OnlineObtainedItems = instance.ItemPool.Values.Where(x => x.AmountAquiredOnline.Any(x => x.Value > 0)).ToList();
            return dataSets;
        }

        public static string GetLocationEntryArea(object Entry, InstanceData.TrackerInstance Instance)
        {
            if (Entry is LocationData.LocationObject l) { return l.GetDictEntry().Area; }
            else if (Entry is LocationData.LocationProxy p) { return p.Area; }
            return "Error";
        }

        private static bool EntranceAppearsinListbox(EntranceData.EntranceRandoExit Location, InstanceData.TrackerInstance Instance)
        {
            return !Location.IsJunk() && !Location.IsUnrandomized(MiscData.UnrandState.Unrand);
        }

        //GetData To Print
        public static bool SortByAvailability(object Entry, TrackerLocationDataList Data)
        {
            if (Data.ShowUnavailableEntries) { return true; }
            if (!Data.Instance.StaticOptions.OptionFile.SeperateUnavailableMarkedLocations) { return true; }
            if (Entry is LocationData.LocationObject l) { return l.Available; }
            else if (Entry is LocationData.LocationProxy p) { return p.ProxyAvailable(); }
            return false;
        }

        public static void PopulateAvailableLocationList(TrackerLocationDataList Data)
        {
            var Groups = Utility.GetCategoriesFromFile(Data.Instance);

            var AvailableProxies = Data.DataSets.ProxyISMarkedOrISAvailableAndUnchecked;
            if (Data.ShowUnavailableEntries) { AvailableProxies = Data.DataSets.ProxyStateIsNOTChecked; }

            var AvailableLocations = Data.DataSets.LocationISMarkedOrISAvailableAndUnchecked;
            if (Data.ShowUnavailableEntries) { AvailableLocations = Data.DataSets.LocationStateIsNOTChecked; }

            IEnumerable<object> AvailableLocationsEntries = AvailableLocations.Where(x => !Data.Instance.LocationProxyData.LocationsWithProxys.ContainsKey(x.ID));
            AvailableLocationsEntries = AvailableLocationsEntries.Concat(AvailableProxies);
            AvailableLocationsEntries = AvailableLocationsEntries.OrderByDescending(x => SortByAvailability(x, Data))
                .ThenBy(x => (Groups.ContainsKey(GetLocationEntryArea(x, Data.Instance).ToLower().Trim()) ? Groups[GetLocationEntryArea(x, Data.Instance).ToLower().Trim()] : Data.DataSets.LocationISMarkedOrISAvailableAndUnchecked.Count() + 1))
                .ThenBy(x => GetLocationEntryArea(x, Data.Instance))
                .ThenBy(x => Utility.GetLocationDisplayName(x, Data.InstanceContainer)).ToList();

            IEnumerable<object> HiddenLocations = AvailableLocationsEntries.Where(x => Utility.DynamicPropertyExist(x, "Hidden") && (x as dynamic).Hidden).OrderBy(x => Utility.GetLocationDisplayName(x, Data.InstanceContainer));
            AvailableLocationsEntries = AvailableLocationsEntries.Where(x => !Utility.DynamicPropertyExist(x, "Hidden") || !(x as dynamic).Hidden);

            var AvailableHints = Data.DataSets.HIntISMarkedOrISAvailableAndUnchecked;
            if (Data.ShowUnavailableEntries) { AvailableHints = Data.DataSets.HintStateIsNOTChecked; }


            if (Data.Reverse)
            {
                AvailableLocations.Reverse();
                WriteHiddenLocations(Data, HiddenLocations);
                WriteOptions(Data, 1);
                WriteHints(Data, AvailableHints);
                WriteEntrances();
                WriteLocations(Data, AvailableLocationsEntries);
            }
            else
            {
                WriteLocations(Data, AvailableLocationsEntries);
                WriteEntrances();
                WriteHints(Data, AvailableHints);
                WriteOptions(Data, 1);
                WriteHiddenLocations(Data, HiddenLocations);
            }

            return;

            void WriteEntrances()
            {
                if (Data.InstanceContainer.Instance.StaticOptions.OptionFile.EntranceRandoFeatures) { return; }
                PopulateAvailableEntraceList(Data);
            }
        }

        public static void PopulateAvailableEntraceList(TrackerLocationDataList Data)
        {
            bool InLocationBox = !Data.Instance.StaticOptions.OptionFile.EntranceRandoFeatures;

            bool SeperateMarked = Data.Instance.StaticOptions.OptionFile.SeperateUnavailableMarkedLocations;

            var Groups = Utility.GetCategoriesFromFile(Data.Instance);

            List<EntranceRandoExit> ValidExits = Data.DataSets.ExitISMarkedOrISAvailableAndUnchecked;
            if (Data.ShowUnavailableEntries) { ValidExits = Data.DataSets.ExitStateIsNOTChecked; }

            ValidExits = ValidExits.OrderByDescending(x => SeperateMarked && x.Available).ThenBy(x => x.DisplayArea()).ThenBy(x => x.DisplayName).ToList();
            string CurrentArea = "";
            foreach(var i in ValidExits)
            {
                if (!EntranceAppearsinListbox(i, Data.Instance) && !Data.ShowInvalidEntries) { continue; }
                Data.ItemsFound++;
                string ItemArea = InLocationBox ? $"{i.DisplayArea()} Entrances" : i.DisplayArea();
                i.DisplayName = i.GetEntranceDisplayName();
                if (!SearchStringParser.FilterSearch(Data.Instance, i, Data.Filter, i.GetEntranceDisplayName())) { continue; }
                Data.ItemsDisplayed++;
                if (CurrentArea != ItemArea)
                {
                    CurrentArea = ItemArea;
                    if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                    Data.FinalData.Add(new MiscData.Areaheader { Area = CurrentArea });
                }
                Data.FinalData.Add(i);
            }
        }

        public static void PopulateCheckedLocationList(TrackerLocationDataList Data)
        {
            var Groups = Utility.GetCategoriesFromFile(Data.Instance);
            IEnumerable<object> CheckedLocations = Data.DataSets.LocationStateIsChecked.Where(x => !Data.Instance.LocationProxyData.LocationsWithProxys.ContainsKey(x.ID));
            CheckedLocations = CheckedLocations.Concat(Data.DataSets.ProxyStateIsChecked);
            CheckedLocations = CheckedLocations
                .OrderBy(x => (Groups.ContainsKey(GetLocationEntryArea(x, Data.Instance).ToLower().Trim()) ? Groups[GetLocationEntryArea(x, Data.Instance).ToLower().Trim()] : Data.DataSets.LocationISMarkedOrISAvailableAndUnchecked.Count() + 1))
                .ThenBy(x => GetLocationEntryArea(x, Data.Instance))
                .ThenBy(x => Utility.GetLocationDisplayName(x, Data.InstanceContainer)).ToList();

            if (Data.Reverse)
            {
                CheckedLocations.Reverse();
                WriteStartingAndOnlineItems(Data);
                WriteOptions(Data, 2);
                WriteHints(Data, Data.DataSets.HintStateIsChecked);
                WriteCheckedEntrances(Data);
                WriteLocations(Data, CheckedLocations);
            }
            else
            {
                WriteLocations(Data, CheckedLocations);
                WriteCheckedEntrances(Data);
                WriteHints(Data, Data.DataSets.HintStateIsChecked);
                WriteOptions(Data, 2);
                WriteStartingAndOnlineItems(Data);
            }
        }

        //
        private static void WriteOptions(TrackerLocationDataList Data, int DisplayListBoxesIndex)
        {
            if (Data.Instance.StaticOptions.ShowOptionsInListBox == null || Data.Instance.StaticOptions.ShowOptionsInListBox != OptionData.DisplayListBoxes[DisplayListBoxesIndex]) { return; }

            List<dynamic> ChoiceOptions = Data.Instance.ChoiceOptions.Values.Where(x => x.ValueList.Count > 1).Cast<dynamic>().ToList();
            List<dynamic> MultiSelectOptions = Data.Instance.MultiSelectOptions.Values.Cast<dynamic>().ToList();
            List<dynamic> ToggleOptions = Data.Instance.ToggleOptions.Values.Cast<dynamic>().ToList();
            List<dynamic> IntOptions = Data.Instance.IntOptions.Values.Cast<dynamic>().ToList();
            List<dynamic> All = ChoiceOptions.Concat(ToggleOptions).Concat(IntOptions).Concat(MultiSelectOptions).OrderBy(x => x.Priority).ToList();

            Dictionary<string, List<dynamic>> Categorized = new Dictionary<string, List<dynamic>>();
            foreach (var item in All)
            {
                if (!Data.InstanceContainer.logicCalculation.ConditionalsMet(item.Conditionals, new List<string>())) { continue; }
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
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, c, Data.Filter, c.ToString())) { continue; }
                    if (CurrentCategory is null || CurrentCategory != i.Key)
                    {
                        if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                        Data.FinalData.Add(new MiscData.Areaheader { Area = i.Key == "" ? "Options" : $"Options: {i.Key}" });
                        CurrentCategory = i.Key;
                    }
                    Data.ItemsDisplayed++;
                    Data.FinalData.Add(c);
                    if (c is OptionData.MultiSelectOption MSO)
                    {
                        foreach (var op in MSO.ValueList.Values)
                        {
                            Data.FinalData.Add(new OptionData.MultiSelectValueListDisplay { Parent = MSO, Value = op });
                        }
                    }
                }
            }
        }
        private static void WriteHints(TrackerLocationDataList Data, IEnumerable<HintObject> HintList)
        {
            if (HintList.Any())
            {
                bool DividerCreated = false;
                foreach (var i in HintList)
                {
                    if (i.RandomizedState == MiscData.RandomizedState.ForcedJunk && !Data.ShowInvalidEntries) { continue; }
                    i.DisplayName = (i.CheckState != MiscData.CheckState.Unchecked) ? $"{i.GetDictEntry().Name}: {i.HintText}" : i.GetDictEntry().Name;
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, i, Data.Filter, i.DisplayName)) { continue; }
                    Data.ItemsDisplayed++;
                    if (!DividerCreated)
                    {
                        if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                        Data.FinalData.Add(new MiscData.Areaheader { Area = "HINTS" });
                        DividerCreated = true;
                    }
                    Data.FinalData.Add(i);
                }
            }
        }
        private static void WriteStartingAndOnlineItems(TrackerLocationDataList Data)
        {
            if (Data.DataSets.CurrentStartingItems.Any())
            {
                bool DividerCreated = false;
                foreach (var i in Data.DataSets.CurrentStartingItems)
                {
                    string Display = $"{i.GetDictEntry().GetName()} X{i.AmountInStartingpool}";
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, i, Data.Filter, Display)) { continue; }
                    if (!DividerCreated)
                    {
                        if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                        Data.FinalData.Add(new MiscData.Areaheader { Area = "Starting Items" });
                        DividerCreated = true;
                    }
                    Data.ItemsDisplayed++;
                    Data.FinalData.Add(Display);
                }
            }

            if (Data.DataSets.OnlineObtainedItems.Any())
            {
                bool DividerCreated = false;
                foreach (var i in Data.DataSets.OnlineObtainedItems)
                {
                    foreach (var j in i.AmountAquiredOnline)
                    {
                        string Display = $"{i.GetDictEntry().GetName()} X{j.Value}: Player {j.Key}";
                        Data.ItemsFound++;
                        if (!SearchStringParser.FilterSearch(Data.Instance, i, Data.Filter, Display)) { continue; }
                        if (!DividerCreated)
                        {
                            if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                            Data.FinalData.Add(new MiscData.Areaheader { Area = "MultiWorld Items" });
                            DividerCreated = true;
                        }
                        Data.ItemsDisplayed++;
                        Data.FinalData.Add(Display);
                    }
                }
            }
        }
        private static void WriteCheckedEntrances(TrackerLocationDataList Data)
        {
            List<EntranceData.EntranceRandoExit> ValidExits = new List<EntranceData.EntranceRandoExit>();
            foreach (var area in Data.Instance.EntrancePool.AreaList)
            {
                var CheckLoadingZoneExits = area.Value.RandomizableExits().Where(x => x.Value.CheckState == MiscData.CheckState.Checked && EntranceAppearsinListbox(x.Value, Data.Instance));
                var FilteredCheckedExits = CheckLoadingZoneExits.Where(x => SearchStringParser.FilterSearch(Data.Instance, x.Value, Data.Filter, x.Value.GetEntranceDisplayName()));

                Data.ItemsFound += CheckLoadingZoneExits.Count();
                Data.ItemsDisplayed += FilteredCheckedExits.Count();
                if (!FilteredCheckedExits.Any()) { continue; }
                foreach (var i in FilteredCheckedExits)
                {
                    i.Value.DisplayName = i.Value.GetEntranceDisplayName();
                    ValidExits.Add(i.Value);
                }
            }
            ValidExits = ValidExits.OrderBy(x => x.DisplayArea()).ThenBy(x => x.DisplayName).ToList();
            string CurrentArea = "";
            foreach (var i in ValidExits)
            {
                string ItemArea = $"{i.DisplayArea()} Exits";
                if (CurrentArea != ItemArea)
                {
                    CurrentArea = ItemArea;
                    if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                    Data.FinalData.Add(new MiscData.Areaheader { Area = CurrentArea });
                }
                Data.FinalData.Add(i);
            }
        }
        private static void WriteHiddenLocations(TrackerLocationDataList Data, IEnumerable<object> HiddenLocations)
        {
            List<object> TempDataSource = new List<object>();
            foreach (var obj in HiddenLocations)
            {
                var CurrentArea = "";
                if (obj is LocationData.LocationObject i)
                {
                    if (!i.AppearsinListbox(Data.ShowInvalidEntries)) { continue; }
                    i.DisplayName = Utility.GetLocationDisplayName(i, Data.InstanceContainer);
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, i, Data.Filter, i.DisplayName)) { continue; }
                    Data.ItemsDisplayed++;
                    CurrentArea = i.GetDictEntry().Area;
                }
                else if (obj is LocationData.LocationProxy p)
                {
                    if (!p.GetReferenceLocation().AppearsinListbox(Data.ShowInvalidEntries)) { continue; }
                    p.DisplayName = Utility.GetLocationDisplayName(p, Data.InstanceContainer);
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, p, Data.Filter, p.DisplayName)) { continue; }
                    Data.ItemsDisplayed++;
                    CurrentArea = p.Area;
                }
                else { continue; }
                TempDataSource.Add(obj);
            }
            if (TempDataSource.Any())
            {
                if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                Data.FinalData.Add(new MiscData.Areaheader { Area = "Hidden Locations" });
                Data.FinalData.AddRange(TempDataSource);
            }
        }
        private static void WriteLocations(TrackerLocationDataList Data, IEnumerable<object> AvailableLocationsEntries)
        {
            string CurrentLocation = "";
            foreach (var obj in AvailableLocationsEntries)
            {
                var CurrentArea = "";
                if (obj is LocationData.LocationObject i)
                {
                    if (!i.AppearsinListbox(Data.ShowInvalidEntries)) { continue; }
                    i.DisplayName = Utility.GetLocationDisplayName(i, Data.InstanceContainer);
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, i, Data.Filter, i.DisplayName)) { continue; }
                    Data.ItemsDisplayed++;
                    CurrentArea = i.GetDictEntry().Area;
                }
                else if (obj is LocationData.LocationProxy p)
                {
                    if (!p.GetReferenceLocation().AppearsinListbox(Data.ShowInvalidEntries)) { continue; }
                    p.DisplayName = Utility.GetLocationDisplayName(p, Data.InstanceContainer);
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, p, Data.Filter, p.DisplayName)) { continue; }
                    Data.ItemsDisplayed++;
                    CurrentArea = p.Area;
                }
                else { continue; }
                if (CurrentLocation != CurrentArea)
                {
                    if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                    Data.FinalData.Add(new MiscData.Areaheader { Area = CurrentArea });
                    CurrentLocation = CurrentArea;
                }
                Data.FinalData.Add(obj);
            }
        }
    }
}
