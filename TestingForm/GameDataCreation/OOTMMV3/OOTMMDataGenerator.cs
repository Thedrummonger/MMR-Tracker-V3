﻿using MMR_Tracker_V3;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System.Diagnostics;
using TDMUtils;
using static MMR_Tracker_V3.Logic.LogicUtilities;
using static MMR_Tracker_V3.TrackerObjects.MMRData;
using static TestingForm.GameDataCreation.OOTMMV3.OOTMMDataClasses;
using static TestingForm.GameDataCreation.OOTMMV3.OOTMMUtility;

namespace TestingForm.GameDataCreation.OOTMMV3
{
    public class OOTMMExtraFunctions
    {
        public static void WriteAreaFile()
        {
            var OOTMMDict = DataFileUtilities.LoadObjectFromFileOrDefault<LogicDictionaryData.LogicDictionary>
                (Path.Join(References.Globalpaths.BaseDictionaryPath, "OOTMM V3.json"));

            Dictionary<string, string> AreaDict = [];
            foreach(var i in OOTMMDict.LocationList.Values)
            {
                AreaDict[i.ID] = i.Area;
            }
            File.WriteAllText(OOTMMPaths.AreaFile, AreaDict.ToFormattedJson());
        }
    }
    public class OOTMMDataGenerator
    {
        public LogicStringParser LogicStringParser = new();
        public MMRData.LogicFile LogicFile = new() { GameCode = "OOTMM", Logic = [], Version = 3 };
        public LogicDictionaryData.LogicDictionary dictionary = new LogicDictionaryData.LogicDictionary()
        {
            RootAreas = ["MENU"],
            WinCondition = "Game_Clear",
            GameCode = "OOTMM",
            LogicVersion = 3,
            SpoilerLogInstructions = new()
            {
                ParserPath = "OOTMM",
                FileImports = new() { { "SpoilerLog", ("OOTMM Spoiler Log", ["txt"]) } }
            }
        };
        public ExtraData extraData;
        public void GenerateOOTMMData()
        {
            extraData = JsonConvert.DeserializeObject<ExtraData>(File.ReadAllText(OOTMMPaths.ExtraDataFile));

            OOTMMLogicFileParsing LogicParsing = new(this);
            OOTMMDictionaryCreation DictCreation = new(this);
            OOTMMLogicSettingCreation SettingCreation = new(this);

            LogicParsing.ReadAndParseWorldFiles();
            LogicParsing.AddEntriesToLogicFile();
            LogicParsing.ParseDownFunctions();
            LogicParsing.AddMissingLogic();
            LogicParsing.AddMissingGameCodes();
            LogicParsing.HandleNegations();

            DictCreation.CreateDictItems();
            DictCreation.CreateDictLocations();
            DictCreation.CreateDictHints();
            DictCreation.CreateDictEntrances();

            SettingCreation.AddAgeLogic();
            SettingCreation.CreateMiscLogicItemCollections();
            SettingCreation.CreateSettings();
            SettingCreation.WorldEventRequirementOptions();
            SettingCreation.AddMasterQuestDungeons();

            AddGameClearChecks();
            HandleTingle();
            AddLogicTricks();
            FixAreaClearLogic();
            HandleSingleGameEntranceLogic();
            //SettingCreation.CleanSettings();
            CleanLogic();
            FixAreas();

            File.WriteAllText(Path.Combine(TestingReferences.GetTestingLogicPresetsPath(), "DEV-OOTMM Casual.json"), LogicFile.ToFormattedJson());
            File.WriteAllText(Path.Combine(TestingReferences.GetTestingDictionaryPath(), "OOTMM V3.json"), dictionary.ToFormattedJson());
        }

