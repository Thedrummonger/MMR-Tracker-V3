using MMR_Tracker_V3.TrackerObjects;
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

namespace CLIFrontEnd
{
    internal class MainDisplay(InstanceContainer instanceContainer)
    {
        DisplayListType displayType = DisplayListType.Locations;
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
                    case DisplayListType.Locations:
                        Objects = ShowAvailableLocations();
                        break;
                    case DisplayListType.Entrances:
                        Objects = ShowAvailableEntrances();
                        break;
                    case DisplayListType.Checked:
                        Objects = ShowCheckedLocations();
                        break;
                }

                var input = Console.ReadLine();
                CheckState checkState = displayType == DisplayListType.Checked ? CheckState.Unchecked : CheckState.Checked;

                if (input == "l") { displayType = DisplayListType.Locations; }
                else if (input == "e") { displayType = instanceContainer.Instance.CombineEntrancesWithLocations() ? DisplayListType.Locations : DisplayListType.Entrances; }
                else if (input == "c") { displayType = DisplayListType.Checked; }
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
            Data.PopulateAvailableLocationList();
            Dictionary<int, object> Locations = Data.FinalData.Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
            int Padding = Locations.Keys.Max().ToString().Length;
            bool InMinimized = false;
            foreach (var LocationObject in Locations)
            {
                if (LocationObject.Value is Areaheader area) { InMinimized = area.IsMinimized(DisplayListType.Locations, instanceContainer.Instance.StaticOptions); }
                else if (InMinimized) { continue; }
                Console.WriteLine($"{LocationObject.Key.ToString($"D{Padding}")}: {LocationObject.Value}");
            }
            return Locations;
        }
        private Dictionary<int, object> ShowAvailableEntrances()
        {
            Console.Clear();
            instanceContainer.logicCalculation.CalculateLogic();
            var Data = new MiscData.TrackerLocationDataList(new MiscData.Divider("==========="), instanceContainer, Filter);
            Data.PopulateAvailableEntranceList();
            Dictionary<int, object> Locations = Data.FinalData.Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
            int Padding = Locations.Keys.Max().ToString().Length;
            bool InMinimized = false;
            foreach (var LocationObject in Locations)
            {
                if (LocationObject.Value is Areaheader area) { InMinimized = area.IsMinimized(DisplayListType.Entrances, instanceContainer.Instance.StaticOptions); }
                else if (InMinimized) { continue; }
                Console.WriteLine($"{LocationObject.Key.ToString($"D{Padding}")}: {LocationObject.Value}");
            }
            return Locations;
        }
        private Dictionary<int, object> ShowCheckedLocations()
        {
            Console.Clear();
            instanceContainer.logicCalculation.CalculateLogic();
            var Data = new TrackerLocationDataList(new Divider("==========="), instanceContainer, Filter);
            Data.PopulateCheckedLocationList(); 
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
