using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
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
            TrackerInstanceCreation.InstanceCreated += WinFormInstanceCreation_InstanceCreated;
        }

        private static void WinFormInstanceCreation_InstanceCreated(MMR_Tracker_V3.TrackerObjects.InstanceData.InstanceContainer obj)
        {
            TestingForm.CurrentForm.Invoke(new MethodInvoker(TestingForm.CurrentForm.UpdateDebugActions));
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
