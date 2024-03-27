using MathNet.Symbolics;
using MMR_Tracker_V3;
using MMR_Tracker_V3.Logic;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static TestingForm.GameDataCreation.OOTMMV3.OOTMMDataClasses;

namespace TestingForm.GameDataCreation.OOTMMV3
{
    public static class OOTMMUtility
    {
        public static string ReplaceVariableWithParam(string LogicLine, string Param, string Value)
        {
            return Regex.Replace(LogicLine, @$"\b{Param}\b", Value);
        }
        public static OOTMMLogicFunction? IsLogicFunction(string Key)
        {
            if (!Key.Contains('(') || !Key.EndsWith(')') || Key.StartsWith('(')) { return null; }
            if (Key.Count(x => x == '(') != Key.Count(x => x == ')')) { return null; }

            var Sections = Key.SplitOnce('(');
            string Func = Sections.Item1;
            string Parm = Sections.Item2[..^1];
            return new OOTMMLogicFunction(Func, Parm);
        }

        public static bool IsMathExpression(string input, out int result)
        {
            result = 0;
            Expression LogicSet = Infix.ParseOrUndefined(input);
            if (LogicSet == null) { return false; }
            //var Output = Algebraic.Expand(LogicSet);
            var Solved = Infix.Format(LogicSet);
            if (!int.TryParse(Solved, out result)) { return false; }
            return true;
        }

        public static string GetMQString(OOTMMLocationArea data, bool ISMQ, string GameCode)
        {
            if (data.boss || string.IsNullOrWhiteSpace(data.dungeon) || GameCode == "MM" || data.dungeon.In("TCG", "GF", "Tower")) { return string.Empty; }
            return $" && setting(MasterQuest, {data.dungeon}, {ISMQ})";
        }

        public static string AddGameCodeToLogicID(string ID, string GameCode, bool FillSpace = true)
        {
            string Spacer = FillSpace ? "_" : " ";
            if (ID.StartsWith($"MM{Spacer}") || ID.StartsWith($"OOT{Spacer}") || ID.StartsWith($"SHARED{Spacer}")) { return ID; }
            return $"{GameCode}{Spacer}{ID}";
        }

        public static string GetGamecode(string ID)
        {
            if (ID.StartsWith($"MM_") || ID.StartsWith($"MM ")) { return "MM"; }
            else if (ID.StartsWith("OOT_") || ID.StartsWith("OOT ")) { return "OOT"; }
            else if (ID.StartsWith("SHARED_") || ID.StartsWith("SHARED ")) { return "SHARED"; }
            throw new Exception($"Could not find gamecode for {ID}");
        }

        public static bool HasGamecode(string ID)
        {
            if (ID.StartsWith($"MM_") || ID.StartsWith($"MM ")) { return true; }
            else if (ID.StartsWith("OOT_") || ID.StartsWith("OOT ")) { return true; }
            else if (ID.StartsWith("SHARED_") || ID.StartsWith("SHARED ")) { return true; }
            return false;
        }

