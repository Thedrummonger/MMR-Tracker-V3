using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3;
using System.Diagnostics;
using Microsoft.VisualBasic.Logging;

namespace Windows_Form_Frontend
{
    internal class LocationListContextMenu(MainInterface mainInterface, ListBox listBox, InstanceContainer InstanceContainer)
    {
        ContextMenuStrip contextMenuStrip = new();
        List<object> SelectedItems = [];
        List<CheckableLocation> CheckableLocations = [];
        List<CheckableLocation> CheckableLocationsProxyAsLocation = [];
        List<CheckableLocation> CheckableLocationsProxyAsLogicRef = [];
        List<CheckableLocation> PricedLocations = [];
        List<CheckableLocation> CheckedLocations = [];
        List<CheckableLocation> MarkedLocations = [];
        List<CheckableLocation> NotCheckedLocations = [];
        List<CheckableLocation> NotMarkedLocations = [];
        List<MiscData.Areaheader> AreaHeaders = [];
        List<MiscData.Areaheader> NavigatableAreas = [];
        List<OptionData.ChoiceOption> ChoiceOptions = [];
        List<OptionData.ToggleOption> ToggleOptions = [];
        List<OptionData.MultiSelectOption> MultiOptions = [];
        List<OptionData.IntOption> IntOptions = [];
        List<OptionData.LogicOption> AllOptions = [];

