using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static MMR_Tracker_V3.TrackerObjects.EntranceData;
using static MMR_Tracker_V3.TrackerObjects.HintData;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public class LocationChecker
    {
        public static event Action<List<object>, InstanceData.TrackerInstance> UserOptionUpdated;
        public static event Action<List<object>, InstanceData.TrackerInstance> CheckStateChanged;
        public static void TriggerCheckStateChangedEvent(List<object> UpdatedObjects, InstanceData.TrackerInstance instance)
        {
            CheckStateChanged?.Invoke(UpdatedObjects, instance);
        }
        public static void TriggerUserOptionUpdatedEvent(List<object> UpdatedOptions, InstanceData.TrackerInstance instance)
        {
            UserOptionUpdated?.Invoke(UpdatedOptions, instance);
        }
        //Check Sets of items
        public static List<object> CheckSelectedItems(IEnumerable<object> SelectedObjects, InstanceData.InstanceContainer instanceContainer, CheckItemSetting Options)
        {
            List<object> UpdatedLogicObjects = new List<object>();
            List<object> UpdatedHintObjects = new List<object>();
            List<object> UpdatedSettingObjects = new List<object>();

            List<object> UpdatedOptions = SetOptionCheckState(SelectedObjects, instanceContainer, Options);
            List<LocationObject> UpdatedLocations = SetLocationsCheckState(SelectedObjects, instanceContainer, Options);
            List<EntranceRandoExit> UpdatedExits = SetEntrancesCheckState(SelectedObjects, instanceContainer, Options);

            UpdatedSettingObjects.AddRange(UpdatedOptions);
            UpdatedLogicObjects.AddRange(UpdatedLocations);
            UpdatedLogicObjects.AddRange(UpdatedExits);

            if ((UpdatedLogicObjects.Count != 0 || UpdatedSettingObjects.Count != 0) && Options.TargetCheckState != MiscData.CheckState.Marked)
            {
                instanceContainer.logicCalculation.CalculateLogic(Options.TargetCheckState);
            }

            List<HintObject> UpdatedHints = SetHintsCheckState(SelectedObjects, instanceContainer, Options);
            List<LocationObject> HintedLocationsToUpdate = GetHIntedLocationsToUpdated(UpdatedHints, instanceContainer);

            List<LocationObject> UpdatedHintedLocations = SetLocationsCheckState(HintedLocationsToUpdate, instanceContainer, new CheckItemSetting(Options).SetTargetCheckState(CheckState.Marked));

            UpdatedHintObjects.AddRange(UpdatedHints);
            UpdatedLogicObjects.AddRange(UpdatedHintedLocations);

            if (UpdatedExits.Any() && Options.TargetCheckState == MiscData.CheckState.Checked)
            {
                Dictionary<EntranceRandoExit, EntranceRandoDestination> PairedExits = GetEntrancePairsToUpdate(UpdatedExits, instanceContainer);
                foreach (var i in PairedExits) { if (i.Key.GetDestinationAtExit() is null) { i.Key.DestinationExit = i.Value; } }
                IEnumerable<EntranceRandoExit> PairedExitsToMark = PairedExits.Keys.Where(x => !x.Available);
                IEnumerable<EntranceRandoExit> PairedExitsToCheck = PairedExits.Keys.Where(x => x.Available);
                List<EntranceRandoExit> UpdatedMarkedPairedExits = SetEntrancesCheckState(PairedExitsToMark, instanceContainer, Options.Copy().SetTargetCheckState(CheckState.Marked).SetEnforceMarkAction(true));
                List<EntranceRandoExit> UpdatedCheckedPairedExits = SetEntrancesCheckState(PairedExitsToCheck, instanceContainer, Options.Copy().SetTargetCheckState(CheckState.Checked).SetEnforceMarkAction(true));
                if (UpdatedCheckedPairedExits.Any())
                {
                    instanceContainer.logicCalculation.CalculateLogic(Options.TargetCheckState);
                }
                UpdatedLogicObjects.AddRange(UpdatedMarkedPairedExits);
                UpdatedLogicObjects.AddRange(UpdatedCheckedPairedExits);
            }

            List<object> AllUpdatedObjects = [.. UpdatedLogicObjects, .. UpdatedHintObjects, .. UpdatedSettingObjects];

            if (UpdatedLogicObjects.Count != 0) { TriggerCheckStateChangedEvent(UpdatedLogicObjects, instanceContainer.Instance); }
            if (UpdatedSettingObjects.Count != 0) { TriggerUserOptionUpdatedEvent(UpdatedLogicObjects, instanceContainer.Instance); }

            return AllUpdatedObjects;
        }

        public static List<object> SetOptionCheckState(IEnumerable<object> SelectedObjects, InstanceData.InstanceContainer instanceContainer, CheckItemSetting Options)
        {
            List<object> UpdatedObjects = new List<object>();

            //Handle Options
            IEnumerable<OptionData.ChoiceOption> choiceOptions = SelectedObjects.OfType<OptionData.ChoiceOption>();
            IEnumerable<OptionData.IntOption> IntOptions = SelectedObjects.OfType<OptionData.IntOption>();
            IEnumerable<OptionData.ToggleOption> ToggleOptions = SelectedObjects.OfType<OptionData.ToggleOption>();
            IEnumerable<OptionData.MultiSelectValueListDisplay> MultiSelectOptions = SelectedObjects.OfType<OptionData.MultiSelectValueListDisplay>();

            foreach (var i in ToggleOptions)
            {
                i.ToggleValue();
            }
            foreach (var i in MultiSelectOptions)
            {
                i.Parent.ToggleValue(i.Value.ID);
            }
            if (choiceOptions.Any())
            {
                var Result = Options.CheckChoiceOptions(choiceOptions, instanceContainer);
                foreach (var O in Result) { O.GetChoiceOption().ChoiceOption.SetValue(O.GetChoiceOption().Val); }
            }
            if (IntOptions.Any())
            {
                var Result = Options.CheckIntOptions(IntOptions, instanceContainer);
                foreach (var O in Result) { O.GetIntOption().IntOption.SetValue(O.GetIntOption().Val); }
            }

            UpdatedObjects.AddRange(choiceOptions);
            UpdatedObjects.AddRange(IntOptions);
            UpdatedObjects.AddRange(ToggleOptions);

            if (UpdatedObjects.Any()) { instanceContainer.logicCalculation.CompileOptionActionEdits(); }

            return UpdatedObjects;
        }

        public static List<LocationObject> SetLocationsCheckState(IEnumerable<object> SelectedObjects, InstanceData.InstanceContainer instanceContainer, CheckItemSetting Options)
        {
            List<LocationObject> UpdatedObjects = new List<LocationObject>();

            //Handle Locations
            IEnumerable<LocationObject> locationObjects = SelectedObjects.OfType<LocationObject>();
            locationObjects = locationObjects.Concat(SelectedObjects.OfType<LocationProxy>().Select(x => x.GetReferenceLocation()));
            locationObjects = locationObjects.Distinct();
            //If we are performing an uncheck action there should be no unchecked locations in the list and even if there are nothing will be done to them anyway
            //This check is neccessary for the "UnMark Only" action.
            IEnumerable<LocationObject> UncheckedlocationObjects = (Options.TargetCheckState == MiscData.CheckState.Unchecked) ?
                new List<LocationObject>() :
                locationObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);

            foreach (LocationObject LocationObject in UncheckedlocationObjects)
            {
                LocationObject.Randomizeditem.Item = LocationObject.GetItemAtCheck();
            }
            //Get Entries that need a value manually assigned and pass them to given method to be assigned.
            IEnumerable<LocationObject> ManualLocationChecks = UncheckedlocationObjects.Where(x => x.Randomizeditem.Item == null); //Locations with no item
            if (ManualLocationChecks.Any())
            {
                var Result = Options.CheckUnassignedLocations(ManualLocationChecks, instanceContainer);
                foreach (var O in Result) { O.GetItemLocation().Check.Randomizeditem.Item = O.GetItemLocation().ItemData.ItemID; O.GetItemLocation().Check.Randomizeditem.OwningPlayer = O.GetItemLocation().ItemData.Player; }
            }
            foreach (LocationObject LocationObject in locationObjects)
            {
                //When we mark a location, the action is always sent as Marked, but if the location is already marked we should instead Unchecked it unless EnforceMarkAction is true.
                var Action = (Options.TargetCheckState == MiscData.CheckState.Marked && LocationObject.CheckState == MiscData.CheckState.Marked) && !Options.EnforceMarkAction ? MiscData.CheckState.Unchecked : Options.TargetCheckState;
                if (LocationObject.ToggleChecked(Action))
                {
                    UpdatedObjects.Add(LocationObject);
                }
            }

            return UpdatedObjects;
        }

        public static List<EntranceRandoExit> SetEntrancesCheckState(IEnumerable<object> SelectedObjects, InstanceData.InstanceContainer instanceContainer, CheckItemSetting Options)
        {
            List<EntranceRandoExit> UpdatedObjects = new List<EntranceRandoExit>();

            //Handle Exits
            IEnumerable<EntranceRandoExit> ExitObjects = SelectedObjects.OfType<EntranceRandoExit>();
            IEnumerable<EntranceRandoExit> UncheckedExitObjects = (Options.TargetCheckState == MiscData.CheckState.Unchecked) ?
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
                foreach (var O in Result) { O.GetExitDestination().Exit.DestinationExit = O.GetExitDestination().Destination; }
            }
            foreach (EntranceRandoExit ExitObject in ExitObjects)
            {
                var Action = (Options.TargetCheckState == MiscData.CheckState.Marked && ExitObject.CheckState == MiscData.CheckState.Marked) && !Options.EnforceMarkAction ? MiscData.CheckState.Unchecked : Options.TargetCheckState;
                if (ExitObject.ToggleExitChecked(Action))
                {
                    UpdatedObjects.Add(ExitObject);
                }
            }
            return UpdatedObjects;
        }

        public static List<HintObject> SetHintsCheckState(IEnumerable<object> SelectedObjects, InstanceData.InstanceContainer instanceContainer, CheckItemSetting Options)
        {
            List<HintObject> UpdatedObjects = new List<HintObject>();
            //Hints======================================
            List<HintObject> HintObjects = SelectedObjects.OfType<HintObject>().ToList();

            var UncheckedHintObjects = HintObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);
            foreach (var i in UncheckedHintObjects.Where(x => !string.IsNullOrWhiteSpace(x.SpoilerHintText)))
            {
                i.HintText = i.SpoilerHintText;
            }
            IEnumerable<HintObject> UncheckedVariableObjects = UncheckedHintObjects.Where(x => string.IsNullOrWhiteSpace(x.HintText));
            if (UncheckedVariableObjects.Any())
            {
                var Result = Options.CheckUnassignedHints(UncheckedVariableObjects, instanceContainer);
                foreach (var O in Result) { O.GetGossipHint().HintCheck.HintText = O.GetGossipHint().HintText; }
            }
            foreach (HintObject hintObject in HintObjects)
            {
                UpdatedObjects.Add(hintObject);
                var CheckAction = (Options.TargetCheckState == MiscData.CheckState.Marked && hintObject.CheckState == MiscData.CheckState.Marked) && !Options.EnforceMarkAction ? MiscData.CheckState.Unchecked : Options.TargetCheckState;
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
                    string ProperCheckItem = Location.GetItemAtCheck();
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
                if (!exit.IsRandomizableEntrance() || exit.EntrancePair is null) { continue; }
                //Get the Pair of the exits destination
                Debug.WriteLine(exit.DestinationExit.ToFormattedJson());
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
    }
}
