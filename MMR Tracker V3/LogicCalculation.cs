using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        private static bool RequirementsMet(List<string> Requirements, LogicObjects.TrackerInstance instance)
        {
            foreach (string i in Requirements)
            {
                if (LogicOptionEntry(instance, i, out bool OptionEntryValid))
                {
                    if (OptionEntryValid) { continue; }
                    else { return false; }
                }

                MultipleItemEntry(i, out string LogicItem, out int Amount);
                bool Literal = LogicItem.IsLiteralID(out LogicItem);
                var type = instance.GetItemEntryType(LogicItem, Literal);

                if (type == LogicEntryType.Bool)
                {
                    if (bool.TryParse(LogicItem, out bool BoolEntry) && !BoolEntry) { return false; }
                }
                else if(type == LogicEntryType.item)
                {
                    if (!instance.GetItemByID(LogicItem, "RequirementsMet").Useable(Amount)) { return false; }
                }
                else if (type == LogicEntryType.macro)
                {
                    var MacroObject = instance.GetMacroByID(LogicItem);
                    if (!MacroObject.Aquired) { return false; }
                }
                else 
                {
                    Debug.WriteLine($"{LogicItem} Was not a valid Item");
                    return false; 
                }
            }
            return true;
        }

        private static bool ConditionalsMet(List<List<string>> Conditionals, LogicObjects.TrackerInstance instance)
        {
            if (!Conditionals.Any()) { return true; }
            foreach (var i in Conditionals)
            {
                if (RequirementsMet(i, instance)) { return true; }
            }
            return false;
        }

        public static void CalculateLogic(LogicObjects.TrackerInstance instance)
        {
            Stopwatch stopwatch = new Stopwatch();
            AverageTime = new List<int>();

            Utility.TimeCodeExecution(stopwatch);
            while (true)
            {
                bool MacroChanged = CalculateMacros(instance);
                bool UnrandomizedItemAquired = CheckUrandomizedLocations(instance);
                if (!MacroChanged && !UnrandomizedItemAquired) { break; }
            }
            Utility.TimeCodeExecution(stopwatch, "Macro and Unrandomized Item Calculation", 1);
            foreach (var i in instance.LocationPool.Values.Where(x => x.RandomizedState != RandomizedState.Unrandomized))
            {
                var Logic = instance.GetLogic(i.ID);
                i.Available = RequirementsMet(Logic.RequiredItems, instance) && ConditionalsMet(Logic.ConditionalItems, instance);
            }
            Utility.TimeCodeExecution(stopwatch, "Location Calculation", 1);
            foreach (var i in instance.HintPool)
            {
                var Logic = instance.GetLogic(i.Key);
                i.Value.Available = RequirementsMet(Logic.RequiredItems, instance) && ConditionalsMet(Logic.ConditionalItems, instance);
            }
            Utility.TimeCodeExecution(stopwatch, "Hint Location Calculation", 1);
            Debug.WriteLine($"MultiItem Check took {AverageTime.Sum() / TimeSpan.TicksPerMillisecond}");
        }

        public static bool CalculateMacros(LogicObjects.TrackerInstance instance)
        {
            bool MacroStateChanged = false;
            foreach(var i in instance.MacroPool)
            {
                var Logic = instance.GetLogic(i.Key);
                bool Available;
                if (Logic.IsTrick && !i.Value.TrickEnabled) { Available = false; }
                else { Available = RequirementsMet(Logic.RequiredItems, instance) && ConditionalsMet(Logic.ConditionalItems, instance); }

                if (Available != i.Value.Aquired)
                {
                    MacroStateChanged = true;
                    i.Value.Aquired = Available;
                }
            }
            return MacroStateChanged;
        }

        public static MMRData.JsonFormatLogicItem GetLogic(this LogicObjects.TrackerInstance instance, string OriginalID)
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
            else
            {
                Requirements.Add(NewWallet);
            }
            NewRequirements = Requirements;
            NewConditionals = Conditionals;
        }

        public static bool CheckUrandomizedLocations(LogicObjects.TrackerInstance instance)
        {
            bool ItemStateChanged = false;
            foreach (var i in instance.LocationPool.Where(x => x.Value.RandomizedState == RandomizedState.Unrandomized))
            {
                var Logic = instance.GetLogic(i.Key);
                var Available = RequirementsMet(Logic.RequiredItems, instance) && ConditionalsMet(Logic.ConditionalItems, instance);

                bool ShouldBeChecked = Available && i.Value.CheckState == CheckState.Unchecked;
                bool ShouldBeUnChecked = !Available && i.Value.CheckState == CheckState.Checked;

                if (ShouldBeChecked || ShouldBeUnChecked)
                {
                    ItemStateChanged = true;
                    i.Value.Available = Available;
                    CheckState checkState = i.Value.Available ? CheckState.Checked : CheckState.Unchecked;
                    if (checkState == CheckState.Unchecked) { i.Value.Randomizeditem.Item = null; }
                    if (checkState == CheckState.Checked) { i.Value.Randomizeditem.Item = i.Value.GetItemAtCheck(instance); }

                    i.Value.ToggleChecked(checkState, instance);
                }
            }
            return ItemStateChanged;
        }

        public static bool MultipleItemEntry(string Entry, out string Item, out int Amount)
        {
            Item = Entry;
            Amount = 1;
            if (!Entry.Contains(",")) { return false; }
            var data = Entry.Split(',');
            Item = data[0];
            if(!int.TryParse(data[1].Trim(), out Amount)) { return false; }
            return true;
        }

        public static bool LogicOptionEntry(LogicObjects.TrackerInstance instance, string i, out bool optionEntryValid)
        {
            optionEntryValid = false;
            if (!i.Contains("==") && !i.Contains("!=")) { return false; }
            bool inverse = i.Contains("!=");
            string[] data = inverse ? i.Split("!=") : i.Split("==");
            if (data.Length != 2) { return false; }
            optionEntryValid = checkOptionEntry(instance, data, inverse);
            return true;
        }

        private static bool checkOptionEntry(LogicObjects.TrackerInstance instance, string[] data, bool inverse)
        {
            bool LiteralOption = data[0].Trim().IsLiteralID(out string CleanedOptionName);
            bool LiteralValue = data[1].Trim().IsLiteralID(out string CleanedOptionValue);
            var OptionType = instance.GetLocationEntryType(CleanedOptionName, LiteralOption);

            if (OptionType == LogicEntryType.Option)
            {
                var Option = instance.UserOptions[CleanedOptionName];
                return (Option.CurrentValue == CleanedOptionValue) == !inverse;
            }
            else if (OptionType == LogicEntryType.location)
            {
                var Location = instance.LocationPool[CleanedOptionName];
                return (Location.GetItemAtCheck(instance) == CleanedOptionValue) == !inverse;
            }
            return false;
        }

        public static MMRData.JsonFormatLogicItem GetOriginalLogic(this LogicObjects.TrackerInstance instance, string ID, bool copy = false)
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