        private void HandleSingleGameEntranceLogic()
        {
            var MenuToOOT = "MENU => OOT SPAWN";
            var MenuToMM = "MENU => MM Clock Town";
            LogicFile.Logic.Add(new JsonFormatLogicItem { Id = MenuToOOT, RequiredItems = ["setting{games, mm, false}"] });
            LogicFile.Logic.Add(new JsonFormatLogicItem { Id = MenuToMM, RequiredItems = ["setting{games, mm}"] });
            dictionary.EntranceList.Add(MenuToOOT, new LogicDictionaryData.DictionaryEntranceEntries
            {
                Area = MenuToOOT.TrimSplit("=>")[0],
                Exit = MenuToOOT.TrimSplit("=>")[1],
                ID = MenuToOOT,
                RandomizableEntrance = false
            });
            dictionary.EntranceList.Add(MenuToMM, new LogicDictionaryData.DictionaryEntranceEntries
            {
                Area = MenuToMM.TrimSplit("=>")[0],
                Exit = MenuToMM.TrimSplit("=>")[1],
                ID = MenuToMM,
                RandomizableEntrance = false
            });
            foreach(var entry in dictionary.EntranceList)
            {
                if (!HasGamecode(entry.Value.Area)) { continue; }
                var AreaGameCode = OOTMMUtility.GetGamecode(entry.Value.Area);
                var ExitGameCode = OOTMMUtility.GetGamecode(entry.Value.Exit);
                var LogicEntry = LogicFile.Logic.FirstOrDefault(x => x.Id == entry.Key);
                if (AreaGameCode != ExitGameCode && LogicEntry is not null)
                {
                    LogicEntry.RequiredItems.Add("setting{games, ootmm}");
                    Debug.WriteLine($"Exit {entry.Key} Restricted to combo randomizer");
                }
            }
        }

        public void CleanLogic()
        {
            foreach (var i in LogicFile.Logic)
            {
                RemoveRedundantConditionals(i);
                MakeCommonConditionalsRequirements(i);
                for(var ind = 0; ind < i.ConditionalItems.Count; ind++ )
                {
                    i.ConditionalItems[ind] = [.. i.ConditionalItems[ind].OrderBy(x => !LogicFunctions.IsLogicFunction(x)).ThenBy(x => x)];
                }
                i.ConditionalItems = [.. i.ConditionalItems.OrderBy(x => x.Count).ThenBy(x => string.Join(" ", x))];
            }
        }

        public void AddLogicTricks()
        {
            foreach (var i in extraData.tricks.OrderBy(x => x.Key))
            {
                addTrick(i.Key, i.Value);
            }

            void addTrick(string ID, OOTMMTrickData Trick)
            {
                bool IsGlitch = Trick.glitch is not null && (bool)Trick.glitch;
                LogicFile.Logic.Add(new MMRData.JsonFormatLogicItem
                {
                    Id = ID,
                    IsTrick = true,
                    TrickCategory = $"{Trick.game} {(IsGlitch ? "Glitches" : "Tricks")}",
                    TrickTooltip = Trick.tooltip,
                    TrickUrl = Trick.linkText??Trick.linkVideo
                });
                dictionary.MacroList.Add(ID, new LogicDictionaryData.DictionaryMacroEntry
                {
                    ID = ID,
                    Name = Trick.name
                });
            }
        }

        public void HandleTingle()
        {
            Dictionary<string, (string, string)> Data = [];
            Data.Add("MM Tingle Map Clock Town", ("MM Tingle Town", "MM Tingle Ikana"));
            Data.Add("MM Tingle Map Woodfall", ("MM Tingle Swamp", "MM Tingle Town"));
            Data.Add("MM Tingle Map Snowhead", ("MM Tingle Mountain", "MM Tingle Swamp"));
            Data.Add("MM Tingle Map Ranch", ("MM Tingle Ranch", "MM Tingle Mountain"));
            Data.Add("MM Tingle Map Great Bay", ("MM Tingle Great Bay", "MM Tingle Ranch"));
            Data.Add("MM Tingle Map Ikana", ("MM Tingle Ikana", "MM Tingle Great Bay"));

            int header = "MM Tingle ".Count();
            foreach (var i in Data)
            {
                var LogicEntry = this.LogicFile.Logic.First(x => x.Id == i.Key);
                var DictEntry = this.dictionary.LocationList[i.Key];
                LogicUtilities.MoveRequirementsToConditionals(LogicEntry);
                var Set1 = LogicEntry.ConditionalItems.Where(x => x.Contains(i.Value.Item1)).ToList();
                var Set2 = LogicEntry.ConditionalItems.Where(x => x.Contains(i.Value.Item2)).ToList();

                var NewLogic1 = new MMRData.JsonFormatLogicItem() { Id = $"{i.Key} in {i.Value.Item1[header..]}", ConditionalItems = Set1 };
                var NewLogic2 = new MMRData.JsonFormatLogicItem() { Id = $"{i.Key} in {i.Value.Item2[header..]}", ConditionalItems = Set2 };
                LogicFile.Logic.Add(NewLogic1);
                LogicFile.Logic.Add(NewLogic2);
                LogicEntry.ConditionalItems.Clear();
                LogicEntry.ConditionalItems = [[NewLogic1.Id], [NewLogic2.Id]];

                TransformLogicItems(NewLogic1, x => x.StartsWith($"price{{{i.Key}") ? x.Replace($"price{{{i.Key}", $"price{{{NewLogic1.Id}") : x);
                TransformLogicItems(NewLogic2, x => x.StartsWith($"price{{{i.Key}") ? x.Replace($"price{{{i.Key}", $"price{{{NewLogic2.Id}") : x);

                DictEntry.LocationProxys.Add(new LogicDictionaryData.DictLocationProxy
                {
                    ID = $"{NewLogic1.Id} Proxy",
                    Area = "Tingle Maps",
                    LogicInheritance = NewLogic1.Id,
                    Name = $"{i.Key} (in {i.Value.Item1[header..]})"
                });
                DictEntry.LocationProxys.Add(new LogicDictionaryData.DictLocationProxy
                {
                    ID = $"{NewLogic2.Id} Proxy",
                    Area = "Tingle Maps",
                    LogicInheritance = NewLogic2.Id,
                    Name = $"{i.Key} (in {i.Value.Item2[header..]})"
                });
            }

        }

