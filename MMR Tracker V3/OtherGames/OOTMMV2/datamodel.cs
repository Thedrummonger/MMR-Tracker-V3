using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames.OOTMMV2
{
    internal class datamodel
    {
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
            public string location;
            public string type;
            public string hint;
            public string scene;
            public string id;
            public string item;
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
    }
}
