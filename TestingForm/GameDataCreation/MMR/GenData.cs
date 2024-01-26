using MMR_Tracker_V3;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System.Diagnostics;
using static MMR_Tracker_V3.TrackerObjects.LogicDictionaryData;
using static MMR_Tracker_V3.TrackerObjects.MMRData;

namespace TestingForm.GameDataCreation.MMR
{
    public class GenData
    {
        public static LogicStringParser MMRLogicStringParser = new LogicStringParser();
        public static LogicDictionaryData.LogicDictionary CreateMMRFiles()
        {
            MMRExportClass.MMRData ExportData = JsonConvert.DeserializeObject<MMRExportClass.MMRData>(File.ReadAllText(Path.Combine(TestingReferences.GetOtherGameDataPath("MMR"), "MMRDataExport.json")));
            ExportData.AdjustedAreaClear = new List<string> { "CanClearDungeonAtWoodfall", "CanClearDungeonAtSnowhead", "CanClearDungeonAtGreatBay", "CanClearDungeonAtStoneTower" };
            LogicDictionaryData.LogicDictionary MMRDictV16 = new LogicDictionaryData.LogicDictionary
            {
                GameCode = "MMR",
                LogicVersion = 24,
                WinCondition = "OtherCredits",
                DefaultSettings = new LogicDictionaryData.DefaultSettings()
            };
            MMRDictV16.DefaultSettings.CustomItemListString = "-------------------------40c-80000000----21ffff-ffffffff-ffffffff-f0000000-7bbeeffa-7fffffff-e6f1fffe-ffffffff";
            MMRDictV16.DefaultSettings.CustomJunkLocationsString = "------------------------------200000-----400000--f000";
            MMRDictV16.DefaultSettings.CustomStartingItemListString = "--1fbfc-5800000-";
            MMRDictV16.DefaultSettings.ManualRandomizationState = new Dictionary<string, MiscData.RandomizedState>
            {
                { "AreaWoodFallTempleAccess", MiscData.RandomizedState.Unrandomized },
                { "AreaSnowheadTempleAccess", MiscData.RandomizedState.Unrandomized },
                { "AreaGreatBayTempleAccess", MiscData.RandomizedState.Unrandomized },
                { "AreaInvertedStoneTowerTempleAccess", MiscData.RandomizedState.Unrandomized },
                { "AreaOdolwasLair", MiscData.RandomizedState.Unrandomized },
                { "AreaGohtsLair", MiscData.RandomizedState.Unrandomized },
                { "AreaGyorgsLair", MiscData.RandomizedState.Unrandomized },
                { "AreaTwinmoldsLair", MiscData.RandomizedState.Unrandomized },
            };
            foreach (var GaroHint in ExportData.Hints.Where(x => x.ID.StartsWith("HintGaro"))) { MMRDictV16.DefaultSettings.ManualRandomizationState.Add(GaroHint.ID, MiscData.RandomizedState.Unrandomized); }
            MMRDictV16.DefaultSettings.EnabledTricks = new List<string>
            {
                "Exit OSH Without Goron",
                "Lensless Chests",
                "Day 2 Grave Without Lens of Truth",
                "SHT Lensless Walls/Ceilings",
                "Pinnacle Rock without Seahorse",
                "Run Through Poisoned Water",
                "WFT 2nd Floor Skip",
            };

            ReadLocationData(ExportData, MMRDictV16);
            ReadItemData(ExportData, MMRDictV16);
            ReadMacroData(ExportData, MMRDictV16);
            ReadHintData(ExportData, MMRDictV16);
            ReadEntranceData(ExportData, MMRDictV16);
            AddTrackerLogic(MMRDictV16);
            AddLogicCollections(MMRDictV16);
            CreateLogicSettings(ExportData, MMRDictV16);
            HandleAreaClearLogic(ExportData, MMRDictV16);
            ManualTweaking(MMRDictV16);

            List<string> Areas = new List<string>();
            foreach(var i in MMRDictV16.LocationList)
            {
                if (!Areas.Contains(i.Value.Area)) { Areas.Add(i.Value.Area); }
                if (!i.Value.LocationProxys.Any()) { continue; }
                foreach(var j in i.Value.LocationProxys)
                {
                    if (!Areas.Contains(j.Area)) { Areas.Add(j.Area); }
                }
            }
            Debug.WriteLine(Areas.ToFormattedJson());

            return MMRDictV16;
        }

