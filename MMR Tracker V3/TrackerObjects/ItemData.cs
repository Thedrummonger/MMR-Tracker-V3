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
            private string[] CurrentPool;
            public string[] Pool { get { return CurrentPool; } }
            public void PolulateItemPool(string[] ItemPool)
            {
                CurrentPool = ItemPool;
            }
        }

        public class ItemObject
        {
            public string Id { get; set; }
            public string ItemName { get; set; }
            public List<string> AltItemNames { get; set; } = new List<string>();

            public int AmountAquiredInGame { get; set; } = 0;
            public int AmountAquiredOnline { get; set; } = 0;
            public int AmountInStartingpool { get; set; } = 0;
            public Dictionary<int, int> AmountSentToPlayer { get; set; } = new Dictionary<int, int>();

            public ProgressiveItemData ProgressiveItemData { get; set; } = new ProgressiveItemData();

            public int GetTotalUsable()
            {
                return AmountAquiredInGame + AmountAquiredOnline + AmountInStartingpool;
            }

            public bool GetItemByString(string item)
            {
                return (item == Id || item == ItemName || AltItemNames.Contains(item));
            }

            public bool Useable(int Amount = 1)
            {
                if (ProgressiveItemData != null && ProgressiveItemData.IsProgressive())
                {
                    int ItemsAquired = 0;
                    foreach(var i in ProgressiveItemData.ItemSet)
                    {
                        ItemsAquired += i.GetTotalUsable();
                    }
                    return ProgressiveItemData.AmountNeeded >= ItemsAquired;
                }
                else
                {
                    return this.GetTotalUsable() >= Amount;
                }
            }

        }
    }
}
