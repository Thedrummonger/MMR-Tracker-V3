using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.datamodel;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.OOTMMUtil;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.FunctionParsing;

namespace MMR_Tracker_V3.OtherGames.OOTMMV2
{
    public static class GenData
    {
        public static LogicStringParser OOTMMLogicStringParser = new LogicStringParser();
        public static void ReadData()
        {
            //Shared Data
            string OOTMMCorePath = Path.Combine(References.TestingPaths.GetDevTestingPath(), "OoTMM-develop", "packages", "core");
            string OOTMMTestingFolder = Path.Combine(References.TestingPaths.GetDevTestingPath(), "OOTMMTesting");

            //OOT Data Paths
            string OOTData = Path.Combine(OOTMMCorePath, "data", "oot");
            string OOTWorld = Path.Combine(OOTData, "world");
            string OOTMQWorld = Path.Combine(OOTData, "world_mq");

            //OOT Data Files
            string OOTEntrancesFile = Path.Combine(OOTData, "entrances.csv");
            string OOTMacroFile = Path.Combine(OOTData, "macros.yml");
            string OOTPoolFile = Path.Combine(OOTData, "pool.csv");
            string OOTHintFile = Path.Combine(OOTData, "hints.csv");

            //MM Data
            string MMData = Path.Combine(OOTMMCorePath, "data", "mm");
            string MMWorld = Path.Combine(MMData, "world");

            //MM Data Files
            string MMEntrancesFile = Path.Combine(MMData, "entrances.csv");
            string MMMacroFile = Path.Combine(MMData, "macros.yml");
            string MMPoolFile = Path.Combine(MMData, "pool.csv");
            string MMHintFile = Path.Combine(MMData, "hints.csv");

            //Shared Data Files
            string SHAREDMacroFile = Path.Combine(OOTMMCorePath, "data", "macros.yml");

            LogicDictionaryData.LogicDictionary logicDictionaryData = new LogicDictionaryData.LogicDictionary() { GameCode = "OOTMM", RootArea = "OOT SPAWN", LogicVersion = 2 };

            MMRData.LogicFile LogicFile = ReadAndParseLogicFile(OOTWorld, OOTMQWorld, MMWorld, OOTMacroFile, MMMacroFile, SHAREDMacroFile);

            CreateStaticCountFunctions(LogicFile);
            ParseLogicFunctions(LogicFile);

            File.WriteAllText(Path.Combine(OOTMMTestingFolder,"LogicFile.json"), JsonConvert.SerializeObject(LogicFile, Formatting.Indented));
            Debug.WriteLine("Finished");
            EvalLogicEntryTypes(LogicFile);
            Debug.WriteLine(LogicEditing.IsLogicFunction($"Test(Func)", out string Func, out _, new('(', ')')));
        }

        private static void EvalLogicEntryTypes(MMRData.LogicFile LogicFile)
        {
            List<string> Functions = new List<string>();
            foreach (string Entry in LogicFile.Logic.SelectMany(x => x.ConditionalItems.SelectMany(x => x)))
            {
                if (LogicEditing.IsLogicFunction(Entry, out string Func, out _, new('(', ')')) && !Functions.Contains(Func))
                {
                    Functions.Add(Func);
                }
            }
            Debug.WriteLine(JsonConvert.SerializeObject(Functions, Formatting.Indented));
        }

