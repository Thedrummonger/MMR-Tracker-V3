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
        public static MainInterface TestingInterface;
        public static TestingForm CurrentForm;
        public TestingForm()
        {
            CurrentForm = this;
            CLITracker.HideConsole = HideCLI;
            InitializeComponent();
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();

        private void MainInterface_Load(object sender, EventArgs e)
        {
            AddDebugActions(listBox1);
            AllocConsole();
            ShowWindow(GetConsoleWindow(), SW_HIDE);
        }

        public static void MainInterface_FormClosing()
        {
            Debug.WriteLine("WinForm Closed");
        }

        public static void AddDebugActions(ListBox codeTestingToolStripMenuItem)
        {
            codeTestingToolStripMenuItem.Items.Clear();
            List<Tuple<string, Action, Func<bool>?>> DevFunctions = new List<Tuple<string, Action, Func<bool>?>>
            {
                new ("Open WinForm Tracker Debug", OpenWinForm, null),
                new ("OPen CLI Tracker Debug", OpenCLITracker, null),
                new ("Save Tracker State", SaveTrackerState, CanSaveTrackerState),
                new ("Load Tracker State", LoadTrackerState, CanLoadTrackerState),
                new ("Print Selected Object to Console", PrintDevData, null),
                new ("Create TPR Data", TPRCreateData, null),
                new ("Create OOTMM Data", OOTMMCreateData, null),
                new ("Create PMR Data", PMRCreateData, null),
            };

            foreach (var Function in DevFunctions)
            {
                if (Function.Item3 is not null && !Function.Item3()) { continue; }
                var MenuItem = new MiscData.StandardListBoxItem { Display = Function.Item1, tagAction = Function.Item2 };
                codeTestingToolStripMenuItem.Items.Add(MenuItem);
            }
        }
        private void LB_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is MiscData.StandardListBoxItem LBI) { LBI.tagAction(); }
            AddDebugActions(listBox1);
        }

        private static void OpenCLITracker()
        {
            AllocConsole();
            ShowWindow(GetConsoleWindow(), SW_SHOW);
            CLITracker.Main(Environment.GetCommandLineArgs());
        }

        private static void HideCLI()
        {
            Debug.WriteLine("Hiding CLI");
            ShowWindow(GetConsoleWindow(), SW_HIDE);
        }

        public static void PrintDevData()
        {
            if (TestingInterface is null || MainInterface.CurrentProgram is null) { Debug.WriteLine($"WinForm Instance was not created"); return; }
            if (MainInterface.InstanceContainer?.Instance is null) { Debug.WriteLine($"Tracker Instance Not Created"); return; }
            MainInterface WFP = MainInterface.CurrentProgram;
            ListBox ActiveListbox = null;
            if (WFP.ActiveControl == WFP.LBValidEntrances) { ActiveListbox = WFP.LBValidEntrances; }
            else if (WFP.ActiveControl == WFP.LBValidLocations) { ActiveListbox = WFP.LBValidLocations; }
            else if (WFP.ActiveControl == WFP.LBCheckedLocations) { ActiveListbox = WFP.LBCheckedLocations; }
            if (ActiveListbox is null) { Debug.WriteLine($"Active item is not a valid List Box"); return; }
            if (ActiveListbox.SelectedItem is null) { Debug.WriteLine($"No valid item selected in {ActiveListbox.Name}"); return; }

            string RandomizedItem = null;
            Debug.WriteLine($"Data for {ActiveListbox.SelectedItem}=========================================================");
            Debug.WriteLine(JsonConvert.SerializeObject(ActiveListbox.SelectedItem, MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
            if (ActiveListbox.SelectedItem is LocationData.LocationObject DebugLocObj)
            {
                Debug.WriteLine($"Dictionary Entry");
                Debug.WriteLine(JsonConvert.SerializeObject(DebugLocObj.GetDictEntry(MainInterface.InstanceContainer.Instance), MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
                RandomizedItem = DebugLocObj.Randomizeditem.Item;

            }
            if (ActiveListbox.SelectedItem is LocationData.LocationProxy DebugProxyObj)
            {
                var ProxyRef = MainInterface.InstanceContainer.Instance.GetLocationByID(DebugProxyObj.ReferenceID);
                Debug.WriteLine($"Proxied Entry");
                Debug.WriteLine(JsonConvert.SerializeObject(ProxyRef, MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
                Debug.WriteLine($"Dictionary Entry");
                Debug.WriteLine(JsonConvert.SerializeObject(ProxyRef?.GetDictEntry(MainInterface.InstanceContainer.Instance), MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
                RandomizedItem = ProxyRef.Randomizeditem.Item;
            }
            if (RandomizedItem !=null)
            {
                var Item = MainInterface.InstanceContainer.Instance.GetItemByID(RandomizedItem);
                if (Item is not null)
                {
                    Debug.WriteLine($"Randomized Item");
                    Debug.WriteLine(JsonConvert.SerializeObject(Item, MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
                    Debug.WriteLine($"Randomized Item Dictionary Entry");
                    Debug.WriteLine(JsonConvert.SerializeObject(Item?.GetDictEntry(MainInterface.InstanceContainer.Instance), MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
                }
            }


            Debug.WriteLine(MainInterface.CurrentProgram.ActiveControl.Name);
        }

        public static bool WinformLoaded()
        {
            return TestingForm.TestingInterface is not null && MainInterface.CurrentProgram is not null;
        }

        public static bool CanSaveTrackerState()
        {
            return WinformLoaded() && MainInterface.InstanceContainer?.Instance is not null;
        }
        public static void SaveTrackerState()
        {
            if (!CanSaveTrackerState()) { return; }
            LogicRecreation.SaveTrackerState(MainInterface.InstanceContainer);
            Debug.WriteLine($"Tracker Instance Saved;");
        }
        public static bool CanLoadTrackerState()
        {
            if (!WinformLoaded()) { Debug.WriteLine($"WinForm Instance was not created"); return false; }
            if (MainInterface.InstanceContainer?.Instance is null) { Debug.WriteLine($"Tracker Instance Not Created"); return false; }
            if (LogicRecreation.CurrentSaveState is null) { Debug.WriteLine($"No tracker state was saved"); return false; }
            return true;
        }
        public static void LoadTrackerState()
        {
            if (!CanLoadTrackerState()) { return; }
            LogicRecreation.LoadTrackerState(MainInterface.InstanceContainer);
            MainInterface.InstanceContainer.logicCalculation.CalculateLogic();
            MainInterface.CurrentProgram.UpdateUI();
        }

        public static void OpenWinForm()
        {
            Utility.ActivateWinFormInterface();
        }

        public static void PMRCreateData()
        {
            Utility.ActivateWinFormInterface();
            MMR_Tracker_V3.OtherGames.PaperMarioRando.ReadData.ReadEadges(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            Utility.TestLogicForInvalidItems(MainInterface.InstanceContainer);
        }

        public static void TPRCreateData()
        {
            Utility.ActivateWinFormInterface();
            MMR_Tracker_V3.OtherGames.TPRando.ReadAndParseData.CreateFiles(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            Utility.TestLogicForInvalidItems(MainInterface.InstanceContainer);
            Utility.TestLocationsForInvalidVanillaItem(MainInterface.InstanceContainer);

            //Testing.PrintObjectToConsole(MMR_Tracker_V3.OtherGames.TPRando.ParseMacrosFromCode.ReadMacrosFromCode());

            List<string> Areas = MainInterface.InstanceContainer.Instance.LocationPool.Values.Select(x => x.GetDictEntry(MainInterface.InstanceContainer.Instance).Area).Distinct().ToList();
            MMR_Tracker_V3.Utility.PrintObjectToConsole(Areas);

            List<string> Bugs = MainInterface.InstanceContainer.Instance.ItemPool.Values.Where(x => x.Id.StartsWith("Female_") || x.Id.StartsWith("Male_")).Select(x => x.Id).ToList();
            string AnyBug = string.Join(" or ", Bugs);
            Debug.WriteLine(AnyBug);

            string Root = MainInterface.InstanceContainer.Instance.EntrancePool.RootArea;
            Debug.WriteLine($"Root Area {Root} Valid {MainInterface.InstanceContainer.Instance.EntrancePool.AreaList.ContainsKey(Root)} Aquired {MainInterface.InstanceContainer.logicCalculation.LogicEntryAquired(Root, new List<string>())}");
        }

        public static void OOTMMCreateData()
        {
            Utility.ActivateWinFormInterface();
            MMR_Tracker_V3.OtherGames.OOTMMV2.GenData.ReadData(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            foreach (var i in MainInterface.InstanceContainer.Instance.EntrancePool.AreaList.Values)
            {
                foreach (var j in i.RandomizableExits(MainInterface.InstanceContainer.Instance)) { j.Value.RandomizedState = MiscData.RandomizedState.Unrandomized; }
            }
            MainInterface.InstanceContainer.logicCalculation.CalculateLogic();
            MainInterface.CurrentProgram.UpdateUI();
            Utility.TestLogicForInvalidItems(MainInterface.InstanceContainer);
            Utility.TestLocationsForInvalidVanillaItem(MainInterface.InstanceContainer);
        }

        public static void TestFuncParse()
        {
            MMRData.JsonFormatLogicItem logicItem = new MMRData.JsonFormatLogicItem() { Id = "Test" };
            string Test = "setting(hookshotAnywhereOot) && !setting(ageChange, none), small_keys_forest(5), small_keys_forest(2)";

            string result = MMR_Tracker_V3.OtherGames.OOTMMV2.FunctionParsing.ParseCondFunc(Test, logicItem);

            Debug.WriteLine(result);
        }
    }
}