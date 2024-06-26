﻿using MMR_Tracker_V3.TrackerObjectExtensions;
using Newtonsoft.Json;
using System.Diagnostics;
using TDMUtils;
using static MMR_Tracker_V3.References;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument.

namespace TestingForm
{
    internal static class TestingUtility
    {
        public static string CreateTestingFilePath(string Name, string Extention = "txt")
        {
            return Path.Combine(TestingReferences.GetDevTestingPath(), $"{Name}.{Extention}");
        }

        public static void CreateTestingFile(object Data, string Name, string Extention = "txt")
        {
            File.WriteAllText(CreateTestingFilePath(Name, Extention), JsonConvert.SerializeObject(Data, NewtonsoftExtensions.DefaultSerializerSettings));
        }

        public static void TestLogicForInvalidItems(MMR_Tracker_V3.TrackerObjects.InstanceData.InstanceContainer Container)
        {
            foreach (var i in Container.Instance.LogicFile.Logic)
            {
                var logicitems = i.ConditionalItems.SelectMany(x => x).Concat(i.RequiredItems).ToArray();
                foreach (var l in logicitems)
                {
                    Container.logicCalculation.LogicEntryAquired(l);
                }
            }
        }
        public static void TestLocationsForInvalidVanillaItem(MMR_Tracker_V3.TrackerObjects.InstanceData.InstanceContainer Container)
        {
            foreach (var i in Container.Instance.LocationPool)
            {
                string OriginalItem = i.Value.GetDictEntry().OriginalItem;
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
            TestingReferences.DevINI DevINI = new();
            if (File.Exists(Globalpaths.DevFile)) { DevINI = TestingReferences.GetDevINI(); }

            bool PathsUpdated = false;
            if (DevINI?.TrackerCodePath is null || !Directory.Exists(DevINI?.TrackerCodePath))
            {
                FolderBrowserDialog dialog = new()
                {
                    InitialDirectory = TestingUtility.TryGetSolutionDirectoryInfo().FullName,
                    Description = "Select the directory containing the .sln file"
                };
                dialog.ShowDialog();
                DevINI.TrackerCodePath = dialog.SelectedPath;
                PathsUpdated = true;
            }
            if (DevINI.TestingFolder is null || !Directory.Exists(DevINI.TestingFolder))
            {
                FolderBrowserDialog dialog = new() { Description = "Select the Testing output folder" };
                dialog.ShowDialog();
                DevINI.TestingFolder = dialog.SelectedPath;
                PathsUpdated = true;
            }
            if (PathsUpdated) { File.WriteAllText(Globalpaths.DevFile, JsonConvert.SerializeObject(DevINI, NewtonsoftExtensions.DefaultSerializerSettings)); }
        }
    }
}
