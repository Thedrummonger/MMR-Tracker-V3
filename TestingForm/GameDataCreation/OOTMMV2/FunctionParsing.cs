using MathNet.Symbolics;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using TestingForm.GameDataCreation.OOTMMV2;
using static Microsoft.FSharp.Core.ByRefKinds;
using static MMR_Tracker_V3.GameDataCreation.OOTMMV2.OOTMMUtil;
using static MMR_Tracker_V3.GameDataCreation.OOTMMV2.SettingsCreation;

namespace MMR_Tracker_V3.GameDataCreation.OOTMMV2
{
    public class FunctionParsing
    {
        public static Dictionary<string, datamodel.FuncMacro> LogicMAcroFunctions = new Dictionary<string, datamodel.FuncMacro>();
        public static LogicStringParser OOTMMLogicStringParser = new LogicStringParser();

        public static void AddExpandedLine(List<MMRData.JsonFormatLogicItem> NewLogic, string ID, string Param, string Gamecode, string LogicLine)
        {
            if (NewLogic.Any(x => x.Id == ID)) { return; }
            var NewLogicItem = new MMRData.JsonFormatLogicItem { Id = ID };
            NewLogicItem.ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, LogicLine, ID);
            NewLogic.Add(NewLogicItem);
        }

        public static void ParseLogicFunctions(MMRData.LogicFile LogicFile)
        {
            bool FunctionParsed = true;
            while (FunctionParsed)
            {
                FunctionParsed = false;
                foreach (var LogicItem in LogicFile.Logic)
                {
                    string Gamecode = GetGamecodeFromID(LogicItem);
                    List<List<string>> NewConditionalSet = new List<List<string>>();
                    foreach (var ConditionalSet in LogicItem.ConditionalItems)
                    {
                        List<string> NewConditionals = new List<string>();
                        foreach (var Conditional in ConditionalSet)
                        {
                            if (IsOOTMMLogicFunction(Conditional, out _, out _, new('(', ')')))
                            {
                                NewConditionals.Add(ParseConditionalFunction(Conditional, LogicItem));
                                FunctionParsed = true;
                            }
                            else if (!LogicEntryHasGamecode(Conditional))
                            {
                                NewConditionals.Add($"{GetGamecodeFromID(LogicItem)}_{Conditional}");
                            }
                            else
                            {
                                NewConditionals.Add(Conditional);
                            }
                        }
                        NewConditionalSet.Add(NewConditionals);
                    }
                    string ExapandedNewConditionalSet = LogicStringConverter.ConvertConditionalToLogicString(OOTMMLogicStringParser, NewConditionalSet);
                    LogicItem.ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, ExapandedNewConditionalSet, LogicItem.Id);
                    LogicUtilities.RemoveRedundantConditionals(LogicItem);
                }
            }

            foreach (var LogicItem in LogicFile.Logic)
            {
                LogicUtilities.MakeCommonConditionalsRequirements(LogicItem);
            }

