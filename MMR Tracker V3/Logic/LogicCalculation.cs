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

namespace MMR_Tracker_V3.Logic
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
            foreach (var Req in Requirements)
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
            foreach (var Set in Conditionals)
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
            if (LogicFunctions.CheckLogicFunction(container.Instance, i, SubUnlockData, out bool FunctionEntryValid))
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
                    return container.Instance.GetItemByID(LogicItem).Useable(Amount);
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
                        { UsableItems.AddRange(Enumerable.Repeat(LogicItem, (ItemObj as ItemObject).GetTotalUsable())); }
                        else if (LogicEntryAquired(i, SubUnlockData)) { UsableItems.Add(i); }
                    }
                }
            }
        }

        private bool AreaReached(string Area, List<string> TempUnlockData = null)
        {
            bool IsRoot = Area is null || Area == container.Instance.EntrancePool.RootArea;
            string TargetArea = Area ?? container.Instance.EntrancePool.RootArea;
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
                    Name = i.Value.GetDictEntry().GetOptionEditDefinedName(Actions),
                    MaxAmount = i.Value.GetDictEntry().GetOptionEditDefinedMaxAmountInWorld(Actions)
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
                foreach (var i in Area.Value.RandomizableExits().Where(x => !x.Value.IsUnrandomized(UnrandState.Unrand)))
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
                foreach (var i in Area.Value.Exits.Where(x => x.Value.IsUnrandomized(UnrandState.Unrand) || !x.Value.IsRandomizableEntrance()))
                {
                    i.Value.ToggleExitChecked(CheckState.Unchecked);
                }
            }
            foreach (var i in container.Instance.LocationPool.Where(x => x.Value.IsUnrandomized(UnrandState.Unrand)))
            {
                i.Value.ToggleChecked(CheckState.Unchecked);
            }
        }

        private bool CheckUrandomizedExits(int itterations)
        {
            bool ItemStateChanged = false;
            foreach (var Area in container.Instance.EntrancePool.AreaList)
            {
                foreach (var i in Area.Value.Exits.Where(x => x.Value.IsUnrandomized(UnrandState.Unrand) || !x.Value.IsRandomizableEntrance()))
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
                        i.Value.ToggleExitChecked(checkState);
                        if (Available) { AutoObtainedObjects[i.Value] = itterations; }
                    }
                }
            }
            return ItemStateChanged;
        }

        private bool CalculateMacros(int itterations)
        {
            bool MacroStateChanged = false;
            foreach (var i in container.Instance.MacroPool)
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
            foreach (var i in container.Instance.LocationPool.Where(x => x.Value.IsUnrandomized(UnrandState.Unrand)))
            {
                if (string.IsNullOrWhiteSpace(i.Value.GetDictEntry().OriginalItem)) { continue; }
                var Logic = LogicMap[i.Key];
                var Available = CalculatReqAndCond(Logic, i.Key, null);
                bool ShouldBeChecked = Available && i.Value.CheckState != CheckState.Checked;
                bool ShouldBeUnChecked = !Available && i.Value.CheckState == CheckState.Checked;
                if (ShouldBeChecked || ShouldBeUnChecked)
                {
                    ItemStateChanged = true;
                    i.Value.Available = Available;
                    CheckState checkState = i.Value.Available ? CheckState.Checked : CheckState.Unchecked;
                    if (checkState == CheckState.Checked) { i.Value.Randomizeditem.Item = i.Value.GetDictEntry().OriginalItem; }
                    i.Value.ToggleChecked(checkState);
                    if (Available) { AutoObtainedObjects[i.Value] = itterations; }
                }
            }
            return ItemStateChanged;
        }

    }
}
