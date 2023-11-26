using MathNet.Symbolics;
using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using static MMR_Tracker_V3.TrackerObjects.MMRData;

namespace TestingForm.GameDataCreation.MMR
{
    public class GenData
    {
        public static LogicStringParser MMRLogicStringParser = new LogicStringParser();
        public static LogicDictionaryData.LogicDictionary CreateMMRFiles()
        {
            MMRExportClass.MMRData ExportData = JsonConvert.DeserializeObject<MMRExportClass.MMRData>(File.ReadAllText(Path.Combine(TestingReferences.GetOtherGameDataPath("MMR"), "MMRDataExport.json")));
            LogicDictionaryData.LogicDictionary MMRDictV16 = new LogicDictionaryData.LogicDictionary
            {
                GameCode = "MMR",
                LogicVersion = 24,
                WinCondition = "OtherCredits"
            };
            ReadLocationData(ExportData, MMRDictV16);
            ReadItemData(ExportData, MMRDictV16);
            ReadMacroData(ExportData, MMRDictV16);
            ReadHintData(ExportData, MMRDictV16);
            ReadEntranceData(ExportData);
            AddTrackerLogic(MMRDictV16);
            AddLogicCollections(MMRDictV16);
            CreateLogicSettings(ExportData, MMRDictV16);

            return MMRDictV16;


        }

