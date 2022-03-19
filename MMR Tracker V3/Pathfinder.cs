using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public class Pathfinder
    {
        public Dictionary<string, string> FinalPath = null;
        public bool FindPath(
            LogicObjects.TrackerInstance instance, 
            string CurrentArea, 
            string Goal, 
            List<string> SeenAreas,
            Dictionary<string, string> Path, 
            bool Root = true
        )
        {
            SeenAreas.Add(CurrentArea);
            var validExits = instance.EntrancePool.AreaList[CurrentArea].LoadingZoneExits.Values.Where(x => x.Available && !SeenAreas.Contains(x.ParentAreaID));
            if (Root) { validExits = validExits.Concat(instance.EntrancePool.AreaList.SelectMany(x => x.Value.LoadingZoneExits.Values.Where(x => x.IsWarp))); }
            if (validExits.Any(x => x.ID == Goal))
            {
                Path.Add(CurrentArea, validExits.First(x => x.ID == Goal).ID);
                FinalPath = Path;
                return true;
            }
            foreach (var i in validExits)
            {
                var PathCopy = Path.ToDictionary(entry => entry.Key, entry => entry.Value);
                var SeenAreasCopy = SeenAreas.Select(entry => entry);
                PathCopy.Add(CurrentArea, i.ID);
                var PathFound = FindPath(instance, i.ID, Goal, SeenAreasCopy.ToList(), PathCopy, false);
                if (PathFound) { return true; }
            }
            return false;
        }
    }
}
