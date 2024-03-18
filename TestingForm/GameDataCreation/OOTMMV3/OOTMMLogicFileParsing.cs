using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MMRData;
using static TestingForm.GameDataCreation.OOTMMV3.OOTMMDataClasses;
using MMR_Tracker_V3.TrackerObjects;

namespace TestingForm.GameDataCreation.OOTMMV3
{
    internal class OOTMMLogicFileParsing(OOTMMDataGenerator generator)
    {
        public Dictionary<string, OOTMMLocationEntry> LogicEntries = [];
        public Dictionary<string, OOTMMLogicFunction> MacroFunctions = [];
        public void ReadAndParseWorldFiles()
        {
            foreach (var worldFile in Directory.GetFiles(OOTMMPaths.OOTWorldFolderPath))
            {
                var Json = TestingUtility.ConvertYamlStringToJsonString(File.ReadAllText(worldFile), true);
                Dictionary<string, OOTMMLocationArea> locationArea = JsonConvert.DeserializeObject<Dictionary<string, OOTMMLocationArea>>(Json);
                foreach (var area in locationArea)
                {
                    foreach (var locLogic in area.Value.locations) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "OOT", false, OOTMMDataType.location); }
                    foreach (var locLogic in area.Value.exits) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "OOT", false, OOTMMDataType.Exit); }
                    foreach (var locLogic in area.Value.events) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "OOT", false, OOTMMDataType.Event); }
                }
            }
            foreach (var worldFile in Directory.GetFiles(OOTMMPaths.MQWorldFolderPath))
            {
                var Json = TestingUtility.ConvertYamlStringToJsonString(File.ReadAllText(worldFile), true);
                Dictionary<string, OOTMMLocationArea> locationArea = JsonConvert.DeserializeObject<Dictionary<string, OOTMMLocationArea>>(Json);
                foreach (var area in locationArea)
                {
                    foreach (var locLogic in area.Value.locations) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "OOT", true, OOTMMDataType.location); }
                    foreach (var locLogic in area.Value.exits) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "OOT", true, OOTMMDataType.Exit); }
                    foreach (var locLogic in area.Value.events) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "OOT", true, OOTMMDataType.Event); }
                }
            }
            foreach (var worldFile in Directory.GetFiles(OOTMMPaths.MMWorldFolderPath))
            {
                var Json = TestingUtility.ConvertYamlStringToJsonString(File.ReadAllText(worldFile), true);
                Dictionary<string, OOTMMLocationArea> locationArea = JsonConvert.DeserializeObject<Dictionary<string, OOTMMLocationArea>>(Json);
                foreach (var area in locationArea)
                {
                    foreach (var locLogic in area.Value.locations) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "MM", false, OOTMMDataType.location); }
                    foreach (var locLogic in area.Value.exits) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "MM", false, OOTMMDataType.Exit); }
                    foreach (var locLogic in area.Value.events) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "MM", false, OOTMMDataType.Event); }
                }
            }
            var CommonMacroJson = TestingUtility.ConvertYamlStringToJsonString(File.ReadAllText(OOTMMPaths.CommonMacroFile), true);
            Dictionary<string, string> CommonMacros = JsonConvert.DeserializeObject<Dictionary<string, string>>(CommonMacroJson);
            var MMMacroJson = TestingUtility.ConvertYamlStringToJsonString(File.ReadAllText(OOTMMPaths.MMMacroFile), true);
            Dictionary<string, string> MMMacros = JsonConvert.DeserializeObject<Dictionary<string, string>>(MMMacroJson);
            var OOTMacroJson = TestingUtility.ConvertYamlStringToJsonString(File.ReadAllText(OOTMMPaths.OOTMacroFile), true);
            Dictionary<string, string> OOTMacros = JsonConvert.DeserializeObject<Dictionary<string, string>>(OOTMacroJson);

            foreach (var macro in MMMacros)
            {
                AddMacroEntry(macro.Key, macro.Value, "MM");
            }
            foreach (var macro in OOTMacros)
            {
                AddMacroEntry(macro.Key, macro.Value, "OOT");
            }
            foreach (var macro in CommonMacros)
            {
                AddMacroEntry(macro.Key, macro.Value, "MM");
                AddMacroEntry(macro.Key, macro.Value, "OOT");
            }
        }

        public void AddEntriesToLogicFile()
        {
            foreach (var i in LogicEntries)
            {
                generator.LogicFile.Logic.Add(new JsonFormatLogicItem
                {
                    Id = i.Key,
                    ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(generator.LogicStringParser, i.Value.Logic, i.Key)
                });
                LogicUtilities.RemoveRedundantConditionals(generator.LogicFile.Logic.Last());
            }
        }

        public void AddMacroEntry(string Key, string Logic, string GameCode)
        {
            var LogicFunction = OOTMMUtility.IsLogicFunction(Key);
            if (LogicFunction != null)
            {
                LogicFunction.Logic = Logic;
                LogicFunction.function = OOTMMUtility.AddGameCodeToLogicID(LogicFunction.function, GameCode);
                MacroFunctions.Add(LogicFunction.function, LogicFunction);
                return;
            }
            string ID = OOTMMUtility.AddGameCodeToLogicID(Key, GameCode);
            LogicEntries.Add(ID, new OOTMMLocationEntry { Key = ID, Logic = Logic });
        }

        public void AddLogicEntry(OOTMMLocationArea AreaData, string Area, string ID, string Logic, string GameCode, bool ISMQ, OOTMMDataType Type)
        {
            Area = OOTMMUtility.AddGameCodeToLogicID(Area, GameCode, false);
            ID = OOTMMUtility.AddGameCodeToLogicID(ID, GameCode, Type != OOTMMDataType.location && Type != OOTMMDataType.Exit);
            if (Type == OOTMMDataType.Exit) { ID = $"{Area} => {ID}"; }
            else if (Type == OOTMMDataType.Event) { ID = $"{ID}_EVENT"; }

            string FinalLogic = $"({Area}{OOTMMUtility.GetMQString(AreaData, ISMQ, GameCode)} && ({Logic}))";
            if (LogicEntries.TryGetValue(ID, out OOTMMLocationEntry? value))
            {
                value.Logic += $" || {FinalLogic}";
            }
            else
            {
                LogicEntries[ID] = new OOTMMLocationEntry() { Key = ID, Logic = FinalLogic };
            }
        }
        public void ParseDownFunctions()
        {
            bool ChangesMade = true;
            while (ChangesMade)
            {
                ChangesMade = false;
                foreach (var i in generator.LogicFile.Logic)
                {
                    string GameCode = OOTMMUtility.GetGamecode(i.Id);
                    LogicUtilities.TransformLogicItems(i, (x) =>
                    {
                        var LogicFuncData = OOTMMUtility.IsLogicFunction(x);
                        if (LogicFuncData is not null && MacroFunctions.TryGetValue(OOTMMUtility.AddGameCodeToLogicID(LogicFuncData.function, GameCode), out OOTMMLogicFunction Function))
                        {
                            ChangesMade = true;
                            if (Function.Param.Length != LogicFuncData.Param.Length) { throw new Exception($"Param Length MissMatch {x}"); }
                            var Params = Function.Param.Zip(LogicFuncData.Param);
                            var FunctionLogic = Function.Logic;
                            foreach (var param in Params)
                            {
                                FunctionLogic = $"({OOTMMUtility.ReplaceVariableWithParam(FunctionLogic, param.First, param.Second)})";
                            }
                            return FunctionLogic;
                        }
                        return x;
                    });
                    LogicUtilities.TransformLogicItems(i, (x) =>
                    {
                        var LogicFuncData = OOTMMUtility.IsLogicFunction(x);
                        if (LogicFuncData is not null && ParseManualFunction(i, x, LogicFuncData, GameCode, out string NewLogicItem))
                        {
                            ChangesMade = true;
                            return NewLogicItem;
                        }
                        return x;
                    });

                    i.ConditionalItems = LogicStringConverter.ReParseConditional(generator.LogicStringParser, i.ConditionalItems, i.Id);
                    LogicUtilities.RemoveRedundantConditionals(i);
                }
            }
        }

        private bool ParseManualFunction(JsonFormatLogicItem LogicItem, string CurrentLogicItem, OOTMMLogicFunction logicFuncData, string GameCode, out string NewLogicItem)
        {
            NewLogicItem = CurrentLogicItem;
            if (logicFuncData.function == "has")
            {
                NewLogicItem = $"{OOTMMUtility.AddGameCodeToLogicID(logicFuncData.Param[0], GameCode)}";
                if (logicFuncData.Param.Length > 1)
                {
                    if (int.TryParse(logicFuncData.Param[1], out int Count))
                    {
                        NewLogicItem += $", {Count}";
                        if (Count < 1) { NewLogicItem = "True"; }
                    }
                    else if (OOTMMUtility.IsMathExpression(logicFuncData.Param[1], out int SolvedEquation))
                    {
                        NewLogicItem += $", {SolvedEquation}";
                        if (SolvedEquation < 1) { NewLogicItem = "True"; }
                    }
                    else
                    {
                        throw new Exception($"${logicFuncData.Param[1]} was not a valid item count");
                    }
                }
            }
            else if (logicFuncData.function.In("trick", "glitch"))
            {
                NewLogicItem = $"trick{{{OOTMMUtility.AddGameCodeToLogicID(logicFuncData.Param[0], GameCode)}}}";
            }
            else if (logicFuncData.function == "event")
            {
                NewLogicItem = $"{OOTMMUtility.AddGameCodeToLogicID(logicFuncData.Param[0], GameCode)}_EVENT";
            }
            else if (logicFuncData.function == "setting" || logicFuncData.function == "!setting")
            {
                string Modifier = logicFuncData.function.StartsWith('!') ? ", False" : "";
                NewLogicItem = $"setting{{{string.Join(", ", logicFuncData.Param)}{Modifier}}}";
            }
            else if (logicFuncData.function == "cond")
            {
                if (logicFuncData.Param.Length != 3) { throw new Exception($"COND Function had Illegal number of args\n{logicFuncData.ToFormattedJson()}"); }
                NewLogicItem = OOTMMUtility.ParseCondFunc(logicFuncData, LogicItem.Id, generator.LogicStringParser);
            }
            else if (logicFuncData.function == "renewable")
            {
                NewLogicItem = $"renewable{{{OOTMMUtility.AddGameCodeToLogicID(logicFuncData.Param[0], GameCode)}}}";
            }
            else if (logicFuncData.function == "license")
            {
                NewLogicItem = $"renewable{{{OOTMMUtility.AddGameCodeToLogicID(logicFuncData.Param[0], GameCode)}, False}}";
            }
            else if (logicFuncData.function == "price")
            {
                NewLogicItem = $"price{{{LogicItem.Id}, {logicFuncData.Param[2]}}}";
            }
            else if (logicFuncData.function.In("mm_time", "oot_time"))
            {
                NewLogicItem = $"time{{{logicFuncData.function}, {logicFuncData.RawParam}}}";
            }
            else if (logicFuncData.function == "age")
            {
                NewLogicItem = $"age_{logicFuncData.RawParam}";
            }
            else if (logicFuncData.function == "has_pond_fish")
            {
                NewLogicItem = $"{string.Join("_", logicFuncData.Param)}, 1";
            }
            else if (logicFuncData.function == "masks")
            {
                NewLogicItem = $"MM_masks, {logicFuncData.RawParam}";
            }
            else if (logicFuncData.function == "special")
            {
                NewLogicItem = $"HAS_SPECIAL_{logicFuncData.RawParam}";
            }
            return NewLogicItem != CurrentLogicItem;
        }

        public void AddMissingGameCodes()
        {
            HashSet<string> ChangedEntries = [];
            foreach (var i in generator.LogicFile.Logic)
            {
                string GameCode = OOTMMUtility.GetGamecode(i.Id);
                LogicUtilities.TransformLogicItems(i, (x) =>
                {
                    if (x.In("age_adult", "age_child", "is_goal_triforce")) { return x; }
                    if (OOTMMUtility.HasGamecode(x)) { return x; }
                    if (LogicFunctions.IsLogicFunction(x)) { return x; }
                    if (bool.TryParse(x, out _)) { return x; }
                    ChangedEntries.Add(x);
                    string finalID = x;
                    bool Negate = false;
                    if (finalID.StartsWith('!')) { finalID = finalID[1..]; Negate = true; }
                    finalID = OOTMMUtility.AddGameCodeToLogicID(finalID, GameCode);
                    if (Negate) { finalID = $"!{finalID}"; }
                    return finalID;
                });
            }
            //Utility.PrintObjectToConsole(ChangedEntries);
        }

        public void HandleNegations()
        {
            foreach (var i in generator.LogicFile.Logic)
            {
                string GameCode = OOTMMUtility.GetGamecode(i.Id);
                LogicUtilities.TransformLogicItems(i, (x) =>
                {
                    if (!x.StartsWith('!')) { return x; }
                    string finalID = x; 
                    finalID = finalID[1..];
                    finalID = $"available{{{finalID}, false}}";
                    return finalID;
                });
            }
        }

        public void AddMissingLogic()
        {
            generator.LogicFile.Logic.Add(LogicUtilities.CreateInaccessibleLogic("MM_BOMBCHU_EVENT"));
            generator.LogicFile.Logic.Add(LogicUtilities.CreateInaccessibleLogic("MM_BOMBS_EVENT"));
            generator.LogicFile.Logic.Add(LogicUtilities.CreateInaccessibleLogic("MM_CHATEAU_EVENT"));
        }
    }
}
