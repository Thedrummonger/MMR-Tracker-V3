using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames.OOTMMV2
{
    internal class OOTMMUtil
    {
        public static string GetGamecodeFromID(MMRData.JsonFormatLogicItem LogicItem)
        {
            if (LogicItem.Id.StartsWith("OOT_")) { return "OOT"; }
            if (LogicItem.Id.StartsWith("MM_")) { return "MM"; }
            var Segments = LogicItem.Id.Split(' ');
            return Segments[0];
        }

        public static bool LogicEntryHasGamecode(string LogicItem)
        {
            if (bool.TryParse(LogicItem, out _)) { return true; }
            if (LogicEditing.IsLogicFunction(LogicItem, out _, out _, new('(', ')'))) { return true; }
            if (LogicEditing.IsLogicFunction(LogicItem, out _, out _)) { return true; }
            if (LogicItem.StartsWith("OOT_")) { return true; }
            if (LogicItem.StartsWith("MM_")) { return true; }
            if (LogicItem.StartsWith("SHARED_")) { return true; }
            var Segments = LogicItem.Split(' ');
            if (Segments[0].Trim() == "OOT") { return true; }
            if (Segments[0].Trim() == "MM") { return true; }
            if (Segments[0].Trim() == "SHARED") { return true; }
            return false;
        }
    }
}
