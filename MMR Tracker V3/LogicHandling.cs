using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    class LogicHandling
    {
        public static InstanceState PopulateTrackerObject(string LogicFile, string DictionaryFile, LogicObjects.TrackerInstance Instance)
        {
            JsonConvert.DeserializeObject<MMRData.LogicFile>(LogicFile);

            return InstanceState.Success;
        }

        public enum InstanceState
        {
            Success,
            LogicFailure,
            DictionaryFailure,
            Formattingfailure
        }
    }
}
