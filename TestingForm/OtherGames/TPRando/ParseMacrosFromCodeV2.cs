using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestingForm;

namespace MMR_Tracker_V3.OtherGames.TPRando
{
    public static class ParseMacrosFromCodeV2
    {
        public static List<MMRData.JsonFormatLogicItem> ReadLines()
        {
            LogicStringParser parser = new LogicStringParser();

            string TPRFiles = TestingReferences.GetOtherGameDataPath("TPRando");
            string MacroFile = Path.Combine(TPRFiles, "MacroCode.txt");
            var MacroLines = File.ReadLines(MacroFile);

            MMRData.LogicFile logicFile = new MMRData.LogicFile();
            logicFile.Logic = new List<MMRData.JsonFormatLogicItem>();

            string CurrentString = string.Empty;
            bool ReadingFuctionName = false;
            bool InFunction = false;
            int BracketLevel = 0;
            string CurrentFucntion = string.Empty;
            string CurrentFucntionData = string.Empty;

            foreach (var Rawline in MacroLines)
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
                        if (C == '{') { BracketLevel++; }
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

            void CommitFunction(string rawname, string function)
            {
                string name = rawname.Trim();
                string CleanFunc = function.TrimSpaces().Trim();
                if (!CleanFunc.StartsWith("return")) { Debug.WriteLine($"Function {name} was not parsable"); return; }
                CleanFunc = CleanFunc["return".Length..^1].Trim();

                //Hopefully a better way to do this in the future.
                for (int i = 1; i < 10; i++)
                {
                    CleanFunc = CleanFunc.Replace($") >= {i}", $", {i})");
                    CleanFunc = CleanFunc.Replace($") > {i}", $", {i})");
                }

                logicFile.Logic.Add(new MMRData.JsonFormatLogicItem { Id = name, ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(parser, CleanFunc, name) });
            }

            List<MMRData.JsonFormatLogicItem> FormattedLogic = new List<MMRData.JsonFormatLogicItem>();

            foreach(var Item in logicFile.Logic)
            {
                List<List<string>> NewSet = new List<List<string>>();
                foreach(var set in Item.ConditionalItems)
                {
                    List<string> FormattedItems = new List<string>();
                    foreach(var item in set)
                    {
                        FormattedItems.Add(FormatItem(item));
                    }
                    NewSet.Add(FormattedItems);
                }
                FormattedLogic.Add(new MMRData.JsonFormatLogicItem { Id = Item.Id, ConditionalItems = NewSet });
            }

            return FormattedLogic;
        }

        private static string FormatItem(string item)
        {
            string FormattedItem = item;

            if (FormattedItem == "GetItemWheelSlotCount(, 3)") { FormattedItem = "ItemWheelItems, 3"; }
            if (FormattedItem.EndsWith("()")) { FormattedItem = FormattedItem[..^2]; }
            if (FormattedItem.StartsWith("CanUse(Item.")) { FormattedItem = FormattedItem["CanUse(Item.".Length..^1]; }
            if (FormattedItem.StartsWith("getItemCount(Item.")) { FormattedItem = FormattedItem["getItemCount(Item.".Length..^1]; }
            if (FormattedItem.StartsWith("Randomizer.Rooms.RoomDict[")) 
            { 
                FormattedItem = FormattedItem["Randomizer.Rooms.RoomDict[".Length..^"].ReachedByPlaythrough".Length];
                FormattedItem = FormattedItem.Replace("\"", "").Trim();
            }
            if (FormattedItem.StartsWith("Randomizer.SSettings."))
            {
                FormattedItem = FormattedItem["Randomizer.SSettings.".Length..];
                var Settingparams = FormattedItem.StringSplit(" == ");
                string Settingname = Settingparams[0];
                if (Settingparams.Length == 1)
                {
                    FormattedItem = $"option{{{Settingname}}}";
                }
                else
                {
                    string Settingvalue = Settingparams[1].Contains('.') ? Settingparams[1].Split(".")[1] : Settingparams[1];
                    if (bool.TryParse(Settingvalue, out bool SettingValBool)) { Settingvalue = SettingValBool.ToString(); }
                    FormattedItem = $"option{{{Settingname}, {Settingvalue}}}";
                }
            }

            return FormattedItem;
        }
    }
}
