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
        public static bool CreateWinFormInstance(string Logic = null)
        {
            MainInterface.MainUITrackerInstance = new MMR_Tracker_V3.LogicObjects.TrackerInstance();
            if (Logic == null)
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.ShowDialog();
                Logic = File.ReadAllText(fileDialog.FileName);
            }

            var Result = TrackerInstanceCreation.ApplyLogicAndDict(MainInterface.MainUITrackerInstance, Logic);

            if (Result == TrackerInstanceCreation.InstanceState.LogicFailure || MainInterface.MainUITrackerInstance.LogicFile.Logic == null)
            {
                MessageBox.Show("Failed To Load Logic");
                return false;
            }
            if (Result == TrackerInstanceCreation.InstanceState.DictionaryFailure || MainInterface.MainUITrackerInstance.LogicDictionary == null)
            {
                MessageBox.Show("Failed To Load Dict");
                return false;
            }

            TrackerInstanceCreation.PopulateTrackerObject(MainInterface.MainUITrackerInstance);

            ApplyWinFormSpecificDat(MainInterface.MainUITrackerInstance);

            return true;
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
