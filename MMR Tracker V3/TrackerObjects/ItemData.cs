using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class ItemData
    {
        public class ItemPool
        {
            public ItemObject[] CurrentPool { get; set; }
        }

        public class ItemObject
        {
            public string Id { get; set; }
            public string ItemName { get; set; }
            public string[] AltItemNames { get; set; }

            public int AmountAquiredLocally { get; set; } = 0;
            public int AmountInStartingpool { get; set; } = 0;
            public Dictionary<int, int> AmountAquiredOnline { get; set; } = new Dictionary<int, int>();
            public Dictionary<int, int> AmountSentToPlayer { get; set; } = new Dictionary<int, int>();

            public string[] ItemTypes { get; set; } = Array.Empty<string>();

            public int MaxAmountInPool { get; set; } = -1;

            public string DisplayName { get; set; }

            public override string ToString()
            {
                return DisplayName ?? ItemName ?? Id;
            }
        }
    }
}
