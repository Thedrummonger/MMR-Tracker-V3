using MathNet.Symbolics;
using MMR_Tracker_V3;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using static MMR_Tracker_V3.Logic.LogicUtilities;
using static MMR_Tracker_V3.TrackerObjects.MMRData;
using static TestingForm.GameDataCreation.OOTMMV3.OOTMMDataClasses;
using static TestingForm.GameDataCreation.OOTMMV3.OOTMMUtility;

namespace TestingForm.GameDataCreation.OOTMMV3
{
    public class OOTMMDataGenerator
    {
        public LogicStringParser LogicStringParser = new();
        public MMRData.LogicFile LogicFile = new() { GameCode = "OOTMM", Logic = [], Version = 3 };
        public LogicDictionaryData.LogicDictionary dictionary = new LogicDictionaryData.LogicDictionary()
        {
            RootArea = "OOT SPAWN",
            WinCondition = "Game_Clear",
            GameCode = "OOTMM",
            LogicVersion = 3
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
            DictCreation.CreateDictEntrances();

            SettingCreation.AddAgeLogic();
            SettingCreation.CreateMiscLogicItemCollections();
            SettingCreation.CreateSettings();
            SettingCreation.WorldEventRequirementOptions();
            SettingCreation.AddMasterQuestDungeons();

            HandleTingle();
            AddLogicTricks();
            CleanLogic();

            File.WriteAllText(Path.Combine(OOTMMPaths.OOTMMTestingFolderPath, "OOTMMLogicV3.json"), LogicFile.ToFormattedJson());
            File.WriteAllText(Path.Combine(OOTMMPaths.OOTMMTestingFolderPath, "OOTMMDictV3.json"), dictionary.ToFormattedJson());
        }

        public void CleanLogic()
        {
            foreach (var i in LogicFile.Logic)
            {
                RemoveRedundantConditionals(i);
                MakeCommonConditionalsRequirements(i);
            }
        }

        public void AddLogicTricks()
        {
            foreach (var i in extraData.tricks.OrderBy(x => x.Value))
            {
                addTrick(i.Key, i.Value, "Tricks");
            }
            foreach (var i in extraData.glitches.OrderBy(x => x.Value))
            {
                addTrick(i.Key, i.Value, "Glitches");
            }

            void addTrick(string ID, string Name, string Type)
            {
                LogicFile.Logic.Add(new MMRData.JsonFormatLogicItem
                {
                    Id = ID,
                    IsTrick = true,
                    TrickCategory = $"{GetGamecode(ID)} {Type}"
                });
                dictionary.MacroList.Add(ID, new LogicDictionaryData.DictionaryMacroEntry
                {
                    ID = ID,
                    Name = Name
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

    }
}
