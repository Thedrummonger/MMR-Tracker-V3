using MMR_Tracker_V3.TrackerObjectExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.SpoilerLogImporter
{
    internal class LASSpoilerLogTools
    {
        public static void ParseSpoiler(TrackerObjects.InstanceData.TrackerInstance Instance)
        {
            var Items = GetSection(Instance.SpoilerLog.Log, "", "settings:");
            Items = Items.Where(x => x.Contains(":") && !string.IsNullOrWhiteSpace(x.Split(':')[1]) ).ToList();
            Utility.PrintObjectToConsole(Items);
            var Settings = GetSection(Instance.SpoilerLog.Log, "settings:", "dungeon-entrances:");
            var DungeonEntrances = GetSection(Instance.SpoilerLog.Log, "dungeon-entrances:", "excluded-locations:");
            var ExcludedLocations = GetSection(Instance.SpoilerLog.Log, "excluded-locations:", "starting-items:");
            var StartingItems = GetSection(Instance.SpoilerLog.Log, "starting-items:", "");

            foreach(var I in Items)
            {
                var Data = I.TrimSplit(":");
                var location = Instance.LocationPool[Data[0]];
                if (!string.IsNullOrWhiteSpace(location.SingleValidItem)) { continue; }
                var Item = Instance.GetItemToPlace(Data[1], false, CheckItemID: true);
                if (location is null) { throw new Exception($"{Data[0]} Was not a valid Location"); }
                if (Data[1] == "trap") { location.Randomizeditem.SpoilerLogGivenItem = "Trap"; continue; }
                if (Item is null)
                {
                    if (Instance.ItemPool.ContainsKey(Data[1])) { throw new Exception($"No More {Data[1]} could be placed"); }
                    else { throw new Exception($"{Data[1]} Was not a valid Item"); }
                }
                Debug.WriteLine($"{location.GetName()} assigned to {Item.GetDictEntry().GetName()}");
                location.Randomizeditem.SpoilerLogGivenItem = Item.ID;
            }
            foreach (var D in DungeonEntrances)
            {
                var Data = D.TrimSplit(" -> ");
                var DungeonEntrance = Instance.LocationPool[Data[0]];
                var DungeonExit = Instance.ItemPool[Data[1]];
                DungeonEntrance.Randomizeditem.SpoilerLogGivenItem = DungeonExit.ID;
            }
            foreach (var E in ExcludedLocations)
            {
                Instance.LocationPool[E.Trim()].SetRandomizedState(TrackerObjects.MiscData.RandomizedState.ForcedJunk);
            }
            foreach (var S in StartingItems)
            {
                Instance.GetItemToPlace(S, false, CheckItemID: true);
            }
            Dictionary<string, string> Options = [];
            foreach (var D in Settings)
            {
                var Data = D.TrimSplit(":");
                Options.Add(Data[0].Trim(), Data[1].Trim());
            }
            foreach(var t in Instance.ToggleOptions)
            {
                if (!Options.ContainsKey(t.Key)) { Debug.WriteLine($"{t.Key} was not found in log"); continue; }
                t.Value.SetValue(bool.Parse(Options[t.Key]));
            }
            foreach (var t in Instance.ChoiceOptions)
            {
                if (!Options.ContainsKey(t.Key)) { Debug.WriteLine($"{t.Key} was not found in log"); continue; }
                t.Value.SetValue(Options[t.Key]);
            }
        }

        public static List<string> GetSection(IEnumerable<string> Log, string Start, string End)
        {
            bool AtSection = String.IsNullOrWhiteSpace(Start);
            List<string> Section = new List<string>();
            foreach(string s in Log)
            {
                if (!AtSection && s.StartsWith(Start)) { AtSection = true; continue; }
                if (!AtSection) { continue; }
                if (AtSection && !string.IsNullOrWhiteSpace(End) && s.StartsWith(End)) { break; }
                if (string.IsNullOrWhiteSpace(s)) { continue; }
                Section.Add(s);
            }
            return Section;
        }
    }
}
