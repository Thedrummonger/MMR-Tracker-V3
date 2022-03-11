using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.OptionData;

namespace MMR_Tracker_V3
{
    public class Testing
    {
        public static bool ViewAsUserMode = false;
        public static bool ISDebugging = false;

        public static string GetLogicPath()
        {
            var testFolder = @"D:\Testing";
            var LogicFile = Path.Combine(testFolder, "Logic.txt");
            if (File.Exists(LogicFile))
            {
                return LogicFile;
            }
            Console.WriteLine("Enter Logic Path");
            return Console.ReadLine();
        }

        public static string GetDictPath()
        {
            var testFolder = @"D:\Testing";
            var DictionaryFile = Path.Combine(testFolder, "NewDict.json");
            if (File.Exists(DictionaryFile))
            {
                return DictionaryFile;
            }
            Console.WriteLine("Enter Dictionary Path");
            return Console.ReadLine();
        }

        public static string GetSavePath(bool Loading)
        {
            var testFolder = @"D:\Testing";
            var SaveFile = @"D:\Testing\Save.json";
            if (Loading)
            {
                if (File.Exists(SaveFile)) { return SaveFile; }
                else
                {
                    Console.WriteLine("Enter Save File Path");
                    return Console.ReadLine();
                }
            }
            else
            {
                if (Directory.Exists(testFolder)) { return SaveFile; }
                else
                {
                    Console.WriteLine("Enter Save File Path");
                    return Console.ReadLine();
                }
            }
        }

