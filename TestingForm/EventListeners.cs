using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows_Form_Frontend;

namespace TestingForm
{
    internal class EventListeners
    {
        public static void BuildWinFormEventListeners()
        {
            MainInterface.CurrentProgram.FormClosed += new FormClosedEventHandler(MainInterface_FormClosing);
            MainInterface.CurrentProgram.LBValidLocations.SelectedIndexChanged += LBSelectedIndexChanged;
            MainInterface.CurrentProgram.LBValidEntrances.SelectedIndexChanged += LBSelectedIndexChanged;
            MainInterface.CurrentProgram.LBCheckedLocations.SelectedIndexChanged += LBSelectedIndexChanged;
            MMR_Tracker_V3.TrackerDataHandeling.CheckedObjectsUpdate += TrackerDataHandeling_CheckedObjectsUpdate;
        }

        private static void TrackerDataHandeling_CheckedObjectsUpdate(List<object> obj, MMR_Tracker_V3.InstanceData.TrackerInstance instance)
        {

        }

        private static void LBSelectedIndexChanged(object? sender, EventArgs e)
        {
            var ActiveControl = MainInterface.CurrentProgram.ActiveControl;
            if (ActiveControl is not ListBox ActiveLB) { return ; }
            if (ActiveLB.SelectedItem is null) { return; }
            bool FirstSelection = WinFormTesting.LastSelectedObject is null;
            WinFormTesting.LastSelectedObject = ActiveLB.SelectedItem;
            if (FirstSelection) { TestingForm.CurrentForm.UpdateDebugActions(); }
        }

        public static void MainInterface_FormClosing(object? s, EventArgs e)
        {
            TestingForm.CurrentForm.UpdateDebugActions();
            WinFormTesting.CleanUpWinForm();
            TestingForm.CurrentForm.UpdateDebugActions();
        }
    }
}
