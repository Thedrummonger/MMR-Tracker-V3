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

            public List<HintData.HintObject> AvailableHints { get; set; } = new List<HintData.HintObject>();
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
            dataSets.AvailableLocations = dataSets.AllAvailableLocations.Where(x => x.Available).ToList();

            dataSets.UnheckedHints = instance.HintPool.Values.Where(x => x.CheckState == MiscData.CheckState.Unchecked).ToList();
            dataSets.MarkedHints = instance.HintPool.Values.Where(x => x.CheckState == MiscData.CheckState.Marked).ToList();
            dataSets.CheckedHints = instance.HintPool.Values.Where(x => x.CheckState == MiscData.CheckState.Checked).ToList();
            dataSets.AvailableHints = instance.HintPool.Values.Where(x => x.CheckState != MiscData.CheckState.Checked && x.Available).ToList();

            dataSets.Tricks = instance.MacroPool.Values.Where(x => x.isTrick(instance)).ToList();
            dataSets.AvailableStartingItems = instance.ItemPool.Values.Where(x => x.ValidStartingItem(instance)).ToList();

            dataSets.LocalObtainedItems = instance.ItemPool.Values.Where(x => x.AmountAquiredLocally > 0).ToList();
            dataSets.CurrentStartingItems = instance.ItemPool.Values.Where(x => x.AmountInStartingpool > 0).ToList();
            dataSets.OnlineObtainedItems = instance.ItemPool.Values.Where(x => x.AmountAquiredOnline.Any(x => x.Value > 0)).ToList();
            return dataSets;
        }

        public static List<object> PrintToCheckedList(Dictionary<string, int> Groups, DataSets DataSets, string Divider, LogicObjects.TrackerInstance Instance, string Filter, bool reverse = false)
        {
            if (Groups == null) { Groups = Utility.GetCategoriesFromFile(Instance); }
            if (DataSets == null) { DataSets = PopulateDataSets(Instance); }

            List<object> DataSource = new List<object>();

            var CheckedLocations = DataSets.CheckedLocations;
            CheckedLocations = CheckedLocations
                .OrderBy(x => (Groups.ContainsKey(x.GetDictEntry(Instance).Area.ToLower().Trim()) ? Groups[x.GetDictEntry(Instance).Area.ToLower().Trim()] : DataSets.CheckedLocations.Count + 1))
                .ThenBy(x => x.GetDictEntry(Instance).Area)
                .ThenBy(x => Utility.GetDisplayName(1, x, Instance)).ToList();

            if (reverse)
            {
                CheckedLocations.Reverse();
                WriteStartingAndOnlineItems(DataSets);
                WriteHints(DataSets);
                WriteLocations(CheckedLocations);
                return DataSource;
            }

            WriteLocations(CheckedLocations);
            WriteHints(DataSets);
            WriteStartingAndOnlineItems(DataSets);

            return DataSource;

            void WriteLocations(List<LocationData.LocationObject> CheckedLocations)
            {
                string CurrentLocation = "";
                foreach (var i in CheckedLocations)
                {
                    if (!LocationAppearsinListbox(i, Instance)) { continue; }

                    i.DisplayName = Utility.GetDisplayName(1, i, Instance);

                    if (!SearchStringParser.FilterSearch(Instance, i, Filter, i.DisplayName)) { continue; }

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
                if (DataSets.CheckedHints.Any())
                {
                    bool DividerCreated = false;
                    foreach (var i in DataSets.CheckedHints)
                    {
                        i.DisplayName = $"{i.GetDictEntry(Instance).Name}: {i.HintText}";
                        if (!SearchStringParser.FilterSearch(Instance, i, Filter, i.DisplayName)) { continue; }
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

            void WriteStartingAndOnlineItems(DataSets DataSets)
            {
                if (DataSets.CurrentStartingItems.Any())
                {
                    if (DataSource.Count > 0) { DataSource.Add(Divider); }
                    DataSource.Add(new MiscData.Areaheader { Area = "Starting Items" });
                    foreach (var i in DataSets.CurrentStartingItems)
                    {
                        string Display = $"{i.GetDictEntry(Instance).GetItemName(Instance)} X{i.AmountInStartingpool}";
                        if (!SearchStringParser.FilterSearch(Instance, i, Filter, Display)) { continue; }
                        DataSource.Add(Display);
                    }
                }

                if (DataSets.OnlineObtainedItems.Any())
                {
                    if (DataSource.Count > 0) { DataSource.Add(Divider); }
                    DataSource.Add(new MiscData.Areaheader { Area = "MultiWorld Items" });
                    foreach (var i in DataSets.OnlineObtainedItems)
                    {
                        foreach (var j in i.AmountAquiredOnline)
                        {
                            string Display = $"{i.GetDictEntry(Instance).GetItemName(Instance)} X{j.Value}: Player {j.Key}";
                            if (!SearchStringParser.FilterSearch(Instance, i, Filter, Display)) { continue; }
                            DataSource.Add(Display);
                        }
                    }
                }
            }
        }

        public static List<object> PrintToLocationList(Dictionary<string, int> Groups, DataSets DataSets, string Divider, LogicObjects.TrackerInstance Instance, string Filter, bool All, bool reverse = false)
        {
            List<object> DataSource = new List<object>();

            var AvailableLocations = DataSets.AvailableLocations;
            if (Filter.StartsWith("^") || All) { AvailableLocations = DataSets.AllAvailableLocations; }

            AvailableLocations = AvailableLocations
                .OrderBy(x => (Groups.ContainsKey(x.GetDictEntry(Instance).Area.ToLower().Trim()) ? Groups[x.GetDictEntry(Instance).Area.ToLower().Trim()] : DataSets.AvailableLocations.Count() + 1))
                .ThenBy(x => x.GetDictEntry(Instance).Area)
                .ThenBy(x => Utility.GetDisplayName(0, x, Instance)).ToList();

            if (reverse)
            {
                AvailableLocations.Reverse();
                WriteHints(DataSets);
                WriteLocations(AvailableLocations);
                return DataSource;
            }

            WriteLocations(AvailableLocations);
            WriteHints(DataSets);

            return DataSource;

            void WriteLocations(List<LocationData.LocationObject> AvailableLocations)
            {
                string CurrentLocation = "";
                foreach (var i in AvailableLocations)
                {
                    if (!LocationAppearsinListbox(i, Instance)) { continue; }

                    i.DisplayName = Utility.GetDisplayName(0, i, Instance);

                    if (!SearchStringParser.FilterSearch(Instance, i, Filter, i.DisplayName)) { continue; }

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
                var AvailableHints = DataSets.AvailableHints;
                if (Filter.StartsWith("^") || All)
                {
                    AvailableHints = DataSets.UnheckedHints;
                }
                if (AvailableHints.Any())
                {
                    bool DividerCreated = false;
                    foreach (var i in AvailableHints)
                    {
                        i.DisplayName = (i.CheckState == MiscData.CheckState.Marked) ? $"{i.GetDictEntry(Instance).Name}: {i.HintText}" : i.GetDictEntry(Instance).Name;
                        if (!SearchStringParser.FilterSearch(Instance, i, Filter, i.DisplayName)) { continue; }
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
        }

        private static bool LocationAppearsinListbox(LocationData.LocationObject Location, LogicObjects.TrackerInstance Instance)
        {
            return
                Location.RandomizedState != MiscData.RandomizedState.ForcedJunk &&
                Location.RandomizedState != MiscData.RandomizedState.Unrandomized &&
                !string.IsNullOrWhiteSpace(Location.GetDictEntry(Instance).Name);
        }
    }
}
