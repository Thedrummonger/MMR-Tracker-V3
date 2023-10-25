using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System.Diagnostics;
using static MMR_Tracker_V3.References;

namespace TestingForm
{
    internal class Utility
    {
        public static string CreateTestingFile(string Name, string Extention = "txt")
        {
            return Path.Combine(TestingReferences.GetDevTestingPath(), $"{Name}.{Extention}");
        }

        public static void CreateTestingFile(string Name, object Data, string Extention = "txt")
        {
            File.WriteAllText(CreateTestingFile(Name, Extention), JsonConvert.SerializeObject(Data, MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
        }

        public static void TestLogicForInvalidItems(MiscData.InstanceContainer Container)
        {
            foreach (var i in Container.Instance.LogicFile.Logic)
            {
                var logicitems = i.ConditionalItems.SelectMany(x => x).Concat(i.RequiredItems).ToArray();
                foreach (var l in logicitems)
                {
                    Container.logicCalculation.LogicEntryAquired(l, new List<string>());
                }
            }
        }
        public static void TestLocationsForInvalidVanillaItem(MiscData.InstanceContainer Container)
        {
            foreach (var i in Container.Instance.LocationPool)
            {
                string OriginalItem = i.Value.GetDictEntry(Container.Instance).OriginalItem;
                if (Container.Instance.GetItemByID(OriginalItem) is null)
                {
                    Debug.WriteLine($"{OriginalItem} at loc {i.Key} is not a valid item");
                }
            }
        }
        public static DirectoryInfo TryGetSolutionDirectoryInfo(string currentPath = null)
        {
            var directory = new DirectoryInfo(
                currentPath ?? Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            return directory;
        }

        public static void ValidateDevFiles()
        {
            if (!Directory.Exists(Globalpaths.BaseAppdataPath)) { Directory.CreateDirectory(Globalpaths.BaseAppdataPath); }
            Dictionary<string, string> DevINI = new Dictionary<string, string>();
            if (File.Exists(Globalpaths.DevFile)) { DevINI = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Globalpaths.DevFile))??new Dictionary<string, string>(); }

            bool PathsUpdated = false;
            if (!DevINI.ContainsKey("TrackerCodePath") || !Directory.Exists(DevINI["TrackerCodePath"]))
            {
                FolderBrowserDialog dialog = new()
                {
                    InitialDirectory = Utility.TryGetSolutionDirectoryInfo().FullName,
                    Description = "Select the directory containing the .sln file"
                };
                dialog.ShowDialog();
                DevINI["TrackerCodePath"] = dialog.SelectedPath;
                PathsUpdated = true;
            }
            if (!DevINI.ContainsKey("TestingFolder") || !Directory.Exists(DevINI["TestingFolder"]))
            {
                FolderBrowserDialog dialog = new() { Description = "Select the Testing output folder" };
                dialog.ShowDialog();
                DevINI["TestingFolder"] = dialog.SelectedPath;
                PathsUpdated = true;
            }
            if (PathsUpdated) { File.WriteAllText(Globalpaths.DevFile, JsonConvert.SerializeObject(DevINI, MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions)); }
        }
    }
}
