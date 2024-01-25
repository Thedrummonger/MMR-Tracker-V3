using MMR_Tracker_V3.TrackerObjectExtentions;
using System;
using System.Collections.Generic;
using System.Linq;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class EntranceData
    {
        public class EntrancePool(InstanceData.TrackerInstance Parent)
        {
            private InstanceData.TrackerInstance _parent = Parent;
            public InstanceData.TrackerInstance GetParent() { return _parent; }
            public void SetParent(InstanceData.TrackerInstance parent) { _parent = parent; }

            public Dictionary<string, EntranceData.EntranceRandoExit> ExitLookupByID { get; set; } = [];

            //All Areas in the game and the exits they contain
            public Dictionary<string, EntranceRandoArea> AreaList { get; set; } = [];
            //The area accessable from the beggining of the game
            public string RootArea { get; set; } = "Root";
            public bool IsEntranceRando { get; set; } = false;

            public bool CheckForRandomEntrances()
            {
                return AreaList.Any(x => x.Value.RandomizableExits().Any(x => x.Value.RandomizedState == RandomizedState.Randomized));
            }
            public int GetAmountOfRandomizedEntrances()
            {
                return AreaList.SelectMany(x => x.Value.RandomizableExits().Where(y => y.Value.IsRandomized())).Count();
            }
        }
        public class EntranceRandoArea(InstanceData.TrackerInstance Parent)
        {
            private InstanceData.TrackerInstance _parent = Parent;
            public InstanceData.TrackerInstance GetParent() { return _parent; }
            public void SetParent(InstanceData.TrackerInstance parent) { _parent = parent; }

            public string ID { get; set; }
            public int ExitsAcessibleFrom { get; set; } = 0;
            public Dictionary<string, EntranceRandoExit> Exits { get; set; } = [];
            public EntranceRandoExit GetExit(string ID)
            {
                if (Exits.TryGetValue(ID, out EntranceRandoExit value)) { return value; }
                return null;
            }
            public Dictionary<string, EntranceRandoExit> RandomizableExits()
            {
                return Exits.Where(x => x.Value.IsRandomizableEntrance()).ToDictionary(x => x.Key, v => v.Value);
            }
            public Dictionary<string, EntranceRandoExit> NonRandomizableExits()
            {
                return Exits.Where(x => !x.Value.IsRandomizableEntrance()).ToDictionary(x => x.Key, v => v.Value);
            }
        }
        public class EntranceRandoExit(InstanceData.TrackerInstance Parent, EntranceRandoArea ParentArea) : CheckableLocation(Parent)
        {
            private EntranceRandoArea _parentArea = ParentArea;
            public EntranceRandoArea GetParentArea() { return _parentArea; }
            public void SetParent(EntranceRandoArea parent)
            {
                SetParent(parent.GetParent());
                _parentArea = parent;
            }
            public string ExitID { get; set; }
            public bool IsWarp { get; set; } = false;
            public EntranceRandoDestination DestinationExit { get; set; }
            public EntranceRandoDestination SpoilerDefinedDestinationExit { get; set; }
            public EntranceAreaPair EntrancePair { get; set; }

            public LogicDictionaryData.DictionaryEntranceEntries GetDictEntry()
            {
                return GetParent().LogicDictionary.EntranceList[ID];
            }

            public override string ToString()
            {
                return DisplayName ?? ExitID;
            }
            public string GetStringID()
            {
                return $"{GetParentArea().ID} X {this.ExitID}";
            }
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
