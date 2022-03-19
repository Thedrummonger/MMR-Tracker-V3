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
        public bool FindPath(
            LogicObjects.TrackerInstance instance, 
            string CurrentArea, 
            string Goal, 
            List<string> SeenAreas,
            Dictionary<string, string> Path, 
            bool Root = true
        )
        {
            if (Root) { FinalPath.Clear(); }
            SeenAreas.Add(CurrentArea);
            var validLoadingZoneExits = instance.EntrancePool.AreaList[CurrentArea].LoadingZoneExits.Values.Where(x => x.CheckState == MiscData.CheckState.Checked);
            var validMacroExits = instance.EntrancePool.AreaList[CurrentArea].MacroExits.Values.Where(x => x.CheckState == MiscData.CheckState.Checked);
            var WarpExits = instance.EntrancePool.AreaList.SelectMany(x => x.Value.LoadingZoneExits.Values.Where(x => x.IsWarp && x.CheckState == MiscData.CheckState.Checked));
            var validExits = validLoadingZoneExits.Concat(validMacroExits);
            if (Root) { validExits = validExits.Concat(WarpExits); }
            if (validExits.Any(x => x.DestinationExit.region == Goal))
            {
                var DestinationExit = validExits.First(x => x.DestinationExit.region == Goal);
                if (!validMacroExits.Contains(DestinationExit) || instance.StaticOptions.ShowMacroExitsPathfinder)
                {
                    if (WarpExits.Contains(DestinationExit)) { Path.Add(DestinationExit.ParentAreaID, DestinationExit.ID); }
                    else { Path.Add(CurrentArea, DestinationExit.ID); }
                }
                Path.Add(Goal, "");
                FinalPath.Add(Path);
                return true;
            }
            foreach (var i in validExits)
            {
                if (SeenAreas.Contains(i.DestinationExit.region)) { continue; }
                var PathCopy = Path.ToDictionary(entry => entry.Key, entry => entry.Value);
                var SeenAreasCopy = SeenAreas.Select(entry => entry);
                if (!validMacroExits.Contains(i) || instance.StaticOptions.ShowMacroExitsPathfinder) 
                { 
                    if (WarpExits.Contains(i)) { PathCopy.Add(i.ParentAreaID, i.ID); }
                    else { PathCopy.Add(CurrentArea, i.ID); }
                }
                var PathFound = FindPath(instance, i.DestinationExit.region, Goal, SeenAreasCopy.ToList(), PathCopy, false);
                if (PathFound) { return false; }
            }
            return false;
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
