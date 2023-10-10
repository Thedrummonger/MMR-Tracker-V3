using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames.TPRV2
{
    public static class ReadAndParse
    {
        public static void ReadLines()
        {
            LogicStringParser parser = new LogicStringParser();

            string CodePath = Path.Combine(References.TestingPaths.GetDevCodePath(), "MMR Tracker V3");
            string TPRFiles = Path.Combine(CodePath, "OtherGames", "TPRV2");
            string MacroFile = Path.Combine(TPRFiles, "Macros.txt");
            var MacroLines = File.ReadLines(MacroFile);

            MMRData.LogicFile logicFile = new MMRData.LogicFile();
            logicFile.Logic = new List<MMRData.JsonFormatLogicItem>();

            string CurrentString = string.Empty;
            bool ReadingFuctionName = false;
            bool InFunction = false;
            int BracketLevel = 0;
            string CurrentFucntion = string.Empty;
            string CurrentFucntionData = string.Empty;

            foreach(var Rawline in MacroLines)
            {
                string line = Rawline;
                if (line.Trim().StartsWith("//")) { continue; }
                if (line.Contains("//"))
                {
                    line = line[..line.IndexOf("//")];
                }
                foreach (var C in line)
                {
                    CurrentString += C;

                    if (CurrentString.EndsWith("public static bool ")) { ReadingFuctionName = true; }
                    if (ReadingFuctionName && CurrentString.EndsWith("(")) { ReadingFuctionName = false; }
                    if (ReadingFuctionName) { CurrentFucntion += C; }

                    if (CurrentFucntion != string.Empty && C == '{') 
                    { 
                        InFunction = true; 
                    }
                    if (InFunction)
                    {
                        if (C =='{') { BracketLevel++; }
                        else if (C == '}') { BracketLevel--; }
                        else { CurrentFucntionData += C; }
                        if (BracketLevel == 0)
                        {
                            CommitFunction(CurrentFucntion, CurrentFucntionData);
                            CurrentFucntion = string.Empty;
                            CurrentFucntionData = string.Empty;
                            InFunction = false;
                        }
                    }
                }
            }

            void CommitFunction(string name, string function)
            {
                string CleanFunc = function.TrimSpaces().Trim();
                if (!CleanFunc.StartsWith("return")) { Debug.WriteLine($"Fucntion {name} was not parsable"); return; }
                CleanFunc = CleanFunc["return".Length..^1].Trim();

                //Hopefully a better way to do this in the future.
                for (int i = 1; i < 10; i++) 
                { 
                    CleanFunc = CleanFunc.Replace($") >= {i}", $", {i})");
                    CleanFunc = CleanFunc.Replace($") > {i}", $", {i})");
                }

                logicFile.Logic.Add(new MMRData.JsonFormatLogicItem { Id = name, ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(parser, CleanFunc, name) });
            }

            string LogicOutput = Path.Combine(References.TestingPaths.GetDevTestingPath(), "TPRTesting", "MacroLogic.json");
            File.WriteAllText(LogicOutput, logicFile.ToString());
        }
    }
}
