using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.datamodel;

namespace MMR_Tracker_V3.SpoilerLogImporter
{
    internal class OOTMMSpoilerLogTools
    {
        public static void readAndApplySpoilerLog(InstanceData.TrackerInstance Instance)
        {
            ResetInstanceData(Instance);
            Dictionary<string, string> LocationData = GetDictionaryFromSpoiler(Instance, "Location List", true);
            Dictionary<string, string> SettingData = GetDictionaryFromSpoiler(Instance, "Settings");
            Dictionary<string, int> StartingItemData = GetDictionaryFromSpoiler(Instance, "Starting Items").ToDictionary(x => x.Key, x => int.TryParse(x.Value, out int SIC) ? SIC : -1);
            Dictionary<string, string> ExitData = GetEntranceListFromSpoiler(Instance, "Entrances");
            List<string> HintData = GetListFromSpoiler(Instance, "Hints");
            List<string> TrickData = GetListFromSpoiler(Instance, "Tricks").Concat(GetListFromSpoiler(Instance, "Glitches")).ToList();
            List<string> JunkLocationData = GetListFromSpoiler(Instance, "Junk Locations");
            List<string> MQDungeons = GetListFromSpoiler(Instance, "MQ Dungeons");
            List<string> AccessConditions = GetListFromSpoiler(Instance, "Special Conditions");

            Debug.WriteLine("\nApplying Settings");
            ApplySettings(Instance, SettingData);
            Debug.WriteLine("\nApplying Master Quest Data");
            HandleMQChecks(Instance, MQDungeons);
            Debug.WriteLine("\nToggling Tricks");
            ToggleTricks(Instance, TrickData);
            Debug.WriteLine("\nApplying Starting Items");
            SetStartingItems(Instance, StartingItemData);
            Debug.WriteLine("\nPlacing Items at locations");
            PlaceItemsAtLocations(Instance, LocationData);
            Debug.WriteLine("\nApplying Hints");
            ApplyHintData(Instance, ParseHintData(Instance, HintData));
            Debug.WriteLine("\nApplying Junk Locations");
            SetjunkedLocations(Instance, JunkLocationData);
            Debug.WriteLine("\nApplying Entrance Data");
            ApplyEntrances(Instance, ExitData);

            Debug.WriteLine("\nSetting Special Conditions");
            ToggleSpecialConditions(Instance, AccessConditions);

        }

        private static void ApplyEntrances(InstanceData.TrackerInstance instance, Dictionary<string, string> exitData)
        {
            var AllRandomizableExits = instance.EntrancePool.AreaList.Values.SelectMany(x => x.RandomizableExits(instance));
            foreach (var Entrance in exitData)
            {
                var SpoilerEntrance = AllRandomizableExits.First(x => x.Value.GetDictEntry(instance).SpoilerData.SpoilerLogNames.Contains(Entrance.Key));
                var SpoilerExit = AllRandomizableExits.First(x => x.Value.GetDictEntry(instance).SpoilerData.SpoilerLogNames.Contains(Entrance.Value));
                SpoilerEntrance.Value.RandomizedState = MiscData.RandomizedState.Randomized;
                SpoilerEntrance.Value.SpoilerDefinedDestinationExit = new EntranceData.EntranceRandoDestination { region = SpoilerExit.Value.ID, from = SpoilerExit.Value.ParentAreaID };
            }
        }

        private static void HandleMQChecks(InstanceData.TrackerInstance instance, List<string> mQDungeons)
        {
            foreach (var i in instance.Variables.Where(x => x.Value.SubCategory == "Master Quest Dungeons"))
            {
                bool ISMQ = mQDungeons.Any(x => i.Key.ToLower().StartsWith(x.ToLower()));
                i.Value.Value = ISMQ;

                string DungeonCode = i.Key[..^2];

                Debug.WriteLine($"{DungeonCode} MQ {ISMQ}");
                string OppositeLogic = ISMQ ? $"var{{{i.Key}, false}}" : $"var{{{i.Key}}}";
                var JunkChecks = instance.LocationPool.Values.Where(x => instance.GetLogic(x.ID).RequiredItems.Contains(OppositeLogic));
                foreach (var Check in JunkChecks)
                {
                    Check.SetRandomizedState(MiscData.RandomizedState.ForcedJunk, instance);
                }
            }
        }

