using MMR_Tracker_V3;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TestingForm.GameDataCreation.OOTMMV3.DataClasses;

namespace TestingForm.GameDataCreation.OOTMMV3
{
    internal class OOTMMDataGenerator
    {
        public Dictionary<string, OOTMMLocationEntry> LogicEntries = [];
        public Dictionary<string, OOTMMLogicFunction> MacroFunctions = [];
        public LogicStringParser LogicStringParser = new();
        public MMRData.LogicFile LogicFile = new() { GameCode = "OOTMM", Logic = [], Version = 3 };
        public void GenerateOOTMMData()
        {
            foreach(var worldFile in Directory.GetFiles(Paths.OOTWorldFolderPath))
            {
                var Json = TestingUtility.ConvertYamlStringToJsonString(File.ReadAllText(worldFile), true);
                Dictionary<string, OOTMMLocationArea> locationArea = JsonConvert.DeserializeObject<Dictionary<string, OOTMMLocationArea>>(Json);
                foreach(var area in locationArea)
                {
                    foreach (var locLogic in area.Value.locations) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "OOT", false, OOTMMDataType.location); }
                    foreach (var locLogic in area.Value.exits) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "OOT", false, OOTMMDataType.Exit); }
                    foreach (var locLogic in area.Value.exits) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "OOT", false, OOTMMDataType.Macro); }
                }
            }
            foreach (var worldFile in Directory.GetFiles(Paths.MQWorldFolderPath))
            {
                var Json = TestingUtility.ConvertYamlStringToJsonString(File.ReadAllText(worldFile), true);
                Dictionary<string, OOTMMLocationArea> locationArea = JsonConvert.DeserializeObject<Dictionary<string, OOTMMLocationArea>>(Json);
                foreach (var area in locationArea)
                {
                    foreach (var locLogic in area.Value.locations) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "OOT", true, OOTMMDataType.location); }
                    foreach (var locLogic in area.Value.exits) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "OOT", true, OOTMMDataType.Exit); }
                    foreach (var locLogic in area.Value.exits) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "OOT", true, OOTMMDataType.Macro); }
                }
            }
            foreach (var worldFile in Directory.GetFiles(Paths.MMWorldFolderPath))
            {
                var Json = TestingUtility.ConvertYamlStringToJsonString(File.ReadAllText(worldFile), true);
                Dictionary<string, OOTMMLocationArea> locationArea = JsonConvert.DeserializeObject<Dictionary<string, OOTMMLocationArea>>(Json);
                foreach (var area in locationArea)
                {
                    foreach (var locLogic in area.Value.locations) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "MM", false, OOTMMDataType.location); }
                    foreach (var locLogic in area.Value.exits) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "MM", false, OOTMMDataType.Exit); }
                    foreach (var locLogic in area.Value.exits) { AddLogicEntry(area.Value, area.Key, locLogic.Key, locLogic.Value, "MM", false, OOTMMDataType.Macro); }
                }
            }

            foreach(var i in LogicEntries)
            {
                LogicFile.Logic.Add(new MMRData.JsonFormatLogicItem
                {
                    Id = i.Key,
                    ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(LogicStringParser, i.Value.Logic, i.Key)
                });
                LogicUtilities.RemoveRedundantConditionals(LogicFile.Logic.Last());
            }

            foreach(var i in LogicFile.Logic)
            {
                LogicUtilities.TransformLogicItems(i, (x) =>
                {
                    var LogicFuncData = OOTMMUtility.IsLogicFunction(x);
                    if (LogicFuncData is not null && MacroFunctions.TryGetValue(LogicFuncData.function, out OOTMMLogicFunction Function))
                    {
                        if (Function.Param.Length != LogicFuncData.Param.Length) { throw new Exception($"Param Length MissMatch {x}"); }
                        var Params = Function.Param.Zip(LogicFuncData.Param);
                        foreach ( var param in Params)
                        {
                            var NewLine = OOTMMUtility.ReplaceVariableWithParam(param.First, param.Second, Function.Logic);
                        }
                    }
                    return x;
                });
            }

            File.WriteAllText(Path.Combine(Paths.OOTMMTestingFolderPath, "OOTMMLogicV3.json"), LogicFile.ToFormattedJson());
        }

        public void AddLogicEntry(OOTMMLocationArea AreaData, string Area, string ID, string Logic, string GameCode, bool ISMQ, OOTMMDataType Type)
        {
            Area = OOTMMUtility.AddGameCodeToLogicID(Area, GameCode);
            ID = OOTMMUtility.AddGameCodeToLogicID(ID, GameCode);
            if (Type == OOTMMDataType.Exit) { ID = $"{Area} => {ID}"; }
            else if (Type == OOTMMDataType.Macro) { ID = $"EVENT {ID}"; }

            string FinalLogic = $"({Area}{OOTMMUtility.GetMQString(AreaData, ISMQ)} && ({Logic}))";
            if (LogicEntries.TryGetValue(ID, out OOTMMLocationEntry? value))
            {
                value.Logic += $" || {FinalLogic}";
            }
            else
            {
                LogicEntries[ID] = new OOTMMLocationEntry() { Key = ID, Logic = FinalLogic };
            }
        }
    }
}
