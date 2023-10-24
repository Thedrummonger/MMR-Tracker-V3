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
        }

        private static void LBSelectedIndexChanged(object? sender, EventArgs e)
        {
            var ActiveControl = MainInterface.CurrentProgram.ActiveControl;
            if (ActiveControl is not ListBox ActiveLB) { return ; }
            WinFormTesting.LastSelectedObject = ActiveLB.SelectedItem;

            Debug.WriteLine($"Control Focused [{ActiveLB.Name}] | [{ActiveLB.SelectedItem.ToString()}]");
        }

        public static void MainInterface_FormClosing(object? s, EventArgs e)
        {
            TestingForm.CurrentForm.UpdateDebugActions();
            WinFormTesting.CleanUpWinForm();
            TestingForm.CurrentForm.UpdateDebugActions();
        }
    }
}
