using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System.Diagnostics;
using TDMUtils;
using Windows_Form_Frontend;
using static TDMUtils.MiscUtilities;

namespace TestingForm
{
    internal class GameFileCreation
    {
        public static void MinecraftCreateData()
        {
            GameDataCreation.Minecraft.CreateData.ReadAndParse(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormTesting.ActivateWinFormInterface();
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            TestingUtility.TestLogicForInvalidItems(MainInterface.InstanceContainer);
            TestingUtility.TestLocationsForInvalidVanillaItem(MainInterface.InstanceContainer);
        }
        public static void WWRCreateData()
        {
            GameDataCreation.WindWakerRando.Generator.GenData(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormTesting.ActivateWinFormInterface();
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            TestingUtility.TestLogicForInvalidItems(MainInterface.InstanceContainer);
            //TestingUtility.TestLocationsForInvalidVanillaItem(MainInterface.InstanceContainer);
        }
        public static void WWHDRCreateData()
        {
            GameDataCreation.WindWakerHD.Generate.GenerateData(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormTesting.ActivateWinFormInterface();
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            TestingUtility.TestLogicForInvalidItems(MainInterface.InstanceContainer);
            TestingUtility.TestLocationsForInvalidVanillaItem(MainInterface.InstanceContainer);
        }
        public static void BTCreateData()
        {
            GameDataCreation.BanjoTooie.DataGenerator.GenData(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormTesting.ActivateWinFormInterface();
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            TestingUtility.TestLogicForInvalidItems(MainInterface.InstanceContainer);
            TestingUtility.TestLocationsForInvalidVanillaItem(MainInterface.InstanceContainer);
        }
        public static void LASCreateData()
        {
            GameDataCreation.LinksAwakeningSwitch.Gen.GenData(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormTesting.ActivateWinFormInterface();
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            TestingUtility.TestLogicForInvalidItems(MainInterface.InstanceContainer);
            TestingUtility.TestLocationsForInvalidVanillaItem(MainInterface.InstanceContainer);
        }
        public static void MMRCreateData()
        {
            MMRData.LogicFile CasualLogic = JsonConvert.DeserializeObject<MMRData.LogicFile>(File.ReadAllText(Path.Combine(TestingReferences.GetDevTestingPath(), "MMR 116", "REQ_CASUAL.txt")));
            var MMRDictV16 = GameDataCreation.MMR.GenData.CreateMMRFiles();

            File.WriteAllText(Path.Combine(TestingReferences.GetLibraryDictionaryPath(), "MMR V24.json"), JsonConvert.SerializeObject(MMRDictV16, NewtonsoftExtensions.DefaultSerializerSettings));

            TestingUtility.CreateTestingFile(MMRDictV16, @"MMR 116\MMRV16Dict", "json");

            WinFormTesting.ActivateWinFormInterface();
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(CasualLogic), JsonConvert.SerializeObject(MMRDictV16));

        }

        public static void PMRCreateData()
        {
            WinFormTesting.ActivateWinFormInterface();
            GameDataCreation.PMR_AP.GenData.ReadAndGenData(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
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
            PrintObjectToConsole(Areas);

            List<string> Bugs = MainInterface.InstanceContainer.Instance.ItemPool.Values.Where(x => x.ID.StartsWith("Female_") || x.ID.StartsWith("Male_")).Select(x => x.ID).ToList();
            string AnyBug = string.Join(" or ", Bugs);
            Debug.WriteLine(AnyBug);

        }

        public static void OOTMMCreateData()
        {
            var Generator = new GameDataCreation.OOTMMV3.OOTMMDataGenerator();
            Generator.GenerateOOTMMData();
            WinFormTesting.ActivateWinFormInterface();
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Generator.LogicFile), JsonConvert.SerializeObject(Generator.dictionary));
            TestingUtility.TestLogicForInvalidItems(MainInterface.InstanceContainer);
            TestingUtility.TestLocationsForInvalidVanillaItem(MainInterface.InstanceContainer);
            /*
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
            */
        }
    }
}
