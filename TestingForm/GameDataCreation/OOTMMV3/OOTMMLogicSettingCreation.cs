using MMR_Tracker_V3;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TestingForm.GameDataCreation.OOTMMV3
{
    internal class OOTMMLogicSettingCreation(OOTMMDataGenerator MainDataGenerator)
    {
        OOTMMDataGenerator Generator = MainDataGenerator;
        int OptionPriority = 1;

        public void AddAgeLogic()
        {
            string CanBeChild = "setting{startingAge, child} || OOT_TIME_TRAVEL_EVENT";
            string CanBeAdult = "setting{startingAge, adult} || OOT_TIME_TRAVEL_EVENT";
            string age_child = $"(setting{{age_filter, adult, false}}) && ({CanBeChild})";
            string age_adult = $"(setting{{age_filter, child, false}}) && ({CanBeAdult})";

            Generator.LogicFile.Logic.Add(LogicUtilities.CreateLogicEntryFromLogicLine("age_child", Generator.LogicStringParser, age_child));
            Generator.LogicFile.Logic.Add(LogicUtilities.CreateLogicEntryFromLogicLine("age_adult", Generator.LogicStringParser, age_adult));

            OptionData.ChoiceOption option = new(null)
            {
                ID = "age_filter",
                Name = "Age Filter",
                Value = "both",
                Priority = OptionPriority
            };
            option.CreateSimpleValues("child", "adult", "both");
            Generator.dictionary.ChoiceOptions.Add(option.ID, option);

            OptionPriority++;
        }

        public void CreateSettings()
        {
            foreach(var setting in Generator.extraData.options)
            {
                switch(setting.type) 
                {
                    case "number":
                        AddIntOption(setting);
                        break;
                    case "boolean":
                        AddToggleOption(setting);
                        break;
                    case "enum":
                        AddChoiceOption(setting);
                        break;
                    case "set":
                        AddMultiSelectOption(setting);
                        break;
                }
            }
        }

        private void AddMultiSelectOption(OOTMMDataClasses.OOTMMSetting Setting)
        {
            OptionData.MultiSelectOption option = new OptionData.MultiSelectOption(null)
            {
                ID = Setting.key,
                Name = Setting.name,
                SubCategory = Utility.ConvertToCamelCase(Setting.category.Replace(".", "/")),
                EnabledValues = [],
                ValueList = [],
                Priority = OptionPriority
            };

            foreach(OOTMMDataClasses.OOTMMSettingValue i in Setting.values)
            {
                OptionData.OptionValue optionValue = new OptionData.OptionValue
                {
                    ID = i.value,
                    Name = i.name
                };
                option.ValueList.Add(i.value, optionValue);
            }
            Generator.dictionary.MultiSelectOptions.Add(Setting.key, option);
            OptionPriority++;
        }

        private void AddChoiceOption(OOTMMDataClasses.OOTMMSetting Setting)
        {
            OptionData.ChoiceOption option = new OptionData.ChoiceOption(null)
            {
                ID = Setting.key,
                Name = Setting.name,
                SubCategory = Utility.ConvertToCamelCase(Setting.category.Replace(".", "/")),
                Value = Setting.defaultvalue.ToString(),
                ValueList = new Dictionary<string, OptionData.OptionValue>(),
                Priority = OptionPriority
            };
            option.CreateSimpleValues(Setting.values.Select(x => x.value).ToArray());
            foreach (var i in Setting.values) { option.ValueList[i.value].Name = i.name; }
            Generator.dictionary.ChoiceOptions.Add(Setting.key, option);
            OptionPriority++;
        }

        private void AddToggleOption(OOTMMDataClasses.OOTMMSetting Setting)
        {
            OptionData.ToggleOption option = new OptionData.ToggleOption(null)
            {
                ID = Setting.key,
                Name = Setting.name,
                SubCategory = Utility.ConvertToCamelCase(Setting.category.Replace(".", "/")),
                Priority = OptionPriority
            };
            option.CreateSimpleValues();
            option.SetValue((bool)Setting.defaultvalue);
            Generator.dictionary.ToggleOptions.Add(Setting.key, option);
            OptionPriority++;
        }

        private void AddIntOption(OOTMMDataClasses.OOTMMSetting Setting)
        {
            OptionData.IntOption option = new OptionData.IntOption(null)
            {
                ID = Setting.key,
                Name = Setting.name,
                SubCategory = Utility.ConvertToCamelCase(Setting.category.Replace(".", "/")),
                Value = (int)Setting.defaultvalue,
                Priority = OptionPriority,
                Min = 0
            };
            if (Setting.min is not null) { option.Min = (int)Setting.min; }
            if (Setting.max is not null) { option.Max = (int)Setting.max; }
            Generator.dictionary.IntOptions.Add(Setting.key, option);
            OptionPriority++;
        }

        public void AddMasterQuestDungeons()
        {
            OptionData.MultiSelectOption option = new OptionData.MultiSelectOption(null)
            {
                ID = "MasterQuest",
                Name = "Master Quest Dungeons",
                EnabledValues = [],
                ValueList = [],
                Priority = OptionPriority
            };
            option.CreateSimpleValues([.. Generator.extraData.dungeons.Keys]);
            foreach(var o in option.ValueList)
            {
                o.Value.Name = Generator.extraData.dungeons[o.Key];
            }
            Generator.dictionary.MultiSelectOptions.Add(option.ID, option);
            OptionPriority++;
        }

        public void CreateMiscLogicItemCollections()
        {
            AddCollection("OOT_stones", Generator.extraData.stones);
            AddCollection("OOT_medallions", Generator.extraData.medallions);
            AddCollection("MM_remains", Generator.extraData.remains);
            string[] DUNGEON_REWARDS = [.. Generator.extraData.stones, .. Generator.extraData.medallions, .. Generator.extraData.remains];
            AddCollection("SHARED_dungeon_rewards", DUNGEON_REWARDS);

            AddCollection("MM_masks", Generator.extraData.mm_masks);
            AddCollection("OOT_masks", Generator.extraData.oot_masks);
            
            AddCollection("MM_masks_transformation", Generator.extraData.mm_masks_transformation);

            AddFishCollection("child", "fish", 7, 14);
            AddFishCollection("child", "loach", 14, 19);
            AddFishCollection("adult", "fish", 8, 25);
            AddFishCollection("adult", "loach", 29, 36);

            void AddFishCollection(string Age, string type, int min, int max)
            {
                List<string> FISH = [];
                for (int i = min; i <= max; i++) { FISH.Add($"OOT_FISHING_POND_{Age.ToUpper()}_{type.ToUpper()}_{i}LBS"); }
                AddCollection($"OOT_{Age.ToUpper()}_{type.ToUpper()}_{min}_{max}", FISH);
            }

            void AddCollection(string ID, IEnumerable<string> Values)
            {
                OptionData.LogicEntryCollection ReqVar = new OptionData.LogicEntryCollection();
                ReqVar.ID = ID;
                ReqVar.Entries = Values.ToList();
                Generator.dictionary.LogicEntryCollections.Add(ID, ReqVar);
            }
        }

        public void WorldEventRequirementOptions()
        {
            Dictionary<string, string[]> PossibleReqs = new()
            {
                { "Spiritual Stones|stones", Generator.extraData.stones},
                { "Medallions|medallions", Generator.extraData.medallions },
                { "Boss Remains|remains", Generator.extraData.remains },
                { "Gold Skulltulas Tokens|skullsGold", new string[] { "OOT_GS_TOKEN" } },
                { "Swamp Skulltulas Tokens|skullsSwamp", new string[] { "MM_GS_TOKEN_SWAMP" } },
                { "Ocean Skulltulas Tokens|skullsOcean", new string[] { "MM_GS_TOKEN_OCEAN" } },
                { "Stray Fairies (Woodfall)|fairiesWF", new string[] { "MM_STRAY_FAIRY_WF" } },
                { "Stray Fairies (Snowhead)|fairiesSH", new string[] { "MM_STRAY_FAIRY_SH" } },
                { "Stray Fairies (Great Bay)|fairiesGB", new string[] { "MM_STRAY_FAIRY_GB" } },
                { "Stray Fairies (Stone Tower)|fairiesST", new string[] { "MM_STRAY_FAIRY_ST" } },
                { "Stray Fairy (Clock Town)|fairyTown", new string[] { "MM_STRAY_FAIRY_TOWN" } },
                { "Regular Masks (MM)|masksRegular", Generator.extraData.mm_masks },
                { "Transformation Masks (MM)|masksTransform", Generator.extraData.mm_masks_transformation },
                { "Masks (OoT)|masksOot", Generator.extraData.oot_masks },
                { "Triforce Pieces|triforce", new string[] { "SHARED_TRIFORCE" } },
                { "Coins (Red)|coinsRed", new string[]{ "OOT_COIN_RED" } },
                { "Coins (Green)|coinsGreen", new string[]{ "OOT_COIN_GREEN" } },
                { "Coins (Blue)|coinsBlue", new string[]{ "OOT_COIN_BLUE" } },
                { "Coins (Yellow)|coinsYellow", new string[]{ "OOT_COIN_YELLOW" } },
            };

            AddCondition("moon", "mm", "Moon Access Conditions", "Boss Remains", 4);
            AddCondition("majora", "MM", "Majora Child Conditions", Logic: "setting{majoraChild, custom}");
            AddCondition("bridge", "oot", "Rainbow Bridge Conditions", "Medallions", 6);
            AddCondition("lacs", "oot", "Light Arrow Cutscene Conditions", Logic: "setting{lacs, custom}");
            AddCondition("ganon_bk", "oot", "Ganon Boss Key Conditions", Logic: "setting{ganonBossKey, custom}");

            void AddCondition(string ID, string Game, string Category, string DefaultValue = null, int DefaultCount = 0, string Logic = null)
            {
                Generator.dictionary.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem
                {
                    Id = $"{Game.ToUpper()}_HAS_SPECIAL_{ID.ToUpper()}",
                    RequiredItems = [$"{ID.ToLower()}_req, {ID.ToLower()}_count"]
                });
                foreach (var i in PossibleReqs)
                {
                    var namedata = i.Key.Split('|');

                    OptionData.ToggleOption Requirement = new(null);
                    Requirement.ID = $"{ID.ToLower()}_{namedata[1]}";
                    Requirement.Name = namedata[0];
                    Requirement.SubCategory = Category;
                    Requirement.Value = DefaultValue is null ? "false" : (namedata[0] == DefaultValue).ToString().ToLower();
                    Requirement.CreateSimpleValues(true);
                    Requirement.Conditionals = Logic is null ? [] : LogicStringConverter.ConvertLogicStringToConditional(Generator.LogicStringParser, Logic, ID);
                    Requirement.Enabled.Actions = new OptionData.Action();
                    Requirement.Enabled.Actions.VariableEdit.Add($"{ID.ToLower()}_req", new OptionData.CollectionEditData
                    {
                        action = MiscData.MathOP.add,
                        Values = (namedata[1].StartsWith("fair") || namedata[1].StartsWith("skull") || namedata[1] == "triforce" || namedata[1].StartsWith("coins")) ? [.. i.Value] : i.Value.Select(x => $"{x}, 1").ToList()
                    });
                    Requirement.Priority = OptionPriority;
                    Generator.dictionary.ToggleOptions.Add(Requirement.ID, Requirement);
                    OptionPriority++;
                }
                OptionData.LogicEntryCollection ReqVar = new OptionData.LogicEntryCollection();
                ReqVar.ID = $"{ID.ToLower()}_req";
                ReqVar.Entries = new List<string>();
                Generator.dictionary.LogicEntryCollections.Add(ReqVar.ID, ReqVar);

                OptionData.IntOption req_count = new OptionData.IntOption(null);
                req_count.SubCategory = Category;
                req_count.Name = "Items Required";
                req_count.ID = $"{ID.ToLower()}_count";
                req_count.Value = DefaultCount;
                req_count.Conditionals = Logic is null ? new List<List<string>>() : LogicStringConverter.ConvertLogicStringToConditional(Generator.LogicStringParser, Logic, ID);
                req_count.Priority = OptionPriority;
                req_count.Min = 0;
                Generator.dictionary.IntOptions.Add(req_count.ID, req_count);
                OptionPriority++;
            }

        }
    }
}
