using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class ObtainableObject(InstanceData.TrackerInstance Parent)
    {
        private InstanceData.TrackerInstance _parent = Parent;
        public InstanceData.TrackerInstance GetParent() { return _parent; }
        public void SetParent(InstanceData.TrackerInstance parent) { _parent = parent; }

        public string ID { get; set; }
        public int AmountAquiredLocally { get; set; } = 0;
        public int AmountInStartingpool { get; set; } = 0;
        public Dictionary<int, int> AmountAquiredOnline { get; set; } = [];
        public Dictionary<int, int> AmountSentToPlayer { get; set; } = [];
        public string DisplayName { get; set; }
        public InstanceData.ReferenceData referenceData { get; set; } = new InstanceData.ReferenceData();
    }
}
