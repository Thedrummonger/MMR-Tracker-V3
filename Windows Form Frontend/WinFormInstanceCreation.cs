using MMR_Tracker_V3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Windows_Form_Frontend
{
    public class WinFormInstanceCreation
    {
        public static void CreateWinFormInstance(string Logic = null)
        {
            MainInterface.MainUITrackerInstance = new MMR_Tracker_V3.LogicObjects.TrackerInstance();
            if (Logic == null)
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.ShowDialog();
                Logic = File.ReadAllText(fileDialog.FileName);
            }

            var Result = TrackerInstanceCreation.ApplyLogicAndDict(MainInterface.MainUITrackerInstance, Logic);

            TrackerInstanceCreation.PopulateTrackerObject(MainInterface.MainUITrackerInstance);

            ApplyWinFormSpecificDat(MainInterface.MainUITrackerInstance);

            if (Result == TrackerInstanceCreation.InstanceState.LogicFailure)
            {
                MessageBox.Show("Failed To Load Logic");
            }
            if (Result == TrackerInstanceCreation.InstanceState.DictionaryFailure)
            {
                MessageBox.Show("Failed To Load Dict");
            }
        }

        public static void ApplyWinFormSpecificDat(LogicObjects.TrackerInstance instance)
        {
            if (string.IsNullOrWhiteSpace(instance.Options.WinformData.FormFont))
            {
                instance.Options.WinformData.FormFont = WinFormUtils.ConvertFontToString(null);
            }
        } 
    }
}
