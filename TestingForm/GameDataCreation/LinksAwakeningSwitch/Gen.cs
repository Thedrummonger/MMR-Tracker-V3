using Microsoft.VisualBasic.Logging;
using MMR_Tracker_V3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TestingForm.GameDataCreation.LinksAwakeningSwitch.Data;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;

namespace TestingForm.GameDataCreation.LinksAwakeningSwitch
{
    internal class Gen
    {
        public static void GenData()
        {
            var LogicFile = File.ReadAllText(Paths.RandoLogicFile());
            var LogicFileYamlString = TestingUtility.ConvertYamlStringToJsonString(LogicFile);
            var LogicFileObject = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(LogicFileYamlString);

            var ItemsFile = File.ReadAllText(Paths.RandoItemsFile());
            var ItemsFileYamlString = TestingUtility.ConvertYamlStringToJsonString(ItemsFile);
            var ItemsFileObject = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, object>>>>(ItemsFileYamlString);

            //MMR_Tracker_V3.Utility.PrintObjectToConsole(LogicFileObject);
            List<ExpandedLogicEntry> expandedLogicEntries = new List<ExpandedLogicEntry>();

            LogicStringParser logicParser = new(LogicStringParser.OperatorType.CStlyeSingle);
            foreach (var LogicEntry in LogicFileObject)
            {
                ExpandedLogicEntry Entry = new();
                Entry.Name = LogicEntry.Key;
                Dictionary<string, string> LogicAugments = [];
                foreach(var data in LogicEntry.Value)
                {
                    switch (data.Key)
                    {
                        case "type":
                            Entry.type = data.Value; break;
                        case "condition-basic":
                            Entry.LogicBasic = data.Value; break;
                        case "condition-advanced":
                            Entry.LogicAdvanced = data.Value; break;
                        case "condition-glitched":
                            Entry.LogicGlitched = data.Value; break;
                        case "condition-hell":
                            Entry.LogicHell = data.Value; break;
                        default:
                            if (data.Key.StartsWith("condition-")) { LogicAugments.Add(data.Key.SplitOnce('-').Item2, data.Value); }
                            break;
                    }
                }
                string HighestLevelLogic = "true";

                if (string.IsNullOrWhiteSpace(Entry.LogicBasic)) { Entry.LogicBasic = HighestLevelLogic; }
                else { HighestLevelLogic = Entry.LogicBasic; }
                Entry.LogicBasic = $"(setting{{logic, basic}} & ({Entry.LogicBasic}))";
                string FinalLogicString = Entry.LogicBasic;

                if (string.IsNullOrWhiteSpace(Entry.LogicAdvanced)) { Entry.LogicAdvanced = HighestLevelLogic; }
                else { HighestLevelLogic = Entry.LogicAdvanced; }
                Entry.LogicAdvanced = $"(setting{{logic, advanced}} & ({Entry.LogicAdvanced}))";
                FinalLogicString += $" | {Entry.LogicAdvanced}";

                if (string.IsNullOrWhiteSpace(Entry.LogicGlitched)) { Entry.LogicGlitched = HighestLevelLogic; }
                else { HighestLevelLogic = Entry.LogicGlitched; }
                Entry.LogicGlitched = $"(setting{{logic, glitched}} & ({Entry.LogicGlitched}))";
                FinalLogicString += $" | {Entry.LogicGlitched}";

                if (string.IsNullOrWhiteSpace(Entry.LogicHell)) { Entry.LogicHell = HighestLevelLogic; }
                else { HighestLevelLogic = Entry.LogicHell; }
                Entry.LogicHell = $"(setting{{logic, hell}} & ({Entry.LogicHell}))";
                FinalLogicString += $" | {Entry.LogicHell}";

                Entry.Logic = LogicStringConverter.ConvertLogicStringToConditional(logicParser, FinalLogicString, LogicEntry.Key);
                expandedLogicEntries.Add(Entry);

                foreach (var entry in LogicAugments) 
                {
                    ExpandedLogicEntry AugmentedEntry = new() { 
                        Name = Entry.Name + $"[{entry.Key}]",
                        LogicBasic = $"({Entry.Name} | ({entry.Value}))",
                        region = Entry.region,
                        type = Entry.type
                    };
                    AugmentedEntry.Logic = LogicStringConverter.ConvertLogicStringToConditional(logicParser, AugmentedEntry.LogicBasic, LogicEntry.Key);
                    expandedLogicEntries.Add(AugmentedEntry);
                }
            }
            MMRData.LogicFile LogicObject = new()
            {
                GameCode = "LAS",
                Version = 1,
                Logic = []
            };
            foreach(var entry in expandedLogicEntries)
            {
                MMRData.JsonFormatLogicItem logicItem = new()
                {
                    Id = entry.Name,
                    ConditionalItems = entry.Logic,
                    IsTrick = entry.type == "trick"
                };
                LogicUtilities.RemoveRedundantConditionals(logicItem);
                LogicUtilities.MakeCommonConditionalsRequirements(logicItem);
                LogicUtilities.TransformLogicItems(logicItem, (x) => { return x.Replace(":", ", "); });
                if (logicItem.ConditionalItems.All(x => x.Count == 1 && x.All(y => y.StartsWith("setting{logic, "))))
                {
                    logicItem.ConditionalItems = [];
                }

                LogicObject.Logic.Add(logicItem);
            }
            File.WriteAllText(Path.Combine(TestingReferences.GetDevTestingPath(), "LASLogic.json"), LogicObject.ToFormattedJson());

            LogicDictionaryData.LogicDictionary logicDictionary = new LogicDictionaryData.LogicDictionary()
            {
                GameCode = "LAS",
                LogicVersion = 1,
                WinCondition = "kill-nightmare",
            };
        }
    }
}
