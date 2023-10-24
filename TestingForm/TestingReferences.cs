using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingForm
{
    internal class TestingReferences
    {
        public static string GetDevTestingPath()
        {
            var DevINI = MMR_Tracker_V3.References.TestingPaths.GetDevINI();
            if (DevINI == null) { return null; }
            if (!DevINI.ContainsKey("TestingFolder")) { return null; }
            return DevINI["TestingFolder"];
        }
        public static string GetDevCodePath()
        {
            var DevINI = MMR_Tracker_V3.References.TestingPaths.GetDevINI();
            if (DevINI == null) { return null; }
            if (!DevINI.ContainsKey("TrackerCodePath")) { return null; }
            return DevINI["TrackerCodePath"];
        }
        public static string GetTrackerLibraryDataPath()
        {
            return Path.Join(GetDevCodePath(), "MMR Tracker V3");
        }
        public static string GetDictionaryPath()
        {
            return Path.Join(GetTrackerLibraryDataPath(), "Recources", "Dictionaries");
        }
        public static string GetLogicPresetsPath()
        {
            return Path.Join(GetTrackerLibraryDataPath(), "Recources", "Presets");
        }
        public static string GetTestingFormDataPath()
        {
            return Path.Join(GetDevCodePath(), "TestingForm");
        }
        public static string GetOtherGameDataPath(string GameFolder = "")
        {
            if (string.IsNullOrWhiteSpace(GameFolder)) { return Path.Join(GetTestingFormDataPath(), "OtherGames"); }
            return Path.Join(GetTestingFormDataPath(), "OtherGames", GameFolder);
        }
    }
}
