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
            dataSets.AvailableLocations = instance.LocationPool.Values.Where(x => x.CheckState != MiscData.CheckState.Checked && x.Available).ToList();

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

        public static List<object> PrintToCheckedList(Dictionary<string, int> Groups, TrackerDataHandeling.DataSets DataSets, string Divider, LogicObjects.TrackerInstance Instance, string Filter)
        {
            List<object> DataSource = new List<object>();

            var CheckedLocations = DataSets.CheckedLocations;
            CheckedLocations = CheckedLocations
                .OrderBy(x => (Groups.ContainsKey(x.GetDictEntry(Instance).Area.ToLower().Trim()) ? Groups[x.GetDictEntry(Instance).Area.ToLower().Trim()] : DataSets.CheckedLocations.Count() + 1))
                .ThenBy(x => x.GetDictEntry(Instance).Area)
                .ThenBy(x => Utility.GetDisplayName(1, x, Instance)).ToList();

            WriteLocations(CheckedLocations);
            WriteHints(DataSets);
            WriteStartingAndOnlineItems(DataSets);

            return DataSource;

            void WriteLocations(List<LocationData.LocationObject> CheckedLocations)
            {
                string CurrentLocation = "";
                foreach (var i in CheckedLocations)
                {
                    if (i.RandomizedState == MiscData.RandomizedState.Unrandomized) { continue; }

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

            void WriteHints(TrackerDataHandeling.DataSets DataSets)
            {
                if (DataSets.CheckedHints.Any())
                {
                    bool DividerCreated = false;
                    foreach (var i in DataSets.CheckedHints)
                    {
                        i.DisplayName = $"{i.GetDictEntry(Instance).Name}: {i.HintText}";
                        if (!i.DisplayName.ToLower().Contains(Filter.ToLower())) { continue; }
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

            void WriteStartingAndOnlineItems(TrackerDataHandeling.DataSets DataSets)
            {
                if (DataSets.CurrentStartingItems.Any())
                {
                    if (DataSource.Count > 0) { DataSource.Add(Divider); }
                    DataSource.Add(new MiscData.Areaheader { Area = "Starting Items" });
                    foreach (var i in DataSets.CurrentStartingItems)
                    {
                        DataSource.Add($"{i.GetDictEntry(Instance).GetItemName(Instance)} X{i.AmountInStartingpool}");
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
                            DataSource.Add($"{i.GetDictEntry(Instance).GetItemName(Instance)} X{j.Value}: Player {j.Key}");
                        }
                    }
                }
            }
        }

        public static List<object> PrintToLocationList(Dictionary<string, int> Groups, TrackerDataHandeling.DataSets DataSets, string Divider, LogicObjects.TrackerInstance Instance, string Filter, bool All)
        {
            List<object> DataSource = new List<object>();

            var AvailableLocations = DataSets.AvailableLocations;
            if (Filter.StartsWith("^") || All) { AvailableLocations = DataSets.UncheckedLocations; }

            AvailableLocations = AvailableLocations.Concat(DataSets.MarkedLocations)
                .OrderBy(x => (Groups.ContainsKey(x.GetDictEntry(Instance).Area.ToLower().Trim()) ? Groups[x.GetDictEntry(Instance).Area.ToLower().Trim()] : DataSets.AvailableLocations.Count() + 1))
                .ThenBy(x => x.GetDictEntry(Instance).Area)
                .ThenBy(x => Utility.GetDisplayName(0, x, Instance)).ToList();

            WriteLocations(AvailableLocations);
            WriteHints(DataSets);

            return DataSource;

            void WriteLocations(List<LocationData.LocationObject> AvailableLocations)
            {
                string CurrentLocation = "";
                foreach (var i in AvailableLocations)
                {
                    if (i.RandomizedState == MiscData.RandomizedState.ForcedJunk) { continue; }

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

            void WriteHints(TrackerDataHandeling.DataSets DataSets)
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
                        if (!i.DisplayName.ToLower().Contains(Filter.ToLower())) { continue; }
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
    }
}