        private static void ManualTweaking(LogicDictionary MMRDictV16)
        {
            MMRDictV16.LocationList["HeartContainerWoodfall"].Name = "Odolwa's Lair Heart Container";
            MMRDictV16.LocationList["HeartContainerSnowhead"].Name = "Goht's Lair Heart Container";
            MMRDictV16.LocationList["HeartContainerGreatBay"].Name = "Gyorg's Lair Heart Container";
            MMRDictV16.LocationList["HeartContainerStoneTower"].Name = "Twinmold's Lair Heart Container";

            MMRDictV16.LocationList["RemainsOdolwa"].Name = "Odolwa's Lair Boss Warp";
            MMRDictV16.LocationList["RemainsGoht"].Name = "Goht's Lair Boss Warp";
            MMRDictV16.LocationList["RemainsGyorg"].Name = "Gyorg's Lair Boss Warp";
            MMRDictV16.LocationList["RemainsTwinmold"].Name = "Twinmold's Lair Boss Warp";
        }

        private static void HandleAreaClearLogic(MMRExportClass.MMRData ExportData, LogicDictionaryData.LogicDictionary MMRDictV16)
        {
            var DungeonEntrances = ExportData.Entrances.Where(x => !x.ID.Contains("Lair")).Select(x => x.ID).ToArray();
            var BossDoorEntrances = ExportData.Entrances.Where(x => x.ID.Contains("Lair")).Select(x => x.ID).ToArray();
            var AreaClearMacros = ExportData.AreaClear.Select(x => x.ID).ToArray();
            var RemainsChecks = MMRDictV16.LocationList.Keys.Where(x => x.StartsWith("Remains")).ToArray();
            string[] BossShortcutRemainsMacros = new string[]
            {
                "MMRTWoodfallRemains",
                "MMRTSnowheadRemains",
                "MMRTGreatBaylRemains",
                "MMRTStoneTowerRemains"
            };

            foreach (var AAC in ExportData.AdjustedAreaClear)
            {
                List<string> Logic = new List<string>();
                var DungeonAtArea = DungeonEntrances[ExportData.AdjustedAreaClear.IndexOf(AAC)];
                foreach (var DEDest in DungeonEntrances)
                {
                    var BossDoorInDungeon = BossDoorEntrances[Array.IndexOf(DungeonEntrances, DEDest)];
                    foreach (var BE in BossDoorEntrances)
                    {
                        var ACFromBoss = AreaClearMacros[Array.IndexOf(BossDoorEntrances, BE)];
                        string LogicLine = $"(contains{{{DungeonAtArea}, {DEDest}}} && contains{{{BossDoorInDungeon}, {BE}}} && {ACFromBoss} && check{{{BossDoorInDungeon}}})";
                        Logic.Add(LogicLine);
                    }
                }

                MMRDictV16.AdditionalLogic.Add(new JsonFormatLogicItem { Id = AAC, ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(MMRLogicStringParser, string.Join(" || ", Logic), AAC) });
            }
            foreach (var ShortcutMacro in BossShortcutRemainsMacros)
            {
                int MacroInd = Array.IndexOf(BossShortcutRemainsMacros, ShortcutMacro);
                List<List<string>> Logic = new() { new List<string> { $"randomized{{{BossDoorEntrances[MacroInd]}, false}}", RemainsChecks[MacroInd] } };
                foreach (var i in BossDoorEntrances)
                {
                    int BDInd = Array.IndexOf(BossDoorEntrances, i);
                    Logic.Add(new List<string> { $"contains{{{BossDoorEntrances[MacroInd]}, {i}}}", RemainsChecks[BDInd] });
                }
                MMRDictV16.AdditionalLogic.Add(new JsonFormatLogicItem { Id = ShortcutMacro, ConditionalItems = Logic });
            }
        }

