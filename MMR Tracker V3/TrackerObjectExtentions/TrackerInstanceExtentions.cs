using MMR_Tracker_V3.DataStructure;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static MMR_Tracker_V3.DataStructure.MiscData;
using static MMR_Tracker_V3.TrackerObjects.EntranceData;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.OptionData;

namespace MMR_Tracker_V3.TrackerObjectExtentions
{
    public static class TrackerInstanceExtentions
    {
        public static LogicItemData GetLogicItemData(this InstanceData.TrackerInstance Instance, string LogicItem)
        {
            bool MultiItem = Instance.MultipleItemEntry(LogicItem, out string _LogicItem, out int Amount);
            bool Literal = _LogicItem.IsLiteralID(out _LogicItem);
            var type = Instance.GetItemEntryType(_LogicItem, Literal, out object obj);
            return new LogicItemData { CleanID = _LogicItem, Literal = Literal, RawID = LogicItem, Type = type, Amount = Amount, Object = obj, HadItemCount = MultiItem };
        }
        public static ItemObject GetItemByID(this InstanceData.TrackerInstance instance, string item)
        {
            if (item is null) { return null; }
            if (!instance.ItemPool.ContainsKey(item)) { return null; }
            return instance.ItemPool[item];
        }
        public static LocationObject GetLocationByID(this InstanceData.TrackerInstance instance, string item)
        {
            if (item is null) { return null; }
            if (!instance.LocationPool.ContainsKey(item)) { return null; }
            return instance.LocationPool[item];
        }

        public static MacroObject GetMacroByID(this InstanceData.TrackerInstance instance, string item)
        {
            if (item is null) { return null; }
            if (!instance.MacroPool.ContainsKey(item)) { return null; }
            return instance.MacroPool[item];
        }

        public static HintData.HintObject GetHintByID(this InstanceData.TrackerInstance instance, string item)
        {
            if (item is null) { return null; }
            if (!instance.HintPool.ContainsKey(item)) { return null; }
            return instance.HintPool[item];
        }

        public static EntranceRandoExit GetExitByLogicID(this InstanceData.TrackerInstance instance, string item)
        {
            if (item is null) { return null; }
            if (!instance.EntrancePool.ExitLookupByID.ContainsKey(item)) { return null; }
            return instance.EntrancePool.ExitLookupByID[item];
        }

        public static ChoiceOption GetChoiceOptionByID(this InstanceData.TrackerInstance instance, string item)
        {
            if (item is null) { return null; }
            if (!instance.ChoiceOptions.ContainsKey(item)) { return null; }
            return instance.ChoiceOptions[item];
        }
        public static ToggleOption GetToggleOptionByID(this InstanceData.TrackerInstance instance, string item)
        {
            if (item is null) { return null; }
            if (!instance.ToggleOptions.ContainsKey(item)) { return null; }
            return instance.ToggleOptions[item];
        }
        public static IntOption GetIntOptionByID(this InstanceData.TrackerInstance instance, string item)
        {
            if (item is null) { return null; }
            if (!instance.IntOptions.ContainsKey(item)) { return null; }
            return instance.IntOptions[item];
        }
        public static LogicEntryCollection GetLogicEntryCollectionByID(this InstanceData.TrackerInstance instance, string item)
        {
            if (item is null) { return null; }
            if (!instance.LogicEntryCollections.ContainsKey(item)) { return null; }
            return instance.LogicEntryCollections[item];
        }

