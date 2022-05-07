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
    public class LogicCalculation
    {
        public readonly InstanceContainer container = new InstanceContainer();
        public Dictionary<string, List<string>> LogicUnlockData = new Dictionary<string, List<string>>();
        public Dictionary<string, MMRData.JsonFormatLogicItem> LogicMap = new Dictionary<string, MMRData.JsonFormatLogicItem>();
        public Dictionary<object, int> AutoObtainedObjects = new Dictionary<object, int>();

        public LogicCalculation(InstanceContainer _container)
        {
            container = _container;
        }

        private bool RequirementsMet(List<string> Requirements, string ID, Dictionary<string, List<string>> TempUnlockData)
        {
            List<string> SubUnlockData = new List<string>();
            bool reqMet = Requirements.All(x => LogicEntryAquired(x, SubUnlockData));
            if (TempUnlockData != null && reqMet)
            {
                if (!TempUnlockData.ContainsKey(ID)) { TempUnlockData.Add(ID, new List<string>()); }
                TempUnlockData[ID] = TempUnlockData[ID].Concat(Requirements).Concat(SubUnlockData).ToList();
            }
            return reqMet;
        }

        private bool ConditionalsMet(List<List<string>> Conditionals, string ID, Dictionary<string, List<string>> TempUnlockData)
        {
            if (!Conditionals.Any()) { return true; }
            var CondMet = Conditionals.FirstOrDefault(x => RequirementsMet(x, ID, TempUnlockData));
            return CondMet != null;
        }

        public bool LogicEntryAquired(string i, List<string> SubUnlockData)
        {
            if (container.Instance.LogicOptionEntry(i, out bool OptionEntryValid)) { return OptionEntryValid; }

            container.Instance.MultipleItemEntry(i, out string LogicItem, out int Amount);
            bool Literal = LogicItem.IsLiteralID(out LogicItem);
            var type = container.Instance.GetItemEntryType(LogicItem, Literal, out _);

            switch (type)
            {
                case LogicEntryType.Bool:
                    return bool.Parse(LogicItem);
                case LogicEntryType.item:
                    return container.Instance.GetItemByID(LogicItem).Useable(container.Instance, Amount);
                case LogicEntryType.macro:
                    return container.Instance.GetMacroByID(LogicItem).Aquired; 
                case LogicEntryType.Area:
                    return AreaReached(LogicItem);
                case LogicEntryType.variableString:
                    return LogicEntryAquired(container.Instance.Variables[LogicItem].Value as string, SubUnlockData);
                case LogicEntryType.variableList:
                    return ItemArrayUseable(LogicItem, Amount, SubUnlockData);
                case LogicEntryType.variableBool:
                    return container.Instance.Variables[LogicItem].Value;
                default:
                    Debug.WriteLine($"{LogicItem} Was not a valid Logic Entry");
                    return false;
            }
        }

        private bool ItemArrayUseable(string ArrVar, int amount, List<string> SubUnlockData)
        {
            if (container.Instance.Variables[ArrVar].Value is not List<string> VariableEntries) { return false; }
            int UseableItems = 0;
            List<string> UsedMacros = new List<string>();
            Dictionary<string, int> ItemTracking = new Dictionary<string, int>();
            bool CountMet = false;
            foreach (string i in VariableEntries)
            {
                if (UseableItems >= amount) { CountMet = true; break; }
                if (LogicEntryAquired(i, SubUnlockData))
                {
                    bool MultiItem = container.Instance.MultipleItemEntry(i, out string LogicItem, out int Amount);
                    bool Literal = LogicItem.IsLiteralID(out LogicItem);
                    var type = container.Instance.GetItemEntryType(LogicItem, Literal, out object ItemObj);

                    if (type == LogicEntryType.item && !MultiItem)
                    {
                        for (var j = 0; j < (ItemObj as ItemObject).GetTotalUsable(container.Instance); j++)
                        {
                            if (!ItemTracking.ContainsKey(LogicItem)) { ItemTracking.Add(LogicItem, 0); }
                            ItemTracking[LogicItem]++;
                            UseableItems++;
                            if (UseableItems >= amount) { CountMet = true; break; }
                        }
                    }
                    else
                    {
                        UsedMacros.Add(i);
                        UseableItems++;
                    }

                }
            }
            if (CountMet)
            {
                foreach (var x in UsedMacros) { SubUnlockData.Add(x); }
                foreach (var x in ItemTracking) { SubUnlockData.Add($"{x.Key}, {x.Value}"); }
            }
            return CountMet;
        }

        private bool AreaReached(string Area)
        {
            if (Area == container.Instance.EntrancePool.RootArea) { return true; }
            return container.Instance.EntrancePool.AreaList.ContainsKey(Area) && container.Instance.EntrancePool.AreaList[Area].ExitsAcessibleFrom > 0;
        }

        public bool CalculatReqAndCond(MMRData.JsonFormatLogicItem Logic, string ID, string Area)
        {
            Dictionary<string, List<string>> TempUnlockData = new Dictionary<string, List<string>>();
            bool Available =
                (Area == null || AreaReached(Area)) &&
                RequirementsMet(Logic.RequiredItems, ID, TempUnlockData) &&
                ConditionalsMet(Logic.ConditionalItems, ID, TempUnlockData);

            if (!LogicUnlockData.ContainsKey(ID) && Available)
            {
                LogicUnlockData[ID] = TempUnlockData[ID];
            }

            return Available;
        }

        private void FillLogicMap()
        {
            foreach (var i in container.Instance.MacroPool) 
            { 
                LogicMap[i.Key] = container.Instance.GetLogic(i.Key); 
            }
            foreach (var i in container.Instance.LocationPool)
            {
                LogicMap[i.Key] = container.Instance.GetLogic(i.Key); 
            }
            foreach (var i in container.Instance.HintPool)
            {
                LogicMap[i.Key] = container.Instance.GetLogic(i.Key); 
            }
            foreach (var Area in container.Instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.LoadingZoneExits.Concat(Area.Value.MacroExits))
                {
                    LogicMap[container.Instance.GetLogicNameFromExit(i.Value)] = container.Instance.GetLogic(container.Instance.GetLogicNameFromExit(i.Value));
                }
            }
        }

        public void CalculateLogic(CheckState checkState = CheckState.Unchecked)
        {
            Debug.WriteLine("Logic Calculation ----------------------");
            Stopwatch LogicTime = new Stopwatch();
            Utility.TimeCodeExecution(LogicTime);

            FillLogicMap();
            Utility.TimeCodeExecution(LogicTime, "Getting Logic", 1);

            if (checkState == CheckState.Unchecked) { ResetAutoObtainedItems(); }

            Debug.WriteLine(checkState);
            Utility.TimeCodeExecution(LogicTime, "Resetting Auto checked Items", 1);

            AutoObtainedObjects.Clear();

            int Itterations = 0;
            while (true)
            {
                bool UnrandomizedItemChecked = CheckUrandomizedLocations(Itterations);
                bool UnrandomizedExitChecked = CheckUrandomizedExits(Itterations);
                bool MacroChanged = CalculateMacros(Itterations);
                Itterations++;
                 if (!MacroChanged && !UnrandomizedItemChecked && !UnrandomizedExitChecked) { break; }
            }
            Debug.WriteLine($"Auto Item calculation took {Itterations} Itterations");
            Utility.TimeCodeExecution(LogicTime, "Calculating Auto Checked Items", 1);
            foreach (var i in container.Instance.LocationPool.Values.Where(x => !x.IsUnrandomized(1) && !x.IsJunk()))
            {
                var Logic = LogicMap[i.ID];
                i.Available = CalculatReqAndCond(Logic, i.ID, null);
            }
            Utility.TimeCodeExecution(LogicTime, "Calculating Locations", 1);
            foreach (var Area in container.Instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.LoadingZoneExits.Where(x => !x.Value.IsUnrandomized(1) && !x.Value.IsJunk()))
                {
                    var Logic = LogicMap[container.Instance.GetLogicNameFromExit(i.Value)];
                    i.Value.Available = CalculatReqAndCond(Logic, container.Instance.GetLogicNameFromExit(i.Value), Area.Key);
                }
            }
            Utility.TimeCodeExecution(LogicTime, "Caclulating Exits", 1);
            foreach (var i in container.Instance.HintPool)
            {
                var Logic = LogicMap[i.Key];
                i.Value.Available = CalculatReqAndCond(Logic, i.Key, null);
            }
            Utility.TimeCodeExecution(LogicTime, "Calculating Hints", 1);

            if (checkState == CheckState.Unchecked) { CleanUnlockData(); }
            
            Utility.TimeCodeExecution(LogicTime, "Clean Unlock Data", -1);
        }

        private void CleanUnlockData()
        {
            //If an entry in the unlock data is no longer available, remove it from the unlock data.
            //This has to be done all at once after all logic calculations are done because unrandomized items and macros
            //become unckecked and rechecked during logic calculation and we don't want them resetting their unlock data when this happens.
            foreach (var i in container.Instance.LocationPool.Values)
            {
                if (!i.Available && LogicUnlockData.ContainsKey(i.ID)) { LogicUnlockData.Remove(i.ID); }
            }
            foreach (var Area in container.Instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.LoadingZoneExits.Values.Concat(Area.Value.MacroExits.Values))
                {
                    var ID = container.Instance.GetLogicNameFromExit(i);
                    if (!i.Available && LogicUnlockData.ContainsKey(ID)) { LogicUnlockData.Remove(ID); }
                }
            }
            foreach (var i in container.Instance.HintPool.Values)
            {
                if (!i.Available && LogicUnlockData.ContainsKey(i.ID)) { LogicUnlockData.Remove(i.ID); }
            }
            foreach (var i in container.Instance.MacroPool.Values)
            {
                if (!i.Aquired && LogicUnlockData.ContainsKey(i.ID)) { LogicUnlockData.Remove(i.ID); }
            }
        }

        private void ResetAutoObtainedItems()
        {
            foreach (var Area in container.Instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.LoadingZoneExits.Where(x => x.Value.IsUnrandomized(1)).Concat(Area.Value.MacroExits))
                {
                    i.Value.ToggleExitChecked(CheckState.Unchecked, container.Instance);
                }
            }
            foreach (var i in container.Instance.LocationPool.Where(x => x.Value.IsUnrandomized(1)))
            {
                i.Value.ToggleChecked(CheckState.Unchecked, container.Instance);
            }
        }

        private bool CheckUrandomizedExits(int itterations)
        {
            bool ItemStateChanged = false;
            foreach (var Area in container.Instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.LoadingZoneExits.Where(x => x.Value.IsUnrandomized(1)).Concat(Area.Value.MacroExits))
                {
                    var Logic = LogicMap[container.Instance.GetLogicNameFromExit(i.Value)];
                    var Available = CalculatReqAndCond(Logic, container.Instance.GetLogicNameFromExit(i.Value), Area.Key);
                    bool ShouldBeChecked = Available && i.Value.CheckState != CheckState.Checked;
                    bool ShouldBeUnChecked = !Available && i.Value.CheckState == CheckState.Checked;
                    if (ShouldBeChecked || ShouldBeUnChecked)
                    {
                        ItemStateChanged = true;
                        i.Value.Available = Available;
                        CheckState checkState = i.Value.Available ? CheckState.Checked : CheckState.Unchecked;
                        if (checkState == CheckState.Checked) { i.Value.DestinationExit = i.Value.GetVanillaDestination(); }
                        i.Value.ToggleExitChecked(checkState, container.Instance);
                        if (Available) { AutoObtainedObjects[i.Value] = itterations; }
                    }
                }
            }
            return ItemStateChanged;
        }

        private bool CalculateMacros(int itterations)
        {
            bool MacroStateChanged = false;
            foreach(var i in container.Instance.MacroPool)
            {
                var Logic = LogicMap[i.Key];
                bool Available = false;
                if (Logic.IsTrick && !i.Value.TrickEnabled) { Available = false; }
                else
                {
                    Available = CalculatReqAndCond(Logic, i.Key, null);
                    if (Available) { AutoObtainedObjects[i.Value] = itterations; }
                }

                if (Available != i.Value.Aquired)
                {
                    MacroStateChanged = true;
                    i.Value.Aquired = Available;
                }
            }
            return MacroStateChanged;
        }

        private bool CheckUrandomizedLocations(int itterations)
        {
            bool ItemStateChanged = false;
            foreach (var i in container.Instance.LocationPool.Where(x => x.Value.IsUnrandomized(1)))
            {
                if (string.IsNullOrWhiteSpace(i.Value.GetDictEntry(container.Instance).OriginalItem)) { continue; }
                var Logic = LogicMap[i.Key];
                var Available = CalculatReqAndCond(Logic, i.Key, null);
                bool ShouldBeChecked = Available && i.Value.CheckState != CheckState.Checked;
                bool ShouldBeUnChecked = !Available && i.Value.CheckState == CheckState.Checked;
                if (ShouldBeChecked || ShouldBeUnChecked)
                {
                    ItemStateChanged = true;
                    i.Value.Available = Available;
                    CheckState checkState = i.Value.Available ? CheckState.Checked : CheckState.Unchecked;
                    if (checkState == CheckState.Checked) { i.Value.Randomizeditem.Item = i.Value.GetDictEntry(container.Instance).OriginalItem; }
                    i.Value.ToggleChecked(checkState, container.Instance);
                    if (Available) { AutoObtainedObjects[i.Value] = itterations; }
                }
            }
            return ItemStateChanged;
        }

    }

    public static class LogicEditing
    {
        public static void HandlePriceLogic(TrackerInstance instance, int Price, List<string> Requirements, List<List<string>> Conditionals, out List<string> NewRequirements, out List<List<string>> NewConditionals)
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

        public static bool LogicOptionEntry(this TrackerInstance instance, string i, out bool optionEntryValid)
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

        public static bool CheckEntrancePair(this TrackerInstance instance)
        {
            bool ChangesMade = false;
            foreach (var i in instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values))
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

        public static bool UnCheckEntrancePair(this TrackerInstance instance)
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
