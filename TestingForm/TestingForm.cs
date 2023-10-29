using MMR_Tracker_V3.TrackerObjects;
using CLIFrontEnd;
using Windows_Form_Frontend;
using System.Diagnostics;
using MMR_Tracker_V3.TrackerObjectExtentions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MathNet.Numerics.Statistics;
using System.Security.Policy;
using System.IO;
using Octokit;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using MathNet.Numerics;
using System;
using MMR_Tracker_V3;
using FParsec;

namespace TestingForm
{
    public partial class TestingForm : Form
    {
        public static TestingForm CurrentForm;
        public static NetClient CurrentNetClientForm;
        public TestingForm()
        {
            CurrentForm = this;
            CLITracker.HideConsole = CLITrackerTesting.HideCLI;
            InitializeComponent();
        }

        private void MainInterface_Load(object sender, EventArgs e)
        {
            Utility.ValidateDevFiles();
            UpdateDebugActions();
            DLLImport.AllocConsole();
            DLLImport.ShowWindow(DLLImport.GetConsoleWindow(), DLLImport.SW_HIDE);
        }

        public class DevAction
        {
            public string Name;
            public Action action;
            public Func<bool>? Conitional;
            public Action? RefreshAction;
            public DevAction(string _Name, Action _action, Action? _RefreshAction = null, Func<bool>? _Conitional = null)
            {
                Name = _Name; action = _action; Conitional = _Conitional; RefreshAction = _RefreshAction;
            }
            public void Run() 
            { 
                action(); 
                if (RefreshAction is not null) { RefreshAction(); }
            }
            public override string ToString()
            {
                return Name;
            }
        }

        public void UpdateDebugActions()
        {
            listBox1.Items.Clear();
            //Debug Action Name, Debug action Code, Show action Check, Refresh After activation

            List<DevAction> DevFunctions = new List<DevAction>()
            {
                new DevAction("Open WinForm Tracker Debug", WinFormTesting.ActivateWinFormInterface, UpdateDebugActions, () => !WinFormTesting.WinformLoaded()),
                new DevAction("Open CLI Tracker Debug", CLITrackerTesting.OpenCLITracker, UpdateDebugActions),
                new DevAction("Save Tracker State", WinFormTesting.SaveWinformTrackerState, UpdateDebugActions, WinFormTesting.CanSaveWinformTrackerState),
                new DevAction("Load Tracker State", WinFormTesting.LoadWinformTrackerState, UpdateDebugActions, WinFormTesting.CanLoadWinformTrackerState),
                new DevAction("Print Selected Object to Console", WinFormTesting.PrintWinformSelectedObject, UpdateDebugActions, () => WinFormTesting.LastSelectedObject is not null),
                new DevAction("Create TPR Data", GameFileCreation.TPRCreateData, UpdateDebugActions),
                new DevAction("Create OOTMM Data", GameFileCreation.OOTMMCreateData, UpdateDebugActions),
                new DevAction("Create PMR Data", GameFileCreation.PMRCreateData, UpdateDebugActions),
                new DevAction("Connect To Async Web Server P1", OpenWebClient, UpdateDebugActions, () => { return CurrentNetClientForm is null && WinFormTesting.WinformInstanceLoaded();  }),
            };

            foreach (var Function in DevFunctions)
            {
                if (Function.Conitional is not null && !Function.Conitional()) { continue; }
                listBox1.Items.Add(Function);
            }
        }

        private void OpenWebClient()
        {
            CurrentNetClientForm = new NetClient(this, MainInterface.InstanceContainer);
            CurrentNetClientForm.Show();
        }

        private void LB_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is DevAction DevAction) 
            {
                DevAction.Run();
            }
        }
    }
}