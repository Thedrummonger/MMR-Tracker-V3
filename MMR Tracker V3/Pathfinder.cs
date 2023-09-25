using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

        public void FindPath(LogicObjects.TrackerInstance instance, string Start, string Goal, List<Dictionary<string, string>> Paths = null, int RunCount = 1)
        {

            bool IsRoot = false;
            if (Paths == null) 
            {
                IsRoot = true;
                BuildEntranceMap(instance);
                Paths = new List<Dictionary<string, string>>
                {
                    new Dictionary<string, string> { { Start, "" } }
                };
            }

            List<Dictionary<string, string>> NewPaths = new List<Dictionary<string, string>>();
            foreach (var Path in Paths)
            {
                var PathEnd = Path.Last();
                var ValidExits = EntranceMap[PathEnd.Key];
                if (IsRoot)
                {
                    foreach (var warp in Warps)
                    {
                        if (!ValidExits.ContainsKey(warp.Key)) { ValidExits.Add(warp.Key, warp.Value); }
                    }
                }
                foreach (var i in ValidExits)
                {
                    if (Path.ContainsKey(i.Value.region)) { continue; }
                    Dictionary<string, string> NewPath = new Dictionary<string, string>();
                    foreach(var j in Path)
                    {
                        NewPath.Add(j.Key, j.Value);
                    }
                    NewPath.Add(i.Value.region, i.Key);
                    NewPaths.Add(NewPath);
                }
            }


            if (NewPaths.Any(x => x.Last().Key == Goal))
            {
                var GoalPaths = NewPaths.Where(x => x.Last().Key == Goal);
                foreach(var GoalPath in GoalPaths)
                {
                    Dictionary<string, string> FormattedPath = new Dictionary<string, string>();
                    int NextInd = 1;
                    foreach(var stop in GoalPath)
                    {
                        string Area = stop.Key;
                        string Exit = NextInd >= GoalPath.Count ? "" : GoalPath[GoalPath.Keys.ToArray()[NextInd]];
                        FormattedPath.Add(Area, Exit);
                        NextInd++;
                    }
                    FinalPath.Add(FormattedPath);
                }

                NewPaths = NewPaths.Where(x => x.Last().Key != Goal).ToList();
            }

            //Debug.WriteLine($"Layer {RunCount}\nPaths to Check: {NewPaths.Count}");

            if (NewPaths.Any() && NewPaths.Count < 100000 && FinalPath.Count < 20) { FindPath(instance, Start, Goal, NewPaths, RunCount: RunCount + 1); }

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
                    if (j.Value.IsWarp) { Warps[j.Key] = j.Value.DestinationExit; }
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