        private static void SetjunkedLocations(InstanceData.TrackerInstance instance, List<string> junkLocationData)
        {
            foreach (var i in junkLocationData)
            {
                if (!instance.LocationPool.ContainsKey(i)) { Debug.WriteLine($"{i} Was not a valid Location"); continue; }
                instance.LocationPool[i].SetRandomizedState(MiscData.RandomizedState.ForcedJunk, instance);
            }
        }

        private static void ToggleTricks(InstanceData.TrackerInstance instance, List<string> trickData)
        {
            var allTricks = instance.MacroPool.Values.Where(x => x.isTrick(instance)).ToArray();
            foreach (var i in instance.MacroPool.Values.Where(x => x.isTrick(instance))) { i.TrickEnabled = false; }
            foreach (var i in trickData)
            {
                var CurrentTrick = allTricks.FirstOrDefault(x => x.GetDictEntry(instance).Name == i);
                if (CurrentTrick is null) { Debug.WriteLine($"{i} is not a valid trick"); }
                else { CurrentTrick.TrickEnabled = true; }
            }
        }

        private static void PlaceItemsAtLocations(InstanceData.TrackerInstance instance, Dictionary<string, string> LocationData)
        {
            List<LocationData.LocationObject> LocationFilled = new List<LocationData.LocationObject>();
            foreach (var i in LocationData)
            {
                var location = instance.GetLocationByID(i.Key);
                string ItemName = instance.ItemPool.ContainsKey(i.Value) ? instance.ItemPool[i.Value].GetDictEntry(instance).SpoilerData.SpoilerLogNames.First() : i.Value;
                var Item = instance.GetItemToPlace(ItemName);
                string ItemID;
                if (location is null)
                {
                    Debug.WriteLine($"Location {i.Key} is not valid");
                    continue;
                }
                else if (Item is null)
                {
                    var ValidItems = instance.ItemPool.Values.Where(x => x.GetDictEntry(instance).SpoilerData.SpoilerLogNames.Contains(ItemName));
                    if (ValidItems.Any())
                    {
                        Debug.WriteLine($"Item {i.Value} has been placed more times than is allowed!");
                        ItemID = ValidItems.First().Id;
                    }
                    else
                    {
                        Debug.WriteLine($"Item {i.Value} Is not a valid Item!");
                        ItemID = i.Value;
                        continue;
                    }
                }
                else
                {
                    ItemID = Item.Id;
                }
                location.Randomizeditem.SpoilerLogGivenItem = ItemID;
                LocationFilled.Add(location);
            }
            foreach (var i in instance.LocationPool.Values)
            {
                if (string.IsNullOrWhiteSpace(i.Randomizeditem.SpoilerLogGivenItem) && i.RandomizedState == MiscData.RandomizedState.Randomized)
                {
                    Debug.WriteLine($"{i.ID} was randomized but had no spoiler data. Assuming it's unrandomized");
                    if (i.SingleValidItem is not null) { i.SetRandomizedState(MiscData.RandomizedState.UnrandomizedManual, instance); }
                    else { i.SetRandomizedState(MiscData.RandomizedState.Unrandomized, instance); }
                }
            }
        }