        private static void CreateLogicSettings(MMRExportClass.MMRData exportData, LogicDictionaryData.LogicDictionary MMRDictV16)
        {
            void AddSimpleToggle(string ID, string Name, bool Default)
            {
                MMRDictV16.ToggleOptions.Add(ID, new OptionData.ToggleOption { ID = ID, Value = Default.ToString().ToLower(), Name = Name }.CreateSimpleValues(true));
            }
            void AddSimpleMultiSelect(string ID, string Name, params string[] Values)
            {
                MMRDictV16.MultiSelectOptions.Add(ID, new OptionData.MultiSelectOption { ID = ID, Name = Name, EnabledValues = new HashSet<string>() }.CreateSimpleValues(Values));
            }
            void AddSimpleChoice(string ID, string Name, string Default, params string[] Values)
            {
                MMRDictV16.ChoiceOptions.Add(ID, new OptionData.ChoiceOption { ID = ID, Name = Name, Value = Default }.CreateSimpleValues(Values));
            }
            OptionData.AdditionalLogic CreateAdditionalLogic(OptionData.OptionValue Value)
            {
                Value.Actions.AdditionalLogic = Value.Actions.AdditionalLogic.Concat(new OptionData.AdditionalLogic[] { new OptionData.AdditionalLogic() }).ToArray();
                return Value.Actions.AdditionalLogic.Last();
            }
            void AddAdditionalRequirement(OptionData.AdditionalLogic Value, params string[] Additions)
            {
                Value.AdditionalRequirements = Value.AdditionalRequirements.Concat(Additions).ToArray();
            }
            void AddAdditionalConditional(OptionData.AdditionalLogic Value, params string[] Additions)
            {
                Value.AdditionalConditionals = Value.AdditionalConditionals.Concat(Additions.Select(x => x.Split("|").ToList())).ToArray();
            }

            OptionData.LogicReplacement CreateLogicReplacement(OptionData.OptionValue Value)
            {
                Value.Actions.LogicReplacements = Value.Actions.LogicReplacements.Concat(new OptionData.LogicReplacement[] { new OptionData.LogicReplacement() }).ToArray();
                return Value.Actions.LogicReplacements.Last();
            }
            void AddLogicReplacement(OptionData.LogicReplacement entry, params string[] Replacements)
            {
                foreach (var Replacement in Replacements)
                {
                    string[] Segments = Replacement.Split('|');
                    entry.ReplacementList.Add(Segments[0], Segments[1]);
                };
            }
            //==========================================================================================================================================================================
            //Keysy Options
            AddSimpleMultiSelect("SmallKeyMode", "Small Key Mode", "DoorsOpen", "KeepWithinTemples", "KeepWithinArea", "KeepWithinOverworld", "KeepThroughTime");
            string[] AllSmallKeys = MMRDictV16.ItemList.Values.Where(x => x.Name.EndsWith("Small Key")).Select(x => x.ID + "|true").ToArray();
            AddLogicReplacement(CreateLogicReplacement(MMRDictV16.MultiSelectOptions["SmallKeyMode"].ValueList["DoorsOpen"]), AllSmallKeys);

            AddSimpleMultiSelect("BossKeyMode", "Boss Key Mode", "DoorsOpen", "GreatFairyRewards", "KeepWithinTemples", "KeepWithinArea", "KeepWithinOverworld", "KeepThroughTime");
            string[] AllBossKeys = MMRDictV16.ItemList.Values.Where(x => x.Name.EndsWith("Boss Key")).Select(x => x.ID + "|true").ToArray();
            AddLogicReplacement(CreateLogicReplacement(MMRDictV16.MultiSelectOptions["BossKeyMode"].ValueList["DoorsOpen"]), AllBossKeys);

            //==========================================================================================================================================================================
            //Victory Mode Options
            AddSimpleMultiSelect("VictoryMode", "Victory Mode", "DirectToCredits", "CantFightMajora", "Fairies", "SkullTokens", "NonTransformationMasks", "TransformationMasks", "Notebook", "Hearts", "BossRemains");
            
            //==========================================================================================================================================================================
            //Hint Options
            AddSimpleToggle("FreeHints", "Free Hints", true);
            AddSimpleToggle("FreeGaroHints", "Free Garo Hints", false);
            var RemoveMaskTruthGossipEdit = CreateLogicReplacement(MMRDictV16.ToggleOptions["FreeHints"].Enabled);
            var RemoveMaskTruthGaroEdit = CreateLogicReplacement(MMRDictV16.ToggleOptions["FreeGaroHints"].Enabled);
            AddLogicReplacement(RemoveMaskTruthGossipEdit, "MaskTruth|true");
            AddLogicReplacement(RemoveMaskTruthGaroEdit, "MaskTruth|true");
            RemoveMaskTruthGossipEdit.LocationWhitelist = MMRDictV16.HintSpots.Keys.Where(x => x.StartsWith("Gossip")).ToArray();
            RemoveMaskTruthGaroEdit.LocationWhitelist = MMRDictV16.HintSpots.Keys.Where(x => x.StartsWith("HintGaro")).ToArray();

            //==========================================================================================================================================================================
            //Boss Remian Moon Requirement
            MMRDictV16.IntOptions.Add("RequiredBossRemains", new OptionData.IntOption
            {
                ID = "RequiredBossRemains",
                Name = "Required Boss Remains",
                Max = 4,
                Min = 0,
                Value = 4
            });

            //==========================================================================================================================================================================
            //BYOAmmo Options
            void AddBYOAAdditionalLogic(string Location, string NewLogic)
            {
                var LogicAddition = CreateAdditionalLogic(MMRDictV16.ToggleOptions["ByoAmmo"].Enabled);
                AddAdditionalRequirement(LogicAddition, NewLogic);
                LogicAddition.LocationWhitelist = new string[] { Location };
            }

            AddSimpleToggle("ByoAmmo", "Bring Your Own Ammo", false);
            AddBYOAAdditionalLogic("UpgradeBigQuiver", "MMRTArrows40");
            AddBYOAAdditionalLogic("UpgradeBiggestQuiver", "MMRTArrows40");
            AddBYOAAdditionalLogic("HeartPieceSwampArchery", "MMRTArrows40");
            AddBYOAAdditionalLogic("HeartPieceTownArchery", "MMRTArrows50");
            AddBYOAAdditionalLogic("HeartPieceHoneyAndDarling", "MMRTbombchu10");
            AddBYOAAdditionalLogic("MaskRomani", "MMRTEscortCremia");


            //==========================================================================================================================================================================
            //Progressive Item Options
            Dictionary<string, string> ProgressiveItemData = new Dictionary<string, string>();
            foreach(var i in exportData.Items)
            {
                if (string.IsNullOrWhiteSpace(i.ProgressiveGroup)) { continue; }
                ProgressiveItemData[i.ID] = $"{i.ProgressiveGroup}|MMRTProgressive{i.ID}" ;
            }

            AddSimpleToggle("ProgressiveUpgrades", "Progressive Upgrades", false);
            var ProgressiveUpgradesReplacements = CreateLogicReplacement(MMRDictV16.ToggleOptions["ProgressiveUpgrades"].Enabled);
            AddLogicReplacement(ProgressiveUpgradesReplacements, ProgressiveItemData.Select(x => $"{x.Key}|{x.Value.Split('|')[1]}").ToArray());
            ProgressiveUpgradesReplacements.LocationBlacklist = ProgressiveItemData.Values.Select(x => x.Split('|')[1]).ToArray();
            MMRDictV16.ToggleOptions["ProgressiveUpgrades"].Enabled.Actions.ItemNameOverride = ProgressiveItemData.ToDictionary(x => x.Key, x => x.Value.Split('|')[0]);

            //==========================================================================================================================================================================
            //Bombchu Drops Option

            AddSimpleToggle("BombchuDrops", "Bombchu Drops", false);
            var BombchuDropMMRTbombchu10 = CreateAdditionalLogic(MMRDictV16.ToggleOptions["BombchuDrops"].Enabled);
            AddAdditionalConditional(BombchuDropMMRTbombchu10, "ChestIkanaSecretShrineGrotto", "ChestTerminaGrottoBombchu", "ChestGreatBayCapeGrotto", "ChestGraveyardGrotto", "ChestToIkanaGrotto", "ChestToGoronRaceGrotto");
            BombchuDropMMRTbombchu10.LocationWhitelist = new string[] { "MMRTbombchu10", "Any Bombchu Pack" };

            //==========================================================================================================================================================================
            //Simple Options

            AddSimpleToggle("CloseCows", "Close Cows", false);
            AddSimpleToggle("IronGoron", "Iron Goron", false);
            AddSimpleToggle("ClimbMostSurfaces", "Climb Most Surfaces", false);
            AddSimpleToggle("HookshotAnySurface", "Hookshot Any Surface", false);
            AddSimpleToggle("GiantMaskAnywhere", "Giant Mask Anywhere", false);
            AddSimpleToggle("RandomizeEnemies", "Randomize Enemies", false);
            AddSimpleChoice("DamageMode", "Damage Mode", "Default", "Default", "Double", "Quadruple", "OHKO", "Doom");
            AddSimpleToggle("EnableSunsSong", "Enable Suns Song", false);
            AddSimpleToggle("AllowFierceDeityAnywhere", "Fierce Deity Anywhere", false);
            AddSimpleToggle("DeathMoonCrash", "Death is Moon Crash", false);
            AddSimpleChoice("Character", "Character Model", "LinkMM", "LinkMM", "LinkOOT", "AdultLink", "Kafei");
            AddSimpleMultiSelect("PriceMode", "Price Mode", "Purchases", "Minigames", "Misc", "AccountForRoyalWallet");
            AddSimpleChoice("BlastMaskCooldown", "Blast Mask Cooldown", "Default", "Default", "Instant", "VeryShort", "Short", "Long", "VeryLong");

            //==========================================================================================================================================================================
            //Static Edits

            AddSimpleChoice("StaticEdits", "Do Static Edits", "Static", "Static");
            var StaticLogicBeanReplacements = CreateLogicReplacement(MMRDictV16.ChoiceOptions["StaticEdits"].ValueList["Static"]);
            StaticLogicBeanReplacements.LocationWhitelist = new string[] { "ShopItemBusinessScrubMagicBeanInSwamp", "ShopItemBusinessScrubMagicBeanInTown" };
            AddLogicReplacement(StaticLogicBeanReplacements, "OtherMagicBean|MMRTCanBuyFromBeanScrub");
        }

