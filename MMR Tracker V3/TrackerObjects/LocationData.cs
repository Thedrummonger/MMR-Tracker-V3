using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.MMRData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class LocationData
    {
        public class LocationPool
        {
            public List<LocationObject> Locations { get; set; } = new List<LocationObject>();
        }
        public class LocationObject
        {
            public JsonFormatLogicItem LogicData { get; set; } = new JsonFormatLogicItem();
            public LocationFrontendData UIData { get; set; } = new LocationFrontendData();
            public LocationObjectData TrackerData { get; set; } = new LocationObjectData();
            public override string ToString()
            {
                return UIData.DisplayName ?? LogicData.Id;
            }

        }

        public class LocationObjectData
        {
            public bool Available { get; set; } = false;
            public CheckState CheckState { get; set; } = CheckState.Unchecked;
            public int ItemBelongedToPlayer { get; set; } = -1;
            public string VanillaItem { get; set; } = null;
            public string RandomizedItem { get; set; } = null;
            public string SpoilerLogGivenItem { get; set; } = null;
            public string AlteredItem { get; set; } = null; //The Item that was altered when this was checked. null if the item is not checked
            public RandomizedState RandomizedState { get; set; } = RandomizedState.Randomized;
            public string[] ValidItemTypes { get; set; } = Array.Empty<string>();
        }

        public class LocationFrontendData
        {
            public string LocationName { get; set; }
            public bool Starred { get; set; }
            public string LocationArea { get; set; }
            public string DisplayName { get; set; }
        }
    }
}
