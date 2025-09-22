using Microsoft.VisualBasic;
using MMR_Tracker_V3.TrackerObjects;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TDMUtils;
using TestingForm.GameDataCreation.OOTMMV3;
using Windows_Form_Frontend;
using static TDMUtils.DataFileUtilities;
using static TDMUtils.MiscUtilities;

namespace TestingForm
{
    public partial class TestingForm : Form
    {
        public static TestingForm CurrentForm;
        public static NetClient CurrentNetClientForm;
        public TestingForm()
        {
            CurrentForm = this;
            InitializeComponent();
        }

        private void MainInterface_Load(object sender, EventArgs e)
        {
            TestingUtility.ValidateDevFiles();
            UpdateDebugActions();
            DLLImport.AllocConsole();
            DLLImport.ShowWindow(DLLImport.GetConsoleWindow(), DLLImport.SW_HIDE);
            CLITrackerTesting.CLIDebugListener = new(Console.Out);
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

            List<DevAction> DevFunctions =
            [
                new("Open WinForm Tracker Debug", WinFormTesting.ActivateWinFormInterface, UpdateDebugActions, () => !WinFormTesting.WinformLoaded()),
                new("Print Debug to Console", CLITrackerTesting.AddCLIDebugListener, UpdateDebugActions, () => CLITrackerTesting.IsCLIDebugListenerInactive() && !CLITrackerTesting.IsCLIActive()),
                new("Stop Printing Debug to Console", CLITrackerTesting.RemoveCLIDebugListener, UpdateDebugActions, () =>CLITrackerTesting.IsCLIDebugListenerActive() && !CLITrackerTesting.IsCLIActive()),
                new("Open CLI Tracker Debug", CLITrackerTesting.OpenCLITracker, UpdateDebugActions, () => !CLITrackerTesting.IsCLIActive()),
                new("Save Tracker State", WinFormTesting.SaveWinformTrackerState, UpdateDebugActions, WinFormTesting.CanSaveWinformTrackerState),
                new("Load Tracker State", WinFormTesting.LoadWinformTrackerState, UpdateDebugActions, WinFormTesting.CanLoadWinformTrackerState),
                new("Print Selected Object to Console", WinFormTesting.PrintWinformSelectedObject, UpdateDebugActions, () => WinFormTesting.LastSelectedObject is not null),
                new("Give Item", WinFormTesting.GiveItem, UpdateDebugActions, WinFormTesting.WinformLoaded),
                new("Create MMR Data", GameFileCreation.MMRCreateData, UpdateDebugActions),
                new("Create TPR Data", GameFileCreation.TPRCreateData, UpdateDebugActions),
                new("Create OOTMM Data", GameFileCreation.OOTMMCreateData, UpdateDebugActions),
                new("Create OOTMM Areas", OOTMMExtraFunctions.WriteAreaFile, UpdateDebugActions),
                new("Create PMR Data", GameFileCreation.PMRCreateData, UpdateDebugActions),
                new("Create LAS Data", GameFileCreation.LASCreateData, UpdateDebugActions),
                new("Create WWR Data", GameFileCreation.WWRCreateData, UpdateDebugActions),
                new("Create WWHDR Data", GameFileCreation.WWHDRCreateData, UpdateDebugActions),
                new("Create BanjoTooie Data", GameFileCreation.BTCreateData, UpdateDebugActions),
                new("Create BanjoTooie Logic File Templates", () => { new GameDataCreation.BanjoTooie.WorldTemplate().WriteWorldFiles(); }, UpdateDebugActions),
                new("Create Minecraft Data", GameFileCreation.MinecraftCreateData, UpdateDebugActions),
                new("Create Pikmin2 Data", GameFileCreation.Pikmin2CreateData, UpdateDebugActions),
                new("Print Archipelago Server Data", Archipelago, UpdateDebugActions),
                new("Test Py Spoiler Parser", TestPythonParser, UpdateDebugActions),
                new("Test Random Stuff", RandomTests, UpdateDebugActions),
            ];

            foreach (var Function in DevFunctions)
            {
                if (Function.Conitional is not null && !Function.Conitional()) { continue; }
                listBox1.Items.Add(Function);
            }
        }

        private void Archipelago()
        {
            string Game = Interaction.InputBox("Enter Game");
            string Slot = Interaction.InputBox("Enter Slot ID");
            string Passwrd = Interaction.InputBox("Enter Password");
            string Server = Interaction.InputBox("Enter Server Address");

            MMR_Tracker_V3.NetCode.ArchipelagoConnector archipelago = new(Game, Slot, Passwrd, Server);

            if (!archipelago.WasConnectionSuccess(out string[] Error)) {
                MessageBox.Show(string.Join("\n", Error));
                return;
            }

            var Data = archipelago.Session.Locations.ScoutLocationsAsync(archipelago.Session.Locations.AllLocations.ToArray()).Result;
            var CleanData = Data.Locations.Select(x =>
                (archipelago.Session.Locations.GetLocationNameFromId(x.Location),
                archipelago.Session.Items.GetItemName(x.Item)));
            Debug.WriteLine(CleanData.ToFormattedJson());
            Debug.WriteLine(archipelago.GetLoginSuccessInfo().ToFormattedJson());
            Debug.WriteLine(archipelago.Session.DataStorage.GetHints(archipelago.Session.ConnectionInfo.Slot).ToFormattedJson());
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

        public void TestPythonParser()
        {
            string TestScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SpoilerLogHandling", "Python", "Testpy.py");
            MMR_Tracker_V3.SpoilerLogHandling.Python.Test.ReadFromPyFile(TestScriptPath);
        }

        private void RandomTests()
        {
            Font F = SystemFonts.DefaultFont;
            Debug.WriteLine(F.ToFormattedJson());
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

            bool t1 = DynamicMethodExists(ChoiceOption, "SetValue");
            bool t2 = DynamicMethodExists(MultiOption, "SetValue");
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

        private void LB_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is DevAction DevAction) 
            {
                DevAction.Run();
            }
        }
    }
}