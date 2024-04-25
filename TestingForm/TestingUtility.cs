using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System.Diagnostics;
using YamlDotNet.Serialization;
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
            File.WriteAllText(CreateTestingFilePath(Name, Extention), JsonConvert.SerializeObject(Data, MMR_Tracker_V3.Utility.DefaultSerializerSettings));
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
            if (PathsUpdated) { File.WriteAllText(Globalpaths.DevFile, JsonConvert.SerializeObject(DevINI, MMR_Tracker_V3.Utility.DefaultSerializerSettings)); }
        }

        /// <summary>
        /// Converts the given YAML string to a serialized JSON string
        /// </summary>
        /// <param name="YAML">YAML string</param>
        /// <param name="Format">Whether or not to format the resulting JSON string</param>
        /// <returns></returns>
        public static string ConvertYamlStringToJsonString(string YAML, bool Format = false)
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
            object yamlIsDumb = deserializer.Deserialize<object>(YAML);
            if (Format) { return JsonConvert.SerializeObject(yamlIsDumb, Utility.DefaultSerializerSettings); }
            return JsonConvert.SerializeObject(yamlIsDumb);
        }

        public static string ToYamlString(this object e)
        {
            var serializer = new YamlDotNet.Serialization.SerializerBuilder().Build();
            return serializer.Serialize(e);
        }

        public static T DeserializeJsonFile<T>(string Path)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(Path));
        }
        public static T DeserializeCSVFile<T>(string Path)
        {
            var Json = TestingUtility.ConvertCsvFileToJsonObject(File.ReadAllLines(Path));
            return JsonConvert.DeserializeObject<T>(Json);
        }

        public static T DeserializeYAMLFile<T>(string Path)
        {
            var Json = TestingUtility.ConvertYamlStringToJsonString(File.ReadAllText(Path), true);
            return JsonConvert.DeserializeObject<T>(Json);
        }

        /// <summary>
        /// Converts the Given object to a YAML string
        /// </summary>
        /// <param name="OBJ">The source Object</param>
        /// <returns></returns>
        public static string ConvertObjectToYamlString(object OBJ)
        {
            var serializer = new SerializerBuilder().Build();
            var stringResult = serializer.Serialize(OBJ);
            return stringResult;
        }
        public static string ConvertCsvFileToJsonObject(string[] lines)
        {
            var csv = new List<string[]>();

            var properties = lines[0].Split(',');

            foreach (string line in lines)
            {
                var LineData = line.Split(',');
                csv.Add(LineData);
            }

            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) { continue; }
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                    objResult.Add(properties[j].Trim(), csv[i][j].Trim());

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult, Utility.DefaultSerializerSettings);
        }
    }
}