        private void AddGameClearChecks()
        {
            dictionary.LocationList.Add("MM_BOSS_MAJORA", new LogicDictionaryData.DictionaryLocationEntries
            {
                ID = "MM_BOSS_MAJORA",
                Name = "Majora",
                Area = "The Moon",
                OriginalItem = "MM_MASK_MAJORA",
                ValidItemTypes = ["Majora"],
                SpoilerData = new MMRData.SpoilerlogReference()
            });
            LogicFile.Logic.Add(new MMRData.JsonFormatLogicItem
            {
                Id = "MM_BOSS_MAJORA",
                RequiredItems = ["MM_MAJORA_EVENT"]
            });

            dictionary.ItemList.Add("MM_MASK_MAJORA", new LogicDictionaryData.DictionaryItemEntries
            {
                ID = "MM_MASK_MAJORA",
                Name = "Majoras Mask",
                ValidStartingItem = false,
                MaxAmountInWorld = 1,
                ItemTypes = ["Majora"],
                SpoilerData = new MMRData.SpoilerlogReference()
            });

            dictionary.LocationList.Add("OOT_BOSS_GANON", new LogicDictionaryData.DictionaryLocationEntries
            {
                ID = "OOT_BOSS_GANON",
                Name = "Ganon",
                Area = "Ganon's Castle",
                OriginalItem = "OOT_TRIFORCE",
                ValidItemTypes = ["Ganon"],
                SpoilerData = new MMRData.SpoilerlogReference()
            });

            LogicFile.Logic.Add(new MMRData.JsonFormatLogicItem
            {
                Id = "OOT_BOSS_GANON",
                RequiredItems = ["OOT_GANON_EVENT"]
            });

            dictionary.ItemList.Add("OOT_TRIFORCE", new LogicDictionaryData.DictionaryItemEntries
            {
                ID = "OOT_TRIFORCE",
                Name = "Triforce",
                ValidStartingItem = false,
                MaxAmountInWorld = 1,
                ItemTypes = ["Ganon"],
                SpoilerData = new MMRData.SpoilerlogReference()
            });

            //Game Clear

            string GameClearLogic =
                "(setting{goal, any} && (OOT_TRIFORCE || MM_MASK_MAJORA)) || " +
                "(setting{goal, both} && OOT_TRIFORCE && MM_MASK_MAJORA) || " +
                "(setting{goal, ganon} && OOT_TRIFORCE) || " +
                "(setting{goal, majora} && MM_MASK_MAJORA) || " +
                "(setting{goal, triforce} && SHARED_TRIFORCE, triforceGoal) || " +
                "(setting{goal, triforce3} && SHARED_TRIFORCE_POWER && SHARED_TRIFORCE_COURAGE && SHARED_TRIFORCE_WISDOM)";

            LogicFile.Logic.Add(new MMRData.JsonFormatLogicItem { Id = "Game_Clear", ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(LogicStringParser, GameClearLogic, "Game_Clear") });
            dictionary.MacroList.Add("Game_Clear", new LogicDictionaryData.DictionaryMacroEntry { ID = "Game_Clear", Name = "Both Games Cleared" });
            dictionary.WinCondition = "Game_Clear";
        }