        private static void ToggleSpecialConditions(InstanceData.TrackerInstance instance, List<string> AccessConditions)
        {
            string Section = "";
            foreach (var i in AccessConditions)
            {
                if (i == "BRIDGE:") { Section = "bridge"; continue; }
                if (i == "MOON:") { Section = "moon"; continue; }
                if (i == "LACS:") { Section = "lacs"; continue; }
                if (i == "GANON_BK:") { Section = "ganon_bk"; continue; }
                if (i == "MAJORA:") { Section = "majora"; continue; }
                if (string.IsNullOrWhiteSpace(i)) { return; }
                var Data = i.Split(':').Select(x => x.Trim()).ToArray();
                string ID = $"{Section}_{Data[0]}";
                if (instance.UserOptions.ContainsKey(ID)) { instance.UserOptions[ID].CurrentValue = Data[1]; }
                else if (instance.Variables.ContainsKey(ID))
                {
                    if (int.TryParse(Data[1], out int count)) { instance.Variables[ID].Value = count; }
                    else { instance.Variables[ID].Value = Data[1]; }
                }
                else { Debug.WriteLine($"{ID} was not an option or variable"); }
            }

        }

        private static void ApplySettings(InstanceData.TrackerInstance instance, Dictionary<string, string> settingData)
        {
            foreach (var setting in settingData)
            {
                if (instance.UserOptions.ContainsKey(setting.Key) && instance.UserOptions[setting.Key].Values.ContainsKey(setting.Value))
                {
                    instance.UserOptions[setting.Key].CurrentValue = setting.Value;
                }
                else if (instance.Variables.ContainsKey(setting.Key))
                {
                    if (bool.TryParse(setting.Value, out bool BoolVal)) { instance.Variables[setting.Key].Value = BoolVal; }
                    else if (long.TryParse(setting.Value, out long IntVal)) { instance.Variables[setting.Key].Value = IntVal; }
                    else { instance.Variables[setting.Key].Value = setting.Value; }
                }
                else
                {
                    if (instance.UserOptions.ContainsKey(setting.Key)) { Debug.WriteLine($"Setting {setting.Key} did not have option {setting.Value}"); }
                    else { Debug.WriteLine($"Setting {setting.Key} did not exist"); }
                }

                if (setting.Key == "mapCompassShuffle" && setting.Value == "starting")
                {
                    var MapsCompasses = instance.ItemPool.Values.Where(x =>
                    x.Id.StartsWith("MM_MAP_") ||
                    x.Id.StartsWith("OOT_MAP_") ||
                    x.Id.StartsWith("MM_COMPASS_") ||
                    x.Id.StartsWith("OOT_COMPASS_"));
                    foreach (var i in MapsCompasses) { i.AmountInStartingpool = 1; }
                }
                if (setting.Key == "tingleShuffle" && setting.Value == "starting")
                {
                    var MapsCompasses = instance.ItemPool.Values.Where(x => x.Id.StartsWith("MM_WORLD_MAP_"));
                    foreach (var i in MapsCompasses) { i.AmountInStartingpool = 1; }
                }
                if (setting.Key == "strayFairyChestShuffle" && setting.Value == "starting")
                {
                    instance.ItemPool.Values.First(x => x.Id == "MM_STRAY_FAIRY_WF").AmountInStartingpool += 3;
                    instance.ItemPool.Values.First(x => x.Id == "MM_STRAY_FAIRY_SH").AmountInStartingpool += 7;
                    instance.ItemPool.Values.First(x => x.Id == "MM_STRAY_FAIRY_GB").AmountInStartingpool += 6;
                    instance.ItemPool.Values.First(x => x.Id == "MM_STRAY_FAIRY_ST").AmountInStartingpool += 15;
                }
                if (setting.Key == "strayFairyOtherShuffle" && (setting.Value == "starting" || setting.Value == "removed"))
                {
                    instance.ItemPool.Values.First(x => x.Id == "MM_STRAY_FAIRY_WF").AmountInStartingpool += 12;
                    instance.ItemPool.Values.First(x => x.Id == "MM_STRAY_FAIRY_SH").AmountInStartingpool += 8;
                    instance.ItemPool.Values.First(x => x.Id == "MM_STRAY_FAIRY_GB").AmountInStartingpool += 9;
                }
                if (setting.Key == "shufflePotsOot" && setting.Value == "false")
                {
                    foreach (var location in instance.LocationPool.Where(x => x.Value.GetDictEntry(instance).ValidItemTypes.Contains("pot") && x.Key.StartsWith("OOT ")))
                    {
                        location.Value.SetRandomizedState(MiscData.RandomizedState.Unrandomized, instance);
                    }
                }
                if (setting.Key == "shufflePotsMm" && setting.Value == "false")
                {
                    foreach (var location in instance.LocationPool.Where(x => x.Value.GetDictEntry(instance).ValidItemTypes.Contains("pot") && x.Key.StartsWith("MM ")))
                    {
                        location.Value.SetRandomizedState(MiscData.RandomizedState.Unrandomized, instance);
                    }
                }
                if (setting.Key == "shuffleGrassOot" && setting.Value == "false")
                {
                    foreach (var location in instance.LocationPool.Where(x => x.Value.GetDictEntry(instance).ValidItemTypes.Contains("grass") && x.Key.StartsWith("OOT ")))
                    {
                        location.Value.SetRandomizedState(MiscData.RandomizedState.Unrandomized, instance);
                    }
                }
                if (setting.Key == "shuffleGrassMm" && setting.Value == "false")
                {
                    foreach (var location in instance.LocationPool.Where(x => x.Value.GetDictEntry(instance).ValidItemTypes.Contains("grass") && x.Key.StartsWith("MM ")))
                    {
                        location.Value.SetRandomizedState(MiscData.RandomizedState.Unrandomized, instance);
                    }
                }
            }
        }

