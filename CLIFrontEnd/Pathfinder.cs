using MathNet.Numerics;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CLIFrontEnd.CLIUtility;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CLIFrontEnd
{
    internal class Pathfinder(InstanceData.InstanceContainer container)
    {
        public void Show()
        {
            string selectedStartingArea = null;
            string selectedDestinationArea = null;
            while (selectedStartingArea ==  null)
            {
                Console.Clear();
                var StartingAreas = PrintStartingAreas();
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
                var DestinationAreas = PrintDestinationAreas();
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

        private Dictionary<int, string> PrintStartingAreas()
        {
            return PrintAreas(MMR_Tracker_V3.Pathfinder.GetValidStartingAreas(container.Instance).OrderBy(x => x));
        }
        private Dictionary<int, string> PrintDestinationAreas()
        {
            return PrintAreas(MMR_Tracker_V3.Pathfinder.GetValidDestinationAreas(container.Instance).OrderBy(x => x));
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
