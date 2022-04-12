using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames
{
    class PMRTools
    {
        class PaperMarioLogicJSON
        {
            public PMRArea from;
            public PMRArea to;
            public List<Tuple<string, string>> reqs;
            public List<string> pseudoitems;
        }
        class PMRArea
        {
            public string map;
            public dynamic id;
        }

        public static void ReadLogicJson()
        {

        }
    }
}
