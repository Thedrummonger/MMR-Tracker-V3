using MMR_Tracker_V3;
using MMR_Tracker_V3.SpoilerLogImporter;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using static CLIFrontEnd.CLIUtility;
using static MMR_Tracker_V3.TrackerDataHandling;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace CLIFrontEnd
{
    internal class MainDisplay(InstanceContainer _instanceContainer)
    {
        public InstanceContainer instanceContainer = _instanceContainer;
        public CLIDisplayListType displayType = CLIDisplayListType.Locations;
        public bool ShowHelp = true;
        public string Filter = "";

        public void Display()
        {
            Dictionary<string, DisplayAction> Options = new()
            {
                { "l", new(() => { displayType = CLIDisplayListType.Locations; }, "Displays Available Locations") },
                { "e", new(() => { displayType = instanceContainer.Instance.CombineEntrancesWithLocations() ? CLIDisplayListType.Locations : CLIDisplayListType.Entrances;}, "Displays Available Entrances") },
                { "c", new(() => { displayType = CLIDisplayListType.Checked; }, "Displays Checked Locations/entrances") },
                { "o", new(() => { displayType = CLIDisplayListType.Options; }, "Displays Logic Options") },
                { "p", new(() => { displayType = CLIDisplayListType.Pathfinder; }, "Displays Pathfinder") },
                { "i", new(() => { new ItemPoolEditor().Start(instanceContainer); instanceContainer.logicCalculation.CompileOptionActionEdits(); }, "Item Pool Options") },
                { "z", new(() => { instanceContainer.DoUndo(); instanceContainer.logicCalculation.CompileOptionActionEdits(); }, "Undo last action") },
                { "y", new(() => { instanceContainer.DoRedo(); instanceContainer.logicCalculation.CompileOptionActionEdits(); }, "Redo last undo action") },
                { "s", new(SaveInstance, "Save as") },
                { "r", new(ImportSpoilerLog, "Import Spoiler Log") },
                { "h", new(() => { ShowHelp = !ShowHelp; }, "Toggle Help Menu") },
            };
            while (true)
            {
                Console.Clear();

                Dictionary<int, object> Objects = [];
                switch (displayType)
                {
                    case CLIDisplayListType.Locations:
                        Objects = ShowAvailableLocations();
                        break;
                    case CLIDisplayListType.Entrances:
                        Objects = ShowAvailableEntrances();
                        break;
                    case CLIDisplayListType.Checked:
                        Objects = ShowCheckedLocations();
                        break;
                    case CLIDisplayListType.Options:
                        Objects = ShowOptions();
                        break;
                    case CLIDisplayListType.Pathfinder:
                        Pathfinder pathfinder = new Pathfinder(instanceContainer);
                        pathfinder.Show();
                        displayType = CLIDisplayListType.Locations;
                        continue;
                }


                Console.WriteLine(CreateDivider());
                if (ShowHelp)
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("Type the items index to select it");
                    Console.WriteLine("Multiple items can be separated with , and ranges can be defined with -");
                    Console.WriteLine("Example: 1,12,18-25");
                    Console.WriteLine("Add one of the following Prefixes to change the action");
                    Console.WriteLine(@"#: Mark the selected location");
                    Console.WriteLine(@"@: Opens a context menu for the selected items containing additional actions.");
                    Console.WriteLine("\nAdditional Commands:");
                    foreach (var i in Options)
                    {
                        Console.WriteLine($"{i.Key.ToUpper()}: {i.Value.Description}");
                    }
                    Console.WriteLine(@"\: Use this followed by a search string to filter the list");
                    Console.WriteLine(@"x: Return to the main menu");
                    Console.WriteLine(CreateDivider(1));
                }
                else
                {
                    Console.WriteLine("Use h to show Help Menu");
                }

                var input = Console.ReadLine() ?? "";
                if (input.Equals("x", StringComparison.CurrentCultureIgnoreCase))
                {
                    var MenuSelect = new CLISelectMenu(["Yes", "No"]);
                    MenuSelect.Run(["Are you sure you want to exit?"]);
                    if (MenuSelect.selectedLineIndex == 0)
                    {
                        if (instanceContainer.UnsavedChanges)
                        {
                            MenuSelect.Run(["You have Unsaved Changes, would you like to save?"]);
                            if (MenuSelect.selectedLineIndex == 0) { SaveInstance(); }
                        }
                        return;
                    }
                }

                if (Options.TryGetValue(input.ToLower(), out DisplayAction? value))
                {
                    value.action();
                    continue;
                }

                var InputData = new InputPrefixData(input);

                if (InputData.Prefixes.Contains('\\'))
                {
                    Filter = InputData.ParsedInput;
                    continue;
                }
                else if (InputData.Prefixes.Contains('@'))
                {
                    CLIContextMenu.OpenContextMenu(this, InputData, Objects);
                }
                else
                {
                    CheckState checkState = displayType == CLIDisplayListType.Checked ? CheckState.Unchecked : CheckState.Checked;
                    if (InputData.Prefixes.Contains('#')) { checkState = CheckState.Marked; }
                    if (CheckForAreaHeaderFilterSelection(InputData.Indexes, Objects) is string S) { Filter = S; continue; }
                    instanceContainer.SaveState();
                    CheckItems(InputData.Indexes, Objects, checkState, displayType);
                    if (displayType == CLIDisplayListType.Options) { instanceContainer.logicCalculation.CompileOptionActionEdits(); }
                }

            }
        }

        private string? CheckForAreaHeaderFilterSelection(List<int> Indexes, Dictionary<int, object> reference)
        {
            DisplayListType? Source = displayType.ToStandardDisplayListType();
            if (Source is not null && Indexes.All(x => reference.TryGetValue(x, out object? value) && value is MiscData.Areaheader))
            {
                List<string> areas = [];
                foreach(int index in Indexes)
                {
                    var Area = (Areaheader)reference[index];
                    areas.Add(Area.Area);
                }
                return string.Join(" || ", areas.Select(x => $"#{x}"));
            }
            return null;
        }

        private void SaveInstance()
        {
            var Result = NativeFileDialogSharp.Dialog.FileSave("mmrtsav");
            if (Result.IsOk) 
            { 
                bool SaveSuccess = instanceContainer.SaveInstance(Result.Path); 
                if (SaveSuccess) { instanceContainer.UnsavedChanges = false; }
            }
        }

        private void ImportSpoilerLog()
        {
            if (instanceContainer.Instance.SpoilerLog == null)
            {
                var Result = NativeFileDialogSharp.Dialog.FileOpen(SpoilerLogTools.GetSpoilerLogFilter(instanceContainer.Instance).SplitOnce('.', true).Item2);
                if (Result.IsOk)
                {
                    instanceContainer.SaveState();
                    SpoilerLogTools.ImportSpoilerLog(Result.Path, instanceContainer);
                }
            }
            else
            {
                instanceContainer.SaveState();
                SpoilerLogTools.RemoveSpoilerData(instanceContainer.Instance);
            }
        }

        private void HideLocations(List<int> indexes, Dictionary<int, object> reference)
        {
            List<object> CheckObjects = indexes.Where(x => reference.ContainsKey(x)).Select(x => reference[x]).ToList();
            foreach (var i in CheckObjects)
            {
                if (i is CheckableLocation checkableLocation)
                {
                    checkableLocation.Hidden = !checkableLocation.Hidden;
                }
            }
        }

        private void SetLocationPrice(List<int> indexes, Dictionary<int, object> reference)
        {
            List<object> CheckObjects = indexes.Where(x => reference.ContainsKey(x)).Select(x => reference[x]).ToList();
            foreach (var i in CheckObjects)
            {
                if (i is CheckableLocation checkableLocation)
                {
                    LocationChecking.SetPrice(checkableLocation);
                }
            }
        }

        private void CheckItems(List<int> Indexes, Dictionary<int, object> reference, CheckState checkState, CLIDisplayListType displayType)
        {
            List<object> CheckObjects = Indexes.Where(x => reference.ContainsKey(x)).Select(x => reference[x]).ToList();
            var CheckObjectOptions = new MiscData.CheckItemSetting(checkState)
                            .SetCheckUnassignedLocations(LocationChecking.HandleUnAssignedLocations)
                            .SetCheckUnassignedEntrances(LocationChecking.HandleUnAssignedLocations)
                            .SetCheckUnassignedHints(LocationChecking.HandleUnAssignedVariables)
                            .SetCheckChoiceOptions(LocationChecking.HandleUnAssignedLocations)
                            .SetCheckIntOptions(LocationChecking.HandleUnAssignedVariables);
            LocationChecker.CheckSelectedItems(CheckObjects, instanceContainer, CheckObjectOptions);
        }

        private Dictionary<int, object> ShowAvailableLocations()
        {
            Console.Clear();
            instanceContainer.logicCalculation.CalculateLogic();
            var Data = new MiscData.TrackerLocationDataList(new Divider(""), instanceContainer, Filter).PrintReverse();
            Data.WriteHints(MiscData.CheckState.Unchecked);
            if (instanceContainer.Instance.CombineEntrancesWithLocations()) { Data.WriteEntrances(MiscData.CheckState.Unchecked, true); }
            Data.WriteLocations(MiscData.CheckState.Unchecked, true).WriteLocations(MiscData.CheckState.Unchecked, false);
            return PrintData(Data, DisplayListType.Locations);
        }
        private Dictionary<int, object> ShowAvailableEntrances()
        {
            Console.Clear();
            instanceContainer.logicCalculation.CalculateLogic();
            var Data = new MiscData.TrackerLocationDataList(new Divider(""), instanceContainer, Filter).PrintReverse();
            Data.WriteEntrances(MiscData.CheckState.Unchecked, false);
            return PrintData(Data, DisplayListType.Entrances);
        }
        private Dictionary<int, object> ShowCheckedLocations()
        {
            Console.Clear();
            instanceContainer.logicCalculation.CalculateLogic();
            var Data = new TrackerLocationDataList(new Divider(""), instanceContainer, Filter).PrintReverse();
            if (instanceContainer.Instance.StaticOptions.ShowOptionsInListBox == DisplayListType.Checked) { Data.WriteOptions(); }
            Data.WriteOnlineItems().WriteStartingItems().WriteHints(MiscData.CheckState.Checked).WriteEntrances(MiscData.CheckState.Checked, true)
                .WriteLocations(MiscData.CheckState.Checked, true).WriteLocations(MiscData.CheckState.Checked, false);
            return PrintData(Data, DisplayListType.Checked);
        }

        private Dictionary<int, object> ShowOptions()
        {
            Console.Clear();
            instanceContainer.logicCalculation.CalculateLogic();
            var Data = new TrackerLocationDataList(new Divider(""), instanceContainer, Filter);
            Data.WriteOptions();
            return PrintData(Data, null);
        }

        private Dictionary<int, object> PrintData(TrackerLocationDataList Data, DisplayListType? Source)
        {
            Dictionary<int, object> Locations = Data.FinalData.Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
            if (Locations.Count == 0) { return Locations; }
            int Padding = Locations.Keys.Max().ToString().Length;
            bool InMinimized = false;
            foreach (var LocationObject in Locations)
            {
                object Value = LocationObject.Value;
                if (Source is not null && Value is Areaheader area)
                {
                    InMinimized = area.IsMinimized((DisplayListType)Source, instanceContainer.Instance.StaticOptions);
                    if (InMinimized) { area.DisplayOverride = area.Area + " ---"; }
                }
                else if (InMinimized) { continue; }
                if (Value is MiscData.Divider) { Value = CLIUtility.CreateDivider(LocationObject.Key.ToString($"D{Padding}").Length + 3); }
                Console.WriteLine($"{LocationObject.Key.ToString($"D{Padding}")}: {Value}");
            }
            return Locations;
        }
    }
}
