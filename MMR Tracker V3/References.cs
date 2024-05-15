using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MMR_Tracker_V3
{
    public static class References
    {
        public static Version trackerVersion = new(2, 0, 0);
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

            public static readonly string BaseDictionaryPath = Path.Combine(RecourcesFolder, "Dictionaries");
            public static readonly string HeaderSortingFile = Path.Combine(RecourcesFolder, "Headers.yaml");
            public static readonly string WebPresets = Path.Combine(PresetFolder, "WebPresets.txt");

            public static readonly string BaseAppdataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MMRTracker");
            public static readonly string OptionFile = Path.Combine(BaseAppdataPath, "options.txt");
            public static readonly string DevFile = Path.Combine(BaseAppdataPath, "devpc.ini");
            public static readonly string UserData = Path.Combine(BaseAppdataPath, "UserData.ini");
        }
    }
}
