using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames.OOTR
{
    public static class ReadAndParseOOTRLogic
    {
        public static string TestFolder = References.TestingPaths.GetDevTestingPath();
        public static string Logicdata = Path.Combine(TestFolder, @"OoT-Randomizer-Dev-R", "data");
        public static string CasualLogic = Path.Combine(Logicdata, "World");
        public static string GlitchedLogic = Path.Combine(Logicdata, "Glitched World");
        public static string[] AllLogicFiles = Directory.GetFiles(CasualLogic);

        public static string CodeFolder = References.TestingPaths.GetDevCodePath();
        public static string OOTRCode = Path.Combine(CodeFolder, @"MMR Tracker V3", "OtherGames", "OOTR");
        public static string OOTRMacroFile = Path.Combine(OOTRCode, @"Macros.json");
        public static string OOTRTricksFile = Path.Combine(OOTRCode, @"LogicTricks.json");
        public static string OOTRItemFile = Path.Combine(OOTRCode, @"OOTRItemList.json");
        public static string OOTRLocationFile = Path.Combine(OOTRCode, @"OOTRLocationList.json");
        public static string OOTRVariableFile = Path.Combine(OOTRCode, @"Variables.json");

        public static Dictionary<string, string> has_projectile = new Dictionary<string, string>
        {
            { "child", "(Slingshot or Boomerang)" },
            { "adult", "(Bow or Hookshot)" },
            { "both", "((Slingshot or Boomerang) and (Bow or Hookshot))" },
            { "either", "(Slingshot or Boomerang or Bow or Hookshot)" },
        };
        public static Dictionary<string, string> can_use = new Dictionary<string, string>
        {
            { "Dins_Fire", "Magic_Meter" },
            { "Farores_Wind", "Magic_Meter" },
            { "Nayrus_Love", "Magic_Meter" },
            { "Lens_of_Truth", "Magic_Meter" },
            { "Bow", "is_adult" },
            { "Megaton_Hammer", "is_adult" },
            { "Iron_Boots", "is_adult" },
            { "Hover_Boots", "is_adult" },
            { "Hookshot", "is_adult" },
            { "Longshot", "is_adult" },
            { "Silver_Gauntlets", "is_adult" },
            { "Golden_Gauntlets", "is_adult" },
            { "Goron_Tunic", "is_adult" },
            { "Zora_Tunic", "is_adult" },
            { "Scarecrow", "is_adult" },
            { "Distant_Scarecrow", "is_adult" },
            { "Mirror_Shield", "is_adult" },
            { "Slingshot", "is_child" },
            { "Boomerang", "is_child" },
            { "Kokiri_Sword", "is_child" },
            { "Sticks", "is_child" },
            { "Deku_Shield", "is_child" },
            { "Fire_Arrows", "is_adult and Bow and Magic_Meter" },
            { "Light_Arrows", "is_adult and Bow and Magic_Meter" },
            { "Blue_Fire_Arrows", "is_adult and Bow and Magic_Meter" },
        };

        public static List<OOTRObjects.LogicFile> ReadAndParse()
        {
            var logicFiles = GetOOTRLogic();
            ParseATFunctions(logicFiles);
            HandleFunctions(logicFiles);
            Cleanup(logicFiles);
            writeNewLogic(logicFiles);
            return logicFiles;
        }

        public static List<OOTRObjects.LogicFile> GetOOTRLogic()
        {
            List<OOTRObjects.LogicFile> LogicFiles = new List<OOTRObjects.LogicFile>();

            foreach (var LogicFile in AllLogicFiles)
            {
                var Data = File.ReadAllLines(LogicFile);
                Data =  RemoveComments(Data);
                OOTRObjects.LogicFile logicFile = new OOTRObjects.LogicFile();
                logicFile.FilePath = LogicFile;
                logicFile.LogicalRegions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OOTRObjects.LogicalRegion>>(string.Join("", Data));
                LogicFiles.Add(logicFile);
            }

            OOTRObjects.LogicFile MacroFile = new OOTRObjects.LogicFile();
            MacroFile.FilePath = OOTRMacroFile;
            OOTRObjects.LogicalRegion MacroRegion = new OOTRObjects.LogicalRegion { 
                region_name = null, 
                events = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(OOTRMacroFile)) 
            };
            MacroFile.LogicalRegions = new List<OOTRObjects.LogicalRegion> { MacroRegion };
            LogicFiles.Add(MacroFile);
            return LogicFiles;
        }

        public static void Cleanup(List<OOTRObjects.LogicFile> logicFiles)
        {
            foreach (var File in logicFiles)
            {
                foreach (var region in File.LogicalRegions)
                {
                    Loopsets(region.locations, File, region);
                    Loopsets(region.exits, File, region, true);
                    Loopsets(region.events, File, region);
                }
            }

            void Loopsets(Dictionary<string, string> List, OOTRObjects.LogicFile File, OOTRObjects.LogicalRegion Region, bool isExit = false)
            {
                foreach (var loc in List?.Keys?.ToList() ?? new List<string>())
                {
                    if (Region.dungeon is not null && !File.FilePath.EndsWith("Bosses.json"))
                    {
                        string Layout = File.IsMQ ? "MQ" : "Vanilla";
                        string DungeonArea = $"{Region.dungeon} Layout == {Layout}";
                        List[loc] = $"{DungeonArea} and ({List[loc]})";
                    }
                    if (!isExit && Region.region_name is not null) { List[loc] = $"{Region.region_name} and ({List[loc]})"; }
                    List[loc] = LogicStringParser.ReplaceLogicOperatorsWithMathOperators(List[loc], new string[] { " or " }, new string[] { " and " });
                    List[loc] = List[loc].Replace("'", "");
                }
            }
        }

        public static void ParseATFunctions(List<OOTRObjects.LogicFile> logicFiles)
        {
            foreach (var File in logicFiles)
            {
                foreach (var region in File.LogicalRegions)
                {
                    Loopsets(region.locations, region);
                    Loopsets(region.exits, region);
                    Loopsets(region.events, region);
                }
            }

            void Loopsets(Dictionary<string, string> List, OOTRObjects.LogicalRegion Region)
            {
                foreach (var loc in List?.Keys?.ToList() ?? new List<string>())
                {
                    var functions = LogicStringParser.GetFunctionsFromLogicString(List[loc]);
                    foreach (var function in functions.Where(x => x.Funtion == "at"))
                    {
                        SplitATFunction(function.Parameters, out string area, out string logic, out string NewLogic);
                        if (logic == "True") { List[loc] = List[loc].Replace(function.ToString(), area); }
                        else { List[loc] = List[loc].Replace(function.ToString(), NewLogic); }
                        Debug.WriteLine($"At Function: {function.ToString()}");
                    }
                }
            }

            void SplitATFunction(string Params, out string Area, out string logic, out string Cleaned)
            {
                int FirstComma = Params.IndexOf(',');
                //var pieces = Params.Split(new[] { ',' }, 2);
                Area = Params[..FirstComma].Trim()[1..].Trim('\'');
                logic = Params[FirstComma..].Trim()[1..].Trim()[0..^1];
                Cleaned = $"({Area} and ({logic}))";
            }
        }

        public static void HandleFunctions(List<OOTRObjects.LogicFile> logicFiles)
        {
            foreach (var File in logicFiles)
            {
                foreach (var region in File.LogicalRegions)
                {
                    Loopsets(region.locations, region);
                    Loopsets(region.exits, region);
                    Loopsets(region.events, region);
                }
            }
            void Loopsets(Dictionary<string, string> List, OOTRObjects.LogicalRegion Region)
            {
                foreach (var loc in List?.Keys?.ToList() ?? new List<string>())
                {
                    var functions = LogicStringParser.GetFunctionsFromLogicString(List[loc]);
                    foreach (var function in functions)
                    {
                        if (function.Funtion == "here")
                        {
                            Debug.WriteLine($"Here Function: {function.ToString()}");
                            Debug.WriteLine($"  -Params: {function.Parameters}");
                            List[loc] = List[loc].Replace(function.ToString(), function.Parameters); 
                        }
                    }
                    functions = LogicStringParser.GetFunctionsFromLogicString(List[loc]);
                    foreach (var function in functions)
                    {
                        if (function.Funtion == "can_play")
                        {
                            Debug.WriteLine($"Can_play Function: {function.ToString()}");
                            List[loc] = List[loc].Replace(function.ToString(), $"(Ocarina and {function.ParametersTrimmed})");
                        }
                    }
                    functions = LogicStringParser.GetFunctionsFromLogicString(List[loc]);
                    foreach (var function in functions)
                    {
                        if (function.Funtion == "has_projectile" && has_projectile.ContainsKey(function.ParametersTrimmed))
                        {
                            Debug.WriteLine($"Has projectile Function: {function.ToString()}");
                            List[loc] = List[loc].Replace(function.ToString(), has_projectile[function.ParametersTrimmed]);
                        }
                    }
                    functions = LogicStringParser.GetFunctionsFromLogicString(List[loc]);
                    foreach (var function in functions)
                    {
                        if (function.Funtion == "can_use" && can_use.ContainsKey(function.ParametersTrimmed))
                        {
                            Debug.WriteLine($"Can Use Function: {function.ToString()}");
                            List[loc] = List[loc].Replace(function.ToString(), $"({function.ParametersTrimmed} and {can_use[function.ParametersTrimmed]})");
                        }
                    }
                }
            }
        }

        public static void writeNewLogic(object Logic)
        {
            var NewLogic = Newtonsoft.Json.JsonConvert.SerializeObject(Logic, Testing._NewtonsoftJsonSerializerOptions);
            File.WriteAllText(Path.Combine(References.TestingPaths.GetDevTestingPath(), "NewLogic.json"), NewLogic);
        }

        public static string[] RemoveComments(string[] LogicFile)
        {
            List<string> CleanedLines = new List<string>();
            foreach (var i in LogicFile)
            {
                string line = i;
                if (line.Trim().StartsWith("#")) { continue; }
                else if (line.Contains('#')) { line = line[0..line.IndexOf("#")]; }
                line = Regex.Replace(line, @"\s+", " ", RegexOptions.Multiline);
                CleanedLines.Add(line);
            }
            return CleanedLines.ToArray();
        }
    }
}
