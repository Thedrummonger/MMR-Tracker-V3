using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingForm.GameDataCreation.LinksAwakeningSwitch
{
    internal class Data
    {
        public class ExpandedLogicEntry
        {
            public string Name;
            public string type;
            public string region;
            public string LogicBasic;
            public string LogicAdvanced;
            public string LogicGlitched;
            public string LogicHell;
            public List<List<string>> Logic;
        }
        public class LASItemPool
        {
            public Dictionary<string, Dictionary<string, object>> Item_Pool;
            public List<string> Starting_Items;
        }
    }
}