        private static MMRData.LogicFile ReadAndParseLogicFile(string OOTWorld, string OOTMQWorld, string MMWorld, string OOTMacroFile, string MMMacroFile, string SHAREDMacroFile)
        {
            MMRData.LogicFile LogicFile = new MMRData.LogicFile() { GameCode = "OOTMM", Version = 2 };
            LogicFile.Logic = new List<MMRData.JsonFormatLogicItem>();

            string[] OOTWorldFiles = Directory.GetFiles(OOTWorld);
            string[] OOTMQWorldFiles = Directory.GetFiles(OOTMQWorld);
            string[] MMWorldFiles = Directory.GetFiles(MMWorld);

            AddFileData(OOTWorldFiles, "OOT");
            AddFileData(OOTMQWorldFiles, "OOT", true);
            AddFileData(MMWorldFiles, "MM");

            AddMacroFile(OOTMacroFile, "OOT");
            AddMacroFile(MMMacroFile, "MM");
            AddMacroFile(SHAREDMacroFile, "OOT");
            AddMacroFile(SHAREDMacroFile, "MM");

            return LogicFile;

            void AddMacroFile(string MacroFile, string GameCode)
            {
                var FileOBJ = JsonConvert.DeserializeObject<Dictionary<string, string>>(Utility.ConvertYamlStringToJsonString(File.ReadAllText(MacroFile)));
                foreach(var item in FileOBJ)
                {
                    if (LogicEditing.IsLogicFunction(item.Key, out string Func, out _, new('(', ')')))
                    {
                        Debug.WriteLine($"Skipping Function Macro {item.Key}");
                        continue;
                    }
                    string ID = $"{GameCode}_{item.Key}";
                    List<List<string>> ConditionalLogic = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, item.Value);
                    MMRData.JsonFormatLogicItem NewLogic = new() { Id = ID, ConditionalItems = ConditionalLogic };
                    LogicUtilities.RemoveRedundantConditionals(NewLogic);
                    LogicFile.Logic.Add(NewLogic);
                }
            }

            void AddFileData(string[] files, string GameCode, bool MQ = false)
            {
                foreach (var WorldFile in files)
                {
                    var FileOBJ = JsonConvert.DeserializeObject<Dictionary<string, MMROOTLogicEntry>>(Utility.ConvertYamlStringToJsonString(File.ReadAllText(WorldFile)));
                    foreach (var Area in FileOBJ)
                    {
                        foreach (var Location in Area.Value.locations)
                        {
                            string ID = $"{GameCode} location {Location.Key}";
                            List<List<string>> ConditionalLogic = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, Location.Value);
                            AddLogicEntry(LogicFile, ID, ConditionalLogic, WorldFile, Area);

                        }
                        foreach (var Location in Area.Value.exits)
                        {
                            string ID = $"{GameCode} exit {Area.Key} => {Location.Key}";
                            List<List<string>> ConditionalLogic = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, Location.Value);
                            AddLogicEntry(LogicFile, ID, ConditionalLogic, WorldFile, Area);
                        }
                        foreach (var Location in Area.Value.events)
                        {
                            string ID = $"{GameCode} event {Location.Key}";
                            List<List<string>> ConditionalLogic = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, Location.Value);
                            AddLogicEntry(LogicFile, ID, ConditionalLogic, WorldFile, Area);
                        }
                    }
                }

                bool HasMQEquivilent(string WorldFile)
                {
                    return OOTMQWorldFiles.Any(x => x == WorldFile.Replace(".json", "_mq.json"));
                }

                void AddLogicEntry(MMRData.LogicFile LogicFile, string ID, List<List<string>> ConditionalLogic, string WorldFile, KeyValuePair<string, MMROOTLogicEntry> Area)
                {
                    var Duplicates = LogicFile.Logic.FirstOrDefault(x => x.Id == ID);

                    foreach (var i in ConditionalLogic) { i.Add($"{GameCode} area {Area.Key}"); }
                    if (HasMQEquivilent(WorldFile)) { foreach (var i in ConditionalLogic) { i.Add($"setting({Area.Value.dungeon}_layout, {(MQ ? "MQ" : "Vanilla")})"); } }

                    if (Duplicates is null) 
                    {
                        MMRData.JsonFormatLogicItem NewLogic = new() { Id = ID, ConditionalItems = ConditionalLogic };
                        LogicUtilities.RemoveRedundantConditionals(NewLogic);
                        LogicFile.Logic.Add(NewLogic); 
                    }
                    else 
                    { 
                        Duplicates.ConditionalItems.AddRange(ConditionalLogic);
                        LogicUtilities.RemoveRedundantConditionals(Duplicates);
                    }
                }
            }
        }
    }
}