        public static LogicEntryType GetLocationEntryType(this InstanceData.TrackerInstance instance, string ID, bool literal, out object Obj)
        {
            if (literal && instance.LocationPool.ContainsKey(ID)) { Obj = instance.LocationPool[ID]; return LogicEntryType.location; }
            if (literal && instance.EntrancePool.ExitLookupByID.ContainsKey(ID)) { Obj = instance.GetExitByLogicID(ID); return LogicEntryType.Exit; }
            if (literal && instance.HintPool.ContainsKey(ID)) { Obj = instance.HintPool[ID]; return LogicEntryType.Hint; }
            if (instance.MacroPool.ContainsKey(ID)) { Obj = instance.MacroPool[ID]; return LogicEntryType.macro; }
            if (!literal && instance.LocationPool.ContainsKey(ID)) { Obj = instance.LocationPool[ID]; return LogicEntryType.location; }
            if (!literal && instance.EntrancePool.ExitLookupByID.ContainsKey(ID)) { Obj = instance.GetExitByLogicID(ID); return LogicEntryType.Exit; }
            if (!literal && instance.HintPool.ContainsKey(ID)) { Obj = instance.HintPool[ID]; return LogicEntryType.Hint; }

            if (instance.ChoiceOptions.ContainsKey(ID)) { Obj = instance.ChoiceOptions[ID]; return LogicEntryType.ChoiceOption; }
            if (instance.MultiSelectOptions.ContainsKey(ID)) { Obj = instance.MultiSelectOptions[ID]; return LogicEntryType.MultiSelectOption; }
            if (instance.ToggleOptions.ContainsKey(ID)) { Obj = instance.ToggleOptions[ID]; return LogicEntryType.ToggleOption; }
            if (instance.IntOptions.ContainsKey(ID)) { Obj = instance.IntOptions[ID]; return LogicEntryType.IntOption; }
            if (instance.LogicEntryCollections.ContainsKey(ID)) { Obj = instance.LogicEntryCollections[ID]; return LogicEntryType.LogicEntryCollection; }
            Obj = null;
            return LogicEntryType.error;
        }

        public static LogicEntryType GetItemEntryType(this InstanceData.TrackerInstance instance, string OriginalID, bool literal, out object obj)
        {
            instance.MultipleItemEntry(OriginalID, out string ID, out _);
            if (literal && instance.ItemPool.ContainsKey(ID)) { obj = instance.ItemPool[ID]; return LogicEntryType.item; }
            if (literal && instance.EntrancePool.AreaList.ContainsKey(ID)) { obj = instance.EntrancePool.AreaList[ID]; return LogicEntryType.Area; }
            if (instance.MacroPool.ContainsKey(ID)) { obj = instance.MacroPool[ID]; return LogicEntryType.macro; }
            if (!literal && instance.ItemPool.ContainsKey(ID)) { obj = instance.ItemPool[ID]; return LogicEntryType.item; }
            if (!literal && instance.EntrancePool.AreaList.ContainsKey(ID)) { obj = instance.EntrancePool.AreaList[ID]; return LogicEntryType.Area; }

            if (instance.ChoiceOptions.ContainsKey(ID)) { obj = instance.ChoiceOptions[ID]; return LogicEntryType.ChoiceOption; }
            if (instance.MultiSelectOptions.ContainsKey(ID)) { obj = instance.MultiSelectOptions[ID]; return LogicEntryType.MultiSelectOption; }
            if (instance.ToggleOptions.ContainsKey(ID)) { obj = instance.ToggleOptions[ID]; return LogicEntryType.ToggleOption; }
            if (instance.IntOptions.ContainsKey(ID)) { obj = instance.IntOptions[ID]; return LogicEntryType.IntOption; }
            if (instance.LogicEntryCollections.ContainsKey(ID)) { obj = instance.LogicEntryCollections[ID]; return LogicEntryType.LogicEntryCollection; }

            if (bool.TryParse(ID, out bool result)) { obj = result; return LogicEntryType.Bool; }
            obj = null;
            if (LogicFunctions.IsLogicFunction(OriginalID)) { return LogicEntryType.function; }
            return LogicEntryType.error;
        }

        public static List<ItemObject> GetValidItemsForLocation(this InstanceData.TrackerInstance _Instance, LocationData.LocationObject Location, string Filter = "", bool ForOtherPlayer = false)
        {
            var EnteredItems = new List<ItemObject>();
            var Names = new List<string>();
            foreach (var i in _Instance.ItemPool.Values)
            {
                if (string.IsNullOrWhiteSpace(i.GetDictEntry().GetName())) { continue; }
                i.DisplayName = i.GetDictEntry().GetName();
                if (!SearchStringParser.FilterSearch(_Instance, i, Filter, i.DisplayName)) { continue; }
                if ((i.CanBePlaced() || ForOtherPlayer) && Location.CanContainItem(i) && !EnteredItems.Contains(i) && !Names.Contains(i.ToString()))
                {
                    Names.Add(i.ToString());
                    EnteredItems.Add(i);
                }
            }

            return EnteredItems.OrderBy(x => x.GetDictEntry().GetName()).ToList();
        }


