using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net;
using YamlDotNet.Serialization.NamingConventions;
using System.Text.RegularExpressions;
using MMR_Tracker_V3.TrackerObjects;
using System.Collections;
using System.Transactions;
using Octokit;
using static Microsoft.FSharp.Core.ByRefKinds;
using YamlDotNet.Core.Tokens;

namespace MMR_Tracker_V3.OtherGames.OOTMMRCOMBO
{

    public static class ReadAndParseData
    {
        public class MMROOTLocation
        {
            public string location;
            public string type;
            public string hint;
            public string scene;
            public string id;
            public string item;
        }

        public class MMROOTLogicEntry
        {
            public string dungeon;
            public string boss;
            public string region;
            public Dictionary<string, string> exits =  new Dictionary<string, string>();
            public Dictionary<string, string> locations = new Dictionary<string, string>();
            public Dictionary<string, string> events = new Dictionary<string, string>();
            public Dictionary<string, string> gossip = new Dictionary<string, string>();
        }
        public class MMROOTEntranceData
        {
            public string to;
            public string from;
            public string id;
        }

        public static LogicStringParser OOTMMLogicParser = new LogicStringParser();

        public static void CreateFiles(out TrackerObjects.MMRData.LogicFile Logic, out TrackerObjects.LogicDictionaryData.LogicDictionary dictionary)
        {
            string TestFolder = Path.Combine(References.TestingPaths.GetDevCodePath(), "MMR Tracker V3", "Recources");
            string FinalDictFile = Path.Combine(TestFolder, "Dictionaries", @"OOTMM V1.json");
            string FinalLogicFile = Path.Combine(TestFolder, "Presets", @"DEV-OOTMM Casual.json");

            AddEntriesFromItemPools(out TrackerObjects.MMRData.LogicFile LogicFile, out TrackerObjects.LogicDictionaryData.LogicDictionary dictionaryFile);

            AddEntriesFromLogicFiles(LogicFile, dictionaryFile);

            CleanLogicAndParse(LogicFile);

            AddVariablesandOptions(dictionaryFile);

            RemoveGameFromNames(dictionaryFile);

            FixAreaClearLogic(LogicFile);

            CreateEntrancePairs(dictionaryFile);

            CreateLocationProxies(dictionaryFile);

            AddRenewableChecks(LogicFile, dictionaryFile);

            FinalLogicCleanup(LogicFile);


            foreach (var i in dictionaryFile.LocationList) { i.Value.ID = null; }
            foreach (var i in dictionaryFile.ItemList) { i.Value.ID = null; }
            foreach (var i in dictionaryFile.EntranceList) { i.Value.ID = null; }
            foreach (var i in dictionaryFile.HintSpots) { i.Value.ID = null; }
            foreach (var i in dictionaryFile.MacroList) { i.Value.ID = null; }
            foreach (var i in dictionaryFile.Options) { i.Value.ID = null; }
            foreach (var i in dictionaryFile.Variables) { i.Value.ID = null; }

            Logic = LogicFile;
            dictionary = dictionaryFile;
            File.WriteAllText(FinalLogicFile, JsonConvert.SerializeObject(LogicFile, Testing._NewtonsoftJsonSerializerOptions));
            File.WriteAllText(FinalDictFile, JsonConvert.SerializeObject(dictionaryFile, Testing._NewtonsoftJsonSerializerOptions));


        }

        private static void FinalLogicCleanup(MMRData.LogicFile logicFile)
        {
            foreach(var logic in logicFile.Logic)
            {
                foreach(var condSet in logic.ConditionalItems)
                {
                    if (condSet.Any(x => bool.TryParse(x, out bool BT) && BT) && condSet.Any(x => !bool.TryParse(x, out _)))
                    {
                        int Match = condSet.RemoveAll(x => bool.TryParse(x, out bool BR) && BR);
                    }
                }
            }
        }

