using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.EntranceData;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.OptionData;

namespace MMR_Tracker_V3.TrackerObjectExtentions
{
    public static class TrackerInstanceExtentions
    {
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
            if (!instance.InstanceReference.EntranceLogicNameToEntryData.ContainsKey(item)) { return null; }
            var EntranceData = instance.InstanceReference.EntranceLogicNameToEntryData[item];
            if (!instance.EntrancePool.AreaList.ContainsKey(EntranceData.Area)) { return null; }
            if (!instance.EntrancePool.AreaList[EntranceData.Area].Exits.ContainsKey(EntranceData.Exit)) { return null; }
            return instance.EntrancePool.AreaList[EntranceData.Area].Exits[EntranceData.Exit];
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
            if (literal && instance.InstanceReference.EntranceLogicNameToEntryData.ContainsKey(ID)) { Obj = instance.GetExitByLogicID(ID);  return LogicEntryType.Exit; }
            if (literal && instance.HintPool.ContainsKey(ID)) { Obj = instance.HintPool[ID]; return LogicEntryType.Hint; }
            if (instance.MacroPool.ContainsKey(ID)) { Obj = instance.MacroPool[ID]; return LogicEntryType.macro; }
            if (!literal && instance.LocationPool.ContainsKey(ID)) { Obj = instance.LocationPool[ID]; return LogicEntryType.location; }
            if (!literal && instance.InstanceReference.EntranceLogicNameToEntryData.ContainsKey(ID)) { Obj = instance.GetExitByLogicID(ID); return LogicEntryType.Exit; }
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
            if (LogicEditing.CheckLogicFunction(instance, OriginalID, new List<string>(), out _, false)) { return LogicEntryType.function; }
            return LogicEntryType.error;
        }

        public static List<ItemObject> GetValidItemsForLocation(this InstanceData.TrackerInstance _Instance, LocationData.LocationObject Location, string Filter = "", bool ForOtherPlayer = false)
        {
            var EnteredItems = new List<ItemObject>();
            var Names = new List<string>();
            foreach (var i in _Instance.ItemPool.Values)
            {
                if (string.IsNullOrWhiteSpace(i.GetDictEntry(_Instance).GetName(_Instance))) { continue; }
                i.DisplayName = i.GetDictEntry(_Instance).GetName(_Instance);
                if (!SearchStringParser.FilterSearch(_Instance, i, Filter, i.DisplayName)) { continue; }
                if ((i.CanBePlaced(_Instance) || ForOtherPlayer) && Location.CanContainItem(i, _Instance) && !EnteredItems.Contains(i) && !Names.Contains(i.ToString()))
                {
                    Names.Add(i.ToString());
                    EnteredItems.Add(i);
                }
            }

            return EnteredItems.OrderBy(x => x.GetDictEntry(_Instance).GetName(_Instance)).ToList();
        }


