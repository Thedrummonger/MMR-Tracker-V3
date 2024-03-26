using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;

namespace CLIFrontEnd
{
    internal class LocationChecking
    {
        public static void SetPrice(CheckableLocation entry)
        {
            entry.GetPrice(out int p, out _);
            if (p > -1) { entry.SetPrice(-1); return; }
            Console.Clear();
            while (p == -1)
            {
                Console.WriteLine($"Enter Price for {entry.GetName()}");
                var input = Console.ReadLine() ?? "";
                if (int.TryParse(input, out int newPrice) && newPrice > -1) { entry.SetPrice(newPrice); }
                else { Console.WriteLine($"{input} is not a valid price. Price must be a positive number"); }

                entry.GetPrice(out p, out _);
            }
        }

        public static List<MiscData.ManualCheckObjectResult> HandleUnAssignedLocations(IEnumerable<object> CheckObject, InstanceData.InstanceContainer Instance)
        {
            List<MiscData.ManualCheckObjectResult> Result = [];
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
                if (i is OptionData.ChoiceOption CO)
                {
                    Result.Add(LoopChoiceOptionSelect(CO));
                }
            }
            return Result;
        }

        public static List<MiscData.ManualCheckObjectResult> HandleUnAssignedVariables(IEnumerable<object> CheckObject, InstanceData.InstanceContainer Instance)
        {
            List<MiscData.ManualCheckObjectResult> Result = [];
            foreach (var i in CheckObject)
            {
                if (i is HintData.HintObject HO)
                {
                    Result.Add(LoopHintSelect(HO));
                }
                else if (i is OptionData.IntOption IO)
                {
                    int? Val = null;
                    while (Val is null)
                    {
                        Console.Clear();
                        Console.WriteLine($"Enter value for {IO.getOptionName()} (Current: {IO.Value}) (Range: {IO.Min} - {IO.Max})");
                        if (int.TryParse(Console.ReadLine(), out int SelectedVal) && SelectedVal <= IO.Max && SelectedVal >= IO.Min) { Val = SelectedVal; }
                    }
                    Result.Add(new MiscData.ManualCheckObjectResult().SetIntOption(IO, (int)Val));
                }
            }
            return Result;
        }
        private static MiscData.ManualCheckObjectResult LoopItemSelect(LocationData.LocationObject Location)
        {
            string Filter = "";
            while (true)
            {
                Console.Clear();
                var ValidItems = Location.GetValidItems(Filter);
                Dictionary<int, ItemData.ItemObject> Items = ValidItems.Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
                PrintItems(Items);
                Console.WriteLine(CLIUtility.CreateDivider());
                Console.WriteLine("Select Item at " + Location.GetDictEntry().GetName());
                var input = Console.ReadLine() ?? "";
                if (int.TryParse(input, out int index) && Items.TryGetValue(index, out ItemData.ItemObject? value))
                {
                    return new MiscData.ManualCheckObjectResult().SetItemLocation(Location, value.ID);
                }
                else if (input.StartsWith(@"\"))
                {
                    Filter = input[1..];
                }
            }
        }
        private static MiscData.ManualCheckObjectResult LoopEntranceSelect(EntranceData.EntranceRandoExit Exit)
        {
            var Instance = Exit.GetParent();
            string Filter = "";
            while (true)
            {
                Console.Clear();
                Dictionary<int, EntranceData.EntranceRandoDestination> EnteredItems = Instance.GetAllLoadingZoneDestinations(Filter)
                    .Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
                PrintItems(EnteredItems);
                Console.WriteLine(CLIUtility.CreateDivider());
                Console.WriteLine("Select Destination at Exit " + Exit.GetParentArea().ID + " -> " + Exit.ExitID);
                var input = Console.ReadLine() ?? "";
                if (int.TryParse(input, out int index) && EnteredItems.TryGetValue(index, out EntranceData.EntranceRandoDestination? value))
                {
                    return new MiscData.ManualCheckObjectResult().SetExitDestination(Exit, value);
                }
                else if (input.StartsWith(@"\"))
                {
                    Filter = input[1..];
                }
            }
        }

        private static MiscData.ManualCheckObjectResult LoopChoiceOptionSelect(OptionData.ChoiceOption Option)
        {
            string Filter = "";
            while (true)
            {
                Console.Clear();
                var ValidItems = Option.ValueList.Values;
                Dictionary<int, OptionData.OptionValue> Items = ValidItems.Select((s, index) => new { s, index }).ToDictionary(x => x.index + 1, x => x.s);
                PrintItems(Items);
                Console.WriteLine(CLIUtility.CreateDivider());
                Console.WriteLine("Select Value for " + Option.getOptionName() + $" Current: {Option.Value}");
                var input = Console.ReadLine() ?? "";
                if (int.TryParse(input, out int index) && Items.TryGetValue(index, out OptionData.OptionValue? value))
                {
                    return new MiscData.ManualCheckObjectResult().SetChoiceOption(Option, value.ID);
                }
                else if (input.StartsWith(@"\"))
                {
                    Filter = input[1..];
                }
            }
        }
        private static MiscData.ManualCheckObjectResult LoopHintSelect(HintData.HintObject HintSpot)
        {
            Console.Clear();
            Console.WriteLine($"Enter Hint at {HintSpot.ID}");
            string hint = Console.ReadLine();
            return new MiscData.ManualCheckObjectResult().SetGossipHint(HintSpot, hint);
        }
        public static void PrintItems<T>(Dictionary<int, T> Items)
        {
            if (Items.Count == 0) { return; }
            int Padding = Items.Keys.Max().ToString().Length;
            foreach (var i in Items)
            {
                Console.WriteLine($"{i.Key.ToString($"D{Padding}")}: {i.Value}");
            }
        }
    }
}
