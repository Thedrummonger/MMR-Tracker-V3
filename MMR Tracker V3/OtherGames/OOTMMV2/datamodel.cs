using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames.OOTMMV2
{
    public class datamodel
    {
        public enum HintType
        {
            ItemExact,
            ItemRegion,
            Hero,
            Foolish,
            none
        }
        public class SpoilerHintData
        {
            public HintType HintType { get; set; }
            public string PrettyLocationText { get; set; }
            public string[] HintedLocations { get; set; }
            public string[] HintedItemNames { get; set; }
            public string[] HintedItems { get; set; }
        }
        public class AreaConnections
        {
            public string Area { get; set; }
            public string Exit { get; set; }

        }

        public class OOTMMSetting
        {
            public string key { get; set; }
            public string name { get; set; }
            public string category { get; set; }
            public string type { get; set; }
            public string description { get; set; }
            public OOTMMSettingValue[] values { get; set;}
            [JsonProperty(PropertyName = "default")]
            public dynamic defaultvalue { get; set;}
        }

        public class OOTMMSettingValue
        {
            public string value { get; set; }
            public string name { get; set; }
            public string description { get; set; }
        }
        public class MMROOTLocation
        {
            public string location { get; set; }
            public string type { get; set; }
            public string hint { get; set; }
            public string scene { get; set; }
            public string id { get; set; }
            public string item { get; set; }
        }

        public class MMROOTLogicEntry
        {
            public string dungeon;
            public string boss;
            public string region;
            public Dictionary<string, string> exits = new Dictionary<string, string>();
            public Dictionary<string, string> locations = new Dictionary<string, string>();
            public Dictionary<string, string> events = new Dictionary<string, string>();
            public Dictionary<string, string> gossip = new Dictionary<string, string>();
        }
        public class MMROOTEntranceData
        {
            public string to;
            public string from;
            public string type;
            public string id;
            public string reverse;
        }
        public class MMROOTHintData
        {
            public string location;
            public string type;
            public string id;
        }

        public class OOTMMParserData
        {
            //Data
            public Dictionary<string, AreaConnections> AreaConnections { get; set; }
            public Dictionary<string, string> LocationAreas { get; set; }
            public List<string> DungeonLayouts { get; set; } = new List<string>();

            //CodePaths
            public string MMRTrackerCodePath { get; set; }
            public string OOTMMV2CodeFolder { get; set; }
            public string SettingsFile { get; set; }
            public string TricksFile { get; set; }
            public string ItemsFile { get; set; }
            public string ItemNamesFile { get; set; }
            public string RegionNamesFile { get; set; } 

            //Shared Data
            public string OOTMMCorePath { get; set; }
            public string OOTMMTestingFolder { get; set; }

            //OOT Data Paths
            public string OOTData { get; set; }
            public string OOTWorld { get; set; }
            public string OOTMQWorld { get; set; }

            //OOT Data Files
            public string OOTEntrancesFile { get; set; }
            public string OOTMacroFile { get; set; }
            public string OOTPoolFile { get; set; }
            public string OOTHintFile { get; set; }

            //MM Data
            public string MMData { get; set; }
            public string MMWorld { get; set; }

            //MM Data Files
            public string MMEntrancesFile { get; set; }
            public string MMMacroFile { get; set; }
            public string MMPoolFile { get; set; }
            public string MMHintFile { get; set; }

            //Shared Data Files
            public string SHAREDMacroFile { get; set; }
        }
    }
}
