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

    }
}