        public static List<EntranceRandoDestination> GetAllLoadingZoneDestinations(this InstanceData.TrackerInstance _Instance, string Filter = "")
        {
            var Names = new List<string>();
            var EnteredItems = new List<EntranceRandoDestination>();
            foreach (var area in _Instance.EntrancePool.AreaList.Values.Where(x => x.RandomizableExits(_Instance).Any()).ToList().SelectMany(x => x.RandomizableExits(_Instance)).OrderBy(x => x.Value.ID))
            {
                var Entry = new EntranceRandoDestination
                {
                    region = area.Value.ID,
                    from = area.Value.ParentAreaID,
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
                    x.GetDictEntry(instance)?.SpoilerData?.SpoilerLogNames != null &&
                    x.GetDictEntry(instance).SpoilerData.SpoilerLogNames.Contains(Item) &&
                    (x.CanBePlaced(instance) || IgnoreMaxAmount) && (x.ValidStartingItem(instance) || !ForStartingPool))).ToList();
            }
            if (CheckItemName)
            {
                ValidItem = ValidItem.Concat(instance.ItemPool.Values.Where(x =>
                    x.GetDictEntry(instance).GetName(instance, DoNameEdits) != null && x.GetDictEntry(instance).GetName(instance, DoNameEdits) == Item &&
                    (x.CanBePlaced(instance) || IgnoreMaxAmount) && (x.ValidStartingItem(instance) || !ForStartingPool))).ToList();
            }
            if (CheckItemID)
            {
                ValidItem = ValidItem.Concat(instance.ItemPool.Values.Where(x =>
                    x.ID== Item && (x.CanBePlaced(instance) || IgnoreMaxAmount) && (x.ValidStartingItem(instance) || !ForStartingPool))).ToList();
            }
            if (!ValidItem.Any()) { return null; }
            return ValidItem[0];
        }

        public static void ToggleAllTricks(this InstanceData.TrackerInstance instance, bool? state)
        {
            if (state is null) { foreach (var i in instance.MacroPool.Values.Where(x => x.isTrick(instance))) { i.TrickEnabled = !i.TrickEnabled; } }
            else { foreach (var i in instance.MacroPool.Values.Where(x => x.isTrick(instance))) { i.TrickEnabled = (bool)state; } }
        }

        public static List<char> GetAllCurrencies(this InstanceData.TrackerInstance instance, bool all = false)
        {
            return instance.PriceData.CapacityMap.Keys.Where(x => x != '*' || all).ToList();
        }
        public static string GetLogicNameFromExit(this InstanceData.TrackerInstance instance, EntranceData.EntranceRandoExit Exit)
        {
            return instance.InstanceReference.ExitLogicMap[$"{Exit.ParentAreaID} X {Exit.ID}"];
        }
        public static string GetLogicNameFromExit(this InstanceData.TrackerInstance instance, EntranceData.EntranceAreaPair Exit)
        {
            return instance.InstanceReference.ExitLogicMap[$"{Exit.Area} X {Exit.Exit}"];
        }
        public static string GetLogicNameFromExit(this InstanceData.TrackerInstance instance, EntranceData.EntranceRandoDestination Exit)
        {
            return instance.InstanceReference.ExitLogicMap[$"{Exit.region} X {Exit.from}"];
        }
        public static void AddLogicExitReference(this InstanceData.TrackerInstance instance, EntranceData.EntranceAreaPair Exit, string LogicName)
        {
            instance.InstanceReference.ExitLogicMap.Add($"{Exit.Area} X {Exit.Exit}", LogicName);
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
                if (p > -1 && !instance.PriceData.GetCapacityMap(c).ContainsValue(ID)&& DoEdits)
                {
                    LogicEditing.HandlePriceLogic(instance, p, c, CopyRequirements, CopyConditionals, out CopyRequirements, out CopyConditionals);
                }
            }
            else if (entryType == LogicEntryType.location)
            {
                var LocationData = instance.GetLocationByID(ID);
                LocationData.GetPrice(out int p, out char c);
                if (p > -1 && !instance.PriceData.GetCapacityMap(c).ContainsValue(ID)&& DoEdits)
                {
                    LogicEditing.HandlePriceLogic(instance, p, c, CopyRequirements, CopyConditionals, out CopyRequirements, out CopyConditionals);
                }
            }

            if (DoEdits) 
            { 
                LogicEditing.HandleOptionLogicEdits(actions, ID, CopyRequirements, CopyConditionals, out CopyRequirements, out CopyConditionals);
                LogicEditing.HandleOptionLogicEdits(actions, ID, CopyRequirements, CopyConditionals, out CopyRequirements, out CopyConditionals);
            }

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

        public static List<string> GetAllInaccessableLogic(this TrackerInstance instance)
        {
            List<string> InaccessableLogic = new List<string>();
            bool InaccessableFound = true;
            while (InaccessableFound)
            {
                InaccessableFound = false;
                foreach (var i in instance.LogicFile.Logic)
                {
                    if (InaccessableLogic.Contains(i.Id)) { continue; }
                    if (instance.IsLogicInaccessable(i.Id, InaccessableLogic))
                    {
                        InaccessableLogic.Add(i.Id);
                        InaccessableFound = true;
                    }
                }
            }
            return InaccessableLogic;
        }

        public static bool IsLogicInaccessable(this TrackerInstance instance, string OriginalID, List<string> KnownInaccessable = null)
        {
            var Logic = instance.GetLogic(OriginalID);
            if (RequirementInaccessable(Logic.RequiredItems)) { return true; }
            if (Logic.ConditionalItems.Any() && Logic.ConditionalItems.All(c => RequirementInaccessable(c))) { return true; }
            return false;

            bool RequirementInaccessable(List<string> Req)
            {
                return Req.Any(x => IsInacceableEntry(x));
            }

            bool IsInacceableEntry(string x)
            {
                if (KnownInaccessable is not null && KnownInaccessable.Contains(x)) { return true; }
                if (bool.TryParse(x, out bool FoundBool) && !FoundBool) { return true; }
                if (LogicEditing.IsLogicFunction(x, out string func, out string Param) && func == "option" && !LogicEditing.CheckOptionFunction(instance, Param.Split(",").Select(x => x.Trim()).ToArray()))
                {
                    return true;
                }
                var SubLogic = instance.GetLogic(x);
                if (SubLogic is not null && SubLogic.IsTrick && instance.MacroPool.ContainsKey(x) && !instance.MacroPool[x].TrickEnabled) { return true; }
                return false;
            }
        }


        public static bool MultipleItemEntry(this InstanceData.TrackerInstance instance, string Entry, out string Item, out int Amount)
        {
            Item = Entry;
            Amount = 1;
            if (!Entry.Contains(",")) { return false; }
            if (LogicEditing.CheckLogicFunction(instance, Entry, new List<string>(), out _, false)) { return false; }
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
                return LO.GetDictEntry(instance)?.GetName(instance)??LO.ID;
            }
            else if (Entry is MacroObject MO)
            {
                return MO.GetDictEntry(instance)?.Name??MO.ID;
            }
            else if (Entry is ItemObject IO)
            {
                return IO.GetDictEntry(instance)?.Name??IO.ID;
            }
            else if (Entry is ChoiceOption CO)
            {
                return CO.Name??CO.ID;
            }
            else if (Entry is ToggleOption TO)
            {
                return TO.Name??TO.ID;
            }
            else if (Entry is IntOption NO)
            {
                return NO.Name??NO.ID;
            }
            else if (Entry is HintData.HintObject HO)
            {
                return HO.GetDictEntry(instance)?.Name??HO.ID;
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
    }
}
