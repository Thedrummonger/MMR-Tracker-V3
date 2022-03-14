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

        public static bool RequirementsMet(List<string> Requirements, TrackerInstance instance)
        {
            return Requirements.All(x => LogicEntryAquired(instance, x));
        }

        public static bool ConditionalsMet(List<List<string>> Conditionals, TrackerInstance instance)
        {
            if (!Conditionals.Any()) { return true; }
            return Conditionals.Any(x => RequirementsMet(x, instance));
        }

        public static bool LogicEntryAquired(TrackerInstance instance, string i)
        {
            if (LogicOptionEntry(instance, i, out bool OptionEntryValid)) { return OptionEntryValid; }

            MultipleItemEntry(instance, i, out string LogicItem, out int Amount);
            bool Literal = LogicItem.IsLiteralID(out LogicItem);
            var type = instance.GetItemEntryType(LogicItem, Literal);

            switch (type)
            {
                case LogicEntryType.Bool:
                    return bool.Parse(LogicItem);
                case LogicEntryType.item:
                    return instance.GetItemByID(LogicItem).Useable(Amount);
                case LogicEntryType.macro:
                    return instance.GetMacroByID(LogicItem).Aquired; 
                case LogicEntryType.Area:
                    return AreaReached(LogicItem, instance);
                case LogicEntryType.variableString:
                    return LogicEntryAquired(instance, instance.Variables[LogicItem].Value as string);
                case LogicEntryType.variableList:
                    return ItemArrayUseable(instance, LogicItem, Amount);
                case LogicEntryType.variableBool:
                    return instance.Variables[LogicItem].Value;
                default:
                    Debug.WriteLine($"{LogicItem} Was not a valid Logic Entry");
                    return false;
            }
        }

        private static bool ItemArrayUseable(TrackerInstance instance, string LogicItem, int amount)
        {
            if (instance.Variables[LogicItem].Value is not List<string> ItemArray) { return false; }
            int UseableItems = 0;
            foreach (string i in ItemArray)
            {
                if (LogicEntryAquired(instance, i)) { UseableItems ++; }
            }
            return UseableItems >= amount;
        }

        private static bool AreaReached(string Area, TrackerInstance instance)
        {
            if (Area == instance.EntrancePool.RootArea) { return true; }
            return instance.EntrancePool.AreaList.ContainsKey(Area) && instance.EntrancePool.AreaList[Area].ExitsAcessibleFrom > 0;
        }

        public static void CalculateLogic(TrackerInstance instance, bool log = false)
        {
            Debug.WriteLine("Logic Calculation ----------------------");
            Stopwatch LogicTime = new Stopwatch();
            Utility.TimeCodeExecution(LogicTime);
            ResetAutoObtainedItems(instance);
            Utility.TimeCodeExecution(LogicTime, "Resetting Auto checked Items", 1);
            int Itterations = 0;
            while (true)
            {
                bool EntranceChanges = CalculateEntranceMacros(instance);
                bool UnrandomizedItemChecked = CheckUrandomizedLocations(instance);
                bool MacroChanged = CalculateMacros(instance);
                Itterations++;
                if (!MacroChanged && !EntranceChanges && !UnrandomizedItemChecked) { break; }
            }
            Debug.WriteLine($"Auto Item calculation took {Itterations} Itterations");
            Utility.TimeCodeExecution(LogicTime, "Calculating Auto Checked Items", 1);
            foreach (var i in instance.LocationPool.Values.Where(x => !x.IsUnrandomized(1) && !x.IsJunk()))
            {
                var Logic = instance.GetLogic(i.ID);
                i.Available =
                    RequirementsMet(Logic.RequiredItems, instance) &&
                    ConditionalsMet(Logic.ConditionalItems, instance);
            }
            Utility.TimeCodeExecution(LogicTime, "Calculating Locations", 1);
            foreach (var Area in instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.LoadingZoneExits)
                {
                    var Logic = instance.GetLogic(instance.EntrancePool.GetLogicNameFromExit(i.Value));
                    i.Value.Available =
                        AreaReached(Area.Key, instance) &&
                        RequirementsMet(Logic.RequiredItems, instance) &&
                        ConditionalsMet(Logic.ConditionalItems, instance);
                }
            }
            Utility.TimeCodeExecution(LogicTime, "Caclulating Exits", 1);
            foreach (var i in instance.HintPool)
            {
                var Logic = instance.GetLogic(i.Key);
                i.Value.Available =
                    RequirementsMet(Logic.RequiredItems, instance) &&
                    ConditionalsMet(Logic.ConditionalItems, instance);
            }
            Utility.TimeCodeExecution(LogicTime, "Calculating Hints", 1);
            Debug.WriteLine("----------------------------------------");
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
            }
            foreach(var i in instance.LocationPool.Where(x => x.Value.IsUnrandomized(1)))
            {
                if (i.Value.CheckState == CheckState.Checked)
                {
                    i.Value.ToggleChecked(CheckState.Unchecked, instance);
                }
            }
        }

        private static bool CalculateEntranceMacros(TrackerInstance instance)
        {
            bool MacroStateChanged = false; 
            foreach (var Area in instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.MacroExits)
                {
                    var Logic = instance.GetLogic(instance.EntrancePool.GetLogicNameFromExit(i.Value));
                    bool Available =
                        AreaReached(Area.Key, instance) &&
                        RequirementsMet(Logic.RequiredItems, instance) && 
                        ConditionalsMet(Logic.ConditionalItems, instance);
                    if (Available && i.Value.CheckState != CheckState.Checked)
                    {
                        MacroStateChanged = true;
                        i.Value.CheckState = CheckState.Checked;
                        var Destination = instance.EntrancePool.AreaList[i.Value.ID];
                        Destination.ExitsAcessibleFrom++;
                    }
                    else if (!Available && i.Value.CheckState == CheckState.Checked)
                    {
                        MacroStateChanged = true;
                        i.Value.CheckState = CheckState.Unchecked;
                        var Destination = instance.EntrancePool.AreaList[i.Value.ID];
                        Destination.ExitsAcessibleFrom--;
                    }
                }
            }
            return MacroStateChanged;
        }

        public static bool CalculateMacros(TrackerInstance instance)
        {
            bool MacroStateChanged = false;
            foreach(var i in instance.MacroPool)
            {
                var Logic = instance.GetLogic(i.Key);
                bool Available;
                if (Logic.IsTrick && !i.Value.TrickEnabled) { Available = false; }
                else 
                {
                    Available =
                        RequirementsMet(Logic.RequiredItems, instance) &&
                        ConditionalsMet(Logic.ConditionalItems, instance);
                }

                if (Available != i.Value.Aquired)
                {
                    MacroStateChanged = true;
                    i.Value.Aquired = Available;
                }
            }
            return MacroStateChanged;
        }

        public static bool CheckUrandomizedLocations(TrackerInstance instance)
        {
            bool ItemStateChanged = false;
            foreach (var i in instance.LocationPool)
            {
                if (!i.Value.IsUnrandomized(1)) { continue; }
                var Logic = instance.GetLogic(i.Key);
                var Available =
                    RequirementsMet(Logic.RequiredItems, instance) &&
                    ConditionalsMet(Logic.ConditionalItems, instance);

                bool ShouldBeChecked = Available && i.Value.CheckState != CheckState.Checked;
                bool ShouldBeUnChecked = !Available && i.Value.CheckState == CheckState.Checked;

                if (ShouldBeChecked || ShouldBeUnChecked)
                {
                    ItemStateChanged = true;
                    i.Value.Available = Available;
                    CheckState checkState = i.Value.Available ? CheckState.Checked : CheckState.Unchecked;
                    if (checkState == CheckState.Checked) { i.Value.Randomizeditem.Item = i.Value.GetDictEntry(instance).OriginalItem; }
                    i.Value.ToggleChecked(checkState, instance);
                }
            }
            return ItemStateChanged;
        }

        public static MMRData.JsonFormatLogicItem GetLogic(this TrackerInstance instance, string OriginalID)
        {

            bool Literal = OriginalID.IsLiteralID(out string ID);
            var Logic = new MMRData.JsonFormatLogicItem() { Id = ID, IsTrick = false };
            List<string> Requirements = new();
            List<List<string>> Conditionals = new();
            int CheckPrice = -1;

            LogicEntryType entryType = instance.GetLocationEntryType(ID, Literal);
            if (entryType == LogicEntryType.macro)
            {
                var MacroData = instance.GetMacroByID(ID);
                ID = GetDynamicLogicID(instance, ID);
                if (ID == null) { return Utility.CreateInaccessableLogic(ID); }
                if (MacroData.MacroPrice > -1 && !instance.PriceData.CapacityMap.Values.Contains(ID)) { CheckPrice = MacroData.MacroPrice; }
            }
            else if (entryType == LogicEntryType.location)
            {
                var LocationData = instance.GetLocationByID(ID);
                if (LocationData.CheckPrice > -1 && !instance.PriceData.CapacityMap.Values.Contains(ID)) { CheckPrice = LocationData.CheckPrice; }
            }

            if (instance.InstanceReference.LogicFileMapping.ContainsKey(ID))
            {
                int Index = instance.InstanceReference.LogicFileMapping[ID];
                var LogicEntry = instance.LogicFile.Logic[Index];
                Logic.IsTrick = LogicEntry.IsTrick;
                Logic.TimeAvailable = LogicEntry.TimeAvailable;
                Logic.TimeNeeded = LogicEntry.TimeNeeded;
                Logic.TimeSetup = LogicEntry.TimeSetup;
                Utility.DeepCloneLogic(LogicEntry.RequiredItems, LogicEntry.ConditionalItems, out Requirements, out Conditionals);
            }
            if (instance.LogicOverride.ContainsKey(ID))
            {
                List<string> OverrideRequirements = Requirements;
                List<List<string>> OverrideConditionals = Conditionals;
                if (instance.LogicOverride[ID].RequiredItems != null) { OverrideRequirements = instance.LogicOverride[ID].RequiredItems; }
                if (instance.LogicOverride[ID].ConditionalItems != null) { OverrideConditionals = instance.LogicOverride[ID].ConditionalItems; }
                Utility.DeepCloneLogic(OverrideRequirements, OverrideConditionals, out Requirements, out Conditionals);
            }

            var ValidOptions = instance.UserOptions.Values.Where(x => x.GetActions().LocationValid(ID));
            if (ValidOptions.Any()) { HandleOptionLogicEdits(ValidOptions, ID, Requirements, Conditionals, out Requirements, out Conditionals); }

            if (CheckPrice > -1) { HandlePriceLogic(instance, CheckPrice, Requirements, Conditionals, out Requirements, out Conditionals); }

            Logic.RequiredItems = Requirements;
            Logic.ConditionalItems = Conditionals;
            return Logic;
        }

        public static void HandleOptionLogicEdits(IEnumerable<OptionData.TrackerOption> Options, string ID, List<string> InRequirements, List<List<string>> InConditionals, out List<string> OutRequirements, out List<List<string>> OutConditionals)
        {
            List<string> Requirements = InRequirements;
            List<List<string>> Conditionals = InConditionals;
            foreach (var option in Options)
            {
                foreach (var replacements in option.GetActions().LogicReplacements)
                {
                    if (!replacements.LocationValid(ID)) { continue; }
                    foreach (var i in replacements.ReplacementList)
                    {
                        Requirements = Requirements
                            .Select(x => x == i.Target ? x.Replace(i.Target, i.Replacement) : x.Replace(" ", " ")).ToList();
                        for (var p = 0; p < Conditionals.Count; p++)
                        {
                            Conditionals[p] = Conditionals[p]
                                .Select(x => x == i.Target ? x.Replace(i.Target, i.Replacement) : x.Replace(" ", " ")).ToList();
                        }
                    }
                }
                foreach (var additionalSet in option.GetActions().AdditionalLogic)
                {
                    if (!additionalSet.LocationValid(ID)) { continue; }
                    foreach (var i in additionalSet.AdditionalRequirements) { Requirements.Add(i); }
                    foreach (var i in additionalSet.AdditionalConditionals) { Conditionals.Add(i); }
                }
            }
            OutRequirements = Requirements;
            OutConditionals = Conditionals;
        }

        private static void HandlePriceLogic(TrackerInstance instance, int Price, List<string> Requirements, List<List<string>> Conditionals, out List<string> NewRequirements, out List<List<string>> NewConditionals)
        {
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

            if (instance.Variables.ContainsKey(CleanedOptionName) && instance.Variables[CleanedOptionName].Value is List<string> VarOptionList) { OptionList = VarOptionList; }
            if (instance.Variables.ContainsKey(CleanedOptionValue) && instance.Variables[CleanedOptionValue].Value is List<string> VarValueList) { ValueList = VarValueList; }

            bool RequiremntMet = false;
            foreach (var currentOption in OptionList)
            {
                var CurrentOptionType = instance.GetOptionEntryType(currentOption, LiteralOption);
                foreach (var CurrentValue in ValueList)
                {
                    string CheckValue = CurrentValue;
                    if (!LiteralValue && instance.Variables.ContainsKey(CurrentValue)) { CheckValue = instance.Variables[CurrentValue].ValueToString(); }
                    if (CurrentOptionType == LogicEntryType.Option && (instance.UserOptions[currentOption].CurrentValue == CheckValue)) { RequiremntMet = true; }
                    //This should always return whatever value will result in false if the item is unknown. This is because checking a location should never 
                    //result in another location becoming unavilable otherwise we could enter an infinite loop if both those location are checked automatically
                    else if (CurrentOptionType == LogicEntryType.location && (instance.LocationPool[currentOption].GetItemAtCheck(instance) == null)) { RequiremntMet = inverse; }
                    else if (CurrentOptionType == LogicEntryType.location && (instance.LocationPool[currentOption].GetItemAtCheck(instance) == CheckValue)) { RequiremntMet = true; }
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

        public static MMRData.JsonFormatLogicItem GetOriginalLogic(this TrackerInstance instance, string ID, bool copy = false)
        {
            bool Literal = ID.IsLiteralID(out ID);

            List<string> Requirements = new();
            List<List<string>> Conditionals = new();
            if (instance.InstanceReference.LogicFileMapping.ContainsKey(ID))
            {
                int Index = instance.InstanceReference.LogicFileMapping[ID];
                var LogicEntry = instance.LogicFile.Logic[Index];
                if (!copy) { return LogicEntry; }
                Utility.DeepCloneLogic(LogicEntry.RequiredItems, LogicEntry.ConditionalItems, out Requirements, out Conditionals);
            }

            var Logic = new MMRData.JsonFormatLogicItem()
            {
                Id = ID,
                RequiredItems = Requirements,
                ConditionalItems = Conditionals,
                IsTrick = false
            };

            return Logic;
        }

        public static string GetDynamicLogicID(TrackerInstance instance, string ID)
        {
            var MacroObject = instance.GetMacroByID(ID);
            var MacroDictObject = MacroObject.GetDictEntry(instance);
            if (MacroDictObject.DynamicLogicData != null)
            {
                if (!instance.LocationPool.ContainsKey(MacroDictObject.DynamicLogicData.LocationToCompare)) { return ID; }
                var LocationToCompare = instance.LocationPool[MacroDictObject.DynamicLogicData.LocationToCompare];
                foreach (var arg in MacroDictObject.DynamicLogicData.Arguments)
                {
                    if (LocationToCompare.GetItemAtCheck(instance) == arg.ItemAtLocation)
                    {
                        return arg.LogicToUse;
                    }
                }
                return null;
            }
            else
            {
                return ID;
            }
        }
    }
}
