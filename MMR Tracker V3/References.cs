using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public static class References
    {
        public static string trackerVersion = "V1.3.0";
        public static UpdateManager.TrackerVersionStatus TrackerVersionStatus = new UpdateManager.TrackerVersionStatus();

        public static OSPlatform? CurrentOS = 
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OSPlatform.Windows : 
            (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OSPlatform.OSX :
            (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux : 
            (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) ? OSPlatform.FreeBSD : null)));

        public static class Globalpaths
        {
            public static readonly string BaseProgramPath = AppDomain.CurrentDomain.BaseDirectory;

            public static readonly string RecourcesFolder = Path.Combine(BaseProgramPath, "Recources");
            public static readonly string PresetFolder = Path.Combine(RecourcesFolder, "Presets");
            public static readonly string OtherFilesFolder = Path.Combine(RecourcesFolder, "Presets");

            public static readonly string BaseDictionaryPath = Path.Combine(RecourcesFolder, "Dictionaries");
            public static readonly string CategoryTextFile = Path.Combine(RecourcesFolder, "Categories.txt");
            public static readonly string BaseLogicPresetPath = Path.Combine(OtherFilesFolder, "Custom Logic Presets");
            public static readonly string BaseOtherGameLogic = Path.Combine(OtherFilesFolder, "Other Game Premade Logic");
            public static readonly string WebPresets = Path.Combine(PresetFolder, "WebPresets.txt");

            public static readonly string BaseAppdataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MMRTracker");
            public static readonly string OptionFile = Path.Combine(BaseAppdataPath, "options.txt");
            public static readonly string DevFile = Path.Combine(BaseAppdataPath, "devpc.ini");
            public static readonly string UserData = Path.Combine(BaseAppdataPath, "UserData.ini");
        }
        public static class TestingPaths
        {
            public static Dictionary<string, string> GetDevINI() 
            { 
                if (!File.Exists(Globalpaths.DevFile)) { return null; }
                Dictionary<string, string> DevFile = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Globalpaths.DevFile));
                return DevFile;
            }
            public static string GetDevTestingPath()
            {
                var DevINI = GetDevINI();
                if (DevINI == null) { return null; }
                if (!DevINI.ContainsKey("TestingFolder")) { return null; }
                return DevINI["TestingFolder"];
            }
            public static string GetDevCodePath()
            {
                var DevINI = GetDevINI();
                if (DevINI == null) { return null; }
                if (!DevINI.ContainsKey("TrackerCodePath")) { return null; }
                return DevINI["TrackerCodePath"];
            }
        }
    }
}
