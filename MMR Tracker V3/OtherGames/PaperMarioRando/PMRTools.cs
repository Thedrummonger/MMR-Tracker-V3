using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames
{
    public static class PMRToolsV2
    {
        public class PaperMarioMasterData
        {
            public List<List<PaperMarioLogicJSON>> Logic;
            public Dictionary<string, string> verbose_area_names;
            public Dictionary<string, string> verbose_sub_area_names;
            public Dictionary<string, Dictionary<string, string>> verbose_item_locations;
            public Dictionary<string, string> items;
            public Dictionary<string, string> verbose_item_names;
        }

        public class PaperMarioLogicJSON
        {
            public PMRLogicArea from;
            public PMRLogicArea to;
            public List<List<string>> reqs;
            public List<string> pseudoitems;
        }
        public class PMRLogicArea
        {
            public string map;
            public dynamic id;
        }

        public static string CreateIDName(string Text)
        {
            return Regex.Replace(Text, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        public static LogicObjects.TrackerInstance CreatePMRdata()
        {
            string PaperMarioDataFile = Path.Combine(References.TestingPaths.GetDevCodePath(), "MMR Tracker V3", "OtherGames", "PaperMarioRando", "PaperMarioRandoLogic.json");

            PaperMarioMasterData RefFileObject = JsonConvert.DeserializeObject<PaperMarioMasterData>(File.ReadAllText(PaperMarioDataFile));

            LogicDictionaryData.LogicDictionary PRMDict = new LogicDictionaryData.LogicDictionary
            {
                GameCode = "PMR",
                LogicVersion = 2,
                LogicFormat = "JSON",
            };
            Dictionary<string, string> LogicMapping = new Dictionary<string, string>();

            MakeAreaList(PRMDict, RefFileObject);
            CreateEntranceList(PRMDict, RefFileObject, LogicMapping);
            CreateLocationList(PRMDict, RefFileObject, LogicMapping);
            CreateItemList(PRMDict, RefFileObject, LogicMapping);
            SortMacros(PRMDict, RefFileObject, LogicMapping);
            AddVariables(PRMDict);
            Addoptions(PRMDict);
            AddHardCodedMacrios(LogicMapping);
            AddRootEntrances(PRMDict, LogicMapping);    

            MMRData.LogicFile PRMLogic = new MMRData.LogicFile
            {
                GameCode = "PMR",
                Version = 2,
                Logic = new List<MMRData.JsonFormatLogicItem>()
            };
            CreateLogicFile(LogicMapping, PRMLogic);

            string OutputDict = Path.Combine(References.TestingPaths.GetDevTestingPath(), "PMRDict.json");
            File.WriteAllText(OutputDict, JsonConvert.SerializeObject(PRMDict, Testing._NewtonsoftJsonSerializerOptions));

            string OutputLogic = Path.Combine(References.TestingPaths.GetDevTestingPath(), "PMRLogic.json");
            File.WriteAllText(OutputLogic, JsonConvert.SerializeObject(PRMLogic, Testing._NewtonsoftJsonSerializerOptions));

            return new LogicObjects.TrackerInstance { LogicDictionary = PRMDict, LogicFile = PRMLogic };

        }

        private static void MakeAreaList(LogicDictionaryData.LogicDictionary pRMDict, PaperMarioMasterData refFileObject)
        {
            List<string> MasterAreaList = new List<string>();
            List<PaperMarioLogicJSON> Logic = refFileObject.Logic.SelectMany(x => x).ToList();
            foreach (var i in Logic)
            {
                if (i.to.map == null) { continue; }
                MasterAreaList.Add(i.from.ConvertToAreaName(refFileObject));
                MasterAreaList.Add(i.to.ConvertToAreaName(refFileObject));
            }
            pRMDict.AreaList = MasterAreaList.Distinct().OrderBy(x => x).ToList();
        }

        private static void CreateEntranceList(LogicDictionaryData.LogicDictionary pRMDict, PaperMarioMasterData refFileObject, Dictionary<string, string> logicMapping)
        {
            List<LogicDictionaryData.DictionaryEntranceEntries> EntranceList = new List<LogicDictionaryData.DictionaryEntranceEntries>();
            List<PaperMarioLogicJSON> Logic = refFileObject.Logic.SelectMany(x => x).ToList();
            foreach (var i in Logic)
            {
                if (i.to.map == null) { continue; }
                if (i.to.map == i.from.map && i.to.id.ToString() == i.from.id.ToString()) { continue; }
                string ID = $"{i.from.ConvertToAreaName(refFileObject)} X {i.to.ConvertToAreaName(refFileObject)}";

                string CurrentLogic = $"({i.from.ConvertToAreaName(refFileObject)})";
                if (i.reqs is not null && i.reqs.Any()) { CurrentLogic += $" & ({string.Join(" & ", i.reqs.Select(x => string.Join(" | ", x))) })"; }
                CurrentLogic = $"({CurrentLogic})";

                if (EntranceList.Any(x => x.ID == ID))
                {
                    Debug.WriteLine($"{ID} Already existed in macro pool");
                    logicMapping[ID] += $" | {CurrentLogic}";
                    continue;
                }
                LogicDictionaryData.DictionaryEntranceEntries entranceEntry = new LogicDictionaryData.DictionaryEntranceEntries
                {
                    ID = ID,
                    AlwaysAccessable = false,
                    Area = i.from.ConvertToAreaName(refFileObject),
                    Exit = i.to.ConvertToAreaName(refFileObject),
                    EntrancePairID = null,
                    DestinationHasSingleEntrance = false,
                    RandomizableEntrance = false
                };
                logicMapping.Add(ID, CurrentLogic);
                EntranceList.Add(entranceEntry);
            }
            pRMDict.EntranceList = EntranceList;
        }

        private static void CreateLocationList(LogicDictionaryData.LogicDictionary pRMDict, PaperMarioMasterData refFileObject, Dictionary<string, string> logicMapping)
        {
            List<LogicDictionaryData.DictionaryLocationEntries> LocationList = new List<LogicDictionaryData.DictionaryLocationEntries>();
            foreach (var area in refFileObject.verbose_item_locations)
            {
                foreach (var loc in area.Value)
                {
                    string ID = $"{area.Key}_{loc.Key}";
                    if (LocationList.Any(x => x.ID == ID)) { Debug.WriteLine($"{ID} Already existed in macro pool"); continue; }

                    string OriginalItem = null;
                    if (ID.EndsWith("_HiddenPanel")) { OriginalItem = "StarPiece"; }
                    if (ID.EndsWith("_Partner")) { OriginalItem = loc.Value.Replace(" Partner", ""); }

                    LogicDictionaryData.DictionaryLocationEntries LocationEntry = new LogicDictionaryData.DictionaryLocationEntries
                    {
                        ID = ID,
                        Area = $"{refFileObject.verbose_area_names[GetBaseMap(area.Key)]}",
                        Name = $"{refFileObject.verbose_sub_area_names[area.Key]} - {loc.Value}",
                        OriginalItem = OriginalItem,
                        ValidItemTypes =  new string[] { "ITEM" },
                        SpoilerData = new MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { $"({refFileObject.verbose_sub_area_names[area.Key]} - {loc.Value})", $"{refFileObject.verbose_sub_area_names[area.Key]} - {loc.Value}" } }
                    };
                    LocationList.Add(LocationEntry);

                    PMRLogicArea logicArea = new PMRLogicArea() { map = area.Key, id = loc.Key };
                    var RequiredArea = logicArea.ConvertToAreaName(refFileObject);
                    if (!pRMDict.AreaList.Contains(RequiredArea)) { Debug.WriteLine($"{RequiredArea} Was not valid"); }
                    else { logicMapping.Add(ID, RequiredArea); }
                }
            }
            pRMDict.LocationList = LocationList;
        }

        private static void CreateItemList(LogicDictionaryData.LogicDictionary pRMDict, PaperMarioMasterData refFileObject, Dictionary<string, string> logicMapping)
        {
            List<LogicDictionaryData.DictionaryItemEntries> ItemList = new List<LogicDictionaryData.DictionaryItemEntries>();
            foreach (var i in refFileObject.items)
            {
                LogicDictionaryData.DictionaryItemEntries ItemEntry = new LogicDictionaryData.DictionaryItemEntries
                {
                    ID = i.Key,
                    MaxAmountInWorld = -1,
                    ItemTypes = new string[] { "ITEM", i.Value },
                    Name = CapitalizeItemName(refFileObject.verbose_item_names.ContainsKey(i.Key) ? refFileObject.verbose_item_names[i.Key] : i.Key),
                    SpoilerData = new MMRData.SpoilerlogReference { SpoilerLogNames = refFileObject.verbose_item_names.ContainsKey(i.Key) ? new string[] { refFileObject.verbose_item_names[i.Key], i.Key } : new string[] { i.Key } },
                    ValidStartingItem = true
                };
                ItemList.Add(ItemEntry);
            }
            pRMDict.ItemList = ItemList;
        }

        private static void SortMacros(LogicDictionaryData.LogicDictionary pRMDict, PaperMarioMasterData refFileObject, Dictionary<string, string> logicMapping)
        {
            string[] starSpirits = new string[] { "Twink", "Eldstar", "Mamar", "Skolar", "Muskular", "Misstar", "Klevar", "Kalmar" };
            List<PaperMarioLogicJSON> Logic = refFileObject.Logic.SelectMany(x => x).ToList();
            foreach (var entry in Logic.Where(x => x.pseudoitems is not null && x.pseudoitems.Any()))
            {
                string CurrentLogic = $"({entry.from.ConvertToAreaName(refFileObject)})".Replace("'", "").Replace(",", "");
                if (entry.reqs is not null && entry.reqs.Any()) { CurrentLogic += $" & ({string.Join(" & ", entry.reqs.Select(x => string.Join(" | ", x))) })"; }
                CurrentLogic = $"({CurrentLogic})";

                foreach (var i in entry.pseudoitems)
                {
                    if (pRMDict.MacroList.Any(x => x.ID == i))
                    {
                        Debug.WriteLine($"{i} Already existed in macro pool");
                        logicMapping[i] += $" | {CurrentLogic}";
                        continue;
                    }
                    if (i.StartsWith("StarPiece_")) { AddMacroCheck(entry, i, "StarPiece", $"Chuck Quizmo Reward {i.Split("_")[2]}", CurrentLogic); }
                    else if (i.StartsWith("EQUIPMENT_Hammer_Progressive")) { AddMacroCheck(entry, $"{entry.from.map}_ProgressiveHammer", "ProgressiveHammer", $"Progressive Hammer", CurrentLogic); }
                    else if (i.StartsWith("EQUIPMENT_Boots_Progressive")) { AddMacroCheck(entry, $"{entry.from.map}_ProgressiveBoots", "ProgressiveBoots", $"Progressive Boots", CurrentLogic); }
                    else if (i.StartsWith("RF_SavedYoshiKid")) { AddMacroCheck(entry, $"{entry.from.map}_YoshiKid", "YoshiKid", $"Yoshi Kid", CurrentLogic); }
                    else if (i.StartsWith("STARSPIRIT_")) { AddMacroCheck(entry, $"{entry.from.map}_STARSPIRIT", $"{starSpirits[int.Parse(i.Split("_")[1])]}", $"Star Spirit", CurrentLogic); }
                    else if (i == "MysticalKey") { AddMacroCheck(entry, $"{entry.from.map}_MysticalKey", $"MysticalKey", $"Mystical Key Chest", CurrentLogic); }
                    else
                    {
                        logicMapping.Add(i, CurrentLogic);
                        LogicDictionaryData.DictionaryMacroEntry macroEntry = new LogicDictionaryData.DictionaryMacroEntry { ID = i };
                        pRMDict.MacroList.Add(macroEntry);
                    }
                }
            }

            void AddMacroCheck(PaperMarioLogicJSON entry, string ID, string OriginalItem, string Name, string CurrentLogic)
            {
                LogicDictionaryData.DictionaryLocationEntries LocationEntry = new LogicDictionaryData.DictionaryLocationEntries
                {
                    ID = ID,
                    Area = $"{refFileObject.verbose_area_names[GetBaseMap(entry.from.map)]}",
                    Name = $"{refFileObject.verbose_sub_area_names[entry.from.map]} - {Name}",
                    OriginalItem = OriginalItem,
                    ValidItemTypes =  new string[] { "ITEM" },
                    SpoilerData = new MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { $"{refFileObject.verbose_sub_area_names[entry.from.map]} - {Name}" } }
                };
                if (pRMDict.LocationList.Any(x => x.ID == LocationEntry.ID))
                {
                    Debug.WriteLine($"{ID} Already existed in Location pool");
                    logicMapping[ID] += $" | {CurrentLogic}";
                    return;
                }

                logicMapping.Add(LocationEntry.ID, CurrentLogic);
                pRMDict.LocationList.Add(LocationEntry);
            }
        }

        private static void AddVariables(LogicDictionaryData.LogicDictionary logicDictionary)
        {
            logicDictionary.Variables.Add(new TrackerObjects.LogicDictionaryData.TrackerVariable
            {
                ID = "starspirits",
                Name = "starspirits",
                Static = true,
                Value = new List<string> { "Eldstar", "Mamar", "Skolar", "Muskular", "Misstar", "Klevar", "Kalmar" }
            });
            logicDictionary.Variables.Add(new TrackerObjects.LogicDictionaryData.TrackerVariable
            {
                ID = "letter",
                Name = "letter",
                Static = true,
                Value = new List<string> { "Letter01", "Letter02", "Letter03", "Letter04", "Letter05", "Letter06", "Letter07", "Letter08", "Letter09", "Letter10", "Letter11", "Letter12", "Letter13", "Letter14", "Letter15", "Letter16", "Letter17", "Letter18", "Letter19", "Letter20", "Letter21", "Letter22", "Letter23", "Letter24", "Letter25" }
            });
            logicDictionary.Variables.Add(new TrackerObjects.LogicDictionaryData.TrackerVariable
            {
                ID = "starpieces",
                Name = "starpieces",
                Static = true,
                Value = new List<string> { "StarPiece", "ThreeStarPieces", "ThreeStarPieces", "ThreeStarPieces" }
            });
        }

        private static void Addoptions(LogicDictionaryData.LogicDictionary logicDictionary)
        {
            AddOption("starting_area", "Toad Town", "Starting Area", new string[] { "Goomba Village", "Toad Town", "Dry Dry Outpost", "Yoshi Village" });

            AddToggleOption("BlueHouseOpen", "false", "Open Blue House", CreateSimpleReplacementOption("GF_MAC02_UnlockedHouse"));
            AddToggleOption("FlowerGateOpen", "false", "Open Flower Gate", CreateSimpleReplacementOption("RF_Ch6_FlowerGateOpen"));
            AddToggleOption("WhaleOpen", "false", "Open Whale", CreateSimpleReplacementOption("RF_CanRideWhale"));
            AddToggleOption("ToyboxOpen", "false", "Open Toy Box");

            AddToggleOption("HiddenBlocksVisible", "true", "Hidden Blocks Always Visible");

            //Partners Always usable option
            var option = new TrackerObjects.OptionData.TrackerOption
            {
                ID = "partners_always_usable",
                CurrentValue = "false",
                DisplayName = "Partners Always useable",
                Values = new Dictionary<string, TrackerObjects.OptionData.actions>()
                    {
                        { "true", new TrackerObjects.OptionData.actions() { LogicReplacements = new TrackerObjects.OptionData.LogicReplacement[] { new TrackerObjects.OptionData.LogicReplacement { ReplacementList = new Dictionary<string, string> { { "Goombario", "true" }, { "Kooper", "true" }, { "Bombette", "true" }, { "Parakarry", "true" }, { "Watt", "true" }, { "Sushie", "true" }, { "Lakilester", "true" }, { "Bow", "true" } } } }  } },
                        { "false", new TrackerObjects.OptionData.actions()}
                    }
            };
            logicDictionary.Options.Add(option);

            void AddOption(string ID, string CurrentValue, string Name, string[] Values)
            {
                var option = new TrackerObjects.OptionData.TrackerOption
                {
                    ID = ID,
                    CurrentValue = CurrentValue,
                    DisplayName = Name,
                    Values = Values.ToDictionary(x => x, x => new TrackerObjects.OptionData.actions())
                };
                logicDictionary.Options.Add(option);
            }

            void AddToggleOption(string ID, string CurrentValue, string Name, TrackerObjects.OptionData.actions trueAction = null)
            {
                var option = new TrackerObjects.OptionData.TrackerOption
                {
                    ID = ID,
                    CurrentValue = CurrentValue,
                    DisplayName = Name,
                    Values = new Dictionary<string, TrackerObjects.OptionData.actions>()
                    {
                        { "true", trueAction is null ? new TrackerObjects.OptionData.actions() : trueAction},
                        { "false", new TrackerObjects.OptionData.actions()}
                    }
                };
                logicDictionary.Options.Add(option);
            }

            OptionData.actions CreateSimpleReplacementOption(string Replace, string with = "true")
            {
                return new TrackerObjects.OptionData.actions() { LogicReplacements = new TrackerObjects.OptionData.LogicReplacement[] { new TrackerObjects.OptionData.LogicReplacement { ReplacementList = new Dictionary<string, string> { { Replace, "true" } } } } };
            }
        }

        private static void AddHardCodedMacrios(Dictionary<string, string> logicMapping)
        {
            logicMapping.Add("can_flip_panels", "ProgressiveBoots, 1 | ProgressiveHammer, 2");
            logicMapping.Add("can_shake_trees", "Bombette | ProgressiveHammer, 0");
            logicMapping.Add("has_parakarry_3_letters", "letter, 3");
            logicMapping.Add("RF_HiddenBlocksVisible", "HiddenBlocksVisible == true");
            logicMapping.Add("RF_Missable", "true");
            logicMapping.Add("RF_OutOfLogic", "true");
            logicMapping.Add("RF_ToyboxOpen", "ToyboxOpen == true");
            logicMapping.Add("saved_all_yoshikids", "YoshiKid, 5");
        }

        private static void AddRootEntrances(LogicDictionaryData.LogicDictionary logicDictionary, Dictionary<string, string> CondensedLogic)
        {
            logicDictionary.AreaList.Add("Root");
            //Add Root Entrances
            CondensedLogic.Add("Root X Toad Town - Gate District - 4", "starting_area == Toad Town");
            logicDictionary.EntranceList.Add(new TrackerObjects.LogicDictionaryData.DictionaryEntranceEntries
            {
                AlwaysAccessable = true,
                Area = "Root",
                ID = "Root X Toad Town - Gate District - 4",
                Exit = "Toad Town - Gate District - 4",
                RandomizableEntrance = false
            });
            CondensedLogic.Add("Root X Goomba Region - Behind the Village - 0", "starting_area == Goomba Village");
            logicDictionary.EntranceList.Add(new TrackerObjects.LogicDictionaryData.DictionaryEntranceEntries
            {
                AlwaysAccessable = true,
                Area = "Root",
                ID = "Root X Goomba Region - Behind the Village - 0",
                Exit = "Goomba Region - Behind the Village - 0",
                RandomizableEntrance = false
            });
            CondensedLogic.Add("Root X Dry Dry Outpost - Outpost 1 - 0", "starting_area == Dry Dry Outpost");
            logicDictionary.EntranceList.Add(new TrackerObjects.LogicDictionaryData.DictionaryEntranceEntries
            {
                AlwaysAccessable = true,
                Area = "Root",
                ID = "Root X Dry Dry Outpost - Outpost 1 - 0",
                Exit = "Dry Dry Outpost - Outpost 1 - 0",
                RandomizableEntrance = false
            });
            CondensedLogic.Add("Root X Jade Jungle - Village Cove - 0", "starting_area == Yoshi Village");
            logicDictionary.EntranceList.Add(new TrackerObjects.LogicDictionaryData.DictionaryEntranceEntries
            {
                AlwaysAccessable = true,
                Area = "Root",
                ID = "Root X Jade Jungle - Village Cove - 0",
                Exit = "Jade Jungle - Village Cove - 0",
                RandomizableEntrance = false
            });
        }

        private static void CreateLogicFile(Dictionary<string, string> LogicMapping, MMRData.LogicFile PRMLogic)
        {
            foreach (var i in LogicMapping)
            {
                var LogicEntry = new MMRData.JsonFormatLogicItem
                {
                    Id = i.Key,
                    ConditionalItems = LogicStringParser.ConvertLogicStringToConditional(i.Value.Replace("'", ""), true)
                };
                logicCleaner.RemoveRedundantConditionals(LogicEntry);
                logicCleaner.MakeCommonConditionalsRequirements(LogicEntry);
                DoPMRLogicEdits(LogicEntry);
                PRMLogic.Logic.Add(LogicEntry);
            }
        }


        private static void DoPMRLogicEdits(MMRData.JsonFormatLogicItem logicEntry)
        {
            Dictionary<string, string> LogicReplacements = new Dictionary<string, string>
            {
                { "boots, 1", "ProgressiveBoots, 0" },
                { "boots, 2", "ProgressiveBoots, 1" },
                { "boots, 3", "ProgressiveBoots, 2" },
                { "hammer, 1", "ProgressiveHammer, 0" },
                { "hammer, 2", "ProgressiveHammer, 1" },
                { "hammer, 3", "ProgressiveHammer, 2" }
            };

            foreach (var replacements in LogicReplacements)
            {
                logicEntry.RequiredItems = logicEntry.RequiredItems.Select(x => LogicReplacements.ContainsKey(x) ? LogicReplacements[x] : x).ToList();
                logicEntry.ConditionalItems = logicEntry.ConditionalItems.Select(set => set.Select(x => LogicReplacements.ContainsKey(x) ? LogicReplacements[x] : x).ToList()).ToList();
            }
        }

        private static string CapitalizeItemName(string text, bool preserveAcronyms = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        private static string ConvertToAreaName(this PMRLogicArea area, PaperMarioMasterData refFileObject)
        {
            var BaseMap = GetBaseMap(area.map);
            var SubMap = area.map;
            var SubMapSection = area.id;
            return $"{refFileObject.verbose_area_names[BaseMap]} - {refFileObject.verbose_sub_area_names[SubMap]} - {SubMapSection}".Replace("'", "").Replace(",", "");
        }

        private static string GetBaseMap(string map)
        {
            return map.Split('_')[0];
        }


        public static Dictionary<string, dynamic> GetSettingObjectFromSeed(string SpoilerLogPath)
        {
            string seed = Path.GetFileNameWithoutExtension(SpoilerLogPath).Replace("_spoiler", "");
            string URL = $"https://paper-mario-randomizer-server.ue.r.appspot.com/randomizer_settings/{seed}";
            System.Net.WebClient wc = new System.Net.WebClient();
            string SettingData = wc.DownloadString(URL);
            Dictionary<string, dynamic> Settings = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(SettingData);
            return Settings;
        }

        public static void ReadSpoilerLog(string[] spoilerLog, string OriginalFile, LogicObjects.TrackerInstance Instance)
        {
            var SettingData = GetSettingObjectFromSeed(OriginalFile);
            List<string> AppliedLocations = new List<string>();
            foreach (var i in spoilerLog)
            {
                if (i.StartsWith("Sphere 0:")) { break; }
                string line = i;
                if (!line.Contains(":")) { continue; }
                var data = line.Split(new string[] { "):" }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
                data[0] = data[0][1..];
                if (data.Length < 2 || string.IsNullOrWhiteSpace(data[1])) { continue; }
                data[1] = data[1].Replace("*", "");

                if (data[1].StartsWith("ThreeStarPieces")) { data[1] = "ThreeStarPieces"; }
                if (data[1] == "Starpiece" || data[1] == "StarPiece21") { data[1] = "StarPiece"; }
                if (data[0] == "Outside Lower Jail - Yellow Block") { data[0] = "Outside Lower Jail (No Lava) - Yellow Block"; }
                if (data[0] == "Outside Lower Jail - Defeat Koopatrol Reward") { data[0] = "Outisde Lower Jail (Lava) - Defeat Koopatrol Reward"; }

                var Location = Instance.LocationPool.Values.FirstOrDefault(x => x.GetDictEntry(Instance).SpoilerData.SpoilerLogNames.Contains(data[0]) && !AppliedLocations.Contains(x.ID));
                var Item = Instance.ItemPool.Values.FirstOrDefault(x => x.GetDictEntry(Instance).SpoilerData.SpoilerLogNames.Contains(data[1]));

                if (data[1].StartsWith("TRAP")) { Item = new ItemData.ItemObject { Id = data[1] }; }

                if (data[0] == "(Start) Mario's inventory" && Item is not null) { Item.AmountInStartingpool++; continue; }

                if (Location == null) { Debug.WriteLine($"Could not find Location {data[0]}"); continue; }
                if (Item == null) { Debug.WriteLine($"Could not find item {data[1]}"); continue; }
                Location.Randomizeditem.SpoilerLogGivenItem = Item.Id;
                AppliedLocations.Add(Location.ID);
            }

            foreach (var i in Instance.LocationPool)
            {
                if (i.Key.EndsWith("HiddenPanel") && !SettingData["IncludePanels"])
                {
                    i.Value.SetRandomizedState(MiscData.RandomizedState.ForcedJunk, Instance);
                }
                else if ((i.Key.Contains("DojoChan") || i.Key.Contains("DojoLee") || i.Key.Contains("DojoMaster")) && !SettingData["IncludeDojo"])
                {
                    i.Value.SetRandomizedState(MiscData.RandomizedState.ForcedJunk, Instance);
                }

                if (i.Value.Randomizeditem.SpoilerLogGivenItem == null)
                {
                    if (i.Value.GetDictEntry(Instance).OriginalItem != null) 
                    {
                        i.Value.SetRandomizedState(MiscData.RandomizedState.UnrandomizedManual, Instance);
                    }
                    else
                    {
                        Debug.WriteLine($"{i.Value.GetDictEntry(Instance).Name} Did not have spoiler data");
                    }
                }
            }

            Instance.UserOptions["BlueHouseOpen"].CurrentValue = SettingData["BlueHouseOpen"].ToString().ToLower();
            Instance.UserOptions["FlowerGateOpen"].CurrentValue = SettingData["FlowerGateOpen"].ToString().ToLower();
            Instance.UserOptions["WhaleOpen"].CurrentValue = SettingData["WhaleOpen"].ToString().ToLower();
            Instance.UserOptions["ToyboxOpen"].CurrentValue = SettingData["ToyboxOpen"].ToString().ToLower();
            Instance.UserOptions["partners_always_usable"].CurrentValue = SettingData["PartnersAlwaysUsable"].ToString().ToLower();

            Instance.UserOptions["HiddenBlocksVisible"].CurrentValue = ((int)SettingData["HiddenBlockMode"] == 3).ToString().ToLower();

            Dictionary<int, string> StartingAreas = new Dictionary<int, string>() { { 257, "Goomba Village" }, { 65796, "Toad Town" }, { 590080, "Dry Dry Outpost" }, { 1114882, "Yoshi Village" } };

            var StartingAreaOption = Instance.UserOptions["starting_area"];
            int SettingStartingMap = (int)SettingData["StartingMap"];
            string MappedTrackerArea = StartingAreas[SettingStartingMap];

            StartingAreaOption.CurrentValue = MappedTrackerArea;

        }
    }
}
