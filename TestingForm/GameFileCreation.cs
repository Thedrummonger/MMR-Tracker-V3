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
    internal class GameFileCreation
    {
        public static void MMRCreateData()
        {
            MMRData.LogicFile CasualLogic = JsonConvert.DeserializeObject<MMRData.LogicFile>(File.ReadAllText(Path.Combine(TestingReferences.GetDevTestingPath(), "MMR 116", "REQ_CASUAL.txt")));
            var MMRDictV16 = GameDataCreation.MMR.GenData.CreateMMRFiles();

            File.WriteAllText(Path.Combine(TestingReferences.GetLibraryDictionaryPath(), "MMR V24.json"), JsonConvert.SerializeObject(MMRDictV16, MMR_Tracker_V3.Utility.DefaultSerializerSettings));

            TestingUtility.CreateTestingFile(MMRDictV16, @"MMR 116\MMRV16Dict", "json");

            WinFormTesting.ActivateWinFormInterface();
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(CasualLogic), JsonConvert.SerializeObject(MMRDictV16));

        }

        public static void PMRCreateData()
        {
            WinFormTesting.ActivateWinFormInterface();
            MMR_Tracker_V3.GameDataCreation.PaperMarioRando.ReadData.ReadEadges(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            TestingUtility.TestLogicForInvalidItems(MainInterface.InstanceContainer);
        }

        public static void TPRCreateData()
        {
            WinFormTesting.ActivateWinFormInterface();
            MMR_Tracker_V3.GameDataCreation.TPRando.ReadAndParseData.CreateFiles(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            TestingUtility.TestLogicForInvalidItems(MainInterface.InstanceContainer);
            TestingUtility.TestLocationsForInvalidVanillaItem(MainInterface.InstanceContainer);

            //Testing.PrintObjectToConsole(MMR_Tracker_V3.OtherGames.TPRando.ParseMacrosFromCode.ReadMacrosFromCode());

            List<string> Areas = MainInterface.InstanceContainer.Instance.LocationPool.Values.Select(x => x.GetDictEntry().Area).Distinct().ToList();
            MMR_Tracker_V3.Utility.PrintObjectToConsole(Areas);

            List<string> Bugs = MainInterface.InstanceContainer.Instance.ItemPool.Values.Where(x => x.ID.StartsWith("Female_") || x.ID.StartsWith("Male_")).Select(x => x.ID).ToList();
            string AnyBug = string.Join(" or ", Bugs);
            Debug.WriteLine(AnyBug);

            string Root = MainInterface.InstanceContainer.Instance.EntrancePool.RootArea;
            Debug.WriteLine($"Root Area {Root} Valid {MainInterface.InstanceContainer.Instance.EntrancePool.AreaList.ContainsKey(Root)} Aquired {MainInterface.InstanceContainer.logicCalculation.LogicEntryAquired(Root, new List<string>())}");
        }

        public static void OOTMMCreateData()
        {
            WinFormTesting.ActivateWinFormInterface();
            MMR_Tracker_V3.GameDataCreation.OOTMMV2.GenData.ReadData(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            foreach (var i in MainInterface.InstanceContainer.Instance.EntrancePool.AreaList.Values)
            {
                foreach (var j in i.RandomizableExits()) { j.Value.RandomizedState = MiscData.RandomizedState.Unrandomized; }
            }
            MainInterface.InstanceContainer.logicCalculation.CalculateLogic();
            MainInterface.CurrentProgram.UpdateUI();
            TestingUtility.TestLogicForInvalidItems(MainInterface.InstanceContainer);
            TestingUtility.TestLocationsForInvalidVanillaItem(MainInterface.InstanceContainer);
        }
    }
}
