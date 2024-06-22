using static MMR_Tracker_V3.References;

namespace TestingForm
{
    internal class TestingReferences
    {
        public static string? GetDevTestingPath()
        {
            var DevINI = GetDevINI();
            if (DevINI == null) { return null; }
            return DevINI.TestingFolder;
        }
        public static string? GetDevCodePath()
        {
            var DevINI = GetDevINI();
            if (DevINI == null) { return null; }
            return DevINI.TrackerCodePath;
        }
        public static string GetTestingDictionaryPath()
        {
            return Path.Join(GetTestingFormDataPath(), "Recources", "Dictionaries");
        }
        public static string GetTestingLogicPresetsPath()
        {
            return Path.Join(GetTestingFormDataPath(), "Recources", "Presets");
        }
        public static string GetTestingFormDataPath()
        {
            return Path.Join(GetDevCodePath(), "TestingForm");
        }
        public static string GetTrackerLibraryDataPath()
        {
            return Path.Join(GetDevCodePath(), "MMR Tracker V3");
        }
        public static string GetLibraryDictionaryPath()
        {
            return Path.Join(GetTrackerLibraryDataPath(), "Recources", "Dictionaries");
        }
        public static string GetLibraryLogicPresetsPath()
        {
            return Path.Join(GetTrackerLibraryDataPath(), "Recources", "Presets");
        }
        public static string GetOtherGameDataPath(string GameFolder = "")
        {
            if (string.IsNullOrWhiteSpace(GameFolder)) { return Path.Join(GetTestingFormDataPath(), "GameDataCreation"); }
            return Path.Join(GetTestingFormDataPath(), "GameDataCreation", GameFolder);
        }
        public class DevINI
        {
            public string? TrackerCodePath { get; set; }
            public string? TestingFolder { get; set; }
        }
        public static DevINI? GetDevINI()
        {
            if (!File.Exists(Globalpaths.DevFile)) { return null; }
            DevINI DevFile = Newtonsoft.Json.JsonConvert.DeserializeObject<DevINI>(File.ReadAllText(Globalpaths.DevFile));
            return DevFile;
        }
    }
}
