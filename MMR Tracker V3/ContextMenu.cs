using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3;
using MathNet.Symbolics;

namespace MMR_Tracker_V3
{
    public class ContextMenu()
    {
        public class ContextMenuItem(LocationListContextMenu Parent)
        {
            public string Display;
            public Func<bool> Condition;
            public Action Action;
        }
        public class SelectedItemGroupings()
        {
            public List<CheckableLocation> CheckableLocations = [];
            public List<CheckableLocation> CheckableLocationsProxyAsLocation = [];
            public List<CheckableLocation> CheckableLocationsProxyAsLogicRef = [];
            public List<CheckableLocation> PricedLocations = [];
            public List<CheckableLocation> CheckedLocations = [];
            public List<CheckableLocation> MarkedLocations = [];
            public List<CheckableLocation> NotCheckedLocations = [];
            public List<CheckableLocation> NotMarkedLocations = [];
            public List<Areaheader> AreaHeaders = [];
            public List<Areaheader> NavigatableAreas = [];
            public List<OptionData.ChoiceOption> ChoiceOptions = [];
            public List<OptionData.ToggleOption> ToggleOptions = [];
            public List<OptionData.MultiSelectOption> MultiOptions = [];
            public List<OptionData.IntOption> IntOptions = [];
            public List<OptionData.LogicOption> AllOptions = [];
        }
        public class LocationListContextMenu
        {
            public InstanceContainer ParentInstanceContainer;
            public SelectedItemGroupings ItemGroupings = new SelectedItemGroupings();
            Dictionary<string, ContextMenuItem> Items = [];
            public LocationListContextMenu(InstanceContainer InstanceContainer, IEnumerable<object> SelectedItems)
            {
                ParentInstanceContainer = InstanceContainer;
                Initialize(SelectedItems, InstanceContainer, ItemGroupings);
            }

            public void AddNewMenuItem(string Key, ContextMenuItem Item) => Items.Add(Key, Item);
            public Dictionary<string, ContextMenuItem> GetMenu() { return Items; }

