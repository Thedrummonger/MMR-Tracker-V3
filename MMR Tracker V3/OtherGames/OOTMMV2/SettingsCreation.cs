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
            foreach (var setting in ParserData.DungeonLayouts)
            {
                var Data = setting.Split("_").ToArray();
                OptionData.TrackerOption IntSettingDictEntry = new OptionData.TrackerOption
                {
                    ID = setting,
                    DisplayName = setting.Replace("_", " "),
                    SubCategory= "Dungeon Layouts",
                    CurrentValue = "Vanilla",
                };
                IntSettingDictEntry.CreateSimpleValues(new string[] { "Vanilla", "MQ" });
                logicDictionaryData.Options.Add(setting, IntSettingDictEntry);
            }
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
        }
    }
}