        private static void CreateLogicSettings(MMRExportClass.MMRData exportData, LogicDictionaryData.LogicDictionary MMRDictV16)
        {
            int SettingPriority = 0;
            void AddSimpleToggle(string ID, string Name, string Category, bool Default)
            {
                SettingPriority++;
                MMRDictV16.ToggleOptions.Add(ID, new OptionData.ToggleOption(null) { ID = ID, Value = Default.ToString().ToLower(), Name = Name, SubCategory = Category, Priority = SettingPriority }.CreateSimpleValues(true));
            }
            void AddSimpleMultiSelect(string ID, string Name, string Category, params string[] Values)
            {
                SettingPriority++;
                MMRDictV16.MultiSelectOptions.Add(ID, new OptionData.MultiSelectOption(null) { ID = ID, Name = Name, EnabledValues = new HashSet<string>(), SubCategory = Category, Priority = SettingPriority }.CreateSimpleValues(Values));
            }
            void AddSimpleChoice(string ID, string Name, string Category, string Default, params string[] Values)
            {
                SettingPriority++;
                MMRDictV16.ChoiceOptions.Add(ID, new OptionData.ChoiceOption(null) { ID = ID, Name = Name, Value = Default, SubCategory = Category, Priority = SettingPriority }.CreateSimpleValues(Values));
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
            //Progressive Item Options
            Dictionary<string, string> ProgressiveItemData = new Dictionary<string, string>();
            foreach (var i in exportData.Items)
            {
                if (string.IsNullOrWhiteSpace(i.ProgressiveGroup)) { continue; }
                ProgressiveItemData[i.ID] = $"{i.ProgressiveGroup}|MMRTProgressive{i.ID}";
            }

            AddSimpleToggle("ProgressiveUpgrades", "Progressive Upgrades", "Main Settings", false);
            var ProgressiveUpgradesReplacements = CreateLogicReplacement(MMRDictV16.ToggleOptions["ProgressiveUpgrades"].Enabled);
            AddLogicReplacement(ProgressiveUpgradesReplacements, ProgressiveItemData.Select(x => $"{x.Key}|{x.Value.Split('|')[1]}").ToArray());
            ProgressiveUpgradesReplacements.LocationBlacklist = ProgressiveItemData.Values.Select(x => x.Split('|')[1]).ToArray();
            MMRDictV16.ToggleOptions["ProgressiveUpgrades"].Enabled.Actions.ItemNameOverride = ProgressiveItemData.ToDictionary(x => x.Key, x => x.Value.Split('|')[0]);

            //Randomized Enemies
            AddSimpleToggle("RandomizeEnemies", "Randomize Enemies", "Main Settings", false);

            //==========================================================================================================================================================================
            //Boss Remian Moon Requirement
            SettingPriority++;
            MMRDictV16.IntOptions.Add("RequiredBossRemains", new OptionData.IntOption(null)
            {
                ID = "RequiredBossRemains",
                Name = "Required Boss Remains",
                Max = 4,
                Min = 0,
                Value = 4,
                SubCategory = "Main Settings",
                Priority = SettingPriority
            });

            //==========================================================================================================================================================================
            //Victory Mode Options
            AddSimpleMultiSelect("VictoryMode", "Victory Mode", "Main Settings", "DirectToCredits", "CantFightMajora", "Fairies", "SkullTokens", "NonTransformationMasks", "TransformationMasks", "Notebook", "Hearts", "OneBossRemains", "TwoBossRemains", "ThreeBossRemains", "FourBossRemains");

            //Price Mode
            AddSimpleMultiSelect("PriceMode", "Price Mode", "Main Settings", "Purchases", "Minigames", "Misc", "AccountForRoyalWallet", "ShuffleOnly");

            //==========================================================================================================================================================================
            //Keysy Options
            AddSimpleMultiSelect("SmallKeyMode", "Small Key Mode", "Main Settings", "DoorsOpen", "KeepWithinTemples", "KeepWithinArea", "KeepWithinOverworld", "KeepThroughTime");
            string[] AllSmallKeys = MMRDictV16.ItemList.Values.Where(x => x.Name.EndsWith("Small Key")).Select(x => x.ID + "|MMRTSmallKeysy").ToArray();
            AddLogicReplacement(CreateLogicReplacement(MMRDictV16.MultiSelectOptions["SmallKeyMode"].ValueList["DoorsOpen"]), AllSmallKeys);

            AddSimpleMultiSelect("BossKeyMode", "Boss Key Mode", "Main Settings", "DoorsOpen", "GreatFairyRewards", "KeepWithinTemples", "KeepWithinArea", "KeepWithinOverworld", "KeepThroughTime");
            string[] AllBossKeys = MMRDictV16.ItemList.Values.Where(x => x.Name.EndsWith("Boss Key")).Select(x => x.ID + "|MMRTBossKeysy").ToArray();
            AddLogicReplacement(CreateLogicReplacement(MMRDictV16.MultiSelectOptions["BossKeyMode"].ValueList["DoorsOpen"]), AllBossKeys);


            AddSimpleMultiSelect("StrayFairyMode", "Stray Fairy Mode", "Main Settings", "ChestsOnly", "KeepWithinTemples", "KeepWithinArea", "KeepWithinOverworld");
            var AllSwampFairies = MMRDictV16.ItemList.Keys.Where(x => x.StartsWith("CollectibleStrayFairyWoodfall"));
            var AllMountainFairies = MMRDictV16.ItemList.Keys.Where(x => x.StartsWith("CollectibleStrayFairySnowhead"));
            var AllOceanFairies = MMRDictV16.ItemList.Keys.Where(x => x.StartsWith("CollectibleStrayFairyGreatBay"));
            var AllCanyonFairies = MMRDictV16.ItemList.Keys.Where(x => x.StartsWith("CollectibleStrayFairyStoneTower"));
            string[] AllFairies = AllSwampFairies.Concat(AllMountainFairies).Concat(AllOceanFairies).Concat(AllCanyonFairies).Select(x => x + "|true").ToArray();
            AddLogicReplacement(CreateLogicReplacement(MMRDictV16.MultiSelectOptions["StrayFairyMode"].ValueList["ChestsOnly"]), AllFairies);

            //Movement Gimicks

            AddSimpleChoice("MovementMode", "Gravity/Speed", "Gimmicks", "Default", "Default", "HighSpeed", "SuperLowGravity", "LowGravity", "HighGravity");
            AddSimpleChoice("FloorType", "Floor Types", "Gimmicks", "Default", "Default", "Sand", "Ice", "Snow", "Random");

            AddSimpleToggle("ContinuousDekuHopping", "Continuous Deku Hopping", "Gimmicks", false);

            AddSimpleToggle("HookshotAnySurface", "Hookshot Any Surface", "Gimmicks", false);
            AddSimpleToggle("ClimbMostSurfaces", "Climb Most Surfaces", "Gimmicks", false);
            AddSimpleToggle("IronGoron", "Iron Goron", "Gimmicks", false);

            AddSimpleChoice("DamageMode", "Damage Mode", "Gimmicks", "Default", "Default", "Double", "Quadruple", "OHKO", "Doom");
            AddSimpleChoice("DamageEffect", "Damage Effects", "Gimmicks", "Default", "Default", "Fire", "Ice", "Shock", "Knockdown", "Random");

            AddSimpleToggle("DeathMoonCrash", "Death is Moon Crash", "Gimmicks", false);
            //==========================================================================================================================================================================
            //BYOAmmo Options
            void AddBYOAAdditionalLogic(string Location, string NewLogic)
            {
                var LogicAddition = CreateAdditionalLogic(MMRDictV16.ToggleOptions["ByoAmmo"].Enabled);
                AddAdditionalRequirement(LogicAddition, NewLogic);
                LogicAddition.LocationWhitelist = new string[] { Location };
            }
            AddSimpleToggle("ByoAmmo", "Bring Your Own Ammo", "Gimmicks", false);
            AddBYOAAdditionalLogic("UpgradeBigQuiver", "MMRTArrows40");
            AddBYOAAdditionalLogic("UpgradeBiggestQuiver", "MMRTArrows40");
            AddBYOAAdditionalLogic("HeartPieceSwampArchery", "MMRTArrows40");
            AddBYOAAdditionalLogic("HeartPieceTownArchery", "MMRTArrows50");
            AddBYOAAdditionalLogic("HeartPieceHoneyAndDarling", "MMRTbombchu10");
            AddBYOAAdditionalLogic("MaskRomani", "MMRTEscortCremia");

            AddSimpleToggle("FewerHealthDrops", "Fewer Health Drops", "Gimmicks", false);


            AddSimpleChoice("BlastMaskCooldown", "Blast Mask Cooldown", "Gimmicks", "Default", "Default", "Instant", "VeryShort", "Short", "Long", "VeryLong");
            AddSimpleChoice("NutandStickDrops", "Nut and Stick Drops", "Gimmicks", "Default", "Default", "Light", "Medium", "Extra", "Mayhem");
            AddSimpleToggle("OcarinaUnderwater", "Ocarina Underwater", "Gimmicks", false);
            AddSimpleToggle("EnableSunsSong", "Enable Suns Song", "Gimmicks", false);
            AddSimpleToggle("FreeScarecrow", "Free Scarecrow's Song", "Gimmicks", false);
            AddSimpleToggle("AllowFierceDeityAnywhere", "Fierce Deity Anywhere", "Gimmicks", false);
            AddSimpleToggle("GiantMaskAnywhere", "Giant Mask Anywhere", "Gimmicks", false);
            AddSimpleToggle("InstantTransform", "Instant Transform", "Gimmicks", false);
            AddSimpleToggle("BombArrows", "Bomb Arrows", "Gimmicks", false);
            AddSimpleToggle("VanillaMoonTrialAccess", "Vanilla Moon Trial Access", "Gimmicks", false);


            //==========================================================================================================================================================================
            //Hint Options
            AddSimpleToggle("FreeHints", "Free Hints", "Comfort", true);
            AddSimpleToggle("FreeGaroHints", "Free Garo Hints", "Comfort", false);
            var RemoveMaskTruthGossipEdit = CreateLogicReplacement(MMRDictV16.ToggleOptions["FreeHints"].Enabled);
            var RemoveMaskTruthGaroEdit = CreateLogicReplacement(MMRDictV16.ToggleOptions["FreeGaroHints"].Enabled);
            AddLogicReplacement(RemoveMaskTruthGossipEdit, "MaskTruth|true");
            AddLogicReplacement(RemoveMaskTruthGaroEdit, "MaskTruth|true");
            RemoveMaskTruthGossipEdit.LocationWhitelist = MMRDictV16.HintSpots.Keys.Where(x => x.StartsWith("Gossip")).ToArray();
            RemoveMaskTruthGaroEdit.LocationWhitelist = MMRDictV16.HintSpots.Keys.Where(x => x.StartsWith("HintGaro")).ToArray();


            //==========================================================================================================================================================================
            //Comforts

            AddSimpleChoice("Character", "Character Model", "Comfort", "LinkMM", "LinkMM", "LinkOOT", "AdultLink", "Kafei");

            AddSimpleToggle("CritWiggleDisable", "Disable Crit Wiggle", "Comfort", false);
            AddSimpleToggle("QuickTextEnabled", "Quick Text", "Comfort", false);
            AddSimpleToggle("FastPush", "Increase Push Speed", "Comfort", false);
            AddSimpleToggle("CloseCows", "Close Cows", "Comfort", false);
            //Imporived Picture Box
            AddSimpleToggle("LenientGoronSpikes", "Lenient Goron Spikes", "Comfort", false);
            //Target Health Bar
            //Fill Wallet
            //Hidden Ruppe Sparkle
            AddSimpleToggle("SaferGlitches", "Safer Glitches", "Comfort", false);
            //Improved Cmaera
            //Easy Frame By Frame
            //Treasure Game Spoilers
            //Update Shops
            AddSimpleToggle("UpdateChests", "Update Chests", "Comfort", false);
            //Update World Models
            //Update NPC Text
            //No Downgrades
            AddSimpleToggle("FixEponaSword", "Fix Epona Sword", "Comfort", false);
            AddSimpleToggle("QuestItemStorage", "Quest Item Storage", "Comfort", false);
            //Quest Items through Time
            //Arrow Cycling
            //Elegy Speedup

            //==========================================================================================================================================================================
            //Bombchu Drops Option

            AddSimpleToggle("BombchuDrops", "Bombchu Drops", "Comfort", false);
            var BombchuDropMMRTbombchu10 = CreateAdditionalLogic(MMRDictV16.ToggleOptions["BombchuDrops"].Enabled);
            AddAdditionalConditional(BombchuDropMMRTbombchu10, "ChestIkanaSecretShrineGrotto", "ChestTerminaGrottoBombchu", "ChestGreatBayCapeGrotto", "ChestGraveyardGrotto", "ChestToIkanaGrotto", "ChestToGoronRaceGrotto");
            BombchuDropMMRTbombchu10.LocationWhitelist = new string[] { "MMRTbombchu10", "Any Bombchu Pack" };

            //Tollerant Gossip Angle
            //Detect Stray Fairy
            //Detect Skulltulla

            //==========================================================================================================================================================================
            //Static Edits

            AddSimpleChoice("StaticEdits", "Do Static Edits", "Static", "Static", "Static");
            var StaticLogicBeanReplacements = CreateLogicReplacement(MMRDictV16.ChoiceOptions["StaticEdits"].ValueList["Static"]);
            StaticLogicBeanReplacements.LocationWhitelist = new string[] { "ShopItemBusinessScrubMagicBeanInSwamp", "ShopItemBusinessScrubMagicBeanInTown" };
            AddLogicReplacement(StaticLogicBeanReplacements, "OtherMagicBean|MMRTCanBuyFromBeanScrub");

            var StaticLogicTradeItemSaftey = CreateLogicReplacement(MMRDictV16.ChoiceOptions["StaticEdits"].ValueList["Static"]);
            AddLogicReplacement(StaticLogicTradeItemSaftey, "TradeItemKafeiLetter|true", "TradeItemPendant|true");
            StaticLogicTradeItemSaftey.LocationWhitelist = new string[] { "UpgradeBigBombBag", "MaskBlast", "NotebookSaveOldLady", "UpgradeMirrorShield" };

            var StaticLogicBottleCatchSaftey1 = CreateLogicReplacement(MMRDictV16.ChoiceOptions["StaticEdits"].ValueList["Static"]);
            AddLogicReplacement(StaticLogicBottleCatchSaftey1, "BottleCatchEgg|true", "BottleCatchBug|true", "BottleCatchFish|true");
            StaticLogicBottleCatchSaftey1.LocationWhitelist = new string[] { "BottleCatchPrincess", "BottleCatchBigPoe" };

            var StaticLogicBottleCatchSaftey2 = CreateLogicReplacement(MMRDictV16.ChoiceOptions["StaticEdits"].ValueList["Static"]);
            AddLogicReplacement(StaticLogicBottleCatchSaftey2, "BottleCatchFish|true");
            StaticLogicBottleCatchSaftey2.LocationWhitelist = new string[] { "BottleCatchEgg" };

            var StaticLogicBoosWarpRemainLogic = CreateLogicReplacement(MMRDictV16.ChoiceOptions["StaticEdits"].ValueList["Static"]);
            AddLogicReplacement(StaticLogicBoosWarpRemainLogic, "RemainsOdolwa|MMRTWoodfallRemains", "RemainsGoht|MMRTSnowheadRemains", "RemainsGyorg|MMRTGreatBaylRemains", "RemainsTwinmold|MMRTStoneTowerRemains");
            StaticLogicBoosWarpRemainLogic.LocationBlacklist = new string[] { "AreaMoonAccess", "MMRTWoodfallRemains", "MMRTSnowheadRemains", "MMRTGreatBaylRemains", "MMRTStoneTowerRemains" };

            var StaticLogicBossRemainWinCon = CreateLogicReplacement(MMRDictV16.ChoiceOptions["StaticEdits"].ValueList["Static"]);
            AddLogicReplacement(StaticLogicBossRemainWinCon, "RemainsOdolwa|BossRemains, RequiredBossRemains", "RemainsGoht|true", "RemainsGyorg|true", "RemainsTwinmold|true");
            StaticLogicBossRemainWinCon.LocationWhitelist = new string[] { "AreaMoonAccess" };

            var AreaClearMacros = exportData.AreaClear.Select(x => x.ID).ToArray();

            var StaticLogicAreaClearReplacements = CreateLogicReplacement(MMRDictV16.ChoiceOptions["StaticEdits"].ValueList["Static"]);
            StaticLogicAreaClearReplacements.LocationBlacklist = exportData.AdjustedAreaClear.ToArray();
            for (var i = 0; i < AreaClearMacros.Length; i++)
            {
                AddLogicReplacement(StaticLogicAreaClearReplacements, $"{AreaClearMacros[i]}|{exportData.AdjustedAreaClear[i]}");
            }
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
            AddAdditionalLogic("OtherCredits", "(setting{VictoryMode, DirectToCredits} || (AreaMoonAccess && OtherKillMajora)) && " +
                "HasVictoryModeFairies && HasVictoryModeSkulls && HasVictoryModeNonTransformationMask && HasVictoryModeTransformationMask && " +
                "HasVictoryModeNotebook && HasVictoryModeHearts && HasVictoryModeRemains1 && HasVictoryModeRemains2 && HasVictoryModeRemains3 && HasVictoryModeRemains4");

            AddAdditionalLogic("HasVictoryModeFairies", "(setting{VictoryMode, Fairies, false} || (SwampFairies, 15 && MountainFairies, 15 && OceanFairies, 15 && CanyonFairies, 15))");
            AddAdditionalLogic("HasVictoryModeSkulls", "(setting{VictoryMode, SkullTokens, false} || (SwampSkulls, 30 && OceanSkulls, 30))");
            AddAdditionalLogic("HasVictoryModeNonTransformationMask", "(setting{VictoryMode, NonTransformationMasks, false} || (NonTransformationMasks, 20))");
            AddAdditionalLogic("HasVictoryModeTransformationMask", "(setting{VictoryMode, TransformationMasks, false} || (MaskDeku && MaskGoron && MaskZora && MaskFierceDeity))");
            AddAdditionalLogic("HasVictoryModeNotebook", "(setting{VictoryMode, Notebook, false} || NotebookEntries, 51)");
            AddAdditionalLogic("HasVictoryModeHearts", "(setting{VictoryMode, Hearts, false} || (HeartPieces, 52 && HeartContainers, 6))");
            AddAdditionalLogic("HasVictoryModeRemains1", "(setting{VictoryMode, OneBossRemains, false} || BossRemains, 1)");
            AddAdditionalLogic("HasVictoryModeRemains2", "(setting{VictoryMode, TwoBossRemains, false} || BossRemains, 2)");
            AddAdditionalLogic("HasVictoryModeRemains3", "(setting{VictoryMode, ThreeBossRemains, false} || BossRemains, 3)");
            AddAdditionalLogic("HasVictoryModeRemains4", "(setting{VictoryMode, FourBossRemains, false} || BossRemains, 4)");

            //Tracker Specific Logic
            AddAdditionalLogic("MMRTCanBuyFromBeanScrub", "randomized{ShopItemBusinessScrubMagicBean} || OtherMagicBean");
            AddAdditionalLogic("MMRTArrows40", "UpgradeBigQuiver || UpgradeBiggestQuiver");
            AddAdditionalLogic("MMRTArrows50", "UpgradeBiggestQuiver");
            AddAdditionalLogic("MMRTbombchu10", "ChestInvertedStoneTowerBombchu10 || ChestLinkTrialBombchu10 || ShopItemBombsBombchu10");
            AddAdditionalLogic("MMRTEscortCremia", "OtherArrow || MaskCircusLeader");

            //Progressive Logic
            AddAdditionalLogic("MMRTProgressiveItemBow", "ItemBow || UpgradeBigQuiver || UpgradeBiggestQuiver");
            AddAdditionalLogic("MMRTProgressiveUpgradeBigQuiver", "(ItemBow && UpgradeBigQuiver) || (ItemBow && UpgradeBiggestQuiver) || (UpgradeBigQuiver && UpgradeBiggestQuiver)");
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

            AddAdditionalLogic("MMRTSmallKeysy", "true");
            AddAdditionalLogic("MMRTBossKeysy", "true");

            MMRDictV16.MacroList.Add("MMRTWallet99", new LogicDictionaryData.DictionaryMacroEntry { ID = "MMRTWallet99", WalletCapacity = 99 });
            MMRDictV16.MacroList.Add("MMRTWallet200", new LogicDictionaryData.DictionaryMacroEntry { ID = "MMRTWallet200", WalletCapacity = 200 });
            MMRDictV16.MacroList.Add("MMRTWallet500", new LogicDictionaryData.DictionaryMacroEntry { ID = "MMRTWallet500", WalletCapacity = 500 });
            MMRDictV16.MacroList.Add("MMRTWallet999", new LogicDictionaryData.DictionaryMacroEntry { ID = "MMRTWallet999", WalletCapacity = 999 });


        }

        private static void ReadEntranceData(MMRExportClass.MMRData ExportData, LogicDictionaryData.LogicDictionary mMRDictV16)
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
                        SpoilerLogNames = new string[] { Entrance.Name }
                    }
                };
                mMRDictV16.LocationList.Add(Entrance.ID, NewLocation);
                LogicDictionaryData.DictionaryItemEntries NewItem = new LogicDictionaryData.DictionaryItemEntries
                {
                    ID = Entrance.ID,
                    Name = Entrance.Name,
                    ItemTypes = new string[] { EntranceType.Replace(" ", "") },
                    MaxAmountInWorld = 1,
                    ValidStartingItem = false,
                    SpoilerData = new MMRData.SpoilerlogReference
                    {
                        SpoilerLogNames = new string[] { Entrance.Name }
                    }
                };
                mMRDictV16.ItemList.Add(Entrance.ID, NewItem);
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
                    IgnoreForSettingString = item.Name.Contains("Heart"),
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
