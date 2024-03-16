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
    internal class OOTMMDataGenerator
    {
        public Dictionary<string, OOTMMLocationEntry> LogicEntries = [];
        public Dictionary<string, OOTMMLogicFunction> MacroFunctions = [];
        public LogicStringParser LogicStringParser = new();
        public MMRData.LogicFile LogicFile = new() { GameCode = "OOTMM", Logic = [], Version = 3 };
        public LogicDictionaryData.LogicDictionary dictionary = new LogicDictionaryData.LogicDictionary()
        {
            RootArea = "OOT SPAWN",
            WinCondition = "Game_Clear",
            GameCode = "OOTMM",
            LogicVersion = 3
        };
        public void GenerateOOTMMData()
        {
            OOTMMLogicFileParsing LogicParsing = new(this);
            LogicParsing.ReadLogicFiles();

            foreach (var i in LogicEntries)
            {
                LogicFile.Logic.Add(new MMRData.JsonFormatLogicItem
                {
                    Id = i.Key,
                    ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(LogicStringParser, i.Value.Logic, i.Key)
                });
                RemoveRedundantConditionals(LogicFile.Logic.Last());
            }
            LogicParsing.ParseDownFunctions();
            LogicParsing.AddMissingGameCodes();

            foreach(var i in LogicFile.Logic)
            {
                LogicUtilities.RemoveRedundantConditionals(i);
                LogicUtilities.MakeCommonConditionalsRequirements(i);
            }

            

            ExtraData extraData = JsonConvert.DeserializeObject<ExtraData>(File.ReadAllText(OOTMMPaths.ExtraDataFile));


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

            List<OOTMMPoolLocation> OOTPool = DeserializeCSVFile<List<OOTMMPoolLocation>>(OOTMMPaths.OOTPoolFile);
            List<OOTMMPoolLocation> MMPool = DeserializeCSVFile<List<OOTMMPoolLocation>>(OOTMMPaths.MMPoolFile);
            Dictionary<string, OOTMMEntranceFileData> Entrances = DeserializeYAMLFile<Dictionary<string, OOTMMEntranceFileData>>(OOTMMPaths.EntranceFile);

            foreach (var item in extraData.items.Distinct())
            {
                LogicDictionaryData.DictionaryItemEntries ItemEntry = new LogicDictionaryData.DictionaryItemEntries()
                {
                    ID = item,
                    ItemTypes = ["item"],
                    Name = extraData.names[item],
                    ValidStartingItem = true,
                    MaxAmountInWorld = -1,
                    SpoilerData = new MMRData.SpoilerlogReference() { SpoilerLogNames = [item, extraData.names[item]] }
                };
                dictionary.ItemList.Add(ItemEntry.ID, ItemEntry);
            }

            foreach (var location in OOTPool)
            {
                AddPoolLocation(location, "OOT");
            }
            foreach (var location in MMPool)
            {
                AddPoolLocation(location, "MM");
            }

            foreach (var logiEntry in LogicFile.Logic.Select(x => x.Id))
            {
                if (!logiEntry.Contains("=>")) { continue; }
                var Data = logiEntry.TrimSplit("=>");
                string Area = Data[0];
                string Destination = Data[1];
                var EntData = Entrances.FirstOrDefault(x => x.Value.areas is not null && x.Value.areas[0] == Area && x.Value.areas[1] == Destination);
                bool IsRandomizable = !EntData.Equals(default(KeyValuePair<string, OOTMMEntranceFileData>));
                LogicDictionaryData.DictionaryEntranceEntries EntranceEntry = new LogicDictionaryData.DictionaryEntranceEntries()
                {
                    Area = Area,
                    Exit = Destination,
                    EntrancePairID = IsRandomizable && !string.IsNullOrWhiteSpace(EntData.Value.reverse) ? new EntranceData.EntranceAreaPair
                    {
                        Area = Entrances[EntData.Value.reverse].areas[0],
                        Exit = Entrances[EntData.Value.reverse].areas[1]
                    } : null,
                    ID = logiEntry,
                    RandomizableEntrance = IsRandomizable,
                    SpoilerData = new MMRData.SpoilerlogReference { 
                        SpoilerLogNames = IsRandomizable ? [EntData.Key] : [],
                        Tags = IsRandomizable ? [EntData.Value.type] : [],
                    }
                };
                dictionary.EntranceList.Add(EntranceEntry.ID, EntranceEntry);
            }



            File.WriteAllText(Path.Combine(OOTMMPaths.OOTMMTestingFolderPath, "OOTMMLogicV3.json"), LogicFile.ToFormattedJson());
            File.WriteAllText(Path.Combine(OOTMMPaths.OOTMMTestingFolderPath, "OOTMMDictV3.json"), dictionary.ToFormattedJson());

            void AddPoolLocation(OOTMMPoolLocation location, string GameCode)
            {
                string ID = OOTMMUtility.AddGameCodeToLogicID(location.location, GameCode, false);
                LogicDictionaryData.DictionaryLocationEntries locationEntries = new LogicDictionaryData.DictionaryLocationEntries()
                {
                    ID = ID,
                    Name = ID,
                    Area = location.scene,
                    OriginalItem = AddGameCodeToLogicID(location.item, GameCode),
                    Repeatable = OOTMMUtility.IsLocationRenewable(location, GameCode, extraData),
                    ValidItemTypes = ["item"],
                    SpoilerData = new MMRData.SpoilerlogReference()
                    {
                        SpoilerLogNames = [ID],
                        Tags = [location.scene, location.type],
                        GossipHintNames = [location.hint]
                    }
                };
                if (locationEntries.OriginalItem.In("OOT_FLEXIBLE", "OOT_RANDOM")) { locationEntries.OriginalItem = AddGameCodeToLogicID("RUPEE_GREEN", GameCode); }
                if (locationEntries.OriginalItem.In("MM_???", "MM_RANDOM")) { locationEntries.OriginalItem = AddGameCodeToLogicID("RUPEE_GREEN", GameCode); }
                if (locationEntries.OriginalItem.In("MM_ARROWS_20")) { locationEntries.OriginalItem = AddGameCodeToLogicID("ARROWS_30", GameCode); }
                if (locationEntries.OriginalItem.In("OOT_NOTHING", "MM_NOTHING")) { locationEntries.OriginalItem = "NOTHING"; }
                dictionary.LocationList.Add(locationEntries.ID, locationEntries);
            }
        }

    }
}
