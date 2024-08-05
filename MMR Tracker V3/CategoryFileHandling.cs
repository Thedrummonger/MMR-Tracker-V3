using MMR_Tracker_V3.TrackerObjects;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TDMUtils;

namespace MMR_Tracker_V3
{
    public class CategoryFileHandling
    {
        public class HeaderSortingFile
        {
            public HashSet<string> Games = [];
            public HashSet<string> Headers = [];
        }
        public static Dictionary<string, int> GetCategoriesFromFile(InstanceData.TrackerInstance Instance)
        {
            if (Instance.LogicDictionary.AreaOrder is not null && Instance.LogicDictionary.AreaOrder.Length > 0)
            {
                return Instance.LogicDictionary.AreaOrder.Distinct().Select((s, ind) => new { s, ind }).ToDictionary(x => x.s.ToLower(), x => x.ind);
            }
            else if (File.Exists(References.Globalpaths.HeaderSortingFile))
            {
                HeaderSortingFile[] headerSortingFile = [];
                try { headerSortingFile = DataFileUtilities.DeserializeYAMLFile<HeaderSortingFile[]>(References.Globalpaths.HeaderSortingFile); }
                catch { Debug.WriteLine("Header File Could Not Be parsed"); return []; }
                var ValidHeaders = headerSortingFile.Where(x => x.Games is null || x.Games.Count == 0 || x.Games.Contains(Instance.LogicDictionary.GameCode));
                var SortOrder = ValidHeaders.SelectMany(x => x.Headers).Distinct().Select((s, ind) => new { s, ind }).ToDictionary(x => x.s.ToLower(), x => x.ind);
                return SortOrder;
            }
            else
            {
                return []; 
            }
        }
    }
}