        private static void AddLogicCollections(LogicDictionaryData.LogicDictionary MMRDictV16)
        {
            void AddVariable(string name, IEnumerable<string> value)
            {
                MMRDictV16.LogicEntryCollections.Add(name, new OptionData.LogicEntryCollection { ID = name, Entries = value.ToList() });
            }
            AddVariable("SwampFairies", MMRDictV16.ItemList.Keys.Where(x => x.StartsWith("CollectibleStrayFairyWoodfall")));
            AddVariable("MountainFairies", MMRDictV16.ItemList.Keys.Where(x => x.StartsWith("CollectibleStrayFairySnowhead")));
            AddVariable("OceanFairies", MMRDictV16.ItemList.Keys.Where(x => x.StartsWith("CollectibleStrayFairyGreatBay")));
            AddVariable("CanyonFairies", MMRDictV16.ItemList.Keys.Where(x => x.StartsWith("CollectibleStrayFairyStoneTower")));
            AddVariable("SwampSkulls", MMRDictV16.ItemList.Keys.Where(x => x.StartsWith("CollectibleSwampSpiderToken")));
            AddVariable("OceanSkulls", MMRDictV16.ItemList.Keys.Where(x => x.StartsWith("CollectibleOceanSpiderToken")));
            string[] TransformationMasks = { "MaskDeku", "MaskGoron", "MaskZora", "MaskFierceDeity" };
            AddVariable("NonTransformationMasks", MMRDictV16.ItemList.Keys.Where(x => x.StartsWith("Mask") && !TransformationMasks.Contains(x)));
            AddVariable("TransformationMasks", TransformationMasks);
            AddVariable("NotebookEntries", MMRDictV16.ItemList.Keys.Where(x => x.StartsWith("Notebook")));
            AddVariable("HeartPieces", MMRDictV16.ItemList.Values.Where(x => x.Name == "Piece of Heart").Select(x => x.ID));
            AddVariable("HeartContainers", MMRDictV16.ItemList.Keys.Where(x => x.StartsWith("HeartContainer") || x.StartsWith("StartingHeartContainer")));
            AddVariable("BossRemains", MMRDictV16.ItemList.Keys.Where(x => x.StartsWith("Remains")));
        }

