using MMR_Tracker_V3.TrackerObjects;
using MMR_Tracker_V3;
using Newtonsoft.Json;
using System.Diagnostics;
using Windows_Form_Frontend;

namespace TestingForm
{
    public partial class TestingForm : Form
    {
        public static MainInterface TestingInterface;
        public TestingForm()
        {
            InitializeComponent();
        }

        private void LB_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is MiscData.StandardListBoxItem LBI) { LBI.tagAction(); }
        }

        private void MainInterface_Load(object sender, EventArgs e)
        {
            AddDebugActions(listBox1);
        }

        public static void AddDebugActions(ListBox codeTestingToolStripMenuItem)
        {
            Dictionary<string, Action> DevFunctions = new Dictionary<string, Action>()
            {
                { "Create TPR Data", TPRCreateData },
                { "Create OOTMM Data", OOTMMCreateData },
                { "Create PMR Data", PMRCreateData },
            };

            foreach (var Function in DevFunctions)
            {
                var MenuItem = new MiscData.StandardListBoxItem { Display = Function.Key, tagAction = Function.Value };
                codeTestingToolStripMenuItem.Items.Add(MenuItem);
            }
        }

        public static void PMRCreateData()
        {
            Utility.ActivateWinFormInterface();
            MMR_Tracker_V3.OtherGames.PaperMarioRando.ReadData.ReadEadges(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            Testing.TestLogicForInvalidItems(MainInterface.InstanceContainer);
        }

        public static void TPRCreateData()
        {
            Utility.ActivateWinFormInterface();
            MMR_Tracker_V3.OtherGames.TPRando.ReadAndParseData.CreateFiles(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            Testing.TestLogicForInvalidItems(MainInterface.InstanceContainer);
            Testing.TestLocationsForInvalidVanillaItem(MainInterface.InstanceContainer);

            //Testing.PrintObjectToConsole(MMR_Tracker_V3.OtherGames.TPRando.ParseMacrosFromCode.ReadMacrosFromCode());

            List<string> Areas = MainInterface.InstanceContainer.Instance.LocationPool.Values.Select(x => x.GetDictEntry(MainInterface.InstanceContainer.Instance).Area).Distinct().ToList();
            Testing.PrintObjectToConsole(Areas);

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
            Testing.TestLogicForInvalidItems(MainInterface.InstanceContainer);
            Testing.TestLocationsForInvalidVanillaItem(MainInterface.InstanceContainer);
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