using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class HintData
    {
        public class HintPool
        {
            public List<HintObject> Hints { get; set; } = new List<HintObject>();
        }

        public class HintObject
        {
            public MMRData.JsonFormatLogicItem LogicData { get; set; } = new MMRData.JsonFormatLogicItem();
            public CheckState CheckState { get; set; } = CheckState.Unchecked;
            public bool Available { get; set; }
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string HintText { get; set; }
            public string SpoilerHintText { get; set; }
            public override string ToString()
            {
                return DisplayName ?? Name ?? LogicData.Id;
            }
        }
    }
}
