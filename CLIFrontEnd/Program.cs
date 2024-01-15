using static System.Net.Mime.MediaTypeNames;
using MMR_Tracker_V3.TrackerObjects;
using MMR_Tracker_V3;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using System.Data;
using System.ComponentModel;
using System.Diagnostics;

namespace CLIFrontEnd
{
    static class Program
    {
        static void Main()
        {
            object[] MainMenu = ["New (From File)", "New (From Preset)", "Load"];
            var MenuSelect = new CLISelectMenu(MainMenu);
            MenuSelect.Run();

            InstanceContainer container = new();

            switch(MenuSelect.selectedLineIndex)
            {
                case 0:
                    LoadFromLogicFile(container);
                    break;
                case 1:
                    LoadFromTemplate(container);
                    break;
                case 3:
                    //LoadFromSaveFile(container);
                    break;
            }
            ShowLocations(container);

        }

        private static void LoadFromLogicFile(InstanceContainer container)
        {
            var Result = FileSelectDialog.ShowDialog("Logic Files (*.json)\0*.json\0All Files (*.*)\0*.*\0");
            container.GenerateInstance(File.ReadAllText(Result));
        }

        public static void ShowLocations(InstanceContainer container)
        {
            Console.Clear();
            container.logicCalculation.CalculateLogic();
            var Data = new TrackerLocationDataList(new Divider("==========="), container, "");
            TrackerDataHandeling.PopulateAvailableLocationList(Data);
            Dictionary<int, object> Locations = Data.FinalData.Select((s, index) => new { s, index }).ToDictionary(x => x.index+1, x => x.s);
            int Padding = Locations.Keys.Max().ToString().Length;
            foreach (var LocationObject in Locations)
            {
                Console.WriteLine($"{LocationObject.Key.ToString($"D{Padding}")}: {LocationObject.Value}");
            }

        }

        private static void LoadFromTemplate(InstanceContainer container)
        {
            Console.Clear();
            Console.WriteLine("Getting Presets...");
            var Templates = LogicPresetHandeling.GetLogicPresets();
            var MenuSelect = new CLISelectMenu(Templates);
            var SelectedTemplate = (LogicPresetHandeling.PresetlogicData)MenuSelect.Run();
            container.GenerateInstance(SelectedTemplate.LogicString, SelectedTemplate.DictionaryString);
        }
    }
}