        public static void CreateOptionsJson()
        {
            List<TrackerOption> Options = new List<TrackerOption>
            {
                new TrackerOption
                {
                    ID = "ProgressiveItems",
                    DisplayName = "Progressive Items",
                    CurrentValue = "disabled",
                    Values = new Dictionary<string, actions>
                    {
                        {"enabled", new actions
                            {
                                LogicReplacements = new LogicReplacement[]
                                {
                                    new LogicReplacement
                                    {

                                        LocationBlacklist = new string[]
                                        {
                                            "MMRTProgressiveBombBag",
                                            "MMRTProgressiveBombBagX2",
                                            "MMRTProgressiveBombBagX3",
                                            "MMRTProgressiveQuiver",
                                            "MMRTProgressiveQuiverX2",
                                            "MMRTProgressiveQuiverX3",
                                            "MMRTProgressiveSword",
                                            "MMRTProgressiveSwordX2",
                                            "MMRTProgressiveSwordX3",
                                            "MMRTProgressiveWallet",
                                            "MMRTProgressiveWalletX2",
                                            "MMRTProgressiveWalletX3",
                                            "MMRTProgressiveMagic",
                                            "MMRTProgressiveMagicX2"
                                        },
                                        ReplacementList = new OptionData.LogicReplacementData[]
                                        {
                                            new OptionData.LogicReplacementData{Target = "ItemBombBag", Replacement = "MMRTProgressiveBombBag"},
                                            new OptionData.LogicReplacementData{Target = "UpgradeBigBombBag", Replacement = "MMRTProgressiveBombBagX2"},
                                            new OptionData.LogicReplacementData{Target = "UpgradeBiggestBombBag", Replacement = "MMRTProgressiveBombBagX3"},
                                            new OptionData.LogicReplacementData{Target = "ItemBow", Replacement = "MMRTProgressiveQuiver"},
                                            new OptionData.LogicReplacementData{Target = "UpgradeBigQuiver", Replacement = "MMRTProgressiveQuiverX2"},
                                            new OptionData.LogicReplacementData{Target = "UpgradeBiggestQuiver", Replacement = "MMRTProgressiveQuiverX3"},
                                            new OptionData.LogicReplacementData{Target = "StartingSword", Replacement = "MMRTProgressiveSword"},
                                            new OptionData.LogicReplacementData{Target = "UpgradeRazorSword", Replacement = "MMRTProgressiveSwordX2"},
                                            new OptionData.LogicReplacementData{Target = "UpgradeGildedSword", Replacement = "MMRTProgressiveSwordX3"},
                                            new OptionData.LogicReplacementData{Target = "UpgradeAdultWallet", Replacement = "MMRTProgressiveWallet"},
                                            new OptionData.LogicReplacementData{Target = "UpgradeGiantWallet", Replacement = "MMRTProgressiveWalletX2"},
                                            new OptionData.LogicReplacementData{Target = "UpgradeRoyalWallet", Replacement = "MMRTProgressiveWalletX3"},
                                            new OptionData.LogicReplacementData{Target = "FairyMagic", Replacement = "MMRTProgressiveMagic"},
                                            new OptionData.LogicReplacementData{Target = "FairyDoubleMagic", Replacement = "MMRTProgressiveMagicX2"},
                                        }
                                    }
                                }
                            } 
                        },
                        { "disabled", new actions() }
                    }
                },
                new TrackerOption
                {
                    ID = "SmallKeysy",
                    DisplayName = "Small Key Doors Unlocked",
                    CurrentValue = "disabled",
                    Values = new Dictionary<string, actions>
                    {
                        {"enabled", new actions
                            {
                                LogicReplacements = new LogicReplacement[]
                                {
                                    new LogicReplacement
                                    {
                                         ReplacementList = new OptionData.LogicReplacementData[]
                                         {
                                             new OptionData.LogicReplacementData{Target = "ItemWoodfallKey1", Replacement = "MMRTSmallKeysy"},
                                             new OptionData.LogicReplacementData{Target = "ItemSnowheadKey1", Replacement = "MMRTSmallKeysy"},
                                             new OptionData.LogicReplacementData{Target = "ItemSnowheadKey2", Replacement = "MMRTSmallKeysy"},
                                             new OptionData.LogicReplacementData{Target = "ItemSnowheadKey3", Replacement = "MMRTSmallKeysy"},
                                             new OptionData.LogicReplacementData{Target = "ItemGreatBayKey1", Replacement = "MMRTSmallKeysy"},
                                             new OptionData.LogicReplacementData{Target = "ItemStoneTowerKey1", Replacement = "MMRTSmallKeysy"},
                                             new OptionData.LogicReplacementData{Target = "ItemStoneTowerKey2", Replacement = "MMRTSmallKeysy"},
                                             new OptionData.LogicReplacementData{Target = "ItemStoneTowerKey3", Replacement = "MMRTSmallKeysy"},
                                             new OptionData.LogicReplacementData{Target = "ItemStoneTowerKey4", Replacement = "MMRTSmallKeysy"}
                                         }
                                    }
                                }
                            }
                        },
                        { "disabled", new actions() }
                    }
                },
                new TrackerOption
                {
                    ID = "BossKeysy",
                    DisplayName = "Boss Key Doors Unlocked",
                    CurrentValue = "disabled",
                    Values = new Dictionary<string, actions>
                    {
                        {"enabled", new actions
                            {
                                LogicReplacements = new LogicReplacement[]
                                {
                                    new LogicReplacement
                                    {
                                         ReplacementList = new OptionData.LogicReplacementData[]
                                         {
                                             new OptionData.LogicReplacementData{Target = "ItemWoodfallBossKey", Replacement = "MMRTBossKeysy"},
                                             new OptionData.LogicReplacementData{Target = "ItemSnowheadBossKey", Replacement = "MMRTBossKeysy"},
                                             new OptionData.LogicReplacementData{Target = "ItemGreatBayBossKey", Replacement = "MMRTBossKeysy"},
                                             new OptionData.LogicReplacementData{Target = "ItemStoneTowerBossKey", Replacement = "MMRTBossKeysy"},
                                         }
                                    }
                                }
                            }
                        },
                        { "disabled", new actions() }
                    }
                },
                new TrackerOption
                {
                    ID = "NoSafteyLogic",
                    DisplayName = "Ignore Trade ItemSaftey Logic",
                    CurrentValue = "enabled",
                    Values = new Dictionary<string, actions>
                    {
                        {"enabled", new actions
                            {
                                LogicReplacements = new LogicReplacement[]
                                {
                                    new LogicReplacement
                                    {
                                        LocationWhitelist = new string[]
                                        {
                                            "UpgradeBigBombBag",
                                            "MaskBlast"
                                        },
                                        ReplacementList = new OptionData.LogicReplacementData[]
                                        {
                                            new OptionData.LogicReplacementData{Target = "ItemWoodfallBossKey", Replacement = "MMRTBossKeysy"},
                                            new OptionData.LogicReplacementData{Target = "ItemSnowheadBossKey", Replacement = "MMRTBossKeysy"},
                                            new OptionData.LogicReplacementData{Target = "ItemGreatBayBossKey", Replacement = "MMRTBossKeysy"},
                                            new OptionData.LogicReplacementData{Target = "ItemStoneTowerBossKey", Replacement = "MMRTBossKeysy"},
                                        }
                                    }
                                }
                            }
                        },
                        { "disabled", new actions() }
                    }
                },
                new TrackerOption
                {
                    ID = "BYOA",
                    DisplayName = "Bring Your Own Ammo",
                    CurrentValue = "disabled",
                    Values = new Dictionary<string, actions>
                    {
                        {"enabled", new actions
                            {
                                AdditionalLogic = new AdditionalLogic[]
                                {
                                    new AdditionalLogic
                                    {
                                        LocationWhitelist = new string[]{ "UpgradeBigQuiver", "UpgradeBiggestQuiver", "HeartPieceSwampArchery" },
                                        AdditionalRequirements = new string[] { "MMRTarrows40" }
                                    },
                                    new AdditionalLogic
                                    {
                                        LocationWhitelist = new string[]{ "HeartPieceTownArchery" },
                                        AdditionalRequirements = new string[] { "MMRTarrows50" }
                                    },
                                    new AdditionalLogic
                                    {
                                        LocationWhitelist = new string[]{ "HeartPieceHoneyAndDarling" },
                                        AdditionalRequirements = new string[] { "MMRTbombchu10" }
                                    },
                                    new AdditionalLogic
                                    {
                                        LocationWhitelist = new string[]{ "MaskRomani" },
                                        AdditionalRequirements = new string[] { "MMRTescortCremia" }
                                    }
                                }
                            }
                        },
                        { "disabled", new actions() }
                    }
                }
            };
            System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(Options, _NewtonsoftJsonSerializerOptions));
        }