        public static List<EntranceRandoDestination> GetAllLoadingZoneDestinations(this InstanceData.TrackerInstance _Instance, string Filter = "")
        {
            var Names = new List<string>();
            var EnteredItems = new List<EntranceRandoDestination>();
            foreach (var area in _Instance.EntrancePool.AreaList.Values.Where(x => x.RandomizableExits().Any()).ToList().SelectMany(x => x.RandomizableExits()).OrderBy(x => x.Value.ExitID))
            {
                var Entry = new EntranceRandoDestination
                {
                    region = area.Value.ExitID,
                    from = area.Value.GetParentArea().ID,
                };
                if (!SearchStringParser.FilterSearch(_Instance, Entry, Filter, Entry.ToString())) { continue; }
                EnteredItems.Add(Entry);
            }

            return EnteredItems;
        }

        public static ItemObject GetItemToPlace(this InstanceData.TrackerInstance instance, string Item, bool CheckSpoilerName = true, bool CheckItemName = false, bool CheckItemID = false, bool ForStartingPool = false, bool IgnoreMaxAmount = false, bool DoNameEdits = false)
        {
            List<ItemObject> ValidItem = new List<ItemObject>();
            if (CheckSpoilerName)
            {
                ValidItem = ValidItem.Concat(instance.ItemPool.Values.Where(x =>
                    x.GetDictEntry()?.SpoilerData?.SpoilerLogNames != null &&
                    x.GetDictEntry().SpoilerData.SpoilerLogNames.Contains(Item) &&
                    (x.CanBePlaced() || IgnoreMaxAmount) && (x.ValidStartingItem() || !ForStartingPool))).ToList();
            }
            if (CheckItemName)
            {
                ValidItem = ValidItem.Concat(instance.ItemPool.Values.Where(x =>
                    x.GetDictEntry().GetName(DoNameEdits) != null && x.GetDictEntry().GetName(DoNameEdits) == Item &&
                    (x.CanBePlaced() || IgnoreMaxAmount) && (x.ValidStartingItem() || !ForStartingPool))).ToList();
            }
            if (CheckItemID)
            {
                ValidItem = ValidItem.Concat(instance.ItemPool.Values.Where(x =>
                    x.ID == Item && (x.CanBePlaced() || IgnoreMaxAmount) && (x.ValidStartingItem() || !ForStartingPool))).ToList();
            }
            if (!ValidItem.Any()) { return null; }
            return ValidItem[0];
        }

        public static void ToggleAllTricks(this InstanceData.TrackerInstance instance, bool? state)
        {
            if (state is null) { foreach (var i in instance.MacroPool.Values.Where(x => x.isTrick())) { i.TrickEnabled = !i.TrickEnabled; } }
            else { foreach (var i in instance.MacroPool.Values.Where(x => x.isTrick())) { i.TrickEnabled = (bool)state; } }
        }

        public static List<char> GetAllCurrencies(this InstanceData.TrackerInstance instance, bool all = false)
        {
            return instance.PriceData.CapacityMap.Keys.Where(x => x != '*' || all).ToList();
        }
        public static MMRData.JsonFormatLogicItem GetLogic(this TrackerInstance instance, string OriginalID, bool DoEdits = true, List<OptionData.Action> actions = null)
        {
            actions ??= instance.GetOptionActions();
            bool Literal = OriginalID.IsLiteralID(out string ID);
            LogicEntryType entryType = instance.GetLocationEntryType(ID, Literal, out dynamic Obj);
            if (entryType == LogicEntryType.error || Obj is null || !Utility.DynamicPropertyExist(Obj, "referenceData")) { return null; }
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
                MacroData.GetPrice(out int p, out char c);
                if (p > -1 && !instance.PriceData.GetCapacityMap(c).ContainsValue(ID) && DoEdits)
                {
                    LogicEditing.HandlePriceLogic(instance, p, c, CopyRequirements, CopyConditionals, out CopyRequirements, out CopyConditionals);
                }
            }
            else if (entryType == LogicEntryType.location)
            {
                var LocationData = instance.GetLocationByID(ID);
                LocationData.GetPrice(out int p, out char c);
                if (p > -1 && !instance.PriceData.GetCapacityMap(c).ContainsValue(ID) && DoEdits)
                {
                    LogicEditing.HandlePriceLogic(instance, p, c, CopyRequirements, CopyConditionals, out CopyRequirements, out CopyConditionals);
                }
            }

            if (DoEdits)
            {
                LogicEditing.HandleOptionLogicEdits(actions, ID, CopyRequirements, CopyConditionals, out CopyRequirements, out CopyConditionals);
            }

