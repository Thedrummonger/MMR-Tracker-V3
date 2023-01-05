using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames.OOTR
{
    public class OOTRObjects
    {
        public class LogicalRegion
        {
            public string region_name { get; set; }
            public string dungeon { get; set; }
            public string hint { get; set; }
            public Dictionary<string, string> events { get; set; }
            public Dictionary<string, string> locations { get; set; }
            public Dictionary<string, string> exits { get; set; }
        }

        public class LogicFile
        {
            public string FilePath { get; set; }
            public List<LogicalRegion> LogicalRegions { get; set; }
            public bool IsMQ { get { return FilePath.EndsWith(" MQ.json"); } }
            public string FileName { get { return System.IO.Path.GetFileNameWithoutExtension(FilePath); } }
        }

        public class trickdata
        {
            public string name { get; set; }
            public string[] tags { get; set; }
            public string tooltip { get; set; }
        }
    }
}
