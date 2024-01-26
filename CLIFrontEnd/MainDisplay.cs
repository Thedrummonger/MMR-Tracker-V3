using MMR_Tracker_V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerDataHandling;
using static CLIFrontEnd.CLIUtility;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using Octokit;
using static MMR_Tracker_V3.Logic.LogicStringParser;
using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;

namespace CLIFrontEnd
{
    internal class MainDisplay(InstanceContainer instanceContainer)
    {
        CLIDisplayListType displayType = CLIDisplayListType.Locations;
        bool ShowHelp = true;
        string Filter = "";

        public void Display()
        {
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
                }

                var input = Console.ReadLine();
                CheckState checkState = displayType == CLIDisplayListType.Checked ? CheckState.Unchecked : CheckState.Checked;

                if (input == "l") { displayType = CLIDisplayListType.Locations; }
                else if (input == "e") { displayType = instanceContainer.Instance.CombineEntrancesWithLocations() ? CLIDisplayListType.Locations : CLIDisplayListType.Entrances; }
                else if (input == "c") { displayType = CLIDisplayListType.Checked; }
                else if (input == "o") { displayType = CLIDisplayListType.Options; }
                else if (input == "z") { instanceContainer.DoUndo(); }
                else if (input == "y") { instanceContainer.DoRedo(); }
                else if (input == "s")
                {
                    var Result = NativeFileDialogSharp.Dialog.FileSave("mmrtsav");
                    if (Result.IsOk) { instanceContainer.SaveInstance(Result.Path); }
                }
                else if (input == "h") { ShowHelp = !ShowHelp; }
                else
                {
                    var Indices = InputParser.ParseIndicesString(input);
                    CheckItems(Indices, Objects, checkState);
                }

            }
        }

        private void CheckItems(List<int> Indexes, Dictionary<int, object> reference, CheckState checkState)
        {
            List<object> CheckObjects = Indexes.Where(x => reference.ContainsKey(x)).Select(x => reference[x]).ToList();
            var CheckObjectOptions = new MiscData.CheckItemSetting(checkState)
                            .SetCheckUnassignedLocations(LocationChecking.HandleUnAssignedLocations)
                            .SetCheckUnassignedEntrances(LocationChecking.HandleUnAssignedLocations)
                            .SetCheckUnassignedHints(LocationChecking.HandleUnAssignedVariables)
                            .SetCheckCoiceOptions(LocationChecking.HandleUnAssignedLocations)
                            .SetCheckIntOPtions(LocationChecking.HandleUnAssignedVariables);
            LocationChecker.CheckSelectedItems(CheckObjects, instanceContainer, CheckObjectOptions);
        }

        private Dictionary<int, object> ShowAvailableLocations()
        {
            Console.Clear();
            instanceContainer.logicCalculation.CalculateLogic();
            var Data = new MiscData.TrackerLocationDataList(new MiscData.Divider("==========="), instanceContainer, Filter);
            Data.WriteLocations(MiscData.CheckState.Unchecked, false).WriteLocations(MiscData.CheckState.Unchecked, true);
            if (instanceContainer.Instance.CombineEntrancesWithLocations()) { Data.WriteEntrances(MiscData.CheckState.Unchecked, true); }
            Data.WriteHints(MiscData.CheckState.Unchecked);
            return PrintData(Data);
        }
        private Dictionary<int, object> ShowAvailableEntrances()
        {
            Console.Clear();
            instanceContainer.logicCalculation.CalculateLogic();
            var Data = new MiscData.TrackerLocationDataList(new MiscData.Divider("==========="), instanceContainer, Filter);
            Data.WriteEntrances(MiscData.CheckState.Unchecked, false);
            return PrintData(Data);
        }
        private Dictionary<int, object> ShowCheckedLocations()
        {
            Console.Clear();
            instanceContainer.logicCalculation.CalculateLogic();
            var Data = new TrackerLocationDataList(new Divider("==========="), instanceContainer, Filter);
            Data.WriteLocations(MiscData.CheckState.Checked, false).WriteLocations(MiscData.CheckState.Checked, true)
                    .WriteEntrances(MiscData.CheckState.Checked, true).WriteHints(MiscData.CheckState.Checked).WriteStartingItems().WriteOnlineItems();
            if (instanceContainer.Instance.StaticOptions.ShowOptionsInListBox == DisplayListType.Checked) { Data.WriteOptions(); }
            return PrintData(Data);
        }

        private Dictionary<int, object> ShowOptions()
        {
            Console.Clear();
            instanceContainer.logicCalculation.CalculateLogic();
            var Data = new TrackerLocationDataList(new Divider("==========="), instanceContainer, Filter); 
            Data.WriteOptions();
            return PrintData(Data);
        }

        private Dictionary<int, object> PrintData(TrackerLocationDataList Data)
        {
            Dictionary<int, object> Locations = Data.FinalData.Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
            int Padding = Locations.Keys.Max().ToString().Length;
            bool InMinimized = false;
            foreach (var LocationObject in Locations)
            {
                if (LocationObject.Value is Areaheader area) { InMinimized = area.IsMinimized(DisplayListType.Checked, instanceContainer.Instance.StaticOptions); }
                else if (InMinimized) { continue; }
                Console.WriteLine($"{LocationObject.Key.ToString($"D{Padding}")}: {LocationObject.Value}");
            }
            return Locations;
        }
    }
}
