using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMR_Tracker_V3.TrackerObjects;
using static MMR_Tracker_V3.GameDataCreation.OOTMMV2.OOTMMUtil;
using System.IO;
using static TestingForm.GameDataCreation.OOTMMV2.datamodel;
using MMR_Tracker_V3.Logic;

namespace MMR_Tracker_V3.GameDataCreation.OOTMMV2
{
    //Settings Parser www.typescriptlang.org
    //Add to the end of the code "console.log(JSON.stringify(SETTINGS))"
    internal class SettingsCreation
    {
        public static void CreateSettings(LogicDictionaryData.LogicDictionary logicDictionaryData, OOTMMParserData ParserData)
        {
            List<OOTMMSetting> SettingsList = JsonConvert.DeserializeObject<List<OOTMMSetting>>(File.ReadAllText(ParserData.SettingsFile));
            int Priority = 1;
            foreach (var Setting in SettingsList)
            {
                if (Setting.type == "number")
                {
                    OptionData.IntOption IntSettingDictEntry = new OptionData.IntOption
                    {
                        ID = Setting.key,
                        Name = Setting.name,
                        SubCategory= Utility.ConvertToCamelCase(Setting.category.Replace(".", " ")),
                        Value = (int)Setting.defaultvalue,
                        Priority= Priority
                    };
                    logicDictionaryData.IntOptions.Add(Setting.key, IntSettingDictEntry);
                    Priority++;
                }
                else if (Setting.type == "boolean")
                {
                    OptionData.ToggleOption IntSettingDictEntry = new OptionData.ToggleOption
                    {
                        ID = Setting.key,
                        Name = Setting.name,
                        SubCategory= Utility.ConvertToCamelCase(Setting.category.Replace(".", " ")),
                        Value = ((bool)Setting.defaultvalue).ToString(),
                        Priority= Priority
                    };
                    IntSettingDictEntry.CreateSimpleValues();
                    logicDictionaryData.ToggleOptions.Add(Setting.key, IntSettingDictEntry);
                    Priority++;
                }
                else if (Setting.type == "enum")
                {
                    OptionData.ChoiceOption IntSettingDictEntry = new OptionData.ChoiceOption
                    {
                        ID = Setting.key,
                        Name = Setting.name,
                        SubCategory= Utility.ConvertToCamelCase(Setting.category.Replace(".", " ")),
                        Value = Setting.defaultvalue.ToString(),
                        ValueList = new Dictionary<string, OptionData.OptionValue>(),
                        Priority= Priority
                    };
                    IntSettingDictEntry.CreateSimpleValues(Setting.values.Select(x => x.value).ToArray());
                    foreach (var i in Setting.values) { IntSettingDictEntry.ValueList[i.value].Name = i.name; }
                    logicDictionaryData.ChoiceOptions.Add(Setting.key, IntSettingDictEntry);
                    Priority++;
                }
                else if (Setting.type == "set")
                {
                    OptionData.MultiSelectOption multiSelectSettingDictEntry = new OptionData.MultiSelectOption
                    {
                        ID = Setting.key,
                        Name = Setting.name,
                        SubCategory= Utility.ConvertToCamelCase(Setting.category.Replace(".", " ")),
                        EnabledValues = new HashSet<string>(),
                        ValueList = new Dictionary<string, OptionData.OptionValue>(),
                        Priority= Priority
                    };

                    foreach (OOTMMSettingValue i in Setting.values)
                    {
                        OptionData.OptionValue optionValue = new OptionData.OptionValue
                        {
                            ID = i.value,
                            Name = i.name
                        };
                        multiSelectSettingDictEntry.ValueList.Add(i.value, optionValue);
                    }
                    logicDictionaryData.MultiSelectOptions.Add(Setting.key, multiSelectSettingDictEntry);
                    Priority++;
                }
            }
            foreach (var setting in ParserData.DungeonLayouts)
            {
                var Data = setting.Split("_").ToArray();
                OptionData.ToggleOption IntSettingDictEntry = new OptionData.ToggleOption
                {
                    ID = setting,
                    Name = Utility.ConvertToCamelCase(setting.Replace("_", " ")),
                    SubCategory= "Master Quest Dungeons",
                    Value = false.ToString(),
                    Priority= Priority
                };
                IntSettingDictEntry.CreateSimpleValues();
                logicDictionaryData.ToggleOptions.Add(setting, IntSettingDictEntry);
                Priority++;
            }

            WorldEventRequirementOptions(logicDictionaryData, Priority);
        }
        private static void WorldEventRequirementOptions(LogicDictionaryData.LogicDictionary dictionaryFile, int Priority)
        {
            var MASKS_REGULAR = new string[] {
              "MM_MASK_CAPTAIN",
              "MM_MASK_GIANT",
              "MM_MASK_ALL_NIGHT",
              "MM_MASK_BUNNY",
              "MM_MASK_KEATON",
              "MM_MASK_GARO",
              "MM_MASK_ROMANI",
              "MM_MASK_TROUPE_LEADER",
              "MM_MASK_POSTMAN",
              "MM_MASK_COUPLE",
              "MM_MASK_GREAT_FAIRY",
              "MM_MASK_GIBDO",
              "MM_MASK_DON_GERO",
              "MM_MASK_KAMARO",
              "MM_MASK_TRUTH",
              "MM_MASK_STONE",
              "MM_MASK_BREMEN",
              "MM_MASK_BLAST",
              "MM_MASK_SCENTS",
              "MM_MASK_KAFEI",
              "SHARED_MASK_TRUTH",
              "SHARED_MASK_BUNNY",
              "SHARED_MASK_KEATON",
            };
            var MASKS_TRANSFORM = new string[]{ 
                "MM_MASK_DEKU", 
                "MM_MASK_GORON", 
                "MM_MASK_ZORA", 
                "MM_MASK_FIERCE_DEITY", 
                "SHARED_MASK_GORON", 
                "SHARED_MASK_ZORA" 
            };
            var MASKS_OOT = new string[]{
              "OOT_MASK_SKULL",
              "OOT_MASK_SPOOKY",
              "OOT_MASK_KEATON",
              "OOT_MASK_BUNNY",
              "OOT_MASK_TRUTH",
              "OOT_MASK_GERUDO",
              "OOT_MASK_GORON",
              "OOT_MASK_ZORA",
              "SHARED_MASK_KEATON",
              "SHARED_MASK_BUNNY",
              "SHARED_MASK_TRUTH",
              "SHARED_MASK_GORON",
              "SHARED_MASK_ZORA",
            };
            var STONES = new string[]{
              "OOT_STONE_EMERALD",
              "OOT_STONE_RUBY",
              "OOT_STONE_SAPPHIRE",
            };

            var MEDALLIONS = new string[]{
              "OOT_MEDALLION_LIGHT",
              "OOT_MEDALLION_FOREST",
              "OOT_MEDALLION_FIRE",
              "OOT_MEDALLION_WATER",
              "OOT_MEDALLION_SPIRIT",
              "OOT_MEDALLION_SHADOW",
            };

            var REMAINS = new string[]{
              "MM_REMAINS_ODOLWA",
              "MM_REMAINS_GOHT",
              "MM_REMAINS_GYORG",
              "MM_REMAINS_TWINMOLD",
            };
            var DUNGEON_REWARDS = STONES.Concat(MEDALLIONS).Concat(REMAINS).ToArray();

            Dictionary<string, string[]> PossibleReqs = new()
            {
                { "Spiritual Stones|stones", STONES},
                { "Medallions|medallions", MEDALLIONS },
                { "Boss Remains|remains", REMAINS },
                { "Gold Skulltulas Tokens|skullsGold", new string[] { "OOT_GS_TOKEN" } },
                { "Swamp Skulltulas Tokens|skullsSwamp", new string[] { "MM_GS_TOKEN_SWAMP" } },
                { "Ocean Skulltulas Tokens|skullsOcean", new string[] { "MM_GS_TOKEN_OCEAN" } },
                { "Stray Fairies (Woodfall)|fairiesWF", new string[] { "MM_STRAY_FAIRY_WF" } },
                { "Stray Fairies (Snowhead)|fairiesSH", new string[] { "MM_STRAY_FAIRY_SH" } },
                { "Stray Fairies (Great Bay)|fairiesGB", new string[] { "MM_STRAY_FAIRY_GB" } },
                { "Stray Fairies (Stone Tower)|fairiesST", new string[] { "MM_STRAY_FAIRY_ST" } },
                { "Stray Fairy (Clock Town)|fairyTown", new string[] { "MM_STRAY_FAIRY_TOWN" } },
                { "Regular Masks (MM)|masksRegular", MASKS_REGULAR },
                { "Transformation Masks (MM)|masksTransform", MASKS_TRANSFORM },
                { "Masks (OoT)|masksOot", MASKS_OOT },
                { "Triforce Pieces|triforce", new string[] { "SHARED_TRIFORCE" } },
                { "Coins (Red)|coinsRed", new string[]{ "OOT_COIN_RED" } },
                { "Coins (Green)|coinsGreen", new string[]{ "OOT_COIN_GREEN" }  },
                { "Coins (Blue)|coinsBlue", new string[]{ "OOT_COIN_BLUE" }  },
                { "Coins (Yellow)|coinsYellow", new string[]{ "OOT_COIN_YELLOW" }  },
            };

            AddCondition("moon", "mm", "Moon Access Conditions", "Boss Remains", 4);
            AddCondition("majora", "MM", "Majora Child Conditions", Logic: "setting{majoraChild, custom}");
            AddCondition("bridge", "oot", "Rainbow Bridge Conditions", "Medallions", 6);
            AddCondition("lacs", "oot", "Light Arrow Cutscene Conditions", Logic: "setting{lacs, custom}");
            AddCondition("ganon_bk", "oot", "Ganon Boss Key Conditions", Logic: "setting{ganonBossKey, custom}");

            void AddCondition(string ID, string Game, string Category, string DefaultValue = null, int DefaultCount = 0, string Logic = null)
            {
                dictionaryFile.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem
                {
                    Id = $"{Game.ToUpper()}_HAS_{ID.ToUpper()}_REQUIREMENTS",
                    RequiredItems = new List<string> { $"{ID.ToLower()}_req, {ID.ToLower()}_count" }
                });
                foreach (var i in PossibleReqs)
                {
                    var namedata = i.Key.Split('|');

                    OptionData.ToggleOption Requirement = new OptionData.ToggleOption();
                    Requirement.ID = $"{ID.ToLower()}_{namedata[1]}";
                    Requirement.Name = namedata[0];
                    Requirement.SubCategory = Category;
                    Requirement.Value = DefaultValue is null ? "false" : (namedata[0] == DefaultValue).ToString().ToLower();
                    Requirement.CreateSimpleValues(true);
                    Requirement.Conditionals = Logic is null ? new List<List<string>>() : LogicStringConverter.ConvertLogicStringToConditional(GenData.OOTMMLogicStringParser, Logic, ID);
                    Requirement.Enabled.Actions = new OptionData.Action();
                    Requirement.Enabled.Actions.VariableEdit.Add($"{ID.ToLower()}_req", new OptionData.CollectionEditData
                    {
                        action = MiscData.MathOP.add,
                        Values = (namedata[1].StartsWith("fair") || namedata[1].StartsWith("skull") || namedata[1] == "triforce" || namedata[1].StartsWith("coins")) ? i.Value.ToList() : i.Value.Select(x => $"{x}, 1").ToList()
                    });
                    Requirement.Priority = Priority;
                    dictionaryFile.ToggleOptions.Add(Requirement.ID, Requirement);
                    Priority++;
                }
                OptionData.LogicEntryCollection ReqVar = new OptionData.LogicEntryCollection();
                ReqVar.ID = $"{ID.ToLower()}_req";
                ReqVar.Entries = new List<string>();
                dictionaryFile.LogicEntryCollections.Add(ReqVar.ID, ReqVar);

                OptionData.IntOption req_count = new OptionData.IntOption();
                req_count.SubCategory = Category;
                req_count.Name = "Items Required";
                req_count.ID = $"{ID.ToLower()}_count";
                req_count.Value = DefaultCount;
                req_count.Conditionals = Logic is null ? new List<List<string>>() : LogicStringConverter.ConvertLogicStringToConditional(GenData.OOTMMLogicStringParser, Logic, ID);
                req_count.Priority = Priority;
                dictionaryFile.IntOptions.Add(req_count.ID, req_count);
                Priority++;
            }

        }
    }
}
