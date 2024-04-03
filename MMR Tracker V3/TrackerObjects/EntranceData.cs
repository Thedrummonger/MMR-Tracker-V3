using MathNet.Numerics;
using MMR_Tracker_V3.TrackerObjectExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class EntranceData
    {
        public class EntranceRandoArea(InstanceData.TrackerInstance Parent) : ObtainableObject(Parent)
        {
            public bool IsRoot = false;
            /// <summary>
            /// Key = The Exits ID, Value = The exits Logic ID
            /// </summary>
            public Dictionary<string, string> Exits = [];

            public EntranceRandoExit GetExitFromExitID(string ExitID)
            {
                if (!Exits.TryGetValue(ExitID, out string ExitLogicID)) { return null; }
                if (!GetParent().ExitPool.TryGetValue(ExitLogicID, out EntranceRandoExit ExitObj)) { return null; }
                return ExitObj;
            }
            public Dictionary<string, EntranceRandoExit> GetAllExits()
            {
                return Exits.ToDictionary(x => x.Value, x => GetParent().ExitPool[x.Value]);
            }
            public Dictionary<string, EntranceRandoExit> GetAllRandomizableExits()
            {
                return GetAllExits().Where(x => x.Value.IsRandomizableEntrance()).ToDictionary();
            }
            public Dictionary<string, EntranceRandoExit> GetAllMacroExits()
            {
                return GetAllExits().Where(x => !x.Value.IsRandomizableEntrance()).ToDictionary();
            }
        }
        public class EntranceRandoExit(InstanceData.TrackerInstance ParentInstance) : CheckableLocation(ParentInstance) 
        {
            public string ParentAreaID { get; set; }
            public string ExitID { get; set; }
            public bool IsWarp { get; set; } = false;
            public EntranceRandoDestination DestinationExit { get; set; }
            public EntranceRandoDestination SpoilerDefinedDestinationExit { get; set; }
            public EntranceAreaPair EntrancePair { get; set; }

            public LogicDictionaryData.DictionaryEntranceEntries GetDictEntry()
            {
                return GetParent().LogicDictionary.EntranceList[ID];
            }
            public EntranceRandoArea GetParentArea()
            {
                return GetParent().AreaPool[ParentAreaID];
            }

            public override string ToString()
            {
                return DisplayName ?? ExitID;
            }
            public string GetStringID()
            {
                return $"{GetParentArea().ID} X {this.ExitID}";
            }

            public override string GetName()
            {
                return GetDictEntry().DisplayExit ?? GetDictEntry().Exit;
            }

            public override LogicDictionaryData.DictionaryCheckableLocationEntry GetAbstractDictEntry() => GetDictEntry();

            public override CheckableLocationTypes LocationType() => CheckableLocationTypes.Exit;
        }
        public class EntranceRandoDestination
        {
            public string region { get; set; }
            public string from { get; set; }
            public override string ToString()
            {
                return $"{region} <= {from}";
            }
        }
        public class EntranceAreaPair
        {
            public string Area { get; set; }
            public string Exit { get; set; }
            public string GetStringID()
            {
                return $"{Area} X {Exit}";
            }
        }
    }

    /* With MMRs current entrance rando system it will be handled as follows
     * Each entrance (Example "EntranceClockTowerRooftopFromSouthClockTown") Will be both an area and an exit
     * as an area, it will have no exits.
     * as an exit, it will be in root area
     * This will mean when the entrance is referred to in logic it will look to see if the area version of that entrance is available
     * 
     * For path finder, I will use the method from the old tracker of finding entrance connections to populate exits into each area.
     * Essentially mark all areas unobtained and exits unavailable and then one by one mark each area as obtained see what exits become available.
     * The ones that become available will be added to that area as an exit.
     * This will then allow pathfinder to be used normally.
     */
}
