using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.EntranceData;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.OptionData;
using System.ComponentModel;

namespace MMR_Tracker_V3.TrackerObjectExtensions
{
    public static class TrackerInstanceExtensions
    {
        public static LogicItemData GetLogicItemData(this InstanceData.TrackerInstance Instance, string LogicItem)
        {
            if (LogicItem == null) { return new LogicItemData(); }
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

        public static EntranceRandoArea GetAreaByLogicID(this InstanceData.TrackerInstance instance, string item)
        {
            if (item is null) { return null; }
            if (!instance.AreaPool.ContainsKey(item)) { return null; }
            return instance.AreaPool[item];
        }

        public static EntranceRandoExit GetExitByLogicID(this InstanceData.TrackerInstance instance, string item)
        {
            if (item is null) { return null; }
            if (!instance.ExitPool.ContainsKey(item)) { return null; }
            return instance.ExitPool[item];
        }
        public static EntranceRandoExit GetExitByAreaIDAndExitID(this InstanceData.TrackerInstance instance, string AreaID, string ExitID)
        {
            var area = instance.GetAreaByLogicID(AreaID);
            if (area is null) { return null; }
            return area.GetExitFromExitID(ExitID);
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

        public static bool GetCheckableLocationByID(this TrackerInstance instance, string ID, bool literal, out CheckableLocation Result)
        {
            Result = GetCheckableLocationByID(instance, ID, literal);
            return Result != null;
        }
        public static CheckableLocation GetCheckableLocationByID(this TrackerInstance instance, string ID, bool literal)
        {
            if (literal && instance.LocationPool.ContainsKey(ID)) { return instance.LocationPool[ID]; }
            if (literal && instance.ExitPool.ContainsKey(ID)) { return instance.ExitPool[ID]; }
            if (literal && instance.HintPool.ContainsKey(ID)) { return instance.HintPool[ID]; }
            if (instance.MacroPool.ContainsKey(ID)) { return instance.MacroPool[ID]; }
            if (!literal && instance.LocationPool.ContainsKey(ID)) { return instance.LocationPool[ID]; }
            if (!literal && instance.ExitPool.ContainsKey(ID)) { return instance.ExitPool[ID]; }
            if (!literal && instance.HintPool.ContainsKey(ID)) { return instance.HintPool[ID]; }
            return null;
        }
        public static bool GetLogicOptionByID(this TrackerInstance instance, string ID, out LogicOption Result)
        {
            Result = GetLogicOptionByID(instance, ID);
            return Result != null;
        }
        public static LogicOption GetLogicOptionByID(this TrackerInstance instance, string ID)
        {
            if (instance.ChoiceOptions.ContainsKey(ID)) { return instance.ChoiceOptions[ID]; }
            if (instance.MultiSelectOptions.ContainsKey(ID)) { return instance.MultiSelectOptions[ID]; }
            if (instance.ToggleOptions.ContainsKey(ID)) { return instance.ToggleOptions[ID]; }
            if (instance.IntOptions.ContainsKey(ID)) { return instance.IntOptions[ID]; }
            return null;
        }

        private static LogicItemTypes GetItemEntryType(this InstanceData.TrackerInstance instance, string ID, bool literal, out object obj)
        {
            if (literal && instance.ItemPool.ContainsKey(ID)) { obj = instance.ItemPool[ID]; return LogicItemTypes.item; }
            if (literal && instance.AreaPool.ContainsKey(ID)) { obj = instance.AreaPool[ID]; return LogicItemTypes.Area; }
            if (instance.MacroPool.ContainsKey(ID)) { obj = instance.MacroPool[ID]; return LogicItemTypes.macro; }
            if (!literal && instance.ItemPool.ContainsKey(ID)) { obj = instance.ItemPool[ID]; return LogicItemTypes.item; }
            if (!literal && instance.AreaPool.ContainsKey(ID)) { obj = instance.AreaPool[ID]; return LogicItemTypes.Area; }
            
            if (instance.LogicEntryCollections.ContainsKey(ID)) { obj = instance.LogicEntryCollections[ID]; return LogicItemTypes.LogicEntryCollection; }

            if (bool.TryParse(ID, out bool result)) { obj = result; return LogicItemTypes.Boolean; }
            obj = null;
            if (LogicFunctions.IsLogicFunction(ID)) { return LogicItemTypes.function; }
            return LogicItemTypes.error;
        }

        public static List<ItemObject> GetValidItemsForLocation(this InstanceData.TrackerInstance _Instance, LocationData.LocationObject Location, string Filter = "", bool IgnorePlaceablility = false)
        {
            var EnteredItems = new List<ItemObject>();
            var Names = new List<string>();
            foreach (var i in _Instance.ItemPool.Values)
            {
                if (string.IsNullOrWhiteSpace(i.GetDictEntry().GetName())) { continue; }
                i.DisplayName = i.GetDictEntry().GetName();
                if (!SearchStringParser.FilterSearch(_Instance, i, Filter, i.DisplayName)) { continue; }
                if ((i.CanBePlaced() || IgnorePlaceablility) && (Location is null || Location.CanContainItem(i)) && !EnteredItems.Contains(i) && !Names.Contains(i.ToString()))
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
            foreach (var area in _Instance.GetAllRandomizableExits().OrderBy(x => x.ExitID))
            {
                var Entry = new EntranceRandoDestination
                {
                    region = area.ExitID,
                    from = area.ParentAreaID,
                };
                if (!SearchStringParser.FilterSearch(_Instance, Entry, Filter, Entry.ToString())) { continue; }
                EnteredItems.Add(Entry);
            }

            return EnteredItems;
        }

        public static List<EntranceRandoExit> GetAllRandomizableExits(this InstanceData.TrackerInstance _Instance)
        {
            return _Instance.ExitPool.Where(x => x.Value.IsRandomizableEntrance()).Select(x => x.Value).ToList();
        }
        public static List<EntranceRandoExit> GetMacroExits(this InstanceData.TrackerInstance _Instance)
        {
            return _Instance.ExitPool.Where(x => !x.Value.IsRandomizableEntrance()).Select(x => x.Value).ToList();
        }
        public static List<EntranceRandoExit> GetAllUnrandomizedExits(this InstanceData.TrackerInstance _Instance, UnrandState state = UnrandState.Any)
        {
            var UnrandomizedExits = _Instance.GetAllRandomizableExits().Where(x => x.IsUnrandomized(state));
            return [.. UnrandomizedExits];
        }
        public static List<EntranceRandoExit> GetAllUnrandomizedAndMacroExits(this InstanceData.TrackerInstance _Instance, UnrandState state = UnrandState.Any)
        {
            return [.. _Instance.GetAllUnrandomizedExits(state), .. _Instance.GetMacroExits()];
        }

        public static ItemObject GetItemToPlace(this InstanceData.TrackerInstance instance, string Item, bool CheckSpoilerName = true, bool CheckItemName = false, bool CheckItemID = false, bool ForStartingPool = false, bool IgnoreMaxAmount = false, bool DoNameEdits = false, bool GetRandom = false)
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
            if (GetRandom) { return ValidItem.PickRandom(); }
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
            CheckableLocation location = instance.GetCheckableLocationByID(ID, Literal);
            if (location is null) { return null; }
            LogicFileType LogicFile = location.referenceData.LogicList;
            int LogicFileIndex = location.referenceData.LogicIndex;
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

            location.GetPrice(out int p, out char c);
            if (p > -1 && !instance.PriceData.GetCapacityMap(c).ContainsValue(ID) && DoEdits)
            {
                LogicEditing.HandlePriceLogic(instance, p, c, CopyRequirements, CopyConditionals, out CopyRequirements, out CopyConditionals);
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
            if (LogicItemData.Type == LogicItemTypes.macro && ((MacroObject)LogicItemData.Object).isTrick() && !((MacroObject)LogicItemData.Object).TrickEnabled) { return true; }
            if (LogicItemData.Type == LogicItemTypes.Boolean && !bool.Parse(LogicItemData.CleanID)) { return true; }
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
            return !Instance.StaticOptions.OptionFile.EntranceRandoFeatures || !Instance.GetAllRandomizableExits().Any(x => x.IsRandomized());
        }
    }
}
