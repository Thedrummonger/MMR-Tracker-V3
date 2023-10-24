using MMR_Tracker_V3.TrackerObjects;
using MMR_Tracker_V3;
using Newtonsoft.Json;
using System.Diagnostics;
using Windows_Form_Frontend;
using System.Windows.Forms;
using MMR_Tracker_V3.TrackerObjectExtentions;
using CLIFrontEnd;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata;
using MathNet.Numerics;

namespace TestingForm
{
    public partial class TestingForm : Form
    {
        public static TestingForm CurrentForm;
        public TestingForm()
        {
            CurrentForm = this;
            CLITracker.HideConsole = CLITrackerTesting.HideCLI;
            InitializeComponent();
        }

        private void MainInterface_Load(object sender, EventArgs e)
        {
            UpdateDebugActions();
            DLLImport.AllocConsole();
            DLLImport.ShowWindow(DLLImport.GetConsoleWindow(), DLLImport.SW_HIDE);
        }

        public void UpdateDebugActions()
        {
            listBox1.Items.Clear();
            List<Tuple<string, Action, Func<bool>?>> DevFunctions = new List<Tuple<string, Action, Func<bool>?>>
            {
                new ("Open WinForm Tracker Debug", WinFormTesting.ActivateWinFormInterface, () => !WinFormTesting.WinformLoaded()),
                new ("Open CLI Tracker Debug", CLITrackerTesting.OpenCLITracker, null),
                new ("Save Tracker State", WinFormTesting.SaveWinformTrackerState, WinFormTesting.CanSaveWinformTrackerState),
                new ("Load Tracker State", WinFormTesting.LoadWinformTrackerState, WinFormTesting.CanLoadWinformTrackerState),
                new ("Print Selected Object to Console", WinFormTesting.PrintWinformSelectedObject, () => WinFormTesting.LastSelectedObject is not null),
                new ("Create TPR Data", GameFileCreation.TPRCreateData, null),
                new ("Create OOTMM Data", GameFileCreation.OOTMMCreateData, null),
                new ("Create PMR Data", GameFileCreation.PMRCreateData, null),
            };

            foreach (var Function in DevFunctions)
            {
                if (Function.Item3 is not null && !Function.Item3()) { continue; }
                var MenuItem = new MiscData.StandardListBoxItem { Display = Function.Item1, tagAction = Function.Item2 };
                listBox1.Items.Add(MenuItem);
            }
        }
        private void LB_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is MiscData.StandardListBoxItem LBI) { LBI.tagAction(); }
            UpdateDebugActions();
        }
    }
}