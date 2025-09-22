using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMUtils;

namespace TestingForm.GameDataCreation.Pikmin2
{
    internal class GenerateData
    {
        public class LocationEntry
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public List<string> Requirements { get; set; } = new();
        }
        public class AreaMappings
        {
            public Dictionary<string, string> AbbrevToName { get; set; } = new();
            public Dictionary<string, string> NameToAbbrev { get; set; } = new();
        }
        public class EntranceInfo
        {
            public List<string> Requirements { get; set; } = new();
            public string RootArea { get; set; } = string.Empty;
        }
        public static void ReadAndParse(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary)
        {
            var Locations = DataFileUtilities.DeserializeJsonFile<LocationEntry[]>(Path.Combine(TestingReferences.GetOtherGameDataPath("Pikmin2"), "Locations.json"));
            var Areas = DataFileUtilities.DeserializeJsonFile<AreaMappings>(Path.Combine(TestingReferences.GetOtherGameDataPath("Pikmin2"), "Areas.json"));
            var Items = DataFileUtilities.DeserializeJsonFile<Dictionary<string, int>>(Path.Combine(TestingReferences.GetOtherGameDataPath("Pikmin2"), "Items.json"));
            var Entrances = DataFileUtilities.DeserializeJsonFile<Dictionary<string, EntranceInfo>>(Path.Combine(TestingReferences.GetOtherGameDataPath("Pikmin2"), "Entrances.json"));

            LogicStringParser LogicParser = new();

            MMRData.LogicFile logicFile = new MMRData.LogicFile()
            {
                GameCode = "PIK2",
                Logic = [],
                Version = 1,
            };

            LogicDictionaryData.LogicDictionary Logicdictionary = new()
            {
                RootAreas = ["Menu"],
                GameCode = "PIK2",
                LogicVersion = 1,
                WinCondition = "defeated_bosses",
                NetPlaySupported = true,
                GameName = "Pikmin 2",
                SpoilerLogInstructions = new()
                {
                    ArchipelagoParserPath = "PIK2",
                    ArchipelagoFileImports = new() { { "APPatchFile", ("Archipelago Pikmin 2 Patch File", ["appik2"]) } }
                }
            };

            Logicdictionary.ChoiceOptions.Add("win_condition", new OptionData.ChoiceOption(null)
            {
                ID = "win_condition",
                Name = "Win Condition",
                Value = "collect_louie"
            }.CreateSimpleValues(("collect_louie", "Collect Louie"), ("collect_pokos", "Collect Pokos"), ("collect_treasure", "Collect Treasure")));

            Logicdictionary.ChoiceOptions.Add("shuffle_caves", new OptionData.ChoiceOption(null)
            {
                ID = "shuffle_caves",
                Name = "Shuffle Caves",
                Value = "no_shuffle"
            }.CreateSimpleValues(("no_shuffle", "No Shuffle"), ("keep_dream_den", "Ignore Dream Den"), ("wistful_wild_lock", "Lock Wistful Wild"), ("full_shuffle", "Full Shuffle")));

            Logicdictionary.ChoiceOptions.Add("cave_keys", new OptionData.ChoiceOption(null)
            {
                ID = "cave_keys",
                Name = "Cave Keys",
                Value = "all_unlocked"
            }.CreateSimpleValues(("all_unlocked", "Unlocked"), ("all_locked", "Locked")));

            Logicdictionary.ChoiceOptions.Add("boss_rando", new OptionData.ChoiceOption(null)
            {
                ID = "boss_rando",
                Name = "Boss Rando",
                Value = "vanilla"
            }.CreateSimpleValues(("vanilla", "Vanilla"), ("shuffle", "Shuffle"), ("full_random", "Full Random")));

            Logicdictionary.ChoiceOptions.Add("onion_shuffle", new OptionData.ChoiceOption(null)
            {
                ID = "onion_shuffle",
                Name = "Onion Shuffle",
                Value = "vanilla"
            }.CreateSimpleValues(("vanilla", "Vanilla"), ("shuffle", "Shuffle"), ("exclude_vanilla", "Exclude Vanilla"), ("in_pool", "In Pool")));

            Logicdictionary.ChoiceOptions.Add("progressive_globes", new OptionData.ChoiceOption(null)
            {
                ID = "progressive_globes",
                Name = "Progressive Globes",
                Value = "non_progressive"
            }.CreateSimpleValues(("non_progressive", "Non Progressive"), ("progressive", "Progressive")));

            Logicdictionary.ChoiceOptions.Add("weapons_in_pool", new OptionData.ChoiceOption(null)
            {
                ID = "weapons_in_pool",
                Name = "Weapons in Pool",
                Value = "no"
            }.CreateSimpleValues(("no", "No"), ("yes", "Yes")));

            Logicdictionary.IntOptions.Add("poko_amount", new OptionData.IntOption(null)
            {
                ID = "poko_amount",
                Name = "Poko Amount",
                Min = 0,
                Max = 26985,
                Value = 10000,
                Conditionals = [["setting{win_condition, collect_pokos}"]]
            });

            Logicdictionary.IntOptions.Add("treasure_amount", new OptionData.IntOption(null)
            {
                ID = "treasure_amount",
                Name = "Treasure Amount",
                Min = 0,
                Max = 201,
                Value = 201,
                Conditionals = [["setting{win_condition, collect_treasure}"]]
            });

            Logicdictionary.IntOptions.Add("debt", new OptionData.IntOption(null)
            {
                ID = "debt",
                Name = "Debt",
                Min = 0,
                Max = 15000,
                Value = 10000
            });

            MMRData.JsonFormatLogicItem MenuToVORLogic = new MMRData.JsonFormatLogicItem() { Id = "Menu => VoR" };
            LogicDictionaryData.DictionaryEntranceEntries MenuToVORMEta = new LogicDictionaryData.DictionaryEntranceEntries()
            {
                ID = "Menu => VoR",
                Area = "Menu",
                Exit = "VoR",
                RandomizableEntrance = false
            };
            Logicdictionary.EntranceList.Add(MenuToVORMEta.ID, MenuToVORMEta);
            logicFile.Logic.Add(MenuToVORLogic);
            MMRData.JsonFormatLogicItem MenuToAWLogic = new MMRData.JsonFormatLogicItem() { 
                Id = "Menu => AW",
                ConditionalItems = [["Spherical Atlas"], ["option{progressive_globes, progressive}", "Geographic Projection"]] 
            };
            LogicDictionaryData.DictionaryEntranceEntries MenuToAWMEta = new LogicDictionaryData.DictionaryEntranceEntries()
            {
                ID = "Menu => AW",
                Area = "Menu",
                Exit = "AW",
                RandomizableEntrance = false
            };
            Logicdictionary.EntranceList.Add(MenuToAWMEta.ID, MenuToAWMEta);
            logicFile.Logic.Add(MenuToAWLogic);
            MMRData.JsonFormatLogicItem MenuToPPLogic = new MMRData.JsonFormatLogicItem()
            {
                Id = "Menu => PP",
                ConditionalItems = [["Spherical Atlas", "Geographic Projection"], ["option{progressive_globes, non_progressive}", "Geographic Projection"]]
            };
            LogicDictionaryData.DictionaryEntranceEntries MenuToPPMEta = new LogicDictionaryData.DictionaryEntranceEntries()
            {
                ID = "Menu => PP",
                Area = "Menu",
                Exit = "PP",
                RandomizableEntrance = false
            };
            Logicdictionary.EntranceList.Add(MenuToPPMEta.ID, MenuToPPMEta);
            logicFile.Logic.Add(MenuToPPLogic);
            MMRData.JsonFormatLogicItem MenuToWWLogic = new MMRData.JsonFormatLogicItem()
            {
                Id = "Menu => WW",
                RequiredItems = ["Spherical Atlas", "Geographic Projection", "Debt Paid"]
            };
            LogicDictionaryData.DictionaryEntranceEntries MenuToWWMEta = new LogicDictionaryData.DictionaryEntranceEntries()
            {
                ID = "Menu => WW",
                Area = "Menu",
                Exit = "WW",
                RandomizableEntrance = false
            };
            Logicdictionary.EntranceList.Add(MenuToWWMEta.ID, MenuToWWMEta);
            logicFile.Logic.Add(MenuToWWLogic);


            logicFile.Logic.Add(new MMRData.JsonFormatLogicItem()
            {
                Id = "Debt Paid",
                RequiredItems = ["TreasureValue, debt"]
            });

            foreach (var i in Locations)
            {
                var data = i.Name.SplitOnce(' ');
                LogicDictionaryData.DictionaryLocationEntries Entry = new LogicDictionaryData.DictionaryLocationEntries();
                Entry.Area = Areas.AbbrevToName[data.Item1];
                Entry.OriginalItem = data.Item2;
                Entry.ValidItemTypes = ["item"];
                Entry.ID = i.Name;
                Entry.SpoilerData.NetIDs = [i.Name];
                Logicdictionary.LocationList.Add(i.Name, Entry);

                MMRData.JsonFormatLogicItem LogicItem = new MMRData.JsonFormatLogicItem();
                LogicItem.Id = i.Name;
                LogicItem.RequiredItems = [.. i.Requirements.Select(x => $"has {x} pikmin"), data.Item1];
                logicFile.Logic.Add(LogicItem);
            }
            List<string> ItemWorth = [];
            foreach (var i in Items)
            {
                LogicDictionaryData.DictionaryItemEntries Item = new LogicDictionaryData.DictionaryItemEntries();
                Item.ID = i.Key;
                Item.SpoilerData.NetIDs = [i.Key];
                Item.ItemTypes = i.Key.EndsWith("Onion") ?["item", "onion"] : ["item"];
                Item.Name = i.Key;
                if (i.Key.EndsWith("key") && Areas.AbbrevToName.TryGetValue(i.Key.SplitOnce(' ').Item1, out var AreaName))
                    Item.Name = AreaName;
                Logicdictionary.ItemList.Add(i.Key, Item);

                for (var j = 0; j < i.Value; j++)
                    ItemWorth.Add(i.Key);
            }
            Logicdictionary.LogicEntryCollections.Add("TreasureValue", new OptionData.LogicEntryCollection
            {
                ID = "TreasureValue",
                Entries = [.. ItemWorth]
            });

            foreach (var item in Entrances)
            {
                string ID = $"{item.Value.RootArea} => {item.Key}";
                LogicDictionaryData.DictionaryEntranceEntries Entrance = new LogicDictionaryData.DictionaryEntranceEntries();
                Entrance.ID = ID;
                Entrance.Name = Areas.AbbrevToName[item.Key] + " Entrance";
                Entrance.Exit = item.Key;
                Entrance.DisplayExit = Areas.AbbrevToName[item.Key];
                Entrance.Area = item.Value.RootArea;
                Entrance.DisplayArea = Areas.AbbrevToName[item.Value.RootArea];
                Entrance.RandomizableEntrance = true;
                Entrance.DestinationHasSingleEntrance = true;
                Logicdictionary.EntranceList.Add(ID, Entrance);

                MMRData.JsonFormatLogicItem LogicItem = new MMRData.JsonFormatLogicItem();
                LogicItem.Id = ID;
                LogicItem.RequiredItems = item.Value.Requirements.Select(x => $"has {x} pikmin").ToList();
                LogicItem.ConditionalItems = [["setting{cave_keys, all_unlocked}"], [$"{item.Key} Entrance Key"]];
                logicFile.Logic.Add(LogicItem);
            }

            logicFile.Logic.Add(new MMRData.JsonFormatLogicItem()
            {
                Id = "has red pikmin",
                ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(LogicParser, "(setting{onion_shuffle, vanilla} && VoR) || Red Onion", "has red pikmin")
            });
            logicFile.Logic.Add(new MMRData.JsonFormatLogicItem()
            {
                Id = "has yellow pikmin",
                ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(LogicParser, "(setting{onion_shuffle, vanilla} && PP && has white pikmin) || Yellow Onion", "has yellow pikmin")
            });
            logicFile.Logic.Add(new MMRData.JsonFormatLogicItem()
            {
                Id = "has blue pikmin",
                ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(LogicParser, "(setting{onion_shuffle, vanilla} && AW && has yellow pikmin) || Blue Onion", "has blue pikmin")
            });
            logicFile.Logic.Add(new MMRData.JsonFormatLogicItem()
            {
                Id = "has white pikmin",
                ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(LogicParser, "AW", "has white pikmin")
            });
            logicFile.Logic.Add(new MMRData.JsonFormatLogicItem()
            {
                Id = "has purple pikmin",
                ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(LogicParser, "VoR", "has purple pikmin")
            });

            logicFile.Logic.Add(new()
            {
                Id = "onions shuffled",
                ConditionalItems = [["setting{onion_shuffle, exclude_vanilla}"], ["setting{onion_shuffle, shuffle}"]]
            });

            Logicdictionary.LocationList.Add("VoR Onion", new()
            {
                ID = "VoR Onion",
                Area = Areas.AbbrevToName["VoR"],
                Name = "VoR Onion",
                ValidItemTypes = ["onion"],
                OriginalItem = "Red Onion"
            });
            logicFile.Logic.Add(new()
            {
                Id = "VoR Onion",
                RequiredItems = ["VoR", "setting{onion_shuffle, vanilla, false}"]
            });

            Logicdictionary.LocationList.Add("AW Onion", new()
            {
                ID = "AW Onion",
                Area = Areas.AbbrevToName["AW"],
                Name = "AW Onion",
                ValidItemTypes = ["onion"],
                OriginalItem = "Blue Onion"
            });
            logicFile.Logic.Add(new()
            {
                Id = "AW Onion",
                RequiredItems = ["AW", "onions shuffled", "has yellow pikmin"]
            });

            Logicdictionary.LocationList.Add("PP Onion", new()
            {
                ID = "PP Onion",
                Area = Areas.AbbrevToName["PP"],
                Name = "PP Onion",
                ValidItemTypes = ["onion"],
                OriginalItem = "Yellow Onion"
            });
            logicFile.Logic.Add(new()
            {
                Id = "PP Onion",
                RequiredItems = ["PP", "onions shuffled", "has white pikmin"]
            });

            Logic = logicFile;
            dictionary = Logicdictionary;

            File.WriteAllText(Path.Combine(TestingReferences.GetTestingLogicPresetsPath(), "Pikmin2 Logic.json"), logicFile.ToFormattedJson());
            File.WriteAllText(Path.Combine(TestingReferences.GetTestingDictionaryPath(), "Pikmin2 V1.json"), Logicdictionary.ToFormattedJson());
        }
    }
}
