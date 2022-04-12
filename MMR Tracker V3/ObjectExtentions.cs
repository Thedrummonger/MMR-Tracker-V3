using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.LogicObjects;
using static MMR_Tracker_V3.TrackerObjects.EntranceData;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public static class ObjectExtentions
    {

        public static bool IsLiteralID(this string ID, out string CleanedID)
        {
            bool Literal = false;
            CleanedID = ID;
            if (ID.StartsWith("'") && ID.EndsWith("'"))
            {
                Literal = true;
                CleanedID = ID.Replace("'", "");
            }
            return Literal;
        }
        public static string GetLogicNameFromExit(this LogicObjects.TrackerInstance instance, EntranceRandoExit Exit)
        {
            return instance.InstanceReference.ExitLogicMap[$"{Exit.ParentAreaID} X {Exit.ID}"];
        }
        public static string GetLogicNameFromExit(this LogicObjects.TrackerInstance instance, EntranceAreaPair Exit)
        {
            return instance.InstanceReference.ExitLogicMap[$"{Exit.Area} X {Exit.Exit}"];
        }
        public static string GetLogicNameFromExit(this LogicObjects.TrackerInstance instance, EntranceRandoDestination Exit)
        {
            return instance.InstanceReference.ExitLogicMap[$"{Exit.region} X {Exit.from}"];
        }
        public static void AddLogicExitReference(this LogicObjects.TrackerInstance instance, EntranceAreaPair Exit, string LogicName)
        {
            instance.InstanceReference.ExitLogicMap.Add($"{Exit.Area} X {Exit.Exit}", LogicName);
        }
    }
}
