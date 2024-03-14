using Microsoft.VisualBasic.Logging;
using MMR_Tracker_V3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TestingForm.GameDataCreation.LinksAwakeningSwitch.Data;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;
using static MMR_Tracker_V3.TrackerObjects.OptionData;
using static MMR_Tracker_V3.TrackerObjects.LogicDictionaryData;

namespace TestingForm.GameDataCreation.LinksAwakeningSwitch
{
    internal class Gen
    {
        public static void GenData(out MMRData.LogicFile OutLogic, out LogicDictionaryData.LogicDictionary outDict)
        {
            var LogicFile = File.ReadAllText(Paths.RandoLogicFile());
            var LogicFileYamlString = TestingUtility.ConvertYamlStringToJsonString(LogicFile);
            var LogicFileObject = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(LogicFileYamlString);

            var ItemsFile = File.ReadAllText(Paths.RandoItemsFile());
            var ItemsFileYamlString = TestingUtility.ConvertYamlStringToJsonString(ItemsFile, true);
            var ItemsFileObject = JsonConvert.DeserializeObject<LASItemPool>(ItemsFileYamlString);

            var LocationsFile = File.ReadAllText(Paths.RandoLocationsFile());
            var LocationsFileYamlString = TestingUtility.ConvertYamlStringToJsonString(LocationsFile, true);
            var LocationsFileObject = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(LocationsFileYamlString);

            LogicDictionaryData.LogicDictionary logicDictionary = new LogicDictionaryData.LogicDictionary()
            {
                GameCode = "LAS",
                LogicVersion = 1,
                WinCondition = "kill-nightmare",
            };
            MMRData.LogicFile LogicObject = new()
            {
                GameCode = "LAS",
                Version = 1,
                Logic = []
            };

            List<ExpandedLogicEntry> expandedLogicEntries = new List<ExpandedLogicEntry>();
            ParseLogicFile(LogicFileObject, expandedLogicEntries);
            FillLogicFile(LogicObject, expandedLogicEntries);
            CreateDictionaryItems(ItemsFileObject, logicDictionary);
            CreateDictionaryLocations(LogicFileObject, LocationsFileObject, logicDictionary);
            AddStaticLocations(logicDictionary);
            CreateSettings(logicDictionary);
            AddLogicModeLogic(LogicObject);
            AddDungeonEntranceChecks(logicDictionary);

            File.WriteAllText(Path.Combine(Paths.RandoTestFolderPath(), "LASLogic.json"), LogicObject.ToFormattedJson());
            File.WriteAllText(Path.Combine(Paths.RandoTestFolderPath(), "LASDict.json"), logicDictionary.ToFormattedJson());
            outDict = logicDictionary;
            OutLogic = LogicObject;
        }

        private static void AddDungeonEntranceChecks(LogicDictionary logicDictionary)
        {
            string[] dungeonentrances = [
                "tail-cave",
                "bottle-grotto",
                "key-cavern",
                "angler-tunnel",
                "catfish-maw",
                "face-shrine",
                "eagle-tower",
                "turtle-rock",
                "color-dungeon"
            ];
            foreach(var d in dungeonentrances)
            {
                LogicDictionaryData.DictionaryLocationEntries locationEntry = new LogicDictionaryData.DictionaryLocationEntries()
                {
                    ID = d,
                    Area = "Dungeon Entrances",
                    Name = d.Replace("-", " ").ConvertToCamelCase(),
                    OriginalItem = d,
                    ValidItemTypes = ["Dungeon"],
                    SpoilerData = new MMRData.SpoilerlogReference()
                };
                LogicDictionaryData.DictionaryItemEntries itemEntry = new LogicDictionaryData.DictionaryItemEntries()
                {
                    ID = d,
                    MaxAmountInWorld = 1,
                    ItemTypes = ["Dungeon"],
                    Name = d.Replace("-", " ").ConvertToCamelCase(),
                    ValidStartingItem = false
                };
                logicDictionary.LocationList[d] = locationEntry;
                logicDictionary.ItemList[d] = itemEntry;
            }
        }

