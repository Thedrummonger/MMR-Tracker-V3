using MMR_Tracker_V3.TrackerObjects;
using MMR_Tracker_V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using MMR_Tracker_V3.TrackerObjectExtentions;

namespace CLIFrontEnd
{
    internal class LocationChecking
    {
        public static void SetPrice(InstanceData.InstanceContainer Container, dynamic entry)
        {
            if (!Utility.DynamicPropertyExist(entry, "Price") && !MMR_Tracker_V3.PriceRando.TestForPriceData(entry)) { return; }
            entry.GetPrice(out int p, out char c);
            if (p > -1) { entry.SetPrice(-1); return; }
            var DictEntry = entry.GetDictEntry(Container.Instance);
            Console.Clear();
            while (p == -1)
            {
                Console.WriteLine($"Enter Price for {DictEntry.Name ?? DictEntry.ID}");
                var input = Console.ReadLine();
                if (int.TryParse(input, out int newPrice) && newPrice > -1) { entry.SetPrice(newPrice); }
                else { Console.WriteLine($"{input} is not a valid price. Price must be a positive number"); }
                entry.GetPrice(out p, out c);
            }
        }

        public static List<ManualCheckObjectResult> HandleUnAssignedLocations(IEnumerable<object> CheckObject, InstanceContainer Instance)
        {
            List<ManualCheckObjectResult> Result = new List<ManualCheckObjectResult>();
            foreach (var i in CheckObject)
            {
                if (i is LocationData.LocationObject LO)
                {
                    Result.Add(LoopItemSelect(LO));
                }
                if (i is EntranceData.EntranceRandoExit EO)
                {
                    Result.Add(LoopEntranceSelect(EO));
                }
            }
            return Result;
        }

        public static List<ManualCheckObjectResult> HandleUnAssignedVariables(IEnumerable<object> CheckObject, InstanceContainer Instance)
        {
            List<ManualCheckObjectResult> Result = new List<ManualCheckObjectResult>();
            foreach (var i in CheckObject)
            {
                if (i is HintData.HintObject HO)
                {
                    Result.Add(LoopHintSelect(HO));
                }
            }
            return Result;
        }
        private static ManualCheckObjectResult LoopItemSelect(LocationObject Location)
        {
            string Filter = "";
            while (true)
            {
                Console.Clear();
                var ValidItems = Location.GetValidItems(Filter);
                Dictionary<int, ItemData.ItemObject> Items = ValidItems.Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
                int Padding = Items.Keys.Max().ToString().Length;
                foreach (var i in Items)
                {
                    Console.WriteLine($"{i.Key.ToString($"D{Padding}")}: {i.Value}");
                }
                Console.WriteLine(CLIUtility.CreateDivider());
                Console.WriteLine("Select Item at " + Location.GetDictEntry().GetName());
                var input = Console.ReadLine();
                if (int.TryParse(input, out int index) && Items.TryGetValue(index, out ItemData.ItemObject? value))
                {
                    return new ManualCheckObjectResult(Location, value.ID);
                }
                else if (input.StartsWith(@"\"))
                {
                    Filter = input[1..];
                }
            }
        }
        private static ManualCheckObjectResult LoopEntranceSelect(EntranceData.EntranceRandoExit Exit)
        {
            var Instance = Exit.GetParent();
            string Filter = "";
            while (true)
            {
                Console.Clear();
                var Counter = 0;
                Dictionary<int, EntranceData.EntranceRandoDestination> EnteredItems = Instance.GetAllLoadingZoneDestinations(Filter)
                    .Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
                int Padding = EnteredItems.Keys.Max().ToString().Length;
                foreach (var Entry in EnteredItems)
                {
                    Console.WriteLine($"{Entry.Key.ToString($"D{Padding}")}: {Entry.Value}");
                }
                Console.WriteLine(CLIUtility.CreateDivider());
                Console.WriteLine("Select Destination at Exit " + Exit.ParentAreaID + " -> " + Exit.ID);
                var input = Console.ReadLine();
                if (int.TryParse(input, out int index) && EnteredItems.TryGetValue(index, out EntranceData.EntranceRandoDestination? value))
                {
                    return new ManualCheckObjectResult(Exit, value);
                }
                else if (input.StartsWith(@"\"))
                {
                    Filter = input[1..];
                }
            }
        }
        private static ManualCheckObjectResult LoopHintSelect(HintData.HintObject HintSpot)
        {
            Console.Clear();
            Console.WriteLine($"Enter Hint at {HintSpot.ID}");
            string hint = Console.ReadLine();
            //HintSpot.HintText = hint;
            return new ManualCheckObjectResult(HintSpot, hint);
        }
    }
}
