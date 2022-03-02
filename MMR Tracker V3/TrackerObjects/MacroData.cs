using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.MMRData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class MacroData
    {
        public List<MacroObject> MacroList { get; set; } = new List<MacroObject>();

    }
    public class MacroObject
    {
        public JsonFormatLogicItem LogicData { get; set; } = new JsonFormatLogicItem();
        public bool Aquired { get; set; } = false;
        public dynamicLogicData DynamicLogic { get; set; } = null;

        private bool _trickEnabled = true;
        public bool TrickEnabled
        {
            get { return LogicData.IsTrick ? _trickEnabled : true; }
            set { _trickEnabled = value; }
        }
    }

    public class dynamicLogicData
    {
        //If "LocationToCompare" has the randomized Item "ItemAtLocation" Use the logic of Macro "LogicToUse"
        public string LocationToCompare { get; set; }
        public List<DynamicLogicArguments> Arguments { get; set; } = new List<DynamicLogicArguments>();

}
    public class DynamicLogicArguments
    {
        public string ItemAtLocation { get; set; }
        public string LogicToUse { get; set; }
    }
}
