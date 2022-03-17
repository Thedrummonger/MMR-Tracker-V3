using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
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

        public static DataSets PopulateDataSets(LogicObjects.TrackerInstance instance)
        {
            DataSets dataSets = new DataSets();

            dataSets.UncheckedLocations = instance.LocationPool.Values.Where(x => x.CheckState == MiscData.CheckState.Unchecked).ToList();
            dataSets.MarkedLocations = instance.LocationPool.Values.Where(x => x.CheckState == MiscData.CheckState.Marked).ToList();
            dataSets.CheckedLocations = instance.LocationPool.Values.Where(x => x.CheckState == MiscData.CheckState.Checked).ToList();
            dataSets.AllAvailableLocations = instance.LocationPool.Values.Where(x => x.CheckState != MiscData.CheckState.Checked).ToList();
            dataSets.AvailableLocations = dataSets.AllAvailableLocations.Where(x => x.Available || x.CheckState == MiscData.CheckState.Marked).ToList();

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
                WriteLocations(AvailableLocations);
            }
            else
            {
                WriteLocations(AvailableLocations);
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

            void WriteLocations(List<LocationData.LocationObject> AvailableLocations)
            {
                string CurrentLocation = "";
                foreach (var i in AvailableLocations)
                {
                    if (!LocationAppearsinListbox(i, Instance)) { continue; }

                    i.DisplayName = Utility.GetDisplayName(0, i, Instance);

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
