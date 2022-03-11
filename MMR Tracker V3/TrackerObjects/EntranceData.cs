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
        //All Areas in the game and the exits they contain
        public Dictionary<string, EntranceRandoArea> AreaList { get; set; } = new Dictionary<string, EntranceRandoArea>();
        //A list of each area and how many times it has been made available when checking an exit
        public Dictionary<string, int> AreaAccessTable { get; set; } = new Dictionary<string, int>();
        //A table Mapping an Exit to its logic name
        public Dictionary<string, string> ExitLogicMap { get; set; } = new Dictionary<string, string>();
        //The area accessable from the beggining of the game
        public string RootArea { get; set; } = "root";
        public bool DecoupleEntrances { get; set; } = false;
        public bool IsEntranceRando { get; set; } = false;

        public string GetLogicName(EntranceRandoExit Exit)
        {
            return ExitLogicMap[$"{Exit.ParentAreaID}:{Exit.ID}"];
        }
        public void AddLogicReference(string LogicName, EntranceRandoExit Exit)
        {
            ExitLogicMap.Add($"{Exit.ParentAreaID}:{Exit.ID}", LogicName);
        }
        public bool CheckForRandomEntrances()
        {
            return AreaList.Any(x => x.Value.LoadingZoneExits.Any(x => x.Value.RandomizedState == RandomizedState.Randomized));
        }
    }
    public class EntranceRandoArea
    {
        public string ID { get; set; }
        public Dictionary<string, EntranceRandoExit> LoadingZoneExits { get; set; } = new Dictionary<string, EntranceRandoExit>();
        public Dictionary<string, EntranceRandoExit> MacroExits { get; set; } = new Dictionary<string, EntranceRandoExit>();
    }
    public class EntranceRandoExit
    {
        //MMR Example: "EntranceEastClockTownFromStockPotInnLower"
        //OOTR Example: "Kokiri Forest" (The parent area differentiates it from other exits with this name)
        public string ID { get; set; }
        public string ParentAreaID { get; set; }
        public bool RandomizableLoadingZone { get; set; } = false;
        public bool Available { get; set; } = false;
        public CheckState CheckState { get; set; } = CheckState.Unchecked;
        public RandomizedState RandomizedState { get; set; } = RandomizedState.Randomized;
        public string DestinationArea { get; set; }
        public string SpoilerDefinedDestinationArea { get; set; }
        public EntrancePair EntrancePair { get; set; }
    }
    public class EntrancePair
    {
        public string Area { get; set; }
        public string Exit { get; set; }
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
