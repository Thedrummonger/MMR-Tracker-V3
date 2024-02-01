using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    break;
                case PoolEditType.EntrancePool:
                    break;
                case PoolEditType.HintPool:
                    break;
                case PoolEditType.StartingItemPool:
                    break;
                case PoolEditType.TrickPool:
                    LoopTrickEdit(container);
                    break;
            }
        }

        private void LoopTrickEdit(InstanceData.InstanceContainer container)
        {
            while (true)
            {
                Console.Clear();
                var TrickList = PrintTricks(container);
                string ParsedInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(ParsedInput)) { return; }
                var Indexes = InputParser.ParseIndicesString(ParsedInput);
                foreach(var Index in Indexes)
                {
                    if (!TrickList.ContainsKey(Index)){ continue; }
                    TrickList[Index].TrickEnabled = !TrickList[Index].TrickEnabled;
                }
            }
        }
        private Dictionary<int, MacroObject> PrintTricks(InstanceContainer container)
        {
            IEnumerable<MacroObject> Tricks = container.Instance.MacroPool.Values.Where(x => x.isTrick())??[];
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
