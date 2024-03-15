using MMR_Tracker_V3.TrackerObjects;
using Windows_Form_Frontend;
using System.Diagnostics;
using MMR_Tracker_V3.TrackerObjectExtensions;
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
using MMR_Tracker_V3.Logic;
using System.Text.RegularExpressions;

namespace TestingForm
{
    public partial class TestingForm : Form
    {
        public static TestingForm CurrentForm;
        public static NetClient CurrentNetClientForm;
        public TestingForm()
        {
            CurrentForm = this;
            //CLITracker.HideConsole = CLITrackerTesting.HideCLI;
            InitializeComponent();
        }

        private void MainInterface_Load(object sender, EventArgs e)
        {
            TestingUtility.ValidateDevFiles();
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
                new DevAction("Create MMR Data", GameFileCreation.MMRCreateData, UpdateDebugActions),
                new DevAction("Create TPR Data", GameFileCreation.TPRCreateData, UpdateDebugActions),
                new DevAction("Create OOTMM Data", GameFileCreation.OOTMMCreateData, UpdateDebugActions),
                new DevAction("Create PMR Data", GameFileCreation.PMRCreateData, UpdateDebugActions),
                new DevAction("Create LAS Data", GameFileCreation.LASCreateData, UpdateDebugActions),
                new DevAction("Open Web Client", OpenWebClient, UpdateDebugActions, () => { return CurrentNetClientForm is null && WinFormTesting.WinformInstanceLoaded();  }),
                new DevAction("Test Random Stuff", RandomTests, UpdateDebugActions),
            };

            foreach (var Function in DevFunctions)
            {
                if (Function.Conitional is not null && !Function.Conitional()) { continue; }
                listBox1.Items.Add(Function);
            }
        }

        [Flags]
        public enum settingTest
        {
            option1 = 1,
            option2 = 1 << 1,
            option3 = 1 << 2,
            option4 = 1 << 3,
            option5 = 1 << 4,
        }

        settingTest Settings = new();

        private void RandomTests()
        {
            MiscTesting.TestFuncParse();
        }

        private void RegexTesting()
        {
            string Test = "(setting(magicalRupee) && has(RUPEE_MAGICAL)) || cond(setting(silverRupeePouches), has(pouch), has(rupee, count))";

            Dictionary<string, string> Replacements = new Dictionary<string, string>()
            {
                { "rupee", "RUPEE_SILVER_GANON_SHADOW" },
                { "pouch", "POUCH_SILVER_GANON_SHADOW" },
                { "count", "5" },
            };

            foreach(var r in Replacements)
            {
                Test = ReplaceParam(Test, r.Key, r.Value);
            }

            Debug.WriteLine(Test);

        }

        private string ReplaceParam(string Logic, string Param, string Value)
        {
            string pattern = @$"\b{Param}\b";
            string result = Regex.Replace(Logic, pattern, Value);
            return result;
        }


        private void TestDynamicMethodLookup()
        {
            dynamic ChoiceOption = new OptionData.ChoiceOption(null);
            dynamic MultiOption = new OptionData.MultiSelectOption(null);

            bool t1 = MMR_Tracker_V3.Utility.DynamicMethodExists(ChoiceOption, "SetValue");
            bool t2 = MMR_Tracker_V3.Utility.DynamicMethodExists(MultiOption, "SetValue");
            Debug.WriteLine(t1);
            Debug.WriteLine(t2);

        }
        private void TestDictExtention()
        {
            printInt(1, 2, 3, 5);
            printInt(new int[] { 1, 2, 3});
        }
        private void printInt(params int[] ints)
        {
            Debug.WriteLine(ints.Length);
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