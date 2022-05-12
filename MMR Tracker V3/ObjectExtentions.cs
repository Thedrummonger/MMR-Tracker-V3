using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.LogicObjects;
using static MMR_Tracker_V3.TrackerObjects.EntranceData;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public static class ObjectExtentions
    {

        public static bool IsLiteralID(this string ID, out string CleanedID)
        {
            bool Literal = false;
            CleanedID = ID;
            if (ID.StartsWith("'") && ID.EndsWith("'"))
            {
                Literal = true;
                CleanedID = ID.Replace("'", "");
            }
            return Literal;
        }
        public static string GetLogicNameFromExit(this LogicObjects.TrackerInstance instance, EntranceRandoExit Exit)
        {
            return instance.InstanceReference.ExitLogicMap[$"{Exit.ParentAreaID} X {Exit.ID}"];
        }
        public static string GetLogicNameFromExit(this LogicObjects.TrackerInstance instance, EntranceAreaPair Exit)
        {
            return instance.InstanceReference.ExitLogicMap[$"{Exit.Area} X {Exit.Exit}"];
        }
        public static string GetLogicNameFromExit(this LogicObjects.TrackerInstance instance, EntranceRandoDestination Exit)
        {
            return instance.InstanceReference.ExitLogicMap[$"{Exit.region} X {Exit.from}"];
        }
        public static void AddLogicExitReference(this LogicObjects.TrackerInstance instance, EntranceAreaPair Exit, string LogicName)
        {
            instance.InstanceReference.ExitLogicMap.Add($"{Exit.Area} X {Exit.Exit}", LogicName);
        }
        public static MMRData.JsonFormatLogicItem GetLogic(this TrackerInstance instance, string OriginalID, bool DoEdits = true)
        {
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
                if (MacroData.Price > -1 && !instance.PriceData.CapacityMap.Values.Contains(ID) && DoEdits)
                {
                    LogicEditing.HandlePriceLogic(instance, MacroData.Price, CopyRequirements, CopyConditionals, out CopyRequirements, out CopyConditionals);
                }
            }
            else if (entryType == LogicEntryType.location)
            {
                var LocationData = instance.GetLocationByID(ID);
                if (LocationData.Price > -1 && !instance.PriceData.CapacityMap.Values.Contains(ID) && DoEdits)
                {
                    LogicEditing.HandlePriceLogic(instance, LocationData.Price, CopyRequirements, CopyConditionals, out CopyRequirements, out CopyConditionals);
                }
            }

            if (DoEdits) { LogicEditing.HandleOptionLogicEdits(instance.UserOptions.Values, ID, CopyRequirements, CopyConditionals, out CopyRequirements, out CopyConditionals); }

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


        public static bool MultipleItemEntry(this LogicObjects.TrackerInstance instance, string Entry, out string Item, out int Amount)
        {
            Item = Entry;
            Amount = 1;
            if (!Entry.Contains(",")) { return false; }
            var data = Entry.Split(',').Select(x => x.Trim()).ToArray();
            Item = data[0];
            if (data.Length < 2) { return false; }
            if (int.TryParse(data[1].Trim(), out Amount))
            {
                return true;
            }
            else if (instance.Variables.ContainsKey(data[1]) && instance.Variables[data[1]].Value is Int64 amount)
            {
                Amount = (int)amount;
                return true;
            }
            else
            {
                Amount = 1;
                return false;
            }
        }

        public static bool CanContainItem(this LocationData.LocationObject loc, ItemData.ItemObject item, TrackerInstance instance, bool EmptyIsWildcard = true)
        {
            var LocTypes = loc?.GetDictEntry(instance)?.ValidItemTypes;
            var itemTypes = item?.GetDictEntry(instance)?.ItemTypes;
            if (LocTypes is null || itemTypes is null) { return EmptyIsWildcard; }
            return LocTypes.Intersect(itemTypes).Any();
        }

        public static List<ItemData.ItemObject> GetValidItemsForLocation(this LogicObjects.TrackerInstance _Instance, LocationData.LocationObject Location, string Filter = "")
        {
            var EnteredItems = new List<ItemData.ItemObject>();
            var Names = new List<string>();
            foreach (var i in _Instance.ItemPool.Values)
            {
                if (string.IsNullOrWhiteSpace(i.GetDictEntry(_Instance).GetName(_Instance))) { continue; }
                i.DisplayName = i.GetDictEntry(_Instance).GetName(_Instance);
                if (!SearchStringParser.FilterSearch(_Instance, i, Filter, i.DisplayName)) { continue; }
                if (i.CanBePlaced(_Instance) && Location.CanContainItem(i, _Instance) && !EnteredItems.Contains(i) && !Names.Contains(i.ToString()))
                {
                    Names.Add(i.ToString());
                    EnteredItems.Add(i);
                }
            }

            return EnteredItems.OrderBy(x => x.GetDictEntry(_Instance).GetName(_Instance)).ToList();
        }


        public static List<EntranceData.EntranceRandoDestination> GetAllLoadingZoneDestinations(this LogicObjects.TrackerInstance _Instance, string Filter = "")
        {
            var Names = new List<string>();
            var EnteredItems = new List<EntranceRandoDestination>();
            foreach (var area in _Instance.EntrancePool.AreaList.Values.Where(x => x.LoadingZoneExits.Any()).ToList().SelectMany(x => x.LoadingZoneExits).OrderBy(x => x.Value.ID))
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
    }
}
