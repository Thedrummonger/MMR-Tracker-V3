using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CLIFrontEnd.CLIUtility;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace CLIFrontEnd
{
    public class ItemPoolEditor
    {
        public enum PoolEditType
        {
            ItemPool,
            EntrancePool,
            HintPool,
            StartingItemPool,
            TrickPool
        }
        public void Start(InstanceData.InstanceContainer container)
        {
            List<StandardListBoxItem> Options = 
                [
                    new("Item Pool", PoolEditType.ItemPool), 
                    new("Starting Item Pool", PoolEditType.StartingItemPool), 
                    new("Tricks", PoolEditType.TrickPool)
                ];
            if (container.Instance.EntrancePool.HasRandomizableEntrances()) { Options.Insert(1, new("Entrance Pool", PoolEditType.EntrancePool)); }
            if (container.Instance.HintPool.Count > 0) { Options.Insert(1, new("Hint Pool", PoolEditType.HintPool)); }
            CLISelectMenu Mode = new(Options);
            var Result = (StandardListBoxItem)Mode.Run();
            switch (Result.Tag)
            {
                case PoolEditType.ItemPool:
                    LoopLocationStateChange(container, [.. container.Instance.LocationPool.Values]);
                    break;
                case PoolEditType.EntrancePool:
                    LoopLocationStateChange(container, [.. container.Instance.EntrancePool.GetAllRandomizableExits()]);
                    break;
                case PoolEditType.HintPool:
                    LoopLocationStateChange(container, [.. container.Instance.HintPool.Values]);
                    break;
                case PoolEditType.StartingItemPool:
                    LoopStartPoolEdit(container);
                    break;
                case PoolEditType.TrickPool:
                    LoopTrickEdit(container);
                    break;
            }
        }

        private void LoopStartPoolEdit(InstanceContainer container)
        {
            string Filter = "";
            while (true)
            {
                Console.Clear();
                var ObjectList = PrintStartingItems(container, Filter);
                string ParsedInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(ParsedInput)) { return; }
                if (ParsedInput.StartsWith('\\')) { Filter = ParsedInput[1..]; continue; }
                var InputData = new InputPrefixData(ParsedInput);
                foreach (var Index in InputData.Indexes)
                {
                    if (!ObjectList.TryGetValue(Index, out ItemData.ItemObject IO)) { continue; }
                    int Amount = -1;
                    string? Error = null;
                    while (Amount < 0)
                    {
                        Console.Clear();
                        int MaxAmountInWorld = IO.GetDictEntry().GetMaxAmountInWorld();
                        Console.WriteLine($"Starting Pool Amounts for: {IO.GetDictEntry().GetName()}");
                        Console.WriteLine($"Current Amount in Starting Pool: {IO.AmountInStartingpool}");
                        Console.WriteLine($"Current Amount Placed in other locations: {IO.GetAmountPlaced() - IO.AmountInStartingpool}");
                        Console.WriteLine($"Max Amount in Placeable: {(MaxAmountInWorld < 0 ? 99999 : MaxAmountInWorld)}");
                        int AmountLeftToPlace = IO.GetAmountLeftToPlace();
                        string Range = AmountLeftToPlace == 0 ? "0" : $"0 - {AmountLeftToPlace}";
                        Console.WriteLine($"Set Amount in starting pool: (Valid Range: {Range})");
                        if (Error is not null) { Console.WriteLine($"Error: {Error}"); }
                        string Input = Console.ReadLine() ?? "";
                        if (string.IsNullOrWhiteSpace(Input)) { break; }
                        if (int.TryParse(Input, out var amount))
                        {
                            Amount = amount;
                            if (amount < 0) { Error = $"{amount} is not valid!"; }
                        }
                        if (Amount > IO.GetAmountLeftToPlace()) { Error = $"{Amount} is not valid!"; Amount = -1;  }
                    }
                    if (Amount > -1) { IO.AmountInStartingpool = Amount; }
                }
            }
        }

        private Dictionary<int, ItemData.ItemObject> PrintStartingItems(InstanceContainer container, string filter)
        {
            List<ItemData.ItemObject> Items = container.Instance.ItemPool.Values.Where(x => x.ValidStartingItem() && (x.GetAmountLeftToPlace() > 0 || x.AmountInStartingpool > 0)).ToList();
            Items = Items.Where(x => SearchStringParser.FilterSearch(container.Instance, x, filter, x.GetDictEntry().GetName())).ToList();
            Dictionary<int, ItemData.ItemObject> StartingItems = Items.Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
            int Padding = StartingItems.Keys.Max().ToString().Length;
            foreach (var LocationObject in StartingItems)
            {
                Console.WriteLine($"{LocationObject.Key.ToString($"D{Padding}")}: [X{LocationObject.Value.AmountInStartingpool}] {LocationObject.Value.GetDictEntry().GetName()}");
            }
            return StartingItems;
        }

        private void LoopLocationStateChange(InstanceContainer container, List<object> Objects)
        {
            string Filter = "";
            while (true)
            {
                Console.Clear();
                var ObjectList = PrintCheckableLocations(container, Objects, Filter);
                string ParsedInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(ParsedInput)) { return; }
                if (ParsedInput.StartsWith('\\')) { Filter = ParsedInput[1..]; continue; }
                var InputData = new InputPrefixData(ParsedInput);

                var EnumList = Enum.GetValues(typeof(MiscData.RandomizedState)).Cast<MiscData.RandomizedState>().Select(x => new MiscData.StandardListBoxItem(x.ToString(), x));

                CLISelectMenu CheckState = new(EnumList);
                CheckState.Run(["Select New Randomized State"]);

                foreach (var Index in InputData.Indexes)
                {
                    if (!ObjectList.TryGetValue(Index, out CheckableLocation Location)) { continue; }
                    Location.RandomizedState = (RandomizedState)((StandardListBoxItem)CheckState.SelectedObject).Tag;
                }
            }
        }

        private Dictionary<int, CheckableLocation> PrintCheckableLocations(InstanceContainer container, List<object> objects, string filter)
        {
            List<CheckableLocation> checkableLocations = objects.Where(x => x is CheckableLocation).Cast<CheckableLocation>().ToList();
            checkableLocations = checkableLocations.Where(x => SearchStringParser.FilterSearch(container.Instance, x, filter, x.GetName())).ToList();
            Dictionary<int, CheckableLocation> Locations = checkableLocations.Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
            int Padding = Locations.Keys.Max().ToString().Length;
            foreach (var LocationObject in Locations)
            {
                Console.WriteLine($"{LocationObject.Key.ToString($"D{Padding}")}: [{LocationObject.Value.RandomizedState}] {LocationObject.Value.GetName()}");
            }
            return Locations;
        }

        private void LoopTrickEdit(InstanceData.InstanceContainer container)
        {
            string Filter = "";
            while (true)
            {
                Console.Clear();
                var TrickList = PrintTricks(container, Filter);
                string ParsedInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(ParsedInput)) { return; }

                if (ParsedInput.StartsWith('\\')) { Filter = ParsedInput[1..]; continue; }

                var InputData = new InputPrefixData(ParsedInput);

                foreach (var Index in InputData.Indexes)
                {
                    if (!TrickList.ContainsKey(Index)){ continue; }
                    if (InputData.Prefixes.Contains('!')) { TrickList[Index].TrickEnabled = false; }
                    else if (InputData.Prefixes.Contains('@')) { TrickList[Index].TrickEnabled = true; }
                    else { TrickList[Index].TrickEnabled = !TrickList[Index].TrickEnabled; }
                }
            }
        }
        private Dictionary<int, MacroObject> PrintTricks(InstanceContainer container, string filter)
        {
            IEnumerable<MacroObject> Tricks = container.Instance.MacroPool.Values.Where(x => x.isTrick())??[];
            Tricks = Tricks.Where(x => SearchStringParser.FilterSearch(container.Instance, x, filter, x.GetName())).ToList();
            Dictionary<int, MacroObject> Locations = Tricks.Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
            int Padding = Locations.Keys.Max().ToString().Length;
            foreach (var LocationObject in Locations)
            {
                Console.WriteLine($"{LocationObject.Key.ToString($"D{Padding}")}: [{LocationObject.Value.TrickEnabled}] {LocationObject.Value.GetName()}");
            }
            return Locations;
        }
    }
}