        public static string[] SplitParams(string Params, char Splitchar = ',', char openPar = '(', char closePar = ')')
        {
            List<string> result = [];
            int Parlevel = 0;
            StringBuilder CurrentString = new();
            foreach (var i in Params)
            {
                if (i == openPar) { Parlevel++; }
                if (i == closePar) { Parlevel--; }
                if (i == Splitchar && Parlevel == 0) { result.Add(CurrentString.ToString().Trim()); CurrentString.Clear(); continue; }
                CurrentString.Append(i);
            }
            result.Add(CurrentString.ToString().Trim());
            return result.ToArray();
        }
        public static string ParseCondFunc(OOTMMLogicFunction Function, string ID, LogicStringParser Parser)
        {
            string Clause = Function.Param[0];
            string IfTrue = Function.Param[1];
            string IfFalse = Function.Param[2];
            string FalseClause;
            var ParsedClause = LogicStringConverter.ConvertLogicStringToConditional(Parser, Clause, ID, true);

            for (var i = 0; i < ParsedClause.Count; i++)
            {
                for (var j = 0; j < ParsedClause[i].Count; j++)
                {
                    string Inverse = "false";
                    string Item = ParsedClause[i][j];
                    if (Item.StartsWith('!')) { Item = Item[1..]; Inverse = "true"; }

                    if (Item.StartsWith("trick") || Item.StartsWith("setting")) { Item = Item[..^1] + $", {Inverse})"; }
                    else if (Item == "is_adult") { Item = "is_child"; }
                    else
                    {
                        string Gamecode = GetGamecode(ID);
                        Item = $"available{{{Gamecode}_{Item}, {Inverse}}}";
                    }
                    ParsedClause[i][j] = Item;
                }
            }

            FalseClause = LogicStringConverter.ConvertConditionalToLogicString(Parser, ParsedClause);

            return $"((({Clause}) && ({IfTrue})) || (({FalseClause}) && ({IfFalse})))";
        }

        public static T DeserializeYAMLFile<T>(string Path)
        {
            var Json = TestingUtility.ConvertYamlStringToJsonString(File.ReadAllText(Path), true);
            return JsonConvert.DeserializeObject<T>(Json);
        }
        public static T DeserializeCSVFile<T>(string Path)
        {
            var Json = TestingUtility.ConvertCsvFileToJsonObject(File.ReadAllLines(Path));
            return JsonConvert.DeserializeObject<T>(Json);
        }

        public static bool IsLocationRenewable(this OOTMMPoolLocation location, string GameCode, ExtraData extraData)
        {
            var ID = AddGameCodeToLogicID(location.id, GameCode);
            if (ID.In(extraData.nonrenewablelocations)) { return false; }
            if (ID.In(extraData.renewablelocations)) { return true; }
            if (location.type.In(extraData.renewabletypes)) { return true; }
            return false;
        }

        public static class CLockLogicHandling
        {
            static Dictionary<string, string> map = new Dictionary<string, string> 
            {
                { "DAY1", "clock_day1" },
                { "NIGHT1", "clock_night1" },
                { "DAY2", "clock_day2" },
                { "NIGHT2", "clock_night2" },
                { "DAY3", "clock_day3" },
                { "NIGHT3", "clock_night3" },
            };

            public static string GetTimeAt(string TimeString)
            {
                string Time = TimeString.Split("_")[0];
                return map[Time];
            }

            public static string GetTimeBefore(string TimeString)
            {
                string Time = TimeString.Split("_")[0];
                if (Time == "NIGHT3") { return "true"; }
                List<string> Times = [];
                foreach(var i in map.Keys)
                {
                    Times.Add(map[i]);
                    if (i == Time) { break; }
                }
                return $"({string.Join(" || ", Times)})";
            }

            public static string GetTimeAfter(string TimeString)
            {
                string Time = TimeString.Split("_")[0];
                if (Time == "DAY1") { return "true"; }
                List<string> Times = [];
                foreach (var i in map.Keys.Reverse())
                {
                    Times.Add(map[i]);
                    if (i == Time) { break; }
                }
                return $"({string.Join(" || ", Times)})";
            }

            public static string GetTimeBetween(string StartTimeString, string EndTimeString)
            {
                string StartTime = StartTimeString.Split("_")[0];
                string EndTime = EndTimeString.Split("_")[0];
                List<string> Times = [];
                bool AtTime = false;
                foreach (var i in map.Keys)
                {
                    if (i == StartTime) { AtTime = true; }
                    if (!AtTime) { continue; }
                    Times.Add(map[i]);
                    if (i == EndTime) { break; }
                }
                Debug.WriteLine($"Between({StartTimeString}, {EndTimeString}) Parsed to\n({string.Join(" || ", Times)})");
                return $"({string.Join(" || ", Times)})";
            }

        }
    }
}
