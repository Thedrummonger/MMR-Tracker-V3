﻿using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;
using TDMUtils;

namespace TestingForm.GameDataCreation.Minecraft
{
    internal class CreateData
    {
        public static void ReadAndParse(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary)
        {
            var excluded_locations = DataFileUtilities.DeserializeJsonFile<Data.MC_excluded_locations>(Data.MCPaths.excluded_locations_File);
            var items = DataFileUtilities.DeserializeJsonFile<Data.MC_items>(Data.MCPaths.items_File);
            var locations = DataFileUtilities.DeserializeJsonFile<Data.MC_locations>(Data.MCPaths.locations_File);
            var regions = DataFileUtilities.DeserializeJsonFile<Data.MC_regions>(Data.MCPaths.regions_File);
            var logic = DataFileUtilities.DeserializeJsonFile<Data.MC_logic>(Data.MCPaths.logic_File);


            LogicStringParser MCLogicParser = new(quotes: '\'');

            MMRData.LogicFile logicFile = new MMRData.LogicFile()
            {
                GameCode = "MC",
                Logic = [],
                Version = 1,
            };

            LogicDictionaryData.LogicDictionary Logicdictionary = new()
            {
                RootAreas = ["Menu"],
                GameCode = "MC",
                LogicVersion = 1,
                WinCondition = "defeated_bosses",
                NetPlaySupported = true,
                GameName = "Minecraft",
                SpoilerLogInstructions = new()
                {
                    ArchipelagoParserPath = "MC"
                }
            };

            foreach (var l in logic.locations) { AddLogic(l.Key, l.Value); }
            foreach (var l in logic.entrances) { AddLogic(l.Key, l.Value); }
            foreach (var l in logic.macros) { AddLogic(l.Key, l.Value); }

            foreach (var i in regions.regions) { ScanEntrances(i); }
            foreach (var i in regions.mandatory_connections) { ScanEntrances(i); }
            foreach (var i in regions.default_connections) { ScanEntrances(i, true); }

            foreach(var region in locations.locations_by_region)
            {
                foreach(var location in region.Value)
                {
                    LogicDictionaryData.DictionaryLocationEntries locationEntry = new()
                    {
                        ID = location,
                        Area = region.Key,
                        Name = location,
                        OriginalItem = "advancement",
                        ValidItemTypes = ["item"],
                        SpoilerData = new MMRData.SpoilerlogReference() { NetIDs = [location], Tags = GetLocationTags(location) },
                    };
                    Logicdictionary.LocationList.Add(location, locationEntry);
                }
            }
            foreach (var location in locations.all_locations)
            {
                if (!Logicdictionary.LocationList.ContainsKey(location))
                {
                    LogicDictionaryData.DictionaryLocationEntries locationEntry = new()
                    {
                        ID = location,
                        Area = "Archipelago",
                        Name = location,
                        OriginalItem = "advancement",
                        ValidItemTypes = ["item"],
                        SpoilerData = new MMRData.SpoilerlogReference() { NetIDs = [location], Tags = GetLocationTags(location) },
                    };
                    Logicdictionary.LocationList.Add(location, locationEntry);
                }
                if (!logicFile.Logic.Any(x => x.Id == location))
                {
                    AddLogic(location, Logicdictionary.LocationList[location].Area);
                }
            }

            string[] GetLocationTags(string ID)
            {
                List<string> Tags = [];
                if (excluded_locations.hard.Contains(ID)) { Tags.Add("hard"); }
                if (excluded_locations.unreasonable.Contains(ID)) { Tags.Add("unreasonable"); }
                if (excluded_locations.ender_dragon.Contains(ID)) { Tags.AddRange(["post_game", "ender_dragon"]); }
                if (excluded_locations.wither.Contains(ID)) { Tags.AddRange(["post_game", "wither"]); }
                return [.. Tags];
            }

            foreach (var region in locations.locations_by_region)
            {
                foreach (var location in region.Value)
                {
                    var LogicEntry = logicFile.Logic.First(x => x.Id == location);
                    LogicEntry.RequiredItems.Add(region.Key);
                }
            }

            foreach (var item in items.all_items)
            {
                LogicDictionaryData.DictionaryItemEntries itemEntry = new()
                {
                    ID = item,
                    ItemTypes = ["item"],
                    Name = item,
                    SpoilerData = new MMRData.SpoilerlogReference() { NetIDs = [item] },
                };
                Logicdictionary.ItemList.Add(item, itemEntry);
            }

            Logicdictionary.ItemList.Add("advancement", new() { ID = "advancement", Name = "Advancement" });
            Logicdictionary.MacroList.Add("defeated_bosses", new() { ID = "defeated_bosses", Name = "Defeat Required Bosses" });

            Logicdictionary.LogicEntryCollections.Add("checked_locations", new OptionData.LogicEntryCollection
            {
                ID = "checked_locations",
                Entries = Logicdictionary.LocationList.Select(x => $"checked{{{x.Key}}}").ToList()
            });
            Logicdictionary.IntOptions.Add("advancement_goal", new OptionData.IntOption(null)
            {
                ID = "advancement_goal",
                Name = "Advancement Goal",
                Min = 0,
                Value = 40
            });
            Logicdictionary.IntOptions.Add("egg_shards_required", new OptionData.IntOption(null)
            {
                ID = "egg_shards_required",
                Name = "Dragon Egg Shards Required",
                Min = 0,
                Value = 0
            });
            Logicdictionary.ChoiceOptions.Add("combat_difficulty", new OptionData.ChoiceOption(null)
            {
                ID = "combat_difficulty",
                Name = "Combat Difficulty",
                Value = "normal"
            }.CreateSimpleValues(("easy", "Easy"), ("normal", "Normal"), ("hard", "Hard")));
            Logicdictionary.ChoiceOptions.Add("required_bosses", new OptionData.ChoiceOption(null)
            {
                ID = "required_bosses",
                Name = "Required Bosses",
                Value = "ender_dragon"
            }.CreateSimpleValues(("none", "None"), ("ender_dragon", "Ender Dragon"), ("wither", "Wither"), ("both", "Both")));
            Logicdictionary.ToggleOptions.Add("structure_compasses", new OptionData.ToggleOption(null)
            {
                ID = "structure_compasses",
                Name = "Structure Compasses",
                Value = true
            }.CreateSimpleValues());
            Logicdictionary.ToggleOptions.Add("death_link", new OptionData.ToggleOption(null)
            {
                ID = "death_link",
                Name = "Death Link",
                Value = false
            }.CreateSimpleValues());

            foreach(var i in logicFile.Logic)
            {
                LogicUtilities.MoveRequirementsToConditionals(i);
                LogicUtilities.RemoveRedundantConditionals(i);
                LogicUtilities.MakeCommonConditionalsRequirements(i);
                LogicUtilities.SortConditionals(i);
            }

            void ScanEntrances(object[] Data, bool Randomizable = false)
            {
                string Area;
                string[] Exits;
                (Area, Exits) = regions.ParseObj(Data);
                foreach (var exit in Exits)
                {
                    string ID = $"{Area} => {exit}";
                    if (!logicFile.Logic.Any(x => x.Id == ID)) { AddLogic(ID, "true"); }
                    LogicDictionaryData.DictionaryEntranceEntries entranceEntry = new()
                    {
                        Area = Area,
                        Exit = exit,
                        ID = ID,
                        RandomizableEntrance = Randomizable,
                    };
                    Logicdictionary.EntranceList.Add(ID, entranceEntry);
                }
            }

            void AddLogic(string key, string Logic)
            {
                var Entry = new MMRData.JsonFormatLogicItem()
                {
                    Id = key,
                    ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(MCLogicParser, Logic, key)
                };
                logicFile.Logic.Add(Entry);
            }



            File.WriteAllText(Path.Combine(TestingReferences.GetTestingLogicPresetsPath(), "Minecraft Logic.json"), logicFile.ToFormattedJson());
            File.WriteAllText(Path.Combine(TestingReferences.GetTestingDictionaryPath(), "Minecraft V1.json"), Logicdictionary.ToFormattedJson());

            Logic = logicFile;
            dictionary = Logicdictionary;
        }
    }
}
