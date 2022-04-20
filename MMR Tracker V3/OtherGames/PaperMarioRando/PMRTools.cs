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
            public string OriginalItem;
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
            //PaperMarioMasterData MasterReference = JsonConvert.DeserializeObject<PaperMarioMasterData>(wc.DownloadString(@"https://raw.githubusercontent.com/Thedrummonger/MMR-Tracker-V3/master/MMR%20Tracker%20V3/OtherGames/PaperMarioRandoLogic.json"));
            PaperMarioMasterData MasterReference = JsonConvert.DeserializeObject<PaperMarioMasterData>(File.ReadAllText(@"D:\Visual Studio Code Stuff\MMR-Tracker-V3\MMR Tracker V3\OtherGames\PaperMarioRandoLogic.json"));

            PMRData MasterData = new PMRData();

            AddItems(MasterReference, MasterData); //Add Items from the Ref Sheet
            GetDataFromLogicFile(MasterReference, MasterData); //Add Locations, Macro and entrances from the Ref Sheet
            CreateAreaList(MasterData); //Create a list of all areas

            LogicDictionaryData.LogicDictionary logicDictionary = CreateLogicDictionary(MasterData); //Create the logic dictionary using the collected data
            AddVariables(logicDictionary);//Add variables
            Addoptions(logicDictionary);//add options

            Dictionary<string, string> CondensedLogic = CondenseLogic(MasterData);//Condence the logic combining all duplicate entries
            AddRootEntrances(logicDictionary, CondensedLogic);//Add entrance Macros that detail starting area access
            AddCustomLocations(logicDictionary, CondensedLogic);//Add entrance Macros that detail starting area access
            AddHardCodedMacros(CondensedLogic);//Add macros that are created by the randomizer at run time and don't exist in the logic
            MMRData.LogicFile logicFile = CreateLogicFile(CondensedLogic);//Create a logic file using the collected data

            //Why
            var StartpieceEntry = logicDictionary.ItemList.First(x => x.ID == "StarPiece");
            StartpieceEntry.SpoilerData.SpoilerLogNames = StartpieceEntry.SpoilerData.SpoilerLogNames.Concat(new string[] { "Starpiece", "StarPiece21" }).ToArray();
            var FPPlusEntry = logicDictionary.ItemList.First(x => x.ID == "FPPlus");
            FPPlusEntry.SpoilerData.SpoilerLogNames = FPPlusEntry.SpoilerData.SpoilerLogNames.Concat(new string[] { "FPPlusC" }).ToArray();
            var KPALowerJailYellowBlock = logicDictionary.LocationList.First(x => x.ID == "BowsersCastleOutsideLowerJailNoLava4YellowBlockYBlockA");
            KPALowerJailYellowBlock.SpoilerData.SpoilerLogNames = KPALowerJailYellowBlock.SpoilerData.SpoilerLogNames.Concat(new string[] { "Outside Lower Jail - Yellow Block" }).ToArray();
            var KPALowerJailKoopatrol= logicDictionary.LocationList.First(x => x.ID == "BowsersCastleOutisdeLowerJailLava0DefeatKoopatrolRewardItemA");
            KPALowerJailKoopatrol.SpoilerData.SpoilerLogNames = KPALowerJailKoopatrol.SpoilerData.SpoilerLogNames.Concat(new string[] { "Outside Lower Jail - Defeat Koopatrol Reward" }).ToArray();
            var FFHPPlusBlock = logicDictionary.LocationList.First(x => x.ID == "ForeverForestBeeHiveHPPlus1CentralBlockRBlockA");
            FFHPPlusBlock.SpoilerData.SpoilerLogNames = FFHPPlusBlock.SpoilerData.SpoilerLogNames.Concat(new string[] { "Laughing Rock - Central Block" }).ToArray();

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

        private static void AddCustomLocations(LogicDictionaryData.LogicDictionary logicDictionary, Dictionary<string, string> condensedLogic)
        {
            //This location is missed because of a strage techincallity in logic. The only way to this area is from an "item" area, which is skipped by this code
            //And assumed to be just for item access, this looks to be the only instance of it but it may cme back to bite me later.
            condensedLogic.Add("MtRuggedMtRugged20ParakarryLedgeItemA", "Mt Rugged - Mt Rugged 2 - 0 & CanUseParakarry");
            logicDictionary.LocationList.Add(new LogicDictionaryData.DictionaryLocationEntries
            {
                ID = "MtRuggedMtRugged20ParakarryLedgeItemA",
                Name = "Mt Rugged 2 - Parakarry Ledge",
                Area = "Mt Rugged",
                ValidItemTypes = new string[] { "item" },
                SpoilerData =  new MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { "Mt Rugged 2 - Parakarry Ledge" } }
            });
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
                logicItem.ConditionalItems = DoPMRLogicEdits(logicItem.ConditionalItems, i.Key);
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
                    OriginalItem = x.OriginalItem,
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
                    MaxAmountInWorld = -1,
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
            string[] starSpirits = new string[] { "Eldstar", "Mamar", "Skolar", "Muskular", "Misstar", "Klevar", "Kalmar" };
            foreach (var i in MasterReference.Logic)
            {
                if (i.to.id is string && i.from.id is string) { Debug.WriteLine($"WARNING I DIDNT EXPECT THIS {i.to.map}:{i.to.id} -> {i.from.map}:{i.from.id}"); }

                //Debug.WriteLine($"{i.from.map}_{i.from.id}:{i.to.map}_{i.to.id}");
                if (i.from.id is string || i.to.id is null || i.to.map is null) { continue; }

                string LogicLine = GetLogicDetails(i, MasterReference);

                if (i.pseudoitems is not null && i.pseudoitems.Any())
                {
                    foreach (var Macro in i.pseudoitems)
                    {
                        if (Macro.StartsWith("StarPiece_")) { addChuckQuizmoCheck(Macro, i, LogicLine); }
                        else if (Macro == "EQUIPMENT_Hammer_Progressive" || Macro == "EQUIPMENT_Boots_Progressive") { addProgressiveEquipmenetCheck(Macro, i, LogicLine); }
                        else if (Macro == "RF_SavedYoshiKid") { addYoshiKidCheck(Macro, i, LogicLine); }
                        else if (Macro.StartsWith("STARSPIRIT_")) { addStarSpiritCheck(Macro, i, LogicLine); }
                        else
                        {
                            PMRMacro NewMacro = new PMRMacro();
                            NewMacro.ID = Macro;
                            NewMacro.Area = i.from.map;
                            NewMacro.Logic = LogicLine;
                            MasterData.Macros.Add(NewMacro);
                        }
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

                    //Stupid edge case of an item being accessable from two different location resulting in the location existing twice
                    if (NewItemLocation.ID == "DryDryRuinsSandDrainageRoom33OnLedgeItemA") { NewItemLocation.ID = "DryDryRuinsSandDrainageRoom32OnLedgeItemA"; }

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

            void addChuckQuizmoCheck(string Name, PaperMarioLogicJSON i, string LogicLine)
            {
                var Sets = Name.Split("_");
                PMRItemLocation NewItemLocation = new PMRItemLocation();
                NewItemLocation.ID = Name;
                NewItemLocation.Name = $"{MasterReference.verbose_sub_area_names[i.from.map]} - Chuck Quizmo Quiz {Sets[2]}";
                NewItemLocation.Logic = LogicLine;
                NewItemLocation.Area = $"{i.from.GetGeneralArea(MasterReference)}";
                NewItemLocation.SpoilerNames.Add($"{MasterReference.verbose_sub_area_names[i.from.map]} - Chuck Quizmo Quiz {Sets[2]}");
                NewItemLocation.OriginalItem = $"StarPiece";
                MasterData.itemLocations.Add(NewItemLocation);
            }

            void addProgressiveEquipmenetCheck(string Name, PaperMarioLogicJSON i, string LogicLine)
            {
                var Sets = Name.Split("_");
                PMRItemLocation NewItemLocation = new PMRItemLocation();
                NewItemLocation.ID = CreateIDName($"{i.from.GetFullArea(MasterReference)}{Name}");
                NewItemLocation.Name = $"{MasterReference.verbose_sub_area_names[i.from.map]} - {Sets[2]} {Sets[1]}";
                NewItemLocation.Logic = LogicLine;
                NewItemLocation.Area = $"{i.from.GetGeneralArea(MasterReference)}";
                NewItemLocation.SpoilerNames.Add($"{MasterReference.verbose_sub_area_names[i.from.map]} - {i.to.id}");
                NewItemLocation.OriginalItem = $"{Sets[2]}{Sets[1]}";
                MasterData.itemLocations.Add(NewItemLocation);
            }

            void addYoshiKidCheck(string Name, PaperMarioLogicJSON i, string LogicLine)
            {
                PMRItemLocation NewItemLocation = new PMRItemLocation();
                NewItemLocation.ID = CreateIDName($"{i.from.GetFullArea(MasterReference)}{Name}");
                NewItemLocation.Name = $"{MasterReference.verbose_sub_area_names[i.from.map]} - Yoshi Kid";
                NewItemLocation.Logic = LogicLine;
                NewItemLocation.Area = $"{i.from.GetGeneralArea(MasterReference)}";
                NewItemLocation.SpoilerNames.Add($"{MasterReference.verbose_sub_area_names[i.from.map]} - Yoshi Kid");
                NewItemLocation.OriginalItem = $"YoshiKid";
                MasterData.itemLocations.Add(NewItemLocation);
            }

            void addStarSpiritCheck(string Name, PaperMarioLogicJSON i, string LogicLine)
            {
                var Sets = Name.Split("_");
                PMRItemLocation NewItemLocation = new PMRItemLocation();
                NewItemLocation.ID = CreateIDName($"{i.from.GetFullArea(MasterReference)}{Name}");
                NewItemLocation.Name = $"{MasterReference.verbose_sub_area_names[i.from.map]} - Star Spirit";
                NewItemLocation.Logic = LogicLine;
                NewItemLocation.Area = $"{i.from.GetGeneralArea(MasterReference)}";
                NewItemLocation.SpoilerNames.Add($"{MasterReference.verbose_sub_area_names[i.from.map]} - Star Spirit");
                NewItemLocation.OriginalItem = starSpirits[int.Parse(Sets[1]) - 1];
                MasterData.itemLocations.Add(NewItemLocation);
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
            CondensedLogic.Add("RF_CanGetBombette", "starting_partner_Bombette == false");
            CondensedLogic.Add("RF_CanGetBow", "starting_partner_Bow == false");
            CondensedLogic.Add("RF_CanGetGoombario", "starting_partner_Goombario == false");
            CondensedLogic.Add("RF_CanGetKooper", "starting_partner_Kooper == false");
            CondensedLogic.Add("RF_CanGetLakilester", "starting_partner_Lakilester == false");
            CondensedLogic.Add("RF_CanGetParakarry", "starting_partner_Parakarry == false");
            CondensedLogic.Add("RF_CanGetSushie", "starting_partner_Sushie == false");
            CondensedLogic.Add("RF_CanGetWatt", "starting_partner_Watt == false");
            CondensedLogic.Add("RF_Missable", "true");
            CondensedLogic.Add("saved_all_yoshikids", "YoshiKid, 5");
            CondensedLogic.Add("can_flip_panels", "ProgressiveBoots, 1 | ProgressiveHammer, 2");
            CondensedLogic.Add("can_shake_trees", "Bombette | ProgressiveHammer, 0");
            CondensedLogic.Add("has_parakarry_3_letters", "letter, 3");
            CondensedLogic.Add("RF_HiddenBlocksVisible", "HiddenBlocksVisible == true");
            CondensedLogic.Add("RF_ToyboxOpen", "ToyboxOpen == true");
            CondensedLogic.Add("CanUseGoombario", "partners_always_usable == true | Goombario | starting_partner_Goombario == true");
            CondensedLogic.Add("CanUseKooper", "partners_always_usable == true | Kooper | starting_partner_Kooper == true");
            CondensedLogic.Add("CanUseBombette", "partners_always_usable == true | Bombette | starting_partner_Bombette == true");
            CondensedLogic.Add("CanUseParakarry", "partners_always_usable == true | Parakarry | starting_partner_Parakarry == true");
            CondensedLogic.Add("CanUseWatt", "partners_always_usable == true | Watt | starting_partner_Watt == true");
            CondensedLogic.Add("CanUseSushie", "partners_always_usable == true | Sushie | starting_partner_Sushie == true");
            CondensedLogic.Add("CanUseLakilester", "partners_always_usable == true | Lakilester | starting_partner_Lakilester == true");
            CondensedLogic.Add("CanUseBow", "partners_always_usable == true | Bow | starting_partner_Bow == true");
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
        }

        private static void Addoptions(LogicDictionaryData.LogicDictionary logicDictionary)
        {
            AddOption("starting_area", "Toad Town", "Starting Area", new string[] { "Goomba Village", "Toad Town", "Dry Dry Outpost", "Yoshi Village" });

            AddToggleOption("BlueHouseOpen", "false", "Open Blue House", CreateSimpleReplacementOption("GF_MAC02_UnlockedHouse"));
            AddToggleOption("FlowerGateOpen", "false", "Open Flower Gate", CreateSimpleReplacementOption("RF_Ch6_FlowerGateOpen"));
            AddToggleOption("WhaleOpen", "false", "Open Whale", CreateSimpleReplacementOption("RF_CanRideWhale"));
            AddToggleOption("ToyboxOpen", "false", "Open Toy Box");

            AddToggleOption("HiddenBlocksVisible", "true", "Hidden Blocks Always Visible");

            AddStartingPartnerOption("Goombario");
            AddStartingPartnerOption("Kooper");
            AddStartingPartnerOption("Bombette");
            AddStartingPartnerOption("Parakarry");
            AddStartingPartnerOption("Watt");
            AddStartingPartnerOption("Sushie");
            AddStartingPartnerOption("Lakilester");
            AddStartingPartnerOption("Bow");
            AddToggleOption("partners_always_usable", "false", "Partners Always useable");


            void AddStartingPartnerOption(string PartnerName) { AddToggleOption($"starting_partner_{PartnerName}", "false", $"Start With {PartnerName}"); }

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

        public static List<List<string>> DoPMRLogicEdits(List<List<string>> Conditionals, string CheckID)
        {
            Dictionary<string, string> PartnerOverrides = new Dictionary<string, string>{
                { "Goombario", "CanUseGoombario" },
                { "Kooper", "CanUseKooper" },
                { "Bombette", "CanUseBombette" },
                { "Parakarry", "CanUseParakarry" },
                { "Watt", "CanUseWatt" },
                { "Sushie", "CanUseSushie" },
                { "Lakilester", "CanUseLakilester" },
                { "Bow", "CanUseBow" },
            };

            var NewCond = Conditionals.Select(x => x.Select(y => y));
            //Fix Star Spirit Requirements
            NewCond = NewCond.Select(x => x.Select(y => y.StartsWith("starspirits_") ? y.Replace("_", ", ") : y));
            //Fix Star Piece Requirements
            NewCond = NewCond.Select(x => x.Select(y => y.StartsWith("starpieces_") ? y.Replace("_", ", ").Replace("starpieces", "StarPiece") : y));
            //Fix Boots Requirements
            NewCond = NewCond.Select(x => x.Select(y => y.StartsWith("boots_") ? y.Replace("boots_", "ProgressiveBoots, ") : y));
            //Fix Hammer Requirements
            NewCond = NewCond.Select(x => x.Select(y => y.StartsWith("hammer_") ? y.Replace("hammer_", "ProgressiveHammer, ") : y));
            //Override Partner Entries to fix Issues
            NewCond = NewCond.Select(x => x.Select(y => PartnerOverrides.ContainsKey(y) && CheckID != PartnerOverrides[y] ? PartnerOverrides[y] : y));
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
            foreach(var i in spoilerLog)
            {
                string line = i;
                if (!line.Contains(":")) { continue; }
                var data = line.Split(new string[] { "):" }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
                data[0] = data[0][1..];
                if (data.Length < 2 || string.IsNullOrWhiteSpace(data[1])) { continue; }

                if (data[1].StartsWith("ThreeStarPieces")) { data[1] = "StarPiece"; }

                var Location = Instance.LocationPool.Values.FirstOrDefault(x => x.GetDictEntry(Instance).SpoilerData.SpoilerLogNames.Contains(data[0]) && !AppliedLocations.Contains(x.ID));
                var Item = Instance.ItemPool.Values.FirstOrDefault(x => x.GetDictEntry(Instance).SpoilerData.SpoilerLogNames.Contains(data[1]));

                if (Location == null) { Debug.WriteLine($"Could not find Location {data[0]}"); continue; }
                if (Item == null) { Debug.WriteLine($"Could not find item {data[1]}"); continue; }
                Location.Randomizeditem.SpoilerLogGivenItem = Item.Id;
                AppliedLocations.Add(Location.ID);
            }

            foreach(var i in Instance.LocationPool)
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
                    if (i.Key.StartsWith("StarPiece_")) { i.Value.Randomizeditem.SpoilerLogGivenItem = "StarPiece"; }
                    else if (i.Key.EndsWith("EQUIPMENT_Hammer_Progressive") || i.Key.EndsWith("EQUIPMENT_Boots_Progressive") || i.Key.EndsWith("RF_SavedYoshiKid") || i.Key.Contains("STARSPIRIT_")) 
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

            Dictionary<int, string> StartingAreas = new Dictionary<int, string>() { { 257, "Goomba Village" }, { 65796, "Toad Town" }, { 590080, "Dry Dry Outpost"  }, { 1114882, "Yoshi Village" } };

            var StartingAreaOption = Instance.UserOptions["starting_area"];
            int SettingStartingMap = (int)SettingData["StartingMap"];
            string MappedTrackerArea = StartingAreas[SettingStartingMap];

            StartingAreaOption.CurrentValue = MappedTrackerArea;

        }
    }

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
            public string OriginalItem;
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

        public static void CreatePMRdata()
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


            string OutputDict = Path.Combine(References.TestingPaths.GetDevTestingPath(), "PMRDict.json");
            File.WriteAllText(OutputDict, JsonConvert.SerializeObject(PRMDict, Testing._NewtonsoftJsonSerializerOptions));

            string OutputLogic = Path.Combine(References.TestingPaths.GetDevTestingPath(), "PMRLogic.json");
            File.WriteAllText(OutputLogic, JsonConvert.SerializeObject(LogicMapping, Testing._NewtonsoftJsonSerializerOptions));

        }

        private static void SortMacros(LogicDictionaryData.LogicDictionary pRMDict, PaperMarioMasterData refFileObject, Dictionary<string, string> logicMapping)
        {
            string[] starSpirits = new string[] { "Twink", "Eldstar", "Mamar", "Skolar", "Muskular", "Misstar", "Klevar", "Kalmar" };
            List<PaperMarioLogicJSON> Logic = refFileObject.Logic.SelectMany(x => x).ToList();
            foreach (var entry in Logic.Where(x => x.pseudoitems is not null && x.pseudoitems.Any()))
            {
                string CurrentLogic = $"({refFileObject.verbose_area_names[GetBaseMap(entry.to.map)]} - {refFileObject.verbose_sub_area_names[entry.to.map]} - {entry.to.id})";
                if (entry.reqs is not null && entry.reqs.Any()) { CurrentLogic += $" & ({string.Join(" & ", entry.reqs.Select(x => string.Join(" | ", x))) })"; }

                foreach (var i in entry.pseudoitems)
                {
                    if (pRMDict.MacroList.Any(x => x.ID == i)) { Debug.WriteLine($"{i} Already existed in macro pool"); continue; }
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
                if (pRMDict.LocationList.Any(x => x.ID == LocationEntry.ID)) { return; }

                logicMapping.Add(LocationEntry.ID, CurrentLogic);
                pRMDict.LocationList.Add(LocationEntry);
            }
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

        private static void CreateLocationList(LogicDictionaryData.LogicDictionary pRMDict, PaperMarioMasterData refFileObject, Dictionary<string, string> logicMapping)
        {
            List<LogicDictionaryData.DictionaryLocationEntries> LocationList = new List<LogicDictionaryData.DictionaryLocationEntries>();
            foreach (var area in refFileObject.verbose_item_locations)
            {
                foreach(var loc in area.Value)
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
                        SpoilerData = new MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { $"({refFileObject.verbose_sub_area_names[area.Key]} - {loc.Value})" } }
                    };
                    LocationList.Add(LocationEntry);

                    var RequiredArea = $"{refFileObject.verbose_area_names[GetBaseMap(area.Key)]} - {refFileObject.verbose_sub_area_names[area.Key]} - {loc.Key}";
                    if (!pRMDict.AreaList.Contains(RequiredArea)) { Debug.WriteLine($"{RequiredArea} Was not valid"); }
                    else { logicMapping.Add(ID, RequiredArea); }
                }
            }
            pRMDict.LocationList = LocationList;
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
                if (EntranceList.Any(x => x.ID == ID)) { Debug.WriteLine($"{ID} Already existed in macro pool"); continue; }
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
                EntranceList.Add(entranceEntry);
            }
            pRMDict.EntranceList = EntranceList;
        }

        private static void MakeAreaList(LogicDictionaryData.LogicDictionary pRMDict, PaperMarioMasterData refFileObject)
        {
            List<string> MasterAreaList = new List<string>();
            List<PaperMarioLogicJSON> Logic = refFileObject.Logic.SelectMany(x => x).ToList();
            foreach(var i in Logic)
            {
                if (i.to.map == null) { continue; }
                MasterAreaList.Add(i.from.ConvertToAreaName(refFileObject));
                MasterAreaList.Add(i.to.ConvertToAreaName(refFileObject));
            }
            pRMDict.AreaList = MasterAreaList.Distinct().OrderBy(x => x).ToList();
        }

        private static string ConvertToAreaName(this PMRLogicArea area, PaperMarioMasterData refFileObject)
        {
            var BaseMap = GetBaseMap(area.map);
            var SubMap = area.map;
            var SubMapSection = area.id;
            return $"{refFileObject.verbose_area_names[BaseMap]} - {refFileObject.verbose_sub_area_names[SubMap]} - {SubMapSection}";
        }

        private static string GetBaseMap(string map)
        {
            return map.Split('_')[0];
        }
    }
}
