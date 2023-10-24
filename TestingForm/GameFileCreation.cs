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

        public static void PMRCreateData()
        {
            WinFormTesting.ActivateWinFormInterface();
            MMR_Tracker_V3.OtherGames.PaperMarioRando.ReadData.ReadEadges(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            Utility.TestLogicForInvalidItems(MainInterface.InstanceContainer);
        }

        public static void TPRCreateData()
        {
            WinFormTesting.ActivateWinFormInterface();
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
            WinFormTesting.ActivateWinFormInterface();
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
    }
}
