using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public class Dictionaryhandeling
    {
        public static string GetJSONDictionaryPathForLogicFile(MMRData.LogicFile logicFile)
        {
            string currentdictionary = "";
            int Versionoffset = -1;
            foreach (var i in Directory.GetFiles(References.Globalpaths.BaseDictionaryPath, "*", SearchOption.AllDirectories).ToArray())
            {
                LogicDictionaryData.LogicDictionary LogicDic = new LogicDictionaryData.LogicDictionary();
                try
                {
                    LogicDic = JsonConvert.DeserializeObject<LogicDictionaryData.LogicDictionary>(File.ReadAllText(i));
                    if (logicFile.GameCode == LogicDic.GameCode)
                    {
                        int offset = Math.Abs(logicFile.Version - LogicDic.LogicVersion);
                        if (Versionoffset == -1 || Versionoffset > offset)
                        {
                            currentdictionary = i;
                            Versionoffset = offset;
                        }
                    }
                }
                catch { continue; }
            }
            Debug.WriteLine("Json Dictionary " + currentdictionary);
            Debug.WriteLine($"Dictionary was {Versionoffset} versions off");
            return currentdictionary;
        }
    }
}
