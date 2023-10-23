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
    public class FunctionParsing
    {
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
                            if (ParseConditionalFunction(NewConditionals, Conditional, LogicItem))
                            {
                                FunctionParsed = true;
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
                        case "soul_enemy":
                            string EnemySoulSetting = OriginalGamecode == "MM" ? "soulsEnemyMm" : "soulsEnemyOot";
                            ParsedConditional = $"({Gamecode}_{Param} || setting{{{EnemySoulSetting}, false}})";
                            FunctionParsed = true;
                            break;
                        case "soul_boss":
                            string BossSoulSetting = OriginalGamecode == "MM" ? "soulsBossMm" : "soulsBossOot";
                            ParsedConditional = $"({Gamecode}_{Param} || setting{{{BossSoulSetting}, false}})";
                            FunctionParsed = true;
                            break;
                        case "soul_npc":
                            string NPCSoulSetting = OriginalGamecode == "MM" ? "soulsNpcMm" : "soulsNpcOot";
                            ParsedConditional = $"({Gamecode}_{Param} || setting{{{NPCSoulSetting}, false}})";
                            FunctionParsed = true;
                            break;
                        case "has":
                            ParsedConditional = $"{Gamecode}_{Param}";
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
                        case "can_play":
                            ParsedConditional = $"({OriginalGamecode}_has_ocarina && has({OriginalParam}))";
                            FunctionParsed = true;
                            break;
                        case "small_keys":
                            string SmallKeySetting = Gamecode == "MM" ? "smallKeyShuffleMm" : "smallKeyShuffleOot";
                            string[] SmallKeyParams = Param.Split(",").Select(x => x.Trim()).ToArray();
                            ParsedConditional =$"(has_skeleton_key || setting({SmallKeySetting}, removed) || has({SmallKeyParams[1]}) || has({SmallKeyParams[0]}, {SmallKeyParams[2]}))";
                            FunctionParsed = true;
                            break;
                        case "boss_key":
                            string BossKeySetting = Gamecode == "MM" ? "bossKeyShuffleMm" : "bossKeyShuffleOot";
                            ParsedConditional =$"(setting({BossKeySetting}, removed) || has({Param}))";
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
                                ParsedConditional = $"age_hookshot && has_hookshot({Param})";
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
                        case "small_keys_hideout":
                        case "small_keys_botw":
                        case "small_keys_forest":
                        case "small_keys_ganon":
                        case "small_keys_gb":
                        case "small_keys_gtg":
                        case "small_keys_sh":
                        case "small_keys_shadow":
                        case "small_keys_spirit":
                        case "small_keys_st":
                        case "small_keys_water":
                        case "small_keys_wf":
                            string KeyCode = Func.Split('_')[2].ToUpper();
                            if (KeyCode == "HIDEOUT") { KeyCode = "GF"; }
                            int SmallKeyCount = int.Parse(Param);
                            ParsedConditional =  $"(small_keys(SMALL_KEY_{KeyCode}, KEY_RING_{KeyCode}, {Param}))";
                            FunctionParsed = true;
                            break;
                        case "small_keys_fire":
                            int SmallKeyFireCount = int.Parse(Param);
                            ParsedConditional =  $"(cond(setting(smallKeyShuffleOot, anywhere), small_keys(SMALL_KEY_FIRE, KEY_RING_FIRE, {SmallKeyFireCount+1}), small_keys(SMALL_KEY_FIRE, KEY_RING_FIRE, {SmallKeyFireCount})))";
                            break;
                        case "small_keys_tcg":
                            int SmallKeyTCGCount = int.Parse(Param);
                            ParsedConditional =  $"cond(setting(smallKeyShuffleChestGame, vanilla), has_lens_strict && can_use_wallet(1), has_skeleton_key || has(KEY_RING_TCG) || has(SMALL_KEY_TCG, {SmallKeyTCGCount}))";
                            break;
                        case "can_play_cross":
                            if (Gamecode == "MM")
                            {
                                ParsedConditional = $"can_play({OriginalParam}) && setting(crossWarpOot)";
                            }
                            else
                            {
                                ParsedConditional = $"can_play({OriginalParam}) && (setting(crossWarpMm, full) || (setting(crossWarpMm, childOnly) && is_child))";
                            }
                            break;
                        case "ocarina_button":
                            string[] OCButtonParams = Param.Split(",").Select(x => x.Trim()).ToArray();
                            if (Gamecode == "MM")
                            {
                                ParsedConditional = $"cond(setting(ocarinaButtonsShuffleMm), cond(setting(sharedOcarinaButtons), has({OCButtonParams[1]}), has({OCButtonParams[0]})), true)";
                            }
                            else
                            {
                                ParsedConditional = $"cond(setting(ocarinaButtonsShuffleOot), cond(setting(sharedOcarinaButtons), has({OCButtonParams[1]}), has({OCButtonParams[0]})), true)";
                            }
                            break;
                        case "silver_rupees":
                            string[] SilverRuppeParams = Param.Split(",").Select(x => x.Trim()).ToArray();
                            ParsedConditional = $"(setting(magicalRupee) && has(RUPEE_MAGICAL)) || cond(setting(silverRupeePouches), has({SilverRuppeParams[1]}), has({SilverRuppeParams[0]}, {SilverRuppeParams[2]}))";
                            break;

                    }
                }
                NewConditionals.Add($"({ParsedConditional})");
                return FunctionParsed;
            }
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
                    string Item = ParsedClause[i][j];
                    if (Item.StartsWith("!")) { Item = Item.Substring(1); }
                    else if (Item.StartsWith("trick") || Item.StartsWith("setting")) { Item = Item[..^1] + ", false)"; }
                    else if (Item == "is_adult") { Item = "is_child"; }
                    else
                    {
                        string Gamecode = GetGamecodeFromID(logicItem);
                        Item = $"available{{{Gamecode}_{Item}, false}}"; 
                    }
                    ParsedClause[i][j] = Item;
                }
            }

            FalseClause = LogicStringConverter.ConvertConditionalToLogicString(OOTMMLogicStringParser, ParsedClause);

            return $"((({Clause}) && ({IfTrue})) || (({FalseClause}) && ({IfFalse})))";
        }
    }
}