            string ParseConditionalFunction(string Conditional, MMRData.JsonFormatLogicItem LogicItem)
            {
                string OriginalGamecode = GetGamecodeFromID(LogicItem);
                string Gamecode = GetGamecodeFromID(LogicItem);
                string ParsedConditional = Conditional;
                IsOOTMMLogicFunction(Conditional, out string Func, out string Param, new('(', ')'));
                string OriginalParam = Param;
                switch (Func)
                {
                    case "trick":
                    case "glitch":
                        ParsedConditional = $"trick{{TRICK_{Param}}}";
                        FunctionParsed = true;
                        break;
                    case "event":
                        if (LogicEntryHasGamecode(Param)) { ParsedConditional = $"{SplitGamecodeFromID(Param).Item1}_EVENT_{SplitGamecodeFromID(Param).Item2}"; }
                        else { ParsedConditional = $"{Gamecode}_EVENT_{Param}"; }
                        FunctionParsed = true;
                        break;
                    case "has":
                        if (LogicEntryHasGamecode(Param)) { ParsedConditional = $"{Param}"; }
                        else { ParsedConditional = $"{Gamecode}_{Param}"; }
                        FunctionParsed = true;
                        break;
                    case "setting":
                        var SubParams = OriginalParam.Split(',').Select(x => x.Trim()).ToList();
                        if (SubParams.Count < 2) { ParsedConditional = $"setting{{{OriginalParam}}}"; }
                        else if (int.TryParse(SubParams[1], out _) || bool.TryParse(SubParams[1], out _)) { ParsedConditional = $"setting{{{OriginalParam}}}"; }
                        else { ParsedConditional = $"setting{{{OriginalParam}}}"; }
                        FunctionParsed = true;
                        break;
                    case "!setting":
                        var NSubParams = OriginalParam.Split(',').Select(x => x.Trim()).ToList();
                        if (NSubParams.Count < 2) { ParsedConditional = $"setting{{{OriginalParam}, false}}"; }
                        else if (int.TryParse(NSubParams[1], out _) || bool.TryParse(NSubParams[1], out _)) { ParsedConditional = $"setting{{{OriginalParam}, false}}"; }
                        else { ParsedConditional = $"setting{{{OriginalParam}, false}}"; }
                        FunctionParsed = true;
                        break;
                    case "cond":
                        ParsedConditional = ParseCondFunc(OriginalParam, LogicItem);
                        FunctionParsed = true;
                        break;
                    case "can_use_wallet":
                        int WalletNeeded = int.Parse(OriginalParam);
                        if (WalletNeeded < 2)
                        {
                            ParsedConditional = $"(has_rupees && (cond(setting(childWallets), has(WALLET, {WalletNeeded}) || has(SHARED_WALLET, {WalletNeeded}), true)))";
                        }
                        else
                        {
                            ParsedConditional = $"(has_rupees && (cond(setting(childWallets), has(WALLET, {WalletNeeded}) || has(SHARED_WALLET, {WalletNeeded}), has(WALLET, {WalletNeeded-1}) || has(SHARED_WALLET, {WalletNeeded-1}))))";
                        }
                        FunctionParsed = true;
                        break;
                    case "scrub_price":
                    case "shop_price":
                    case "tingle_price":
                    case "shop_ex_price":
                        //Price will be handled by the trackers built in price tracking, but add the child wallet (99) as default
                        //TODO, Change this to somehow add the wallet needed for the vanilla price.
                        ParsedConditional = $"{Gamecode}_COST_99";
                        FunctionParsed = true;
                        break;
                    case "masks":
                        ParsedConditional = $"MM_MASKS, {Param}";
                        FunctionParsed = true;
                        break;
                    case "before":
                        ParsedConditional = $"time{{Before_{Param}}}";
                        FunctionParsed = true;
                        break;
                    case "after":
                        ParsedConditional = $"time{{After_{Param}}}";
                        FunctionParsed = true;
                        break;
                    case "between":
                        ParsedConditional =  $"time{{Between_{Param}}}";
                        FunctionParsed = true;
                        break;
                    case "oot_time":
                        ParsedConditional = $"time{{{Param}}}";
                        FunctionParsed = true;
                        break;
                    case "at":
                        ParsedConditional =  $"time{{At_{Param}}}";
                        FunctionParsed = true;
                        break;
                    case "renewable":
                        if (LogicEntryHasGamecode(Param)) { ParsedConditional = $"renewable{{{Param}}}"; }
                        else { ParsedConditional = $"renewable{{{Gamecode}_{Param}}}"; }
                        FunctionParsed = true;
                        break;
                    case "age":
                        if (OriginalParam == "child") { ParsedConditional = "setting(startingAge, child) || OOT_EVENT_TIME_TRAVEL"; }
                        else { ParsedConditional = "setting(startingAge, adult) || OOT_EVENT_TIME_TRAVEL"; }
                        FunctionParsed = true;
                        break;
                    case "license": //Getting the item from a non renewable source give you the license
                        ParsedConditional =  $"renewable{{{Gamecode}_{Param}, false}}";
                        FunctionParsed = true;
                        break;
                    case "special":
                        ParsedConditional = $"{Gamecode}_HAS_{Param}_REQUIREMENTS";
                        break;
                    case "small_keys_extra":
                        string[] skeV = Param.TrimSplit(",");
                        ParsedConditional = $"has_skeleton_key || (setting(smallKeyShuffleOot, removed) || cond(setting(smallKeyRingOot, {skeV[0]}), has({skeV[2]}), has({skeV[1]}, {int.Parse(skeV[3]) + 1})))";
                        break;
                    case "has_pond_fish":
                        string[] PondFistParams = Param.Split(",").Select(x => x.Trim()).ToArray();
                        string PondFishType = PondFistParams[0];
                        int PondFishMin = int.Parse(PondFistParams[1]);
                        int PondFishMax = int.Parse(PondFistParams[2]);
                        ParsedConditional = $"OOT_HAS_{PondFishType}_{PondFishMin}_TO_{PondFishMax}";
                        break;
                    default:
                        string[] PKEY = LogicMAcroFunctions[$"{Gamecode}_{Func}"].Params;
                        string[] PVAL = Param.Split(",").Select(x => x.Trim()).ToArray();
                        var dic = PKEY.Zip(PVAL, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
                        ParsedConditional = LogicMAcroFunctions[$"{Gamecode}_{Func}"].Logic;
                        foreach (var kvp in dic)
                        {
                            ParsedConditional = ReplaceParam(ParsedConditional, kvp.Key, kvp.Value);
                        }
                        if (ParsedConditional == Conditional) { throw new Exception($"Conditional not parsed, this is an infinite loop\n" +
                            $"{Conditional.ToFormattedJson()}\n" +
                            $"{ParsedConditional.ToFormattedJson()}\n" +
                            $"{ReplaceParam(ParsedConditional, PKEY[0], PVAL[0])}\n" +
                            $"{PKEY.ToFormattedJson()}\n" +
                            $"{PVAL.ToFormattedJson()}\n" +
                            $"{dic.ToFormattedJson()}"); }
                        break;

                }
                if (ParsedConditional == Conditional) { throw new Exception("Conditional not parsed, this is an infinite loop"); }
                return $"({ParsedConditional})";
            }
        }

