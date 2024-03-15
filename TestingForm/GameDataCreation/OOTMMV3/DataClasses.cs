using MMR_Tracker_V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingForm.GameDataCreation.OOTMMV3
{
    internal class DataClasses
    {
        public enum OOTMMDataType
        {
            location,
            Exit,
            Macro
        }
        public class OOTMMLogicFunction
        {
            public OOTMMLogicFunction(string func, string ParamString, string LogicLine = "")
            {
                function = func;
                Param = ParamString.TrimSplit(",");
                Logic = LogicLine;
            }
            public string function;
            public string[] Param;
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