        private static void AddTrackerLogic(LogicDictionaryData.LogicDictionary MMRDictV16)
        {
            void AddAdditionalLogic(string ID, string Logic)
            {
                var AdditionalLogicEntry = new MMRData.JsonFormatLogicItem
                {
                    Id = ID,
                    ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(MMRLogicStringParser, Logic, ID)
                };
                LogicUtilities.RemoveRedundantConditionals(AdditionalLogicEntry);
                LogicUtilities.MakeCommonConditionalsRequirements(AdditionalLogicEntry);
                MMRDictV16.AdditionalLogic.Add(AdditionalLogicEntry);
            }

            //Logic Overrides
            AddAdditionalLogic("OtherInaccessible", "false");
            AddAdditionalLogic("AreaMoonAccess", "BossRemains, RequiredBossRemains && Play Oath to Order");
            AddAdditionalLogic("OtherCredits", "(setting{VictoryMode, DirectToCredits} || (AreaMoonAccess && OtherKillMajora)) && " +
                "HasVictoryModeFairies && HasVictoryModeSkulls && HasVictoryModeNonTransformationMask && HasVictoryModeTransformationMask && " +
                "HasVictoryModeNotebook && HasVictoryModeHearts && HasVictoryModeRemains");

            AddAdditionalLogic("HasVictoryModeFairies", "(setting{VictoryMode, Fairies, false} || (SwampFairies, 15 && MountainFairies, 15 && OceanFairies, 15 && CanyonFairies, 15))");
            AddAdditionalLogic("HasVictoryModeSkulls", "(setting{VictoryMode, SkullTokens, false} || (SwampSkulls, 30 && OceanSkulls, 30))");
            AddAdditionalLogic("HasVictoryModeNonTransformationMask", "(setting{VictoryMode, NonTransformationMasks, false} || (NonTransformationMasks, 20))");
            AddAdditionalLogic("HasVictoryModeTransformationMask", "(setting{VictoryMode, TransformationMasks, false} || (MaskDeku && MaskGoron && MaskZora && MaskFierceDeity))");
            AddAdditionalLogic("HasVictoryModeNotebook", "(setting{VictoryMode, Notebook, false} || NotebookEntries, 51)");
            AddAdditionalLogic("HasVictoryModeHearts", "(setting{VictoryMode, Hearts, false} || (HeartPieces, 52 && HeartContainers, 6))");
            AddAdditionalLogic("HasVictoryModeRemains", "(setting{VictoryMode, BossRemains, false} || BossRemains, 4)");

            //Tracker Specific Logic
            AddAdditionalLogic("MMRTCanBuyFromBeanScrub", "randomized{ShopItemBusinessScrubMagicBean} || OtherMagicBean");
            AddAdditionalLogic("MMRTArrows40", "UpgradeBigQuiver || UpgradeBiggestQuiver");
            AddAdditionalLogic("MMRTArrows50", "UpgradeBiggestQuiver");
            AddAdditionalLogic("MMRTbombchu10", "ChestInvertedStoneTowerBombchu10 || ChestLinkTrialBombchu10 || ShopItemBombsBombchu10");
            AddAdditionalLogic("MMRTEscortCremia", "OtherArrow || MaskCircusLeader");

            //Progressive Logic
            AddAdditionalLogic("MMRTProgressiveItemBow", "ItemBow || UpgradeBigQuiver || UpgradeBiggestQuiver");
            AddAdditionalLogic("MMRTProgressiveUpgradeBigQuiver", "(ItemBow && UpgradeBigQuiver) || (ItemBow && UpgradeBiggestQuiver) || (UpgradeBigQuiver && UpgradeBigQuiver)");
            AddAdditionalLogic("MMRTProgressiveUpgradeBiggestQuiver", "ItemBow && UpgradeBigQuiver && UpgradeBiggestQuiver");
            AddAdditionalLogic("MMRTProgressiveItemBombBag", "ItemBombBag || UpgradeBigBombBag || UpgradeBiggestBombBag");
            AddAdditionalLogic("MMRTProgressiveUpgradeBigBombBag", "(ItemBombBag && UpgradeBigBombBag) || (ItemBombBag && UpgradeBiggestBombBag) || (UpgradeBigBombBag && UpgradeBiggestBombBag)");
            AddAdditionalLogic("MMRTProgressiveUpgradeBiggestBombBag", "ItemBombBag && UpgradeBigBombBag && UpgradeBiggestBombBag");
            AddAdditionalLogic("MMRTProgressiveFairyMagic", "FairyMagic || FairyDoubleMagic");
            AddAdditionalLogic("MMRTProgressiveFairyDoubleMagic", "FairyMagic && FairyDoubleMagic");
            AddAdditionalLogic("MMRTProgressiveStartingSword", "StartingSword || UpgradeRazorSword || UpgradeGildedSword");
            AddAdditionalLogic("MMRTProgressiveUpgradeRazorSword", "(StartingSword && UpgradeRazorSword) || (StartingSword && UpgradeGildedSword) || (UpgradeRazorSword && UpgradeGildedSword)");
            AddAdditionalLogic("MMRTProgressiveUpgradeGildedSword", "StartingSword && UpgradeRazorSword && UpgradeGildedSword");
            AddAdditionalLogic("MMRTProgressiveSongLullaby", "SongLullabyIntro || SongLullaby");
            AddAdditionalLogic("MMRTProgressiveSongLullabyIntro", "SongLullabyIntro && SongLullaby");
            AddAdditionalLogic("MMRTProgressiveUpgradeAdultWallet", "UpgradeAdultWallet || UpgradeGiantWallet || UpgradeRoyalWallet");
            AddAdditionalLogic("MMRTProgressiveUpgradeGiantWallet", "(UpgradeAdultWallet && UpgradeGiantWallet) || (UpgradeAdultWallet && UpgradeRoyalWallet) || (UpgradeGiantWallet && UpgradeRoyalWallet)");
            AddAdditionalLogic("MMRTProgressiveUpgradeRoyalWallet", "UpgradeAdultWallet && UpgradeGiantWallet && UpgradeRoyalWallet");

            //Wallet Logic Entries
            AddAdditionalLogic("MMRTWallet99", "true");
            AddAdditionalLogic("MMRTWallet200", "UpgradeAdultWallet || UpgradeGiantWallet || UpgradeRoyalWallet");
            AddAdditionalLogic("MMRTWallet500", "UpgradeGiantWallet || UpgradeRoyalWallet");
            AddAdditionalLogic("MMRTWallet999", "UpgradeRoyalWallet");

            MMRDictV16.MacroList.Add("MMRTWallet99", new LogicDictionaryData.DictionaryMacroEntry { ID = "MMRTWallet99", WalletCapacity = 99 });
            MMRDictV16.MacroList.Add("MMRTWallet200", new LogicDictionaryData.DictionaryMacroEntry { ID = "MMRTWallet200", WalletCapacity = 200 });
            MMRDictV16.MacroList.Add("MMRTWallet500", new LogicDictionaryData.DictionaryMacroEntry { ID = "MMRTWallet500", WalletCapacity = 500 });
            MMRDictV16.MacroList.Add("MMRTWallet999", new LogicDictionaryData.DictionaryMacroEntry { ID = "MMRTWallet999", WalletCapacity = 999 });

        }

