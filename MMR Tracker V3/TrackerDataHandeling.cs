using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using static MMR_Tracker_V3.TrackerObjects.EntranceData;
using static MMR_Tracker_V3.TrackerObjects.HintData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public class TrackerDataHandeling
    {

        public static event Action<List<object>, InstanceData.TrackerInstance, CheckState> CheckedObjectsUpdate;
        public static void TriggerCheckedObjectsUpdate(List<object> objs, InstanceData.TrackerInstance instance, CheckState c) 
        {
            if (CheckedObjectsUpdate is null) { return; }
            CheckedObjectsUpdate(objs, instance, c); 
        }
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

        //Check Sets of items
        public static List<object> CheckSelectedItems(IEnumerable<object> SelectedObjects, MiscData.InstanceContainer instanceContainer, CheckItemSetting Options)
        {
            List<object> UpdatedObjects = new List<object>();

            List<object> UpdatedOptions = SetOptionCheckState(SelectedObjects, instanceContainer, Options);
            List<LocationObject> UpdatedLocations = SetLocationsCheckState(SelectedObjects, instanceContainer, Options);
            List<EntranceRandoExit> UpdatedExits = SetEntrancesCheckState(SelectedObjects, instanceContainer, Options);

            UpdatedObjects.AddRange(UpdatedOptions);
            UpdatedObjects.AddRange(UpdatedLocations);
            UpdatedObjects.AddRange(UpdatedExits);

            if (UpdatedObjects.Any() && Options.TargetheckState != MiscData.CheckState.Marked)
            {
                instanceContainer.logicCalculation.CalculateLogic(Options.TargetheckState);
            }

            List<HintObject> UpdatedHints = SetHintsCheckState(SelectedObjects, instanceContainer, Options);
            List<LocationObject> HintedLocationsToUpdate = GetHIntedLocationsToUpdated(UpdatedHints, instanceContainer);

            List<LocationObject> UpdatedHintedLocations = SetLocationsCheckState(HintedLocationsToUpdate, instanceContainer, new CheckItemSetting(Options).SetTargetheckState(CheckState.Marked));

            UpdatedObjects.AddRange(UpdatedHints);
            UpdatedObjects.AddRange(UpdatedHintedLocations);

            if (UpdatedExits.Any() && Options.TargetheckState == MiscData.CheckState.Checked)
            {
                Dictionary<EntranceRandoExit, EntranceRandoDestination> PairedExits = GetEntrancePairsToUpdate(UpdatedExits, instanceContainer);
                foreach (var i in PairedExits) { if (i.Key.GetDestinationAtExit() is null) { i.Key.DestinationExit = i.Value; } }
                IEnumerable<EntranceRandoExit> PairedExitsToMark = PairedExits.Keys.Where(x => !x.Available);
                IEnumerable<EntranceRandoExit> PairedExitsToCheck = PairedExits.Keys.Where(x => x.Available);
                List<EntranceRandoExit> UpdatedMarkedPairedExits = SetEntrancesCheckState(PairedExitsToMark, instanceContainer, Options.Copy().SetTargetheckState(CheckState.Marked).SetEnforceMarkAction(true));
                List<EntranceRandoExit> UpdatedCheckedPairedExits = SetEntrancesCheckState(PairedExitsToCheck, instanceContainer, Options.Copy().SetTargetheckState(CheckState.Checked).SetEnforceMarkAction(true));
                if (UpdatedCheckedPairedExits.Any())
                {
                    instanceContainer.logicCalculation.CalculateLogic(Options.TargetheckState);
                }
                UpdatedObjects.AddRange(UpdatedMarkedPairedExits);
                UpdatedObjects.AddRange(UpdatedCheckedPairedExits);
            }


            if (UpdatedObjects.Any() && CheckedObjectsUpdate is not null) { CheckedObjectsUpdate(UpdatedObjects, instanceContainer.Instance, Options.TargetheckState); }
            return UpdatedObjects;
        }

        public static List<object> SetOptionCheckState(IEnumerable<object> SelectedObjects, MiscData.InstanceContainer instanceContainer, CheckItemSetting Options)
        {
            List<object> UpdatedObjects = new List<object>();

            //Handle Options
            IEnumerable<OptionData.ChoiceOption> choiceOptions = SelectedObjects.Where(x => x is OptionData.ChoiceOption).Select(x => x as OptionData.ChoiceOption);
            IEnumerable<OptionData.IntOption> IntOptions = SelectedObjects.Where(x => x is OptionData.IntOption).Select(x => x as OptionData.IntOption);
            IEnumerable<OptionData.ToggleOption> ToggleOptions = SelectedObjects.Where(x => x is OptionData.ToggleOption).Select(x => x as OptionData.ToggleOption);
            IEnumerable<OptionData.MultiSelectValueListDisplay> MultiSelectOptions = SelectedObjects.Where(x => x is OptionData.MultiSelectValueListDisplay).Select(x => x as OptionData.MultiSelectValueListDisplay);

            foreach (var i in ToggleOptions)
            {
                i.ToggleValue();
            }
            foreach(var i in MultiSelectOptions)
            {
                i.Parent.ToggleValue(i.Value.ID);
            }
            if (choiceOptions.Any())
            {
                var Result = Options.CheckCoiceOptions(choiceOptions, instanceContainer);
                foreach(var O in Result) { O.GetCheck<OptionData.ChoiceOption>().SetValue(O.GetItem<string>()); }
            }
            if (IntOptions.Any())
            {
                var Result = Options.CheckIntOPtions(IntOptions, instanceContainer);
                foreach (var O in Result) { O.GetCheck<OptionData.IntOption>().SetValue(O.GetItem<int>()); }
            }

            UpdatedObjects.AddRange(choiceOptions);
            UpdatedObjects.AddRange(IntOptions);
            UpdatedObjects.AddRange(ToggleOptions);

            if (UpdatedObjects.Any()) { instanceContainer.logicCalculation.CompileOptionActionEdits(); }

            return UpdatedObjects;
        }

        public static List<LocationObject> SetLocationsCheckState(IEnumerable<object> SelectedObjects, MiscData.InstanceContainer instanceContainer, CheckItemSetting Options)
        {
            List<LocationObject> UpdatedObjects = new List<LocationObject>();

            //Handle Locations
            IEnumerable<LocationObject> locationObjects = SelectedObjects.Where(x => x is LocationObject).Select(x => x as LocationObject);
            locationObjects = locationObjects.Concat(SelectedObjects.Where(x => x is LocationProxy).Select(x => (x as LocationProxy).GetReferenceLocation(instanceContainer.Instance)));
            locationObjects = locationObjects.Distinct();
            //If we are performing an uncheck action there should be no unchecked locations in the list and even if there are nothing will be done to them anyway
            //This check is neccessary for the "UnMark Only" action.
            IEnumerable<LocationObject> UncheckedlocationObjects = (Options.TargetheckState == MiscData.CheckState.Unchecked) ?
                new List<LocationObject>() :
                locationObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);

            foreach (LocationObject LocationObject in UncheckedlocationObjects)
            {
                LocationObject.Randomizeditem.Item = LocationObject.GetItemAtCheck(instanceContainer.Instance);
            }
            //Get Entries that need a value manually assigned and pass them to given method to be assigned.
            IEnumerable<LocationObject> ManualLocationChecks = UncheckedlocationObjects.Where(x => x.Randomizeditem.Item == null); //Locations with no item
            if (ManualLocationChecks.Any())
            {
                var Result = Options.CheckUnassignedLocations(ManualLocationChecks, instanceContainer);
                foreach (var O in Result) { O.GetCheck<LocationObject>().Randomizeditem.Item = O.GetItem<string>(); O.GetCheck<LocationObject>().Randomizeditem.OwningPlayer = O.OwningPlayer; }
            }
            foreach (LocationObject LocationObject in locationObjects)
            {
                //When we mark a location, the action is always sent as Marked, but if the location is already marked we should instead Unchecked it unless EnforceMarkAction is true.
                var Action = (Options.TargetheckState == MiscData.CheckState.Marked && LocationObject.CheckState == MiscData.CheckState.Marked) && !Options.EnforceMarkAction ? MiscData.CheckState.Unchecked : Options.TargetheckState;
                if (LocationObject.ToggleChecked(Action, instanceContainer.Instance))
                {
                    UpdatedObjects.Add(LocationObject);
                }
            }

            return UpdatedObjects;
        }

        public static List<EntranceRandoExit> SetEntrancesCheckState(IEnumerable<object> SelectedObjects, MiscData.InstanceContainer instanceContainer, CheckItemSetting Options)
        {
            List<EntranceRandoExit> UpdatedObjects = new List<EntranceRandoExit>();

            //Handle Exits
            IEnumerable<EntranceRandoExit> ExitObjects = SelectedObjects.Where(x => x is EntranceRandoExit).Select(x => x as EntranceRandoExit);
            IEnumerable<EntranceRandoExit> UncheckedExitObjects = (Options.TargetheckState == MiscData.CheckState.Unchecked) ?
                new List<EntranceRandoExit>() :
                ExitObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);
            foreach (EntranceRandoExit ExitObject in UncheckedExitObjects)
            {
                ExitObject.DestinationExit = ExitObject.GetDestinationAtExit();
            }
            IEnumerable<EntranceRandoExit> ManualExitChecks = UncheckedExitObjects.Where(x => x.DestinationExit == null); //Exits With No Destination
            if (ManualExitChecks.Any())
            {
                var Result = Options.CheckUnassignedEntrances(ManualExitChecks, instanceContainer);
                foreach (var O in Result) { O.GetCheck<EntranceRandoExit>().DestinationExit = O.GetItem<EntranceRandoDestination>(); }
            }
            foreach (EntranceRandoExit ExitObject in ExitObjects)
            {
                var Action = (Options.TargetheckState == MiscData.CheckState.Marked && ExitObject.CheckState == MiscData.CheckState.Marked) && !Options.EnforceMarkAction ? MiscData.CheckState.Unchecked : Options.TargetheckState;
                if (ExitObject.ToggleExitChecked(Action, instanceContainer.Instance))
                {
                    UpdatedObjects.Add(ExitObject);
                }
            }
            return UpdatedObjects;
        }

        public static List<HintObject> SetHintsCheckState(IEnumerable<object> SelectedObjects, MiscData.InstanceContainer instanceContainer, CheckItemSetting Options)
        {
            List<HintObject> UpdatedObjects = new List<HintObject>();
            //Hints======================================
            List<HintObject> HintObjects = SelectedObjects.Where(x => x is HintObject).Select(x => x as HintObject).ToList();

            var UncheckedHintObjects = HintObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);
            foreach (var i in UncheckedHintObjects.Where(x => !string.IsNullOrWhiteSpace(x.SpoilerHintText)))
            {
                i.HintText = i.SpoilerHintText;
            }
            IEnumerable<HintObject> UncheckedVariableObjects = UncheckedHintObjects.Where(x => string.IsNullOrWhiteSpace(x.HintText));
            if (UncheckedVariableObjects.Any())
            {
                var Result = Options.CheckUnassignedHints(UncheckedVariableObjects, instanceContainer);
                foreach (var O in Result) { O.GetCheck<HintObject>().HintText = O.GetItem<string>(); }
            }
            foreach (HintObject hintObject in HintObjects)
            {
                UpdatedObjects.Add(hintObject);
                var CheckAction = (Options.TargetheckState == MiscData.CheckState.Marked && hintObject.CheckState == MiscData.CheckState.Marked) && !Options.EnforceMarkAction ? MiscData.CheckState.Unchecked : Options.TargetheckState;
                hintObject.CheckState = CheckAction;
                hintObject.HintText = CheckAction == MiscData.CheckState.Unchecked ? null : hintObject.HintText;
            }
            return UpdatedObjects;
        }

        //Utility
        public static List<LocationObject> GetHIntedLocationsToUpdated(List<HintObject> UpdatedHints, InstanceContainer instanceContainer)
        {
            List<LocationObject> LocationsToMark = new List<LocationObject>();
            foreach (var hint in UpdatedHints)
            {
                if (hint.CheckState != MiscData.CheckState.Checked) { continue; }
                foreach (var i in hint.ParsedHintData)
                {
                    var Location = instanceContainer.Instance.GetLocationByID(i.Key);
                    var Item = instanceContainer.Instance.GetItemByID(i.Value);
                    if (Location is null || Item is null || Location.CheckState != MiscData.CheckState.Unchecked) { continue; }
                    string ProperCheckItem = Location.GetItemAtCheck(instanceContainer.Instance);
                    if (string.IsNullOrWhiteSpace(ProperCheckItem) || ProperCheckItem != Item.ID) { continue; }
                    LocationsToMark.Add(Location);
                }
            }
            return LocationsToMark;
        }

        public static Dictionary<EntranceRandoExit, EntranceRandoDestination> GetEntrancePairsToUpdate(List<EntranceRandoExit> UpdatedExits, InstanceContainer instanceContainer)
        {
            Dictionary<EntranceRandoExit, EntranceRandoDestination> PairedExits = new Dictionary<EntranceRandoExit, EntranceRandoDestination>();
            foreach (var exit in UpdatedExits)
            {
                if (exit.CheckState != CheckState.Checked) { continue; } //Doesn't support unchecking pairs
                if (!exit.IsRandomizableEntrance(instanceContainer.Instance) || exit.EntrancePair is null) { continue; }
                //Get the Pair of the exits destination
                var PairExit = exit.DestinationExit.AsExit(instanceContainer.Instance).EntrancePair?.AsExit(instanceContainer.Instance);
                if (PairExit == null) { continue; }
                //If the pair has already been checked or if it's already in the state it needs to be skip it
                CheckState CheckAction = PairExit.Available ? CheckState.Checked : CheckState.Marked;
                if (PairExit.CheckState == CheckAction || PairExit.CheckState == CheckState.Checked) { continue; }
                //Get the destination of the pair exit and the destination the spoiler has has defined
                var PairDestination = exit.EntrancePair.AsDestination();
                var ProperDestination = PairExit.GetDestinationAtExit();
                //If DefinedDestination is null the pair destination did not match the spoiler defined destination meaning the entrance is not coupled
                EntranceData.EntranceRandoDestination DefinedDestination = null;
                if (ProperDestination is null) { DefinedDestination = PairDestination; }
                else if (ProperDestination.region == PairDestination.region && ProperDestination.from == PairDestination.from) { DefinedDestination = ProperDestination; }
                if (DefinedDestination is null) { continue; }
                PairedExits[PairExit] = DefinedDestination;
            }
            return PairedExits;
        }

        public static DataSets PopulateDataSets(InstanceData.TrackerInstance instance)
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

            foreach(var i in instance.LocationProxyData.LocationProxies.Values)
            {
                var CheckState = i.GetReferenceLocation(instance).CheckState;
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
                        if (i.ProxyAvailable(instance)) { dataSets.ProxyISMarkedOrISAvailableAndUnchecked.Add(i); }
                        break;
                }
            }

            //dataSets.ProxyStateIsMarked = instance.LocationProxyData.LocationProxies.Values.Where(x => x.GetReferenceLocation(instance).CheckState == MiscData.CheckState.Marked).ToList();
            //dataSets.ProxyStateIsNOTChecked = instance.LocationProxyData.LocationProxies.Values.Where(x => x.GetReferenceLocation(instance).CheckState != MiscData.CheckState.Checked).ToList();
            //dataSets.ProxyStateIsMarkedAndAvailable = dataSets.ProxyStateIsNOTChecked.Where(x => x.ProxyAvailable(instance) || x.GetReferenceLocation(instance).CheckState == MiscData.CheckState.Marked).ToList();

            var AllExits = instance.EntrancePool.AreaList.Values.SelectMany(x => x.RandomizableExits(instance).Values);
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
                        if(i.Available) { dataSets.ExitISMarkedOrISAvailableAndUnchecked.Add(i); }
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

            dataSets.Tricks = instance.MacroPool.Values.Where(x => x.isTrick(instance)).ToList();
            dataSets.AvailableStartingItems = instance.ItemPool.Values.Where(x => x.ValidStartingItem(instance)).ToList();

            dataSets.LocalObtainedItems = instance.ItemPool.Values.Where(x => x.AmountAquiredLocally > 0).ToList();
            dataSets.CurrentStartingItems = instance.ItemPool.Values.Where(x => x.AmountInStartingpool > 0).ToList();
            dataSets.OnlineObtainedItems = instance.ItemPool.Values.Where(x => x.AmountAquiredOnline.Any(x => x.Value > 0)).ToList();
            return dataSets;
        }

        public static string GetLocationEntryArea(object Entry, InstanceData.TrackerInstance Instance)
        {
            if (Entry is LocationData.LocationObject l) { return l.GetDictEntry(Instance).Area; }
            else if (Entry is LocationData.LocationProxy p) { return p.Area; }
            return "Error";
        }

        private static bool EntranceAppearsinListbox(EntranceData.EntranceRandoExit Location, InstanceData.TrackerInstance Instance)
        {
            return !Location.IsJunk() && !Location.IsUnrandomized(MiscData.UnrandState.Unrand);
        }

        //GetData To Print
        public static bool GetLocationEntryAvailablility(object Entry, InstanceData.TrackerInstance Instance)
        {
            if (Entry is LocationData.LocationObject l) { return l.Available; }
            else if (Entry is LocationData.LocationProxy p) { return p.ProxyAvailable(Instance); }
            return false;
        }

        public static void PopulateAvailableLocationList(TrackerLocationDataList Data)
        {
            bool SeperateMarked = Data.Instance.StaticOptions.OptionFile.SeperateUnavailableMarkedLocations;

            var Groups = Utility.GetCategoriesFromFile(Data.Instance);

            var AvailableProxies = Data.DataSets.ProxyISMarkedOrISAvailableAndUnchecked;
            if (Data.ShowUnavailableEntries) { AvailableProxies = Data.DataSets.ProxyStateIsNOTChecked; }

            var AvailableLocations = Data.DataSets.LocationISMarkedOrISAvailableAndUnchecked;
            if (Data.ShowUnavailableEntries) { AvailableLocations = Data.DataSets.LocationStateIsNOTChecked; }

            IEnumerable<object> AvailableLocationsEntries = AvailableLocations.Where(x => !Data.Instance.LocationProxyData.LocationsWithProxys.ContainsKey(x.ID));
            AvailableLocationsEntries = AvailableLocationsEntries.Concat(AvailableProxies);
            AvailableLocationsEntries = AvailableLocationsEntries.OrderByDescending(x => SeperateMarked && GetLocationEntryAvailablility(x, Data.Instance))
                .ThenBy(x => (Groups.ContainsKey(GetLocationEntryArea(x, Data.Instance).ToLower().Trim()) ? Groups[GetLocationEntryArea(x, Data.Instance).ToLower().Trim()] : Data.DataSets.LocationISMarkedOrISAvailableAndUnchecked.Count() + 1))
                .ThenBy(x => GetLocationEntryArea(x, Data.Instance))
                .ThenBy(x => Utility.GetLocationDisplayName(x, Data.InstanceContainer)).ToList();

            IEnumerable<object> HiddenLocations = AvailableLocationsEntries.Where(x => Utility.DynamicPropertyExist(x, "Hidden") && (x as dynamic).Hidden).OrderBy(x => Utility.GetLocationDisplayName(x, Data.InstanceContainer));
            AvailableLocationsEntries = AvailableLocationsEntries.Where(x => !Utility.DynamicPropertyExist(x, "Hidden") || !(x as dynamic).Hidden);

            var AvailableHints = Data.DataSets.HIntISMarkedOrISAvailableAndUnchecked;
            if (Data.ShowUnavailableEntries) { AvailableHints = Data.DataSets.HintStateIsNOTChecked; }


            if (Data.Reverse)
            {
                AvailableLocations.Reverse();
                WriteHiddenLocations(Data, HiddenLocations);
                WriteOptions(Data, 1);
                WriteHints(Data, AvailableHints);
                WriteEntrances();
                WriteLocations(Data, AvailableLocationsEntries);
            }
            else
            {
                WriteLocations(Data, AvailableLocationsEntries);
                WriteEntrances();
                WriteHints(Data, AvailableHints);
                WriteOptions(Data, 1);
                WriteHiddenLocations(Data, HiddenLocations);
            }

            return;

            void WriteEntrances()
            {
                if (Data.InstanceContainer.Instance.StaticOptions.OptionFile.EntranceRandoFeatures) { return; }
                PopulateAvailableEntraceList(Data);
            }
        }

        public static void PopulateAvailableEntraceList(TrackerLocationDataList Data)
        {
            bool InLocationBox = !Data.Instance.StaticOptions.OptionFile.EntranceRandoFeatures;

            bool SeperateMarked = Data.Instance.StaticOptions.OptionFile.SeperateUnavailableMarkedLocations;

            var Groups = Utility.GetCategoriesFromFile(Data.Instance);

            List<EntranceRandoExit> ValidExits = Data.DataSets.ExitISMarkedOrISAvailableAndUnchecked;
            if (Data.ShowUnavailableEntries) { ValidExits = Data.DataSets.ExitStateIsNOTChecked; }

            ValidExits = ValidExits.OrderByDescending(x => SeperateMarked && x.Available).ThenBy(x => x.DisplayArea(Data.Instance)).ThenBy(x => x.DisplayName).ToList();
            string CurrentArea = "";
            foreach(var i in ValidExits)
            {
                if (!EntranceAppearsinListbox(i, Data.Instance) && !Data.ShowInvalidEntries) { continue; }
                Data.ItemsFound++;
                string ItemArea = InLocationBox ? $"{i.DisplayArea(Data.Instance)} Entrances" : i.DisplayArea(Data.Instance);
                i.DisplayName = i.GetEntranceDisplayName(Data.Instance);
                if (!SearchStringParser.FilterSearch(Data.Instance, i, Data.Filter, i.GetEntranceDisplayName(Data.Instance))) { continue; }
                Data.ItemsDisplayed++;
                if (CurrentArea != ItemArea)
                {
                    CurrentArea = ItemArea;
                    if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                    Data.FinalData.Add(new MiscData.Areaheader { Area = CurrentArea });
                }
                Data.FinalData.Add(i);
            }
        }

        public static void PopulateCheckedLocationList(TrackerLocationDataList Data)
        {
            var Groups = Utility.GetCategoriesFromFile(Data.Instance);
            IEnumerable<object> CheckedLocations = Data.DataSets.LocationStateIsChecked.Where(x => !Data.Instance.LocationProxyData.LocationsWithProxys.ContainsKey(x.ID));
            CheckedLocations = CheckedLocations.Concat(Data.DataSets.ProxyStateIsChecked);
            CheckedLocations = CheckedLocations
                .OrderBy(x => (Groups.ContainsKey(GetLocationEntryArea(x, Data.Instance).ToLower().Trim()) ? Groups[GetLocationEntryArea(x, Data.Instance).ToLower().Trim()] : Data.DataSets.LocationISMarkedOrISAvailableAndUnchecked.Count() + 1))
                .ThenBy(x => GetLocationEntryArea(x, Data.Instance))
                .ThenBy(x => Utility.GetLocationDisplayName(x, Data.InstanceContainer)).ToList();

            if (Data.Reverse)
            {
                CheckedLocations.Reverse();
                WriteStartingAndOnlineItems(Data);
                WriteOptions(Data, 2);
                WriteHints(Data, Data.DataSets.HintStateIsChecked);
                WriteCheckedEntrances(Data);
                WriteLocations(Data, CheckedLocations);
            }
            else
            {
                WriteLocations(Data, CheckedLocations);
                WriteCheckedEntrances(Data);
                WriteHints(Data, Data.DataSets.HintStateIsChecked);
                WriteOptions(Data, 2);
                WriteStartingAndOnlineItems(Data);
            }
        }

        //
        private static void WriteOptions(TrackerLocationDataList Data, int DisplayListBoxesIndex)
        {
            if (Data.Instance.StaticOptions.ShowOptionsInListBox == null || Data.Instance.StaticOptions.ShowOptionsInListBox != OptionData.DisplayListBoxes[DisplayListBoxesIndex]) { return; }

            List<dynamic> ChoiceOptions = Data.Instance.ChoiceOptions.Values.Where(x => x.ValueList.Count > 1).Cast<dynamic>().ToList();
            List<dynamic> MultiSelectOptions = Data.Instance.MultiSelectOptions.Values.Cast<dynamic>().ToList();
            List<dynamic> ToggleOptions = Data.Instance.ToggleOptions.Values.Cast<dynamic>().ToList();
            List<dynamic> IntOptions = Data.Instance.IntOptions.Values.Cast<dynamic>().ToList();
            List<dynamic> All = ChoiceOptions.Concat(ToggleOptions).Concat(IntOptions).Concat(MultiSelectOptions).OrderBy(x => x.Priority).ToList();

            Dictionary<string, List<dynamic>> Categorized = new Dictionary<string, List<dynamic>>();
            foreach (var item in All)
            {
                if (!Data.InstanceContainer.logicCalculation.ConditionalsMet(item.Conditionals, new List<string>())) { continue; }
                string Sub = item.SubCategory;
                Sub ??= "";
                if (!Categorized.ContainsKey(Sub)) { Categorized.Add(Sub, new List<dynamic>()); }
                Categorized[Sub].Add(item);
            }

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
        }
        private static void WriteHints(TrackerLocationDataList Data, IEnumerable<HintObject> HintList)
        {
            if (HintList.Any())
            {
                bool DividerCreated = false;
                foreach (var i in HintList)
                {
                    if (i.RandomizedState == MiscData.RandomizedState.ForcedJunk && !Data.ShowInvalidEntries) { continue; }
                    i.DisplayName = (i.CheckState != MiscData.CheckState.Unchecked) ? $"{i.GetDictEntry(Data.Instance).Name}: {i.HintText}" : i.GetDictEntry(Data.Instance).Name;
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
        }
        private static void WriteStartingAndOnlineItems(TrackerLocationDataList Data)
        {
            if (Data.DataSets.CurrentStartingItems.Any())
            {
                bool DividerCreated = false;
                foreach (var i in Data.DataSets.CurrentStartingItems)
                {
                    string Display = $"{i.GetDictEntry(Data.Instance).GetName(Data.Instance)} X{i.AmountInStartingpool}";
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, i, Data.Filter, Display)) { continue; }
                    if (!DividerCreated)
                    {
                        if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                        Data.FinalData.Add(new MiscData.Areaheader { Area = "Starting Items" });
                        DividerCreated = true;
                    }
                    Data.ItemsDisplayed++;
                    Data.FinalData.Add(Display);
                }
            }

            if (Data.DataSets.OnlineObtainedItems.Any())
            {
                bool DividerCreated = false;
                foreach (var i in Data.DataSets.OnlineObtainedItems)
                {
                    foreach (var j in i.AmountAquiredOnline)
                    {
                        string Display = $"{i.GetDictEntry(Data.Instance).GetName(Data.Instance)} X{j.Value}: Player {j.Key}";
                        Data.ItemsFound++;
                        if (!SearchStringParser.FilterSearch(Data.Instance, i, Data.Filter, Display)) { continue; }
                        if (!DividerCreated)
                        {
                            if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                            Data.FinalData.Add(new MiscData.Areaheader { Area = "MultiWorld Items" });
                            DividerCreated = true;
                        }
                        Data.ItemsDisplayed++;
                        Data.FinalData.Add(Display);
                    }
                }
            }
        }
        private static void WriteCheckedEntrances(TrackerLocationDataList Data)
        {
            List<EntranceData.EntranceRandoExit> ValidExits = new List<EntranceData.EntranceRandoExit>();
            foreach (var area in Data.Instance.EntrancePool.AreaList)
            {
                var CheckLoadingZoneExits = area.Value.RandomizableExits(Data.Instance).Where(x => x.Value.CheckState == MiscData.CheckState.Checked && EntranceAppearsinListbox(x.Value, Data.Instance));
                var FilteredCheckedExits = CheckLoadingZoneExits.Where(x => SearchStringParser.FilterSearch(Data.Instance, x.Value, Data.Filter, x.Value.GetEntranceDisplayName(Data.Instance)));

                Data.ItemsFound += CheckLoadingZoneExits.Count();
                Data.ItemsDisplayed += FilteredCheckedExits.Count();
                if (!FilteredCheckedExits.Any()) { continue; }
                foreach (var i in FilteredCheckedExits)
                {
                    i.Value.DisplayName = i.Value.GetEntranceDisplayName(Data.Instance);
                    ValidExits.Add(i.Value);
                }
            }
            ValidExits = ValidExits.OrderBy(x => x.DisplayArea(Data.Instance)).ThenBy(x => x.DisplayName).ToList();
            string CurrentArea = "";
            foreach (var i in ValidExits)
            {
                string ItemArea = $"{i.DisplayArea(Data.Instance)} Exits";
                if (CurrentArea != ItemArea)
                {
                    CurrentArea = ItemArea;
                    if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                    Data.FinalData.Add(new MiscData.Areaheader { Area = CurrentArea });
                }
                Data.FinalData.Add(i);
            }
        }
        private static void WriteHiddenLocations(TrackerLocationDataList Data, IEnumerable<object> HiddenLocations)
        {
            List<object> TempDataSource = new List<object>();
            foreach (var obj in HiddenLocations)
            {
                var CurrentArea = "";
                if (obj is LocationData.LocationObject i)
                {
                    if (!i.AppearsinListbox(Data.InstanceContainer.Instance, Data.ShowInvalidEntries)) { continue; }
                    i.DisplayName = Utility.GetLocationDisplayName(i, Data.InstanceContainer);
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, i, Data.Filter, i.DisplayName)) { continue; }
                    Data.ItemsDisplayed++;
                    CurrentArea = i.GetDictEntry(Data.Instance).Area;
                }
                else if (obj is LocationData.LocationProxy p)
                {
                    if (!p.GetReferenceLocation(Data.Instance).AppearsinListbox(Data.Instance, Data.ShowInvalidEntries)) { continue; }
                    p.DisplayName = Utility.GetLocationDisplayName(p, Data.InstanceContainer);
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, p, Data.Filter, p.DisplayName)) { continue; }
                    Data.ItemsDisplayed++;
                    CurrentArea = p.Area;
                }
                else { continue; }
                TempDataSource.Add(obj);
            }
            if (TempDataSource.Any())
            {
                if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                Data.FinalData.Add(new MiscData.Areaheader { Area = "Hidden Locations" });
                Data.FinalData.AddRange(TempDataSource);
            }
        }
        private static void WriteLocations(TrackerLocationDataList Data, IEnumerable<object> AvailableLocationsEntries)
        {
            string CurrentLocation = "";
            foreach (var obj in AvailableLocationsEntries)
            {
                var CurrentArea = "";
                if (obj is LocationData.LocationObject i)
                {
                    if (!i.AppearsinListbox(Data.Instance, Data.ShowInvalidEntries)) { continue; }
                    i.DisplayName = Utility.GetLocationDisplayName(i, Data.InstanceContainer);
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, i, Data.Filter, i.DisplayName)) { continue; }
                    Data.ItemsDisplayed++;
                    CurrentArea = i.GetDictEntry(Data.Instance).Area;
                }
                else if (obj is LocationData.LocationProxy p)
                {
                    if (!p.GetReferenceLocation(Data.Instance).AppearsinListbox(Data.Instance, Data.ShowInvalidEntries)) { continue; }
                    p.DisplayName = Utility.GetLocationDisplayName(p, Data.InstanceContainer);
                    Data.ItemsFound++;
                    if (!SearchStringParser.FilterSearch(Data.Instance, p, Data.Filter, p.DisplayName)) { continue; }
                    Data.ItemsDisplayed++;
                    CurrentArea = p.Area;
                }
                else { continue; }
                if (CurrentLocation != CurrentArea)
                {
                    if (Data.FinalData.Count > 0) { Data.FinalData.Add(Data.Divider); }
                    Data.FinalData.Add(new MiscData.Areaheader { Area = CurrentArea });
                    CurrentLocation = CurrentArea;
                }
                Data.FinalData.Add(obj);
            }
        }
    }
}
