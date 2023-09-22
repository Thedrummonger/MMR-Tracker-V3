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
using static Microsoft.FSharp.Core.LanguagePrimitives;
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
        public Dictionary<string, List<string>> LogicUnlockData = new();
        public Dictionary<string, MMRData.JsonFormatLogicItem> LogicMap = new();
        public Dictionary<object, int> AutoObtainedObjects = new();

        public LogicCalculation(InstanceContainer _container)
        {
            container = _container;
        }

        private bool RequirementsMet(List<string> Requirements, string ID, Dictionary<string, List<string>> TempUnlockData)
        {
            List<string> SubUnlockData = new();
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
                    return bool.Parse(LogicItem);
                case LogicEntryType.item:
                    return container.Instance.GetItemByID(LogicItem).Useable(container.Instance, Amount);
                case LogicEntryType.macro:
                    return container.Instance.GetMacroByID(LogicItem).Aquired; 
                case LogicEntryType.Area:
                    return AreaReached(LogicItem, "");
                case LogicEntryType.variableString:
                    return LogicEntryAquired(container.Instance.Variables[LogicItem].GetValue(container.Instance) as string, SubUnlockData);
                case LogicEntryType.variableList:
                    return CheckItemArray(LogicItem, Amount, SubUnlockData, out int _);
                case LogicEntryType.variableBool:
                    return container.Instance.Variables[LogicItem].GetValue(container.Instance);
                default:
                    Debug.WriteLine($"{LogicItem} Was not a valid Logic Entry");
                    return false;
            }
        }

        public bool CheckItemArray(string ArrVar, int amount, List<string> SubUnlockData, out int TotalUsable)
        {
            TotalUsable = 0;
            if (container.Instance.Variables[ArrVar].GetType() != VariableEntryType.varlist) { return false; }
            List<string> VariableEntries = (List<string>)container.Instance.Variables[ArrVar].GetValue(container.Instance);
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
                    if (type == LogicEntryType.variableList) { LoopVarEntry((ItemObj as OptionData.TrackerVar).GetValue(container.Instance) as List<string>); }
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
            string TargetArea = Area ??container.Instance.EntrancePool.RootArea;
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
            Dictionary<string, List<string>> TempUnlockData = new();
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
                foreach (var i in Area.Value.Exits)
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
                case "var":
                case "variable":
                    if (!DoCheck) { return true; }
                    LogicFuntionValid = CheckVarFunction(instance, Param.Split(",").Select(x => x.Trim()).ToArray());
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
                x.Randomizeditem.Item == item.Id);
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
            else if (func == "available" && Utility.DynamicPropertyExist(obj, "Available")) { Debug.WriteLine($"Is Available: {obj.Available}"); return  obj.Available != Inverted; }
            else if (func == "available" && Utility.DynamicPropertyExist(obj, "Aquired")) { Debug.WriteLine($"Is Available: {obj.Aquired}"); return obj.Aquired != Inverted; }
            else { Debug.WriteLine($"{paramClean} could not be checked. Type: {type}"); }
            return false;
        }

        private static bool CheckVarFunction(TrackerInstance instance, string[] param)
        {
            if (param.Length < 1) { return false; } //No Values Pased

            //Parse Function Values
            string FunctionRawValue = param[0];
            if (!instance.Variables.ContainsKey(FunctionRawValue)) { return false; }
            VariableEntryType functype = instance.Variables[FunctionRawValue].GetType();
            object funcValue = instance.Variables[FunctionRawValue].GetValue(instance);

            //If a parameter was not passed, check if the function was a bool var
            if (param.Length < 2)
            {
                if (functype == VariableEntryType.varbool) { return (bool)funcValue; }
                else { return false; }
            }

            //Parse Parameter Values
            string ParamRawValue = param[1];
            OptionData.TrackerVar ParamAsVarObj = new() { Value = ParamRawValue };
            VariableEntryType paramtype = ParamAsVarObj.GetType();
            object paramValue = ParamAsVarObj.Value;

            bool inverse = param.Length > 2 && bool.TryParse(param[2], out bool inverseValue) && !inverseValue; //If a third param is passed and its a false bool, invert the result

            switch (functype)
            {
                case VariableEntryType.error:
                case VariableEntryType.varstring:
                    if (paramtype == VariableEntryType.varlist) { return GetReturnValue((paramValue as List<string>).Contains(funcValue.ToString())); }
                    return GetReturnValue((string)funcValue == paramValue.ToString());
                case VariableEntryType.varint:
                    if (paramtype == VariableEntryType.varlist) { return GetReturnValue((paramValue as List<string>).Contains(funcValue.ToString())); }
                    return GetReturnValue(int.TryParse(paramValue.ToString(), out int intresult) && (int)funcValue == intresult);
                case VariableEntryType.varbool:
                    if (paramtype == VariableEntryType.varlist) { return GetReturnValue((paramValue as List<string>).Contains(funcValue.ToString())); }
                    return GetReturnValue( bool.TryParse(paramValue.ToString(), out bool BoolResult) && (bool)funcValue == BoolResult);
                case VariableEntryType.varlist:
                    if (paramtype == VariableEntryType.varlist) { return GetReturnValue((funcValue as List<string>).ToHashSet().SetEquals(funcValue as List<string>)); }
                    return GetReturnValue((funcValue as List<string>).Contains(paramValue.ToString()));
                default: return false;
            }

            bool GetReturnValue(bool expression)
            {
                return expression != inverse;
            }
        }

        public static bool CheckOptionFunction(TrackerInstance instance, string[] param)
        {
            if (param.Length < 1) { return false; } //No Values Pased

            //Evaluate the Function Param
            object funcValue = param[0]; //The function as an object after its variable value has been parsed
            string funcValueString = param[0]; //The function as an strng after its variable value has been parsed
            VariableEntryType functype = instance.Variables.ContainsKey(param[0]) ? instance.Variables[param[0]].GetType() : VariableEntryType.error;
            if (functype != VariableEntryType.error) { funcValueString = instance.Variables[param[0]].ToString(); }
            List<string> OptionList = functype == VariableEntryType.varlist ? (List<string>)funcValue : new() { funcValueString };

            //Evaluate the Value Param
            object varValue;
            string varValueString;
            VariableEntryType vartype;
            if (param.Length < 2) 
            { 
                varValue = OptionData.GetToggleValues().Keys.ToList(); 
                varValueString = "true";
                vartype = VariableEntryType.varlist;
            }
            else 
            { 
                varValue = param[1]; 
                varValueString = param[1];
                vartype = instance.Variables.ContainsKey(param[1]) ? instance.Variables[param[1]].GetType() : VariableEntryType.error;
                if (vartype != VariableEntryType.error) { varValueString = instance.Variables[param[1]].ToString(); }
            }
            List<string> ValueList = vartype == VariableEntryType.varlist ? (List<string>)varValue : new() { varValueString };

            bool inverse = param.Length > 2 && bool.TryParse(param[2], out bool inverseValue) && !inverseValue;

            bool RequiremntMet = false;
            foreach (var currentOption in OptionList)
            {
                foreach (var CurrentValue in ValueList)
                {
                    if (instance.UserOptions.ContainsKey(currentOption) && instance.UserOptions[currentOption].CurrentValue == CurrentValue) { RequiremntMet = true; }
                    if (!instance.UserOptions.ContainsKey(currentOption)) { Debug.WriteLine($"{currentOption} Was not a valid Option"); }
                }
            }
            return RequiremntMet != inverse;

        }

        private static bool CheckContainsFunction(TrackerInstance instance, string[] param)
        {
            if (param.Length < 2) { return false; } //Not enough Values Pased

            string FunctionValue = param[0];
            string ParamValue = param[1];
            List<string> OptionList = new() { FunctionValue };
            List<string> ValueList = new() { ParamValue };

            if (instance.Variables.ContainsKey(FunctionValue))
            {
                bool IsList = instance.Variables[FunctionValue].GetType() == MiscData.VariableEntryType.varlist;
                OptionList = IsList ? (List<string>)instance.Variables[FunctionValue].GetValue(instance) : new() { instance.Variables[FunctionValue].GetValue(instance) };
            }

            if (instance.Variables.ContainsKey(ParamValue))
            {
                bool IsList = instance.Variables[ParamValue].GetType() == MiscData.VariableEntryType.varlist;
                ValueList = IsList ? (List<string>)instance.Variables[ParamValue].GetValue(instance) : new() { instance.Variables[ParamValue].GetValue(instance) };
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
                    else if (CurrentOptionType == LogicEntryType.Exit && ExitLeadsToArea(CurrentOptionEntry, CurrentValue)) { RequiremntMet = true; }
                }
            }
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
            foreach (var i in instance.EntrancePool.AreaList.Values.SelectMany(x => x.RandomizableExits(instance).Values))
            {
                if (i.CheckState == CheckState.Checked && i.EntrancePair != null)
                {
                    var EntrancePair = instance.EntrancePool.GetEntrancePairOfDestination(i.DestinationExit);
                    if (EntrancePair == null) { continue; }
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
            foreach (var i in instance.EntrancePool.AreaList.Values.SelectMany(x => x.RandomizableExits(instance).Values))
            {
                if (i.CheckState != CheckState.Checked && i.EntrancePair != null)
                {
                    var EntrancePair = instance.EntrancePool.GetEntrancePairOfDestination(i.DestinationExit);
                    if (EntrancePair == null) { continue; }
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
