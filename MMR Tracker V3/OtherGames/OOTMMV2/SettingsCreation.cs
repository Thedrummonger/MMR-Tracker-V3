using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMR_Tracker_V3.TrackerObjects;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.datamodel;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.OOTMMUtil;
using System.IO;

namespace MMR_Tracker_V3.OtherGames.OOTMMV2
{
    internal class SettingsCreation
    {
        public static void CreateSettings(LogicDictionaryData.LogicDictionary logicDictionaryData, OOTMMParserData ParserData)
        {
            List<OOTMMSetting> SettingsList = JsonConvert.DeserializeObject<List<OOTMMSetting>>(File.ReadAllText(ParserData.SettingsFile));
            foreach (var Setting in SettingsList)
            {
                if (Setting.defaultvalue is Int64 IntValue)
                {
                    OptionData.TrackerVar IntSettingDictEntry = new OptionData.TrackerVar
                    {
                        ID = Setting.key,
                        Name = Setting.name,
                        Static = false,
                        SubCategory= Setting.category.Replace(".", " "),
                        Value = IntValue
                    };
                    logicDictionaryData.Variables.Add(Setting.key, IntSettingDictEntry);
                }
                else if (Setting.defaultvalue is bool BoolValue)
                {
                    OptionData.TrackerVar IntSettingDictEntry = new OptionData.TrackerVar
                    {
                        ID = Setting.key,
                        Name = Setting.name,
                        Static = false,
                        SubCategory= Setting.category.Replace(".", " "),
                        Value = BoolValue
                    };
                    logicDictionaryData.Variables.Add(Setting.key, IntSettingDictEntry);
                }
                else if (Setting.defaultvalue is string StringValue)
                {
                    OptionData.TrackerOption IntSettingDictEntry = new OptionData.TrackerOption
                    {
                        ID = Setting.key,
                        DisplayName = Setting.name,
                        SubCategory= Setting.category.Replace(".", " "),
                        CurrentValue = StringValue,
                        Values = new Dictionary<string, OptionData.actions>()
                    };
                    IntSettingDictEntry.CreateSimpleValues(Setting.values.Select(x => x.value).ToArray());
                    foreach (var i in Setting.values) { IntSettingDictEntry.Values[i.value].Name = i.name; }
                    logicDictionaryData.Options.Add(Setting.key, IntSettingDictEntry);
                }
            }
            foreach (var setting in ParserData.DungeonLayouts)
            {
                var Data = setting.Split("_").ToArray();
                OptionData.TrackerVar IntSettingDictEntry = new OptionData.TrackerVar
                {
                    ID = setting,
                    Static = false,
                    Name = setting.Replace("_", " "),
                    SubCategory= "Master Quest Dungeons",
                    Value = false,
                };
                logicDictionaryData.Variables.Add(setting, IntSettingDictEntry);
            }

            WorldEventRequirementOptions(logicDictionaryData);
        }
        private static void WorldEventRequirementOptions(LogicDictionaryData.LogicDictionary dictionaryFile)
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
            };

            AddCondition("moon", "mm", "Moon Access Conditions", "Boss Remains", 4);
            AddCondition("majora", "MM", "Majora Child Conditions");
            AddCondition("bridge", "oot", "Rainbow Bridge Conditions", "Medallions", 6);
            AddCondition("lacs", "oot", "Light Arrow Cutscene Conditions");
            AddCondition("ganon_bk", "oot", "Ganon Boss Key Conditions");

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
                        EditValue = (namedata[1].StartsWith("fair") || namedata[1].StartsWith("skull") || namedata[1] == "triforce") ? i.Value : i.Value.Select(x => $"{x}, 1").ToArray()
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

        }
    }
}
