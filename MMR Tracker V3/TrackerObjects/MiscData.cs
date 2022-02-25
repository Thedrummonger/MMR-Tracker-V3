using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.ItemData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class MiscData
    {
        public class ProgressiveItemData
        {
            public List<ItemObject> ItemSet;
            public int AmountNeeded;
            public bool IsProgressive()
            {
                return ItemSet != null && ItemSet.Any();
            }
        }

        public enum CheckState
        {
            Checked = 0,
            Marked = 1,
            Unchecked = 2
        }

        public enum RandomizedState
        {
            Randomized = 0,
            Unrandomized = 1,
            UnrandomizedManual = 2,
            ForcedJunk = 3
        }

        public enum KeyType
        {
            None = 0,
            Small = 1,
            Boss = 2
        }

        public enum MiddleClickFunction
        {
            star = 0,
            set = 1
        }
        public enum TimeOfDay
        {
            None,
            Day1 = 1,
            Night1 = 2,
            Day2 = 4,
            Night2 = 8,
            Day3 = 16,
            Night3 = 32,
        }
    }
}
