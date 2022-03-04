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

            dataSets.UncheckedLocations = instance.LocationPool.Locations.Where(x => x.TrackerData.CheckState == MiscData.CheckState.Unchecked).ToList();
            dataSets.MarkedLocations = instance.LocationPool.Locations.Where(x => x.TrackerData.CheckState == MiscData.CheckState.Marked).ToList();
            dataSets.CheckedLocations = instance.LocationPool.Locations.Where(x => x.TrackerData.CheckState == MiscData.CheckState.Checked).ToList();
            dataSets.AvailableLocations = instance.LocationPool.Locations.Where(x => x.TrackerData.CheckState != MiscData.CheckState.Checked && x.TrackerData.Available).ToList();

            dataSets.UnheckedHints = instance.HintPool.Hints.Where(x => x.CheckState == MiscData.CheckState.Unchecked).ToList();
            dataSets.MarkedHints = instance.HintPool.Hints.Where(x => x.CheckState == MiscData.CheckState.Marked).ToList();
            dataSets.CheckedHints = instance.HintPool.Hints.Where(x => x.CheckState == MiscData.CheckState.Checked).ToList();
            dataSets.AvailableHints = instance.HintPool.Hints.Where(x => x.CheckState != MiscData.CheckState.Checked && x.Available).ToList();

            dataSets.Tricks = instance.Macros.MacroList.Where(x => x.LogicData.IsTrick).ToList();
            dataSets.AvailableStartingItems = instance.ItemPool.CurrentPool.Where(x => x.CanBeStartingItem).ToList();

            dataSets.LocalObtainedItems = instance.ItemPool.CurrentPool.Where(x => x.AmountAquiredLocally > 0).ToList();
            dataSets.CurrentStartingItems = instance.ItemPool.CurrentPool.Where(x => x.AmountInStartingpool > 0).ToList();
            dataSets.OnlineObtainedItems = instance.ItemPool.CurrentPool.Where(x => x.AmountAquiredOnline.Any(x => x.Value > 0)).ToList();
            return dataSets;
        }
    }
}
