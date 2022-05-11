using MMR_Tracker_V3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Windows_Form_Frontend
{
    public class WinFormInstanceCreation
    {
        public static bool CreateWinFormInstance(string Logic = null, string Dictionary = null)
        {
            var NewInstance = new LogicObjects.TrackerInstance();
            if (Logic == null)
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.ShowDialog();
                Logic = File.ReadAllText(fileDialog.FileName);
            }

            var Result = TrackerInstanceCreation.ApplyLogicAndDict(NewInstance, Logic, Dictionary);

            if (Result == TrackerInstanceCreation.InstanceState.LogicFailure || NewInstance.LogicFile.Logic == null)
            {
                MessageBox.Show("Failed To Load Logic");
                return false;
            }
            if (Result == TrackerInstanceCreation.InstanceState.DictionaryFailure || NewInstance.LogicDictionary == null)
            {
                MessageBox.Show("Failed To Load Dict");
                return false;
            }

            MainInterface.InstanceContainer.CurrentSavePath = "";

            TrackerInstanceCreation.PopulateTrackerObject(NewInstance);

            if (File.Exists(References.WindowsPaths.OptionFile))
            {
                NewInstance.StaticOptions.OptionFile = JsonConvert.DeserializeObject<LogicObjects.OptionFile>(File.ReadAllText(References.WindowsPaths.OptionFile));
            }

            MainInterface.InstanceContainer.Instance = NewInstance;

            ApplyWinFormSpecificDat(NewInstance);
            MainInterface.InstanceContainer.logicCalculation.CalculateLogic();
            MainInterface.CurrentProgram.UpdateUI();

            return true;
        }

        public static void ApplyWinFormSpecificDat(LogicObjects.TrackerInstance instance)
        {
            if (string.IsNullOrWhiteSpace(instance.StaticOptions.OptionFile.WinformData.FormFont))
            {
                instance.StaticOptions.OptionFile.WinformData.FormFont = WinFormUtils.ConvertFontToString(null);
            }
        } 

        public static void ApplyUserPretLogic()
        {
            MainInterface.CurrentProgram.presetsToolStripMenuItem.DropDownItems.Clear();

            foreach (var i in LogicPresetHandeling.GetLogicPresets())
            {
                Debug.WriteLine($"Adding Preset {i.Name}");
                ToolStripMenuItem menuItem = new() { Text = i.Name };
                menuItem.Click += (s, ee) =>
                {
                    if (!MainInterface.CurrentProgram.PromptSave()) { return; }
                    CreateWinFormInstance(i.LogicString, i.DictionaryString); 
                };
                MainInterface.CurrentProgram.presetsToolStripMenuItem.DropDownItems.Add(menuItem);
            }
        }
    }
}
