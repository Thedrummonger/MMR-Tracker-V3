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
            public bool IsMacro { get; set; } = false;
            public ItemObject VanillaItem { get; set; }
            public RandomizedState RandomizedState { get; set; } = RandomizedState.Randomized;
            public ItemObject RandomizedItem { get; set; }

            public void ToggleChecked(CheckState State)
            {
                CheckState CurrentState = this.CheckState;
                this.CheckState = State;
                ItemObject ItemAtLocation;
                if (RandomizedState == RandomizedState.UnrandomizedManual || RandomizedState == RandomizedState.Unrandomized)
                {
                    ItemAtLocation = VanillaItem;
                }
                else if (RandomizedItem != null)
                {
                    ItemAtLocation = RandomizedItem;
                }
                else { return; }

                int IncrementAction = 0;
                if ((CurrentState == CheckState.Marked || CurrentState == CheckState.Unchecked) && State == CheckState.Checked) { IncrementAction = 1; }
                else if ((State == CheckState.Marked || State == CheckState.Unchecked) && CurrentState == CheckState.Checked) { IncrementAction = -1; }

                if (ItemBelongedToPlayer != -1)
                {
                    if (!ItemAtLocation.AmountSentToPlayer.ContainsKey(ItemBelongedToPlayer)) { ItemAtLocation.AmountSentToPlayer.Add(ItemBelongedToPlayer,0); }
                    ItemAtLocation.AmountSentToPlayer[ItemBelongedToPlayer] += IncrementAction;
                }
                else
                {
                    ItemAtLocation.AmountAquiredInGame += IncrementAction;
                }
            }
        }

        public class LocationFrontendData
        {
            public string LocationName { get; set; }
            public string JunkItemType { get; set; }
            public bool Starred { get; set; }
            public string LocationArea { get; set; }
            public string DisplayName { get; set; }
            public bool RandomizedItemRevealed { get; set; } = false;
        }
    }
}
