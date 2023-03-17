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
        public static LogicDictionaryData.LogicDictionary FindValidDictionary(MMRData.LogicFile logicFile, string DictFolderPath)
        {
            return FindValidDictionary(logicFile, DictFolderPath, out _, out _, out _);
        }
        public static LogicDictionaryData.LogicDictionary FindValidDictionary(MMRData.LogicFile logicFile, string DictFolderPath, out string DictPath, out string DictType, out int VersionOffset)
        {
            if (string.IsNullOrWhiteSpace(DictFolderPath)) { DictFolderPath = References.Globalpaths.BaseDictionaryPath; }
            LogicDictionaryData.LogicDictionary currentdictionaryObj = null;
            DictPath = "";
            DictType = "";
            VersionOffset = 0;
            int Versionoffset = -1;
            foreach (var i in Directory.GetFiles(DictFolderPath, "*", SearchOption.AllDirectories).ToArray())
            {
                LogicDictionaryData.LogicDictionary LogicDic = null;
                string CurType = "Error";
                string FileText = File.ReadAllText(i);
                try { LogicDic = JsonConvert.DeserializeObject<LogicDictionaryData.LogicDictionary>(FileText); CurType = "Json"; } catch { }
                try { LogicDic = JsonConvert.DeserializeObject<LogicDictionaryData.LogicDictionary>(Utility.ConvertYamlStringToJsonString(FileText)); CurType = "Yaml"; } catch { }
                if (LogicDic is not null && logicFile.GameCode == LogicDic.GameCode)
                {
                    int offset = Math.Abs(logicFile.Version - LogicDic.LogicVersion);
                    if (Versionoffset == -1 || Versionoffset > offset)
                    {
                        DictType = CurType;
                        DictPath = i;
                        VersionOffset = offset;
                        currentdictionaryObj = LogicDic;
                    }
                }

            }
            return currentdictionaryObj;
        }
    }
}