        private static void ParseLogicFile(Dictionary<string, Dictionary<string, string>>? LogicFileObject, List<ExpandedLogicEntry> expandedLogicEntries)
        {
            LogicStringParser logicParser = new(LogicStringParser.OperatorType.CStlyeSingle);
            foreach (var LogicEntry in LogicFileObject)
            {
                ExpandedLogicEntry Entry = new();
                Entry.Name = LogicEntry.Key;
                Dictionary<string, string> LogicAugments = [];
                foreach (var data in LogicEntry.Value)
                {
                    switch (data.Key)
                    {
                        case "type":
                            Entry.type = data.Value; break;
                        case "condition-basic":
                            Entry.LogicBasic = data.Value; break;
                        case "condition-advanced":
                            Entry.LogicAdvanced = data.Value; break;
                        case "condition-glitched":
                            Entry.LogicGlitched = data.Value; break;
                        case "condition-hell":
                            Entry.LogicHell = data.Value; break;
                        default:
                            if (data.Key.StartsWith("condition-")) { LogicAugments.Add(data.Key.SplitOnce('-').Item2, data.Value); }
                            break;
                    }
                }

                string FinalLogicString = string.IsNullOrWhiteSpace(Entry.LogicBasic) ? "true" : $"({Entry.LogicBasic})";
                if (!string.IsNullOrWhiteSpace(Entry.LogicAdvanced)) { FinalLogicString += $" | (LogicIsAdvanced & {Entry.LogicAdvanced})"; }
                if (!string.IsNullOrWhiteSpace(Entry.LogicGlitched)) { FinalLogicString += $" | (LogicIsGlitched & {Entry.LogicGlitched})"; }
                if (!string.IsNullOrWhiteSpace(Entry.LogicHell)) { FinalLogicString += $" | (LogicIsHell & {Entry.LogicHell})"; }

                string Region = LogicEntry.Value.TryGetValue("region", out string ValRegion) ? ValRegion : "true";
                FinalLogicString = $"({Region} & ({FinalLogicString}))";

                Entry.Logic = LogicStringConverter.ConvertLogicStringToConditional(logicParser, FinalLogicString, LogicEntry.Key);
                expandedLogicEntries.Add(Entry);

                foreach (var entry in LogicAugments)
                {
                    ExpandedLogicEntry AugmentedEntry = new()
                    {
                        Name = Entry.Name + $"[{entry.Key}]",
                        LogicBasic = $"({Entry.Name} | ({entry.Value}))",
                        region = Entry.region,
                        type = Entry.type
                    };
                    AugmentedEntry.Logic = LogicStringConverter.ConvertLogicStringToConditional(logicParser, AugmentedEntry.LogicBasic, LogicEntry.Key);
                    expandedLogicEntries.Add(AugmentedEntry);
                }
            }
        }

        private static void FillLogicFile(MMRData.LogicFile LogicObject, List<ExpandedLogicEntry> expandedLogicEntries)
        {
            foreach (var entry in expandedLogicEntries)
            {
                MMRData.JsonFormatLogicItem logicItem = new()
                {
                    Id = entry.Name,
                    ConditionalItems = entry.Logic,
                    IsTrick = entry.type == "trick"
                };
                LogicUtilities.RemoveRedundantConditionals(logicItem);
                LogicUtilities.MakeCommonConditionalsRequirements(logicItem);
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x.Replace(":", ", "); });
                FixLogicErrors(logicItem);
                ConvertSettingEntries(logicItem);

                if (logicItem.ConditionalItems.All(x => x.Count == 1 && x.All(y => y.StartsWith("setting{logic, "))))
                {
                    logicItem.ConditionalItems = [];
                }

                LogicObject.Logic.Add(logicItem);
            }

