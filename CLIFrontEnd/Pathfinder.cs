﻿using MMR_Tracker_V3.TrackerObjects;

namespace CLIFrontEnd
{
    internal class Pathfinder(InstanceData.InstanceContainer container)
    {
        public void Show()
        {
            var ValidStartingAreas = MMR_Tracker_V3.Pathfinder.GetValidStartingAreas(container.Instance);
            var ValidDestinationAreas = MMR_Tracker_V3.Pathfinder.GetValidDestinationAreas(container.Instance);
            if (ValidStartingAreas.Length == 0 || ValidDestinationAreas.Length == 0) { return; }

            string selectedStartingArea = null;
            string selectedDestinationArea = null;
            while (selectedStartingArea ==  null)
            {
                Console.Clear();
                var StartingAreas = PrintAreas(ValidStartingAreas.OrderBy(x => x));
                Console.WriteLine(CLIUtility.CreateDivider());
                Console.WriteLine("Select Starting Area");
                if (!int.TryParse(Console.ReadLine(), out int selectedStartingAreaInd) ||
                    !StartingAreas.TryGetValue(selectedStartingAreaInd, out selectedStartingArea)) 
                {
                    selectedStartingArea = null;
                    continue; 
                }
            }
            while (selectedDestinationArea == null)
            {
                Console.Clear();
                var DestinationAreas = PrintAreas(ValidDestinationAreas.OrderBy(x => x));
                Console.WriteLine(CLIUtility.CreateDivider());
                Console.WriteLine("Select Destination Area");
                if (!int.TryParse(Console.ReadLine(), out int selectedDestinationAreaInd) ||
                    !DestinationAreas.TryGetValue(selectedDestinationAreaInd, out selectedDestinationArea))
                {
                    selectedDestinationArea = null;
                    continue; 
                }
            }
            MMR_Tracker_V3.Pathfinder pathfinder = new();
            pathfinder.FindPath(container.Instance, selectedStartingArea, selectedDestinationArea);

            Console.Clear();
            foreach (var path in pathfinder.FinalPath)
            {
                Console.WriteLine(CLIUtility.CreateDivider());
                foreach (var stop in path)
                {
                    Console.WriteLine(stop.Value == "" ? stop.Key : $"{stop.Key} => {stop.Value}");
                }
            }
            Console.ReadLine();
        }

        private Dictionary<int, string> PrintAreas(IEnumerable<string> Areas)
        {
            Dictionary<int, string> StartingAreas = Areas.Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
            int Padding = StartingAreas.Keys.Max().ToString().Length;
            foreach (var x in StartingAreas)
            {
                Console.WriteLine($"{x.Key.ToString($"D{Padding}")}: {x.Value}");
            }
            return StartingAreas;
        }
    }
}