        private static string ReplaceParam(string Logic, string Param, string Value)
        {
            string pattern = @$"\b{Param}\b";
            string result = Regex.Replace(Logic, pattern, Value);
            return result;
        }

        public static string ParseCondFunc(string param, MMRData.JsonFormatLogicItem logicItem)
        {
            string Clause = string.Empty;
            string IfTrue = string.Empty;
            string IfFalse = string.Empty;
            int ParLevel = 0;
            int Segment = 0;
            foreach(char i in param)
            {
                if (i == '(') { ParLevel++; }
                else if (i == ')') { ParLevel--; }
                if (ParLevel == 0 && i == ',')
                {
                    Segment++;
                    continue;
                }
                switch (Segment)
                {
                    case 0:
                        Clause += i;
                        break; 
                    case 1:
                        IfTrue += i;
                        break;
                    case 2:
                        IfFalse += i;
                        break;
                    default: 
                        throw new Exception("Condition Had more than 3 parts?");
                }
            }
            Clause = Clause.Trim();
            IfTrue = IfTrue.Trim();
            IfFalse = IfFalse.Trim();

            string FalseClause;

            var ParsedClause = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, Clause, logicItem.Id, true);

            for(var i = 0; i < ParsedClause.Count; i++)
            {
                for(var j = 0; j < ParsedClause[i].Count; j++)
                {
                    string Inverse = "false";
                    string Item = ParsedClause[i][j];
                    if (Item.StartsWith("!")) { Item = Item.Substring(1); Inverse = "true"; }
                    else if (Item.StartsWith("trick") || Item.StartsWith("setting")) { Item = Item[..^1] + $", {Inverse})"; }
                    else if (Item == "is_adult") { Item = "is_child"; }
                    else
                    {
                        string Gamecode = GetGamecodeFromID(logicItem);
                        Item = $"available{{{Gamecode}_{Item}, {Inverse}}}"; 
                    }
                    ParsedClause[i][j] = Item;
                }
            }

            FalseClause = LogicStringConverter.ConvertConditionalToLogicString(OOTMMLogicStringParser, ParsedClause);

            return $"((({Clause}) && ({IfTrue})) || (({FalseClause}) && ({IfFalse})))";
        }
    }
}
