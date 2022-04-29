using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static MMR_Tracker_V3.LogicObjects;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public static class LogicCalculation
    {
        static bool LogItem = false;
        static List<int> AverageTime = new List<int>();
        public static Dictionary<string, List<string>> LogicUnlockData = new Dictionary<string, List<string>>();
        private static Dictionary<string, MMRData.JsonFormatLogicItem> LogicMap = new Dictionary<string, MMRData.JsonFormatLogicItem>();

        public static bool RequirementsMet(List<string> Requirements, TrackerInstance instance, string ID, Dictionary<string, List<string>> UnlockData = null)
        {
            List<string> SubUnlockData = new List<string>();
            bool reqMet = Requirements.All(x => LogicEntryAquired(instance, x, SubUnlockData));
            if (ID != null && UnlockData != null && reqMet)
            {
                if (!UnlockData.ContainsKey(ID)) { UnlockData.Add(ID, new List<string>()); }
                UnlockData[ID] = UnlockData[ID].Concat(SubUnlockData).ToList();
            }
            return reqMet;
        }

        public static bool ConditionalsMet(List<List<string>> Conditionals, TrackerInstance instance, string ID, Dictionary<string, List<string>> UnlockData = null)
        {
            if (!Conditionals.Any()) { return true; }
            var CondMet = Conditionals.FirstOrDefault(x => RequirementsMet(x, instance, ID, UnlockData));
            return CondMet != null;
        }

        public static bool LogicEntryAquired(TrackerInstance instance, string i, List<string> SubUnlockData)
        {
            SubUnlockData.Add(i);
            if (LogicOptionEntry(instance, i, out bool OptionEntryValid)) { return OptionEntryValid; }

            MultipleItemEntry(instance, i, out string LogicItem, out int Amount);
            bool Literal = LogicItem.IsLiteralID(out LogicItem);
            var type = instance.GetItemEntryType(LogicItem, Literal, out _);

            switch (type)
            {
                case LogicEntryType.Bool:
                    return bool.Parse(LogicItem);
                case LogicEntryType.item:
                    return instance.GetItemByID(LogicItem).Useable(instance, Amount);
                case LogicEntryType.macro:
                    return instance.GetMacroByID(LogicItem).Aquired; 
                case LogicEntryType.Area:
                    return AreaReached(LogicItem, instance);
                case LogicEntryType.variableString:
                    return LogicEntryAquired(instance, instance.Variables[LogicItem].Value as string, SubUnlockData);
                case LogicEntryType.variableList:
                    return ItemArrayUseable(instance, LogicItem, Amount, SubUnlockData);
                case LogicEntryType.variableBool:
                    return instance.Variables[LogicItem].Value;
                default:
                    Debug.WriteLine($"{LogicItem} Was not a valid Logic Entry");
                    return false;
            }
        }

        private static bool ItemArrayUseable(TrackerInstance instance, string ArrVar, int amount, List<string> SubUnlockData)
        {
            if (instance.Variables[ArrVar].Value is not List<string> ItemArray) { return false; }
            int UseableItems = 0;
            Dictionary<string, int> UsedItems = new Dictionary<string, int>();
            bool CountMet = false;
            foreach (string i in ItemArray)
            {
                if (LogicEntryAquired(instance, i, SubUnlockData))
                {
                    bool MultiItem = MultipleItemEntry(instance, i, out string LogicItem, out int Amount);
                    bool Literal = LogicItem.IsLiteralID(out LogicItem);
                    var type = instance.GetItemEntryType(LogicItem, Literal, out object ItemObj);

                    if (type == LogicEntryType.item && !MultiItem) 
                    {
                        int Needed = 0;
                        for(var j = 0; j < (ItemObj as ItemObject).GetTotalUsable(instance); j++)
                        {
                            if (!CountMet) { Needed++; }
                            UseableItems++;
                            if (UseableItems > amount) { CountMet = true; }
                        }
                        if (!CountMet && (!UsedItems.ContainsKey(LogicItem) || UsedItems[LogicItem] < Needed)) { UsedItems[LogicItem] = Needed; }
                    }
                    else 
                    { 
                        if (!CountMet && (!UsedItems.ContainsKey(LogicItem) || UsedItems[LogicItem] < Amount)) { UsedItems[LogicItem] = Amount; }
                        UseableItems++;
                        if (UseableItems > amount) { CountMet = true; }
                    }

                }
            }
            foreach(var x in UsedItems) { SubUnlockData.Add($"{x.Key}, {x.Value}"); }
            return UseableItems >= amount;
        }

        public static bool AreaReached(string Area, TrackerInstance instance)
        {
            if (Area == instance.EntrancePool.RootArea) { return true; }
            return instance.EntrancePool.AreaList.ContainsKey(Area) && instance.EntrancePool.AreaList[Area].ExitsAcessibleFrom > 0;
        }

        public static bool CalculatReqAndCond(MMRData.JsonFormatLogicItem Logic, TrackerInstance instance, string ID, string Area, Dictionary<string, List<string>> UnlockData)
        {
            Dictionary<string, List<string>> TempUnlockData = new Dictionary<string, List<string>>();
            bool Available =
                (Area == null || AreaReached(Area, instance)) &&
                RequirementsMet(Logic.RequiredItems, instance, ID, TempUnlockData) &&
                ConditionalsMet(Logic.ConditionalItems, instance, ID, TempUnlockData);

            if (!UnlockData.ContainsKey(ID) && Available)
            {
                UnlockData[ID] = TempUnlockData[ID];
            }
            if (UnlockData.ContainsKey(ID) && !Available)
            {
                UnlockData.Remove(ID);
            }

            return Available;
        }

        public static void FillLogicMap(TrackerInstance instance, Dictionary<string, MMRData.JsonFormatLogicItem> LogicMap)
        {
            foreach (var i in instance.MacroPool) 
            { 
                LogicMap[i.Key] = instance.GetLogic(i.Key); 
            }
            foreach (var i in instance.LocationPool)
            {
                LogicMap[i.Key] = instance.GetLogic(i.Key); 
            }
            foreach (var i in instance.HintPool)
            {
                LogicMap[i.Key] = instance.GetLogic(i.Key); 
            }
            foreach (var Area in instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.LoadingZoneExits)
                {
                    LogicMap[instance.GetLogicNameFromExit(i.Value)] = instance.GetLogic(instance.GetLogicNameFromExit(i.Value));
                }
                foreach (var i in Area.Value.MacroExits)
                {
                    LogicMap[instance.GetLogicNameFromExit(i.Value)] = instance.GetLogic(instance.GetLogicNameFromExit(i.Value));
                }
            }
        }
        public static void CalculateLogic(TrackerInstance instance, Dictionary<string, List<string>> UnlockData, Dictionary<object, int> AutoObtainedObjects =  null)
        {
            Debug.WriteLine("Logic Calculation ----------------------");
            Stopwatch LogicTime = new Stopwatch();
            Utility.TimeCodeExecution(LogicTime);

            FillLogicMap(instance, LogicMap);
            Utility.TimeCodeExecution(LogicTime, "Getting Logic", 1);

            ResetAutoObtainedItems(instance);

            Utility.TimeCodeExecution(LogicTime, "Resetting Auto checked Items", 1);

            if (AutoObtainedObjects is null) { AutoObtainedObjects = new Dictionary<object, int>(); }
            AutoObtainedObjects.Clear();

            int Itterations = 0;
            while (true)
            {
                bool EntranceChanges = CalculateEntranceMacros(instance, UnlockData, Itterations, AutoObtainedObjects);
                bool UnrandomizedItemChecked = CheckUrandomizedLocations(instance, UnlockData, Itterations, AutoObtainedObjects);
                bool UnrandomizedExitChecked = CheckUrandomizedExits(instance, UnlockData, Itterations, AutoObtainedObjects);
                bool MacroChanged = CalculateMacros(instance, UnlockData, Itterations, AutoObtainedObjects);
                Itterations++;
                 if (!MacroChanged && !EntranceChanges && !UnrandomizedItemChecked && !UnrandomizedExitChecked) { break; }
            }
            Debug.WriteLine($"Auto Item calculation took {Itterations} Itterations");
            Utility.TimeCodeExecution(LogicTime, "Calculating Auto Checked Items", 1);
            foreach (var i in instance.LocationPool.Values.Where(x => !x.IsUnrandomized(1) && !x.IsJunk()))
            {
                var Logic = LogicMap[i.ID];
                i.Available = CalculatReqAndCond(Logic, instance, i.ID, null, UnlockData);
            }
            Utility.TimeCodeExecution(LogicTime, "Calculating Locations", 1);
            foreach (var Area in instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.LoadingZoneExits.Where(x => !x.Value.IsUnrandomized(1) && !x.Value.IsJunk()))
                {
                    var Logic = LogicMap[instance.GetLogicNameFromExit(i.Value)];
                    i.Value.Available = CalculatReqAndCond(Logic, instance, instance.GetLogicNameFromExit(i.Value), Area.Key, UnlockData);
                }
            }
            Utility.TimeCodeExecution(LogicTime, "Caclulating Exits", 1);
            foreach (var i in instance.HintPool)
            {
                var Logic = LogicMap[i.Key];
                i.Value.Available = CalculatReqAndCond(Logic, instance, i.Key, null, UnlockData);
            }
            Utility.TimeCodeExecution(LogicTime, "Calculating Hints", -1);
        }

        private static bool CheckUrandomizedExits(TrackerInstance instance, Dictionary<string, List<string>> UnlockData, int itterations, Dictionary<object, int> autoObtainedObjects)
        {
            bool ItemStateChanged = false;
            foreach (var Area in instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.LoadingZoneExits.Where(x => x.Value.IsUnrandomized(1)))
                {
                    var Logic = LogicMap[instance.GetLogicNameFromExit(i.Value)];
                    var Available = CalculatReqAndCond(Logic, instance, instance.GetLogicNameFromExit(i.Value), Area.Key, UnlockData);
                    bool ShouldBeChecked = Available && i.Value.CheckState != CheckState.Checked;
                    bool ShouldBeUnChecked = !Available && i.Value.CheckState == CheckState.Checked;
                    if (ShouldBeChecked || ShouldBeUnChecked)
                    {
                        ItemStateChanged = true;
                        i.Value.Available = Available;
                        CheckState checkState = i.Value.Available ? CheckState.Checked : CheckState.Unchecked;
                        if (checkState == CheckState.Checked) { i.Value.DestinationExit = i.Value.GetVanillaDestination(); }
                        i.Value.ToggleExitChecked(checkState, instance);
                        if (Available) { autoObtainedObjects[i.Value] = itterations; }
                    }
                }
            }
            return ItemStateChanged;
        }

        private static void ResetAutoObtainedItems(TrackerInstance instance)
        {
            foreach (var Area in instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.MacroExits)
                {
                    if (i.Value.CheckState == CheckState.Checked)
                    {
                        i.Value.CheckState = CheckState.Unchecked;
                        var Destination = instance.EntrancePool.AreaList[i.Value.ID];
                        Destination.ExitsAcessibleFrom--;
                    }
                }
                foreach (var i in Area.Value.LoadingZoneExits.Where(x => x.Value.IsUnrandomized(1)))
                {
                    if (i.Value.CheckState == CheckState.Checked)
                    {
                        i.Value.CheckState = CheckState.Unchecked;
                        var Destination = instance.EntrancePool.AreaList[i.Value.ID];
                        Destination.ExitsAcessibleFrom--;
                    }
                }
            }
            foreach(var i in instance.LocationPool.Where(x => x.Value.IsUnrandomized(1)))
            {
                if (i.Value.CheckState == CheckState.Checked)
                {
                    i.Value.ToggleChecked(CheckState.Unchecked, instance);
                }
            }
        }

        private static bool CalculateEntranceMacros(TrackerInstance instance, Dictionary<string, List<string>> UnlockData, int itterations, Dictionary<object, int> autoObtainedObjects)
        {
            bool MacroStateChanged = false; 
            foreach (var Area in instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.MacroExits)
                {
                    var Logic = LogicMap[instance.GetLogicNameFromExit(i.Value)];
                    i.Value.Available = CalculatReqAndCond(Logic, instance, instance.GetLogicNameFromExit(i.Value), Area.Key, UnlockData);
                    if (i.Value.Available && i.Value.CheckState != CheckState.Checked)
                    {
                        MacroStateChanged = true;
                        i.Value.CheckState = CheckState.Checked;
                        i.Value.DestinationExit = i.Value.GetVanillaDestination();
                        var Destination = instance.EntrancePool.AreaList[i.Value.ID];
                        Destination.ExitsAcessibleFrom++;
                        autoObtainedObjects[i.Value] = itterations;
                    }
                    else if (!i.Value.Available && i.Value.CheckState == CheckState.Checked)
                    {
                        MacroStateChanged = true;
                        i.Value.CheckState = CheckState.Unchecked;
                        i.Value.DestinationExit = null;
                        var Destination = instance.EntrancePool.AreaList[i.Value.ID];
                        Destination.ExitsAcessibleFrom--;
                    }
                }
            }
            return MacroStateChanged;
        }

        private static bool CalculateMacros(TrackerInstance instance, Dictionary<string, List<string>> UnlockData, int itterations, Dictionary<object, int> autoObtainedObjects)
        {
            bool MacroStateChanged = false;
            foreach(var i in instance.MacroPool)
            {
                var Logic = LogicMap[i.Key];
                bool Available = false;
                if (Logic.IsTrick && !i.Value.TrickEnabled) { Available = false; }
                else
                {
                    Available = CalculatReqAndCond(Logic, instance, i.Key, null, UnlockData);
                    if (Available) { autoObtainedObjects[i.Value] = itterations; }
                }

                if (Available != i.Value.Aquired)
                {
                    MacroStateChanged = true;
                    i.Value.Aquired = Available;
                }
            }
            return MacroStateChanged;
        }

        public static bool CheckUrandomizedLocations(TrackerInstance instance, Dictionary<string, List<string>> UnlockData, int itterations, Dictionary<object, int> autoObtainedObjects)
        {
            bool ItemStateChanged = false;
            foreach (var i in instance.LocationPool)
            {
                if (string.IsNullOrWhiteSpace(i.Value.GetDictEntry(instance).OriginalItem)) { continue; }
                if (!i.Value.IsUnrandomized(1)) { continue; }
                var Logic = LogicMap[i.Key];
                var Available = CalculatReqAndCond(Logic, instance, i.Key, null, UnlockData);

                bool ShouldBeChecked = Available && i.Value.CheckState != CheckState.Checked;
                bool ShouldBeUnChecked = !Available && i.Value.CheckState == CheckState.Checked;

                if (ShouldBeChecked || ShouldBeUnChecked)
                {
                    ItemStateChanged = true;
                    i.Value.Available = Available;
                    CheckState checkState = i.Value.Available ? CheckState.Checked : CheckState.Unchecked;
                    if (checkState == CheckState.Checked) { i.Value.Randomizeditem.Item = i.Value.GetDictEntry(instance).OriginalItem; }
                    i.Value.ToggleChecked(checkState, instance);
                    if (Available) { autoObtainedObjects[i.Value] = itterations; }
                }
            }
            return ItemStateChanged;
        }

        public static MMRData.JsonFormatLogicItem GetLogic(this TrackerInstance instance, string OriginalID, bool DoEdits = true)
        {
            bool Literal = OriginalID.IsLiteralID(out string ID);
            LogicEntryType entryType = instance.GetLocationEntryType(ID, Literal, out dynamic Obj);
            LogicFileType LogicFile = Obj.referenceData.LogicList;
            int LogicFileIndex = Obj.referenceData.LogicIndex;
            MMRData.JsonFormatLogicItem LogicFileEntry = null;

            switch (LogicFile)
            {
                case LogicFileType.Logic:
                    LogicFileEntry = instance.LogicFile.Logic[LogicFileIndex];
                    break;
                case LogicFileType.Additional:
                    LogicFileEntry = instance.LogicDictionary.AdditionalLogic[LogicFileIndex];
                    break;
                case LogicFileType.Runtime:
                    LogicFileEntry = instance.RuntimeLogic[OriginalID];
                    break;
            }

            Utility.DeepCloneLogic(LogicFileEntry.RequiredItems, LogicFileEntry.ConditionalItems, out List<string> CopyRequirements, out List<List<string>> CopyConditionals);

            if (entryType == LogicEntryType.macro)
            {
                var MacroData = instance.GetMacroByID(ID);
                if (MacroData.Price > -1 && !instance.PriceData.CapacityMap.Values.Contains(ID) && DoEdits) 
                {
                    HandlePriceLogic(instance, MacroData.Price, CopyRequirements, CopyConditionals, out CopyRequirements, out CopyConditionals);
                }
            }
            else if (entryType == LogicEntryType.location)
            {
                var LocationData = instance.GetLocationByID(ID);
                if (LocationData.Price > -1 && !instance.PriceData.CapacityMap.Values.Contains(ID) && DoEdits)
                {
                    HandlePriceLogic(instance, LocationData.Price, CopyRequirements, CopyConditionals, out CopyRequirements, out CopyConditionals);
                }
            }

            if (DoEdits) { HandleOptionLogicEdits(instance.UserOptions.Values, ID, CopyRequirements, CopyConditionals, out CopyRequirements, out CopyConditionals); }

            return new MMRData.JsonFormatLogicItem
            {
                Id = ID,
                IsTrick = LogicFileEntry != null && LogicFileEntry.IsTrick,
                RequiredItems = CopyRequirements,
                ConditionalItems = CopyConditionals,
                TimeAvailable = LogicFileEntry == null ? TimeOfDay.None : LogicFileEntry.TimeAvailable,
                TimeNeeded = LogicFileEntry == null ? TimeOfDay.None : LogicFileEntry.TimeNeeded,
                TimeSetup = LogicFileEntry == null ? TimeOfDay.None : LogicFileEntry.TimeSetup,
                TrickCategory = LogicFileEntry?.TrickCategory,
                TrickTooltip = LogicFileEntry?.TrickTooltip
            };
        }

        public static void HandleOptionLogicEdits(IEnumerable<OptionData.TrackerOption> Options, string ID, List<string> InRequirements, List<List<string>> InConditionals, out List<string> OutRequirements, out List<List<string>> OutConditionals)
        {
            List<string> Requirements = InRequirements;
            List<List<string>> Conditionals = InConditionals;
            foreach (var option in Options)
            {
                foreach (var replacements in option.GetActions().LogicReplacements.Where(x => x.LocationValid(ID)))
                {
                    Requirements = Requirements.Select(x => replacements.ReplacementList.ContainsKey(x) ? replacements.ReplacementList[x] : x).ToList();
                    Conditionals = Conditionals.Select(set => set.Select(x => replacements.ReplacementList.ContainsKey(x) ? replacements.ReplacementList[x] : x).ToList()).ToList();
                }
                foreach (var additionalSet in option.GetActions().AdditionalLogic.Where(x => x.LocationValid(ID)))
                {
                    Requirements = Requirements.Concat(additionalSet.AdditionalRequirements).ToList();
                    Conditionals = Conditionals.Concat(additionalSet.AdditionalConditionals).ToList();
                }
            }
            OutRequirements = Requirements;
            OutConditionals = Conditionals;
        }

        private static void HandlePriceLogic(TrackerInstance instance, int Price, List<string> Requirements, List<List<string>> Conditionals, out List<string> NewRequirements, out List<List<string>> NewConditionals)
        {
            if (!instance.PriceData.CapacityMap.Any())
            {
                NewRequirements = Requirements;
                NewConditionals = Conditionals;
                return; 
            }
            var ValidWallets = instance.PriceData.CapacityMap.Keys.Where(item => item >= Price);
            var MinValue = ValidWallets.Any() ? ValidWallets.Min() : instance.PriceData.CapacityMap.Keys.Max();
            var NewWallet = instance.PriceData.CapacityMap[MinValue];

            var FlattenedLogic = Requirements.Concat(Conditionals.SelectMany(x => x));
            if (FlattenedLogic.Any(x => instance.PriceData.WalletEntries.Contains(x)))
            {
                foreach (var i in instance.PriceData.WalletEntries)
                {
                    Requirements = Requirements.Select(x => x == i ? x.Replace(i, NewWallet) : x.Replace(" ", " ")).ToList();
                    for (var p = 0; p < Conditionals.Count; p++)
                    {
                        Conditionals[p] = Conditionals[p].Select(x => x == i ? x.Replace(i, NewWallet) : x.Replace(" ", " ")).ToList();
                    }
                }
            }
            else { Requirements.Add(NewWallet); }
            NewRequirements = Requirements;
            NewConditionals = Conditionals;
        }

        public static bool MultipleItemEntry(LogicObjects.TrackerInstance instance, string Entry, out string Item, out int Amount)
        {
            Item = Entry;
            Amount = 1;
            if (!Entry.Contains(",")) { return false; }
            var data = Entry.Split(',').Select(x => x.Trim()).ToArray();
            Item = data[0];
            if (data.Length < 2) { return false; }
            if (int.TryParse(data[1].Trim(), out Amount))
            {
                return true;
            }
            else if (instance.Variables.ContainsKey(data[1]) && instance.Variables[data[1]].Value is Int64 amount)
            {
                Amount = (int)amount;
                return true;
            }
            else
            {
                Amount = 1;
                return false;
            }
        }

        public static bool LogicOptionEntry(TrackerInstance instance, string i, out bool optionEntryValid)
        {
            optionEntryValid = false;
            if (!i.Contains("==") && !i.Contains("!=")) { return false; }
            bool inverse = i.Contains("!=");
            string[] data = inverse ? i.Split("!=") : i.Split("==");
            if (data.Length != 2) { return false; }
            optionEntryValid = checkOptionEntry(instance, data, inverse, i);
            return true;
        }

        private static bool checkOptionEntry(TrackerInstance instance, string[] data, bool inverse, string OriginalText)
        {
            bool LiteralOption = data[0].Trim().IsLiteralID(out string CleanedOptionName);
            bool LiteralValue = data[1].Trim().IsLiteralID(out string CleanedOptionValue);

            List<string> OptionList = new() { CleanedOptionName };
            List<string> ValueList = new() { CleanedOptionValue };

            if (instance.Variables.ContainsKey(CleanedOptionName)) 
            { 
                if (instance.Variables[CleanedOptionName].Value is List<string> VarOptionList)
                {
                    OptionList = VarOptionList;
                }
                else if (instance.Variables[CleanedOptionName].Value is string VarOptionString)
                {
                    return (VarOptionString == CleanedOptionValue) != inverse;
                }
                else if (instance.Variables[CleanedOptionName].Value is Int64 VarOptionInt)
                {
                    return (int.TryParse(CleanedOptionValue, out int TryParseValue) && (int)VarOptionInt == TryParseValue) != inverse;
                }
                else if (instance.Variables[CleanedOptionName].Value is bool VarOptionBool)
                {
                    return (bool.TryParse(CleanedOptionValue, out bool TryParseBool) && (bool)VarOptionBool == TryParseBool) != inverse;
                }
            }

            if (instance.Variables.ContainsKey(CleanedOptionValue) && instance.Variables[CleanedOptionValue].Value is List<string> VarValueList) { ValueList = VarValueList; }

            bool RequiremntMet = false;
            foreach (var currentOption in OptionList)
            {
                var CurrentOptionType = instance.GetOptionEntryType(currentOption, LiteralOption, out _);
                foreach (var CurrentValue in ValueList)
                {
                    string CheckValue = CurrentValue;
                    if (!LiteralValue && instance.Variables.ContainsKey(CurrentValue)) { CheckValue = instance.Variables[CurrentValue].ValueToString(); }
                    if (CurrentOptionType == LogicEntryType.Option && (instance.UserOptions[currentOption].CurrentValue == CheckValue)) { RequiremntMet = true; }
                    //This should always return whatever value will result in false if the item is unknown. This is because checking a location should never 
                    //result in another location becoming unavilable otherwise we could enter an infinite loop if both those location are checked automatically
                    else if (CurrentOptionType == LogicEntryType.location && (instance.GetLocationByID(currentOption)?.GetItemAtCheck(instance) == null)) { RequiremntMet = inverse; }
                    else if (CurrentOptionType == LogicEntryType.location && (instance.GetLocationByID(currentOption)?.GetItemAtCheck(instance) == CheckValue)) { RequiremntMet = true; }
                }
            }
            return RequiremntMet != inverse;
        }

        private static bool CheckVariableEntry(string Entry)
        {
            var FunctionLogic = Regex.Matches(Entry, @"(.*?)\((.*?)\)");
            foreach (Match match in FunctionLogic)
            {
                Debug.WriteLine("=======");
                Debug.WriteLine(match.Groups[1].Value);
                Debug.WriteLine(match.Groups[2].Value);
            }
            return false;
        }

        public static bool CheckEntrancePair(TrackerInstance instance)
        {
            bool ChangesMade = false;
            foreach(var i in instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values))
            {
                if (i.CheckState == CheckState.Checked && i.EntrancePair != null)
                {
                    var EntrancePair = instance.EntrancePool.GetEntrancePairOfDestination(i.DestinationExit);
                    if (EntrancePair.Available && EntrancePair.CheckState != CheckState.Checked)
                    {
                        EntrancePair.DestinationExit = i.GetDestnationFromEntrancePair();
                        EntrancePair.ToggleExitChecked(CheckState.Checked, instance);
                    }
                }
            }
            return ChangesMade;
        }

        public static bool UnCheckEntrancePair(TrackerInstance instance)
        {
            bool ChangesMade = false;
            foreach (var i in instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values))
            {
                if (i.CheckState != CheckState.Checked && i.EntrancePair != null)
                {
                    var EntrancePair = instance.EntrancePool.GetEntrancePairOfDestination(i.DestinationExit);
                    if (EntrancePair.CheckState == CheckState.Checked)
                    {
                        EntrancePair.DestinationExit = i.GetDestnationFromEntrancePair();
                        EntrancePair.ToggleExitChecked(CheckState.Unchecked, instance);
                    }
                }
            }
            return ChangesMade;
        }

    }
}
