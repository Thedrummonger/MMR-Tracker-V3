using MMR_Tracker_V3.TrackerObjects;
using static MMR_Tracker_V3.TrackerObjects.LogicDictionaryData;

namespace TestingForm.GameDataCreation.BanjoTooie
{
    internal class MumboTokenLocationHandeling
    {
        static List<(string ID, string NAME, string REGION, string COUPLEDLOCATION, int[] VALIDSETTINGS)> TokenLocations = [];

        public static void AddMumbo(LogicDictionaryData.LogicDictionary logicDictionary, MMRData.LogicFile logicFile)
        {
            //Minigame Hunt
            TokenLocations.Add(("MUMBOTKNGAME1",  "MT: Kickball Mumbo Token",           "MT",       "JIGGYMT3", [1,4]));
            TokenLocations.Add(("MUMBOTKNGAME2",  "GGM: Ordnance Storage Mumbo Token",  "GM",       "JIGGYGM5", [1,4]));
            TokenLocations.Add(("MUMBOTKNGAME3",  "WW: Hoop Hurry Mumbo Token",         "WW",       "JIGGYWW1", [1,4]));
            TokenLocations.Add(("MUMBOTKNGAME4",  "WW: Dodgem Dome Mumbo Token",        "WW",       "JIGGYWW2", [1,4]));
            TokenLocations.Add(("MUMBOTKNGAME5",  "WW: Saucer of Peril Mumbo Token",    "WW",       "JIGGYWW4", [1,4]));
            TokenLocations.Add(("MUMBOTKNGAME6",  "WW: Balloon Burst Mumbo Token",      "WW",       "JIGGYWW5", [1,4]));
            TokenLocations.Add(("MUMBOTKNGAME7",  "JRL: Mini-Sub Challenge Mumbo Token","JR",       "JIGGYJR1", [1,4]));
            TokenLocations.Add(("MUMBOTKNGAME8",  "TDL: Chompas Belly Mumbo Token",     "TL",       "JIGGYTD6", [1,4]));
            TokenLocations.Add(("MUMBOTKNGAME9",  "GI: Clinker's Cavern Mumbo Token",   "GI3ALL",   "JIGGYGI3", [1,4]));
            TokenLocations.Add(("MUMBOTKNGAME10", "GI: Twinkly Packing Mumbo Token",    "GI3ALL",   "JIGGYGI9", [1,4]));
            TokenLocations.Add(("MUMBOTKNGAME11", "HFP: Kickball Mumbo Token",          "HP",       "JIGGYHP8", [1,4]));
            TokenLocations.Add(("MUMBOTKNGAME12", "CCL: Pot O' Gold Mumbo Token",       "CC",       "JIGGYCC3", [1,4]));
            TokenLocations.Add(("MUMBOTKNGAME13", "CCL: Zubbas Mumbo Token",            "CC",       "JIGGYCC5", [1,4]));
            TokenLocations.Add(("MUMBOTKNGAME14", "CCL: Trash Can Mumbo Token",         "CC",       "JIGGYCC8", [1,4]));
            TokenLocations.Add(("MUMBOTKNGAME15", "CCL: Canary Mary Mumbo Token",       "CC",       "JIGGYCC4", [1,4]));

            //Bosses
            TokenLocations.Add(("MUMBOTKNBOSS1", "MT: Targitzan Mumbo Token",       "MT",       "JIGGYMT1", [2, 4]));
            TokenLocations.Add(("MUMBOTKNBOSS2", "GGM: Old King Coal Mumbo Token",  "GM",       "JIGGYGM1", [2, 4]));
            TokenLocations.Add(("MUMBOTKNBOSS3", "WW: Mr. Patch Mumbo Token",       "WW",       "JIGGYWW3", [2, 4]));
            TokenLocations.Add(("MUMBOTKNBOSS4", "JRL: Woo Fak Fak Mumbo Token",    "JR",       "JIGGYJR7", [2, 4]));
            TokenLocations.Add(("MUMBOTKNBOSS5", "TDL: Terry Mumbo Token",          "TL",       "JIGGYTD4", [2, 4]));
            TokenLocations.Add(("MUMBOTKNBOSS6", "GI: Weldar Mumbo Token",          "GI3ALL",   "JIGGYGI2", [2, 4]));
            TokenLocations.Add(("MUMBOTKNBOSS7", "HFP: Dragon Brothers Mumbo Token","HP",       "JIGGYHP1", [2, 4]));
            TokenLocations.Add(("MUMBOTKNBOSS8", "CCL: Mingy Jongo Mumbo Token",    "CC",       "JIGGYCC1", [2, 4]));

            //Jinjo Family
            TokenLocations.Add(("MUMBOTKNJINJO1", "IoH: White Jinjo Family Mumbo Token",    "IOHJV", "JIGGYIH1", [3, 4]));
            TokenLocations.Add(("MUMBOTKNJINJO2", "IoH: Orange Jinjo Family Mumbo Token",   "IOHJV", "JIGGYIH2", [3, 4]));
            TokenLocations.Add(("MUMBOTKNJINJO3", "IoH: Yellow Jinjo Family Mumbo Token",   "IOHJV", "JIGGYIH3", [3, 4]));
            TokenLocations.Add(("MUMBOTKNJINJO4", "IoH: Brown Jinjo Family Mumbo Token",    "IOHJV", "JIGGYIH4", [3, 4]));
            TokenLocations.Add(("MUMBOTKNJINJO5", "IoH: Green Jinjo Family Mumbo Token",    "IOHJV", "JIGGYIH5", [3, 4]));
            TokenLocations.Add(("MUMBOTKNJINJO6", "IoH: Red Jinjo Family Mumbo Token",      "IOHJV", "JIGGYIH6", [3, 4]));
            TokenLocations.Add(("MUMBOTKNJINJO7", "IoH: Blue Jinjo Family Mumbo Token",     "IOHJV", "JIGGYIH7", [3, 4]));
            TokenLocations.Add(("MUMBOTKNJINJO8", "IoH: Purple Jinjo Family Mumbo Token",   "IOHJV", "JIGGYIH8", [3, 4]));
            TokenLocations.Add(("MUMBOTKNJINJO9", "IoH: Black Jinjo Family Mumbo Token",    "IOHJV", "JIGGYIH9", [3, 4]));

            foreach(var location in TokenLocations)
            {
                logicDictionary.LocationList[location.ID].Area = logicDictionary.LocationList[location.COUPLEDLOCATION].Area;
                logicDictionary.LocationList[location.ID].ValidItemTypes = ["MUMBOTOKEN"];

                logicFile.Logic.Add(new MMRData.JsonFormatLogicItem
                {
                    Id = location.ID,
                    RequiredItems = [$"available{{{location.COUPLEDLOCATION}}}"],
                    ConditionalItems = CreateConditionals(location.VALIDSETTINGS, logicDictionary)
                });
            }
        }

        static List<List<string>> CreateConditionals(int[] ValidSettingIndexes, LogicDictionary logicDictionary)
        {
            var SettingNames = ValidSettingIndexes.Select(x => logicDictionary.ChoiceOptions["victory_condition"].ValueList.Keys.ToArray()[x]);

            List<List<string>> Result = [];
            foreach (var setting in SettingNames)
            {
                Result.Add([$"setting{{victory_condition, {setting}}}"]);
            }
            return Result;
        }
    }
}
