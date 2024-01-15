using MMR_Tracker_V3.TrackerObjects;
using MMR_Tracker_V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerDataHandling;

namespace CLIFrontEnd
{
    internal class MainDisplay
    {
        public static void ShowAvailableLocations(InstanceData.InstanceContainer container)
        {
            Console.Clear();
            container.logicCalculation.CalculateLogic();
            var Data = new MiscData.TrackerLocationDataList(new MiscData.Divider("==========="), container, "");
            Data.PopulateAvailableLocationList();
            Dictionary<int, object> Locations = Data.FinalData.Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
            int Padding = Locations.Keys.Max().ToString().Length;
            foreach (var LocationObject in Locations)
            {
                Console.WriteLine($"{LocationObject.Key.ToString($"D{Padding}")}: {LocationObject.Value}");
            }
            Console.ReadLine();
        }
        public static void ShowAvailableEntrances(InstanceData.InstanceContainer container)
        {
            Console.Clear();
            container.logicCalculation.CalculateLogic();
            var Data = new MiscData.TrackerLocationDataList(new MiscData.Divider("==========="), container, "");
            Data.PopulateAvailableEntranceList();
            Dictionary<int, object> Locations = Data.FinalData.Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
            int Padding = Locations.Keys.Max().ToString().Length;
            foreach (var LocationObject in Locations)
            {
                Console.WriteLine($"{LocationObject.Key.ToString($"D{Padding}")}: {LocationObject.Value}");
            }
            Console.ReadLine();
        }
        public static void ShowCheckedLocations(InstanceData.InstanceContainer container)
        {
            Console.Clear();
            container.logicCalculation.CalculateLogic();
            var Data = new MiscData.TrackerLocationDataList(new MiscData.Divider("==========="), container, "");
            Data.PopulateCheckedLocationList(); 
            Dictionary<int, object> Locations = Data.FinalData.Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
            int Padding = Locations.Keys.Max().ToString().Length;
            foreach (var LocationObject in Locations)
            {
                Console.WriteLine($"{LocationObject.Key.ToString($"D{Padding}")}: {LocationObject.Value}");
            }
            Console.ReadLine();
        }
    }
}
