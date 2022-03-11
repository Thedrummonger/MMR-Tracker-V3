using MMR_Tracker_V3;
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
        public class PresetlogicData
        {
            public string Name { get; set; } = null;
            public string DictionaryString { get; set; } = null;
            public string LogicString { get; set; } = null;
        }
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

            TrackerInstanceCreation.PopulateTrackerObject(NewInstance);

            MainInterface.CurrentTrackerInstance = NewInstance;

            ApplyWinFormSpecificDat(NewInstance);
            LogicCalculation.CalculateLogic(NewInstance);
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
            PresetlogicData PresetEntry = new PresetlogicData();
            List<PresetlogicData> Entries = new List<PresetlogicData>();
            if (File.Exists(References.Globalpaths.WebPresets))
            {
                System.Net.WebClient wc = new System.Net.WebClient();
                bool ErrorEntry = false;
                foreach (var i in File.ReadAllLines(References.Globalpaths.WebPresets))
                {
                    if (i.StartsWith("Name:"))
                    {
                        ErrorEntry = false;
                        PresetEntry = new PresetlogicData();
                        PresetEntry.Name = Regex.Replace(i, "Name:", "", RegexOptions.IgnoreCase).Trim();
                    }
                    if (i.StartsWith("Dictionary:") && !ErrorEntry)
                    {
                        try { PresetEntry.DictionaryString = wc.DownloadString(Regex.Replace(i, "Dictionary:", "", RegexOptions.IgnoreCase).Trim()); }
                        catch { ErrorEntry = true; }
                    }
                    if (i.StartsWith("Address:") && !ErrorEntry)
                    {
                        try 
                        {
                            PresetEntry.LogicString = wc.DownloadString(Regex.Replace(i, "Address:", "", RegexOptions.IgnoreCase).Trim());
                            Entries.Add(PresetEntry);
                        }
                        catch { ErrorEntry = true; }
                    }
                }
            }
            foreach (var i in Directory.GetFiles(References.Globalpaths.PresetFolder).Where(x => x != References.Globalpaths.WebPresets))
            {
                PresetEntry = new PresetlogicData();
                PresetEntry.Name = Path.GetFileNameWithoutExtension(i);
                PresetEntry.LogicString = File.ReadAllText(i);
                Entries.Add(PresetEntry);
            }
            foreach (var i in Entries)
            {
                Debug.WriteLine($"Adding Preset {i.Name}");
                ToolStripMenuItem menuItem = new ToolStripMenuItem();
                menuItem.Text = i.Name;
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
