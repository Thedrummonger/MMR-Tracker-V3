using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public class LogicPresetHandeling
    {
        public class PresetlogicData
        {
            public string Name { get; set; } = null;
            public string DictionaryString { get; set; } = null;
            public string LogicString { get; set; } = null;
        }
        public static List<PresetlogicData> GetLogicPresets()
        {
            PresetlogicData PresetEntry = new PresetlogicData();
            List<PresetlogicData> Entries = new List<PresetlogicData>();
            if (File.Exists(References.Globalpaths.WebPresets))
            {
                System.Net.WebClient wc = new System.Net.WebClient();
                bool ErrorEntry = false;
                foreach (var i in File.ReadAllLines(References.Globalpaths.WebPresets))
                {
                    if (i.StartsWith("Name:"))
                    {
                        ErrorEntry = false;
                        PresetEntry = new PresetlogicData();
                        PresetEntry.Name = Regex.Replace(i, "Name:", "", RegexOptions.IgnoreCase).Trim();
                    }
                    if (i.StartsWith("Dictionary:") && !ErrorEntry)
                    {
                        try { PresetEntry.DictionaryString = wc.DownloadString(Regex.Replace(i, "Dictionary:", "", RegexOptions.IgnoreCase).Trim()); }
                        catch { ErrorEntry = true; }
                    }
                    if (i.StartsWith("Address:") && !ErrorEntry)
                    {
                        try
                        {
                            PresetEntry.LogicString = wc.DownloadString(Regex.Replace(i, "Address:", "", RegexOptions.IgnoreCase).Trim());
                            Entries.Add(PresetEntry);
                        }
                        catch { ErrorEntry = true; }
                    }
                }
            }
            foreach (var i in Directory.GetFiles(References.Globalpaths.PresetFolder).Where(x => x != References.Globalpaths.WebPresets))
            {
                PresetEntry = new PresetlogicData();
                PresetEntry.Name = Path.GetFileNameWithoutExtension(i);
                if (PresetEntry.Name.ToLower().StartsWith("dev-") && !Testing.Debugging()) { continue; }
                PresetEntry.LogicString = File.ReadAllText(i);
                Entries.Add(PresetEntry);
            }
            return Entries;
        }
    }
}
