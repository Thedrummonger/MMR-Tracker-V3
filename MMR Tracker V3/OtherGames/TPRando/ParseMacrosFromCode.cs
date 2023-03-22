using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames.TPRando
{
    public class ParseMacrosFromCode
    {
        public static Dictionary<string, string> ReadMacrosFromCode()
        {
            string LogicFunctionsFile = Path.Combine(References.TestingPaths.GetDevTestingPath(), "TPRando", "Randomizer-Web-Generator-main", "Generator", "Logic", "LogicFunctions.cs");

            bool AtReleventLines = false;
            bool InFunc = false;
            string CurrentFunc = "";
            string CurrentText = "";

            Dictionary<string, string> Macros = new Dictionary<string, string>();

            foreach (var i in File.ReadAllLines(LogicFunctionsFile))
            {
                if (i.Contains("// END OF GLITCHED LOGIC")) { break; }
                if (i.Contains("public static bool HasDamagingItem()")) { AtReleventLines = true; }
                if (!AtReleventLines) { continue; }
                if (i.Trim().StartsWith("return")) { InFunc = true; }
                if (i.Trim().StartsWith("public static bool "))
                {
                    CurrentFunc = i.Trim().Replace("public static bool ", "").Replace("()", "");
                }
                if (InFunc)
                {
                    string Line = i;
                    if (Line.Trim().StartsWith("return")) { Line = Line.Replace("return ", ""); }
                    Line = Regex.Replace(Line, @"\s+", " ");
                    Line = Line.Replace("()", "").Replace("CanUse(","(").Replace("getItemCount","").Replace("Item.", "");
                    //Todo make this better
                    for(var c = 0; c <= 99; c++)
                    {
                        Line = Line.Replace($") >= {c}", $", {c})").Replace($") > {c}", $", {c})");
                    }
                    Line = Line.Replace($"&&", $"and").Replace($"||", $"or").Replace(";", "");
                    Line = Line.Replace("Randomizer.Rooms.RoomDict[\"", "").Replace("\"].ReachedByPlaythrough", "");
                    Line = Line.Replace("Randomizer.SSettings.", "option{").Replace(" ==", ",");
                    CurrentText += Line;
                }
                if (i.Trim().EndsWith(";") && InFunc)
                {
                    Macros.Add(CurrentFunc.Trim(), CurrentText.Replace("( ", "(").Replace(" )", ")").Trim()); ;
                    CurrentFunc = "";
                    CurrentText = "";
                    InFunc = false;
                }
            }
            return Macros;
        }
    }
}