        private static void ReadEntranceData(MMRExportClass.MMRData ExportData)
        {
            foreach (var Entrance in ExportData.Entrances)
            {
                string EntranceType = Entrance.ID.EndsWith("Lair") ? "BossDoor Entrance" : "Dungeon Entrance";
                LogicDictionaryData.DictionaryLocationEntries NewLocation = new LogicDictionaryData.DictionaryLocationEntries
                {
                    ID = Entrance.ID,
                    Area = EntranceType + "s",
                    IgnoreForSettingString = true,
                    Name = Entrance.Name,
                    OriginalItem = Entrance.ID,
                    ValidItemTypes = new string[] { EntranceType.Replace(" ", "") },
                    SpoilerData = new MMRData.SpoilerlogReference
                    {
                        SpoilerLogNames = new string[] { Entrance.ID }
                    }
                };
            }
        }

        private static void ReadHintData(MMRExportClass.MMRData ExportData, LogicDictionaryData.LogicDictionary MMRDictV16)
        {
            foreach (var Hint in ExportData.Hints)
            {
                LogicDictionaryData.DictionaryHintEntries NewHint = new LogicDictionaryData.DictionaryHintEntries
                {
                    ID = Hint.ID,
                    Name = Hint.ID
                };
                MMRDictV16.HintSpots.Add(Hint.ID, NewHint);
            }
        }

