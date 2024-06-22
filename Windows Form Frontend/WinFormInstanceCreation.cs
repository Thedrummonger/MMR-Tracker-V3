using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;

namespace Windows_Form_Frontend
{
    public class WinFormInstanceCreation
    {
        public static bool CreateWinFormInstance(string Logic = null, string Dictionary = null)
        {
            var TempContainer = new InstanceContainer();
            if (Logic == null)
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.ShowDialog();
                Logic = File.ReadAllText(fileDialog.FileName);
            }

            var Result = TempContainer.ApplyLogicAndDict(Logic, Dictionary);

            if (Result == TrackerInstanceCreation.InstanceState.LogicFailure || TempContainer.Instance.LogicFile.Logic == null)
            {
                MessageBox.Show("Failed To Load Logic");
                return false;
            }
            if (Result == TrackerInstanceCreation.InstanceState.DictionaryFailure || TempContainer.Instance.LogicDictionary == null)
            {
                MessageBox.Show("Failed To Load Dict");
                return false;
            }

            //If all checks pass overrite the current instance
            MainInterface.InstanceContainer.CopyAndLoadInstance(TempContainer.Instance);

            MainInterface.InstanceContainer.CurrentSavePath = "";
            MainInterface.InstanceContainer.GenerateInstance();

            MainInterface.InstanceContainer.Instance.StaticOptions.OptionFile = TrackerSettings.ReadDefaultOptionsFile();

            ApplyWinFormSpecificData(MainInterface.InstanceContainer.Instance);

            MainInterface.InstanceContainer.logicCalculation.CalculateLogic();
            MainInterface.CurrentProgram.UpdateUI();
            MainInterface.CurrentProgram.AlignUIElements();

            return true;
        }

        public static void ApplyWinFormSpecificData(MMR_Tracker_V3.TrackerObjects.InstanceData.TrackerInstance instance)
        {
            if (string.IsNullOrWhiteSpace(instance.StaticOptions.OptionFile.WinformData.FormFont))
            {
                instance.StaticOptions.OptionFile.WinformData.FormFont = WinFormUtils.ConvertFontToString(null);
            }
        }

        public static void ApplyUserPretLogic()
        {
            MainInterface.CurrentProgram.NewToolStripMenuItem1.DropDownItems.Clear();

            var LogicPresets = LogicPresetHandeling.GetLogicPresets();

            if (LogicPresets.Any())
            {
                ToolStripMenuItem DefaultMenuItem = new() { Text = "From File" };
                DefaultMenuItem.Click += (s, ee) =>
                {
                    MainInterface.CurrentProgram.NewToolStripMenuItem1_Click(s, ee);
                };
                MainInterface.CurrentProgram.NewToolStripMenuItem1.DropDownItems.Add(DefaultMenuItem);
            }

            foreach (var i in LogicPresets)
            {
                Debug.WriteLine($"Adding Preset {i.Name}");
                ToolStripMenuItem menuItem = new() { Text = i.Name };
                menuItem.Click += (s, ee) =>
                {
                    if (!MainInterface.CurrentProgram.PromptSave()) { return; }
                    CreateWinFormInstance(i.LogicString, i.DictionaryString);
                };
                MainInterface.CurrentProgram.NewToolStripMenuItem1.DropDownItems.Add(menuItem);
            }
        }
    }
}