        public class EntranceTable
        {
            public string[] CoupledEntrances { get; set; }
            public string[] OneWayEntrances { get; set; }
        }

        public static void CodeTesting(LogicObjects.TrackerInstance instance)
        {
            EntranceTable TestTable = new EntranceTable();
            TestTable = JsonConvert.DeserializeObject<EntranceTable>(File.ReadAllText(@"C:\Users\ttalbot\Documents\VS CODE STUFF\MMR Tracker V3\MMR Tracker V3\TestingFiles\OOTREntranceReference.json"));

            List<EntranceData.EntranceRandoExit> exits = new List<EntranceData.EntranceRandoExit>();

            foreach(var i in TestTable.CoupledEntrances)
            {
                EntranceData.EntranceRandoExit Exit = new EntranceData.EntranceRandoExit();
                var data = i.Split(new string[] { "->" }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
                Exit.ParentAreaID = data[0];
                Exit.ID = data[1];
                exits.Add(Exit);

            }
            foreach(var i in exits)
            {
                if (exits.Where(x => x.ID == i.ID).Count() == 1)
                {
                    Debug.WriteLine($"[{i.ID}]                       Only has one entrance");
                }
            }

        }

        public readonly static Newtonsoft.Json.JsonSerializerSettings _NewtonsoftJsonSerializerOptions = new Newtonsoft.Json.JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };
    }
}
