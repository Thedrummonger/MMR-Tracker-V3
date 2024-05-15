using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MMR_Tracker_V3
{
    public class LogicPresetHandeling
    {
        public class WebPreset
        {
            public string Name;
            public string Logic;
            public string Dictionary = null;
        }
        public class PresetlogicData
        {
            public string Name { get; set; } = null;
            public string DictionaryString { get; set; } = null;
            public string LogicString { get; set; } = null;
            public override string ToString()
            {
                return Name;
            }
        }
        public static List<PresetlogicData> GetLogicPresets()
        {
            List<PresetlogicData> Entries = new List<PresetlogicData>();
            if (File.Exists(References.Globalpaths.WebPresets))
            {
                WebPreset[] webPresets;
                try { webPresets = Utility.DeserializeYAMLFile<WebPreset[]>(References.Globalpaths.WebPresets); }
                catch { webPresets = []; }
                System.Net.WebClient wc = new System.Net.WebClient();

                foreach (var i in webPresets)
                {
                    if (string.IsNullOrWhiteSpace(i.Name) || string.IsNullOrWhiteSpace(i.Logic)) { continue; }
                    PresetlogicData PresetEntry = new() { Name = i.Name };
                    try { PresetEntry.LogicString = wc.DownloadString(i.Logic.Trim()); }
                    catch { continue; }
                    if (!string.IsNullOrWhiteSpace(i.Dictionary))
                    {
                        try { PresetEntry.DictionaryString = wc.DownloadString(i.Dictionary.Trim()); }
                        catch { continue; }
                    }
                    Entries.Add(PresetEntry);
                }
            }
            foreach (var i in Directory.GetFiles(References.Globalpaths.PresetFolder).Where(x => x != References.Globalpaths.WebPresets))
            {
                var PresetEntry = new PresetlogicData
                {
                    Name = Path.GetFileNameWithoutExtension(i),
                    LogicString = File.ReadAllText(i)
                };
                Entries.Add(PresetEntry);
            }
            return Entries;
        }
    }
}
