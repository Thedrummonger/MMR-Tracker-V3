using MMR_Tracker_V3.OtherGames.OOTMMRCOMBO;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.datamodel;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.OOTMMUtil;

namespace MMR_Tracker_V3.OtherGames.OOTMMV2
{
    internal class FunctionParsing
    {
        public static LogicStringParser OOTMMLogicStringParser = new LogicStringParser();

        public static void AddExpandedLine(List<MMRData.JsonFormatLogicItem> NewLogic, string ID, string Param, string Gamecode, string LogicLine)
        {
            if (NewLogic.Any(x => x.Id == ID)) { return; }
            var NewLogicItem = new MMRData.JsonFormatLogicItem { Id = ID };
            NewLogicItem.ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, LogicLine);
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
                            if (ParseConditionalFunction(NewConditionals, Conditional, LogicItem))
                            {
                                FunctionParsed = true;
                            }
                        }
                        NewConditionalSet.Add(NewConditionals);
                    }
                    string ExapandedNewConditionalSet = LogicStringConverter.ConvertConditionalToLogicString(OOTMMLogicStringParser, NewConditionalSet);
                    LogicItem.ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, ExapandedNewConditionalSet);
                    LogicUtilities.RemoveRedundantConditionals(LogicItem);
                }
            }

            foreach (var LogicItem in LogicFile.Logic)
            {
                LogicUtilities.MakeCommonConditionalsRequirements(LogicItem);
            }

            bool ParseConditionalFunction(List<string> NewConditionals, string Conditional, MMRData.JsonFormatLogicItem LogicItem)
            {
                bool FunctionParsed = false;
                string OriginalGamecode = GetGamecodeFromID(LogicItem);
                string Gamecode = GetGamecodeFromID(LogicItem);
                string ParsedConditional = Conditional;
                if (!LogicEntryHasGamecode(ParsedConditional)) 
                { 
                    ParsedConditional = $"{Gamecode}_{ParsedConditional}";
                    FunctionParsed = true;
                }
                if (LogicEditing.IsLogicFunction(Conditional, out string Func, out string Param, new('(', ')')))
                {
                    string OriginalParam = Param;
                    if (LogicEntryHasGamecode(Param))
                    {
                        List<string> parts = Param.Split('_').ToList();
                        Gamecode = parts[0];
                        parts.RemoveAt(0);
                        Param = string.Join("_", parts);
                    }
                    switch (Func)
                    {
                        case "trick":
                        case "glitch":
                            ParsedConditional = $"trick{{TRICK_{Gamecode}_{Param}}}";
                            FunctionParsed = true;
                            break;
                        case "event":
                            ParsedConditional = $"{Gamecode}_EVENT_{Param}";
                            FunctionParsed = true;
                            break;
                        case "soul":
                            string SoulSetting = OriginalGamecode == "MM" ? "enemySoulsMm" : "enemySoulsOot";
                            ParsedConditional = $"({Gamecode}_{Param} || var{{{SoulSetting}, false}})";
                            FunctionParsed = true;
                            break;
                        case "has":
                            ParsedConditional = $"{Gamecode}_{Param}";
                            FunctionParsed = true;
                            break;
                        case "setting":
                            var SubParams = OriginalParam.Split(',').Select(x => x.Trim()).ToList();
                            if (SubParams.Count < 2) { ParsedConditional = $"var{{{OriginalParam}}}"; }
                            else if (int.TryParse(SubParams[1], out _) || bool.TryParse(SubParams[1], out _)) { ParsedConditional = $"var{{{OriginalParam}}}"; }
                            else { ParsedConditional = $"setting{{{OriginalParam}}}"; }
                            FunctionParsed = true;
                            break;
                        case "!setting":
                            var NSubParams = OriginalParam.Split(',').Select(x => x.Trim()).ToList();
                            if (NSubParams.Count < 2) { ParsedConditional = $"var{{{OriginalParam}, false}}"; }
                            else if (int.TryParse(NSubParams[1], out _) || bool.TryParse(NSubParams[1], out _)) { ParsedConditional = $"var{{{OriginalParam}, false}}"; }
                            else { ParsedConditional = $"setting{{{OriginalParam}, false}}"; }
                            FunctionParsed = true;
                            break;
                        case "can_play":
                            ParsedConditional = $"({OriginalGamecode}_has_ocarina && has({OriginalParam}))";
                            FunctionParsed = true;
                            break;
                        case "small_keys":
                            string SmallKeySetting = Gamecode == "MM" ? "smallKeyShuffleMm" : "smallKeyShuffleOot";
                            ParsedConditional =$"(setting({SmallKeySetting}, removed) || has({Param}))";
                            FunctionParsed = true;
                            break;
                        case "boss_key":
                            string BossKeySetting = Gamecode == "MM" ? "bossKeyShuffleMm" : "bossKeyShuffleOot";
                            ParsedConditional =$"(setting({BossKeySetting}, removed) || has({Param}))";
                            FunctionParsed = true;
                            break;
                        case "has_small_keys_fire":
                            ParsedConditional =$"(setting(smallKeyShuffleOot, removed) || cond(setting(smallKeyShuffleOot, anywhere), has(SMALL_KEY_FIRE, {int.Parse(Param) + 1}), has(SMALL_KEY_FIRE, {Param})))";
                            FunctionParsed = true;
                            break;
                        case "cond":
                            ParsedConditional = ParseCondFunc(OriginalParam);
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
                            ParsedConditional = $"true"; //Price will be handled by the trackers built in price tracking
                            FunctionParsed = true;
                            break;
                        case "adult_trade":
                            ParsedConditional = $"is_adult && has({OriginalParam})";
                            FunctionParsed = true;
                            break;
                        case "can_ride_bean":
                            ParsedConditional = $"is_adult && event({OriginalParam})";
                            FunctionParsed = true;
                            break;
                        case "has_hookshot":
                            if (Gamecode == "OOT") { 
                                ParsedConditional = $"has(HOOKSHOT, {Param}) || has(SHARED_HOOKSHOT, {Param})";
                                FunctionParsed = true;
                            }
                            break;
                        case "has_ocarina_n":
                            if (Gamecode == "MM")
                            {
                                ParsedConditional = $"(has(OCARINA, {OriginalParam}) || has(SHARED_OCARINA, {OriginalParam}))";
                                FunctionParsed = true;
                            }
                            break;
                        case "can_hookshot_n":
                            if (Gamecode == "MM")
                            {
                                ParsedConditional = $"(has(HOOKSHOT, {Param}) || has(SHARED_HOOKSHOT, {Param}))";
                                FunctionParsed = true;
                            }
                            else if (Gamecode == "OOT")
                            {
                                ParsedConditional = $"is_adult && has_hookshot({Param})";
                                FunctionParsed = true;
                            }
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
                            ParsedConditional =  $"renewable{{{Gamecode}_{Param}}}";
                            FunctionParsed = true;
                            break;
                        case "age":
                            if (OriginalParam == "child") { ParsedConditional = "true"; }
                            else { ParsedConditional = "OOT_EVENT_TIME_TRAVEL"; }
                            FunctionParsed = true;
                            break;
                        case "license": //Getting the item from a non renewable source give you the license
                            ParsedConditional =  $"renewable{{{Gamecode}_{Param}, false}}";
                            FunctionParsed = true;
                            break;
                        case "special":
                            ParsedConditional = $"{Gamecode}_HAS_{Param}_REQUIREMENTS";
                            break;

                    }
                }
                NewConditionals.Add($"({ParsedConditional})");
                return FunctionParsed;
            }
        }

        private static string ParseCondFunc(string param)
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

            if (Clause.StartsWith("trick") || Clause.StartsWith("setting")) { FalseClause = Clause[..^1] + ", false)"; }
            else { throw new Exception($"Could not get false clause for {Clause}"); }

            return $"((({Clause}) && ({IfTrue})) || (({FalseClause}) && ({IfFalse})))";
        }
    }
}
