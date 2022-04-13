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
    public static class PMRTools
    {
        public class PaperMarioMasterData
        {
            public List<PaperMarioLogicJSON> Logic;
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
            public List<dynamic> reqs;
            public List<string> pseudoitems;
        }
        public class PMRLogicArea
        {
            public string map;
            public dynamic id;
            public String GetFullArea(PaperMarioMasterData MasterData, bool ID = true)
            {
                var dat = map.Split("_");
                string area = MasterData.verbose_area_names[dat[0]];
                string subArea = MasterData.verbose_sub_area_names[map];
                area = Regex.Replace(area, "[^a-zA-Z0-9 _.]+", "", RegexOptions.Compiled);
                subArea = Regex.Replace(subArea, "[^a-zA-Z0-9 _.]+", "", RegexOptions.Compiled);
                string FinalArea = $"{area} - {subArea}";
                if (ID) { FinalArea += $" - {id}"; }
                return FinalArea;
            }
            public String GetGeneralArea(PaperMarioMasterData MasterData)
            {
                var dat = map.Split("_");
                string area = MasterData.verbose_area_names[dat[0]];
                return area;
            
            }
        }

        public class PMRData
        {
            public List<PMRItemLocation> itemLocations = new List<PMRItemLocation>();
            public List<PMRExit> RealExits = new List<PMRExit>();
            public List<PMRExit> MacroExits = new List<PMRExit>();
            public List<PMRArea> Areas = new List<PMRArea>();
            public List<PMRMacro> Macros = new List<PMRMacro>();
            public List<PMRItemData> Items = new List<PMRItemData>();
        }
        public class PMRItemData
        {
            public string ID;
            public string Name;
            public List<string> Types = new List<string> { "item" };
            public List<string> SpoilerNames = new List<string>();
        }
        public class PMRItemLocation
        {
            public string ID;
            public string Area;
            public string Logic;
            public string Name;
            public List<string> SpoilerNames = new List<string>();
        }
        public class PMRExit
        {
            public string ParentAreaID;
            public string ID;
            public string Logic;
        }
        public class PMRMacro
        {
            public string ID;
            public string Area;
            public string Logic;
        }
        public class PMRArea
        {
            public string ID;
        }

        public static string CreateIDName(string Text)
        {
            return Regex.Replace(Text, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        public static LogicObjects.TrackerInstance ReadLogicJson()
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            PaperMarioMasterData MasterReference = JsonConvert.DeserializeObject<PaperMarioMasterData>(wc.DownloadString(@"https://raw.githubusercontent.com/Thedrummonger/MMR-Tracker-V3/master/MMR%20Tracker%20V3/OtherGames/PaperMarioRandoLogic.json"));

            PMRData MasterData = new PMRData();

            AddItems(MasterReference, MasterData); //Add Items from the Ref Sheet
            GetDataFromLogicFile(MasterReference, MasterData); //Add Locations, Macro and entrances from the Ref Sheet
            CreateAreaList(MasterData); //Create a list of all areas

            LogicDictionaryData.LogicDictionary logicDictionary = CreateLogicDictionary(MasterData); //Create the logic dictionary using the collected data
            AddVariables(logicDictionary);//Add variables
            Addoptions(logicDictionary);//add options

            Dictionary<string, string> CondensedLogic = CondenseLogic(MasterData);//Condence the logic combining all duplicate entries
            AddRootEntrances(logicDictionary, CondensedLogic);//Add entrance Macros that detail starting area access
            AddHardCodedMacros(CondensedLogic);//Add macros that are created by the randomizer at run time and don't exist in the logic
            MMRData.LogicFile logicFile = CreateLogicFile(CondensedLogic);//Create a logic file using the collected data


            LogicObjects.TrackerInstance TestInstance = new LogicObjects.TrackerInstance();
            TestInstance.LogicFile = logicFile;
            TestInstance.LogicDictionary = logicDictionary;

            string testingdir = null;
            if (Directory.Exists(@"C:\Testing\")) { testingdir = @"C:\Testing\"; }
            if (Directory.Exists(@"D:\Testing\")) { testingdir = @"D:\Testing\"; }

            File.WriteAllText(testingdir + @"PMRLogic.json", logicFile.ToString());
            File.WriteAllText(testingdir + @"PMRDict.json", logicDictionary.ToString());
            return TestInstance;
        }

        private static MMRData.LogicFile CreateLogicFile(Dictionary<string, string> CondensedLogic)
        {
            TrackerObjects.MMRData.LogicFile logicFile = new TrackerObjects.MMRData.LogicFile
            {
                GameCode = "PMR",
                Version = 1,
                Logic = new List<TrackerObjects.MMRData.JsonFormatLogicItem>()
            };
            foreach (var i in CondensedLogic)
            {
                TrackerObjects.MMRData.JsonFormatLogicItem logicItem = new TrackerObjects.MMRData.JsonFormatLogicItem();
                logicItem.ConditionalItems = LogicStringParser.ConvertLogicStringToConditional(i.Value, true);
                logicItem.ConditionalItems = DoPMRLogicEdits(logicItem.ConditionalItems);
                logicCleaner.RemoveRedundantConditionals(logicItem);
                logicCleaner.MakeCommonConditionalsRequirements(logicItem);
                logicItem.Id = i.Key;
                logicFile.Logic.Add(logicItem);
            }

            return logicFile;
        }

        private static Dictionary<string, string> CondenseLogic(PMRData MasterData)
        {
            Dictionary<string, string> CondensedLogic = new Dictionary<string, string>();

            foreach (var i in MasterData.itemLocations)
            {
                if (CondensedLogic.ContainsKey(i.ID)) { CondensedLogic[i.ID] += $" | ({i.Logic})"; Debug.WriteLine($"Duplicate Location Entry {i.ID}"); }
                else { CondensedLogic[i.ID] = $"({i.Logic})"; }
            }
            foreach (var i in MasterData.MacroExits)
            {
                var ID = $"{i.ParentAreaID} X {i.ID}";
                if (CondensedLogic.ContainsKey(ID)) { CondensedLogic[ID] += $" | ({i.Logic})"; Debug.WriteLine($"Duplicate Exit Entry {ID}"); }
                else { CondensedLogic[ID] = $"({i.Logic})"; }
            }
            foreach (var i in MasterData.Macros)
            {
                string ID = i.ID;
                if (CondensedLogic.ContainsKey(ID)) { CondensedLogic[ID] += $" | ({i.Logic})"; Debug.WriteLine($"Duplicate Macro Entry {ID}"); }
                else { CondensedLogic[ID] = $"({i.Logic})"; }
            }

            return CondensedLogic;
        }

        private static void AddRootEntrances(LogicDictionaryData.LogicDictionary logicDictionary, Dictionary<string, string> CondensedLogic)
        {
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

        private static LogicDictionaryData.LogicDictionary CreateLogicDictionary(PMRData MasterData)
        {
            return new TrackerObjects.LogicDictionaryData.LogicDictionary
            {
                GameCode = "PMR",
                LogicVersion = 1,
                LogicFormat = "JSON",
                LocationList = MasterData.itemLocations.Select(x => new TrackerObjects.LogicDictionaryData.DictionaryLocationEntries
                {
                    ID = x.ID,
                    Area = x.Area,
                    Name = x.Name,
                    OriginalItem = "Coin",
                    SpoilerData = new TrackerObjects.MMRData.SpoilerlogReference { SpoilerLogNames = x.SpoilerNames.ToArray() },
                    ValidItemTypes = new string[] { "item" }
                }).ToList(),
                AreaList = MasterData.Areas.Select(x => x.ID).ToList(),
                EntranceList = MasterData.MacroExits.Select(x => new TrackerObjects.LogicDictionaryData.DictionaryEntranceEntries
                {
                    Area = x.ParentAreaID,
                    Exit = x.ID,
                    ID = $"{x.ParentAreaID} X {x.ID}",
                    RandomizableEntrance = false
                }).ToList(),
                ItemList = MasterData.Items.Select(x => new TrackerObjects.LogicDictionaryData.DictionaryItemEntries()
                {
                    ID = x.ID,
                    MaxAmountInWorld = x.ID == "Coin" ? -1 : 1,
                    Name = x.Name,
                    ItemTypes = x.Types.ToArray(),
                    SpoilerData = new TrackerObjects.MMRData.SpoilerlogReference { SpoilerLogNames = x.SpoilerNames.ToArray() },
                    ValidStartingItem = true
                }).ToList()
            };
        }

        private static void CreateAreaList(PMRData MasterData)
        {
            List<string> Areas = new List<string>() { "Root" };
            foreach (var i in MasterData.MacroExits)
            {
                if (!Areas.Contains(i.ParentAreaID)) { Areas.Add(i.ParentAreaID); }
                if (!Areas.Contains(i.ID)) { Areas.Add(i.ID); }
            }
            foreach (var i in Areas.OrderBy(x => x)) { MasterData.Areas.Add(new PMRArea { ID = i }); }
        }

        private static void GetDataFromLogicFile(PaperMarioMasterData MasterReference, PMRData MasterData)
        {
            foreach (var i in MasterReference.Logic)
            {
                //Debug.WriteLine($"{i.from.map}_{i.from.id}:{i.to.map}_{i.to.id}");
                if (i.from.id is string || i.to.id is null || i.to.map is null) { continue; }

                string LogicLine = GetLogicDetails(i, MasterReference);

                if (i.pseudoitems is not null && i.pseudoitems.Any())
                {
                    foreach (var Macro in i.pseudoitems)
                    {
                        PMRMacro NewMacro = new PMRMacro();
                        NewMacro.ID = Macro;
                        if (NewMacro.ID == "EQUIPMENT_Hammer_Progressive" && i.from.map.StartsWith("ISK")) { NewMacro.ID += "_1"; }
                        if (NewMacro.ID == "EQUIPMENT_Hammer_Progressive" && i.from.map.StartsWith("KZN")) { NewMacro.ID += "_2"; }
                        if (NewMacro.ID == "EQUIPMENT_Boots_Progressive" && i.from.map.StartsWith("OBK")) { NewMacro.ID += "_1"; }
                        if (NewMacro.ID == "EQUIPMENT_Boots_Progressive" && i.from.map.StartsWith("TIK")) { NewMacro.ID += "_2"; }
                        NewMacro.Area = i.from.map;
                        NewMacro.Logic = LogicLine;
                        MasterData.Macros.Add(NewMacro);
                    }
                }

                if (i.to.id is string) //Item Location
                {
                    PMRItemLocation NewItemLocation = new PMRItemLocation();
                    NewItemLocation.ID = CreateIDName($"{i.from.GetFullArea(MasterReference)}{MasterReference.verbose_item_locations[i.from.map][i.to.id]}{i.to.id}");
                    NewItemLocation.Logic = LogicLine;
                    NewItemLocation.Area = $"{i.from.GetGeneralArea(MasterReference)}";
                    NewItemLocation.Name = $"{MasterReference.verbose_sub_area_names[i.from.map]} - {MasterReference.verbose_item_locations[i.from.map][i.to.id]}";
                    NewItemLocation.SpoilerNames.Add($"{MasterReference.verbose_sub_area_names[i.from.map]} - {MasterReference.verbose_item_locations[i.from.map][i.to.id]}");
                    MasterData.itemLocations.Add(NewItemLocation);
                }
                else if (i.to.id != i.from.id || i.to.map != i.from.map) //Exit
                {
                    PMRExit NewExit = new PMRExit();
                    NewExit.ParentAreaID = i.from.GetFullArea(MasterReference);
                    NewExit.ID = i.to.GetFullArea(MasterReference);
                    NewExit.Logic = LogicLine;
                    MasterData.MacroExits.Add(NewExit);
                }
            }
        }

        private static void AddItems(PaperMarioMasterData MasterReference, PMRData MasterData)
        {
            foreach (var i in MasterReference.items)
            {
                PMRItemData NewItem = new PMRItemData();
                NewItem.ID = i.Key;
                NewItem.Types.Add(i.Value);
                NewItem.Name = i.Key;
                NewItem.SpoilerNames.Add(i.Key);
                if (MasterReference.verbose_item_names.ContainsKey(i.Key))
                {
                    NewItem.Name = MasterReference.verbose_item_names[i.Key];
                    NewItem.SpoilerNames.Add(MasterReference.verbose_item_names[i.Key]);
                }
                MasterData.Items.Add(NewItem);
            }
        }

        private static void AddHardCodedMacros(Dictionary<string, string> CondensedLogic)
        {


            //Add Hard Coded Macros
            CondensedLogic.Add("can_flip_panels", "boots, 1 | hammer, 2");
            CondensedLogic.Add("can_shake_trees", "Bombette | hammer, 0");
            CondensedLogic.Add("has_parakarry_3_letters", "letter, 3");
            CondensedLogic.Add("RF_HiddenBlocksVisible", "HiddenBlocksVisible == true");
            CondensedLogic.Add("RF_ToyboxOpen", "ToyboxOpen == true");
        }

        private static void AddVariables(LogicDictionaryData.LogicDictionary logicDictionary)
        {
            logicDictionary.Variables.Add(new TrackerObjects.LogicDictionaryData.TrackerVariable
            {
                ID = "starspirits",
                Name = "starspirits",
                Static = true,
                Value = new List<string> { "STARSPIRIT_1", "STARSPIRIT_2", "STARSPIRIT_3", "STARSPIRIT_4", "STARSPIRIT_5", "STARSPIRIT_6", "STARSPIRIT_7" }
            });
            logicDictionary.Variables.Add(new TrackerObjects.LogicDictionaryData.TrackerVariable
            {
                ID = "hammer",
                Name = "hammer",
                Static = true,
                Value = new List<string> { "EQUIPMENT_Hammer_Progressive_1", "EQUIPMENT_Hammer_Progressive_2" }
            });
            logicDictionary.Variables.Add(new TrackerObjects.LogicDictionaryData.TrackerVariable
            {
                ID = "boots",
                Name = "boots",
                Static = true,
                Value = new List<string> { "EQUIPMENT_Boots_Progressive_1", "EQUIPMENT_Boots_Progressive_2" }
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
                Value = new List<string> { "StarPiece00", "StarPiece01", "StarPiece02", "StarPiece03", "StarPiece04", "StarPiece05", "StarPiece06", "StarPiece07", "StarPiece08", "StarPiece09", "StarPiece0A", "StarPiece0B", "StarPiece0C", "StarPiece0D", "StarPiece0E", "StarPiece0F", "StarPiece10", "StarPiece11", "StarPiece12", "StarPiece13", "StarPiece14", "StarPiece15", "StarPiece16", "StarPiece17", "StarPiece18", "StarPiece19", "StarPiece1A", "StarPiece1B", "StarPiece1C", "StarPiece1D", "StarPiece1E", "StarPiece1F", "StarPiece20", "StarPiece21", "StarPiece22", "StarPiece23", "StarPiece24", "StarPiece25", "StarPiece26", "StarPiece27", "StarPiece28", "StarPiece29", "StarPiece2A", "StarPiece2B", "StarPiece2C", "StarPiece2D", "StarPiece2E", "StarPiece2F", "StarPiece30", "StarPiece31", "StarPiece32", "StarPiece33", "StarPiece34", "StarPiece35", "StarPiece36", "StarPiece37", "StarPiece38", "StarPiece39", "StarPiece3A", "StarPiece3B", "StarPiece3C", "StarPiece3D", "StarPiece3E", "StarPiece3F", "StarPiece40", "StarPiece41", "StarPiece42", "StarPiece43", "StarPiece44", "StarPiece45", "ThreeStarPieces0", "ThreeStarPieces1", "ThreeStarPieces2", "ThreeStarPieces3", "ThreeStarPieces4" }
            });
        }

        private static void Addoptions(LogicDictionaryData.LogicDictionary logicDictionary)
        {
            //Add options
            logicDictionary.Options.Add(new TrackerObjects.OptionData.TrackerOption
            {
                ID = "starting_area",
                CurrentValue = "Toad Town",
                DisplayName = "Starting Area",
                Values = new Dictionary<string, TrackerObjects.OptionData.actions>()
                {
                    { "Goomba Village", new TrackerObjects.OptionData.actions()},
                    { "Toad Town", new TrackerObjects.OptionData.actions()},
                    { "Dry Dry Outpost", new TrackerObjects.OptionData.actions()},
                    { "Yoshi Village", new TrackerObjects.OptionData.actions()},
                }
            });
            logicDictionary.Options.Add(new TrackerObjects.OptionData.TrackerOption
            {
                ID = "BlueHouseOpen",
                CurrentValue = "false",
                DisplayName = "Open Blue House",
                Values = new Dictionary<string, TrackerObjects.OptionData.actions>()
                {
                    { "true",
                        new TrackerObjects.OptionData.actions() {
                            LogicReplacements = new TrackerObjects.OptionData.LogicReplacement[] {
                                new TrackerObjects.OptionData.LogicReplacement { ReplacementList = new Dictionary<string, string>{{ "GF_MAC02_UnlockedHouse", "true"}} }
                            }
                        }
                    },
                    { "false", new TrackerObjects.OptionData.actions()}
                }
            });
            logicDictionary.Options.Add(new TrackerObjects.OptionData.TrackerOption
            {
                ID = "FlowerGateOpen",
                CurrentValue = "false",
                DisplayName = "Open Flower Gate",
                Values = new Dictionary<string, TrackerObjects.OptionData.actions>()
                {
                    { "true",
                        new TrackerObjects.OptionData.actions() {
                            LogicReplacements = new TrackerObjects.OptionData.LogicReplacement[] {
                                new TrackerObjects.OptionData.LogicReplacement { ReplacementList = new Dictionary<string, string>{{ "RF_Ch6_FlowerGateOpen", "true"}} }
                            }
                        }
                    },
                    { "false", new TrackerObjects.OptionData.actions()}
                }
            });
            logicDictionary.Options.Add(new TrackerObjects.OptionData.TrackerOption
            {
                ID = "WhaleOpen",
                CurrentValue = "false",
                DisplayName = "Open Whale",
                Values = new Dictionary<string, TrackerObjects.OptionData.actions>()
                {
                    { "true",
                        new TrackerObjects.OptionData.actions() {
                            LogicReplacements = new TrackerObjects.OptionData.LogicReplacement[] {
                                new TrackerObjects.OptionData.LogicReplacement { ReplacementList = new Dictionary<string, string>{{ "RF_CanRideWhale", "true"}} }
                            }
                        }
                    },
                    { "false", new TrackerObjects.OptionData.actions()}
                }
            });
            logicDictionary.Options.Add(new TrackerObjects.OptionData.TrackerOption
            {
                ID = "ToyboxOpen",
                CurrentValue = "false",
                DisplayName = "Open Toy Box",
                Values = new Dictionary<string, TrackerObjects.OptionData.actions>()
                {
                    { "true", new TrackerObjects.OptionData.actions()},
                    { "false", new TrackerObjects.OptionData.actions()}
                }
            });
            logicDictionary.Options.Add(new TrackerObjects.OptionData.TrackerOption
            {
                ID = "HiddenBlocksVisible",
                CurrentValue = "true",
                DisplayName = "Hidden Blocks Visible",
                Values = new Dictionary<string, TrackerObjects.OptionData.actions>()
                {
                    { "true", new TrackerObjects.OptionData.actions()},
                    //{ "false", new TrackerObjects.OptionData.actions()}
                }
            });
        }

        public static List<List<string>> DoPMRLogicEdits(List<List<string>> Conditionals)
        {
            var NewCond = Conditionals.Select(x => x.Select(y => y));
            //Fix Star Spirit Requirements
            NewCond = NewCond.Select(x => x.Select(y => y.StartsWith("starspirits_") ? y.Replace("_", ", ") : y));
            //Fix Star Piece Requirements
            NewCond = NewCond.Select(x => x.Select(y => y.StartsWith("starpieces_") ? y.Replace("_", ", ") : y));
            //Fix Boots Requirements
            NewCond = NewCond.Select(x => x.Select(y => y.StartsWith("boots_") ? y.Replace("_", ", ") : y));
            //Fix Hammer Requirements
            NewCond = NewCond.Select(x => x.Select(y => y.StartsWith("hammer_") ? y.Replace("_", ", ") : y));
            return NewCond.Select(x => x.ToList()).ToList();
        }

        public static string GetLogicDetails(PaperMarioLogicJSON i, PaperMarioMasterData masterData)
        {
            List<List<string>> NewLogicSets = new List<List<string>>() { new List<string> { i.from.GetFullArea(masterData) } };
            foreach (var req in i.reqs)
            {
                List<string> set = new List<string>();
                if (req is string StringReq) { set.Add(StringReq); } //Flag
                else
                {
                    foreach (KeyValuePair<string, object> d in req.ToObject<Dictionary<string, object>>())
                    {
                        if (d.Value is string) { set.Add($"{d.Value}"); } //Standard Requirement
                        else if (d.Value is Int64) { set.Add($"{d.Key}_{d.Value}"); } //Requirement with count
                        else
                        {
                            foreach (string st in d.Value as Newtonsoft.Json.Linq.JArray) { set.Add($"{st}"); } //Multiple Requirements of the same type
                        }
                    }
                }
                NewLogicSets.Add(set);
            }
            return string.Join(" & ", NewLogicSets.Where(x => x.Any()).Select(x => "(" + string.Join(" | ", x) + ")"));
        }
    }
}
