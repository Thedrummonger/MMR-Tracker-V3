using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TDMUtils;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.Logic
{
    public class LogicCalculation(InstanceData.InstanceContainer _container)
    {
        public readonly InstanceData.InstanceContainer container = _container;
        public Dictionary<string, MMRData.JsonFormatLogicItem> LogicMap = [];
        public HashSet<object> AutoObtainedObjects = [];
        public bool ReCompileLogicOnCalculation = false;

        public static void CommitUnlockData(Dictionary<string, LogicItemData> Main, Dictionary<string, LogicItemData> Sub)
        {
            if (Main is null) { return; }
            foreach(var i in Sub)
            {
                if (!Main.TryGetValue(i.Key, out LogicItemData value) || value.Amount < i.Value.Amount) 
                {
                    Main[i.Key] = i.Value; 
                }
            }
        }

        public bool CalculatReqAndCond(MMRData.JsonFormatLogicItem Logic, string ID, string Area, bool UpdateUnlockData = false)
        {
            Dictionary<string, LogicItemData> SubUnlockData = [];
            bool Available =
                (Area == null || AreaReached(Area)) &&
                RequirementsMet(Logic.RequiredItems, SubUnlockData) &&
                ConditionalsMet(Logic.ConditionalItems, SubUnlockData);
            if (Available && UpdateUnlockData && !container.Instance.UnlockData.ContainsKey(ID)) { container.Instance.UnlockData[ID] = SubUnlockData; }
            return Available;
        }
        private bool RequirementsMet(List<string> Requirements, Dictionary<string, LogicItemData> UnlockData = null)
        {
            Dictionary<string, LogicItemData> TempUnlockData = [];
            foreach (var Req in Requirements)
            {
                Dictionary<string, LogicItemData> SubUnlockData = [];
                if (!LogicEntryAquired(Req, SubUnlockData)) { return false; }
                CommitUnlockData(TempUnlockData, SubUnlockData);
            }
            CommitUnlockData(UnlockData, TempUnlockData);
            return true;
        }

        public bool ConditionalsMet(List<List<string>> Conditionals, Dictionary<string, LogicItemData> UnlockData = null)
        {
            if (Conditionals.Count == 0) { return true; }
            foreach (var Set in Conditionals)
            {
                Dictionary<string, LogicItemData> SubUnlockData = [];
                if (RequirementsMet(Set, SubUnlockData))
                {
                    CommitUnlockData(UnlockData, SubUnlockData);
                    return true;
                }
            }
            return false;
        }

        public bool LogicEntryAquired(string i, Dictionary<string, LogicItemData> UnlockData = null)
        {
            var LogicItem = container.Instance.GetLogicItemData(i);

            switch (LogicItem.Type)
            {
                case LogicItemTypes.Boolean:
                    return bool.Parse(LogicItem.RawID);
                case LogicItemTypes.item:
                    UnlockData?.Add(LogicItem.CleanID, LogicItem);
                    return ((ItemData.ItemObject)LogicItem.Object).Useable(LogicItem.Amount);
                case LogicItemTypes.macro:
                    UnlockData?.Add(LogicItem.CleanID, LogicItem);
                    return ((MacroObject)LogicItem.Object).Available;
                case LogicItemTypes.Area:
                    return AreaReached(LogicItem.CleanID, UnlockData);
                case LogicItemTypes.LogicEntryCollection:
                    return CheckItemArray(LogicItem.CleanID, LogicItem.Amount, UnlockData);
                case LogicItemTypes.function:
                    UnlockData?.Add(LogicItem.CleanID, LogicItem);
                    return LogicFunctions.LogicFunctionAquired(container.Instance, LogicItem.RawID);
                default:
                    Debug.WriteLine($"{LogicItem.RawID} Was not a valid Logic Entry");
                    return false;
            }
        }

        public bool CheckItemArray(string ArrVar, int amount, Dictionary<string, LogicItemData> UnlockData = null)
        {
            var EditActions = container.Instance.GetOptionActions();
            if (!container.Instance.LogicEntryCollections.TryGetValue(ArrVar, out OptionData.LogicEntryCollection Collection)) { return false; }
            List<string> VariableEntries = Collection.GetValue(container.Instance);
            VariableEntries = ExpandSubCollections(VariableEntries);

            List<string> ExpandedList = [];
            foreach (string VariableEntry in VariableEntries)
            {
                var LogicItem = container.Instance.GetLogicItemData(VariableEntry);
                if (LogicItem.Object is ObtainableObject Item)
                {
                    if (LogicItem.HadItemCount && Item.GetTotalUsable() >= LogicItem.Amount) { ExpandedList.Add(VariableEntry); }
                    else { ExpandedList.AddRange(Enumerable.Repeat(LogicItem.CleanID, Item.GetTotalUsable()).ToList()); }
                }
                else if (LogicItem.Object is MacroObject MO && MO.Available)
                {
                    ExpandedList.Add(VariableEntry);
                }
            }
            if (ExpandedList.Count < amount) { return false; }
            IEnumerable<string> UsedItems = ExpandedList.Take(amount);
            Dictionary<string, LogicItemData> ItemCounts = [];
            Dictionary<string, LogicItemData> HasAmountEntries = [];
            foreach (var i in UsedItems)
            {
                var LogicItem = container.Instance.GetLogicItemData(i);
                if (LogicItem.HadItemCount) 
                {
                    if (!HasAmountEntries.ContainsKey(LogicItem.CleanID)) { HasAmountEntries[LogicItem.CleanID] = LogicItem; }
                    else if (HasAmountEntries[LogicItem.CleanID].Amount < LogicItem.Amount) { HasAmountEntries[LogicItem.CleanID] = LogicItem; }
                }
                else
                {
                    if (ItemCounts.ContainsKey(LogicItem.CleanID)) { ItemCounts[LogicItem.CleanID].Amount++; }
                    else { ItemCounts.SetIfEmpty(LogicItem.CleanID, LogicItem); }
                }
            }
            foreach(var i in HasAmountEntries)
            {
                if (!ItemCounts.ContainsKey(i.Key)) { ItemCounts[i.Key] = i.Value; }
                else if (ItemCounts[i.Key].Amount < i.Value.Amount) { ItemCounts[i.Key] = i.Value; }
            }
            CommitUnlockData(UnlockData, ItemCounts);
            return true;

        }

        private List<string> ExpandSubCollections(List<string> list)
        {
            List<string> FinalCollection = new List<string>();
            foreach(var i in list)
            {
                var LogicItem = container.Instance.GetLogicItemData(i);
                if (LogicItem.Type == LogicItemTypes.LogicEntryCollection) 
                {
                    FinalCollection.AddRange(ExpandSubCollections((LogicItem.Object as OptionData.LogicEntryCollection).GetValue(container.Instance))); 
                }
                else
                {
                    FinalCollection.Add(i);
                }
            }
            return FinalCollection;
        }

        private bool AreaReached(string Area, Dictionary<string, LogicItemData> UnlockData = null)
        {
            Dictionary<string, LogicItemData> TempUnlockData = [];
            if (!container.Instance.AreaPool.ContainsKey(Area)) { return false; }
            var AreaObj = container.Instance.AreaPool[Area];
            TempUnlockData.Add(AreaObj.ID, new LogicItemData { Type = LogicItemTypes.Area, Amount = 1, CleanID = Area, RawID = Area, HadItemCount = false, Literal = false, Object = AreaObj });
            bool Reachable = AreaObj.AmountAquiredLocally > 0 || AreaObj.IsRoot;
            if (!Reachable) { return false; }
            CommitUnlockData(UnlockData, TempUnlockData);
            return Reachable;
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
            foreach (var i in container.Instance.ExitPool)
            {
                LogicMap[i.Key] = container.Instance.GetLogic(i.Key, actions: Actions);
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
            CleanUnlockData();
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
                bool UnrandomizedExitChecked = CheckAutoCheckedExits(Itterations);
                bool MacroChanged = CalculateMacros(Itterations);
                Itterations++;
                if (!MacroChanged && !UnrandomizedItemChecked && !UnrandomizedExitChecked) { break; }
            }
            foreach (var i in container.Instance.LocationPool.Values.Where(x => !x.IsUnrandomized(UnrandState.Unrand)))
            {
                var Logic = LogicMap[i.ID];
                i.Available = CalculatReqAndCond(Logic, i.ID, null, true);
            }
            foreach (var i in container.Instance.ExitPool.Values.Where(x => x.IsRandomizableEntrance() && !x.IsUnrandomized(UnrandState.Unrand)))
            {
                var Logic = LogicMap[i.ID];
                i.Available = CalculatReqAndCond(Logic, i.ID, i.ParentAreaID, true);
            }
            foreach (var i in container.Instance.HintPool)
            {
                var Logic = LogicMap[i.Key];
                i.Value.Available = CalculatReqAndCond(Logic, i.Key, null, true);
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
                if (!i.Available && container.Instance.UnlockData.ContainsKey(i.ID)) { container.Instance.UnlockData.Remove(i.ID); }
            }
            foreach (var i in container.Instance.ExitPool.Values)
            {
                if (!i.Available && container.Instance.UnlockData.ContainsKey(i.ID)) { container.Instance.UnlockData.Remove(i.ID); }
            }
            foreach (var i in container.Instance.HintPool.Values)
            {
                if (!i.Available && container.Instance.UnlockData.ContainsKey(i.ID)) { container.Instance.UnlockData.Remove(i.ID); }
            }
            foreach (var i in container.Instance.MacroPool.Values)
            {
                if (!i.Available && container.Instance.UnlockData.ContainsKey(i.ID)) { container.Instance.UnlockData.Remove(i.ID); }
            }
        }

        public void ResetAutoObtainedItems()
        {
            foreach (var i in container.Instance.GetAllUnrandomizedAndMacroExits(UnrandState.Unrand))
            {
                i.ToggleExitChecked(CheckState.Unchecked);
            }
            foreach (var i in container.Instance.LocationPool.Where(x => x.Value.IsUnrandomized(UnrandState.Unrand)))
            {
                i.Value.ToggleChecked(CheckState.Unchecked);
            }
        }

        private bool CheckAutoCheckedExits(int itterations)
        {
            bool ItemStateChanged = false;

            foreach (var i in container.Instance.GetAllUnrandomizedAndMacroExits(UnrandState.Unrand))
            {
                var Logic = LogicMap[i.ID];
                var Available = CalculatReqAndCond(Logic, i.ID, i.ParentAreaID, true);
                bool ShouldBeChecked = Available && i.CheckState != CheckState.Checked;
                bool ShouldBeUnChecked = !Available && i.CheckState == CheckState.Checked;
                if (ShouldBeChecked || ShouldBeUnChecked)
                {
                    ItemStateChanged = true;
                    i.Available = Available;
                    CheckState checkState = i.Available ? CheckState.Checked : CheckState.Unchecked;
                    if (checkState == CheckState.Checked) { i.DestinationExit = i.GetVanillaDestination(); }
                    i.ToggleExitChecked(checkState);
                    if (Available) { AutoObtainedObjects.Add(i); }
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
                    Available = CalculatReqAndCond(Logic, i.Key, null, true);
                    if (Available) { AutoObtainedObjects.Add(i.Value); }
                }

                if (Available != i.Value.Available)
                {
                    MacroStateChanged = true;
                    i.Value.Available = Available;
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
                var Available = CalculatReqAndCond(Logic, i.Key, null, true);
                bool ShouldBeChecked = Available && i.Value.CheckState != CheckState.Checked;
                bool ShouldBeUnChecked = !Available && i.Value.CheckState == CheckState.Checked;
                if (ShouldBeChecked || ShouldBeUnChecked)
                {
                    ItemStateChanged = true;
                    i.Value.Available = Available;
                    CheckState checkState = i.Value.Available ? CheckState.Checked : CheckState.Unchecked;
                    if (checkState == CheckState.Checked) { i.Value.Randomizeditem.Item = i.Value.GetDictEntry().OriginalItem; }
                    i.Value.ToggleChecked(checkState);
                    if (Available) { AutoObtainedObjects.Add(i.Value); }
                }
            }
            return ItemStateChanged;
        }

    }
}
