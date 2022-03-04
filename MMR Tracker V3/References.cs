using System;
using System.Collections.Generic;
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
            public static readonly string BaseDictionaryPath = BaseProgramPath + @"Recources\Dictionaries";
            public static readonly string BaseLogicPresetPath = BaseProgramPath + @"Recources\Other Files\Custom Logic Presets";
            public static readonly string BaseOtherGameLogic = BaseProgramPath + @"Recources\Other Files\Other Game Premade Logic";
            public static readonly string CategoryTextFile = BaseProgramPath + @"Recources\Categories.txt";
        }
        public static class WindowsPaths
        {
            public static readonly string BaseAppdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\MMRTracker";
            public static readonly string OptionFile = BaseAppdataPath + @"\options.txt";
        }
    }
}
