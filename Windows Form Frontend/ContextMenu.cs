using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace Windows_Form_Frontend
{
    public class WinformContextMenu
    {
        public static void BuildContextMenu(MainInterface mainInterface, ListBox Sender)
        {
            IEnumerable<object> SelectedItems = Sender.SelectedItems.Cast<object>().ToList();
            var IC = MainInterface.InstanceContainer;
            DisplayListType displayList = DisplayListType.Locations;
            TextBox TextBox = null;
            if (Sender == mainInterface.LBValidLocations) { displayList = DisplayListType.Locations; TextBox = mainInterface.TXTLocSearch; }
            if (Sender == mainInterface.LBValidEntrances) { displayList = DisplayListType.Entrances; TextBox = mainInterface.TXTEntSearch; }
            if (Sender == mainInterface.LBCheckedLocations) { displayList = DisplayListType.Checked; TextBox = mainInterface.TXTCheckedSearch; }

            var contextMenu = new ContextMenu.LocationListContextMenu(IC, SelectedItems);
            var ContextMenuBuilder = new ContextMenu.DefaultActionBuilder(contextMenu);
            ContextMenuBuilder.AddRefreshAction(() => { mainInterface.PrintToListBox([Sender]); });
            ContextMenuBuilder.AddNavigateToAction(() => 
                { mainInterface.CMBEnd.SelectedItem = contextMenu.ItemGroupings.NavigatableAreas[0].Area; });
            ContextMenuBuilder.AddFilterAreasAction(() =>
            {
                TextBox.Text = string.Empty;
                foreach (var item in contextMenu.ItemGroupings.AreaHeaders)
                {
                    if (string.IsNullOrWhiteSpace(TextBox.Text)) { TextBox.Text = $"#{item.Area}"; }
                    else { TextBox.Text += $"|#{item.Area}"; };
                }
            });
            ContextMenuBuilder.AddMinimizeHeaderAction(() => mainInterface.PrintToListBox(), displayList);
            ContextMenuBuilder.AddMaximizeHeaderAction(() => mainInterface.PrintToListBox(), displayList);
            ContextMenuBuilder.AddStarAction(() => mainInterface.PrintToListBox());
            ContextMenuBuilder.AddUnstarAction(() => mainInterface.PrintToListBox());
            ContextMenuBuilder.AddHideAction(() => mainInterface.PrintToListBox());
            ContextMenuBuilder.AddUnHideAction(() => mainInterface.PrintToListBox());
            ContextMenuBuilder.AddShowLogicAction(() => 
                { new ShowLogic(contextMenu.ItemGroupings.CheckableLocationsProxyAsLogicRef[0].ID, IC).Show(); });
            ContextMenuBuilder.AddWhatUnlockedAction(() => { ShowUnlockData(contextMenu.ItemGroupings.CheckableLocationsProxyAsLogicRef[0].ID, IC); });
            ContextMenuBuilder.AddCheckUnCheckedAction(() =>
            {
                mainInterface.HandleItemSelect(contextMenu.ItemGroupings.NotCheckedLocations, MiscData.CheckState.Checked, LB: Sender);
            });
            ContextMenuBuilder.AddUnCheckCheckedAction(() =>
            {
                mainInterface.HandleItemSelect(contextMenu.ItemGroupings.CheckedLocations, MiscData.CheckState.Unchecked, LB: Sender);
            });
            ContextMenuBuilder.AddMarkUnMarkedAction(() =>
            {
                mainInterface.HandleItemSelect(contextMenu.ItemGroupings.NotMarkedLocations, MiscData.CheckState.Marked, true, LB: Sender);
            });
            ContextMenuBuilder.AddUnMarkMarkedAction(() =>
            {
                mainInterface.HandleItemSelect(contextMenu.ItemGroupings.MarkedLocations, MiscData.CheckState.Unchecked, LB: Sender);
            });
            ContextMenuBuilder.AddBasic("Edit Selected Options",
                () => { mainInterface.HandleItemSelect(contextMenu.ItemGroupings.AllOptions, MiscData.CheckState.Unchecked, LB: Sender); },
                () => { return contextMenu.ItemGroupings.AllOptions.Any() && AmountOptionTypesSelected(contextMenu) > 1; });
            ContextMenuBuilder.AddBasic("Edit Selected Choice Options",
                () => { mainInterface.HandleItemSelect(contextMenu.ItemGroupings.ChoiceOptions, MiscData.CheckState.Unchecked, LB: Sender); },
                () => { return contextMenu.ItemGroupings.ChoiceOptions.Any(); });
            ContextMenuBuilder.AddBasic("Edit Selected Toggle Options",
                () => { mainInterface.HandleItemSelect(contextMenu.ItemGroupings.ToggleOptions, MiscData.CheckState.Unchecked, LB: Sender); },
                () => { return contextMenu.ItemGroupings.ToggleOptions.Any(); });
            ContextMenuBuilder.AddBasic("Edit Selected Multi Select Options",
                () => { mainInterface.HandleItemSelect(contextMenu.ItemGroupings.MultiOptions, MiscData.CheckState.Unchecked, LB: Sender); },
                () => { return contextMenu.ItemGroupings.MultiOptions.Any(); });
            ContextMenuBuilder.AddBasic("Edit Selected Int Options",
                () => { mainInterface.HandleItemSelect(contextMenu.ItemGroupings.IntOptions, MiscData.CheckState.Unchecked, LB: Sender); },
                () => { return contextMenu.ItemGroupings.IntOptions.Any(); });
            ContextMenuBuilder.AddEditPriceAction(() =>
            {
                var PricedLocations = contextMenu.ItemGroupings.CheckableLocations.Select(x => new VariableInputWindow.PriceContainer(x));
                VariableInputWindow PriceInput = new(PricedLocations, IC);
                PriceInput.ShowDialog();
                return PriceInput._Result;
            }, () => mainInterface.PrintToListBox());
            ContextMenuBuilder.AddClearPriceAction(() => mainInterface.PrintToListBox());
            ContextMenuBuilder.AddItemAtCheckAction(SelectItemFunc, PrintCheckItemResult);
            ContextMenuBuilder.AddItemInAreaAction(SelectItemFunc, PrintCheckItemResult);

            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            foreach (var i in contextMenu.GetMenu())
            {
                if (!i.Value.Condition()) { continue; }
                var MenuItem = new ToolStripMenuItem
                {
                    Text = i.Value.Display
                };
                MenuItem.Click += (sender, e) => { i.Value.Action(); };
                contextMenuStrip.Items.Add(MenuItem);
            }
            if (contextMenuStrip.Items.Count > 0)
            {
                contextMenuStrip.Show(Cursor.Position);
            }

            string SelectItemFunc()
            {
                CheckItemForm checkItemForm = new([null], IC, false);
                checkItemForm.ShowDialog();
                if (checkItemForm._Result.Count == 0) { return null; }
                return checkItemForm._Result[0].GetItemLocation().ItemData.ItemID;
            }
            void PrintCheckItemResult(bool found, string location, string item)
            {
                if (found) { MessageBox.Show($"{item} Can be found at {location}", "Item Found", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                else { MessageBox.Show($"{item} Can NOT be found at {location}", "Item Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }
        private static void ShowUnlockData(string iD, InstanceContainer IC)
        {
            if (!IC.Instance.UnlockData.ContainsKey(iD)) { return; }
            var AdvancedUnlockData = PlaythroughTools.GetAdvancedUnlockData(iD, IC.Instance.UnlockData, IC.Instance);
            var DataDisplay = PlaythroughTools.FormatAdvancedUnlockData(AdvancedUnlockData, IC);
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
        private static dynamic ShowUnlockSubFunction(dynamic dynamic)
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
        private static int AmountOptionTypesSelected(ContextMenu.LocationListContextMenu contextMenu)
        {
            int count = 0;
            if (contextMenu.ItemGroupings.ChoiceOptions.Count > 0) { count++; }
            if (contextMenu.ItemGroupings.IntOptions.Count > 0) { count++; }
            if (contextMenu.ItemGroupings.ToggleOptions.Count > 0) { count++; }
            if (contextMenu.ItemGroupings.MultiOptions.Count > 0) { count++; }
            return count;
        }
    }
}
