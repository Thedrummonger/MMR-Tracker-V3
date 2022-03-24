using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public static class References
    {
        public static string trackerVersion = "V0.3";
        public static int TrackerVersionStatus = 0;

        public static String CurrentSavePath = "";

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
        }
        public static class WindowsPaths
        {
            public static readonly string BaseAppdataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MMRTracker");
            public static readonly string OptionFile = Path.Combine(BaseAppdataPath, "options.txt");
        }
    }
}
