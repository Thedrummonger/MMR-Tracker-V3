using MathNet.Symbolics;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace MMR_Tracker_V3
{
    internal class CategoryFileHandling
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
                Debug.WriteLine("Reading Headers from Dictionary");
                return Instance.LogicDictionary.AreaOrder.Distinct().Select((s, ind) => new { s, ind }).ToDictionary(x => x.s.ToLower(), x => x.ind);
            }
            else if (File.Exists(References.Globalpaths.HeaderSortingFile))
            {
                Debug.WriteLine("Reading Headers from Header File");
                HeaderSortingFile[] headerSortingFile = [];
                try { headerSortingFile = TDMUtils.Utility.DeserializeYAMLFile<HeaderSortingFile[]>(References.Globalpaths.HeaderSortingFile); }
                catch { Debug.WriteLine("Header File Could Not Be parsed"); return []; }
                var ValidHeaders = headerSortingFile.Where(x => x.Games is null || x.Games.Count == 0 || x.Games.Contains(Instance.LogicDictionary.GameCode));
                var SortOrder = ValidHeaders.SelectMany(x => x.Headers).Distinct().Select((s, ind) => new { s, ind }).ToDictionary(x => x.s.ToLower(), x => x.ind);
                return SortOrder;
            }
            else
            {
                Debug.WriteLine("No header order found"); 
                return []; 
            }
        }
    }
}
