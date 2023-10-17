using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class EntranceData
    {
        public class EntrancePool
        {
            //All Areas in the game and the exits they contain
            public Dictionary<string, EntranceRandoArea> AreaList { get; set; } = new Dictionary<string, EntranceRandoArea>();
            //The area accessable from the beggining of the game
            public string RootArea { get; set; } = "Root";
            public bool IsEntranceRando { get; set; } = false;

            public EntranceRandoExit GetEntrancePairOfDestination(EntranceRandoDestination destination)
            {
                var DestinationAsExit = AreaList[destination.from].GetExit(destination.region);
                var EntrancePair = DestinationAsExit.EntrancePair;
                if (EntrancePair == null) { return null; }
                return AreaList[EntrancePair.Area].GetExit(EntrancePair.Exit);
            }
            public bool CheckForRandomEntrances(InstanceData.TrackerInstance Instance)
            {
                return AreaList.Any(x => x.Value.RandomizableExits(Instance).Any(x => x.Value.RandomizedState == RandomizedState.Randomized));
            }
            public int GetAmountOfRandomizedEntrances(InstanceData.TrackerInstance Instance)
            {
                return AreaList.SelectMany(x => x.Value.RandomizableExits(Instance).Where(y => y.Value.IsRandomized())).Count();
            }
            public bool EntranceIsValid(string Area, string Exit, bool Macros, out bool AreaValid, out bool ExitValid)
            {
                AreaValid = AreaList.ContainsKey(Area); 
                ExitValid = AreaValid && AreaList[Area].Exits.ContainsKey(Exit);
                return AreaValid && ExitValid;
            }
        }
        public class EntranceRandoArea
        {
            public string ID { get; set; }
            public int ExitsAcessibleFrom { get; set; } = 0;
            public Dictionary<string, EntranceRandoExit> Exits { get; set; } = new Dictionary<string, EntranceRandoExit>();
            public EntranceRandoExit GetExit(string ID)
            {
                if (Exits.ContainsKey(ID)) { return Exits[ID]; }
                return null;
            }
            public Dictionary<string, EntranceRandoExit> RandomizableExits(InstanceData.TrackerInstance Instance)
            {
                return Exits.Where(x => x.Value.IsRandomizableEntrance(Instance)).ToDictionary(x => x.Key, v => v.Value);
            }
            public Dictionary<string, EntranceRandoExit> NonRandomizableExits(InstanceData.TrackerInstance Instance)
            {
                return Exits.Where(x => !x.Value.IsRandomizableEntrance(Instance)).ToDictionary(x => x.Key, v => v.Value);
            }
        }
        public class EntranceRandoExit
        {
            public string ParentAreaID { get; set; }
            public string ID { get; set; }
            public bool Available { get; set; } = false;
            public bool Starred { get; set; } = false;
            public bool IsWarp { get; set; } = false;
            public CheckState CheckState { get; set; } = CheckState.Unchecked;
            public RandomizedState RandomizedState { get; set; } = RandomizedState.Randomized;
            public EntranceRandoDestination DestinationExit { get; set; }
            public EntranceRandoDestination SpoilerDefinedDestinationExit { get; set; }
            public EntranceAreaPair EntrancePair { get; set; }
            public string DisplayName { get; set; }
            public InstanceData.ReferenceData referenceData { get; set; } = new InstanceData.ReferenceData();

            public string DisplayArea(InstanceData.TrackerInstance Instance)
            {
                return GetDictEntry(Instance)?.DisplayArea??ParentAreaID;
            }
            public string DisplayExit(InstanceData.TrackerInstance Instance)
            {
                return GetDictEntry(Instance)?.DisplayExit??ID;
            }

            public override string ToString()
            {
                return DisplayName ?? ID;
            }

            public bool IsRandomizableEntrance(InstanceData.TrackerInstance currentTrackerInstance)
            {
                return GetDictEntry(currentTrackerInstance).RandomizableEntrance;
            }

            public LogicDictionaryData.DictionaryEntranceEntries GetDictEntry(InstanceData.TrackerInstance Instance)
            {
                return Instance.LogicDictionary.EntranceList[Instance.GetLogicNameFromExit(this)];
            }

            public EntranceRandoDestination GetVanillaDestination()
            {
                return new EntranceRandoDestination { region = ID, from = ParentAreaID };
            }
            public EntranceRandoDestination GetDestnationFromEntrancePair()
            {
                return new EntranceRandoDestination { region = EntrancePair.Exit, from = EntrancePair.Area };
            }
            public bool IsUnrandomized(UnrandState Include = UnrandState.Any)
            {
                if ((Include == UnrandState.Any || Include == UnrandState.Unrand) && RandomizedState == RandomizedState.Unrandomized) { return true; }
                if ((Include == UnrandState.Any || Include == UnrandState.Manual) && RandomizedState == RandomizedState.UnrandomizedManual) { return true; }
                return false;
            }
            public bool IsRandomized()
            {
                return RandomizedState == RandomizedState.Randomized;
            }
            public bool IsJunk()
            {
                return RandomizedState == RandomizedState.ForcedJunk;
            }

            public EntranceRandoDestination GetDestinationAtExit(InstanceData.TrackerInstance currentTrackerInstance)
            {
                var DestinationAtCheck = DestinationExit;
                if (SpoilerDefinedDestinationExit != null)
                {
                    DestinationAtCheck = SpoilerDefinedDestinationExit;
                }
                if ((IsUnrandomized()))
                {
                    DestinationAtCheck = new EntranceRandoDestination { region = ID, from = ParentAreaID };
                }
                return DestinationAtCheck;
            }

            public bool ToggleExitChecked(CheckState NewState, InstanceData.TrackerInstance Instance)
            {
                CheckState CurrentState = CheckState;
                if (CurrentState == NewState)
                {
                    return false;
                }
                else if (CurrentState == CheckState.Checked)
                {
                    var Destination = Instance.EntrancePool.AreaList[DestinationExit.region];
                    Destination.ExitsAcessibleFrom--;
                }
                else if (NewState == CheckState.Checked)
                {
                    if (DestinationExit == null) { return false; }
                    var Destination = Instance.EntrancePool.AreaList[DestinationExit.region];
                    Destination.ExitsAcessibleFrom++;
                }
                else if (CurrentState == CheckState.Unchecked && NewState == CheckState.Marked)
                {
                    if (DestinationExit == null) { return false; }
                }

                if (NewState == CheckState.Unchecked) { DestinationExit = null; }
                CheckState = NewState;
                return true;
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
        }
    }

    /* With MMRs current entrance rando system it will be handled as follows
     * Each entrance (Example "EntranceClockTowerRooftopFromSouthClockTown") Will be both an area and an exit
     * as an area, it will have no exits.
     * as an exit, it will be in root area
     * This will mean when the entrance is reffered to in logic it will look to see if the area version of that entrance is avalable
     * 
     * For path finder, I will use the method from the old tracker of finding entrance connections to populate exits into each area.
     * Essentially mark all areas unobtained and exits unavalable and then one by one mark each area as obtained see what exits become available.
     * The ones that become available will be added to that area as an exit.
     * This will then allow pathfinder to be used normally.
     */
}