            return new MMRData.JsonFormatLogicItem
            {
                Id = ID,
                IsTrick = LogicFileEntry is not null && LogicFileEntry.IsTrick,
                RequiredItems = CopyRequirements,
                ConditionalItems = CopyConditionals,
                TimeAvailable = LogicFileEntry is null ? TimeOfDay.None : LogicFileEntry.TimeAvailable,
                TimeNeeded = LogicFileEntry is null ? TimeOfDay.None : LogicFileEntry.TimeNeeded,
                TimeSetup = LogicFileEntry is null ? TimeOfDay.None : LogicFileEntry.TimeSetup,
                TrickCategory = LogicFileEntry?.TrickCategory,
                TrickTooltip = LogicFileEntry?.TrickTooltip
            };
        }

        public static bool ISLogicItemNeverObtainable(this TrackerInstance instance, string LogicItem)
        {
            if (LogicFunctions.IsLogicFunction(LogicItem, out string Func, out string Param))
            {
                //Debug.WriteLine($"Item Was Function {Func}");
                if (Func.In("contains", "trick", "setting", "option", "rand", "randomized"))
                {
                    //Debug.WriteLine($"Parsing {Func} Function");
                    bool FunctionEntryValid = LogicFunctions.LogicFunctionAquired(instance, LogicItem, new List<string>());
                    //Debug.WriteLine($"Valid? {FunctionEntryValid}");
                    return !FunctionEntryValid;
                }
                else { return false; }
            }
            var LogicItemData = instance.GetLogicItemData(LogicItem);
            if (LogicItemData.Type == LogicEntryType.macro && ((MacroObject)LogicItemData.Object).isTrick() && !((MacroObject)LogicItemData.Object).TrickEnabled) { return true; }
            if (LogicItemData.Type == LogicEntryType.Bool && !bool.Parse(LogicItemData.CleanID)) { return true; }
            return false;
        }


        public static bool MultipleItemEntry(this InstanceData.TrackerInstance instance, string Entry, out string Item, out int Amount)
        {
            Item = Entry;
            Amount = 1;
            if (!Entry.Contains(",")) { return false; }
            if (LogicFunctions.IsLogicFunction(Entry)) { return false; }
            var data = Entry.Split(',').Select(x => x.Trim()).ToArray();
            Item = data[0];
            if (data.Length != 2) { return false; }
            if (int.TryParse(data[1].Trim(), out Amount))
            {
                return true;
            }
            else if (instance.IntOptions.ContainsKey(data[1]))
            {
                Amount = instance.IntOptions[data[1]].Value;
                return true;
            }
            else
            {
                Item = Entry;
                Amount = 1;
                return false;
            }
        }
        public static string GetDynamicObjName(this TrackerInstance instance, object Entry)
        {
            if (Entry is LocationObject LO)
            {
                return LO.GetDictEntry()?.GetName() ?? LO.ID;
            }
            else if (Entry is MacroObject MO)
            {
                return MO.GetDictEntry()?.Name ?? MO.ID;
            }
            else if (Entry is ItemObject IO)
            {
                return IO.GetDictEntry()?.Name ?? IO.ID;
            }
            else if (Entry is ChoiceOption CO)
            {
                return CO.Name ?? CO.ID;
            }
            else if (Entry is ToggleOption TO)
            {
                return TO.Name ?? TO.ID;
            }
            else if (Entry is IntOption NO)
            {
                return NO.Name ?? NO.ID;
            }
            else if (Entry is HintData.HintObject HO)
            {
                return HO.GetDictEntry()?.Name ?? HO.ID;
            }
            else { return null; }
        }


        public static List<OptionData.Action> GetOptionActions(this InstanceData.TrackerInstance Instance)
        {
            return Instance.ChoiceOptions.Values.Select(x => x.GetValue().Actions)
                .Concat(Instance.ToggleOptions.Values.Select(x => x.GetValue().Actions))
                .Concat(Instance.MultiSelectOptions.Values.SelectMany(x => x.GetEnabledValues()).Select(x => x.Actions))
                .ToList();
        }

        public static bool CombineEntrancesWithLocations(this InstanceData.TrackerInstance Instance)
        {
            if (Instance == null) { return true; }
            return !Instance.StaticOptions.OptionFile.EntranceRandoFeatures || !Instance.EntrancePool.IsEntranceRando || !Instance.EntrancePool.CheckForRandomEntrances();
        }
    }
}
