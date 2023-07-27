using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.datamodel;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.OOTMMUtil;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.FunctionParsing;

namespace MMR_Tracker_V3.OtherGames.OOTMMV2
{
    public static class GenData
    {
        public static LogicStringParser OOTMMLogicStringParser = new LogicStringParser();
        public static OOTMMParserData OTTMMPaths = new OOTMMParserData();
        public static void ReadData(out MMRData.LogicFile OutLogic, out LogicDictionaryData.LogicDictionary outDict)
        {
            //CodePaths

            OTTMMPaths.MMRTrackerCodePath = Path.Combine(References.TestingPaths.GetDevCodePath(), "MMR Tracker V3");
            OTTMMPaths.OOTMMV2CodeFolder = Path.Combine(OTTMMPaths.MMRTrackerCodePath, "OtherGames", "OOTMMV2");
            OTTMMPaths.SettingsFile = Path.Combine(OTTMMPaths.OOTMMV2CodeFolder, "settings.json");
            OTTMMPaths.TricksFile = Path.Combine(OTTMMPaths.OOTMMV2CodeFolder, "tricks.json");
            OTTMMPaths.ItemsFile = Path.Combine(OTTMMPaths.OOTMMV2CodeFolder, "items.json");
            OTTMMPaths.ItemNamesFile = Path.Combine(OTTMMPaths.OOTMMV2CodeFolder, "ItemNames.json");
            OTTMMPaths.RegionNamesFile = Path.Combine(OTTMMPaths.OOTMMV2CodeFolder, "RegionNames.json");
            
            //Shared Data
            OTTMMPaths.OOTMMCorePath = Path.Combine(References.TestingPaths.GetDevTestingPath(), "OoTMM-develop", "packages", "core");
            OTTMMPaths.OOTMMTestingFolder = Path.Combine(References.TestingPaths.GetDevTestingPath(), "OOTMMTesting");

            //OOT Data Paths
            OTTMMPaths.OOTData = Path.Combine(OTTMMPaths.OOTMMCorePath, "data", "oot");
            OTTMMPaths.OOTWorld = Path.Combine(OTTMMPaths.OOTData, "world");
            OTTMMPaths.OOTMQWorld = Path.Combine(OTTMMPaths.OOTData, "world_mq");

            //OOT Data Files
            OTTMMPaths.OOTEntrancesFile = Path.Combine(OTTMMPaths.OOTData, "entrances.csv");
            OTTMMPaths.OOTMacroFile = Path.Combine(OTTMMPaths.OOTData, "macros.yml");
            OTTMMPaths.OOTPoolFile = Path.Combine(OTTMMPaths.OOTData, "pool.csv");
            OTTMMPaths.OOTHintFile = Path.Combine(OTTMMPaths.OOTData, "hints.csv");

            //MM Data
            OTTMMPaths.MMData = Path.Combine(OTTMMPaths.OOTMMCorePath, "data", "mm");
            OTTMMPaths.MMWorld = Path.Combine(OTTMMPaths.MMData, "world");

            //MM Data Files
            OTTMMPaths.MMEntrancesFile = Path.Combine(OTTMMPaths.MMData, "entrances.csv");
            OTTMMPaths.MMMacroFile = Path.Combine(OTTMMPaths.MMData, "macros.yml");
            OTTMMPaths.MMPoolFile = Path.Combine(OTTMMPaths.MMData, "pool.csv");
            OTTMMPaths.MMHintFile = Path.Combine(OTTMMPaths.MMData, "hints.csv");

            //Shared Data Files
            OTTMMPaths.SHAREDMacroFile = Path.Combine(OTTMMPaths.OOTMMCorePath, "data", "macros.yml");

            MMRData.LogicFile LogicFile = LogicFileCreation.ReadAndParseLogicFile(OTTMMPaths);
            LogicDictionaryData.LogicDictionary DictionaryFile = LogicDictionaryCreation.CreateDictionary(OTTMMPaths);

            SettingsCreation.CreateSettings(DictionaryFile, OTTMMPaths);

            AddWalletMactros(LogicFile, DictionaryFile);

            AddAdditionalData(LogicFile, DictionaryFile, OTTMMPaths);

            ParseLogicFunctions(LogicFile);

            File.WriteAllText(Path.Combine(OTTMMPaths.OOTMMTestingFolder,"LogicFile.json"), JsonConvert.SerializeObject(LogicFile, Testing._NewtonsoftJsonSerializerOptions));
            File.WriteAllText(Path.Combine(OTTMMPaths.OOTMMTestingFolder, "DictionaryFile.json"), JsonConvert.SerializeObject(DictionaryFile, Testing._NewtonsoftJsonSerializerOptions));
            EvalLogicEntryTypes(LogicFile);

            OutLogic = LogicFile;
            outDict = DictionaryFile;
        }