        private static void AddRenewableChecks(MMRData.LogicFile logicFile, LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            Dictionary<string, Tuple<string, List<string>>> RenewableItems = new()
            {
                { "OOT_renewable_sticks", new("OOT_ANY_STICK", new List<string> { "OOT_STICK", "OOT_STICKS_5", "OOT_STICKS_10", "SHARED_STICK", "SHARED_STICKS_5", "SHARED_STICKS_10" }) },
                { "OOT_renewable_nuts", new("OOT_ANY_NUT", new List<string> { "OOT_NUT", "OOT_NUTS_5", "OOT_NUTS_10", "SHARED_NUT", "SHARED_NUTS_5", "SHARED_NUTS_10" }) },
                { "OOT_renewable_blue_fire", new("OOT_BLUE_FIRE", new List<string>()) },
                { "MM_renewable_red_potion", new("MM_POTION_RED", new List<string>()) },
                { "MM_renewable_blue_potion", new("MM_POTION_BLUE", new List<string>()) },
                { "MM_renewable_milk", new("MM_MILK", new List<string>()) }
            };

            //Add Reneawable Logic
            var OOTRenewableLocations = GetRenewableLocations("OOT");
            var MMRenewableLocations = GetRenewableLocations("MM");
            var RenewableLocations = OOTRenewableLocations.Concat(MMRenewableLocations);

            foreach (var item in RenewableItems)
            {
                MMRData.JsonFormatLogicItem LogicEntry = new() { Id = item.Key, ConditionalItems = new List<List<string>>() };
                foreach (var location in RenewableLocations)
                {
                    LogicEntry.ConditionalItems.Add(new List<string> { $"contains{{{location}, {item.Value.Item1}}}", $"check{{{location}}}" });
                }
                logicFile.Logic.Add(LogicEntry);
                if (item.Value.Item2.Any())
                {
                    dictionaryFile.Variables.Add(item.Value.Item1, new OptionData.TrackerVar { ID = item.Value.Item1, Static = true, Value = item.Value.Item2 });
                }
            }

            List<string> GetRenewableLocations(string GameCode)
            {
                var ONE_TIME_SHOP_CHECKS = new string[] { "MM Bomb Shop Bomb Bag", "MM Bomb Shop Bomb Bag 2", "MM Curiosity Shop All-Night Mask" };
                string path = Path.Combine(References.TestingPaths.GetDevTestingPath(), @"core-develop", "data", GameCode.ToLower(), "pool.csv");
                string[] RenewableTypes = new string[] { "shop", "cow" };
                var Pool = ConvertCsvFileToJsonObject(File.ReadAllLines(path).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
                return JsonConvert.DeserializeObject<List<MMROOTLocation>>(Pool)
                    .Where(x => RenewableTypes.Contains(x.type) && !ONE_TIME_SHOP_CHECKS.Contains($"{GameCode} {x.location}"))
                    .Select(x => $"{GameCode} {x.location}").ToList();
            }
        }

        private static void CreateLocationProxies(LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "CanGetOathWoodfall", RequiredItems = new List<string> { "MM Woodfall Temple After Boss" } });
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "CanGetOathSnowhead", RequiredItems = new List<string> { "MM Snowhead Temple After Boss" } });
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "CanGetOathGreatBay", RequiredItems = new List<string> { "MM Great Bay Temple After Boss" } });
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "CanGetOathStoneTow", RequiredItems = new List<string> { "MM Stone Tower After Boss" } });
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "KeatonInSpring", RequiredItems = new List<string> { "MM_MASK_KEATON", "MM_EVENT_BOSS_SNOWHEAD", "MM Mountain Village" } });


            createLocationProxy("MM Oath to Order", "MMOathWoodfallProxy", "Oath to Order", "Woodfall Temple", "CanGetOathWoodfall");
            createLocationProxy("MM Oath to Order", "MMOathSnowheadProxy", "Oath to Order", "Snowhead Temple", "CanGetOathSnowhead");
            createLocationProxy("MM Oath to Order", "MMOathGreatBayProxy", "Oath to Order", "Great Bay Temple", "CanGetOathGreatBay");
            createLocationProxy("MM Oath to Order", "MMOathStoneTowerProxy", "Oath to Order", "Stone Tower Temple", "CanGetOathStoneTow");

            createLocationProxy("MM Clock Town Keaton HP", "MMKeatonClocktownProxy", "Clock Town Keaton HP", "North Clock Town", "MM Clock Town Keaton HP");
            createLocationProxy("MM Clock Town Keaton HP", "MMKeatonMilkRoadProxy", "Milk Road Keaton HP", "Milk Road", "MM Clock Town Keaton HP");
            createLocationProxy("MM Clock Town Keaton HP", "MMKeatonMountainProxy", "Mountain Village Keaton HP", "Mountain Village", "KeatonInSpring");

            void createLocationProxy(string OriginalLocationID, string ID, string ProxyName, string ProxyArea, string Logic)
            {
                var OriginalLocation = dictionaryFile.LocationList[OriginalLocationID];
                OriginalLocation.LocationProxys.Add(new LogicDictionaryData.DictLocationProxy()
                {
                    ID = ID,
                    Area = ProxyArea,
                    Name = ProxyName,
                    LogicInheritance = Logic
                });
            }
        }

        private static void CreateEntrancePairs(LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            var RandomizedEntrances = dictionaryFile.EntranceList.Where(x => x.Value.RandomizableEntrance);

            foreach(var i in RandomizedEntrances)
            {
                string PairId = $"{i.Value.Exit} => {i.Value.Area}";
                if (RandomizedEntrances.Any(x => x.Key == PairId))
                {
                    i.Value.EntrancePairID = new EntranceData.EntranceAreaPair { Area= i.Value.Exit, Exit = i.Value.Area };
                }
            }
        }

        private static void RemoveGameFromNames(LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            foreach(var i in dictionaryFile.ItemList)
            {
                i.Value.Name = i.Value.Name.Replace("OOT ", "").Replace("MM ", "");
                i.Value.SpoilerData.SpoilerLogNames = i.Value.SpoilerData.SpoilerLogNames.Append(i.Value.Name).Distinct().ToArray();
                i.Value.SpoilerData.GossipHintNames = i.Value.SpoilerData.GossipHintNames.Append(i.Value.Name).Distinct().ToArray();
            }
            foreach (var i in dictionaryFile.LocationList)
            {
                i.Value.Name = i.Value.Name.Replace("OOT ", "").Replace("MM ", "");
            }
        }

        private static void WorldEventRequirementOptions(LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem
            {
                Id = $"MM_HAS_MOON_REQUIREMENTS",
                RequiredItems = new List<string> { "moon_req, moon_count" }
            });
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem
            {
                Id = $"OOT_HAS_BRIDGE_REQUIREMENTS",
                RequiredItems = new List<string> { "bridge_req, bridge_count" }
            });

            Dictionary<string, string[]> PossibleReqs = new()
            {
                { "Spiritual Stones|stones", new string[] { "OOT_STONE_EMERALD", "OOT_STONE_RUBY", "OOT_STONE_SAPPHIRE" } },
                { "Medallions|medallions", new string[] { "OOT_MEDALLION_LIGHT", "OOT_MEDALLION_FOREST", "OOT_MEDALLION_FIRE", "OOT_MEDALLION_WATER", "OOT_MEDALLION_SPIRIT", "OOT_MEDALLION_SHADOW" } },
                { "Boss Remains|remains", new string[] { "MM_REMAINS_ODOLWA", "MM_REMAINS_GOHT", "MM_REMAINS_GYORG", "MM_REMAINS_TWINMOLD" } },
                { "Gold Skulltulas Tokens|skullsGold", new string[] { "OOT_GS_TOKEN" } },
                { "Swamp Skulltulas Tokens|skullsSwamp", new string[] { "MM_GS_TOKEN_SWAMP" } },
                { "Ocean Skulltulas Tokens|skullsOcean", new string[] { "MM_GS_TOKEN_OCEAN" } },
                { "Stray Fairies (Woodfall)|fairiesWF", new string[] { "MM_STRAY_FAIRY_WF" } },
                { "Stray Fairies (Snowhead)|fairiesSH", new string[] { "MM_STRAY_FAIRY_SH" } },
                { "Stray Fairies (Great Bay)|fairiesGB", new string[] { "MM_STRAY_FAIRY_GB" } },
                { "Stray Fairies (Stone Tower)|fairiesST", new string[] { "MM_STRAY_FAIRY_ST" } },
                { "Stray Fairy (Clock Town)|fairyTown", new string[] { "MM_STRAY_FAIRY_TOWN" } },
                { "Regular Masks (MM)|masksRegular", new string[] { "MM_MASK_CAPTAIN", "MM_MASK_GIANT", "MM_MASK_ALL_NIGHT", "MM_MASK_BUNNY", "MM_MASK_KEATON", "MM_MASK_GARO", "MM_MASK_ROMANI", "MM_MASK_TROUPE_LEADER", "MM_MASK_POSTMAN", "MM_MASK_COUPLE", "MM_MASK_GREAT_FAIRY", "MM_MASK_GIBDO", "MM_MASK_DON_GERO", "MM_MASK_KAMARO", "MM_MASK_TRUTH", "MM_MASK_STONE", "MM_MASK_BREMEN", "MM_MASK_BLAST", "MM_MASK_SCENTS", "MM_MASK_KAFEI", "SHARED_MASK_TRUTH", "SHARED_MASK_BUNNY", "SHARED_MASK_KEATON" } },
                { "Transformation Masks (MM)|masksTransform", new string[] { "MM_MASK_DEKU", "MM_MASK_GORON", "MM_MASK_ZORA", "MM_MASK_FIERCE_DEITY", "SHARED_MASK_GORON", "SHARED_MASK_ZORA" } },
                { "Masks (OoT)|masksOot", new string[] { "OOT_MASK_SKULL", "OOT_MASK_SPOOKY", "OOT_MASK_KEATON", "OOT_MASK_BUNNY", "OOT_MASK_TRUTH", "OOT_MASK_GERUDO", "OOT_MASK_GORON", "OOT_MASK_ZORA", "SHARED_MASK_KEATON", "SHARED_MASK_BUNNY", "SHARED_MASK_TRUTH", "SHARED_MASK_GORON", "SHARED_MASK_ZORA" } }
            };

            foreach (var i in PossibleReqs)
            {
                var namedata = i.Key.Split('|');

                OptionData.TrackerOption MoonRequirement = new OptionData.TrackerOption();
                MoonRequirement.ID = $"moon_{namedata[1]}";
                MoonRequirement.DisplayName = namedata[0];
                MoonRequirement.SubCategory = "Moon Access Conditions";
                MoonRequirement.CurrentValue = (namedata[0] == "Boss Remains").ToString().ToLower();
                MoonRequirement.CreateSimpleValues(new string[] { "true", "false" });
                MoonRequirement.Values["true"].VariableEdit.Add("moon_req", new OptionData.VariableEditData { action = MiscData.MathOP.add, EditValue = i.Value });
                dictionaryFile.Options.Add(MoonRequirement.ID, MoonRequirement);

                OptionData.TrackerOption BridgeRequirement = new OptionData.TrackerOption();
                BridgeRequirement.ID = $"bridge_{namedata[1]}";
                BridgeRequirement.DisplayName = namedata[0];
                BridgeRequirement.SubCategory = "Rainbow Bridge Conditions";
                BridgeRequirement.CurrentValue = (namedata[0] == "Medallions").ToString().ToLower();
                BridgeRequirement.CreateSimpleValues(new string[] { "true", "false" });
                BridgeRequirement.Values["true"].VariableEdit.Add("bridge_req", new OptionData.VariableEditData { action = MiscData.MathOP.add, EditValue = i.Value });
                dictionaryFile.Options.Add(BridgeRequirement.ID, BridgeRequirement);
            }

            OptionData.TrackerVar bridge_req = new OptionData.TrackerVar();
            bridge_req.Static = true;
            bridge_req.Name = "bridge_req";
            bridge_req.ID = "bridge_req";
            bridge_req.Value = new List<string>();
            dictionaryFile.Variables.Add(bridge_req.ID, bridge_req);

            OptionData.TrackerVar bridge_req_count = new OptionData.TrackerVar();
            bridge_req_count.Static = false;
            bridge_req_count.SubCategory = "Rainbow Bridge Conditions";
            bridge_req_count.Name = "Items Required";
            bridge_req_count.ID = "bridge_count";
            bridge_req_count.Value = 6;
            dictionaryFile.Variables.Add(bridge_req_count.ID, bridge_req_count);

            OptionData.TrackerVar moon_req = new OptionData.TrackerVar();
            moon_req.Static = true;
            moon_req.Name = "moon_req";
            moon_req.ID = "moon_req";
            moon_req.Value = new List<string>();
            dictionaryFile.Variables.Add(moon_req.ID, moon_req);

            OptionData.TrackerVar moon_req_count = new OptionData.TrackerVar();
            moon_req_count.Static = false;
            moon_req_count.SubCategory = "Moon Access Conditions";
            moon_req_count.Name = "Items Required";
            moon_req_count.ID = "moon_count";
            moon_req_count.Value = 4;
            dictionaryFile.Variables.Add(moon_req_count.ID, moon_req_count);

        }

        private static void AddVariablesandOptions(LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            OptionData.TrackerVar MM_Masks = new OptionData.TrackerVar();
            MM_Masks.Static = true;
            MM_Masks.Value =new List<string> {
                "MM_MASK_POSTMAN",
                "MM_MASK_ALL_NIGHT",
                "MM_MASK_BLAST",
                "MM_MASK_STONE",
                "MM_MASK_GREAT_FAIRY",
                "MM_MASK_KEATON",
                "MM_MASK_BREMEN",
                "MM_MASK_BUNNY",
                "MM_MASK_DON_GERO",
                "MM_MASK_SCENTS",
                "MM_MASK_ROMANI",
                "MM_MASK_TROUPE_LEADER",
                "MM_MASK_KAFEI",
                "MM_MASK_COUPLE",
                "MM_MASK_TRUTH",
                "MM_MASK_KAMARO",
                "MM_MASK_GIBDO",
                "MM_MASK_GARO",
                "MM_MASK_CAPTAIN",
                "MM_MASK_GIANT"
            };
            MM_Masks.Name = "MM Masks";
            MM_Masks.ID = "MM_MASKS";
            dictionaryFile.Variables.Add(MM_Masks.ID, MM_Masks);


            OptionData.TrackerOption ageFilter = new OptionData.TrackerOption();
            ageFilter.ID = "age_filter";
            ageFilter.DisplayName = "Age Filter";
            ageFilter.CurrentValue = "both";
            ageFilter.CreateSimpleValues(new string[] { "both", "adult", "child" });
            dictionaryFile.Options.Add(ageFilter.ID, ageFilter);

            OptionData.TrackerOption EggContentShuffle = new OptionData.TrackerOption();
            EggContentShuffle.ID = "eggShuffle";
            EggContentShuffle.DisplayName = "Egg Content Shuffle";
            EggContentShuffle.CurrentValue = "false";
            EggContentShuffle.CreateSimpleValues(new string[] { "true", "false" });
            EggContentShuffle.Values["false"].AddMaxAmountEdit("OOT_WEIRD_EGG", MiscData.MathOP.set, 0);
            EggContentShuffle.Values["false"].AddMaxAmountEdit("OOT_POCKET_EGG", MiscData.MathOP.set, 0);
            dictionaryFile.Options.Add(EggContentShuffle.ID,EggContentShuffle);

            OptionData.TrackerOption DoorOfTime = new OptionData.TrackerOption();
            DoorOfTime.ID = "doorOfTime";
            DoorOfTime.DisplayName = "Open Door Of Time";
            DoorOfTime.CurrentValue = "closed";
            DoorOfTime.CreateSimpleValues(new string[] { "open", "closed" });
            DoorOfTime.Values["open"].Name = "Open";
            DoorOfTime.Values["closed"].Name = "Closed";
            dictionaryFile.Options.Add(DoorOfTime.ID,DoorOfTime);

            OptionData.TrackerOption CrossGameOOTWarpSong = new OptionData.TrackerOption();
            CrossGameOOTWarpSong.ID = "crossWarpOot";
            CrossGameOOTWarpSong.DisplayName = "Cross-Games OoT Warp Songs";
            CrossGameOOTWarpSong.CurrentValue = "false";
            CrossGameOOTWarpSong.CreateSimpleValues(new string[] { "true", "false" });
            dictionaryFile.Options.Add(CrossGameOOTWarpSong.ID, CrossGameOOTWarpSong);

            OptionData.TrackerOption CrossGameMMWarpSong = new OptionData.TrackerOption();
            CrossGameMMWarpSong.ID = "crossWarpMm";
            CrossGameMMWarpSong.DisplayName = "Cross-Games MM Song of Soaring";
            CrossGameMMWarpSong.CurrentValue = "none";
            CrossGameMMWarpSong.CreateSimpleValues(new string[] { "none", "childOnly", "full" });
            CrossGameMMWarpSong.Values["none"].Name = "None";
            CrossGameMMWarpSong.Values["childOnly"].Name = "Child Only";
            CrossGameMMWarpSong.Values["full"].Name = "Child & Adult";
            dictionaryFile.Options.Add(CrossGameMMWarpSong.ID, CrossGameMMWarpSong);

            OptionData.TrackerOption GanonBossKey = new OptionData.TrackerOption();
            GanonBossKey.ID = "ganonBossKey";
            GanonBossKey.DisplayName = "Ganon's Boss Key";
            GanonBossKey.CurrentValue = "removed";
            GanonBossKey.CreateSimpleValues(new string[] { "removed", "vanilla", "ganon", "anywhere" });
            GanonBossKey.Values["removed"].Name = "Removed";
            GanonBossKey.Values["vanilla"].Name = "Vanilla";
            GanonBossKey.Values["ganon"].Name = "Ganon's Castle";
            GanonBossKey.Values["anywhere"].Name = "Anywhere";
            GanonBossKey.Values["removed"].AddMaxAmountEdit("OOT_BOSS_KEY_GANON", MiscData.MathOP.set, 0);
            dictionaryFile.Options.Add(GanonBossKey.ID, GanonBossKey);

            OptionData.TrackerOption SmallKey = new OptionData.TrackerOption();
            SmallKey.ID = "smallKeyShuffle";
            SmallKey.DisplayName = "Small Key Shuffle";
            SmallKey.CurrentValue = "ownDungeon";
            SmallKey.CreateSimpleValues(new string[] { "ownDungeon", "anywhere" });
            SmallKey.Values["ownDungeon"].Name = "Own Dungeon";
            SmallKey.Values["anywhere"].Name = "Anywhere";
            SmallKey.Values["ownDungeon"].AddMaxAmountEdit("OOT_SMALL_KEY_FIRE", MiscData.MathOP.subtract, 1);
            dictionaryFile.Options.Add(SmallKey.ID, SmallKey);

            OptionData.TrackerOption ProgressiveShieldsOOT = new OptionData.TrackerOption();
            ProgressiveShieldsOOT.ID = "progressiveShieldsOot";
            ProgressiveShieldsOOT.DisplayName = "Progressive OoT Shields";
            ProgressiveShieldsOOT.CurrentValue = "separate";
            ProgressiveShieldsOOT.SubCategory = "Progressive Items";
            ProgressiveShieldsOOT.CreateSimpleValues(new string[] { "separate", "progressive" });
            ProgressiveShieldsOOT.Values["separate"].AddMaxAmountEdit("OOT_SHIELD", MiscData.MathOP.set, 0);
            ProgressiveShieldsOOT.Values["progressive"].AddMaxAmountEdit("OOT_SHIELD_DEKU", MiscData.MathOP.set, 0);
            ProgressiveShieldsOOT.Values["progressive"].AddMaxAmountEdit("OOT_SHIELD_HYLIAN", MiscData.MathOP.set, 0);
            ProgressiveShieldsOOT.Values["progressive"].AddMaxAmountEdit("OOT_SHIELD_MIRROR", MiscData.MathOP.set, 0);
            dictionaryFile.Options.Add(ProgressiveShieldsOOT.ID, ProgressiveShieldsOOT);

            OptionData.TrackerOption ProgressiveSwordsOOT = new OptionData.TrackerOption();
            ProgressiveSwordsOOT.ID = "progressiveSwordsOot";
            ProgressiveSwordsOOT.DisplayName = "Progressive OoT Swords";
            ProgressiveSwordsOOT.CurrentValue = "goron";
            ProgressiveSwordsOOT.SubCategory = "Progressive Items";
            ProgressiveSwordsOOT.CreateSimpleValues(new string[] { "separate", "progressive", "goron" });
            ProgressiveSwordsOOT.Values["separate"].AddMaxAmountEdit("OOT_SWORD", MiscData.MathOP.set, 0);
            ProgressiveSwordsOOT.Values["separate"].AddMaxAmountEdit("OOT_SWORD_GORON", MiscData.MathOP.set, 0);
            ProgressiveSwordsOOT.Values["progressive"].AddMaxAmountEdit("OOT_SWORD_KOKIRI", MiscData.MathOP.set, 0);
            ProgressiveSwordsOOT.Values["progressive"].AddMaxAmountEdit("OOT_SWORD_MASTER", MiscData.MathOP.set, 0);
            ProgressiveSwordsOOT.Values["progressive"].AddMaxAmountEdit("OOT_SWORD_GORON", MiscData.MathOP.set, 0);
            ProgressiveSwordsOOT.Values["progressive"].AddMaxAmountEdit("OOT_SWORD_KNIFE", MiscData.MathOP.set, 0);
            ProgressiveSwordsOOT.Values["progressive"].AddMaxAmountEdit("OOT_SWORD_BIGGORON", MiscData.MathOP.set, 0);
            ProgressiveSwordsOOT.Values["goron"].AddMaxAmountEdit("OOT_SWORD", MiscData.MathOP.set, 0);
            ProgressiveSwordsOOT.Values["goron"].AddMaxAmountEdit("OOT_SWORD_KNIFE", MiscData.MathOP.set, 0);
            ProgressiveSwordsOOT.Values["goron"].AddMaxAmountEdit("OOT_SWORD_BIGGORON", MiscData.MathOP.set, 0);
            ProgressiveSwordsOOT.Values["separate"].Name = "Separate";
            ProgressiveSwordsOOT.Values["goron"].Name = "Progressive Knife and Biggoron";
            ProgressiveSwordsOOT.Values["progressive"].Name = "Progressive";
            dictionaryFile.Options.Add(ProgressiveSwordsOOT.ID, ProgressiveSwordsOOT);

            OptionData.TrackerOption ProgressiveShieldsMM = new OptionData.TrackerOption();
            ProgressiveShieldsMM.ID = "progressiveShieldsMm";
            ProgressiveShieldsMM.DisplayName = "Progressive MM Shields";
            ProgressiveShieldsMM.CurrentValue = "start";
            ProgressiveShieldsMM.SubCategory = "Progressive Items";
            ProgressiveShieldsMM.CreateSimpleValues(new string[] { "separate", "start", "progressive" });
            ProgressiveShieldsMM.Values["separate"].AddMaxAmountEdit("MM_SHIELD", MiscData.MathOP.set, 0);
            ProgressiveShieldsMM.Values["start"].AddMaxAmountEdit("MM_SHIELD", MiscData.MathOP.set, 0);
            ProgressiveShieldsMM.Values["progressive"].AddMaxAmountEdit("MM_SHIELD_MIRROR", MiscData.MathOP.set, 0);
            ProgressiveShieldsMM.Values["separate"].Name = "Separate";
            ProgressiveShieldsMM.Values["start"].Name = "Start with Hero Shield";
            ProgressiveShieldsMM.Values["progressive"].Name = "Progressive";
            dictionaryFile.Options.Add(ProgressiveShieldsMM.ID, ProgressiveShieldsMM);

            OptionData.TrackerOption ProgressiveLullabyMM = new OptionData.TrackerOption();
            ProgressiveLullabyMM.ID = "progressiveGoronLullaby";
            ProgressiveLullabyMM.DisplayName = "Progressive MM Goron Lullaby";
            ProgressiveLullabyMM.CurrentValue = "progressive";
            ProgressiveLullabyMM.SubCategory = "Progressive Items";
            ProgressiveLullabyMM.CreateSimpleValues(new string[] { "single", "progressive" });
            ProgressiveLullabyMM.Values["single"].AddMaxAmountEdit("MM_SONG_GORON_HALF", MiscData.MathOP.set, 0);
            ProgressiveLullabyMM.Values["progressive"].AddMaxAmountEdit("MM_SONG_GORON", MiscData.MathOP.set, 0);
            dictionaryFile.Options.Add(ProgressiveLullabyMM.ID, ProgressiveLullabyMM);

            OptionData.TrackerOption fairyOcarinaMm = new OptionData.TrackerOption();
            fairyOcarinaMm.ID = "fairyOcarinaMm";
            fairyOcarinaMm.DisplayName = "Fairy Ocarina in MM";
            fairyOcarinaMm.CurrentValue = "false";
            fairyOcarinaMm.CreateSimpleValues(new string[] { "true", "false" });
            fairyOcarinaMm.Values["false"].AddMaxAmountEdit("MM_OCARINA", MiscData.MathOP.subtract, 1);
            dictionaryFile.Options.Add(fairyOcarinaMm.ID, fairyOcarinaMm);

            OptionData.TrackerOption shortHookshotMm = new OptionData.TrackerOption();
            shortHookshotMm.ID = "shortHookshotMm";
            shortHookshotMm.DisplayName = "Short Hookshot in MM";
            shortHookshotMm.CurrentValue = "false";
            shortHookshotMm.CreateSimpleValues(new string[] { "true", "false" });
            shortHookshotMm.Values["false"].AddMaxAmountEdit("MM_HOOKSHOT", MiscData.MathOP.subtract, 1);
            dictionaryFile.Options.Add(shortHookshotMm.ID, shortHookshotMm);

            //Game Clear
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "Game_Clear", RequiredItems = new List<string> { "OOT_EVENT_GANON", "MM_EVENT_MAJORA" } });
            dictionaryFile.MacroList.Add("Game_Clear", new LogicDictionaryData.DictionaryMacroEntry { ID = "Game_Clear", Name = "Both Games Cleared" });
            dictionaryFile.WinCondition = "Game_Clear";

            //Temp Workaround for a typo in logic
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "MM_ZORA", RequiredItems = new List<string> { "MM_MASK_ZORA" } });

            AddSharedItemOptions("sharedNutsSticks", "Shared Nuts and Sticks", new string[] { "NUT", "NUTS_5", "NUTS_5_ALT", "NUTS_10", "STICK", "STICKS_5", "STICKS_10" }, 1);
            AddSharedItemOptions("sharedBows", "Shared Bows", new string[] { "BOW" }, 1, new string[] { "ARROWS_5", "ARROWS_10", "ARROWS_30", "ARROWS_40", });
            AddSharedItemOptions("sharedBombBags", "Shared Bomb Bags", new string[] { "BOMB_BAG" }, 1, new string[] { "BOMBS_5", "BOMBS_10", "BOMBS_20", "BOMBS_30", "BOMB" });
            AddSharedItemOptions("sharedMagic", "Shared Magic", new string[] { "MAGIC_UPGRADE" }, 1);
            AddSharedItemOptions("sharedMagicArrows", "Shared Magic Arrows", new string[] { "ARROW_FIRE", "ARROW_ICE", "ARROW_LIGHT" }, 1);
            AddSharedItemOptions("sharedSongs", "Shared Songs", new string[] { "SONG_TIME", "SONG_EPONA", "SONG_STORMS" }, 1);
            AddSharedItemOptions("sharedHookshot", "Shared Hookshots", new string[] { "HOOKSHOT" }, 2);
            AddSharedItemOptions("sharedLens", "Shared Lens of Truth", new string[] { "LENS" }, 1);
            AddSharedItemOptions("sharedOcarina", "Shared Ocarina", new string[] { "OCARINA" }, 2);
            AddSharedItemOptions("sharedMasks", "Shared Masks", new string[] { "MASK_ZORA", "MASK_GORON", "MASK_TRUTH", "MASK_BUNNY", "MASK_KEATON" }, 1);
            AddSharedItemOptions("sharedWallets", "Shared Wallets", new string[] { "WALLET" }, 2, new string[] { "RUPEE_GREEN", "RUPEE_BLUE", "RUPEE_RED", "RUPEE_PURPLE", "RUPEE_SILVER", "RUPEE_GOLD", "RUPEE_HUGE" });
            AddSharedItemOptions("sharedHealth", "Shared Health", new string[] { "HEART_PIECE", "HEART_CONTAINER", "DEFENSE_UPGRADE" }, 1, new string[] { "RECOVERY_HEART", }); //I don't actually think any of this effects logic?

            AddMQOption("DTMQ", "Deku Tree");
            AddMQOption("DCMQ", "Dodongos Cavern");
            AddMQOption("JJMQ", "Jabu Jabu");
            AddMQOption("ForestMQ", "Forest Temple", "OOT_SMALL_KEY_FOREST", 6);
            AddMQOption("FireMQ", "Fire Temple", "OOT_SMALL_KEY_FIRE", 5);
            AddMQOption("WaterMQ", "Water Temple", "OOT_SMALL_KEY_WATER", 2);
            AddMQOption("ShadowMQ", "Shadow Temple", "OOT_SMALL_KEY_SHADOW", 6);
            AddMQOption("SpiritMQ", "Spirit Temple", "OOT_SMALL_KEY_SPIRIT", 7);
            AddMQOption("BotWMQ", "Bottom of the Well", "OOT_SMALL_KEY_BOTW", 2);
            AddMQOption("ICMQ", "Ice Cavern");
            AddMQOption("GTGMQ", "Gerudo Training Grounds", "OOT_SMALL_KEY_GTG", 3);
            AddMQOption("GanonMQ", "Ganon's Castle", "OOT_SMALL_KEY_GANON", 3);

            WorldEventRequirementOptions(dictionaryFile);

            void AddMQOption(string ID, string Name, string Key = null, int MaxKeys = 0)
            {
                OptionData.TrackerOption MQEntry = new OptionData.TrackerOption();
                MQEntry.ID = ID;
                MQEntry.DisplayName = Name;
                MQEntry.SubCategory = "Master Quest Dungeons";
                MQEntry.CurrentValue = "false";
                MQEntry.CreateSimpleValues(new string[] { "true", "false" });
                if (Key != null) { MQEntry.Values["true"].AddMaxAmountEdit(Key, MiscData.MathOP.set, MaxKeys); }
                dictionaryFile.Options.Add(ID, MQEntry);
            }

            void AddSharedItemOptions(string ID, string Name, string[] Items, int LogicalAmount, string[] NonLogicReplacements = null)
            {
                OptionData.LogicReplacement OnActionReplacementData = new();

                OptionData.actions onAction = new();
                OptionData.actions offAction = new();

                foreach (var Item in Items)
                {
                    OnActionReplacementData.ReplacementList.Add($"OOT_{Item}", $"SHARED_{Item}");
                    OnActionReplacementData.ReplacementList.Add($"MM_{Item}", $"SHARED_{Item}");
                    if (LogicalAmount > 1)
                    {
                        for (var i = 2; i <= LogicalAmount; i++)
                        {
                            OnActionReplacementData.ReplacementList.Add($"OOT_{Item}, {LogicalAmount}", $"SHARED_{Item}, {LogicalAmount}");
                            OnActionReplacementData.ReplacementList.Add($"MM_{Item}, {LogicalAmount}", $"SHARED_{Item}, {LogicalAmount}");
                        }
                    }

                    onAction.AddMaxAmountEdit($"OOT_{Item}", MiscData.MathOP.set, 0);
                    onAction.AddMaxAmountEdit($"MM_{Item}", MiscData.MathOP.set, 0);

                    offAction.AddMaxAmountEdit($"SHARED_{Item}", MiscData.MathOP.set, 0);
                }
                onAction.LogicReplacements = new OptionData.LogicReplacement[] { OnActionReplacementData };

                if (NonLogicReplacements != null)
                {
                    foreach(var Item in NonLogicReplacements)
                    {
                        onAction.AddMaxAmountEdit($"OOT_{Item}", MiscData.MathOP.set, 0);
                        onAction.AddMaxAmountEdit($"MM_{Item}", MiscData.MathOP.set, 0);
                        offAction.AddMaxAmountEdit($"SHARED_{Item}", MiscData.MathOP.set, 0);
                    }
                }

                OptionData.TrackerOption SharedItem = new()
                {
                    ID = ID,
                    DisplayName = Name,
                    CurrentValue = "false",
                    SubCategory = "Shared Items",
                    Values = new Dictionary<string, OptionData.actions>
                    {
                        { "false", offAction },
                        { "true", onAction }
                    }
                };

                dictionaryFile.Options.Add(ID, SharedItem);
            }

            foreach (var i in dictionaryFile.ItemList.Where(x => x.Value.Name.StartsWith("Small Key (")))
            {
                int Maxinworld = i.Value.MaxAmountInWorld is not null && i.Value.MaxAmountInWorld > 0 ? (int)i.Value.MaxAmountInWorld : 9;
                if (Maxinworld < 2) { continue; }
                string Dungeon = i.Value.Name.Split('(')[1].Trim().TrimEnd(')');
                string KeyRingName = i.Key.Replace($"SMALL_KEY", "KEY_RING");
                OptionData.TrackerOption KeyRingOption = new OptionData.TrackerOption();
                KeyRingOption.ID = $"{Dungeon.Replace(" ", "")}KeyRing";
                KeyRingOption.DisplayName = $"{Dungeon} Key Ring";
                KeyRingOption.CreateSimpleValues(new string[] { "true", "false", "random" });
                KeyRingOption.CurrentValue = "false";
                KeyRingOption.SubCategory = "Key Rings";
                KeyRingOption.Values["true"].LogicReplacements = new OptionData.LogicReplacement[] { new OptionData.LogicReplacement() };
                KeyRingOption.Values["true"].LogicReplacements[0].ReplacementList.Add(i.Key, i.Key.Replace("SMALL_KEY", "KEY_RING"));
                KeyRingOption.Values["true"].AddMaxAmountEdit(i.Key, MiscData.MathOP.set, 0);
                KeyRingOption.Values["false"].AddMaxAmountEdit(KeyRingName, MiscData.MathOP.set, 0);
                for (var c = 1; c <= Maxinworld; c++)
                {
                    KeyRingOption.Values["true"].LogicReplacements[0].ReplacementList.Add($"{i.Key}, {c}", KeyRingName);
                }
                //dictionaryFile.Options.Add(KeyRingOption);
            }
        }

        public static void CleanLogicAndParse(TrackerObjects.MMRData.LogicFile LogicFile)
        {
            string MMLogic = Path.Combine(References.TestingPaths.GetDevTestingPath(), "core-develop", "data", "mm", "world");
            string OOTLogic = Path.Combine(References.TestingPaths.GetDevTestingPath(), "core-develop", "data", "oot", "world");
            string OOTMQLogic = Path.Combine(References.TestingPaths.GetDevTestingPath(), "core-develop", "data", "oot", "world_mq");
            string MMOOTCodeMMMacros = Path.Combine(References.TestingPaths.GetDevCodePath(), @"MMR Tracker V3", "OtherGames", "OOTMMRCOMBO", @"MMMacroOverride.json");
            string MMOOTCodeOOTMacros = Path.Combine(References.TestingPaths.GetDevCodePath(), @"MMR Tracker V3", "OtherGames", "OOTMMRCOMBO", @"OOTMacroOverride.json");
            Dictionary<string, string> MMMacros = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(MMOOTCodeMMMacros));
            Dictionary<string, string> OOTMacros = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(MMOOTCodeOOTMacros));
            var files = Directory.GetFiles(MMLogic).Concat(Directory.GetFiles(OOTLogic)).Concat(Directory.GetFiles(OOTMQLogic)).ToList();

            var OOTVanillaFiles = Directory.GetFiles(OOTLogic);
            var OOTMQFiles = Directory.GetFiles(OOTMQLogic);
            var VariableDungeons = new List<string>();
            foreach (var file in OOTMQFiles)
            {
                string ValillaName = file.Replace("_mq", "");
                if (OOTVanillaFiles.Contains(ValillaName))
                {
                    VariableDungeons.Add(ValillaName);
                    VariableDungeons.Add(file);
                }
            }

            string GetLogic(string Logic, string GameCode, string Area, bool VariableDungeon, bool MQ, string DungeonCode, bool IsExit = false)
            {
                string ReturnLogic = $"({Logic})";
                if (!IsExit) { ReturnLogic += $" && ({GameCode} {Area})"; }
                if (VariableDungeon) { ReturnLogic += $" && (option{{{DungeonCode}MQ, {MQ.ToString().ToLower()}}})"; }
                return $"({ReturnLogic})";
            }

            string CombineLogicFromOtherSource(MMRData.JsonFormatLogicItem Logic, string NewLogic, string DebugMessage = null)
            {
                if (Logic.ConditionalItems.Any() || Logic.RequiredItems.Any())
                {
                    //Debug.WriteLine(DebugMessage);
                    LogicUtilities.MoveRequirementsToConditionals(Logic);
                    return $"({NewLogic}) || ({LogicStringConverter.ConvertConditionalToLogicString(OOTMMLogicParser, Logic.ConditionalItems)})";
                }
                return NewLogic;
            }

            void ApplyLogic(MMRData.JsonFormatLogicItem LogicFileEntry, string InLogic, string Game, string Debugname = "")
            {
                string Logic = CombineLogicFromOtherSource(LogicFileEntry, InLogic, $"LOGGING- {Debugname} Duplicate Logic Entry");
                LogicFileEntry.ConditionalItems = ParselogicLine(Logic, Game);
                LogicUtilities.RemoveRedundantConditionals(LogicFileEntry);
                LogicUtilities.MakeCommonConditionalsRequirements(LogicFileEntry);
            }

            foreach(var file in files)
            {
                string Game = file.Contains(@"\mm\") ? "MM": "OOT";
                string OpositeGame = Game == "OOT" ? "MM" : "OOT";
                bool VariableDungeon = VariableDungeons.Contains(file);
                bool MQDungeon = file.Contains(@"\world_mq");
                var FileOBJ = JsonConvert.DeserializeObject<Dictionary<string, MMROOTLogicEntry>>(Utility.ConvertYamlStringToJsonString(File.ReadAllText(file)));
                foreach(var key in FileOBJ.Keys)
                {
                    foreach(var i in FileOBJ[key].locations?.Keys.ToArray()??Array.Empty<string>())
                    {
                        var LogicFileEntry = LogicFile.Logic.Find(x => x.Id == i || x.Id == $"{Game} {i}" || x.Id == $"{Game}_{i}");
                        if (LogicFileEntry is not null)
                        {
                            string Logic = GetLogic(FileOBJ[key].locations[i], Game, key, VariableDungeon, MQDungeon, FileOBJ[key].dungeon);
                            ApplyLogic(LogicFileEntry, Logic, Game, $"{key} {i}");
                        }
                    }
                    foreach (var i in FileOBJ[key].exits?.Keys.ToArray()??Array.Empty<string>())
                    {
                        string TrueAreaName = $"{Game} {key}";
                        string TrueExitName = i.StartsWith($"{OpositeGame} ") || i.StartsWith($"{Game} ") ? $"{i}" : $"{Game} {i}";
                        string FullexitName = $"{TrueAreaName} => {TrueExitName}";
                        var LogicFileEntry = LogicFile.Logic.Find(x => x.Id == FullexitName);
                        if (LogicFileEntry is not null)
                        {
                            string Logic = GetLogic(FileOBJ[key].exits[i], Game, key, VariableDungeon, MQDungeon, FileOBJ[key].dungeon, true);
                            ApplyLogic(LogicFileEntry, Logic, Game, FullexitName);
                        }

                    }
                    foreach (var i in FileOBJ[key].events?.Keys.ToArray()??Array.Empty<string>())
                    {
                        var LogicFileEntry = LogicFile.Logic.Find(x => x.Id == i || x.Id == $"{Game} {i}" || x.Id == $"{Game}_EVENT_{i}");
                        if (LogicFileEntry is not null)
                        {
                            string Logic = GetLogic(FileOBJ[key].events[i], Game, key, VariableDungeon, MQDungeon, FileOBJ[key].dungeon);
                            ApplyLogic(LogicFileEntry, Logic, Game, $"{key} {i}");
                        }
                    }
                    foreach (var i in FileOBJ[key].gossip?.Keys.ToArray()??Array.Empty<string>())
                    {
                        var LogicFileEntry = LogicFile.Logic.Find(x => x.Id == i || x.Id == $"{Game} {i}" || x.Id == $"{Game}_{i}");
                        if (LogicFileEntry is not null)
                        {
                            string Logic = GetLogic(FileOBJ[key].gossip[i], Game, key, VariableDungeon, MQDungeon, FileOBJ[key].dungeon);
                            ApplyLogic(LogicFileEntry, Logic, Game, $"{key} {i}");
                        }
                    }
                }
            }
            foreach (var key in MMMacros.Keys)
            {
                var LogicFileEntry = LogicFile.Logic.Find(x => x.Id == key || x.Id == $"MM {key}" || x.Id == $"MM_{key}");
                if (LogicFileEntry is not null)
                {
                    string Logic = $"({MMMacros[key]})";
                    LogicFileEntry.ConditionalItems = ParselogicLine(Logic, "MM");
                    LogicUtilities.RemoveRedundantConditionals(LogicFileEntry);
                    LogicUtilities.MakeCommonConditionalsRequirements(LogicFileEntry);
                }
                else
                {
                    Debug.WriteLine($"{LogicFileEntry} was not added to the logic");
                }
            }
            foreach (var key in OOTMacros.Keys)
            {
                var LogicFileEntry = LogicFile.Logic.Find(x => x.Id == key || x.Id == $"OOT {key}" || x.Id == $"OOT_{key}");
                if (LogicFileEntry is not null)
                {
                    string Logic = $"({OOTMacros[key]})";
                    LogicFileEntry.ConditionalItems = ParselogicLine(Logic, "OOT");
                    LogicUtilities.RemoveRedundantConditionals(LogicFileEntry);
                    LogicUtilities.MakeCommonConditionalsRequirements(LogicFileEntry);
                }
                else
                {
                    Debug.WriteLine($"{LogicFileEntry} was not added to the logic");
                }
            }
        }

        public static List<List<string>> ParselogicLine(string Line, string Game)
        {
            string MMOOTCodeMMMacros = Path.Combine(References.TestingPaths.GetDevCodePath(), @"MMR Tracker V3", "OtherGames", "OOTMMRCOMBO", @"MMMacroOverride.json");
            string MMOOTCodeOOTMacros = Path.Combine(References.TestingPaths.GetDevCodePath(), @"MMR Tracker V3", "OtherGames", "OOTMMRCOMBO", @"OOTMacroOverride.json");
            Dictionary<string, string> MMMacros = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(MMOOTCodeMMMacros));
            Dictionary<string, string> OOTMacros = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(MMOOTCodeOOTMacros));

            string CleanLine = ParseFunction(Line, Game);
            List<List<string>> logic = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicParser, CleanLine);

            List<List<string>> finallogic = new List<List<string>>();

            foreach(var CondSet in logic)
            {
                List<string> newCondSet = new List<string>();
                foreach (var con in CondSet)
                {
                    if (Game == "MM")
                    {
                        if (!con.StartsWith("MM") && MMMacros.ContainsKey($"{con}")) { newCondSet.Add($"MM_{con}"); }
                        else { newCondSet.Add(con); }
                    }
                    else
                    {
                        if (!con.StartsWith("OOT") && OOTMacros.ContainsKey($"{con}")) { newCondSet.Add($"OOT_{con}"); }
                        else { newCondSet.Add(con); }
                    }
                }
                finallogic.Add(newCondSet);
            }

            return finallogic;
        }

        public static string ParseFunction(string LogicLine, string CurrentGame)
        {
            string line = LogicLine;
            var ParsedLine = OOTMMLogicParser.ParseLogicString(line);
            var FunctionsFound = ParsedLine.Where(x => x.Type == LogicStringParser.EntryType.function).Select(x => new LogicUtilities.LogicFunction(x.Text)).ToArray();

            while (FunctionsFound.Any())
            {
                var CurFunction = FunctionsFound[0];
                string FullFunction = CurFunction.ToString();
                string Function = CurFunction.Funtion;
                string Parameters = CurFunction.ParametersTrimmed;
                string Game = CurrentGame;
                if (Parameters.StartsWith($"MM_"))
                {
                    Parameters = Parameters.Replace("MM_", "");
                    Game = "MM";
                }
                else if (Parameters.StartsWith($"OOT_"))
                {
                    Parameters = Parameters.Replace("OOT_", "");
                    Game = "OOT";
                }
                switch (Function)
                {
                    case "special":
                        line = line.Replace(FullFunction, $"{Game}_HAS_{Parameters}_REQUIREMENTS");
                        break;
                    case "trick":
                        line = line.Replace(FullFunction, $"TRICK_{Game}_{Parameters}");
                        break;
                    case "has":
                        string ReplaceText = $"{Game}_{Parameters}";
                        int Amount = 1;
                        if (ReplaceText.Contains(", "))
                        {
                            Amount = Convert.ToInt32(ReplaceText.Split(",")[1].Trim());
                            ReplaceText = ReplaceText.Split(",")[0].Trim();
                        }
                        if (Amount > 1) { ReplaceText += $", {Amount}"; }
                        line = line.Replace(FullFunction, ReplaceText);
                        break;
                    case "event":
                        line = line.Replace(FullFunction, $"{Game}_EVENT_{Parameters}");
                        break;
                    case "adult_trade":
                        line = line.Replace(FullFunction, $"{Game}_is_adult && has({Parameters})");
                        break;
                    case "can_ride_bean":
                        line = line.Replace(FullFunction, $"{Game}_is_adult && event({Parameters})");
                        break;
                    case "masks":
                        line = line.Replace(FullFunction, $"MM_MASKS, {Parameters}");
                        break;
                    case "can_play":
                        line = line.Replace(FullFunction, $"has(OCARINA) && has({Parameters})");
                        break;
                    case "has_small_keys_fire":
                        int Keys = int.Parse(Parameters);
                        line = line.Replace(FullFunction, $"(option{{smallKeyShuffle, anywhere}} && has(SMALL_KEY_FIRE, {Keys + 1})) || (option{{smallKeyShuffle, anywhere, false}} && has(SMALL_KEY_FIRE, {Keys}))");
                        break;
                    case "setting":
                        line = line.Replace(FullFunction, $"option{{{Parameters}}}");
                        break;
                    case "age":
                        if (Parameters == "child") { line = line.Replace(FullFunction, $"option{{age_filter, adult, false}}"); }
                        else if (Parameters == "adult") { line = line.Replace(FullFunction, $"option{{age_filter, child, false}} && OOT_EVENT_TIME_TRAVEL"); }
                        break;
                    case "cond":
                        if (Parameters.StartsWith("setting"))
                        {
                            string CleanedInput = Parameters.Replace("setting(", "(");
                            Debug.WriteLine($"Parsing Cond \n[{Parameters}]");
                            var splitArray = Regex.Split(CleanedInput, @"(?<!,[^(]+\([^)]+),");
                            line = line.Replace(FullFunction, $"((option{{{splitArray[0].TrimStart('(')}, {splitArray[1].TrimEnd(')')}}} && {splitArray[2]}) || (option{{{splitArray[0].TrimStart('(')}, {splitArray[1].TrimEnd(')')}, false}} && {splitArray[3]}))");
                            Debug.WriteLine($"Success \n[(option{{{splitArray[0]}, {splitArray[1]}}} && {splitArray[2]}) || (option{{{splitArray[0]}, {splitArray[1]}, false}} && {splitArray[3]})]");
                        }
                        else
                        {
                            var splitArray = Parameters.Split(",").Select(x => x.Trim()).ToArray();
                            line = line.Replace(FullFunction, $"({splitArray[0]} && {splitArray[1]}) || ({splitArray[2]})");
                        }
                        break;
                    default:
                        line = line.Replace(FullFunction, $"ERROR UNHANDLED FUNTION {Function}");
                        break;
                }
                ParsedLine = OOTMMLogicParser.ParseLogicString(line);
                FunctionsFound = ParsedLine.Where(x => x.Type == LogicStringParser.EntryType.function).Select(x => new LogicUtilities.LogicFunction(x.Text)).ToArray();
            }
            return line;
        }

        public static void AddEntriesFromLogicFiles(TrackerObjects.MMRData.LogicFile LogicFile, TrackerObjects.LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            string TestFolder = References.TestingPaths.GetDevTestingPath();
            string CodeFolder = References.TestingPaths.GetDevCodePath();
            string MMOOTCodeDir = Path.Combine(TestFolder, @"core-develop");
            string MMOOTCodeData = Path.Combine(MMOOTCodeDir, @"data");
            string MMOOTCodeMM = Path.Combine(MMOOTCodeData, @"mm");
            string MMOOTCodeOOT = Path.Combine(MMOOTCodeData, @"oot");
            string OOTMMCode = Path.Combine(CodeFolder, @"MMR Tracker V3", "OtherGames", "OOTMMRCOMBO");
            string MMOOTCodeMMMacros = Path.Combine(OOTMMCode, @"MMMacroOverride.json");
            string MMOOTCodeOOTMacros = Path.Combine(OOTMMCode, @"OOTMacroOverride.json");
            string MMOOTCodeMMWorld = Path.Combine(MMOOTCodeMM, @"world");
            string MMOOTCodeOOTWorld = Path.Combine(MMOOTCodeOOT, @"world");
            string MMOOTCodeOOTMQWorld = Path.Combine(MMOOTCodeOOT, @"world_mq");
            string[] MMOOTCodeMMWorldFiles = Directory.GetFiles(MMOOTCodeMMWorld);
            string[] MMOOTCodeOOTWorldFiles = Directory.GetFiles(MMOOTCodeOOTWorld);
            string[] MMOOTCodeOOTMQWorldFiles = Directory.GetFiles(MMOOTCodeOOTMQWorld);
            string MMEntrances = Path.Combine(MMOOTCodeMM, @"entrances.csv");
            string OOTEntrances = Path.Combine(MMOOTCodeOOT, @"entrances.csv");
            string MMMacrosfile = File.ReadAllText(MMOOTCodeMMMacros);
            string OOTMacrosfile = File.ReadAllText(MMOOTCodeOOTMacros);


            var MMentranceData = JsonConvert.DeserializeObject<List<MMROOTEntranceData>>(ConvertCsvFileToJsonObject(File.ReadAllLines(MMEntrances))).ToDictionary(x => x.to, x => x.from);
            var OOTentranceData = JsonConvert.DeserializeObject<List<MMROOTEntranceData>>(ConvertCsvFileToJsonObject(File.ReadAllLines(OOTEntrances))).ToDictionary(x => x.to, x => x.from);

            Dictionary<string, string> RandoEntrances = MMentranceData.Concat(OOTentranceData).ToDictionary(x => x.Key, x => x.Value);

            Dictionary<string, string> MMMacros = JsonConvert.DeserializeObject<Dictionary<string, string>>(MMMacrosfile);
            Dictionary<string, string> OOTMacros = JsonConvert.DeserializeObject<Dictionary<string, string>>(OOTMacrosfile);

            Dictionary<string, MMROOTLogicEntry> MMLogicEntries = new Dictionary<string, MMROOTLogicEntry>();
            Dictionary<string, MMROOTLogicEntry> OOTLogicEntries = new Dictionary<string, MMROOTLogicEntry>();

            List<string> DungeonAreas = GetDungeonAreas(RandoEntrances);

            addEntranceandEventData("OOT", MMOOTCodeOOTWorldFiles);
            addEntranceandEventData("OOT", MMOOTCodeOOTMQWorldFiles);
            addEntranceandEventData("MM", MMOOTCodeMMWorldFiles);
            addMacros("OOT", OOTMacros);
            addMacros("MM", MMMacros);

            void addMacros(string Game, Dictionary<string, string> MacroFiles)
            {
                foreach(var i in MacroFiles.Keys)
                {
                    string Truename = $"{Game}_{i}";
                    if (LogicFile.Logic.Find(x => x.Id == Truename) == null)
                    {
                        LogicFile.Logic.Add(new TrackerObjects.MMRData.JsonFormatLogicItem { Id = Truename });
                    }
                }
            }

            void addEntranceandEventData(string Game, string[] WorldFiles)
            {
                string OpositeGame = Game == "OOT" ? "MM" : "OOT";
                foreach (var i in WorldFiles)
                {
                    var text = Utility.ConvertYamlStringToJsonString(File.ReadAllText(i));
                    var LogicObject = JsonConvert.DeserializeObject<Dictionary<string, MMROOTLogicEntry>>(text);
                    foreach (var l in LogicObject.Keys)
                    {
                        string TrueAreaName = $"{Game} {l}";
                        foreach (var exit in LogicObject[l]?.exits?.Keys?.ToList()??new List<string>())
                        {
                            string TrueExitName = exit.StartsWith($"{OpositeGame} ") || exit.StartsWith($"{Game} ") ? $"{exit}" : $"{Game} {exit}";
                            if (TrueExitName == "OOT MM SPAWN") { TrueExitName = "MM SPAWN"; }
                            string FullexitName = $"{TrueAreaName} => {TrueExitName}";

                            if (LogicFile.Logic.Find(x => x.Id == FullexitName) == null)
                            {
                                TrackerObjects.MMRData.JsonFormatLogicItem EntranceEntry = new TrackerObjects.MMRData.JsonFormatLogicItem();
                                EntranceEntry.Id = FullexitName;
                                LogicFile.Logic.Add(EntranceEntry);
                            }

                            if (dictionaryFile.EntranceList.Values.FirstOrDefault(x => x.ID == FullexitName) == null)
                            {
                                TrackerObjects.LogicDictionaryData.DictionaryEntranceEntries entranceEntry = new TrackerObjects.LogicDictionaryData.DictionaryEntranceEntries();
                                entranceEntry.ID = FullexitName;
                                entranceEntry.RandomizableEntrance = false;
                                entranceEntry.Area = TrueAreaName;
                                entranceEntry.Exit = TrueExitName;
                                entranceEntry.AlwaysAccessable = false;
                                if (FullexitName.StartsWith("OOT SPAWN => ")) { entranceEntry.AlwaysAccessable = true; }
                                if (RandoEntrances.ContainsKey(exit) && RandoEntrances[exit] == l) 
                                { 
                                    entranceEntry.RandomizableEntrance = true;
                                    entranceEntry.DisplayArea = TrueExitName.EndsWith(" Boss") ? "Boss Room" : "Dungeon";
                                    if (DungeonAreas.Contains(l))
                                    {
                                        entranceEntry.DisplayArea = "Dungeon Exit";
                                        entranceEntry.DisplayExit = $"{entranceEntry.Area} Exit";
                                    }
                                }

                                dictionaryFile.EntranceList.Add(FullexitName, entranceEntry);
                            }
                        }
                        foreach (var Event in LogicObject[l]?.events?.Keys?.ToList()??new List<string>())
                        {
                            string TrueMacroName = $"{Game}_EVENT_{Event}";
                            if (LogicFile.Logic.Find(x => x.Id == TrueMacroName) == null)
                            {
                                LogicFile.Logic.Add(new TrackerObjects.MMRData.JsonFormatLogicItem { Id = TrueMacroName });
                            }
                        }
                        foreach (var hint in LogicObject[l]?.gossip?.Keys?.ToList()??new List<string>())
                        {
                            string TrueHintName = $"{Game} {hint}";
                            if (LogicFile.Logic.Find(x => x.Id == TrueHintName) == null)
                            {
                                LogicFile.Logic.Add(new TrackerObjects.MMRData.JsonFormatLogicItem { Id = TrueHintName });
                            }
                            if (dictionaryFile.HintSpots.Values.FirstOrDefault(x => x.ID == TrueHintName) == null)
                            {
                                LogicDictionaryData.DictionaryHintEntries dictionaryHint = new LogicDictionaryData.DictionaryHintEntries();
                                dictionaryHint.ID = TrueHintName;
                                dictionaryHint.Name = TrueHintName;
                                dictionaryFile.HintSpots.Add(TrueHintName, dictionaryHint);
                            }
                        }
                    }
                }
            }

        }

        private static List<string> GetDungeonAreas(Dictionary<string, string> randoEntrances)
        {
            List<string> DungeonAreas = new List<string>();
            //This is hacky and I hate it
            foreach(var ent in randoEntrances)
            {
                if(ent.Key.EndsWith(" Boss")) { continue; }
                if (!DungeonAreas.Contains(ent.Value))
                {
                    DungeonAreas.Add(ent.Key);
                }
            }
            return DungeonAreas;
        }

        public static void AddEntriesFromItemPools(out TrackerObjects.MMRData.LogicFile Logic, out TrackerObjects.LogicDictionaryData.LogicDictionary dictionary)
        {
            string CodeFolder = References.TestingPaths.GetDevCodePath();
            string OOTMMCode = Path.Combine(CodeFolder, @"MMR Tracker V3", "OtherGames", "OOTMMRCOMBO");
            string MMpool = Path.Combine(References.TestingPaths.GetDevTestingPath(), @"core-develop", "data", "mm", "pool.csv");
            string OOTpool = Path.Combine(References.TestingPaths.GetDevTestingPath(), @"core-develop", "data", "oot", "pool.csv");
            string OOTMMItems = Path.Combine(OOTMMCode, @"items.json");
            string OOTMMTricks = Path.Combine(OOTMMCode, @"tricks.json");
            string OOTMMArea = Path.Combine(OOTMMCode, @"AreaNames.json");
            string OOTMMCounts = Path.Combine(OOTMMCode, @"itemCounts.json");

            Dictionary<string, string> OOTRItemsDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(OOTMMItems));
            Dictionary<string, string> OOTRTricksDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(OOTMMTricks));
            Dictionary<string, string> OOTRAreaDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(OOTMMArea));
            Dictionary<string, int> OOTRItemCounts = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(OOTMMCounts));

            string[] MMPoolWebData = File.ReadAllLines(MMpool);
            var mmPool = ConvertCsvFileToJsonObject(MMPoolWebData.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
            var mmPoolObj = JsonConvert.DeserializeObject<List<MMROOTLocation>>(mmPool);

            string[] OOTPoolWebData = File.ReadAllLines(OOTpool);
            var ootPool = ConvertCsvFileToJsonObject(OOTPoolWebData.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
            var ootPoolObj = JsonConvert.DeserializeObject<List<MMROOTLocation>>(ootPool);

            Dictionary<string, string[]> OOTRCheckDict = new Dictionary<string, string[]>();

            Dictionary<string, List<string>> HintNameData = new Dictionary<string, List<string>>();

            foreach (var i in ootPoolObj)
            {
                OOTRCheckDict.Add($"OOT {i.location}", new string[] { $"OOT_{i.scene}", $"OOT_{i.item}" });
                if (i.hint != "NONE")
                {
                    if (!HintNameData.ContainsKey($"OOT {i.hint}")) { HintNameData[$"OOT {i.hint}"] = new List<string>(); }
                    HintNameData[$"OOT {i.hint}"].Add($"OOT {i.location}");
                }
            }
            foreach (var i in mmPoolObj)
            {
                OOTRCheckDict.Add($"MM {i.location}", new string[] { $"MM_{i.scene}", $"MM_{i.item}" });
                if (i.hint != "NONE")
                {
                    if (!HintNameData.ContainsKey($"MM {i.hint}")) { HintNameData[$"MM {i.hint}"] = new List<string>(); }
                    HintNameData[$"MM {i.hint}"].Add($"MM {i.location}");
                }
            }

            Debug.WriteLine(JsonConvert.SerializeObject(HintNameData));

            MMRData.LogicFile logicFile = new()
            {
                Version = 1,
                GameCode = "OOTMM",
                Logic = new List<MMRData.JsonFormatLogicItem>()
            };

            LogicDictionaryData.LogicDictionary logicDictionary = new()
            {
                LogicVersion = 1,
                GameCode = "OOTMM",
                RootArea = "OOT SPAWN",
                LocationList = new(),
                ItemList = new()
            };

            foreach (var i in OOTRCheckDict.Keys)
            {
                string[] DictValue = OOTRCheckDict[i];
                logicFile.Logic.Add(new MMRData.JsonFormatLogicItem() { Id = i });
                LogicDictionaryData.DictionaryLocationEntries dictEntry = new LogicDictionaryData.DictionaryLocationEntries();
                dictEntry.ID = null;
                dictEntry.SpoilerData = new MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { i }, GossipHintNames = new string[] { i } };
                dictEntry.Name = i;
                dictEntry.ValidItemTypes = new string[] { "item" };
                if (OOTRAreaDict.ContainsKey(i)) { dictEntry.Area = OOTRAreaDict[i]; }
                else if (OOTRAreaDict.ContainsKey(DictValue[0])) { dictEntry.Area = OOTRAreaDict[DictValue[0]]; }
                else { dictEntry.Area = DictValue[0]; Debug.WriteLine($"{i} Had no Area Data"); }
                dictEntry.OriginalItem = DictValue[1];
                logicDictionary.LocationList.Add(i,dictEntry);
            }
            foreach(var i in OOTRItemsDict.Keys)
            {
                TrackerObjects.LogicDictionaryData.DictionaryItemEntries dictItem = new TrackerObjects.LogicDictionaryData.DictionaryItemEntries();
                string ItemName = i;
                ItemName = (string)OOTRItemsDict[i];

                dictItem.Name = ItemName;
                dictItem.MaxAmountInWorld = OOTRItemCounts.ContainsKey(i) ? OOTRItemCounts[i] : -1;
                dictItem.ValidStartingItem = true;
                dictItem.ID = null;
                dictItem.SpoilerData = new TrackerObjects.MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { i }, GossipHintNames =  new string[] { i, ItemName } };
                dictItem.ItemTypes = new string[] { "item" };
                logicDictionary.ItemList.Add(i,dictItem);
            }

            foreach(var i in OOTRTricksDict.Keys)
            {
                string TrickID = "TRICK_" + i;
                TrackerObjects.LogicDictionaryData.DictionaryMacroEntry TrickObject = new TrackerObjects.LogicDictionaryData.DictionaryMacroEntry();
                TrickObject.ID = null;
                TrickObject.Name = OOTRTricksDict[i];
                logicDictionary.MacroList.Add(TrickID, TrickObject);

                string TrickCategory = i.StartsWith("OOT_") ? "Ocarina of Time" : "Majoras Mask";
                logicFile.Logic.Add(new TrackerObjects.MMRData.JsonFormatLogicItem() { Id = TrickID, IsTrick = true, TrickCategory = TrickCategory });
            }

            Logic = logicFile;
            dictionary = logicDictionary;

        }

        public static string ConvertCsvFileToJsonObject(string[] lines)
        {
            var csv = new List<string[]>();

            var properties = lines[0].Split(',');

            foreach (string line in lines)
            {
                var LineData = line.Split(',');
                csv.Add(LineData);
            }

            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                    objResult.Add(properties[j].Trim(), csv[i][j].Trim());

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult, Testing._NewtonsoftJsonSerializerOptions);
        }

        public static void FixAreaClearLogic(MMRData.LogicFile Logic)
        {
            var MM_BOSS_GREAT_BAY = Logic.Logic.First(x => x.Id == "MM_EVENT_BOSS_GREAT_BAY");
            var MM_BOSS_SNOWHEAD = Logic.Logic.First(x => x.Id == "MM_EVENT_BOSS_SNOWHEAD");
            var MM_CLEAN_SWAMP = Logic.Logic.First(x => x.Id == "MM_EVENT_CLEAN_SWAMP");

            string CodeFolder = References.TestingPaths.GetDevCodePath();
            string OOTMMRandoAreaClear = Path.Combine(CodeFolder, @"MMR Tracker V3", "OtherGames", "OOTMMRCOMBO", @"AfterBossMacroFixing.json");
            Dictionary<string, string> RandoAreaClear = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(OOTMMRandoAreaClear));

            CreateLogic(MM_BOSS_GREAT_BAY, "MM Great Bay Temple Pre-Boss", "MM Great Bay Temple Boss");
            CreateLogic(MM_BOSS_SNOWHEAD, "MM Snowhead Temple Center Level 4", "MM Snowhead Temple Boss");
            CreateLogic(MM_CLEAN_SWAMP, "MM Woodfall Temple Pre-Boss", "MM Woodfall Temple Boss");

            void CreateLogic(MMRData.JsonFormatLogicItem Item, string DungeonArea, string BossDoor)
            {
                Item.RequiredItems = new List<string>();
                Item.ConditionalItems = new List<List<string>>();
                foreach (var i in RandoAreaClear)
                {
                    Item.ConditionalItems.Add(new() { $"contains{{{DungeonArea} => {BossDoor}, {i.Key}}}", $"check{{{i.Value}}}" });
                }
            }
        }
    }
}