        private static void ReadMacroData(MMRExportClass.MMRData ExportData, LogicDictionaryData.LogicDictionary MMRDictV16)
        {
            foreach (var macro in ExportData.Macros)
            {
                LogicDictionaryData.DictionaryMacroEntry NewMacro = new LogicDictionaryData.DictionaryMacroEntry
                {
                    ID = macro.ID
                };
                if (macro.PriceNames != null && macro.PriceNames.Any())
                {
                    NewMacro.SpoilerData = new MMRData.SpoilerlogReference { PriceDataNames = macro.PriceNames.ToArray() };
                }
                MMRDictV16.MacroList.Add(macro.ID, NewMacro);
            }
            foreach(var AreaClear in ExportData.AreaClear)
            {
                LogicDictionaryData.DictionaryMacroEntry NewMacro = new LogicDictionaryData.DictionaryMacroEntry
                {
                    ID = AreaClear.ID
                };
                MMRDictV16.MacroList.Add(AreaClear.ID, NewMacro);
            }
        }

        private static void ReadItemData(MMRExportClass.MMRData ExportData, LogicDictionaryData.LogicDictionary MMRDictV16)
        {
            foreach (var item in ExportData.Items)
            {
                LogicDictionaryData.DictionaryItemEntries NewItem = new LogicDictionaryData.DictionaryItemEntries
                {
                    ID = item.ID,
                    MaxAmountInWorld = item.Tags.Contains("Fake") || item.Tags.Contains("Trap") ? -1 : 1,
                    ItemTypes = new string[] { item.ID.StartsWith("BottleCatch") ? "BottleCatch" : "Default" },
                    Name = item.Name,
                    ValidStartingItem = item.StartingItem,
                    WalletCapacity = item.ID.StartsWith("Upgrade") && item.ID.EndsWith("Wallet") ? -1 : null,
                    SpoilerData = new MMRData.SpoilerlogReference
                    {
                        SpoilerLogNames = string.IsNullOrWhiteSpace(item.ProgressiveGroup) ? new string[] { item.Name } : new string[] { item.Name, item.ProgressiveGroup },
                        Tags = item.Tags,
                    }
                };
                MMRDictV16.ItemList.Add(item.ID, NewItem);
            }
        }

        private static void ReadLocationData(MMRExportClass.MMRData ExportData, LogicDictionaryData.LogicDictionary mMRDictV16)
        {
            foreach (var Location in ExportData.Locations)
            {
                LogicDictionaryData.DictionaryLocationEntries NewLocation = new LogicDictionaryData.DictionaryLocationEntries
                {
                    ID = Location.ID,
                    Area = Location.Area,
                    IgnoreForSettingString = false,
                    Name = Location.Name,
                    OriginalItem = Location.ID,
                    ValidItemTypes = new string[] { Location.ID.StartsWith("BottleCatch") ? "BottleCatch" : "Default" },
                    SpoilerData = new MMRData.SpoilerlogReference
                    {
                        Tags = Location.Tags,
                        PriceDataNames = Location.PriceNames?.ToArray()??Array.Empty<string>(),
                        SpoilerLogNames = new string[] { Location.Name }
                    }
                };
                if (Location.Proxies is not null && Location.Proxies.Any())
                {
                    foreach (var Proxy in Location.Proxies)
                    {
                        NewLocation.LocationProxys.Add(new LogicDictionaryData.DictLocationProxy
                        {
                            Area = Proxy.Area,
                            ID = Proxy.ID,
                            LogicInheritance = Proxy.Logic,
                            Name = Proxy.Name
                        });
                    }
                }
                mMRDictV16.LocationList.Add(Location.ID, NewLocation);
            }
        }
    }
}