        DisplayListType displayList = DisplayListType.Locations;
        TextBox TextBox = null;
        public void Show()
        {
            Initialize();
            AddItem("refresh", () => { mainInterface.PrintToListBox([listBox]); });
            if (NavigatableAreas.Count == 1 && InstanceContainer.Instance.GetAllRandomizableExits().Count > 0)
            {
                AddItem("Navigate To this area", () => { mainInterface.CMBEnd.SelectedItem = NavigatableAreas[0].Area; });
            }
            if (AreaHeaders.Count > 0)
            {
                AddItem("Filter Areas", FilterAreas) ;
                if (AreaHeaders.Any(x => !x.IsMinimized(displayList, InstanceContainer.Instance.StaticOptions)))
                {
                    AddItem("Minimize Areas", MinimizeAreas);
                }
                if (AreaHeaders.Any(x => x.IsMinimized(displayList, InstanceContainer.Instance.StaticOptions)))
                {
                    AddItem("UnMinimize Areas", UnMinimizeAreas);
                }
            }
            if (CheckableLocations.Count > 0)
            {
                if (CheckableLocations.Any(x => !x.Starred)) { AddItem("Star Locations", StarObjects); }
                if (CheckableLocations.Any(x => x.Starred)) { AddItem("UnStar Locations", UnStarObjects); }
            }
            if (CheckableLocationsProxyAsLogicRef.Count == 1 && InstanceContainer.Instance.GetLogic(CheckableLocationsProxyAsLogicRef[0].ID) is not null)
            {
                AddItem("Show Logic", () => { new ShowLogic(CheckableLocationsProxyAsLogicRef[0].ID, InstanceContainer).Show(); });
            }
            if (CheckableLocationsProxyAsLogicRef.Count == 1 && InstanceContainer.logicCalculation.UnlockData.ContainsKey(CheckableLocationsProxyAsLogicRef[0].ID))
            {
                AddItem("What Unlocked This", () => { ShowUnlockData(CheckableLocationsProxyAsLogicRef[0].ID); });
            }
            if (NotCheckedLocations.Count > 0)
            {
                AddItem("Check Selected Locations", () => { 
                    mainInterface.HandleItemSelect(NotCheckedLocations, MiscData.CheckState.Checked, LB: listBox); 
                });
            }
            if (CheckedLocations.Count > 0)
            {
                AddItem("UnCheck Selected Locations", () => {
                    mainInterface.HandleItemSelect(CheckedLocations, MiscData.CheckState.Unchecked, LB: listBox);
                });
            }
            if (NotMarkedLocations.Count > 0)
            {
                AddItem("Mark Selected Locations", () => {
                    mainInterface.HandleItemSelect(NotMarkedLocations, MiscData.CheckState.Marked, true, listBox);
                });
            }
            if (MarkedLocations.Count > 0)
            {
                AddItem("UnMark Selected Locations", () => {
                    mainInterface.HandleItemSelect(MarkedLocations, MiscData.CheckState.Unchecked, LB: listBox);
                });
            }
            if (AllOptions.Any() && AmountOptionTypesSelected() > 1)
            {
                AddItem("Edit Selected Options", () => {
                    mainInterface.HandleItemSelect(AllOptions, MiscData.CheckState.Unchecked, LB: listBox);
                });
            }
            if (ChoiceOptions.Any())
            {
                AddItem("Edit Selected Choice Options", () => {
                    mainInterface.HandleItemSelect(ChoiceOptions, MiscData.CheckState.Unchecked, LB: listBox);
                });
            }
            if (MultiOptions.Any())
            {
                AddItem("Edit Selected MultiSelect Options", () => {
                    mainInterface.HandleItemSelect(MultiOptions, MiscData.CheckState.Unchecked, LB: listBox);
                });
            }
            if (ToggleOptions.Any())
            {
                AddItem("Edit Selected Toggle Options", () => {
                    mainInterface.HandleItemSelect(ToggleOptions, MiscData.CheckState.Unchecked, LB: listBox);
                });
            }
            if (IntOptions.Any())
            {
                AddItem("Edit Selected Int Options", () => {
                    mainInterface.HandleItemSelect(IntOptions, MiscData.CheckState.Unchecked, LB: listBox);
                });
            }
            if (CheckableLocations.Any())
            {
                var PricedLocations = CheckableLocations.Select(x => new VariableInputWindow.PriceContainer(x));
                AddItem("Edit Price", () => {
                    VariableInputWindow PriceInput = new(PricedLocations, InstanceContainer);
                    PriceInput.ShowDialog();
                    foreach(var i in PriceInput._Result) { i.GetPricedLocation().Location.SetPrice(i.GetPricedLocation().Price); }
                    InstanceContainer.logicCalculation.CompileOptionActionEdits();
                    InstanceContainer.logicCalculation.CalculateLogic();
                    mainInterface.UpdateUI();
                });
            }

            if (PricedLocations.Any())
            {
                AddItem("Clear Price", () => {
                    foreach (var location in PricedLocations) { location.SetPrice(-1); }
                    InstanceContainer.logicCalculation.CompileOptionActionEdits();
                    InstanceContainer.logicCalculation.CalculateLogic();
                    mainInterface.UpdateUI();
                });
            }

            if (CheckableLocations.Any(x => !x.Hidden))
            {
                AddItem("Hide Locations", () => {
                    foreach (var location in CheckableLocations)
                    {
                        location.Hidden = true;
                    }
                    mainInterface.UpdateUI();
                });
            }
            if (CheckableLocations.Any(x => x.Hidden))
            {
                AddItem("UnHide Locations", () => {
                    foreach (var location in CheckableLocations)
                    {
                        location.Hidden = false;
                    }
                    mainInterface.UpdateUI();
                });
            }
            if (CheckableLocationsProxyAsLocation.Count == 1 && 
                CheckableLocationsProxyAsLocation.First() is LocationData.LocationObject LO1 && 
                LO1.CheckState == MiscData.CheckState.Unchecked && 
                InstanceContainer.Instance.GetItemByID(LO1.GetItemAtCheck()) is not null)
            {
                AddItem("Is Item at check?", () => {
                    CheckItemForm checkItemForm = new([LO1], InstanceContainer, false);
                    checkItemForm.ShowDialog();
                    if (checkItemForm._Result.Count == 0) { return; }
                    string ItemID = checkItemForm._Result[0].GetItemLocation().ItemData.ItemID;
                    var Item = InstanceContainer.Instance.GetItemByID(ItemID);
                    var ItemAtCheck = InstanceContainer.Instance.GetItemByID(LO1.GetItemAtCheck());
                    bool ItemFound = Item.GetDictEntry().GetName() == ItemAtCheck.GetDictEntry().GetName();
                    if (ItemFound) { MessageBox.Show($"{Item.GetDictEntry().GetName()} Can be found at {LO1.GetName()}", "Item Found", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                    else { MessageBox.Show($"{Item.GetDictEntry().GetName()} Can NOT be found at {LO1.GetName()}", "Item Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                });
            }
            if (AreaHeaders.Count == 1 && AreaHeaders.First() is MiscData.Areaheader AH1)
            {
                AddItem("Is Item in area?", () => {
                    CheckItemForm checkItemForm = new([null], InstanceContainer, false);
                    checkItemForm.ShowDialog();
                    if (checkItemForm._Result.Count == 0) { return; }
                    string ItemID = checkItemForm._Result[0].GetItemLocation().ItemData.ItemID;
                    var Item = InstanceContainer.Instance.GetItemByID(ItemID);
                    bool ItemFound = false;
                    foreach(var loc in InstanceContainer.Instance.LocationPool.Values)
                    {
                        var ItemAtCheck = InstanceContainer.Instance.GetItemByID(loc.GetItemAtCheck());
                        if (ItemAtCheck is null) { continue; }
                        if (loc.GetDictEntry().Area == AH1.Area && Item.GetDictEntry().GetName() == ItemAtCheck.GetDictEntry().GetName()) { ItemFound = true; }
                    }
                    if (ItemFound) { MessageBox.Show($"{Item.GetDictEntry().GetName()} can be found at {AH1.Area}", "Item Found", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                    else { MessageBox.Show($"{Item.GetDictEntry().GetName()} can NOT be found at {AH1.Area}", "Item Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                });
            }

            if (contextMenuStrip.Items.Count > 0)
            {
                contextMenuStrip.Show(Cursor.Position);
            }
        }

        private void ShowUnlockData(string iD)
        {
            if (!InstanceContainer.logicCalculation.UnlockData.ContainsKey(iD)) { return; }
            var AdvancedUnlockData = PlaythroughTools.GetAdvancedUnlockData(iD, InstanceContainer.logicCalculation.UnlockData, InstanceContainer.Instance);
            var DataDisplay = PlaythroughTools.FormatAdvancedUnlockData(AdvancedUnlockData, InstanceContainer);
            List<dynamic> Items = new List<dynamic>();
            foreach (var i in DataDisplay)
            {
                var FLI = new MiscData.StandardListBoxItem
                {
                    Display = i is MiscData.Divider DVIx ? DVIx.Display : i.ToString(),
                    Tag = i is MiscData.Divider DVIy ? DVIy : i.ToString(),
                    tagFunc = i is MiscData.Divider ? ShowUnlockSubFunction : null
                };
                Items.Add(FLI);
            }
            BasicDisplay basicDisplay = new BasicDisplay(Items);
            basicDisplay.Text = $"Unlock Data for {iD}";
            basicDisplay.Show();
        }

        private int AmountOptionTypesSelected()
        {
            int count = 0;
            if (ChoiceOptions.Count > 0) { count++; }
            if (IntOptions.Count > 0) { count++; }
            if (ToggleOptions.Count > 0) { count++; }
            if (MultiOptions.Count > 0) { count++; }
            return count;
        }

        private void FilterAreas()
        {
            TextBox.Text = string.Empty;
            foreach (var item in AreaHeaders)
            {
                if (string.IsNullOrWhiteSpace(TextBox.Text)) { TextBox.Text = $"#{item.Area}"; }
                else { TextBox.Text += $"|#{item.Area}"; };
            }
        }
        private void MinimizeAreas()
        {
            TextBox.Text = string.Empty;
            foreach (var item in AreaHeaders)
            {
                item.SetMinimized(displayList, InstanceContainer.Instance.StaticOptions, true);
            }
            mainInterface.PrintToListBox();
        }
        private void UnMinimizeAreas()
        {
            TextBox.Text = string.Empty;
            foreach (var item in AreaHeaders)
            {
                item.SetMinimized(displayList, InstanceContainer.Instance.StaticOptions, false);
            }
            mainInterface.PrintToListBox();
        }
        private void StarObjects()
        {
            TextBox.Text = string.Empty;
            foreach (var item in CheckableLocations) { item.Starred = true; }
            mainInterface.PrintToListBox();
        }
        private void UnStarObjects()
        {
            TextBox.Text = string.Empty;
            foreach (var item in CheckableLocations) { item.Starred = false; }
            mainInterface.PrintToListBox();
        }
        private dynamic ShowUnlockSubFunction(dynamic dynamic)
        {
            if (dynamic is not ValueTuple<List<ValueTuple<object, bool>>, object> TO || TO.Item2 is not MiscData.Divider DIV) { return null; }
            List<ValueTuple<object, bool>> Return = new();
            bool Toggleing = false;
            foreach (var i in TO.Item1)
            {
                bool IsDivider = i.Item1 is MiscData.StandardListBoxItem FLI && FLI.Tag is MiscData.Divider;
                Toggleing = IsDivider ? ((i.Item1 as MiscData.StandardListBoxItem).Tag as MiscData.Divider).Display == DIV.Display : Toggleing;
                bool Shown = (Toggleing ? !i.Item2 : i.Item2) || IsDivider;
                Return.Add((i.Item1, Shown));
            }
            return Return;
        }

        private void Initialize()
        {
            if (listBox == mainInterface.LBValidLocations) { displayList = DisplayListType.Locations; TextBox = mainInterface.TXTLocSearch; }
            if (listBox == mainInterface.LBValidEntrances) { displayList = DisplayListType.Entrances; TextBox = mainInterface.TXTEntSearch; }
            if (listBox == mainInterface.LBCheckedLocations) { displayList = DisplayListType.Checked; TextBox = mainInterface.TXTCheckedSearch; }
            SelectedItems = listBox.SelectedItems.Cast<object>().ToList();
            AreaHeaders = SelectedItems.Where(x => x is Areaheader).Cast<Areaheader>().ToList();
            NavigatableAreas = AreaHeaders.Where(x => InstanceContainer.Instance.AreaPool.ContainsKey(x.Area)).ToList();
            CheckableLocations = SelectedItems.OfType<CheckableLocation>().ToList();
            CheckableLocationsProxyAsLocation = CheckableLocations.Select(x => x is LocationData.LocationProxy PRO ? PRO.GetReferenceLocation() : x ).ToList();
            CheckableLocationsProxyAsLogicRef = CheckableLocations.Select(x => x is LocationData.LocationProxy PRO ? PRO.GetLogicInheritance() : x).ToList();
            CheckedLocations = CheckableLocations.Where(x => x.CheckState == MiscData.CheckState.Checked).ToList();
            MarkedLocations = CheckableLocations.Where(x => x.CheckState == MiscData.CheckState.Marked).ToList();
            NotCheckedLocations = CheckableLocations.Where(x => x.CheckState != MiscData.CheckState.Checked).ToList();
            NotMarkedLocations = CheckableLocations.Where(x => x.CheckState != MiscData.CheckState.Marked).ToList();
            PricedLocations = CheckableLocations.Where(x => x.hasPrice()).ToList();

            AllOptions = SelectedItems.OfType<OptionData.LogicOption>().ToList();
            ChoiceOptions = SelectedItems.OfType<OptionData.ChoiceOption>().ToList();
            ToggleOptions = SelectedItems.OfType<OptionData.ToggleOption>().ToList();
            MultiOptions = SelectedItems.OfType<OptionData.MultiSelectOption>().ToList();
            IntOptions = SelectedItems.OfType<OptionData.IntOption>().ToList();
        }

        private ToolStripItem AddItem(string Title, Action OnClick)
        {
            ToolStripItem ContextItem = contextMenuStrip.Items.Add(Title);
            ContextItem.Click += (sender, e) => OnClick();
            return ContextItem;
        }
    }
}