            static void ConvertSettingEntries(MMRData.JsonFormatLogicItem logicItem)
            {
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "!classic-d2" ? "setting{classic-d2, false}" : x; });
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "!shuffle-bombs" ? "setting{shuffle-bombs, false}" : x; });
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "!shuffle-powder" ? "setting{shuffle-powder, false}" : x; });
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "bad-pets" ? "setting{bad-pets}" : x; });
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "fast-fishing" ? "setting{fast-fishing}" : x; });
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "fast-master-stalfos" ? "setting{fast-master-stalfos}" : x; });
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "fast-trendy" ? "setting{fast-trendy}" : x; });
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "free-book" ? "setting{free-book}" : x; });
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "free-shop" ? "setting{free-shop}" : x; });
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "open-bridge" ? "setting{open-bridge}" : x; });
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "open-kanalet" ? "setting{open-kanalet}" : x; });
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "open-mamu" ? "setting{open-mamu}" : x; });
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "reduced-farming" ? "setting{reduced-farming}" : x; });
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "unlocked-bombs" ? "setting{unlocked-bombs}" : x; });
            }

            static void FixLogicErrors(MMRData.JsonFormatLogicItem logicItem)
            {
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "tracey" ? "tracy" : x; });
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "pegasus-boots" ? "boots" : x; });
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "prairie" ? "plains" : x; });
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x == "dash" ? "dash-jump" : x; });
            }
        }

        private static void CreateDictionaryItems(LASItemPool? ItemsFileObject, LogicDictionary logicDictionary)
        {
            foreach (var i in ItemsFileObject.Item_Pool)
            {
                LogicDictionaryData.DictionaryItemEntries itemEntry = new LogicDictionaryData.DictionaryItemEntries()
                {
                    ID = i.Key,
                    MaxAmountInWorld = i.Value.TryGetValue("quantity", out object amount) ? int.Parse((string)amount) : -1,
                    ItemTypes = ["item"],
                    Name = i.Value.TryGetValue("item-key", out object itemKey) ? ((string)itemKey).AddWordSpacing() : i.Key.AddWordSpacing(),
                    ValidStartingItem = ItemsFileObject.Starting_Items.Contains(i.Key)
                };

                if (itemEntry.ID.StartsWith("rupee-") || itemEntry.ID.EndsWith("-trap")) { itemEntry.MaxAmountInWorld = -1; }
                if (itemEntry.Name.In("Compass", "Stone Beak", "Dungeon Map", "Small Key", "Nightmare Key"))
                {
                    itemEntry.Name += " " + itemEntry.ID.SplitOnce('-', true).Item2;
                }
                if (itemEntry.Name.StartsWith('$')) { itemEntry.Name = "Trap"; }
                if (itemEntry.ID == "stalfos-note") { itemEntry.Name = "Stalfos Note"; }
                if (itemEntry.Name.Contains('_')) { itemEntry.Name = itemEntry.Name.Replace("_", " "); }

                logicDictionary.ItemList[i.Key] = itemEntry;
            }
        }

        private static void CreateDictionaryLocations(Dictionary<string, Dictionary<string, string>>? LogicFileObject, Dictionary<string, List<string>>? LocationsFileObject, LogicDictionary logicDictionary)
        {
            foreach (var LogicEntry in LogicFileObject)
            {
                if (!LogicEntry.Value.TryGetValue("type", out string type) || type != "item") { continue; }
                LogicDictionaryData.DictionaryLocationEntries locationEntry = new LogicDictionaryData.DictionaryLocationEntries()
                {
                    ID = LogicEntry.Key,
                    Area = LogicEntry.Value["spoiler-region"],
                    Name = LogicEntry.Key.Replace("-", " ").ConvertToCamelCase(),
                    OriginalItem = LogicEntry.Value["content"],
                    ValidItemTypes = ["item"],
                    SpoilerData = new MMRData.SpoilerlogReference() { Tags = [LogicEntry.Value["type"], LogicEntry.Value["subtype"], LogicEntry.Value["region"]] }
                };
                var AdditionalData = LocationsFileObject.Where(x => x.Value.Contains(LogicEntry.Key)).Select(x => x.Key);
                if (AdditionalData.Any())
                {
                    locationEntry.SpoilerData.Tags = [.. locationEntry.SpoilerData.Tags, AdditionalData.First()];
                }
                if (locationEntry.OriginalItem == "powders-capacity") { locationEntry.OriginalItem = "powder-capacity"; }
                logicDictionary.LocationList[LogicEntry.Key] = locationEntry;
            }
        }

        private static void AddLogicModeLogic(MMRData.LogicFile LogicObject)
        {
            LogicObject.Logic.Add(new MMRData.JsonFormatLogicItem
            {
                Id = "LogicIsAdvanced",
                ConditionalItems = [
                                ["setting{logic, advanced}"],
                    ["setting{logic, glitched}"],
                    ["setting{logic, hell}"]
                            ]
            });

            LogicObject.Logic.Add(new MMRData.JsonFormatLogicItem
            {
                Id = "LogicIsGlitched",
                ConditionalItems = [
                    ["setting{logic, glitched}"],
                    ["setting{logic, hell}"]
                ]
            });

            LogicObject.Logic.Add(new MMRData.JsonFormatLogicItem
            {
                Id = "LogicIsHell",
                RequiredItems = ["setting{logic, hell}"]
            });
        }

        private static void CreateSettings(LogicDictionaryData.LogicDictionary logicDictionary)
        {
            CreateToggleOption("bad-pets", "Bad Pets");
            CreateToggleOption("classic-d2", "Classic D2");
            CreateToggleOption("fast-fishing", "Fast Fishing", true);
            CreateToggleOption("fast-stalfos", "Fast Master Stalfos");
            CreateToggleOption("fast-trendy", "Fast Trendy Game");
            CreateToggleOption("free-book", "Free Book", true);
            CreateToggleOption("free-shop", "Free Shop", true);
            CreateToggleOption("open-bridge", "Completed Bridge", true);
            CreateToggleOption("open-kanalet", "Open Kanalet", true);
            CreateToggleOption("open-mamu", "OPen Mamu", true);
            CreateToggleOption("reduce-farming", "Reduced Farming", true);
            CreateToggleOption("shuffle-bombs", "Shuffle Bombs");
            CreateToggleOption("shuffle-powder", "Shuffle Powder");
            CreateToggleOption("unlocked-bombs", "Unlocked Bombs", true);

            void CreateToggleOption(string ID, string Name, bool Default = false)
            {
                OptionData.ToggleOption toggleOption = new(null);
                toggleOption.ID = ID;
                toggleOption.Name = Name;
                toggleOption.CreateSimpleValues();
                toggleOption.SetValue(false);
                logicDictionary.ToggleOptions.Add(ID, toggleOption);
            }

            var LogicOption = new OptionData.ChoiceOption(null)
            {
                ID = "logic",
                Name = "Logic Mode",
            };
            LogicOption.CreateSimpleValues("basic", "advanced", "glitched", "hell");
            LogicOption.SetValue("basic");
            logicDictionary.ChoiceOptions.Add(LogicOption.ID, LogicOption);
        }

        private static void AddStaticLocations(LogicDictionaryData.LogicDictionary logicDictionary)
        {
            logicDictionary.ItemList.Add("rooster", new LogicDictionaryData.DictionaryItemEntries
            {
                ID = "rooster",
                Name = "Rooster",
                MaxAmountInWorld = 1,
                ValidStartingItem = false,
                ItemTypes = ["rooster"]
            });
            logicDictionary.ItemList.Add("bow-wow", new LogicDictionaryData.DictionaryItemEntries
            {
                ID = "bow-wow",
                Name = "Bow Wow",
                MaxAmountInWorld = 1,
                ValidStartingItem = false,
                ItemTypes = ["bow-wow"]
            });
            logicDictionary.LocationList.Add("moblin-cave", new LogicDictionaryData.DictionaryLocationEntries
            {
                ID = "moblin-cave",
                Area = "taltal-heights",
                Name = "Bow Wow Follower",
                OriginalItem = "bow-wow",
                ValidItemTypes = ["bow-wow"],
            });
            logicDictionary.LocationList.Add("rooster-statue", new LogicDictionaryData.DictionaryLocationEntries
            {
                ID = "rooster-statue",
                Area = "mabe-village",
                Name = "Rooster Follower",
                OriginalItem = "rooster",
                ValidItemTypes = ["rooster"],
            });
        }
    }
}