        private static void ResetInstanceData(InstanceData.TrackerInstance Instance)
        {
            Instance.ToggleAllTricks(false);
            foreach (var i in Instance.EntrancePool.AreaList.SelectMany(x => x.Value.RandomizableExits(Instance)))
            {
                i.Value.RandomizedState = MiscData.RandomizedState.Unrandomized;
            }
            foreach (var i in Instance.LocationPool.Values)
            {
                i.SetRandomizedState(MiscData.RandomizedState.Randomized, Instance);
            }
            foreach (var i in Instance.ItemPool.Values)
            {
                i.AmountInStartingpool = 0;
            }
        }

        private static Dictionary<string, string> GetDictionaryFromSpoiler(InstanceData.TrackerInstance Instance, string Start, bool Fuzzy = false)
        {
            var Log = Instance.SpoilerLog.Log;
            bool AtReleventData = false;
            var SpoilerData = new Dictionary<string, string>();
            foreach (var x in Log)
            {
                if (x.Trim() == Start || Fuzzy && x.Trim().StartsWith(Start)) { AtReleventData = true; continue; }
                if (AtReleventData && !x.StartsWith(" ") && !string.IsNullOrWhiteSpace(x)) { break; }
                if (!AtReleventData || string.IsNullOrWhiteSpace(x)) { continue; }
                var LineParts = x.Split(':').Select(x => x.Trim()).ToArray();
                if (LineParts.Count() > 1 && !string.IsNullOrWhiteSpace(LineParts[1]))
                {
                    SpoilerData[LineParts[0]] = LineParts[1];
                }
            }
            return SpoilerData;
        }

        private static Dictionary<string, string> GetEntranceListFromSpoiler(InstanceData.TrackerInstance Instance, string Start)
        {
            var Log = Instance.SpoilerLog.Log;
            bool AtReleventData = false;
            var SpoilerData = new Dictionary<string, string>();
            foreach (var x in Log)
            {
                if (x.Trim() == Start) { AtReleventData = true; continue; }
                if (AtReleventData && !x.StartsWith(" ") && !string.IsNullOrWhiteSpace(x)) { break; }
                if (!AtReleventData || string.IsNullOrWhiteSpace(x)) { continue; }
                var LineParts = x.StringSplit("->").Select(x => x.Trim()).ToArray();
                if (LineParts.Count() > 1 && !string.IsNullOrWhiteSpace(LineParts[1]))
                {
                    SpoilerData[LineParts[0]] = LineParts[1];
                }
            }
            return SpoilerData;
        }

