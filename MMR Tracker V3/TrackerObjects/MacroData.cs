using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MMRData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class MacroData
    {
        public List<MacroObject> Macros { get; set; } = new List<MacroObject>();
    }
    public class MacroObject
    {
        public JsonFormatLogicItem LogicData { get; set; } = new JsonFormatLogicItem();

    }
}
