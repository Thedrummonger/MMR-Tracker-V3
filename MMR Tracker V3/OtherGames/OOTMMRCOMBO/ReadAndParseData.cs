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
            public string type;
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

            AddGameClearChecks(LogicFile, dictionaryFile);

            FinalLogicCleanup(LogicFile);

            AddWalletData(dictionaryFile);


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

        private static void AddWalletData(LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            Dictionary<int, int> PriceMap = new Dictionary<int, int>
            {
                { 1, 99 },
                { 2, 200 },
                { 3, 500 },
                { 4, 999 },
            };

            for(var i = 1; i <= 4; i++)
            {
                AddWalletMacroRef(i, "OOT");
                AddWalletMacroRef(i, "MM");
            }

            void AddWalletMacroRef(int count, string Gamecode)
            {
                dictionaryFile.MacroList.Add($"{Gamecode}_can_use_wallet_{count}", new LogicDictionaryData.DictionaryMacroEntry 
                {
                    ID = $"{Gamecode}_can_use_wallet_{count}",
                    WalletCapacity = PriceMap[count],
                    WalletCurrency = Gamecode[0]
                });
            }

        }

        private static void AddGameClearChecks(MMRData.LogicFile logicFile, LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            dictionaryFile.LocationList.Add("MM_BOSS_MAJORA", new LogicDictionaryData.DictionaryLocationEntries
            {
                ID = "MM_BOSS_MAJORA",
                Name = "Majora",
                Area = "The Moon",
                OriginalItem = "MM_MASK_MAJORA",
                ValidItemTypes = new string[] { "Majora" },
                SpoilerData = new MMRData.SpoilerlogReference()
            });
            logicFile.Logic.Add(new MMRData.JsonFormatLogicItem
            {
                Id = "MM_BOSS_MAJORA",
                RequiredItems = new List<string> { "MM_EVENT_MAJORA" }
            });

            dictionaryFile.ItemList.Add("MM_MASK_MAJORA", new LogicDictionaryData.DictionaryItemEntries
            {
                ID = "MM_MASK_MAJORA",
                Name = "Majoras Mask",
                ValidStartingItem = false,
                MaxAmountInWorld = 1,
                ItemTypes= new string[] { "Majora" },
                SpoilerData = new MMRData.SpoilerlogReference()
            });

            dictionaryFile.LocationList.Add("OOT_BOSS_GANON", new LogicDictionaryData.DictionaryLocationEntries
            {
                ID = "OOT_BOSS_GANON",
                Name = "Ganon",
                Area = "Ganon's Castle",
                OriginalItem = "OOT_TRIFORCE",
                ValidItemTypes = new string[] { "Ganon" },
                SpoilerData = new MMRData.SpoilerlogReference()
            });

            logicFile.Logic.Add(new MMRData.JsonFormatLogicItem
            {
                Id = "OOT_BOSS_GANON",
                RequiredItems = new List<string> { "OOT_EVENT_GANON" }
            });

            dictionaryFile.ItemList.Add("OOT_TRIFORCE", new LogicDictionaryData.DictionaryItemEntries
            {
                ID = "OOT_TRIFORCE",
                Name = "Triforce",
                ValidStartingItem = false,
                MaxAmountInWorld = 1,
                ItemTypes= new string[] { "Ganon" },
                SpoilerData = new MMRData.SpoilerlogReference()
            });

            //Game Clear

            string GameClearLogic =
                "(setting{goal, any} && (OOT_TRIFORCE || MM_MASK_MAJORA)) || " +
                "(setting{goal, both} && OOT_TRIFORCE && MM_MASK_MAJORA) || " +
                "(setting{goal, ganon} && OOT_TRIFORCE) || " +
                "(setting{goal, majora} && MM_MASK_MAJORA)";

            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "Game_Clear", ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicParser, GameClearLogic) });
            dictionaryFile.MacroList.Add("Game_Clear", new LogicDictionaryData.DictionaryMacroEntry { ID = "Game_Clear", Name = "Both Games Cleared" });
            dictionaryFile.WinCondition = "Game_Clear";
        }

        private static void FinalLogicCleanup(MMRData.LogicFile logicFile)
        {
            foreach(var logic in logicFile.Logic)
            {
                bool CondAllwaysTrue = false;
                foreach(var condSet in logic.ConditionalItems)
                {
                    if (condSet.Any(x => bool.TryParse(x, out bool BT) && BT) && condSet.Any(x => !bool.TryParse(x, out _)))
                    {
                        int Match = condSet.RemoveAll(x => bool.TryParse(x, out bool BR) && BR);
                    }
                    else if (condSet.All(x => bool.TryParse(x, out bool BT) && BT)) { CondAllwaysTrue = true; }
                }
                if (CondAllwaysTrue) { logic.ConditionalItems.Clear(); }

                List<List<string>> NewConditional = new List<List<string>>();
                foreach (var condSet in logic.ConditionalItems)
                {
                    List<string> NewSet = new List<string>();
                    foreach(var cond in condSet)
                    {
                        NewSet.Add(ApplyLogicReplacements(cond));
                    }
                    if (NewSet.Any()) { NewConditional.Add(NewSet); }
                }
                logic.ConditionalItems = NewConditional;
            }

            string ApplyLogicReplacements(string Cond)
            {
                if (Cond == "setting{erIkanaCastle}") { return "randomized{MM Ikana Castle Exterior => MM Ancient Castle of Ikana}"; }
                if (Cond == "setting{erIkanaCastle, false}") { return "randomized{MM Ikana Castle Exterior => MM Ancient Castle of Ikana, false}"; }
                return Cond;
            }
        }

        private static void AddRenewableChecks(MMRData.LogicFile logicFile, LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            //Add Reneawable Logic
            var OOTRenewableLocations = GetRenewableLocations("OOT");
            var MMRenewableLocations = GetRenewableLocations("MM");
            var RenewableLocations = OOTRenewableLocations.Concat(MMRenewableLocations);

            foreach(var i in RenewableLocations)
            {
                dictionaryFile.LocationList[i].Repeatable = true;
            }

            List<string> GetRenewableLocations(string GameCode)
            {
                string path = Path.Combine(References.TestingPaths.GetDevTestingPath(), "OOTMM", "OoTMM-develop", "packages", "core", "data", GameCode.ToLower(), "pool.csv");
                var Pool = ConvertCsvFileToJsonObject(File.ReadAllLines(path).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
                return JsonConvert.DeserializeObject<List<MMROOTLocation>>(Pool)
                    .Where(x => IsRenewable(x, GameCode))
                    .Select(x => $"{GameCode} {x.location}").ToList();
            }
            bool IsRenewable(MMROOTLocation x, string GameCode)
            {
                var ForceNonRenewable = new string[]
                {
                    "MM Bomb Shop Bomb Bag",
                    "MM Bomb Shop Bomb Bag 2",
                    "MM Curiosity Shop All-Night Mask",
                    "OOT Lost Woods Scrub Sticks Upgrade",
                    "OOT Lost Woods Grotto Scrub Nuts Upgrade",
                    "OOT Hyrule Field Grotto Scrub HP"
                };
                string[] RenewableTypes = new string[] { "shop", "cow", "scrub" };

                return 
                    (RenewableTypes.Contains(x.type) || (GameCode == "MM" && x.id.StartsWith("TINGLE_MAP_"))) && 
                    !ForceNonRenewable.Contains($"{GameCode} {x.location}");
            }
        }

        private static void CreateLocationProxies(LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "CanGetOathWoodfall", RequiredItems = new List<string> { "MM Woodfall Temple After Boss" } });
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "CanGetOathSnowhead", RequiredItems = new List<string> { "MM Snowhead Temple After Boss" } });
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "CanGetOathGreatBay", RequiredItems = new List<string> { "MM Great Bay Temple After Boss" } });
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "CanGetOathStoneTow", RequiredItems = new List<string> { "MM Stone Tower After Boss" } });
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "KeatonInSpring", RequiredItems = new List<string> { "MM_MASK_KEATON", "MM_EVENT_BOSS_SNOWHEAD", "MM Mountain Village" } });

            Dictionary<string, Tuple<string, string[]>> TingleLocations = new Dictionary<string, Tuple<string, string[]>>
            {
                { "Tingle Town", new ("North Clock Town", new string[]{ "Tingle Map Clock Town", "Tingle Map Woodfall" }) },
                { "Tingle Swamp", new ("Road to Southern Swamp", new string[]{ "Tingle Map Woodfall", "Tingle Map Snowhead"  }) },
                { "Tingle Mountain", new ("Twin Islands", new string[]{ "Tingle Map Snowhead", "Tingle Map Ranch" }) },
                { "Tingle Ranch", new ("Milk Road", new string[]{ "Tingle Map Ranch", "Tingle Map Great Bay" }) },
                { "Tingle Great Bay", new ("Great Bay Coast", new string[]{ "Tingle Map Great Bay", "Tingle Map Ikana" }) },
                { "Tingle Ikana", new ("Ikana Canyon", new string[]{ "Tingle Map Ikana", "Tingle Map Clock Town" }) }
            };

            foreach(var area in TingleLocations)
            {
                foreach(var map in area.Value.Item2)
                {
                    string ProxyID = $"{area.Key.Replace(" ", "_")}_{map.Replace(" ", "_")}";
                    var ProxyLogicEntry = new MMRData.JsonFormatLogicItem
                    {
                        Id = ProxyID,
                        ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicParser, ParseFunction($"MM {area.Key} && can_use_wallet(1)", "MM"))
                    };
                    LogicUtilities.RemoveRedundantConditionals(ProxyLogicEntry);
                    LogicUtilities.MakeCommonConditionalsRequirements(ProxyLogicEntry);
                    dictionaryFile.AdditionalLogic.Add(ProxyLogicEntry);
                    createLocationProxy("MM " + map, $"{ProxyID}_Proxy", map, area.Value.Item1, ProxyID);
                }
            }


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

            //Spirit exits to a different area than it enters from, temp manual fix.
            dictionaryFile.EntranceList["OOT Spirit Temple => OOT Desert Colossus Spirit Exit"].EntrancePairID = new EntranceData.EntranceAreaPair { Area= "OOT Desert Colossus", Exit = "OOT Spirit Temple" };
            dictionaryFile.EntranceList["OOT Desert Colossus => OOT Spirit Temple"].EntrancePairID = new EntranceData.EntranceAreaPair { Area= "OOT Spirit Temple", Exit = "OOT Desert Colossus Spirit Exit" };

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

            void AddCondition(string ID, string Game, string Category, string DefaultValue = null, int DefaultCount = 0)
            {
                dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem
                {
                    Id = $"{Game.ToUpper()}_HAS_{ID.ToUpper()}_REQUIREMENTS",
                    RequiredItems = new List<string> { $"{ID.ToLower()}_req, {ID.ToLower()}_count" }
                });
                foreach (var i in PossibleReqs)
                {
                    var namedata = i.Key.Split('|');

                    OptionData.TrackerOption Requirement = new OptionData.TrackerOption();
                    Requirement.ID = $"{ID.ToLower()}_{namedata[1]}";
                    Requirement.DisplayName = namedata[0];
                    Requirement.SubCategory = Category;
                    Requirement.CurrentValue = DefaultValue is null ? "false" : (namedata[0] == DefaultValue).ToString().ToLower();
                    Requirement.CreateSimpleValues(new string[] { "true", "false" });
                    Requirement.Values["true"].VariableEdit.Add($"{ID.ToLower()}_req", new OptionData.VariableEditData 
                    { 
                        action = MiscData.MathOP.add, 
                        EditValue = (namedata[1].StartsWith("fair") || namedata[1].StartsWith("skull")) ? i.Value : i.Value.Select(x => $"{x}, 1").ToArray()
                    });
                    dictionaryFile.Options.Add(Requirement.ID, Requirement);
                }
                OptionData.TrackerVar ReqVar = new OptionData.TrackerVar();
                ReqVar.Static = true;
                ReqVar.Name = $"{ID.ToLower()}_req";
                ReqVar.ID = $"{ID.ToLower()}_req";
                ReqVar.Value = new List<string>();
                dictionaryFile.Variables.Add(ReqVar.ID, ReqVar);

                OptionData.TrackerVar req_count = new OptionData.TrackerVar();
                req_count.Static = false;
                req_count.SubCategory = Category;
                req_count.Name = "Items Required";
                req_count.ID = $"{ID.ToLower()}_count";
                req_count.Value = DefaultCount;
                dictionaryFile.Variables.Add(req_count.ID, req_count);
            }

            AddCondition("moon", "mm", "Moon Access Conditions", "Boss Remains", 4);
            AddCondition("majora", "MM", "Majora Child Conditions");
            AddCondition("bridge", "oot", "Rainbow Bridge Conditions", "Medallions", 6);
            AddCondition("lacs", "oot", "Light Arrow Cutscene Conditions");
            AddCondition("ganon_bk", "oot", "Ganon Boss Key Conditions");

        }

        private static void AddVariablesandOptions(LogicDictionaryData.LogicDictionary dictionaryFile)
        {
            Dictionary<string, string> RenewableItems = new CodeFileReader<Dictionary<string, string>>().ReadCodeFile("RenewableItems");

            Dictionary<string, int> OOTRItemCounts = new CodeFileReader<Dictionary<string, int>>().ReadCodeFile("itemCounts");

            OptionData.TrackerVar MM_Masks = new OptionData.TrackerVar();
            MM_Masks.Static = true;
            MM_Masks.Value =new List<string> {
                "MM_MASK_POSTMAN",
                "MM_MASK_ALL_NIGHT",
                "MM_MASK_BLAST",
                "MM_MASK_STONE",
                "MM_MASK_GREAT_FAIRY",
                "MM_MASK_KEATON",
                "SHARED_MASK_KEATON",
                "MM_MASK_BREMEN",
                "MM_MASK_BUNNY",
                "SHARED_MASK_BUNNY",
                "MM_MASK_DON_GERO",
                "MM_MASK_SCENTS",
                "MM_MASK_ROMANI",
                "MM_MASK_TROUPE_LEADER",
                "MM_MASK_KAFEI",
                "MM_MASK_COUPLE",
                "MM_MASK_TRUTH",
                "SHARED_MASK_TRUTH",
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

            OptionData.TrackerOption itemPool = new OptionData.TrackerOption();
            itemPool.ID = "itemPool";
            itemPool.DisplayName = "Item Pool";
            itemPool.CurrentValue = "normal";
            itemPool.CreateSimpleValues(new string[] { "plentiful", "normal", "scarce", "minimal" });
            foreach(var i in OOTRItemCounts) { itemPool.Values["plentiful"].AddMaxAmountEdit(i.Key, MiscData.MathOP.set, -1); }
            dictionaryFile.Options.Add(itemPool.ID, itemPool);

            OptionData.TrackerOption goal = new OptionData.TrackerOption();
            goal.ID = "goal";
            goal.DisplayName = "Goal";
            goal.CurrentValue = "both";
            goal.CreateSimpleValues(new string[] { "any", "ganon", "majora", "both" });
            goal.Values["any"].Name = "Any Final Boss";
            goal.Values["ganon"].Name = "Ganon";
            goal.Values["majora"].Name = "Majora";
            goal.Values["both"].Name = "Ganon & Majora";
            dictionaryFile.Options.Add(goal.ID, goal);

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
            DoorOfTime.SubCategory = "Events";
            DoorOfTime.CreateSimpleValues(new string[] { "open", "closed" });
            DoorOfTime.Values["open"].Name = "Open";
            DoorOfTime.Values["closed"].Name = "Closed";
            dictionaryFile.Options.Add(DoorOfTime.ID,DoorOfTime);

            OptionData.TrackerOption dekuTree = new OptionData.TrackerOption();
            dekuTree.ID = "dekuTree";
            dekuTree.DisplayName = "Open Deku Tree";
            dekuTree.CurrentValue = "open";
            dekuTree.SubCategory = "Events";
            dekuTree.CreateSimpleValues(new string[] { "open", "closed" });
            dekuTree.Values["open"].Name = "Open";
            dekuTree.Values["closed"].Name = "Closed";
            dictionaryFile.Options.Add(dekuTree.ID, dekuTree);

            OptionData.TrackerOption dekuTreeAdult = new OptionData.TrackerOption();
            dekuTreeAdult.ID = "dekuTreeAdult";
            dekuTreeAdult.DisplayName = "Deku Tree as Adult";
            dekuTreeAdult.CurrentValue = "false";
            dekuTreeAdult.SubCategory = "Events";
            dekuTreeAdult.CreateSimpleValues(new string[] { "true", "false" });
            dictionaryFile.Options.Add(dekuTreeAdult.ID, dekuTreeAdult);

            OptionData.TrackerOption wellAdult = new OptionData.TrackerOption();
            wellAdult.ID = "wellAdult";
            wellAdult.DisplayName = "Well as Adult";
            wellAdult.CurrentValue = "false";
            wellAdult.SubCategory = "Events";
            wellAdult.CreateSimpleValues(new string[] { "true", "false" });
            dictionaryFile.Options.Add(wellAdult.ID, wellAdult);

            OptionData.TrackerOption fireChild = new OptionData.TrackerOption();
            fireChild.ID = "fireChild";
            fireChild.DisplayName = "Fire Temple as Child";
            fireChild.CurrentValue = "false";
            fireChild.SubCategory = "Events";
            fireChild.CreateSimpleValues(new string[] { "true", "false" });
            dictionaryFile.Options.Add(fireChild.ID, fireChild);

            OptionData.TrackerOption kakarikoGate = new OptionData.TrackerOption();
            kakarikoGate.ID = "kakarikoGate";
            kakarikoGate.DisplayName = "Open Kakariko Gate";
            kakarikoGate.CurrentValue = "closed";
            kakarikoGate.SubCategory = "Events";
            kakarikoGate.CreateSimpleValues(new string[] { "open", "closed" });
            kakarikoGate.Values["open"].Name = "Open";
            kakarikoGate.Values["closed"].Name = "Closed";
            dictionaryFile.Options.Add(kakarikoGate.ID, kakarikoGate);

            OptionData.TrackerOption zoraKing = new OptionData.TrackerOption();
            zoraKing.ID = "zoraKing";
            zoraKing.DisplayName = "Zora King";
            zoraKing.CurrentValue = "vanilla";
            zoraKing.SubCategory = "Events";
            zoraKing.CreateSimpleValues(new string[] { "vanilla", "adult", "open" });
            zoraKing.Values["vanilla"].Name = "Vanilla";
            zoraKing.Values["adult"].Name = "Open (Adult Only)";
            zoraKing.Values["open"].Name = "Open";
            dictionaryFile.Options.Add(zoraKing.ID, zoraKing);

            OptionData.TrackerOption gerudoFortress = new OptionData.TrackerOption();
            gerudoFortress.ID = "gerudoFortress";
            gerudoFortress.DisplayName = "Gerudo Fortress";
            gerudoFortress.CurrentValue = "vanilla";
            gerudoFortress.SubCategory = "Events";
            gerudoFortress.CreateSimpleValues(new string[] { "vanilla", "single", "open" });
            gerudoFortress.Values["vanilla"].Name = "Vanilla";
            gerudoFortress.Values["single"].Name = "One Carpenter";
            gerudoFortress.Values["open"].Name = "Open";
            dictionaryFile.Options.Add(gerudoFortress.ID, gerudoFortress);

            OptionData.TrackerOption skipZelda = new OptionData.TrackerOption();
            skipZelda.ID = "skipZelda";
            skipZelda.DisplayName = "Skip Child Zelda";
            skipZelda.CurrentValue = "false";
            skipZelda.SubCategory = "Events";
            skipZelda.CreateSimpleValues(new string[] { "true", "false" });
            dictionaryFile.Options.Add(skipZelda.ID, skipZelda);

            OptionData.TrackerOption lacs = new OptionData.TrackerOption();
            lacs.ID = "lacs";
            lacs.DisplayName = "Light Arrow Cutscene";
            lacs.CurrentValue = "vanilla";
            lacs.SubCategory = "Events";
            lacs.CreateSimpleValues(new string[] { "vanilla", "custom" });
            lacs.Values["vanilla"].Name = "Vanilla";
            lacs.Values["custom"].Name = "Custom";
            dictionaryFile.Options.Add(lacs.ID, lacs);

            OptionData.TrackerOption majoraChild = new OptionData.TrackerOption();
            majoraChild.ID = "majoraChild";
            majoraChild.DisplayName = "Majora Child Requirements";
            majoraChild.CurrentValue = "none";
            majoraChild.SubCategory = "Events";
            majoraChild.CreateSimpleValues(new string[] { "none", "custom" });
            majoraChild.Values["none"].Name = "None";
            majoraChild.Values["custom"].Name = "Custom";
            dictionaryFile.Options.Add(majoraChild.ID, majoraChild);

            OptionData.TrackerOption bossWarpPads = new OptionData.TrackerOption();
            bossWarpPads.ID = "bossWarpPads";
            bossWarpPads.DisplayName = "Boss Warp Pads";
            bossWarpPads.CurrentValue = "bossBeaten";
            bossWarpPads.SubCategory = "Events";
            bossWarpPads.CreateSimpleValues(new string[] { "bossBeaten", "remains" });
            bossWarpPads.Values["bossBeaten"].Name = "Boss Beaten";
            bossWarpPads.Values["remains"].Name = "Remains";
            dictionaryFile.Options.Add(bossWarpPads.ID, bossWarpPads);

            OptionData.TrackerOption CrossGameOOTWarpSong = new OptionData.TrackerOption();
            CrossGameOOTWarpSong.ID = "crossWarpOot";
            CrossGameOOTWarpSong.DisplayName = "Cross-Games OoT Warp Songs";
            CrossGameOOTWarpSong.CurrentValue = "false";
            CrossGameOOTWarpSong.SubCategory = "Cross Game";
            CrossGameOOTWarpSong.CreateSimpleValues(new string[] { "true", "false" });
            dictionaryFile.Options.Add(CrossGameOOTWarpSong.ID, CrossGameOOTWarpSong);

            OptionData.TrackerOption CrossGameMMWarpSong = new OptionData.TrackerOption();
            CrossGameMMWarpSong.ID = "crossWarpMm";
            CrossGameMMWarpSong.DisplayName = "Cross-Games MM Song of Soaring";
            CrossGameMMWarpSong.CurrentValue = "none";
            CrossGameMMWarpSong.SubCategory = "Cross Game";
            CrossGameMMWarpSong.CreateSimpleValues(new string[] { "none", "childOnly", "full" });
            CrossGameMMWarpSong.Values["none"].Name = "None";
            CrossGameMMWarpSong.Values["childOnly"].Name = "Child Only";
            CrossGameMMWarpSong.Values["full"].Name = "Child & Adult";
            dictionaryFile.Options.Add(CrossGameMMWarpSong.ID, CrossGameMMWarpSong);

            OptionData.TrackerOption GanonBossKey = new OptionData.TrackerOption();
            GanonBossKey.ID = "ganonBossKey";
            GanonBossKey.DisplayName = "Ganon's Boss Key";
            GanonBossKey.CurrentValue = "removed";
            GanonBossKey.SubCategory = "Key Shuffle";
            GanonBossKey.CreateSimpleValues(new string[] { "removed", "vanilla", "ganon", "anywhere", "custom" });
            GanonBossKey.Values["removed"].Name = "Removed";
            GanonBossKey.Values["vanilla"].Name = "Vanilla";
            GanonBossKey.Values["ganon"].Name = "Ganon's Castle";
            GanonBossKey.Values["anywhere"].Name = "Anywhere";
            GanonBossKey.Values["custom"].Name = "Custom";
            GanonBossKey.Values["removed"].AddMaxAmountEdit("OOT_BOSS_KEY_GANON", MiscData.MathOP.set, 0);
            dictionaryFile.Options.Add(GanonBossKey.ID, GanonBossKey);

            OptionData.TrackerOption SmallKeyOot = new OptionData.TrackerOption();
            SmallKeyOot.ID = "smallKeyShuffleOot";
            SmallKeyOot.DisplayName = "Small Key Shuffle OOT";
            SmallKeyOot.CurrentValue = "ownDungeon";
            SmallKeyOot.SubCategory = "Key Shuffle";
            SmallKeyOot.CreateSimpleValues(new string[] { "ownDungeon", "anywhere", "removed" });
            SmallKeyOot.Values["ownDungeon"].Name = "Own Dungeon";
            SmallKeyOot.Values["anywhere"].Name = "Anywhere";
            SmallKeyOot.Values["removed"].Name = "Removed";
            SmallKeyOot.Values["ownDungeon"].AddMaxAmountEdit("OOT_SMALL_KEY_FIRE", MiscData.MathOP.subtract, 1);
            dictionaryFile.Options.Add(SmallKeyOot.ID, SmallKeyOot);

            OptionData.TrackerOption SmallKeyMM = new OptionData.TrackerOption();
            SmallKeyOot.ID = "smallKeyShuffleMm";
            SmallKeyOot.DisplayName = "Small Key Shuffle MM";
            SmallKeyOot.CurrentValue = "ownDungeon";
            SmallKeyOot.SubCategory = "Key Shuffle";
            SmallKeyOot.CreateSimpleValues(new string[] { "ownDungeon", "anywhere", "removed" });
            SmallKeyOot.Values["ownDungeon"].Name = "Own Dungeon";
            SmallKeyOot.Values["anywhere"].Name = "Anywhere";
            SmallKeyOot.Values["removed"].Name = "Removed";
            dictionaryFile.Options.Add(SmallKeyOot.ID, SmallKeyOot);

            OptionData.TrackerOption BossKeyOot = new OptionData.TrackerOption();
            BossKeyOot.ID = "bossKeyShuffleOot";
            BossKeyOot.DisplayName = "Boss Key Shuffle OOT";
            BossKeyOot.CurrentValue = "ownDungeon";
            BossKeyOot.SubCategory = "Key Shuffle";
            BossKeyOot.CreateSimpleValues(new string[] { "ownDungeon", "anywhere", "removed" });
            BossKeyOot.Values["ownDungeon"].Name = "Own Dungeon";
            BossKeyOot.Values["anywhere"].Name = "Anywhere";
            BossKeyOot.Values["removed"].Name = "Removed";
            BossKeyOot.Values["ownDungeon"].AddMaxAmountEdit("OOT_SMALL_KEY_FIRE", MiscData.MathOP.subtract, 1);
            dictionaryFile.Options.Add(BossKeyOot.ID, BossKeyOot);

            OptionData.TrackerOption BossKeyMM = new OptionData.TrackerOption();
            BossKeyMM.ID = "bossKeyShuffleMm";
            BossKeyMM.DisplayName = "Boss Key Shuffle MM";
            BossKeyMM.CurrentValue = "ownDungeon";
            BossKeyMM.SubCategory = "Key Shuffle";
            BossKeyMM.CreateSimpleValues(new string[] { "ownDungeon", "anywhere", "removed" });
            BossKeyMM.Values["ownDungeon"].Name = "Own Dungeon";
            BossKeyMM.Values["anywhere"].Name = "Anywhere";
            BossKeyMM.Values["removed"].Name = "Removed";
            dictionaryFile.Options.Add(BossKeyMM.ID, BossKeyMM);

            OptionData.TrackerOption agelessSwords = new OptionData.TrackerOption();
            agelessSwords.ID = "agelessSwords";
            agelessSwords.DisplayName = "Ageless Swords";
            agelessSwords.CurrentValue = "false";
            agelessSwords.SubCategory = "Ageless Items";
            agelessSwords.CreateSimpleValues(new string[] { "true", "false" });
            dictionaryFile.Options.Add(agelessSwords.ID, agelessSwords);

            OptionData.TrackerOption agelessShields = new OptionData.TrackerOption();
            agelessShields.ID = "agelessShields";
            agelessShields.DisplayName = "Ageless Shields";
            agelessShields.CurrentValue = "false";
            agelessShields.SubCategory = "Ageless Items";
            agelessShields.CreateSimpleValues(new string[] { "true", "false" });
            dictionaryFile.Options.Add(agelessShields.ID, agelessShields);

            OptionData.TrackerOption agelessTunics = new OptionData.TrackerOption();
            agelessTunics.ID = "agelessTunics";
            agelessTunics.DisplayName = "Ageless Tunics";
            agelessTunics.CurrentValue = "false";
            agelessTunics.SubCategory = "Ageless Items";
            agelessTunics.CreateSimpleValues(new string[] { "true", "false" });
            dictionaryFile.Options.Add(agelessTunics.ID, agelessTunics);

            OptionData.TrackerOption agelessBoots = new OptionData.TrackerOption();
            agelessBoots.ID = "agelessBoots";
            agelessBoots.DisplayName = "Ageless Boots";
            agelessBoots.CurrentValue = "false";
            agelessBoots.SubCategory = "Ageless Items";
            agelessBoots.CreateSimpleValues(new string[] { "true", "false" });
            dictionaryFile.Options.Add(agelessBoots.ID, agelessBoots);

            OptionData.TrackerOption agelessSticks = new OptionData.TrackerOption();
            agelessSticks.ID = "agelessSticks";
            agelessSticks.DisplayName = "Ageless Sticks";
            agelessSticks.CurrentValue = "false";
            agelessSticks.SubCategory = "Ageless Items";
            agelessSticks.CreateSimpleValues(new string[] { "true", "false" });
            dictionaryFile.Options.Add(agelessSticks.ID, agelessSticks);

            OptionData.TrackerOption agelessBoomerang = new OptionData.TrackerOption();
            agelessBoomerang.ID = "agelessBoomerang";
            agelessBoomerang.DisplayName = "Ageless Boomerang";
            agelessBoomerang.CurrentValue = "false";
            agelessBoomerang.SubCategory = "Ageless Items";
            agelessBoomerang.CreateSimpleValues(new string[] { "true", "false" });
            dictionaryFile.Options.Add(agelessBoomerang.ID, agelessBoomerang);

            OptionData.TrackerOption agelessHammer = new OptionData.TrackerOption();
            agelessHammer.ID = "agelessHammer";
            agelessHammer.DisplayName = "Ageless Hammer";
            agelessHammer.CurrentValue = "false";
            agelessHammer.SubCategory = "Ageless Items";
            agelessHammer.CreateSimpleValues(new string[] { "true", "false" });
            dictionaryFile.Options.Add(agelessHammer.ID, agelessHammer);

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
            ProgressiveShieldsOOT.Values["separate"].Name = "Separate";
            ProgressiveShieldsOOT.Values["progressive"].Name = "Progressive";
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
            ProgressiveShieldsMM.CurrentValue = "separate";
            ProgressiveShieldsMM.SubCategory = "Progressive Items";
            ProgressiveShieldsMM.CreateSimpleValues(new string[] { "separate", "progressive" });
            ProgressiveShieldsMM.Values["separate"].AddMaxAmountEdit("MM_SHIELD", MiscData.MathOP.set, 0);
            ProgressiveShieldsMM.Values["progressive"].AddMaxAmountEdit("MM_SHIELD_MIRROR", MiscData.MathOP.set, 0);
            ProgressiveShieldsMM.Values["separate"].Name = "Separate";
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
            ProgressiveLullabyMM.Values["single"].Name = "Single";
            ProgressiveLullabyMM.Values["progressive"].Name = "Progressive";
            dictionaryFile.Options.Add(ProgressiveLullabyMM.ID, ProgressiveLullabyMM);

            OptionData.TrackerOption fairyOcarinaMm = new OptionData.TrackerOption();
            fairyOcarinaMm.ID = "fairyOcarinaMm";
            fairyOcarinaMm.DisplayName = "Fairy Ocarina in MM";
            fairyOcarinaMm.CurrentValue = "false";
            fairyOcarinaMm.SubCategory = "Item Extensions";
            fairyOcarinaMm.CreateSimpleValues(new string[] { "true", "false" });
            fairyOcarinaMm.Values["false"].AddMaxAmountEdit("MM_OCARINA", MiscData.MathOP.subtract, 1);
            dictionaryFile.Options.Add(fairyOcarinaMm.ID, fairyOcarinaMm);

            OptionData.TrackerOption shortHookshotMm = new OptionData.TrackerOption();
            shortHookshotMm.ID = "shortHookshotMm";
            shortHookshotMm.DisplayName = "Short Hookshot in MM";
            shortHookshotMm.CurrentValue = "false";
            shortHookshotMm.SubCategory = "Item Extensions";
            shortHookshotMm.CreateSimpleValues(new string[] { "true", "false" });
            shortHookshotMm.Values["false"].AddMaxAmountEdit("MM_HOOKSHOT", MiscData.MathOP.subtract, 1);
            dictionaryFile.Options.Add(shortHookshotMm.ID, shortHookshotMm);

            OptionData.TrackerOption sunSongMm = new OptionData.TrackerOption();
            sunSongMm.ID = "sunSongMm";
            sunSongMm.DisplayName = "Sun's Song in MM";
            sunSongMm.CurrentValue = "false";
            sunSongMm.SubCategory = "Item Extensions";
            sunSongMm.CreateSimpleValues(new string[] { "true", "false" });
            sunSongMm.Values["false"].AddMaxAmountEdit("MM_SONG_SUN", MiscData.MathOP.set, 0);
            sunSongMm.Values["true"].ItemNameOverride.Add("OOT_SONG_SUN", "Sun Song (OoT)");
            dictionaryFile.Options.Add(sunSongMm.ID, sunSongMm);

            OptionData.TrackerOption childWallets = new OptionData.TrackerOption();
            childWallets.ID = "childWallets";
            childWallets.DisplayName = "Child Wallets";
            childWallets.CurrentValue = "false";
            childWallets.SubCategory = "Item Extensions";
            childWallets.CreateSimpleValues(new string[] { "true", "false" });
            childWallets.Values["true"].AddMaxAmountEdit("OOT_WALLET", MiscData.MathOP.add, 1);
            childWallets.Values["true"].AddMaxAmountEdit("MM_WALLET", MiscData.MathOP.add, 1);
            childWallets.Values["true"].AddMaxAmountEdit("SHARED_WALLET", MiscData.MathOP.add, 1);
            dictionaryFile.Options.Add(childWallets.ID, childWallets);

            OptionData.TrackerOption colossalWallets = new OptionData.TrackerOption();
            colossalWallets.ID = "colossalWallets";
            colossalWallets.DisplayName = "Colossal Wallets";
            colossalWallets.CurrentValue = "false";
            colossalWallets.SubCategory = "Item Extensions";
            colossalWallets.CreateSimpleValues(new string[] { "true", "false" });
            colossalWallets.Values["true"].AddMaxAmountEdit("OOT_WALLET", MiscData.MathOP.add, 1);
            colossalWallets.Values["true"].AddMaxAmountEdit("MM_WALLET", MiscData.MathOP.add, 1);
            colossalWallets.Values["true"].AddMaxAmountEdit("SHARED_WALLET", MiscData.MathOP.add, 1);
            dictionaryFile.Options.Add(colossalWallets.ID, colossalWallets);

            //Temp Workaround for some typos in logic
            dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = "MM_PICTORGRAPH_BOX", RequiredItems = new List<string> { "MM_PICTOGRAPH_BOX" } });

            AddSharedItemOptions("sharedNutsSticks", "Shared Nuts and Sticks", new string[] { "NUT", "NUTS_5", "NUTS_5_ALT", "NUTS_10", "STICK", "STICKS_5", "STICKS_10" }, 1);
            AddSharedItemOptions("sharedBows", "Shared Bows", new string[] { "ARROWS_5", "ARROWS_10", "ARROWS_30", "ARROWS_40", "BOW" }, 1);
            AddSharedItemOptions("sharedBombBags", "Shared Bomb Bags", new string[] { "BOMBS_5", "BOMBS_10", "BOMBS_20", "BOMBS_30", "BOMB", "BOMB_BAG" }, 1);
            AddSharedItemOptions("sharedMagic", "Shared Magic", new string[] { "MAGIC_UPGRADE" }, 1);
            AddSharedItemOptions("sharedMagicArrowFire", "Shared Fire Arrow", new string[] { "ARROW_FIRE" }, 1);
            AddSharedItemOptions("sharedMagicArrowIce", "Shared Ice Arrow", new string[] { "ARROW_ICE"}, 1);
            AddSharedItemOptions("sharedMagicArrowLight", "Shared Light Arrow", new string[] { "ARROW_LIGHT" }, 1);
            AddSharedItemOptions("sharedSongEpona", "Shared Epona's Song", new string[] { "SONG_EPONA" }, 1);
            AddSharedItemOptions("sharedSongStorms", "Shared Song of Storms", new string[] { "SONG_STORMS" }, 1);
            AddSharedItemOptions("sharedSongTime", "Shared Song of Time", new string[] { "SONG_TIME" }, 1);
            AddSharedItemOptions("sharedSongSun", "Shared Sun's Song", new string[] { "SONG_SUN" }, 1);
            AddSharedItemOptions("sharedHookshot", "Shared Hookshots", new string[] { "HOOKSHOT" }, 2);
            AddSharedItemOptions("sharedLens", "Shared Lens of Truth", new string[] { "LENS" }, 1);
            AddSharedItemOptions("sharedOcarina", "Shared Ocarina", new string[] { "OCARINA" }, 2);
            AddSharedItemOptions("sharedMasks", "Shared Masks", new string[] { "MASK_ZORA", "MASK_GORON", "MASK_TRUTH", "MASK_BUNNY", "MASK_KEATON" }, 1);
            AddSharedItemOptions("sharedWallets", "Shared Wallets", new string[] { "WALLET" }, 4, new string[] { "RUPEE_GREEN", "RUPEE_RED", "RUPEE_PURPLE", "RUPEE_SILVER", "RUPEE_GOLD", "RUPEE_HUGE" }); //"RUPEE_BLUE" is bugged and still appears if shared
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
                if (Key != null) 
                {
                    int KeyDiff = MaxKeys - ((int)dictionaryFile.ItemList[Key].MaxAmountInWorld);
                    MiscData.MathOP Op = KeyDiff < 0 ? MiscData.MathOP.subtract : MiscData.MathOP.add;
                    MQEntry.Values["true"].AddMaxAmountEdit(Key, Op, Math.Abs(KeyDiff)); 
                }
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
                    if (RenewableItems.ContainsKey(Item))
                    {
                        OnActionReplacementData.ReplacementList.Add($"renewable{{OOT_{Item}}}", $"renewable{{SHARED_{Item}}}");
                        OnActionReplacementData.ReplacementList.Add($"renewable{{MM_{Item}}}", $"renewable{{SHARED_{Item}}}");
                    }
                    if (LogicalAmount > 1)
                    {
                        for (var i = 2; i <= LogicalAmount; i++)
                        {
                            OnActionReplacementData.ReplacementList.Add($"OOT_{Item}, {i}", $"SHARED_{Item}, {i}");
                            OnActionReplacementData.ReplacementList.Add($"MM_{Item}, {i}", $"SHARED_{Item}, {i}");
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
            string MMLogic = Path.Combine(References.TestingPaths.GetDevTestingPath(), "OOTMM", "OoTMM-develop", "packages", "core", "data", "mm", "world");
            string OOTLogic = Path.Combine(References.TestingPaths.GetDevTestingPath(), "OOTMM", "OoTMM-develop", "packages", "core", "data", "oot", "world");
            string OOTMQLogic = Path.Combine(References.TestingPaths.GetDevTestingPath(), "OOTMM", "OoTMM-develop", "packages", "core", "data", "oot", "world_mq");
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
                if (VariableDungeon && !string.IsNullOrWhiteSpace(DungeonCode)) { ReturnLogic += $" && (option{{{DungeonCode}MQ, {MQ.ToString().ToLower()}}})"; }
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
            Dictionary<string, string> RenewableItems = new CodeFileReader<Dictionary<string, string>>().ReadCodeFile("RenewableItems");

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
                    case "license":
                        line = line.Replace(FullFunction, $"true");
                        break;
                    case "special":
                        line = line.Replace(FullFunction, $"{Game}_HAS_{Parameters}_REQUIREMENTS");
                        break;
                    case "trick":
                        line = line.Replace(FullFunction, $"trick{{TRICK_{Game}_{Parameters}}}");
                        break;
                    case "renewable":
                        line = line.Replace(FullFunction, $"{Function}{{{Parameters}}}");
                        break;
                    case "has":
                        string ReplaceText = $"{Game}_{Parameters}";
                        int Amount = 1;
                        if (ReplaceText.Contains(", "))
                        {
                            Amount = Convert.ToInt32(ReplaceText.Split(",")[1].Trim());
                            ReplaceText = ReplaceText.Split(",")[0].Trim();
                        }
                        //This might break something? but now with child wallets ammount can actually be 0 in logic meaning it's not needed
                        //I feel like at one point if the amount was zero it was a bug and should just be one?
                        if (Amount > 1) { ReplaceText += $", {Amount}"; }
                        else if (Amount == 0) { ReplaceText = "true"; }
                        line = line.Replace(FullFunction, ReplaceText);
                        break;
                    case "event":
                        string TempGame = Game;
                        string TempParam = Parameters;
                        if (Parameters.StartsWith("OOT_") || Parameters.StartsWith("MM_"))
                        {
                            TempGame = Parameters.Split('_')[0];
                            TempParam = Parameters.Split('_')[1];
                        }
                        line = line.Replace(FullFunction, $"{TempGame}_EVENT_{TempParam}");
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
                        line = line.Replace(FullFunction, $"(option{{smallKeyShuffleOot, anywhere}} && has(SMALL_KEY_FIRE, {Keys + 1})) || (option{{smallKeyShuffleOot, anywhere, false}} && has(SMALL_KEY_FIRE, {Keys}))");
                        break;
                    case "setting":
                        line = line.Replace(FullFunction, $"{Function}{{{Parameters}}}");
                        break;
                    case "!setting":
                        line = line.Replace(FullFunction, $"{Function.Replace("!","")}{{{Parameters},false}}");
                        break;
                    case "age":
                        if (Parameters == "child") { line = line.Replace(FullFunction, $"option{{age_filter, adult, false}}"); }
                        else if (Parameters == "adult") { line = line.Replace(FullFunction, $"option{{age_filter, child, false}} && OOT_EVENT_TIME_TRAVEL"); }
                        break;
                    case "before":
                        line = line.Replace(FullFunction, $"time{{Before_{Parameters}}}");
                        break;
                    case "after":
                        line = line.Replace(FullFunction, $"time{{After_{Parameters}}}");
                        break;
                    case "at":
                        line = line.Replace(FullFunction, $"time{{At_{Parameters}}}");
                        break;
                    case "has_wallet":
                        int WalletAmmount = int.Parse(Parameters);
                        line = line.Replace(FullFunction, $"((option{{childWallets, true}} && has(WALLET, {WalletAmmount})) || (option{{childWallets, false}} && has(WALLET, {WalletAmmount-1})))");
                        break;
                    case "can_use_wallet":
                        //line = line.Replace(FullFunction, $"event(RUPEES) && has_wallet({Parameters})");
                        line = line.Replace(FullFunction, $"{Game}_can_use_wallet_{Parameters}");
                        break;
                    case "cond":
                        Debug.WriteLine($"Parsing Cond\n{FullFunction}\n{Parameters}");
                        var splitArray = Regex.Split(Parameters, @",\s*(?![^()]*\))");
                        string replaceLine = "ErrorCond";
                        if (splitArray[0].StartsWith("setting") || splitArray[0].StartsWith("trick"))
                        {
                            replaceLine = $"(({splitArray[0]} && {splitArray[1]}) || ({splitArray[2]} && {splitArray[0].TrimEnd(')')}, false)))";
                        }
                        else if (splitArray[0] == "is_adult")
                        {
                            replaceLine = $"(({splitArray[0]} && {splitArray[1]}) || ({splitArray[2]} && is_child))";
                        }
                        else
                        {
                            replaceLine = $"(({splitArray[0]} && {splitArray[1]}) || {splitArray[2]})";
                        }
                        Debug.WriteLine($"New Cond\n{replaceLine}");
                        line = line.Replace(FullFunction, replaceLine);
                        break;
                    case "boss_key":
                        line = line.Replace(FullFunction, $"setting(bossKeyShuffle{Game[..1] + Game[1..].ToLower()}, removed) || has({Parameters})");
                        break;
                    case "small_keys":
                        line = line.Replace(FullFunction, $"setting(smallKeyShuffle{Game[..1] + Game[1..].ToLower()}, removed) || has({Parameters})");
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
            string MMOOTCodeMM = Path.Combine(References.TestingPaths.GetDevTestingPath(), "OOTMM", "OoTMM-develop", "packages", "core", @"data", @"mm");
            string MMOOTCodeOOT = Path.Combine(References.TestingPaths.GetDevTestingPath(), "OOTMM", "OoTMM-develop", "packages", "core", @"data", @"oot");
            string MMOOTCodeMMWorld = Path.Combine(MMOOTCodeMM, @"world");
            string MMOOTCodeOOTWorld = Path.Combine(MMOOTCodeOOT, @"world");
            string MMOOTCodeOOTMQWorld = Path.Combine(MMOOTCodeOOT, @"world_mq");
            string[] MMOOTCodeMMWorldFiles = Directory.GetFiles(MMOOTCodeMMWorld);
            string[] MMOOTCodeOOTWorldFiles = Directory.GetFiles(MMOOTCodeOOTWorld);
            string[] MMOOTCodeOOTMQWorldFiles = Directory.GetFiles(MMOOTCodeOOTMQWorld);
            string MMEntrances = Path.Combine(MMOOTCodeMM, @"entrances.csv");
            string OOTEntrances = Path.Combine(MMOOTCodeOOT, @"entrances.csv");


            List<MMROOTEntranceData> MMentranceData = JsonConvert.DeserializeObject<List<MMROOTEntranceData>>(ConvertCsvFileToJsonObject(File.ReadAllLines(MMEntrances)));
            List<MMROOTEntranceData> OOTentranceData = JsonConvert.DeserializeObject<List<MMROOTEntranceData>>(ConvertCsvFileToJsonObject(File.ReadAllLines(OOTEntrances)));

            List<MMROOTEntranceData> RandoEntrances = MMentranceData.Concat(OOTentranceData).ToList();

            Dictionary<string, string> MMMacros = new CodeFileReader<Dictionary<string, string>>().ReadCodeFile(@"MMMacroOverride");
            Dictionary<string, string> OOTMacros = new CodeFileReader<Dictionary<string, string>>().ReadCodeFile(@"OOTMacroOverride");

            Dictionary<string, MMROOTLogicEntry> MMLogicEntries = new Dictionary<string, MMROOTLogicEntry>();
            Dictionary<string, MMROOTLogicEntry> OOTLogicEntries = new Dictionary<string, MMROOTLogicEntry>();

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
                    foreach (var Area in LogicObject.Keys)
                    {
                        string TrueAreaName = $"{Game} {Area}";
                        foreach (var exit in LogicObject[Area]?.exits?.Keys?.ToList()??new List<string>())
                        {
                            string TrueExitName = exit.StartsWith($"{OpositeGame} ") || exit.StartsWith($"{Game} ") ? $"{exit}" : $"{Game} {exit}";
                            if (TrueExitName == "OOT MM SPAWN") { TrueExitName = "MM SPAWN"; }
                            string FullexitName = $"{TrueAreaName} => {TrueExitName}";

                            if (!LogicFile.Logic.Any(x => x.Id == FullexitName))
                            {
                                MMRData.JsonFormatLogicItem EntranceEntry = new();
                                EntranceEntry.Id = FullexitName;
                                LogicFile.Logic.Add(EntranceEntry);
                            }

                            if (!dictionaryFile.EntranceList.Values.Any(x => x.ID == FullexitName))
                            {
                                LogicDictionaryData.DictionaryEntranceEntries entranceEntry = new()
                                {
                                    ID = FullexitName,
                                    RandomizableEntrance = false,
                                    Area = TrueAreaName,
                                    Exit = TrueExitName,
                                    AlwaysAccessable = false
                                };
                                if (FullexitName.StartsWith("OOT SPAWN => ")) { entranceEntry.AlwaysAccessable = true; }
                                if (RandoEntrances.Any(x => x.to == exit && x.from == Area)) 
                                {
                                    var EntranceCSVentry = RandoEntrances.First(x => x.to == exit && x.from == Area);
                                    bool IsBossDoor = EntranceCSVentry.type == "boss";
                                    entranceEntry.RandomizableEntrance = true;
                                }

                                dictionaryFile.EntranceList.Add(FullexitName, entranceEntry);
                            }
                        }
                        foreach (var Event in LogicObject[Area]?.events?.Keys?.ToList()??new List<string>())
                        {
                            string TrueMacroName = $"{Game}_EVENT_{Event}";
                            if (!LogicFile.Logic.Any(x => x.Id == TrueMacroName))
                            {
                                LogicFile.Logic.Add(new TrackerObjects.MMRData.JsonFormatLogicItem { Id = TrueMacroName });
                            }
                        }
                        foreach (var hint in LogicObject[Area]?.gossip?.Keys?.ToList()??new List<string>())
                        {
                            string TrueHintName = $"{Game} {hint}";
                            if (!LogicFile.Logic.Any(x => x.Id == TrueHintName))
                            {
                                LogicFile.Logic.Add(new TrackerObjects.MMRData.JsonFormatLogicItem { Id = TrueHintName });
                            }
                            if (!dictionaryFile.HintSpots.Values.Any(x => x.ID == TrueHintName))
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

        public static void AddEntriesFromItemPools(out TrackerObjects.MMRData.LogicFile Logic, out TrackerObjects.LogicDictionaryData.LogicDictionary dictionary)
        {
            Dictionary<string, string> OOTRItemsDict = new CodeFileReader<Dictionary<string, string>>().ReadCodeFile("items");
            Dictionary<string, string> OOTRTricksDict = new CodeFileReader<Dictionary<string, string>>().ReadCodeFile("tricks");
            Dictionary<string, string> OOTRAreaDict = new CodeFileReader<Dictionary<string, string>>().ReadCodeFile("AreaNames");
            Dictionary<string, int> OOTRItemCounts = new CodeFileReader<Dictionary<string, int>>().ReadCodeFile("itemCounts");

            string MMpool = Path.Combine(References.TestingPaths.GetDevTestingPath(), "OOTMM", "OoTMM-develop", "packages", "core", "data", "mm", "pool.csv");
            string[] MMPoolWebData = File.ReadAllLines(MMpool);
            var mmPool = ConvertCsvFileToJsonObject(MMPoolWebData.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
            var mmPoolObj = JsonConvert.DeserializeObject<List<MMROOTLocation>>(mmPool);

            string OOTpool = Path.Combine(References.TestingPaths.GetDevTestingPath(), "OOTMM", "OoTMM-develop", "packages", "core", "data", "oot", "pool.csv");
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
                dictEntry.WalletCurrency = i[0];
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
                var SpoilerNames = GetItemSpoilerNames(i, ItemName);
                dictItem.SpoilerData = new TrackerObjects.MMRData.SpoilerlogReference { SpoilerLogNames = SpoilerNames, GossipHintNames = SpoilerNames };
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

            string[] GetItemSpoilerNames(string ID, string Name)
            {
                List<string> names = new List<string>() { ID, Name };
                string Game = ID.StartsWith("OOT_") ? "OoT" : "MM";
                if (!Name.EndsWith(')') && !ID.StartsWith("SHARED_")) { names.Add($"{Name} ({Game})"); }
                return names.ToArray();
            }

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

        public class CodeFileReader<T>
        {
            public T ReadCodeFile(string FileName)
            {
                string Name;
                string Extention = ".json";
                if (Path.HasExtension(FileName))
                {
                    Name = Path.GetFileNameWithoutExtension(FileName);
                    Extention = Path.GetExtension(FileName);
                }
                else
                {
                    Name = FileName;
                }
                string Target = Path.Combine(References.TestingPaths.GetDevCodePath(), @"MMR Tracker V3", "OtherGames", "OOTMMRCOMBO", $"{Name}{Extention}");
                string Content = File.ReadAllText(Target);
                return JsonConvert.DeserializeObject<T>(Content);
            }
        }
    }
}
