using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace CLIFrontEnd
{
    internal class CLIContextMenu
    {
        internal static void OpenContextMenu(MainDisplay mainDisplay, CLIUtility.InputPrefixData inputData, Dictionary<int, object> objects)
        {
            var IC = mainDisplay.instanceContainer;
            List<object> SelectedItems = [];
            foreach (var index in inputData.Indexes) { if (objects.TryGetValue(index, out object o)) { SelectedItems.Add(o); } }
            DisplayListType displayList = DisplayListType.Locations;
            if (mainDisplay.displayType == CLIUtility.CLIDisplayListType.Locations) { displayList = DisplayListType.Locations; }
            if (mainDisplay.displayType == CLIUtility.CLIDisplayListType.Entrances) { displayList = DisplayListType.Entrances; }
            if (mainDisplay.displayType == CLIUtility.CLIDisplayListType.Checked) { displayList = DisplayListType.Checked; }

            var contextMenu = new ContextMenu.LocationListContextMenu(IC, SelectedItems);
            var build = new ContextMenu.DefaultActionBuilder(contextMenu);

            build.AddRefreshAction(new Action(() => { }));
            build.AddFilterAreasAction(new Action(() => { mainDisplay.Filter = string.Join(" || ", contextMenu.ItemGroupings.AreaHeaders.Select(x => $"#{x.Area}")); }));
            build.AddMinimizeHeaderAction(null, displayList);
            build.AddMaximizeHeaderAction(null, displayList);
            build.AddStarAction(null);
            build.AddUnstarAction(null);
            build.AddHideAction(null);
            build.AddUnHideAction(null);
            build.AddCheckUnCheckedAction(() => { CheckLocations(IC, contextMenu.ItemGroupings.NotCheckedLocations, CheckState.Checked); });
            build.AddUnCheckCheckedAction(() => { CheckLocations(IC, contextMenu.ItemGroupings.CheckedLocations, CheckState.Unchecked); });
            build.AddMarkUnMarkedAction(() => { CheckLocations(IC, contextMenu.ItemGroupings.NotMarkedLocations, CheckState.Marked); });
            build.AddUnMarkMarkedAction(() => { CheckLocations(IC, contextMenu.ItemGroupings.MarkedLocations, CheckState.Unchecked); });
            build.AddShowLogicAction(() =>
            {
                Console.Clear();
                var SelectedLocation = contextMenu.ItemGroupings.CheckableLocationsProxyAsLogicRef.First();
                var Logic = IC.Instance.GetLogic(SelectedLocation.ID);
                Console.WriteLine(Logic.ToFormattedJson());
                Console.ReadLine();
            });
            build.AddWhatUnlockedAction(() =>
            {
                Console.Clear();
                var SelectedItem = contextMenu.ItemGroupings.CheckableLocationsProxyAsLogicRef[0].ID;
                if (!IC.Instance.UnlockData.ContainsKey(SelectedItem)) { return; }
                var AdvancedUnlockData = PlaythroughTools.GetAdvancedUnlockData(SelectedItem, IC.Instance.UnlockData, IC.Instance);
                var DataDisplay = PlaythroughTools.FormatAdvancedUnlockData(AdvancedUnlockData, IC);
                foreach(var i in DataDisplay) { Console.WriteLine(i.ToString()); }
                Console.ReadLine();
            });
            build.AddBasic("Edit Price", () =>
            {
                foreach (var i in contextMenu.ItemGroupings.CheckableLocations) { LocationChecking.SetPrice(i); }
            }, () =>
            {
                return contextMenu.ItemGroupings.CheckableLocations.Count > 0;
            });
            build.AddClearPriceAction(null);
            build.AddItemAtCheckAction(() => { return LoopAnyItemSelect(IC); }, DisplayCheckItemResult);
            build.AddItemInAreaAction(() => { return LoopAnyItemSelect(IC); }, DisplayCheckItemResult);

            var MenuItems = contextMenu.GetMenu();
            if (MenuItems.Count > 0)
            {
                var DisplayItems = MenuItems.Where(x => x.Value.Condition?.Invoke()??true).Select(x => new StandardListBoxItem(x.Value.Display, x.Value));
                CLISelectMenu ContextMenu = new(DisplayItems);
                ContextMenu.Run();
                if (ContextMenu.SelectedObject is not StandardListBoxItem SLI || SLI.Tag is not ContextMenu.ContextMenuItem CMI) { return; }
                CMI.Action?.Invoke();
            }

            void DisplayCheckItemResult(bool ItemFound, string location, string Item)
            {
                Console.Clear();
                Console.WriteLine($"{location} {(ItemFound ? "contained" : "did NOT contain")} {Item}");
                Console.ReadLine();
            }
        }
        private static void CheckLocations(InstanceData.InstanceContainer instanceContainer, IEnumerable<object> CheckObjects, MiscData.CheckState checkState)
        {
            var CheckObjectOptions = new MiscData.CheckItemSetting(checkState)
                            .SetCheckUnassignedLocations(LocationChecking.HandleUnAssignedLocations)
                            .SetCheckUnassignedEntrances(LocationChecking.HandleUnAssignedLocations)
                            .SetCheckUnassignedHints(LocationChecking.HandleUnAssignedVariables)
                            .SetCheckChoiceOptions(LocationChecking.HandleUnAssignedLocations)
                            .SetCheckIntOptions(LocationChecking.HandleUnAssignedVariables);
            LocationChecker.CheckSelectedItems(CheckObjects, instanceContainer, CheckObjectOptions);
        }
        private static string LoopAnyItemSelect(InstanceData.InstanceContainer IC)
        {
            string Filter = "";
            while (true)
            {
                Console.Clear();
                var ValidItems = IC.Instance.GetValidItemsForLocation(null, Filter, true);
                Dictionary<int, ItemData.ItemObject> Items = ValidItems.Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
                LocationChecking.PrintItems(Items);
                Console.WriteLine(CLIUtility.CreateDivider());
                Console.WriteLine("Select Item");
                var input = Console.ReadLine() ?? "";
                if (int.TryParse(input, out int index) && Items.TryGetValue(index, out ItemData.ItemObject? value))
                {
                    return value.ID;
                }
                else if (input.StartsWith(@"\"))
                {
                    Filter = input[1..];
                }
            }
        }
    }
}
