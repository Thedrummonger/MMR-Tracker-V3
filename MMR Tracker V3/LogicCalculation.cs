using MathNet.Symbolics;
using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.FSharp.Core.LanguagePrimitives;
using static MMR_Tracker_V3.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.LogicDictionaryData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public class LogicCalculation
    {
        public readonly InstanceContainer container;
        public Dictionary<string, List<string>> LogicUnlockData = new();
        public Dictionary<string, MMRData.JsonFormatLogicItem> LogicMap = new();
        public Dictionary<object, int> AutoObtainedObjects = new();
        public bool ReCompileLogicOnCalculation = false;

        public LogicCalculation(InstanceContainer _container)
        {
            container = _container;
        }

        private bool RequirementsMet(List<string> Requirements, List<string> TempUnlockData)
        {
            List<string> SubUnlockData = new();
            foreach(var Req in Requirements)
            {
                if (!LogicEntryAquired(Req, SubUnlockData)) { return false; }
            }
            TempUnlockData?.AddRange(SubUnlockData);
            return true;
        }

        public bool ConditionalsMet(List<List<string>> Conditionals, List<string> TempUnlockData)
        {
            if (!Conditionals.Any()) { return true; }
            List<string> SubUnlockData = new List<string>();
            foreach(var Set in Conditionals)
            {
                if (RequirementsMet(Set, SubUnlockData)) 
                {
                    TempUnlockData.AddRange(SubUnlockData);
                    return true; 
                }
            }
            return false;
        }

        public bool LogicEntryAquired(string i, List<string> SubUnlockData)
        {
            if (LogicEditing.CheckLogicFunction(container.Instance, i, SubUnlockData, out bool FunctionEntryValid))
            {
                return FunctionEntryValid;
            }

            container.Instance.MultipleItemEntry(i, out string LogicItem, out int Amount);
            bool Literal = LogicItem.IsLiteralID(out LogicItem);
            var type = container.Instance.GetItemEntryType(LogicItem, Literal, out _);

            switch (type)
            {
                case LogicEntryType.Bool:
                    return bool.Parse(i);
                case LogicEntryType.item:
                    SubUnlockData.Add(i);
                    return container.Instance.GetItemByID(LogicItem).Useable(container.Instance, Amount);
                case LogicEntryType.macro:
                    SubUnlockData.Add(i);
                    return container.Instance.GetMacroByID(LogicItem).Aquired; 
                case LogicEntryType.Area:
                    return AreaReached(LogicItem, SubUnlockData);
                case LogicEntryType.LogicEntryCollection:
                    return CheckItemArray(LogicItem, Amount, SubUnlockData, out int _);
                default:
                    Debug.WriteLine($"{LogicItem} Was not a valid Logic Entry");
                    return false;
            }
        }

        public bool CheckItemArray(string ArrVar, int amount, List<string> SubUnlockData, out int TotalUsable)
        {
            var EditActions = container.Instance.GetOptionActions();
            TotalUsable = 0;
            if (!container.Instance.LogicEntryCollections.ContainsKey(ArrVar)) { return false; }
            List<string> VariableEntries = container.Instance.LogicEntryCollections[ArrVar].GetValue(container.Instance);

            List<string> UsableItems = new();
            Dictionary<string, int> ItemTracking = new();
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
                    if (type == LogicEntryType.LogicEntryCollection) { LoopVarEntry((ItemObj as OptionData.LogicEntryCollection).GetValue(container.Instance)); }
                    else
                    {
                        if (type == LogicEntryType.item && !MultiItem)
                        { UsableItems.AddRange(Enumerable.Repeat(LogicItem, (ItemObj as ItemObject).GetTotalUsable(container.Instance))); }
                        else if (LogicEntryAquired(i, SubUnlockData)) { UsableItems.Add(i); }
                    }
                }
            }
        }

        private bool AreaReached(string Area, List<string> TempUnlockData = null)
        {
            bool IsRoot = Area is null || Area == container.Instance.EntrancePool.RootArea;
            string TargetArea = Area ??container.Instance.EntrancePool.RootArea;
            bool Reachable = IsRoot || container.Instance.EntrancePool.AreaList.ContainsKey(Area) && container.Instance.EntrancePool.AreaList[Area].ExitsAcessibleFrom > 0;
            if (TempUnlockData != null && Reachable)
            {
                TempUnlockData.Add(TargetArea);
            }
            return Reachable;
        }

        public bool CalculatReqAndCond(MMRData.JsonFormatLogicItem Logic, string ID, string Area)
        {
            List<string> UnlockedWith = new List<string>();
            bool Available =
                (Area == null || AreaReached(Area, UnlockedWith)) &&
                RequirementsMet(Logic.RequiredItems, UnlockedWith) &&
                ConditionalsMet(Logic.ConditionalItems, UnlockedWith);

            if (!LogicUnlockData.ContainsKey(ID) && Available)
            {
                LogicUnlockData[ID] = UnlockedWith.Distinct().ToList();
            }

            return Available;
        }

        public void CompileOptionActionEdits()
        {
            LogicMap.Clear();
            var Actions = container.Instance.GetOptionActions();
            Debug.WriteLine("Recompiling Logic");
            foreach (var i in container.Instance.MacroPool) 
            { 
                LogicMap[i.Key] = container.Instance.GetLogic(i.Key, actions: Actions); 
            }
            foreach (var i in container.Instance.LocationPool)
            {
                LogicMap[i.Key] = container.Instance.GetLogic(i.Key, actions: Actions); 
            }
            foreach (var i in container.Instance.HintPool)
            {
                LogicMap[i.Key] = container.Instance.GetLogic(i.Key, actions: Actions); 
            }
            foreach (var Area in container.Instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.Exits)
                {
                    LogicMap[container.Instance.GetLogicNameFromExit(i.Value)] = container.Instance.GetLogic(container.Instance.GetLogicNameFromExit(i.Value), actions: Actions);
                }
            }
            container.Instance.InstanceReference.OptionActionItemEdits.Clear();
            foreach (var i in container.Instance.LogicEntryCollections)
            {
                container.Instance.InstanceReference.OptionActionCollectionEdits[i.Key] = i.Value.GetOptionEditDefinedValue(Actions);
            }
            foreach (var i in container.Instance.ItemPool)
            {
                container.Instance.InstanceReference.OptionActionItemEdits[i.Key] = new OptionData.ActionItemEdit
                {
                    Name = i.Value.GetDictEntry(container.Instance).GetOptionEditDefinedName(Actions),
                    MaxAmount = i.Value.GetDictEntry(container.Instance).GetOptionEditDefinedMaxAmountInWorld(Actions)
                };
            }
        }

        public void CalculateLogic(CheckState checkState = CheckState.Unchecked)
        {
            bool IncompleteLogicMap = container.Instance.LogicFile.Logic.Any(x => !LogicMap.ContainsKey(x.Id));
            if (ReCompileLogicOnCalculation || IncompleteLogicMap) { CompileOptionActionEdits(); }
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
            foreach (var i in container.Instance.LocationPool.Values.Where(x => !x.IsUnrandomized(UnrandState.Unrand)))
            {
                var Logic = LogicMap[i.ID];
                i.Available = CalculatReqAndCond(Logic, i.ID, null);
            }
            foreach (var Area in container.Instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.RandomizableExits(container.Instance).Where(x => !x.Value.IsUnrandomized(UnrandState.Unrand)))
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
                foreach (var i in Area.Value.Exits.Values)
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

        public void ResetAutoObtainedItems()
        {
            foreach (var Area in container.Instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.Exits.Where(x => x.Value.IsUnrandomized(UnrandState.Unrand) || !x.Value.IsRandomizableEntrance(container.Instance)))
                {
                    i.Value.ToggleExitChecked(CheckState.Unchecked, container.Instance);
                }
            }
            foreach (var i in container.Instance.LocationPool.Where(x => x.Value.IsUnrandomized(UnrandState.Unrand)))
            {
                i.Value.ToggleChecked(CheckState.Unchecked, container.Instance);
            }
        }

        private bool CheckUrandomizedExits(int itterations)
        {
            bool ItemStateChanged = false;
            foreach (var Area in container.Instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.Exits.Where(x => x.Value.IsUnrandomized(UnrandState.Unrand) || !x.Value.IsRandomizableEntrance(container.Instance)))
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
            foreach (var i in container.Instance.LocationPool.Where(x => x.Value.IsUnrandomized(MiscData.UnrandState.Unrand)))
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
                foreach (var Wallet in instance.PriceData.WalletEntries)
                {
                    Requirements = Requirements.Select(x => x == Wallet ? NewWallet : x).ToList();
                    Conditionals = Conditionals.Select(set => set.Select(x => x == Wallet ? NewWallet : x).ToList()).ToList();
                }
            }
            else { Requirements.Add(NewWallet); }
            NewRequirements = Requirements;
            NewConditionals = Conditionals;
        }

        public static void HandleOptionLogicEdits(IEnumerable<OptionData.Action> Actions, string ID, List<string> InRequirements, List<List<string>> InConditionals, out List<string> OutRequirements, out List<List<string>> OutConditionals)
        {
            List<string> Requirements = InRequirements;
            List<List<string>> Conditionals = InConditionals;
            foreach (var action in Actions)
            {
                foreach (var replacements in action.LogicReplacements.Where(x => x.LocationValid(ID)))
                {
                    Requirements = Requirements.Select(x => replacements.ReplacementList.ContainsKey(x) && ID != replacements.ReplacementList[x] ? replacements.ReplacementList[x] : x).ToList();
                    Conditionals = Conditionals.Select(set => set.Select(x => replacements.ReplacementList.ContainsKey(x) && ID != replacements.ReplacementList[x] ? replacements.ReplacementList[x] : x).ToList()).ToList();
                }
                foreach (var additionalSet in action.AdditionalLogic.Where(x => x.LocationValid(ID)))
                {
                    Requirements = Requirements.Concat(additionalSet.AdditionalRequirements).ToList();
                    Conditionals = Conditionals.Concat(additionalSet.AdditionalConditionals).ToList();
                }
            }
            OutRequirements = Requirements;
            OutConditionals = Conditionals;
        }

        public static bool IsLogicFunction(string i, out string Func, out string Param, Tuple<char, char> functionCasing = null)
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

        public static bool CheckLogicFunction(TrackerInstance instance, string i, List<string> SubUnlockData, out bool LogicFuntionValid, bool DoCheck = true)
        {
            LogicFuntionValid=false;

            if (!IsLogicFunction(i, out string Func, out string Param)) { return false; }

            switch (Func)
            {
                case "check":
                case "available":
                    if (!DoCheck) { return true; }
                    LogicFuntionValid = CheckAvailableFunction(instance, Func, Param);
                    break;
                case "contains":
                    if (!DoCheck) { return true; }
                    LogicFuntionValid = CheckContainsFunction(instance, Param.Split(",").Select(x => x.Trim()).ToArray());
                    break;
                case "renewable":
                    if (!DoCheck) { return true; }
                    LogicFuntionValid = CheckRenewableFunction(instance, Param.Split(",").Select(x => x.Trim()).ToArray());
                    break;
                case "rand":
                case "randomized":
                    if (!DoCheck) { return true; }
                    LogicFuntionValid = CheckIsRandomizedFunction(instance, Param.Split(",").Select(x => x.Trim()).ToArray());
                    break;
                case "trick":
                    if (!DoCheck) { return true; }
                    LogicFuntionValid = CheckTrickEnabledFunction(instance, Param.Split(",").Select(x => x.Trim()).ToArray());
                    break;
                case "setting":
                case "option":
                    if (!DoCheck) { return true; }
                    LogicFuntionValid = CheckOptionFunction(instance, Param.Split(",").Select(x => x.Trim()).ToArray());
                    break;
                case "comment":
                case "cmnt":
                case "cc":
                    LogicFuntionValid = true; //Comment Style Check. Will always return true but can state why its returning true and what it's expecting
                    break;
                case "time":
                    LogicFuntionValid = true; //The tracker currently doesn't track time, but this can at least display the data for logic 
                    break;
                default: 
                    return false;
            }

            return true;
        }

        private static bool CheckRenewableFunction(TrackerInstance instance, string[] strings)
        {
            var ShouldBeRepeatable = true;
            if (strings.Length > 1 && bool.TryParse(strings[1], out bool Mod))
            {
                ShouldBeRepeatable = Mod;
            }
            
            var item = instance.GetItemByID(strings[0]);
            if (item is null) { Debug.WriteLine($"{strings[0]} Was not a valid Renewable Item Entry"); return false; }
            if (item.GetTotalUsable(instance) < 1) { return false; }
            return instance.LocationPool.Values.Any(x => 
                x.CheckState == CheckState.Checked && 
                IsAtProperCheck(x, ShouldBeRepeatable) && 
                (x.Randomizeditem.OwningPlayer == -1) && 
                x.Randomizeditem.Item == item.ID);
            bool IsAtProperCheck(LocationObject x, bool ShouldBeRepeatable)
            {
                if (ShouldBeRepeatable) { return x.GetDictEntry(instance).Repeatable is not null && (bool)x.GetDictEntry(instance).Repeatable; }
                else { return x.GetDictEntry(instance).Repeatable is not null && !(bool)x.GetDictEntry(instance).Repeatable; }
            }
        }

        private static bool CheckIsRandomizedFunction(TrackerInstance instance, string[] strings)
        {
            bool Inverse = strings.Length > 1 && bool.TryParse(strings[1], out bool InverseParam) && !InverseParam;
            bool IsLitteral = strings[0].IsLiteralID(out string CleanedID);
            instance.GetLocationEntryType(CleanedID, IsLitteral, out dynamic OBJ);
            if (OBJ is null) { Debug.WriteLine($"{strings[0]} is not a valid logic Entry"); return false; }
            if (!Utility.DynamicPropertyExist(OBJ, "RandomizedState")) { Debug.WriteLine($"{strings[0]} is not a randomizable entry"); return false; }
            RandomizedState randomizedState = OBJ.RandomizedState;
            return (randomizedState == RandomizedState.Randomized) != Inverse;
        }

        private static bool CheckTrickEnabledFunction(TrackerInstance instance, string[] strings)
        {
            bool Inverse = strings.Length > 1 && bool.TryParse(strings[1], out bool InverseParam) && !InverseParam;
            if (!instance.MacroPool.ContainsKey(strings[0])) { Debug.WriteLine($"{strings[0]} Was not a valid Trick Macro"); return false; }  
            if (!instance.MacroPool[strings[0]].isTrick(instance)) { Debug.WriteLine($"{strings[0]} Was a valid macro but was not a trick"); return false; }
            return instance.MacroPool[strings[0]].TrickEnabled != Inverse;
        }

        private static bool CheckAvailableFunction(TrackerInstance instance, string func, string paramString)
        {
            string[] Params = paramString.Split(',').Select(x => x.Trim()).ToArray();
            bool Inverted = Params.Length > 1 && bool.TryParse(Params[1], out bool IsInverted) && !IsInverted;
            bool litteral = Params[0].IsLiteralID(out string paramClean);

            var type = instance.GetLocationEntryType(paramClean, litteral, out dynamic obj);
            if (obj is null) { Debug.WriteLine($"{Params[0]} is not a valid logic Entry"); return false; }

            if (func == "check" && Utility.DynamicPropertyExist(obj, "RandomizedState") && obj.RandomizedState == RandomizedState.ForcedJunk) { func = "available"; }

            if (func == "check" && Utility.DynamicPropertyExist(obj, "CheckState")) { return (obj.CheckState == CheckState.Checked) != Inverted; }
            else if (func == "available" && Utility.DynamicPropertyExist(obj, "Available")) { return  obj.Available != Inverted; }
            else if (func == "available" && Utility.DynamicPropertyExist(obj, "Aquired")) { return obj.Aquired != Inverted; }
            else { Debug.WriteLine($"{paramClean} could not be checked. Type: {type}"); }
            return false;
        }

        public static bool CheckOptionFunction(TrackerInstance instance, string[] param)
        {
            if (param.Length < 1) { return false; } //No Values Pased

            var OptionType = instance.GetLocationEntryType(param[0], false, out _);
            bool Inverse = param.Length > 2 && bool.TryParse(param[2], out bool inverseValue) && !inverseValue;

            if (OptionType == LogicEntryType.ToggleOption)
            {
                var TogOpt = instance.ToggleOptions[param[0]];
                if (param.Length < 2 || TogOpt.Enabled.ID == param[1]) { return (TogOpt.GetValue() == TogOpt.Enabled) != Inverse; }
                else if (TogOpt.Disabled.ID == param[1]) { return (TogOpt.GetValue() == TogOpt.Disabled) != Inverse; }
                else if (bool.TryParse(param[1], out bool ParamBool))
                {
                    if (ParamBool) { return (TogOpt.GetValue() == TogOpt.Enabled) != Inverse; }
                    else { return (TogOpt.GetValue() == TogOpt.Disabled) != Inverse; }
                }
                else
                {
                    return false;
                }
            }
            else if (OptionType == LogicEntryType.ChoiceOption)
            {
                var ChoiceOpt = instance.ChoiceOptions[param[0]];
                if (param.Length < 2) { return false; } //Not enough values passed
                if (!ChoiceOpt.ValueList.ContainsKey(param[1])) { Debug.WriteLine($"{param[1]} was not a valid Value for option {ChoiceOpt.ID}"); }
                return (ChoiceOpt.GetValue().ID == param[1]) != Inverse;
            }
            else if (OptionType == LogicEntryType.MultiSelectOption)
            {
                var ChoiceOpt = instance.MultiSelectOptions[param[0]];
                if (param.Length < 2) { return false; } //Not enough values passed
                if (!ChoiceOpt.ValueList.ContainsKey(param[1])) { Debug.WriteLine($"{param[1]} was not a valid Value for option {ChoiceOpt.ID}"); }
                return (ChoiceOpt.EnabledValues.Contains(param[1])) != Inverse;
            }
            else if (OptionType == LogicEntryType.IntOption)
            {
                var IntOpt = instance.IntOptions[param[0]];
                if (param.Length < 2) { return false; } //Not enough values passed
                if (!int.TryParse(param[1], out int ParamInt)) { return false; }
                return (IntOpt.Value == ParamInt) != Inverse;
            }
            else
            {
                return false;
            }
        }

        private static bool CheckContainsFunction(TrackerInstance instance, string[] param)
        {
            if (param.Length < 2) { return false; } //Not enough Values Pased

            string FunctionValue = param[0];
            string ParamValue = param[1];
            List<string> OptionList = new() { FunctionValue };
            List<string> ValueList = new() { ParamValue };

            if (instance.LogicEntryCollections.ContainsKey(FunctionValue))
            {
                OptionList = instance.LogicEntryCollections[FunctionValue].GetValue(instance);
            }

            if (instance.LogicEntryCollections.ContainsKey(ParamValue))
            {
                ValueList = instance.LogicEntryCollections[ParamValue].GetValue(instance);
            }

            bool inverse = param.Length > 2 && bool.TryParse(param[2], out bool inverseValue) && !inverseValue; //If a third param is passed and its a false bool, invert the result

            bool RequiremntMet = false;
            foreach (var currentOption in OptionList)
            {
                var CurrentOptionType = instance.GetLocationEntryType(currentOption, false, out object CurrentOptionEntry);
                foreach (var CurrentValue in ValueList)
                {
                    //This should always return whatever value will result in false if the item is unknown. This is because checking a location should never 
                    //result in another location becoming unavilable otherwise we could enter an infinite loop if both those location are checked automatically
                    if (CurrentOptionType == LogicEntryType.location && (instance.GetLocationByID(currentOption)?.GetItemAtCheck(instance) == null)) { RequiremntMet = inverse; }
                    else if (CurrentOptionType == LogicEntryType.location && (instance.GetLocationByID(currentOption)?.GetItemAtCheck(instance) == CurrentValue)) { RequiremntMet = true; }
                    else if (CurrentOptionType == LogicEntryType.Exit && (CurrentOptionEntry as EntranceData.EntranceRandoExit)?.DestinationExit == null) { RequiremntMet = inverse; }
                    else if (CurrentOptionType == LogicEntryType.Exit && (CurrentOptionEntry as EntranceData.EntranceRandoExit).LeadsToArea(CurrentValue)) { RequiremntMet = true; }
                }
            }
            return RequiremntMet != inverse;

        }
    }
}