            private void Initialize(IEnumerable<object> _SelectedItems, InstanceContainer IC, SelectedItemGroupings ItemGroupings)
            {
                ItemGroupings.AreaHeaders = _SelectedItems.Where(x => x is Areaheader).Cast<Areaheader>().ToList();
                ItemGroupings.NavigatableAreas = ItemGroupings.AreaHeaders.Where(x => IC.Instance.AreaPool.ContainsKey(x.Area)).ToList();
                ItemGroupings.CheckableLocations = _SelectedItems.OfType<CheckableLocation>().ToList();
                ItemGroupings.CheckableLocationsProxyAsLocation = ItemGroupings.CheckableLocations.Select(x => x is LocationData.LocationProxy PRO ? PRO.GetReferenceLocation() : x).ToList();
                ItemGroupings.CheckableLocationsProxyAsLogicRef = ItemGroupings.CheckableLocations.Select(x => x is LocationData.LocationProxy PRO ? PRO.GetLogicInheritance() : x).ToList();
                ItemGroupings.CheckedLocations = ItemGroupings.CheckableLocations.Where(x => x.CheckState == MiscData.CheckState.Checked).ToList();
                ItemGroupings.MarkedLocations = ItemGroupings.CheckableLocations.Where(x => x.CheckState == MiscData.CheckState.Marked).ToList();
                ItemGroupings.NotCheckedLocations = ItemGroupings.CheckableLocations.Where(x => x.CheckState != MiscData.CheckState.Checked).ToList();
                ItemGroupings.NotMarkedLocations = ItemGroupings.CheckableLocations.Where(x => x.CheckState != MiscData.CheckState.Marked).ToList();
                ItemGroupings.PricedLocations = ItemGroupings.CheckableLocations.Where(x => x.hasPrice()).ToList();

                ItemGroupings.AllOptions = _SelectedItems.OfType<OptionData.LogicOption>().ToList();
                ItemGroupings.ChoiceOptions = _SelectedItems.OfType<OptionData.ChoiceOption>().ToList();
                ItemGroupings.ToggleOptions = _SelectedItems.OfType<OptionData.ToggleOption>().ToList();
                ItemGroupings.MultiOptions = _SelectedItems.OfType<OptionData.MultiSelectOption>().ToList();
                ItemGroupings.IntOptions = _SelectedItems.OfType<OptionData.IntOption>().ToList();
            }
        }
        public class DefaultActionBuilder(LocationListContextMenu Parent)
        {
            public void AddBasic(string ID, Action action, Func<bool> Condition)
            {
                Parent.AddNewMenuItem(ID, new ContextMenuItem(Parent)
                {
                    Action = action,
                    Display = ID,
                    Condition = Condition
                });
            }
            public DefaultActionBuilder AddRefreshAction(Action action)
            {
                AddBasic("Refresh", action, () => { return true; });
                return this;
            }
            public DefaultActionBuilder AddNavigateToAction(Action action)
            {
                AddBasic("Navigate To this area", action, () => { return 
                    Parent.ItemGroupings.NavigatableAreas.Count == 1 && 
                    Parent.ParentInstanceContainer.Instance.GetAllRandomizableExits().Count > 0; });
                return this;
            }
            public DefaultActionBuilder AddFilterAreasAction(Action action)
            {
                AddBasic("Filter Areas", action, () => { return Parent.ItemGroupings.AreaHeaders.Count > 0; });
                return this;
            }
            public DefaultActionBuilder AddMinimizeHeaderAction(Action ListRefreshAction, DisplayListType displayListType)
            {
                AddBasic("Minimize Areas", () => {
                    foreach (var item in Parent.ItemGroupings.AreaHeaders)
                    {
                        item.SetMinimized(displayListType, Parent.ParentInstanceContainer.Instance.StaticOptions, true);
                    }
                    ListRefreshAction?.Invoke();
                }, 
                () => { return
                    Parent.ItemGroupings.AreaHeaders.Any(x => 
                        !x.IsMinimized(displayListType, Parent.ParentInstanceContainer.Instance.StaticOptions)); });
                return this;
            }
            public DefaultActionBuilder AddMaximizeHeaderAction(Action ListRefreshAction, DisplayListType displayListType)
            {
                AddBasic("Maximize Areas", () => {
                    foreach (var item in Parent.ItemGroupings.AreaHeaders)
                    {
                        item.SetMinimized(displayListType, Parent.ParentInstanceContainer.Instance.StaticOptions, false);
                    }
                    ListRefreshAction?.Invoke();
                }, () => {
                    return
                    Parent.ItemGroupings.AreaHeaders.Any(x =>
                        x.IsMinimized(displayListType, Parent.ParentInstanceContainer.Instance.StaticOptions));
                });
                return this;
            }
            public DefaultActionBuilder AddStarAction(Action ListRefreshAction)
            {
                AddBasic("Star locations", () =>
                {
                    foreach (var item in Parent.ItemGroupings.CheckableLocations) { item.Starred = true; }
                    ListRefreshAction?.Invoke();
                }, () => { return Parent.ItemGroupings.CheckableLocations.Any(x => !x.Starred); });
                return this;
            }
            public DefaultActionBuilder AddUnstarAction(Action ListRefreshAction)
            {
                AddBasic("Unstar Locations", () =>
                {
                    foreach (var item in Parent.ItemGroupings.CheckableLocations) { item.Starred = false; }
                    ListRefreshAction?.Invoke();
                }, () => { return Parent.ItemGroupings.CheckableLocations.Any(x => x.Starred); });
                return this;
            }
            public DefaultActionBuilder AddHideAction(Action ListRefreshAction)
            {
                AddBasic("Hide locations", () =>
                {
                    foreach (var item in Parent.ItemGroupings.CheckableLocations) { item.Hidden = true; }
                    ListRefreshAction?.Invoke();
                }, () => { return Parent.ItemGroupings.CheckableLocations.Any(x => !x.Hidden); });
                return this;
            }
            public DefaultActionBuilder AddUnHideAction(Action ListRefreshAction)
            {
                AddBasic("UnHide Locations", () =>
                {
                    foreach (var item in Parent.ItemGroupings.CheckableLocations) { item.Hidden = false; }
                    ListRefreshAction?.Invoke();
                }, () => { return Parent.ItemGroupings.CheckableLocations.Any(x => x.Hidden); });
                return this;
            }
            public DefaultActionBuilder AddShowLogicAction(Action action)
            {
                AddBasic("Show Logic", action, () => { return 
                    Parent.ItemGroupings.CheckableLocationsProxyAsLogicRef.Count == 1 &&
                    Parent.ParentInstanceContainer.Instance.GetLogic(Parent.ItemGroupings.CheckableLocationsProxyAsLogicRef[0].ID) is not null; });
                return this;
            }
            public DefaultActionBuilder AddWhatUnlockedAction(Action action)
            {
                AddBasic("What Unlocked This?", action, () => { return
                    Parent.ItemGroupings.CheckableLocationsProxyAsLogicRef.Count == 1 &&
                    Parent.ParentInstanceContainer.logicCalculation.UnlockData.ContainsKey(Parent.ItemGroupings.CheckableLocationsProxyAsLogicRef[0].ID);
                });
                return this;
            }
            public DefaultActionBuilder AddCheckUnCheckedAction(Action action)
            {
                AddBasic("Check Selected Locations", action, () => { return Parent.ItemGroupings.NotCheckedLocations.Count > 0; });
                return this;
            }
            public DefaultActionBuilder AddUnCheckCheckedAction(Action action)
            {
                AddBasic("UnCheck Selected Locations", action, () => { return Parent.ItemGroupings.CheckedLocations.Count > 0; });
                return this;
            }
            public DefaultActionBuilder AddMarkUnMarkedAction(Action action)
            {
                AddBasic("Mark Selected Locations", action, () => { return Parent.ItemGroupings.NotMarkedLocations.Count > 0; });
                return this;
            }
            public DefaultActionBuilder AddUnMarkMarkedAction(Action action)
            {
                AddBasic("UnMark Selected Locations", action, () => { return Parent.ItemGroupings.MarkedLocations.Count > 0; });
                return this;
            }
            public DefaultActionBuilder AddEditPriceAction(Func<IEnumerable<ManualCheckObjectResult>> PriceSetFunction, Action ListRefreshAction)
            {
                AddBasic("Edit Price", () =>
                {
                    var PriceData = PriceSetFunction();
                    foreach (var i in PriceData) { i.GetPricedLocation().Location.SetPrice(i.GetPricedLocation().Price); }
                    Parent.ParentInstanceContainer.logicCalculation.CompileOptionActionEdits();
                    Parent.ParentInstanceContainer.logicCalculation.CalculateLogic();
                    ListRefreshAction?.Invoke();
                }, () => { return Parent.ItemGroupings.CheckableLocations.Count > 0; });
                return this;
            }
            public DefaultActionBuilder AddClearPriceAction(Action ListRefreshAction)
            {
                AddBasic("Clear Price", () =>
                {
                    foreach (var location in Parent.ItemGroupings.PricedLocations) { location.SetPrice(-1); }
                    Parent.ParentInstanceContainer.logicCalculation.CompileOptionActionEdits();
                    Parent.ParentInstanceContainer.logicCalculation.CalculateLogic();
                    ListRefreshAction?.Invoke();
                }, () => { return Parent.ItemGroupings.PricedLocations.Count > 0; });
                return this;
            }
            public DefaultActionBuilder AddItemAtCheckAction(Func<string> SelectItemFunc, Action<bool, string, string> DisplayResultFunc)
            {
                AddBasic("Is Item At Check?", () =>
                {
                    var Location = Parent.ItemGroupings.CheckableLocationsProxyAsLocation.First() as LocationData.LocationObject;
                    var ItemID = SelectItemFunc();
                    if (ItemID is null) { return; }
                    var Item = Parent.ParentInstanceContainer.Instance.GetItemByID(ItemID);
                    var ItemAtCheck = Parent.ParentInstanceContainer.Instance.GetItemByID(Location.GetItemAtCheck());
                    bool ItemFound = Item.GetDictEntry().GetName() == ItemAtCheck.GetDictEntry().GetName();
                    DisplayResultFunc(ItemFound, Location.GetName(), Item.GetDictEntry().Name);
                }, () => {
                    return
                    Parent.ItemGroupings.CheckableLocationsProxyAsLocation.Count == 1 &&
                    Parent.ItemGroupings.CheckableLocationsProxyAsLocation.First() is LocationData.LocationObject LO1 &&
                    LO1.CheckState == MiscData.CheckState.Unchecked &&
                    Parent.ParentInstanceContainer.Instance.GetItemByID(LO1.GetItemAtCheck()) is not null; });
                return this;
            }
            public DefaultActionBuilder AddItemInAreaAction(Func<string> SelectItemFunc, Action<bool, string, string> DisplayResultFunc)
            {
                AddBasic("Is Item In Area?", () =>
                {
                    var AH1 = Parent.ItemGroupings.AreaHeaders.First();
                    var ItemID = SelectItemFunc();
                    if (ItemID is null) { return; }
                    var Item = Parent.ParentInstanceContainer.Instance.GetItemByID(ItemID);
                    bool ItemFound = false;
                    foreach (var loc in Parent.ParentInstanceContainer.Instance.LocationPool.Values)
                    {
                        var ItemAtCheck = Parent.ParentInstanceContainer.Instance.GetItemByID(loc.GetItemAtCheck());
                        if (ItemAtCheck is null) { continue; }
                        if (loc.GetDictEntry().Area == AH1.Area && Item.GetDictEntry().GetName() == ItemAtCheck.GetDictEntry().GetName()) { ItemFound = true; }
                    }
                    DisplayResultFunc(ItemFound, AH1.Area, Item.GetDictEntry().GetName());
                }, () => { return Parent.ItemGroupings.AreaHeaders.Count == 1; });
                return this;
            }
        }
    }
}
