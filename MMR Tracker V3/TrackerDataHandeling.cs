﻿using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using TDMUtils;
using static MMR_Tracker_V3.TrackerObjects.EntranceData;
using static MMR_Tracker_V3.TrackerObjects.HintData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public static class TrackerDataHandling
    {
        public class DataSets
        {
            public List<LocationData.LocationObject> LocationStateIsUnchecked { get; set; } = new List<LocationData.LocationObject>();
            public List<LocationData.LocationObject> LocationISMarkedOrISAvailableAndUnchecked { get; set; } = new List<LocationData.LocationObject>();
            public List<LocationData.LocationObject> LocationStateIsNOTChecked { get; set; } = new List<LocationData.LocationObject>();
            public List<LocationData.LocationObject> LocationStateIsMarked { get; set; } = new List<LocationData.LocationObject>();
            public List<LocationData.LocationObject> LocationStateIsChecked { get; set; } = new List<LocationData.LocationObject>();

            public List<LocationData.LocationProxy> ProxyISMarkedOrISAvailableAndUnchecked { get; set; } = new List<LocationData.LocationProxy>();
            public List<LocationData.LocationProxy> ProxyStateIsNOTChecked { get; set; } = new List<LocationData.LocationProxy>();
            public List<LocationData.LocationProxy> ProxyStateIsMarked { get; set; } = new List<LocationData.LocationProxy>();
            public List<LocationData.LocationProxy> ProxyStateIsChecked { get; set; } = new List<LocationData.LocationProxy>();

            public List<EntranceData.EntranceRandoExit> ExitStateIsUnchecked { get; set; } = new List<EntranceData.EntranceRandoExit>();
            public List<EntranceData.EntranceRandoExit> ExitISMarkedOrISAvailableAndUnchecked { get; set; } = new List<EntranceData.EntranceRandoExit>();
            public List<EntranceData.EntranceRandoExit> ExitStateIsNOTChecked { get; set; } = new List<EntranceData.EntranceRandoExit>();
            public List<EntranceData.EntranceRandoExit> ExitStateIsChecked { get; set; } = new List<EntranceData.EntranceRandoExit>();
            public List<EntranceData.EntranceRandoExit> ExitStateIsMarked { get; set; } = new List<EntranceData.EntranceRandoExit>();

            public List<HintData.HintObject> HIntISMarkedOrISAvailableAndUnchecked { get; set; } = new List<HintData.HintObject>();
            public List<HintData.HintObject> HintStateIsNOTChecked { get; set; } = new List<HintData.HintObject>();
            public List<HintData.HintObject> HintStateIsUnchecked { get; set; } = new List<HintData.HintObject>();
            public List<HintData.HintObject> HintStateIsChecked { get; set; } = new List<HintData.HintObject>();
            public List<HintData.HintObject> HistStateIsMarked { get; set; } = new List<HintData.HintObject>();

            public List<MacroObject> Tricks { get; set; } = new List<MacroObject>();
            public List<ItemData.ItemObject> AvailableStartingItems { get; set; } = new List<ItemData.ItemObject>();

            public List<ItemData.ItemObject> LocalObtainedItems { get; set; } = new List<ItemData.ItemObject>();
            public List<ItemData.ItemObject> CurrentStartingItems { get; set; } = new List<ItemData.ItemObject>();
            public List<ItemData.ItemObject> OnlineObtainedItems { get; set; } = new List<ItemData.ItemObject>();
        }

        public static DataSets CreateDataSets(InstanceData.TrackerInstance instance)
        {
            DataSets dataSets = new DataSets();

            bool ShowUnavailableMarkedLocations = instance.StaticOptions.OptionFile.ShowUnavailableMarkedLocations;

            foreach (var i in instance.LocationPool.Values)
            {
                switch (i.CheckState)
                {
                    case CheckState.Checked:
                        dataSets.LocationStateIsChecked.Add(i);
                        break;
                    case CheckState.Marked:
                        dataSets.LocationStateIsMarked.Add(i);
                        dataSets.LocationStateIsNOTChecked.Add(i);
                        if (ShowUnavailableMarkedLocations) { dataSets.LocationISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                    case CheckState.Unchecked:
                        dataSets.LocationStateIsUnchecked.Add(i);
                        dataSets.LocationStateIsNOTChecked.Add(i);
                        if (i.Available) { dataSets.LocationISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                }
            }

            //dataSets.UncheckedLocations = instance.LocationPool.Values.Where(x => x.CheckState == MiscData.CheckState.Unchecked).ToList();
            //dataSets.MarkedLocations = instance.LocationPool.Values.Where(x => x.CheckState == MiscData.CheckState.Marked).ToList();
            //dataSets.CheckedLocations = instance.LocationPool.Values.Where(x => x.CheckState == MiscData.CheckState.Checked).ToList();
            //dataSets.UncheckedOrMarkedLocations = instance.LocationPool.Values.Where(x => x.CheckState != MiscData.CheckState.Checked).ToList();
            //dataSets.AvailableLocations = dataSets.UncheckedOrMarkedLocations.Where(x => x.Available || x.CheckState == MiscData.CheckState.Marked).ToList();

            foreach (var i in instance.LocationProxyData.LocationProxies.Values)
            {
                var CheckState = i.GetReferenceLocation().CheckState;
                switch (CheckState)
                {
                    case MiscData.CheckState.Checked:
                        dataSets.ProxyStateIsChecked.Add(i);
                        break;
                    case MiscData.CheckState.Marked:
                        dataSets.ProxyStateIsNOTChecked.Add(i);
                        dataSets.ProxyStateIsMarked.Add(i);
                        if (ShowUnavailableMarkedLocations) { dataSets.ProxyISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                    case MiscData.CheckState.Unchecked:
                        dataSets.ProxyStateIsNOTChecked.Add(i);
                        if (i.ProxyAvailable()) { dataSets.ProxyISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                }
            }

            //dataSets.ProxyStateIsMarked = instance.LocationProxyData.LocationProxies.Values.Where(x => x.GetReferenceLocation(instance).CheckState == MiscData.CheckState.Marked).ToList();
            //dataSets.ProxyStateIsNOTChecked = instance.LocationProxyData.LocationProxies.Values.Where(x => x.GetReferenceLocation(instance).CheckState != MiscData.CheckState.Checked).ToList();
            //dataSets.ProxyStateIsMarkedAndAvailable = dataSets.ProxyStateIsNOTChecked.Where(x => x.ProxyAvailable(instance) || x.GetReferenceLocation(instance).CheckState == MiscData.CheckState.Marked).ToList();

            var AllExits = instance.GetAllRandomizableExits();
            foreach (var i in AllExits)
            {
                switch (i.CheckState)
                {
                    case CheckState.Checked:
                        dataSets.ExitStateIsChecked.Add(i);
                        break;
                    case CheckState.Marked:
                        dataSets.ExitStateIsMarked.Add(i);
                        dataSets.ExitStateIsNOTChecked.Add(i);
                        if (ShowUnavailableMarkedLocations) { dataSets.ExitISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                    case CheckState.Unchecked:
                        dataSets.ExitStateIsUnchecked.Add(i);
                        dataSets.ExitStateIsNOTChecked.Add(i);
                        if (i.Available) { dataSets.ExitISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                }
            }
            //dataSets.ExitStateIsUnchecked = AllExits.Where(x => x.CheckState == MiscData.CheckState.Unchecked).ToList();
            //dataSets.ExitStateIsChecked = AllExits.Where(x => x.CheckState == MiscData.CheckState.Checked).ToList();
            //dataSets.ExitStateIsMarked = AllExits.Where(x => x.CheckState == MiscData.CheckState.Marked).ToList();
            //dataSets.ExitStateIsNOTChecked = AllExits.Where(x => x.CheckState != MiscData.CheckState.Checked).ToList();
            //dataSets.ExitISMarkedOrISAvailableAndUnchecked = AllExits.Where(x => x.Available || x.CheckState == MiscData.CheckState.Marked).ToList();

            foreach (var i in instance.HintPool.Values)
            {
                switch (i.CheckState)
                {
                    case CheckState.Checked:
                        dataSets.HintStateIsChecked.Add(i);
                        break;
                    case CheckState.Marked:
                        dataSets.HistStateIsMarked.Add(i);
                        dataSets.HintStateIsNOTChecked.Add(i);
                        if (ShowUnavailableMarkedLocations) { dataSets.HIntISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                    case CheckState.Unchecked:
                        dataSets.HintStateIsUnchecked.Add(i);
                        dataSets.HintStateIsNOTChecked.Add(i);
                        if (i.Available) { dataSets.HIntISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                }
            }

            //dataSets.HintStateIsUnchecked = instance.HintPool.Values.Where(x => x.CheckState == MiscData.CheckState.Unchecked).ToList();
            //dataSets.HistStateIsMarked = instance.HintPool.Values.Where(x => x.CheckState == MiscData.CheckState.Marked).ToList();
            //dataSets.HintStateIsChecked = instance.HintPool.Values.Where(x => x.CheckState == MiscData.CheckState.Checked).ToList();
            //dataSets.HintStateIsNOTChecked = instance.HintPool.Values.Where(x => x.CheckState != MiscData.CheckState.Checked).ToList();
            //dataSets.HIntISMarkedOrISAvailableAndUnchecked = dataSets.HintStateIsNOTChecked.Where(x => x.Available || x.CheckState == MiscData.CheckState.Marked).ToList();

            dataSets.Tricks = instance.MacroPool.Values.Where(x => x.isTrick()).ToList();
            dataSets.AvailableStartingItems = instance.ItemPool.Values.Where(x => x.ValidStartingItem()).ToList();

            dataSets.LocalObtainedItems = instance.ItemPool.Values.Where(x => x.AmountAquiredLocally > 0).ToList();
            dataSets.CurrentStartingItems = instance.ItemPool.Values.Where(x => x.AmountInStartingpool > 0).ToList();
            dataSets.OnlineObtainedItems = instance.ItemPool.Values.Where(x => x.AmountAquiredOnline.Any(x => x.Value > 0)).ToList();
            return dataSets;
        }

        private static string GetLocationEntryArea(object Entry)
        {
            if (Entry is LocationData.LocationObject l) { return l.GetDictEntry().Area; }
            else if (Entry is LocationData.LocationProxy p) { return p.GetDictEntry().Area; }
            return "Error";
        }

        private static bool EntranceAppearsInListbox(EntranceData.EntranceRandoExit Location)
        {
            return !Location.IsJunk() && !Location.IsUnrandomized(MiscData.UnrandState.Unrand);
        }

        private static bool SortByAvailability(object Entry, TrackerLocationDataList Data)
        {
            if (Data.ShowUnavailableEntries) { return true; }
            if (!Data.Instance.StaticOptions.OptionFile.SeperateUnavailableMarkedLocations) { return true; }
            if (Entry is LocationData.LocationObject l) { return l.Available; }
            else if (Entry is LocationData.LocationProxy p) { return p.ProxyAvailable(); }
            else if (Entry is EntranceData.EntranceRandoExit e) { return e.Available; }
            return false;
        }

        static bool IsHidden(object obj) { return MiscUtilities.DynamicPropertyExist(obj, "Hidden") && (obj as dynamic).Hidden; }

        public static string GetLocationDisplayName(dynamic obj, InstanceData.InstanceContainer instance)
        {
            LocationData.LocationObject Location;
            string LocationDisplay, RandomizedItemDisplay, PriceDisplay, StarredDisplay, ForPlayer;
            if (obj is LocationData.LocationObject lo)
            {
                Location = lo;
                LocationDisplay = Location.GetDictEntry()?.GetName();
                StarredDisplay = lo.Starred ? "*" : "";
            }
            else if (obj is LocationData.LocationProxy po)
            {
                Location = po.GetReferenceLocation();
                LocationDisplay = po.GetDictEntry().Name ?? Location.GetDictEntry()?.GetName();
                StarredDisplay = po.Starred ? "*" : "";
            }
            else { return obj.ToString(); }
            bool Available = Location.Available;

            obj.GetPrice(out int p, out char c);
            PriceDisplay = p < 0 || (!Available) ? "" : $" [{c}{p}]";
            RandomizedItemDisplay = instance.Instance.GetItemByID(Location.Randomizeditem.Item)?.GetDictEntry()?.GetName() ?? Location.Randomizeditem.Item;

            ForPlayer = Location.Randomizeditem.Item is not null && Location.Randomizeditem.OwningPlayer >= 0 ? $" [Player: {PlayerNumber(Location.Randomizeditem.OwningPlayer)}]" : "";

            return Location.CheckState switch
            {
                MiscData.CheckState.Marked => $"{LocationDisplay}: {RandomizedItemDisplay}{ForPlayer}{StarredDisplay}{PriceDisplay}",
                MiscData.CheckState.Unchecked => $"{LocationDisplay}{StarredDisplay}",
                MiscData.CheckState.Checked => $"{RandomizedItemDisplay}{ForPlayer}{PriceDisplay}: {LocationDisplay}{StarredDisplay}",
                _ => obj.ToString(),
            };

            string PlayerNumber(int Player)
            {
                if (instance.netConnection.PlayerNames.TryGetValue(Player, out string name))
                {
                    return $"{Player} ({name})";
                }
                return Player.ToString();
            }
        }


        //ListBoxBuilder
        public static TrackerLocationDataList WriteLocations(this TrackerLocationDataList Data, CheckState checkState, bool Hidden)
        {
            List<LocationProxy> Proxies;
            List<LocationObject> Locations;
            if (checkState == CheckState.Checked)
            {
                Locations = Data.DataSets.LocationStateIsChecked;
                Proxies = Data.DataSets.ProxyStateIsChecked;
            }
            else
            {
                Proxies = Data.ShowUnavailableEntries ? Data.DataSets.ProxyStateIsNOTChecked : Data.DataSets.ProxyISMarkedOrISAvailableAndUnchecked;
                Locations = Data.ShowUnavailableEntries ? Data.DataSets.LocationStateIsNOTChecked : Data.DataSets.LocationISMarkedOrISAvailableAndUnchecked;
            }

            IEnumerable<object> LocationsEntries = Locations.Where(x => !Data.Instance.LocationProxyData.LocationsWithProxys.ContainsKey(x.ID));
            LocationsEntries = LocationsEntries.Concat(Proxies);
            LocationsEntries = LocationsEntries.OrderByDescending(x => SortByAvailability(x, Data))
                .ThenBy(x => (Data.Categories.ContainsKey(GetLocationEntryArea(x).ToLower().Trim()) ? Data.Categories[GetLocationEntryArea(x).ToLower().Trim()] : LocationsEntries.Count() + 1))
                .ThenBy(x => GetLocationEntryArea(x))
                .ThenBy(x => GetLocationDisplayName(x, Data.InstanceContainer)).ToList();

            LocationsEntries = LocationsEntries.Where(x => Hidden ? IsHidden(x) : !IsHidden(x));

            if (Data.Reverse) { LocationsEntries = LocationsEntries.Reverse(); }

            string CurrentLocation = "";
            foreach (var obj in LocationsEntries)
            {
                var CurrentArea = "";
                if (obj is LocationData.LocationObject i)
                {
                    if (!i.AppearsinListbox(Data.ShowInvalidEntries)) { continue; }
                    i.DisplayName = GetLocationDisplayName(i, Data.InstanceContainer);
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, i, Data.Filter, i.DisplayName)) { continue; }
                    Data.ItemsDisplayed++;
                    CurrentArea = i.GetDictEntry().Area;
                }
                else if (obj is LocationData.LocationProxy p)
                {
                    if (!p.GetReferenceLocation().AppearsinListbox(Data.ShowInvalidEntries)) { continue; }
                    p.DisplayName = GetLocationDisplayName(p, Data.InstanceContainer);
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, p, Data.Filter, p.DisplayName)) { continue; }
                    Data.ItemsDisplayed++;
                    CurrentArea = p.GetDictEntry().Area;
                }
                else { continue; }
                if (Hidden) { CurrentArea = "Hidden Locations"; }
                if (CurrentLocation != CurrentArea)
                {
                    if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                    Data.FinalData.Add(new MiscData.Areaheader { Area = CurrentArea });
                    CurrentLocation = CurrentArea;
                }
                Data.FinalData.Add(obj);
            }
            return Data;
        }
        public static TrackerLocationDataList WriteOptions(this TrackerLocationDataList Data)
        {
            List<dynamic> ChoiceOptions = Data.Instance.ChoiceOptions.Values.Where(x => x.ValueList.Count > 1).Cast<dynamic>().ToList();
            List<dynamic> MultiSelectOptions = Data.Instance.MultiSelectOptions.Values.Cast<dynamic>().ToList();
            List<dynamic> ToggleOptions = Data.Instance.ToggleOptions.Values.Cast<dynamic>().ToList();
            List<dynamic> IntOptions = Data.Instance.IntOptions.Values.Cast<dynamic>().ToList();
            List<dynamic> All = ChoiceOptions.Concat(ToggleOptions).Concat(IntOptions).Concat(MultiSelectOptions).OrderBy(x => x.Priority).ToList();

            Dictionary<string, List<dynamic>> Categorized = new Dictionary<string, List<dynamic>>();
            foreach (var item in All)
            {
                if (!Data.InstanceContainer.logicCalculation.ConditionalsMet(item.Conditionals)) { continue; }
                string Sub = item.SubCategory;
                Sub ??= "";
                if (!Categorized.ContainsKey(Sub)) { Categorized.Add(Sub, new List<dynamic>()); }
                Categorized[Sub].Add(item);
            }

            if (Data.Reverse) { Categorized = Categorized.Reverse().ToDictionary(x => x.Key, x => x.Value); }

            string CurrentCategory = null;
            foreach (var i in Categorized)
            {
                foreach (var c in i.Value)
                {
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, c, Data.Filter, c.ToString())) { continue; }
                    if (CurrentCategory is null || CurrentCategory != i.Key)
                    {
                        if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                        Data.FinalData.Add(new MiscData.Areaheader { Area = i.Key == "" ? "Options" : $"Options: {i.Key}" });
                        CurrentCategory = i.Key;
                    }
                    Data.ItemsDisplayed++;
                    Data.FinalData.Add(c);
                    if (c is OptionData.MultiSelectOption MSO)
                    {
                        foreach (var op in MSO.ValueList.Values)
                        {
                            Data.FinalData.Add(new OptionData.MultiSelectValueListDisplay { Parent = MSO, Value = op });
                        }
                    }
                }
            }
            return Data;
        }
        public static TrackerLocationDataList WriteHints(this TrackerLocationDataList Data, CheckState checkState)
        {
            List<HintObject> HintList;
            if (checkState == CheckState.Checked)
            {
                HintList = Data.DataSets.HintStateIsChecked;
            }
            else
            {
                HintList = Data.ShowUnavailableEntries ? Data.DataSets.HintStateIsNOTChecked : Data.DataSets.HIntISMarkedOrISAvailableAndUnchecked;
            }
            if (HintList.Count != 0)
            {
                bool DividerCreated = false;
                if (Data.Reverse) { HintList.Reverse(); }
                foreach (var i in HintList)
                {
                    if (i.RandomizedState == MiscData.RandomizedState.ForcedJunk && !Data.ShowInvalidEntries) { continue; }
                    i.DisplayName = (i.CheckState != MiscData.CheckState.Unchecked) ? $"{i.GetDictEntry().Name}: {i.HintText}" : i.GetDictEntry().Name;
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, i, Data.Filter, i.DisplayName)) { continue; }
                    Data.ItemsDisplayed++;
                    if (!DividerCreated)
                    {
                        if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                        Data.FinalData.Add(new MiscData.Areaheader { Area = "HINTS" });
                        DividerCreated = true;
                    }
                    Data.FinalData.Add(i);
                }
            }
            return Data;
        }
        public static TrackerLocationDataList WriteEntrances(this TrackerLocationDataList Data, CheckState checkState, bool AddAreaEntranceHeader)
        {
            List<EntranceRandoExit> ValidExits;
            if (checkState == CheckState.Checked)
            {
                ValidExits = Data.DataSets.ExitStateIsChecked;
            }
            else
            {
                ValidExits = Data.ShowUnavailableEntries ? Data.DataSets.ExitStateIsNOTChecked : Data.DataSets.ExitISMarkedOrISAvailableAndUnchecked;
            }
            ValidExits = ValidExits.OrderByDescending(x => SortByAvailability(x, Data) && x.Available)
                .ThenBy(x => x.DisplayArea())
                .ThenBy(x => x.DisplayName).ToList();
            if (Data.Reverse) { ValidExits.Reverse(); }

            string CurrentArea = "";
            foreach (var i in ValidExits)
            {
                if (!EntranceAppearsInListbox(i) && !Data.ShowInvalidEntries) { continue; }
                Data.ItemsFound++;
                string ItemArea = AddAreaEntranceHeader ? $"{i.DisplayArea()} Entrances" : i.DisplayArea();
                i.DisplayName = i.GetEntranceDisplayName();
                if (!SearchStringParser.FilterSearch(Data.Instance, i, Data.Filter, i.GetEntranceDisplayName())) { continue; }
                Data.ItemsDisplayed++;
                if (CurrentArea != ItemArea)
                {
                    CurrentArea = ItemArea;
                    if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                    Data.FinalData.Add(new MiscData.Areaheader { Area = CurrentArea });
                }
                Data.FinalData.Add(i);
            }
            return Data;
        }
        public static TrackerLocationDataList WriteStartingItems(this TrackerLocationDataList Data)
        {
            if (Data.DataSets.CurrentStartingItems.Any())
            {
                var StartingItems = Data.Reverse ? Data.DataSets.CurrentStartingItems.ToArray().Reverse() : Data.DataSets.CurrentStartingItems;
                bool DividerCreated = false;
                List<StartingItemDisplay> StartingItemObjs = [];
                foreach (var i in StartingItems)
                {
                    var DisplayObject = new StartingItemDisplay(i);
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, i, Data.Filter, DisplayObject.ToString())) { continue; }
                    if (!DividerCreated)
                    {
                        if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                        Data.FinalData.Add(new MiscData.Areaheader { Area = "Starting Items" });
                        DividerCreated = true;
                    }
                    Data.ItemsDisplayed++;
                    StartingItemObjs.Add(new StartingItemDisplay(i));
                }
                StartingItemObjs = StartingItemObjs.OrderBy(x => x.ItemObject.GetDictEntry().GetName()).ToList();
                Data.FinalData.AddRange(StartingItemObjs);
            }
            return Data;
        }
        public static TrackerLocationDataList WriteOnlineItems(this TrackerLocationDataList Data)
        {
            if (Data.DataSets.OnlineObtainedItems.Any())
            {
                var OnlineItems = Data.Reverse ? Data.DataSets.OnlineObtainedItems.ToArray().Reverse() : Data.DataSets.OnlineObtainedItems;
                bool DividerCreated = false;
                List<OnlineItemDisplay> OnlineItemObjs = [];
                foreach (var i in OnlineItems)
                {
                    foreach (var j in i.AmountAquiredOnline)
                    {
                        var DisplayObject = new OnlineItemDisplay(i, j.Value, j.Key);
                        Data.ItemsFound++;
                        if (!SearchStringParser.FilterSearch(Data.Instance, i, Data.Filter, DisplayObject.ToString())) { continue; }
                        if (!DividerCreated)
                        {
                            if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                            Data.FinalData.Add(new MiscData.Areaheader { Area = "MultiWorld Items" });
                            DividerCreated = true;
                        }
                        Data.ItemsDisplayed++;
                        OnlineItemObjs.Add(DisplayObject);
                    }
                }
                OnlineItemObjs = OnlineItemObjs.OrderBy(x => x.ItemObject.GetDictEntry().GetName()).ThenBy(x => x.PlayerID).ToList();
                Data.FinalData.AddRange(OnlineItemObjs);
            }
            return Data;
        }

        public static TrackerLocationDataList WriteRemoteItemHints(this TrackerLocationDataList Data)
        {
            if (Data.InstanceContainer.netConnection is not null && Data.InstanceContainer.netConnection.RemoteHints.Any())
            {
                var RemoteHintList = Data.InstanceContainer.netConnection.RemoteHints.ToArray();
                var RemoteItemHints = Data.Reverse ? RemoteHintList.Reverse() : RemoteHintList;
                bool DividerCreated = false;
                List<RemoteLocationHint> RemoteHintObjs = [];
                foreach (var item in RemoteItemHints)
                {
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, item, Data.Filter, item.ToString())) { continue; }
                    if (!DividerCreated)
                    {
                        if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                        Data.FinalData.Add(new MiscData.Areaheader { Area = "MULTIWORLD ITEM HINTS" });
                        DividerCreated = true;
                    }
                    Data.ItemsDisplayed++;
                    RemoteHintObjs.Add(item);
                }
                RemoteHintObjs = RemoteHintObjs.OrderBy(x => x.Item.GetDictEntry().GetName()).ThenBy(x => x.Location).ThenBy(x => x.RemotePlayerID).ToList();
                Data.FinalData.AddRange(RemoteHintObjs);
            }
            return Data;
        }

    }
}
