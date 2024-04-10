using MathNet.Numerics;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MMR_Tracker_V3
{
    public class Pathfinder
    {
        public List<Dictionary<string, string>> FinalPath = new List<Dictionary<string, string>>();

        public Dictionary<string, Dictionary<string, EntranceData.EntranceRandoDestination>> EntranceMap = new Dictionary<string, Dictionary<string, EntranceData.EntranceRandoDestination>>();
        public Dictionary<string, EntranceData.EntranceRandoDestination> Warps = new Dictionary<string, EntranceData.EntranceRandoDestination>();
        public Dictionary<string, int> SeenAreas = new Dictionary<string, int>();

        public bool Overloaded = false;

        public int PathlistCap = 500000;

        public void FindPath(InstanceData.TrackerInstance instance, string Start, string Goal, List<Dictionary<string, string>> Paths = null, int RunCount = 1)
        {
            bool ShowMacro = instance.StaticOptions.OptionFile.ShowMacroExitsPathfinder;
            bool IsRoot = false;
            if (Paths == null)
            {
                Overloaded = false;
                IsRoot = true;
                SeenAreas.Clear();
                BuildEntranceMap(instance);
                Paths = [new Dictionary<string, string> { { Start, "" } }];
            }

            if (!instance.StaticOptions.OptionFile.ShowRedundantPathfinder)
            {
                foreach (var i in Paths.Select(x => x.Last().Key))
                {
                    if (!SeenAreas.ContainsKey(i)) { SeenAreas.Add(i, RunCount); }
                }
            }

            Dictionary<string, List<Dictionary<string, string>>> FoundAreas = new Dictionary<string, List<Dictionary<string, string>>>();

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
                    if (i.Value is null) { continue; }
                    if (SeenAreas.ContainsKey(i.Value.region)) { continue; }
                    if (Path.ContainsKey(i.Value.region)) { continue; }
                    Dictionary<string, string> NewPath = new(Path);
                    NewPath[NewPath.Last().Key] = i.Key;
                    NewPath.Add(i.Value.region, "");
                    if (!FoundAreas.ContainsKey(i.Value.region)) { FoundAreas[i.Value.region] = new List<Dictionary<string, string>>(); }
                    FoundAreas[i.Value.region].Add(NewPath);
                }
            }

            if (FoundAreas.ContainsKey(Goal))
            {
                foreach (var GoalPath in FoundAreas[Goal])
                {
                    Dictionary<string, string> FormattedPath = [];
                    int Index = 1;
                    foreach (var stop in GoalPath)
                    {
                        string Area = stop.Key;
                        string Exit = stop.Value;

                        var ExitObject = instance.GetExitByAreaIDAndExitID(Area, Exit);

                        bool ExitValid = ExitObject is not null;
                        bool IsDestination = string.IsNullOrWhiteSpace(Exit);
                        bool IsRandomizedExit = ExitValid &&
                            ExitObject.IsRandomizableEntrance() && 
                            (ExitObject.IsRandomized() || ExitObject.IsUnrandomized(MiscData.UnrandState.Manual));

                        if (IsRandomizedExit || IsDestination || ShowMacro) { FormattedPath.Add(Area, Exit); }
                        Index++;
                    }
                    FinalPath.Add(FormattedPath);
                }
                FoundAreas.Remove(Goal);
            }

            List<Dictionary<string, string>> NewPaths = FoundAreas.Values.SelectMany(x => x).ToList();

            //Debug.WriteLine($"Layer {RunCount}\nPaths to Check: {NewPaths.Count}");

            if (NewPaths.Any() && (NewPaths.Count < PathlistCap || !instance.StaticOptions.OptionFile.ShowRedundantPathfinder) && FinalPath.Count < 20) { FindPath(instance, Start, Goal, NewPaths, RunCount: RunCount + 1); }
            else if (NewPaths.Count >= PathlistCap) { Overloaded = true; }
        }

        private void BuildEntranceMap(InstanceData.TrackerInstance instance)
        {
            EntranceMap.Clear();
            Warps.Clear();

            foreach (var i in instance.AreaPool.Values)
            {
                if (!EntranceMap.ContainsKey(i.ID)) { EntranceMap[i.ID] = new Dictionary<string, EntranceData.EntranceRandoDestination>(); }
                foreach (var j in i.GetAllExits())
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

        public static string[] GetValidStartingAreas(InstanceData.TrackerInstance Instance)
        {
            HashSet<string> result = [];
            bool CanMacro = Instance.StaticOptions.OptionFile.ShowMacroExitsPathfinder;
            foreach (var i in Instance.AreaPool)
            {
                var ExitsToCheck = CanMacro ? i.Value.GetAllExits().Values : i.Value.GetAllRandomizableExits().Values;
                if (!ExitsToCheck.Any(x => x.CheckState == MiscData.CheckState.Checked)) { continue; }
                result.Add(i.Key);
            }
            return [.. result];
        }
        public static string[] GetValidDestinationAreas(InstanceData.TrackerInstance Instance)
        {
            HashSet<string> result = [];
            bool CanMacro = Instance.StaticOptions.OptionFile.ShowMacroExitsPathfinder;
            var AllExits = Instance.ExitPool.Values;
            var ExitsToCheck = CanMacro ? AllExits : Instance.ExitPool.Where(x => x.Value.IsRandomizableEntrance()).Select(x => x.Value);
            foreach (var i in ExitsToCheck)
            {
                if (i.CheckState != MiscData.CheckState.Checked) { continue; }
                if (i.DestinationExit == null) { continue; }
                result.Add(i.DestinationExit.region);
            }
            return [.. result];
        }
    }
}