        private static void EvalLogicEntryTypes(MMRData.LogicFile LogicFile)
        {
            List<string> Functions = new List<string>();
            foreach (string Entry in LogicFile.Logic.SelectMany(x => x.ConditionalItems.SelectMany(x => x)))
            {
                if (LogicEditing.IsLogicFunction(Entry, out string Func, out _, new('(', ')')) && !Functions.Contains(Func))
                {
                    Functions.Add(Func);
                }
            }
            Debug.WriteLine(JsonConvert.SerializeObject(Functions, Formatting.Indented));
        }

        private static void AddWalletMactros(MMRData.LogicFile logicFile, LogicDictionaryData.LogicDictionary DictionaryFile)
        {
            var Costs = new List<int> { 0, 99, 200, 500, 999, 9999 };
            foreach (var Cost in Costs)
            {
                AddCostMacro(Cost, "MM");
                AddCostMacro(Cost, "OOT");
            }
            void AddCostMacro(int Cost, string Gamecode)
            {
                var Logic = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, $"can_use_wallet({Costs.IndexOf(Cost)})");
                string ID = $"{Gamecode}_COST_{Cost}";
                logicFile.Logic.Add(new MMRData.JsonFormatLogicItem { Id = ID, ConditionalItems = Logic });
                DictionaryFile.MacroList.Add(ID, new LogicDictionaryData.DictionaryMacroEntry
                {
                    ID = ID,
                    WalletCapacity = Cost,
                    WalletCurrency = Gamecode[0]
                });
            }
        }

        private static void AddAdditionalData(MMRData.LogicFile LogicFile, LogicDictionaryData.LogicDictionary DictionaryFile, OOTMMParserData OTTMMPaths)
        {
            //This event doesn't exist but logic for magic is asking for it. Just make it always false.
            DictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "MM_EVENT_CHATEAU", RequiredItems = new List<string>() { "false" } });

            var MASKS_REGULAR = new string[] {
              "MM_MASK_CAPTAIN",
              "MM_MASK_GIANT",
              "MM_MASK_ALL_NIGHT",
              "MM_MASK_BUNNY",
              "MM_MASK_KEATON",
              "MM_MASK_GARO",
              "MM_MASK_ROMANI",
              "MM_MASK_TROUPE_LEADER",
              "MM_MASK_POSTMAN",
              "MM_MASK_COUPLE",
              "MM_MASK_GREAT_FAIRY",
              "MM_MASK_GIBDO",
              "MM_MASK_DON_GERO",
              "MM_MASK_KAMARO",
              "MM_MASK_TRUTH",
              "MM_MASK_STONE",
              "MM_MASK_BREMEN",
              "MM_MASK_BLAST",
              "MM_MASK_SCENTS",
              "MM_MASK_KAFEI",
              "SHARED_MASK_TRUTH",
              "SHARED_MASK_BUNNY",
              "SHARED_MASK_KEATON",
            };
            var MASKS_OOT = new string[]{
              "OOT_MASK_SKULL",
              "OOT_MASK_SPOOKY",
              "OOT_MASK_KEATON",
              "OOT_MASK_BUNNY",
              "OOT_MASK_TRUTH",
              "OOT_MASK_GERUDO",
              "OOT_MASK_GORON",
              "OOT_MASK_ZORA",
              "SHARED_MASK_KEATON",
              "SHARED_MASK_BUNNY",
              "SHARED_MASK_TRUTH",
              "SHARED_MASK_GORON",
              "SHARED_MASK_ZORA",
            };
            var STONES = new string[]{
              "OOT_STONE_EMERALD",
              "OOT_STONE_RUBY",
              "OOT_STONE_SAPPHIRE",
            };

            var MEDALLIONS = new string[]{
              "OOT_MEDALLION_LIGHT",
              "OOT_MEDALLION_FOREST",
              "OOT_MEDALLION_FIRE",
              "OOT_MEDALLION_WATER",
              "OOT_MEDALLION_SPIRIT",
              "OOT_MEDALLION_SHADOW",
            };

            var REMAINS = new string[]{
              "MM_REMAINS_ODOLWA",
              "MM_REMAINS_GOHT",
              "MM_REMAINS_GYORG",
              "MM_REMAINS_TWINMOLD",
            };
            var DUNGEON_REWARDS = STONES.Concat(MEDALLIONS).Concat(REMAINS).ToArray();

            OptionData.TrackerVar MMMaskVar = new OptionData.TrackerVar
            {
                ID = "MM_MASKS",
                Static = true,
                Value = MASKS_REGULAR.Select(x => $"{x}, 1").ToList(),
            };
            DictionaryFile.Variables.Add("MM_MASKS", MMMaskVar);

        }
    }
}
