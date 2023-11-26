using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static string GetDictionaryPath()
        {
            return Path.Join(GetTestingFormDataPath(), "Recources", "Dictionaries");
        }
        public static string GetLogicPresetsPath()
        {
            return Path.Join(GetTestingFormDataPath(), "Recources", "Presets");
        }
        public static string GetTestingFormDataPath()
        {
            return Path.Join(GetDevCodePath(), "TestingForm");
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