        public void FixAreaClearLogic()
        {
            var MM_BOSS_GREAT_BAY = LogicFile.Logic.First(x => x.Id == "MM_BOSS_GREAT_BAY_EVENT");
            var MM_BOSS_SNOWHEAD = LogicFile.Logic.First(x => x.Id == "MM_BOSS_SNOWHEAD_EVENT");
            var MM_BOSS_WOODFALL = LogicFile.Logic.First(x => x.Id == "MM_BOSS_WOODFALL_EVENT");
            var OOT_EVENT_WATER_TEMPLE_CLEARED = LogicFile.Logic.First(x => x.Id == "OOT_WATER_TEMPLE_CLEARED_EVENT");

            Dictionary<string, string> RandoAreaClear = new(){
                  {"OOT Deku Tree Boss", "OOT Deku Tree After Boss"},
                  {"OOT Dodongo Cavern Boss", "OOT Dodongo Cavern After Boss"},
                  {"OOT Jabu-Jabu Boss", "OOT Jabu-Jabu After Boss"},
                  {"OOT Forest Temple Boss", "OOT Forest Temple After Boss"},
                  {"OOT Fire Temple Boss", "OOT Fire Temple After Boss"},
                  {"OOT Water Temple Boss", "OOT Water Temple After Boss"},
                  {"OOT Spirit Temple Boss", "OOT Spirit Temple After Boss"},
                  {"OOT Shadow Temple Boss", "OOT Shadow Temple After Boss"},
                  {"MM Woodfall Temple Boss", "MM Woodfall Temple After Boss"},
                  {"MM Great Bay Temple Boss", "MM Great Bay Temple After Boss"},
                  {"MM Snowhead Temple Boss", "MM Snowhead Temple After Boss"},
                  {"MM Stone Tower Temple Boss", "MM Stone Tower After Boss"}
            };

            CreateLogic(MM_BOSS_GREAT_BAY, "MM Great Bay Temple Boss Access", "MM Great Bay Temple Boss");
            CreateLogic(MM_BOSS_SNOWHEAD, "MM Snowhead Temple Boss Access", "MM Snowhead Temple Boss");
            CreateLogic(MM_BOSS_WOODFALL, "MM Woodfall Temple Boss Access", "MM Woodfall Temple Boss");
            CreateLogic(OOT_EVENT_WATER_TEMPLE_CLEARED, "OOT Water Temple Antichamber", "OOT Water Temple Boss");

            void CreateLogic(MMRData.JsonFormatLogicItem Item, string DungeonArea, string BossDoor)
            {
                Item.RequiredItems = new List<string>();
                Item.ConditionalItems = new List<List<string>>();
                foreach (var i in RandoAreaClear)
                {
                    Item.ConditionalItems.Add([$"contains{{{DungeonArea} => {BossDoor}, {i.Key}}}", $"{i.Value}"]);
                }
            }
        }

        public void FixAreas()
        {
            Dictionary<string, string> Areas = [];
            var VanillaSpoiler = Path.Combine(OOTMMPaths.OOTMMTestingFolderPath, "AreaSpoilers", "AreaListVanilla.txt");
            var MQSpoiler = Path.Combine(OOTMMPaths.OOTMMTestingFolderPath, "AreaSpoilers", "AreaListMQ.txt");
            if (File.Exists(OOTMMPaths.AreaFile))
            {
                foreach (var l in DataFileUtilities.LoadObjectFromFileOrDefault<Dictionary<string, string>>(OOTMMPaths.AreaFile))
                {
                    Areas[l.Key] = l.Value;
                }
            }
            if (File.Exists(VanillaSpoiler)) { Read(File.ReadAllLines(VanillaSpoiler)); }
            if (File.Exists(MQSpoiler)) { Read(File.ReadAllLines(MQSpoiler)); }

            void Read(string[] VanillaSPL)
            {
                bool AtData = false;
                string CurrentArea = "";
                foreach (var i in VanillaSPL)
                {
                    if (string.IsNullOrWhiteSpace(i)) { continue; }
                    if (i.StartsWith("Location List (")) { AtData = true; continue; }
                    if (!AtData) { continue; }
                    if (i.EndsWith(':'))
                    {
                        CurrentArea = i.TrimSplit("(")[0];
                        continue;
                    }
                    var data = i.TrimSplit(":");
                    Areas[data[0]] = CurrentArea;
                }
            }

            Debug.WriteLine($"Missing Area Locations:");
            foreach (var l in dictionary.LocationList.Values)
            {
                if (!Areas.ContainsKey(l.ID))
                {
                    Debug.WriteLine(l.ID);
                    continue;
                }
                l.Area = Areas[l.ID];
            }
        }
    }
}
