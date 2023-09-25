using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public class Pathfinder
    {
        public List<Dictionary<string, string>> FinalPath = new List<Dictionary<string, string>>();

        public Dictionary<string, Dictionary<string, EntranceData.EntranceRandoDestination>> EntranceMap = new Dictionary<string, Dictionary<string, EntranceData.EntranceRandoDestination>>();
        public Dictionary<string, EntranceData.EntranceRandoDestination> Warps = new Dictionary<string, EntranceData.EntranceRandoDestination>();

        public bool FindPath(
            LogicObjects.TrackerInstance instance,
            string CurrentArea,
            string Goal,
            List<string> SeenAreas,
            Dictionary<string, string> Path,
            bool Root = true,
            bool IncludeMacroExits = false,
            bool StopAtFirstPath = false
        )
        {
            if (Root) 
            { 
                FinalPath.Clear();
                BuildEntranceMap(instance);
            }
            SeenAreas.Add(CurrentArea);
            var validExits = EntranceMap[CurrentArea];

            if (Root)
            {
                foreach(var ex in Warps ) 
                {
                    if (!validExits.ContainsKey(ex.Key))
                    {
                        validExits[ex.Key] = ex.Value;
                    }
                }
            }

            var FoundGoal = validExits.Values.Where(x => x.region == Goal);

            if (FoundGoal.Any())
            {
                foreach(var goal in FoundGoal)
                {
                    Path.Add(Goal, "");
                    FinalPath.Add(Path);
                }
                return true;
            }
            foreach (var i in validExits)
            {
                if (SeenAreas.Contains(i.Value.region)) { continue; }
                var PathCopy = Path.ToDictionary(entry => entry.Key, entry => entry.Value);
                var SeenAreasCopy = SeenAreas.Select(entry => entry);
                PathCopy.Add(CurrentArea, i.Key);
                var PathFound = FindPath(instance, i.Value.region, Goal, SeenAreasCopy.ToList(), PathCopy, false, IncludeMacroExits, StopAtFirstPath);
                if (PathFound) { return (StopAtFirstPath || FinalPath.Count() > 100); }
            }
            return false;
        }

        private void BuildEntranceMap(LogicObjects.TrackerInstance instance)
        {
            EntranceMap.Clear();
            Warps.Clear();

            foreach (var i in instance.EntrancePool.AreaList.Values)
            {
                if (!EntranceMap.ContainsKey(i.ID)) { EntranceMap[i.ID] = new Dictionary<string, EntranceData.EntranceRandoDestination>(); }
                foreach(var j in i.Exits)
                {
                    if (j.Value.CheckState != MiscData.CheckState.Checked) { continue; }
                    if (j.Value.IsWarp) { Warps[i.ID] = j.Value.DestinationExit; }
                    EntranceMap[i.ID][j.Key] = j.Value.DestinationExit;
                }
            }
        }

        public class PathfinderPath
        {
            public int Index { get; set; }
            public string Display { get; set; }
            public bool Focused { get; set; } = false;
            public override string ToString()
            {
                return Display;
            }
        }
    }
}