        private static List<string> GetListFromSpoiler(InstanceData.TrackerInstance Instance, string Start)
        {
            var Log = Instance.SpoilerLog.Log;
            bool AtReleventData = false;
            var EnabledTricks = new List<string>();
            foreach (var x in Log)
            {
                if (x.Trim() == Start) { AtReleventData = true; continue; }
                if (AtReleventData && !x.StartsWith(" ") && !string.IsNullOrWhiteSpace(x)) { break; }
                if (!AtReleventData || string.IsNullOrWhiteSpace(x)) { continue; }
                EnabledTricks.Add(x.Trim());
            }
            return EnabledTricks;
        }

        private static void SetStartingItems(InstanceData.TrackerInstance instance, Dictionary<string, int> startingItemData)
        {
            foreach (var i in startingItemData)
            {
                if (i.Value < 1) { continue; }
                var item = instance.ItemPool.Values.FirstOrDefault(x => x.Id == i.Key || x.GetDictEntry(instance).SpoilerData.SpoilerLogNames.Contains(i.Key));
                if (item is null)
                {
                    Debug.WriteLine($"{i.Key} is not a valid item");
                    continue;
                }
                for (var c = 0; c < i.Value; c++)
                {
                    if (instance.GetItemToPlace(item.Id) is not null)
                    {
                        item.AmountInStartingpool++;
                    }
                    else
                    {
                        Debug.WriteLine($"Could not place anymore {i.Key}");
                        break;
                    }
                }
            }
        }
        private static void ApplyHintData(InstanceData.TrackerInstance instance, Dictionary<string, SpoilerHintData> hintData)
        {
            foreach (var i in hintData)
            {
                if (!instance.HintPool.ContainsKey(i.Key)) { Debug.WriteLine($"{i.Key} Was not a valid hint locations!"); continue; }
                switch (i.Value.HintType)
                {
                    case HintType.ItemExact:
                        instance.HintPool[i.Key].SpoilerHintText = $"{i.Value.PrettyLocationText} contains {string.Join(" and ", i.Value.HintedItemNames)}";
                        if (i.Value.HintedLocations.Length == i.Value.HintedItems.Length)
                        {
                            for (var hintind = 0; hintind < i.Value.HintedLocations.Length; hintind++)
                            {
                                instance.HintPool[i.Key].ParsedHintData.Add(i.Value.HintedLocations[hintind], i.Value.HintedItems[hintind]);
                            }
                        }
                        break;
                    case HintType.ItemRegion:
                        instance.HintPool[i.Key].SpoilerHintText = $"{i.Value.PrettyLocationText} contains {string.Join(" and ", i.Value.HintedItems)}";
                        break;
                    case HintType.Hero:
                        instance.HintPool[i.Key].SpoilerHintText = $"{i.Value.PrettyLocationText} is Way of the Hero";
                        break;
                    case HintType.Foolish:
                        instance.HintPool[i.Key].SpoilerHintText = $"{i.Value.PrettyLocationText} is Foolish";
                        break;
                }
            }
            foreach (var i in instance.HintPool)
            {
                if (string.IsNullOrWhiteSpace(i.Value.SpoilerHintText))
                {
                    Debug.WriteLine($"Hint {i.Key} has no spoiler data, assuming junk");
                    i.Value.RandomizedState = MiscData.RandomizedState.ForcedJunk;
                }
            }
        }
        private static Dictionary<string, SpoilerHintData> ParseHintData(InstanceData.TrackerInstance Instance, List<string> hintData)
        {
            HintType CurrentType = HintType.none;
            List<string> HeroHints = new();
            List<string> FoolishHints = new();
            List<string> ExactHints = new();
            List<string> RegionHints = new();
            Dictionary<string, SpoilerHintData> Result = new();

            foreach (var x in hintData)
            {
                if (string.IsNullOrWhiteSpace(x)) { continue; }
                else if (x == "Way of the Hero:") { CurrentType = HintType.Hero; continue; }
                else if (x == "Foolish:") { CurrentType = HintType.Foolish; continue; }
                else if (x == "Specific Hints:") { CurrentType = HintType.ItemExact; continue; }
                else if (x == "Regional Hints:") { CurrentType = HintType.ItemRegion; continue; }
                else if (x == "Foolish Regions:") { CurrentType = HintType.none; continue; }
                else if (CurrentType == HintType.Hero) { HeroHints.Add(x); }
                else if (CurrentType == HintType.Foolish) { FoolishHints.Add(x); }
                else if (CurrentType == HintType.ItemExact) { ExactHints.Add(x); }
                else if (CurrentType == HintType.ItemRegion) { RegionHints.Add(x); }
            }
            string CurrentKey = "";
            foreach (var x in HeroHints)
            {
                if (CurrentKey == "") { CurrentKey = x; }
                else
                {
                    Result[CurrentKey] = new SpoilerHintData { HintType = HintType.Hero, PrettyLocationText = $"{x.StringSplit("    ")[0]}" };
                    CurrentKey = "";
                }
            }
            CurrentKey = "";
            foreach (var x in FoolishHints)
            {
                if (CurrentKey != "")
                {
                    Result[CurrentKey] = new SpoilerHintData { HintType = HintType.Foolish, PrettyLocationText = $"{x}" };
                    CurrentKey = "";
                }
                else { CurrentKey = x; }
            }
            CurrentKey = "";
            foreach (var x in RegionHints)
            {
                if (CurrentKey != "")
                {
                    Debug.WriteLine($"Hint Data {x}");
                    var data = x.Split(new[] { '(' }, 2);
                    data[1] = data[1][..^1];
                    Result[CurrentKey] = new SpoilerHintData { HintType = HintType.Hero, PrettyLocationText = data[0], HintedItems = data[1].Split(",") };
                    CurrentKey = "";
                }
                else { CurrentKey = x; }
            }
            CurrentKey = "";
            foreach (var x in ExactHints)
            {
                if (CurrentKey != "")
                {
                    var data = x.Split(new[] { '(' }, 2);
                    data[1] = data[1][..^1];
                    var Locations = data[0].Split(',').Select(x => x.Trim()).ToArray();
                    var Items = data[1].Split(',').Select(x => x.Trim()).ToArray();
                    List<string> ItemIDs = new List<string>();
                    List<string> ItemNames = new List<string>();
                    foreach (var item in Items)
                    {
                        var ValidItem = Instance.ItemPool.Values.FirstOrDefault(x => x.GetDictEntry(Instance).SpoilerData.SpoilerLogNames.Contains(item));
                        ItemIDs.Add(ValidItem is null ? item : ValidItem.Id);
                    }
                    foreach (var item in Items)
                    {
                        ItemNames.Add(Instance.ItemPool.ContainsKey(item) ? Instance.ItemPool[item].GetDictEntry(Instance).GetName(Instance) : item);
                    }
                    Dictionary<string, string> HintedEntries = Locations.Zip(Items, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
                    Result[CurrentKey] = new SpoilerHintData
                    {
                        HintType = HintType.ItemExact,
                        PrettyLocationText = $"{string.Join("and ", Locations)}",
                        HintedLocations = Locations,
                        HintedItemNames = ItemNames.ToArray(),
                        HintedItems = ItemIDs.ToArray()
                    };
                    CurrentKey = "";
                }
                else { CurrentKey = x; }
            }
            return Result;
        }
    }
}
