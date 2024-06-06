using MMR_Tracker_V3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingForm.GameDataCreation.OOTMMV3
{
    public class OOTMMDataClasses
    {
        public class ExtraData
        {
            public string[] items;
            public string[] mm_masks;
            public string[] mm_masks_transformation;
            public string[] oot_masks;
            public string[] stones;
            public string[] medallions;
            public string[] remains;
            public string[] renewablelocations;
            public string[] nonrenewablelocations;
            public string[] renewabletypes;
            public Dictionary<string, string> names;
            public Dictionary<string, string> dungeons;
            public Dictionary<string, OOTMMTrickData> tricks;
            public OOTMMSetting[] options;
        }
        public class OOTMMTrickData
        {
            public string game;
            public string name;
            public string tooltip;
            public string linkText;
            public bool? glitch;
        }
        public class OOTMMSetting
        {
            public string key;
            public string name;
            public string category;
            public string type;
            public string description;
            public OOTMMSettingValue[] values;
            [JsonProperty(PropertyName = "default")]
            public dynamic defaultvalue;
            public int? min;
            public int? max;
        }
        public class OOTMMSettingValue
        {
            public string value;
            public string name;
            public string description;
        }
        public class OOTMMPoolLocation
        {
            public string location;
            public string type;
            public string hint;
            public string scene;
            public string id;
            public string item;
        }
        public class OOTMMEntranceFileData
        {
            public string game;
            public string id;
            public string type;
            public string[] areas;
            public string reverse;
        }
        public enum OOTMMDataType
        {
            location,
            Exit,
            Event
        }
        public class OOTMMLogicFunction
        {
            public OOTMMLogicFunction(string func, string ParamString, string LogicLine = "")
            {
                function = func;
                RawParam = ParamString;
                Param = OOTMMUtility.SplitParams(ParamString).Select(x => x.Trim()).ToArray();
                Logic = LogicLine;
            }
            public string function;
            public string[] Param;
            public string RawParam;
            public string Logic;
        }
        public class OOTMMLocationArea
        {
            public bool boss = false;
            public bool age_change = false;
            public string dungeon;
            public string region;
            public Dictionary<string, string> exits = [];
            public Dictionary<string, string> events = [];
            public Dictionary<string, string> locations = [];
            public Dictionary<string, string> gossip = [];
        }
        public class OOTMMLocationEntry
        {
            public string Key;
            public string Logic;
        }
        public class OOTMMEntranceEntry
        {
            public string Key;
            public string Logic;
        }
        public class OOTMMMacroEntry
        {
            public string Key;
            public string Logic;
            public string GameCode;
        }
    }
}
