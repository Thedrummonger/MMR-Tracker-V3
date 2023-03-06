using MathNet.Symbolics;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static MMR_Tracker_V3.LogicObjects;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.LogicDictionaryData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public class LogicCalculation
    {
        public readonly InstanceContainer container;
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

            if (CheckLogicFunction(i, SubUnlockData, out bool FunctionEntryValid)) { return FunctionEntryValid; }

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
                    return AreaReached(LogicItem, "");
                case LogicEntryType.variableString:
                    return LogicEntryAquired(container.Instance.Variables[LogicItem].Value as string, SubUnlockData);
                case LogicEntryType.variableList:
                    return checkItemArray(LogicItem, Amount, SubUnlockData, out int _);
                case LogicEntryType.variableBool:
                    return container.Instance.Variables[LogicItem].Value;
                default:
                    Debug.WriteLine($"{LogicItem} Was not a valid Logic Entry");
                    return false;
            }
        }

        public bool CheckLogicFunction(string i, List<string> SubUnlockData, out bool LogicFuntionValid)
        {
            LogicFuntionValid=false;
            if (i.IsLiteralID(out _)) { return false; }
            bool squirFunc = i.EndsWith('}') && i.Contains("{");

            if (!squirFunc) { return false; }
            Tuple <char, char> functionCasing = new('{', '}');

            int funcEnd = i.IndexOf(functionCasing.Item1);
            int paramStart = i.IndexOf(functionCasing.Item1) + 1;
            int paramEnd = i.IndexOf(functionCasing.Item2);
            string Func = i[..funcEnd].Trim().ToLower();
            string Param = i[paramStart..paramEnd].Trim();

            switch (Func)
            {
                case "check":
                case "available":
                    bool litteral = Param.IsLiteralID(out Param);
                    container.Instance.GetLocationEntryType(Param, litteral, out dynamic obj);
                    if (Func == "check" && Utility.DynamicPropertyExist(obj, "CheckState")) { LogicFuntionValid = obj.CheckState == CheckState.Checked; }
                    else if (Func == "available" && Utility.DynamicPropertyExist(obj, "Available")) { LogicFuntionValid =  obj.Available; }
                    break;
            }

            return true;
        }

        public bool checkItemArray(string ArrVar, int amount, List<string> SubUnlockData, out int TotalUsable)
        {
            TotalUsable = 0;
            if (container.Instance.Variables[ArrVar].Value is not List<string> VariableEntries) { return false; }
            List<string> UsableItems = new List<string>();
            Dictionary<string, int> ItemTracking = new Dictionary<string, int>();
            LoopVarEntry(VariableEntries);
            bool CountMet = UsableItems.Count >= amount;
            TotalUsable = UsableItems.Count;

            foreach (var i in UsableItems.Take(amount))
            {
                if (!ItemTracking.ContainsKey(i)) { ItemTracking.Add(i, 0); }
                ItemTracking[i]++;
            }
            if (CountMet) { foreach (var x in ItemTracking) { SubUnlockData.Add($"{x.Key}, {x.Value}"); } }
            return CountMet;

            void LoopVarEntry(List<string> VarList)
            {
                foreach (var i in VarList)
                {
                    bool MultiItem = container.Instance.MultipleItemEntry(i, out string LogicItem, out int Amount);
                    bool Literal = LogicItem.IsLiteralID(out LogicItem);
                    var type = container.Instance.GetItemEntryType(LogicItem, Literal, out object ItemObj);
                    if (type == LogicEntryType.variableList) { LoopVarEntry((ItemObj as TrackerVariable).Value as List<string>); }
                    else
                    {
                        if (type == LogicEntryType.item && !MultiItem)
                        { UsableItems.AddRange(Enumerable.Repeat(LogicItem, (ItemObj as ItemObject).GetTotalUsable(container.Instance))); }
                        else if (LogicEntryAquired(i, SubUnlockData)) { UsableItems.Add(i); }
                    }
                }
            }
        }

        private bool AreaReached(string Area, string ID, Dictionary<string, List<string>> TempUnlockData = null)
        {
            bool IsRoot = Area is null || Area == container.Instance.EntrancePool.RootArea;
            string TargetArea = (Area == null) ? container.Instance.EntrancePool.RootArea : Area;
            bool Reachable = IsRoot || container.Instance.EntrancePool.AreaList.ContainsKey(Area) && container.Instance.EntrancePool.AreaList[Area].ExitsAcessibleFrom > 0;
            if (TempUnlockData != null && Reachable)
            {
                if (!TempUnlockData.ContainsKey(ID)) { TempUnlockData.Add(ID, new List<string>()); }
                TempUnlockData[ID].Add(TargetArea);
            }
            return Reachable;
        }

        public bool CalculatReqAndCond(MMRData.JsonFormatLogicItem Logic, string ID, string Area)
        {
            Dictionary<string, List<string>> TempUnlockData = new Dictionary<string, List<string>>();
            bool Available =
                (Area == null || AreaReached(Area, ID, TempUnlockData)) &&
                RequirementsMet(Logic.RequiredItems, ID, TempUnlockData) &&
                ConditionalsMet(Logic.ConditionalItems, ID, TempUnlockData);

            if (!LogicUnlockData.ContainsKey(ID) && Available)
            {
                LogicUnlockData[ID] = TempUnlockData[ID].Distinct().ToList();
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
            FillLogicMap();
            if (checkState == CheckState.Unchecked) { ResetAutoObtainedItems(); }
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
            foreach (var i in container.Instance.LocationPool.Values.Where(x => !x.IsUnrandomized(1) && !x.IsJunk()))
            {
                var Logic = LogicMap[i.ID];
                i.Available = CalculatReqAndCond(Logic, i.ID, null);
            }
            foreach (var Area in container.Instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.LoadingZoneExits.Where(x => !x.Value.IsUnrandomized(1) && !x.Value.IsJunk()))
                {
                    var Logic = LogicMap[container.Instance.GetLogicNameFromExit(i.Value)];
                    i.Value.Available = CalculatReqAndCond(Logic, container.Instance.GetLogicNameFromExit(i.Value), Area.Key);
                }
            }
            foreach (var i in container.Instance.HintPool)
            {
                var Logic = LogicMap[i.Key];
                i.Value.Available = CalculatReqAndCond(Logic, i.Key, null);
            }

            if (checkState == CheckState.Unchecked) { CleanUnlockData(); }
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
        public static void HandlePriceLogic(TrackerInstance instance, int Price, char Currency, List<string> Requirements, List<List<string>> Conditionals, out List<string> NewRequirements, out List<List<string>> NewConditionals)
        {
            if (!instance.PriceData.GetCapacityMap(Currency).Any())
            {
                NewRequirements = Requirements;
                NewConditionals = Conditionals;
                return;
            }
            var ValidWallets = instance.PriceData.GetCapacityMap(Currency).Keys.Where(item => item >= Price);
            var MinValue = ValidWallets.Any() ? ValidWallets.Min() : instance.PriceData.GetCapacityMap(Currency).Keys.Max();
            var NewWallet = instance.PriceData.GetCapacityMap(Currency)[MinValue];

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
            bool WasSetting = true;
            bool WasCompare = true;

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
            else { WasSetting = false; }

            if (instance.Variables.ContainsKey(CleanedOptionValue) && instance.Variables[CleanedOptionValue].Value is List<string> VarValueList) { ValueList = VarValueList; }

            bool RequiremntMet = false;
            foreach (var currentOption in OptionList)
            {
                var CurrentOptionType = instance.GetOptionEntryType(currentOption, LiteralOption, out object CurrentOptionEntry);
                if (CurrentOptionType != LogicEntryType.Option && CurrentOptionType != LogicEntryType.location && CurrentOptionType != LogicEntryType.Exit) { WasCompare = false; }
                foreach (var CurrentValue in ValueList)
                {
                    string CheckValue = CurrentValue;
                    if (!LiteralValue && instance.Variables.ContainsKey(CurrentValue)) { CheckValue = instance.Variables[CurrentValue].ValueToString(); }
                    if (CurrentOptionType == LogicEntryType.Option && (instance.UserOptions[currentOption].CurrentValue == CheckValue)) { RequiremntMet = true; }
                    //This should always return whatever value will result in false if the item is unknown. This is because checking a location should never 
                    //result in another location becoming unavilable otherwise we could enter an infinite loop if both those location are checked automatically
                    else if (CurrentOptionType == LogicEntryType.location && (instance.GetLocationByID(currentOption)?.GetItemAtCheck(instance) == null)) { RequiremntMet = inverse; }
                    else if (CurrentOptionType == LogicEntryType.location && (instance.GetLocationByID(currentOption)?.GetItemAtCheck(instance) == CheckValue)) { RequiremntMet = true; }
                    else if (CurrentOptionType == LogicEntryType.Exit && (CurrentOptionEntry as EntranceData.EntranceRandoExit)?.DestinationExit == null) { RequiremntMet = inverse; }
                    else if (CurrentOptionType == LogicEntryType.Exit && ExitLeadsToArea(CurrentOptionEntry, CheckValue)) { RequiremntMet = true; }
                }
            }
            if (!WasCompare && !WasSetting) { Debug.WriteLine($"{OriginalText} may not be a valid Option Entry"); }
            return RequiremntMet != inverse;
        }

        private static bool ExitLeadsToArea(object Exit, string Area)
        {
            EntranceData.EntranceRandoExit Entrance = Exit as EntranceData.EntranceRandoExit;
            if (Entrance.DestinationExit is not null && Entrance.DestinationExit.region == Area) { return true; }
            return false;
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
