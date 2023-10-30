using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static MMR_Tracker_V3.TrackerObjects.EntranceData;
using static MMR_Tracker_V3.TrackerObjects.HintData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public class TrackerDataHandeling
    {

        public static event Action<List<object>, InstanceData.TrackerInstance> CheckedObjectsUpdate;
        public static void TriggerCheckedObjectsUpdate(List<object> objs, InstanceData.TrackerInstance instance) { CheckedObjectsUpdate(objs, instance); }
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


            if (UpdatedObjects.Any()) { CheckedObjectsUpdate(UpdatedObjects, instanceContainer.Instance); }
            return UpdatedObjects;
        }

        public static List<object> SetOptionCheckState(IEnumerable<object> SelectedObjects, MiscData.InstanceContainer instanceContainer, CheckItemSetting Options)
        {
            List<object> UpdatedObjects = new List<object>();

            //Handle Options
            IEnumerable<OptionData.ChoiceOption> choiceOptions = SelectedObjects.Where(x => x is OptionData.ChoiceOption).Select(x => x as OptionData.ChoiceOption);
            IEnumerable<OptionData.IntOption> IntOptions = SelectedObjects.Where(x => x is OptionData.IntOption).Select(x => x as OptionData.IntOption);
            IEnumerable<OptionData.ToggleOption> ToggleOptions = SelectedObjects.Where(x => x is OptionData.ToggleOption).Select(x => x as OptionData.ToggleOption);

            foreach (var i in ToggleOptions)
            {
                i.ToggleValue();
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
                    if (string.IsNullOrWhiteSpace(ProperCheckItem) || ProperCheckItem != Item.Id) { continue; }
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

            foreach(var i in instance.LocationPool.Values)
            {
                switch (i.CheckState)
                {
                    case CheckState.Checked:
                        dataSets.LocationStateIsChecked.Add(i);
                        break;
                    case CheckState.Marked:
                        dataSets.LocationStateIsMarked.Add(i);
                        dataSets.LocationStateIsNOTChecked.Add(i);
                        dataSets.LocationISMarkedOrISAvailableAndUnchecked.Add(i);
                        break;
                    case CheckState.Unchecked:
                        dataSets.LocationStateIsUnchecked.Add(i);
                        dataSets.LocationStateIsNOTChecked.Add(i);
                        switch (i.Available) { case true: dataSets.LocationISMarkedOrISAvailableAndUnchecked.Add(i); break; }
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
                        break;
                    case MiscData.CheckState.Marked:
                        dataSets.ProxyStateIsNOTChecked.Add(i);
                        dataSets.ProxyStateIsMarked.Add(i);
                        dataSets.ProxyISMarkedOrISAvailableAndUnchecked.Add(i);
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
                        dataSets.ExitISMarkedOrISAvailableAndUnchecked.Add(i);
                        break;
                    case CheckState.Unchecked:
                        dataSets.ExitStateIsUnchecked.Add(i);
                        dataSets.ExitStateIsNOTChecked.Add(i);
                        switch (i.Available) { case true: dataSets.ExitISMarkedOrISAvailableAndUnchecked.Add(i); break; }
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
                        dataSets.HIntISMarkedOrISAvailableAndUnchecked.Add(i);
                        break;
                    case CheckState.Unchecked:
                        dataSets.HintStateIsUnchecked.Add(i);
                        dataSets.HintStateIsNOTChecked.Add(i);
                        switch (i.Available) { case true: dataSets.HIntISMarkedOrISAvailableAndUnchecked.Add(i); break; }
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

        public static List<object> PopulateCheckedLocationList(DataSets DataSets, MiscData.Divider Divider, MiscData.InstanceContainer IC, string Filter, out int OutItemsInListBox, out int OutItemsInListBoxFiltered, bool reverse = false)
        {
            var Groups = Utility.GetCategoriesFromFile(IC.Instance);

            List<object> DataSource = new List<object>();

            var CheckedLocations = DataSets.LocationStateIsChecked;
            CheckedLocations = CheckedLocations
                .OrderBy(x => (Groups.ContainsKey(x.GetDictEntry(IC.Instance).Area.ToLower().Trim()) ? Groups[x.GetDictEntry(IC.Instance).Area.ToLower().Trim()] : DataSets.LocationStateIsChecked.Count + 1))
                .ThenBy(x => x.GetDictEntry(IC.Instance).Area)
                .ThenBy(x => Utility.GetLocationDisplayName(x, IC)).ToList();

            var ItemsInListBox = 0;
            var ItemsInListBoxFiltered = 0;

            if (reverse)
            {
                CheckedLocations.Reverse();
                WriteStartingAndOnlineItems();
                WriteOptions();
                WriteHints();
                WriteEntrances();
                WriteLocations();
            }
            else
            {
                WriteLocations();
                WriteEntrances();
                WriteHints();
                WriteOptions();
                WriteStartingAndOnlineItems();
            }


            OutItemsInListBox = ItemsInListBox;
            OutItemsInListBoxFiltered = ItemsInListBoxFiltered;
            return DataSource;

            void WriteOptions()
            {
                if (IC.Instance.StaticOptions.ShowOptionsInListBox == null || IC.Instance.StaticOptions.ShowOptionsInListBox != OptionData.DisplayListBoxes[2]) { return; }

                List<dynamic> ChoiceOptions = IC.Instance.ChoiceOptions.Values.Where(x => x.ValueList.Count > 1).Cast<dynamic>().ToList();
                List<dynamic> ToggleOptions = IC.Instance.ToggleOptions.Values.Cast<dynamic>().ToList();
                List<dynamic> IntOptions = IC.Instance.IntOptions.Values.Cast<dynamic>().ToList();
                List<dynamic> All = ChoiceOptions.Concat(ToggleOptions).Concat(IntOptions).OrderBy(x => x.Priority).ToList();

                Dictionary<string, List<dynamic>> Categorized = new Dictionary<string, List<dynamic>>();
                foreach (var item in All)
                {
                    if (!IC.logicCalculation.ConditionalsMet(item.Conditionals, new List<string>())) { continue; }
                    string Sub = item.SubCategory;
                    Sub ??= "";
                    if (!Categorized.ContainsKey(Sub)) { Categorized.Add(Sub, new List<dynamic>()); }
                    Categorized[Sub].Add(item);
                }

                string CurrentCategory = null;
                foreach(var i in Categorized)
                {
                    foreach(var c in i.Value)
                    {
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, c, Filter, c.ToString())) { continue; }
                        if (CurrentCategory is null || CurrentCategory != i.Key)
                        {
                            if (DataSource.Count > 0) { DataSource.Add(Divider); }
                            DataSource.Add(new MiscData.Areaheader { Area = i.Key == "" ? "Options" : $"Options: {i.Key}" });
                            CurrentCategory = i.Key;
                        }
                        ItemsInListBoxFiltered++;
                        DataSource.Add(c);
                    }
                }
            }

            void WriteLocations()
            {
                string CurrentLocation = "";
                foreach (var i in CheckedLocations)
                {
                    if (i.IsUnrandomized(MiscData.UnrandState.Unrand) && !Filter.StartsWith("^")) { continue; }
                    i.DisplayName = Utility.GetLocationDisplayName(i, IC);

                    ItemsInListBox++;
                    if (!SearchStringParser.FilterSearch(IC.Instance, i, Filter, i.DisplayName)) { continue; }
                    ItemsInListBoxFiltered++;
                    if (CurrentLocation != i.GetDictEntry(IC.Instance).Area)
                    {
                        if (DataSource.Count > 0) { DataSource.Add(Divider); }
                        DataSource.Add(new MiscData.Areaheader { Area = i.GetDictEntry(IC.Instance).Area });
                        CurrentLocation = i.GetDictEntry(IC.Instance).Area;
                    }
                    DataSource.Add(i);
                }
            }

            void WriteEntrances()
            {
                List<EntranceData.EntranceRandoExit> ValidExits = new List<EntranceData.EntranceRandoExit>();
                foreach (var area in IC.Instance.EntrancePool.AreaList)
                {
                    var CheckLoadingZoneExits = area.Value.RandomizableExits(IC.Instance).Where(x => x.Value.CheckState == MiscData.CheckState.Checked && EntranceAppearsinListbox(x.Value, IC.Instance));
                    var FilteredCheckedExits = CheckLoadingZoneExits.Where(x => SearchStringParser.FilterSearch(IC.Instance, x.Value, Filter, x.Value.GetEntranceDisplayName(IC.Instance)));

                    ItemsInListBox += CheckLoadingZoneExits.Count();
                    ItemsInListBoxFiltered += FilteredCheckedExits.Count();
                    if (!FilteredCheckedExits.Any()) { continue; }
                    foreach (var i in FilteredCheckedExits)
                    {
                        i.Value.DisplayName = i.Value.GetEntranceDisplayName(IC.Instance);
                        ValidExits.Add(i.Value);
                    }
                }
                ValidExits = ValidExits.OrderBy(x => x.DisplayArea(IC.Instance)).ThenBy(x => x.DisplayName).ToList();
                string CurrentArea = "";
                foreach (var i in ValidExits)
                {
                    string ItemArea =  $"{i.DisplayArea(IC.Instance)} Exits";
                    if (CurrentArea != ItemArea)
                    {
                        CurrentArea = ItemArea;
                        if (DataSource.Count > 0) { DataSource.Add(Divider); }
                        DataSource.Add(new MiscData.Areaheader { Area = CurrentArea });
                    }
                    DataSource.Add(i);
                }
            }

            void WriteHints()
            {
                if (DataSets.HintStateIsChecked.Any())
                {
                    bool DividerCreated = false;
                    foreach (var i in DataSets.HintStateIsChecked)
                    {
                        i.DisplayName = $"{i.GetDictEntry(IC.Instance).Name}: {i.HintText}";
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, i, Filter, i.DisplayName)) { continue; }
                        ItemsInListBoxFiltered++;
                        if (!DividerCreated)
                        {
                            if (DataSource.Count > 0) { DataSource.Add(Divider); }
                            DataSource.Add("HINTS:");
                            DividerCreated = true;
                        }
                        DataSource.Add(i);
                    }
                }
            }

            void WriteStartingAndOnlineItems()
            {
                if (DataSets.CurrentStartingItems.Any())
                {
                    bool DividerCreated = false;
                    foreach (var i in DataSets.CurrentStartingItems)
                    {
                        string Display = $"{i.GetDictEntry(IC.Instance).GetName(IC.Instance)} X{i.AmountInStartingpool}";
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, i, Filter, Display)) { continue; }
                        if (!DividerCreated)
                        {
                            if (DataSource.Count > 0) { DataSource.Add(Divider); }
                            DataSource.Add(new MiscData.Areaheader { Area = "Starting Items" });
                            DividerCreated = true;
                        }
                        ItemsInListBoxFiltered++;
                        DataSource.Add(Display);
                    }
                }

                if (DataSets.OnlineObtainedItems.Any())
                {
                    bool DividerCreated = false;
                    foreach (var i in DataSets.OnlineObtainedItems)
                    {
                        foreach (var j in i.AmountAquiredOnline)
                        {
                            string Display = $"{i.GetDictEntry(IC.Instance).GetName(IC.Instance)} X{j.Value}: Player {j.Key}";
                            ItemsInListBox++;
                            if (!SearchStringParser.FilterSearch(IC.Instance, i, Filter, Display)) { continue; }
                            if (!DividerCreated)
                            {
                                if (DataSource.Count > 0) { DataSource.Add(Divider); }
                                DataSource.Add(new MiscData.Areaheader { Area = "MultiWorld Items" });
                                DividerCreated = true;
                            }
                            ItemsInListBoxFiltered++;
                            DataSource.Add(Display);
                        }
                    }
                }
            }
        }

        public static string GetLocationEntryArea(object Entry, InstanceData.TrackerInstance Instance)
        {
            if (Entry is LocationData.LocationObject l) { return l.GetDictEntry(Instance).Area; }
            else if (Entry is LocationData.LocationProxy p) { return p.Area; }
            return "Error";
        }
        public static bool GetLocationEntryAvailablility(object Entry, InstanceData.TrackerInstance Instance)
        {
            if (Entry is LocationData.LocationObject l) { return l.Available; }
            else if (Entry is LocationData.LocationProxy p) { return p.ProxyAvailable(Instance); }
            return false;
        }

        public static List<object> PopulateAvailableLocationList(DataSets DataSets, MiscData.Divider Divider, MiscData.InstanceContainer IC, string Filter, bool ShowUnavailable, out int OutItemsInListBox, out int OutItemsInListBoxFiltered, bool reverse = false)
        {
            bool ShowAllLocation = ShowUnavailable || (Filter.StartsWith("^") && !Filter.StartsWith("^^")) || Filter.StartsWith("^^^");
            bool ShowInvalidLocation = Filter.StartsWith("^^");

            var Groups = Utility.GetCategoriesFromFile(IC.Instance);
            List<object> DataSource = new List<object>();

            var AvailableProxies = DataSets.ProxyISMarkedOrISAvailableAndUnchecked;
            if (ShowAllLocation) { AvailableProxies = DataSets.ProxyStateIsNOTChecked; }

            var AvailableLocations = DataSets.LocationISMarkedOrISAvailableAndUnchecked;
            if (ShowAllLocation) { AvailableLocations = DataSets.LocationStateIsNOTChecked; }

            IEnumerable<object> AvailableLocationsEntries = AvailableLocations.Where(x => !IC.Instance.LocationProxyData.LocationsWithProxys.ContainsKey(x.ID));
            AvailableLocationsEntries = AvailableLocationsEntries.Concat(AvailableProxies);
            AvailableLocationsEntries = AvailableLocationsEntries.OrderByDescending(x => GetLocationEntryAvailablility(x, IC.Instance))
                .ThenBy(x => (Groups.ContainsKey(GetLocationEntryArea(x, IC.Instance).ToLower().Trim()) ? Groups[GetLocationEntryArea(x, IC.Instance).ToLower().Trim()] : DataSets.LocationISMarkedOrISAvailableAndUnchecked.Count() + 1))
                .ThenBy(x => GetLocationEntryArea(x, IC.Instance))
                .ThenBy(x => Utility.GetLocationDisplayName(x, IC)).ToList();

            IEnumerable<object> HiddenLocations = AvailableLocationsEntries.Where(x => Utility.DynamicPropertyExist(x, "Hidden") && (x as dynamic).Hidden).OrderBy(x => Utility.GetLocationDisplayName(x, IC));
            AvailableLocationsEntries = AvailableLocationsEntries.Where(x => !Utility.DynamicPropertyExist(x, "Hidden") || !(x as dynamic).Hidden);

            var AvailableHints = DataSets.HIntISMarkedOrISAvailableAndUnchecked;
            if (ShowAllLocation) { AvailableHints = DataSets.HintStateIsNOTChecked; }

            var ItemsInListBox = 0;
            var ItemsInListBoxFiltered = 0;

            if (reverse)
            {
                AvailableLocations.Reverse();
                WriteHiddenLocations();
                WriteOptions();
                WriteHints();
                WriteEntrances();
                WriteLocations();
            }
            else
            {
                WriteLocations();
                WriteEntrances();
                WriteHints();
                WriteOptions();
                WriteHiddenLocations();
            }

            OutItemsInListBox = ItemsInListBox;
            OutItemsInListBoxFiltered = ItemsInListBoxFiltered;
            return DataSource;

            void WriteEntrances()
            {
                if (IC.Instance.StaticOptions.OptionFile.EntranceRandoFeatures) { return; }
                var Entrances = PopulateAvailableEntraceList(DataSets, Divider, IC, Filter, ShowUnavailable, out int EntCount, out int EntCountFiltered, reverse);
                if (Entrances.Any() && DataSource.Any()) { DataSource.Add(Divider); }
                DataSource.AddRange(Entrances);
                ItemsInListBox += EntCount;
                ItemsInListBoxFiltered += EntCountFiltered;
            }

            void WriteHiddenLocations()
            {
                List<object> TempDataSource = new List<object>();
                foreach (var obj in HiddenLocations)
                {
                    var CurrentArea = "";
                    if (obj is LocationData.LocationObject i)
                    {
                        if (!i.AppearsinListbox(IC.Instance, ShowInvalidLocation)) { continue; }
                        i.DisplayName = Utility.GetLocationDisplayName(i, IC);
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, i, Filter, i.DisplayName)) { continue; }
                        ItemsInListBoxFiltered++;
                        CurrentArea = i.GetDictEntry(IC.Instance).Area;
                    }
                    else if (obj is LocationData.LocationProxy p)
                    {
                        if (!p.GetReferenceLocation(IC.Instance).AppearsinListbox(IC.Instance, ShowInvalidLocation)) { continue; }
                        p.DisplayName = Utility.GetLocationDisplayName(p, IC);
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, p, Filter, p.DisplayName)) { continue; }
                        ItemsInListBoxFiltered++;
                        CurrentArea = p.Area;
                    }
                    else { continue; }
                    TempDataSource.Add(obj);
                }
                if (TempDataSource.Any())
                {
                    if (DataSource.Count > 0) { DataSource.Add(Divider); }
                    DataSource.Add(new MiscData.Areaheader { Area = "Hidden Locations" });
                    DataSource.AddRange(TempDataSource);
                }
            }

            void WriteOptions()
            {
                if (IC.Instance.StaticOptions.ShowOptionsInListBox == null || IC.Instance.StaticOptions.ShowOptionsInListBox != OptionData.DisplayListBoxes[1]) { return; }

                List<dynamic> ChoiceOptions = IC.Instance.ChoiceOptions.Values.Where(x => x.ValueList.Count > 1).Cast<dynamic>().ToList();
                List<dynamic> ToggleOptions = IC.Instance.ToggleOptions.Values.Cast<dynamic>().ToList();
                List<dynamic> IntOptions = IC.Instance.IntOptions.Values.Cast<dynamic>().ToList();
                List<dynamic> All = ChoiceOptions.Concat(ToggleOptions).Concat(IntOptions).OrderBy(x => x.Priority).ToList();

                Dictionary<string, List<dynamic>> Categorized = new Dictionary<string, List<dynamic>>();
                foreach (var item in All)
                {
                    if (!IC.logicCalculation.ConditionalsMet(item.Conditionals, new List<string>())) { continue; }
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
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, c, Filter, c.ToString())) { continue; }
                        if (CurrentCategory is null || CurrentCategory != i.Key)
                        {
                            if (DataSource.Count > 0) { DataSource.Add(Divider); }
                            DataSource.Add(new MiscData.Areaheader { Area = i.Key == "" ? "Options" : $"Options: {i.Key}" });
                            CurrentCategory = i.Key;
                        }
                        ItemsInListBoxFiltered++;
                        DataSource.Add(c);
                    }
                }
            }

            void WriteLocations()
            {
                string CurrentLocation = "";
                foreach (var obj in AvailableLocationsEntries)
                {
                    var CurrentArea = "";
                    if (obj is LocationData.LocationObject i)
                    {
                        if (!i.AppearsinListbox(IC.Instance, ShowInvalidLocation)) { continue; }
                        i.DisplayName = Utility.GetLocationDisplayName(i, IC);
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, i, Filter, i.DisplayName)) { continue; }
                        ItemsInListBoxFiltered++;
                        CurrentArea = i.GetDictEntry(IC.Instance).Area;
                    }
                    else if (obj is LocationData.LocationProxy p)
                    {
                        if (!p.GetReferenceLocation(IC.Instance).AppearsinListbox(IC.Instance, ShowInvalidLocation)) { continue; }
                        p.DisplayName = Utility.GetLocationDisplayName(p, IC);
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, p, Filter, p.DisplayName)) { continue; }
                        ItemsInListBoxFiltered++;
                        CurrentArea = p.Area;
                    }
                    else { continue; }
                    if (CurrentLocation != CurrentArea)
                    {
                        if (DataSource.Count > 0) { DataSource.Add(Divider); }
                        DataSource.Add(new MiscData.Areaheader { Area = CurrentArea });
                        CurrentLocation = CurrentArea;
                    }
                    DataSource.Add(obj);
                }
            }

            void WriteHints()
            {
                if (AvailableHints.Any())
                {
                    bool DividerCreated = false;
                    foreach (var i in AvailableHints)
                    {
                        if (i.RandomizedState == MiscData.RandomizedState.ForcedJunk && !ShowInvalidLocation) { continue; }
                        i.DisplayName = (i.CheckState == MiscData.CheckState.Marked) ? $"{i.GetDictEntry(IC.Instance).Name}: {i.HintText}" : i.GetDictEntry(IC.Instance).Name;
                        ItemsInListBox++;
                        if (!SearchStringParser.FilterSearch(IC.Instance, i, Filter, i.DisplayName)) { continue; }
                        ItemsInListBoxFiltered++;
                        if (!DividerCreated)
                        {
                            if (DataSource.Count > 0) { DataSource.Add(Divider); }
                            DataSource.Add(new MiscData.Areaheader { Area = "HINTS" });
                            DividerCreated = true;
                        }
                        DataSource.Add(i);
                    }
                }
            }
        }

        public static List<object> PopulateAvailableEntraceList(DataSets DataSets, MiscData.Divider Divider, MiscData.InstanceContainer IC, string Filter, bool ShowUnavailable, out int OutItemsInListBox, out int OutItemsInListBoxFiltered, bool reverse = false)
        {
            bool InLocationBox = !IC.Instance.StaticOptions.OptionFile.EntranceRandoFeatures;

            var Groups = Utility.GetCategoriesFromFile(IC.Instance);
            List<object> DataSource = new List<object>();
            OutItemsInListBox = 0;
            OutItemsInListBoxFiltered = 0;

            List<EntranceData.EntranceRandoExit> ValidExits = new List<EntranceData.EntranceRandoExit>();

            foreach (var area in IC.Instance.EntrancePool.AreaList)
            {
                var AvailableExits = area.Value.RandomizableExits(IC.Instance).Where(x => 
                (x.Value.Available || x.Value.CheckState == MiscData.CheckState.Marked || ShowUnavailable || Filter.StartsWith("^")) && 
                x.Value.CheckState != MiscData.CheckState.Checked && EntranceAppearsinListbox(x.Value, IC.Instance));

                var FilteredAvailableExits = AvailableExits.Where(x => SearchStringParser.FilterSearch(IC.Instance, x.Value, Filter, x.Value.GetEntranceDisplayName(IC.Instance)));

                OutItemsInListBox += AvailableExits.Count();
                OutItemsInListBoxFiltered += FilteredAvailableExits.Count();
                if (!FilteredAvailableExits.Any()) { continue; }

                foreach(var i in FilteredAvailableExits)
                {
                    i.Value.DisplayName = i.Value.GetEntranceDisplayName(IC.Instance);
                    ValidExits.Add(i.Value);

                }
            }

            ValidExits = ValidExits.OrderByDescending(x => x.Available).ThenBy(x => x.DisplayArea(IC.Instance)).ThenBy(x => x.DisplayName).ToList();
            string CurrentArea = "";
            foreach(var i in ValidExits)
            {
                string ItemArea = InLocationBox ? $"{i.DisplayArea(IC.Instance)} Entrances" : i.DisplayArea(IC.Instance);
                if (CurrentArea != ItemArea)
                {
                    CurrentArea = ItemArea;
                    if (DataSource.Count > 0) { DataSource.Add(Divider); }
                    DataSource.Add(new MiscData.Areaheader { Area = CurrentArea });
                }
                DataSource.Add(i);
            }
            return DataSource;
        }

        private static bool EntranceAppearsinListbox(EntranceData.EntranceRandoExit Location, InstanceData.TrackerInstance Instance)
        {
            return !Location.IsJunk() && !Location.IsUnrandomized(MiscData.UnrandState.Unrand);
        }
    }
}
