using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;
using static MMR_Tracker_V3.Logic.LogicStringParser;

namespace MMR_Tracker_V3.GameDataCreation.OOTMMV2
{
    internal class OOTMMUtil
    {
        public static string GetGamecodeFromID(MMRData.JsonFormatLogicItem LogicItem)
        {
            return GetGamecodeFromID(LogicItem.Id);
        }
        public static string GetGamecodeFromID(string LogicItem)
        {
            if (LogicItem.StartsWith("OOT_")) { return "OOT"; }
            if (LogicItem.StartsWith("MM_")) { return "MM"; }
            var Segments = LogicItem.Split(' ');
            return Segments[0];
        }
        public static Tuple<string, string> SplitGamecodeFromID(MMRData.JsonFormatLogicItem LogicItem)
        {
            return SplitGamecodeFromID(LogicItem.Id);
        }
        public static Tuple<string, string> SplitGamecodeFromID(string LogicItem)
        {

            var Segments = LogicItem.Split('_');
            return new(Segments[0], string.Join("_",Segments.Skip(1)));
        }

        public static bool LogicEntryHasGamecode(string LogicItem)
        {
            if (bool.TryParse(LogicItem, out _)) { return true; }
            if (IsOOTMMLogicFunction(LogicItem, out _, out _, new('(', ')'))) { return true; }
            if (IsOOTMMLogicFunction(LogicItem, out _, out _)) { return true; }
            if (LogicItem.StartsWith("OOT_")) { return true; }
            if (LogicItem.StartsWith("MM_")) { return true; }
            if (LogicItem.StartsWith("SHARED_")) { return true; }
            var Segments = LogicItem.Split(' ');
            if (Segments[0].Trim() == "OOT") { return true; }
            if (Segments[0].Trim() == "MM") { return true; }
            if (Segments[0].Trim() == "SHARED") { return true; }
            return false;
        }

        public static bool IsOOTMMLogicFunction(string i, out string Func, out string Param, Tuple<char, char> functionCasing = null)
        {
            functionCasing ??= new('{', '}');
            Func = null;
            Param = null;
            if (i.IsLiteralID(out _)) { return false; }
            bool squirFunc = i.EndsWith(functionCasing.Item2) && i.Contains(functionCasing.Item1);

            if (!squirFunc) { return false; }

            int funcEnd = i.IndexOf(functionCasing.Item1);
            int paramStart = i.IndexOf(functionCasing.Item1) + 1;
            int paramEnd = i.LastIndexOf(functionCasing.Item2);
            Func = i[..funcEnd].Trim().ToLower();
            Param = i[paramStart..paramEnd].Trim();

            return true;
        }

        public static string GetExitID(string Area, string Exit, string GameCode)
        {
            string TrueAreaName = $"{GameCode} {Area}";
            string TrueExitName = LogicEntryHasGamecode(Exit) ? $"{Exit}" : $"{GameCode} {Exit}";
            string FullexitName = $"{TrueAreaName} => {TrueExitName}";
            return FullexitName;
        }

        public static bool IsLocationRenewable(string ID, string CheckType)
        {
            var ForceNonRenewable = new string[]
            {
                    "MM Bomb Shop Bomb Bag",
                    "MM Bomb Shop Bomb Bag 2",
                    "MM Curiosity Shop All-Night Mask",
                    "OOT Lost Woods Scrub Sticks Upgrade",
                    "OOT Lost Woods Grotto Scrub Nuts Upgrade",
                    "OOT Hyrule Field Grotto Scrub HP"
            };
            string[] RenewableTypes = new string[] { "shop", "cow", "scrub" };
            return (RenewableTypes.Contains(CheckType) || (ID.StartsWith("MM_TINGLE_MAP_"))) && !ForceNonRenewable.Contains(ID);
        }

        public static string GetItemNiceName(string ItemID, Dictionary<string, string> ItemNames)
        {
            if (ItemNames.ContainsKey(ItemID)) { return ItemNames[ItemID]; }

            var IDParts = ItemID.Split('_').ToList();
            string GameCode = IDParts[0];
            IDParts.RemoveAt(0);
            string NiceName = string.Join(" ", IDParts).ToLower();
            TextInfo cultInfo = new CultureInfo("en-US", false).TextInfo;
            NiceName = cultInfo.ToTitleCase(NiceName);
            if (GameCode == "MM" || GameCode == "OOT") { NiceName = $"{NiceName} ({GameCode})"; }
            return NiceName;
        }
    }
}
