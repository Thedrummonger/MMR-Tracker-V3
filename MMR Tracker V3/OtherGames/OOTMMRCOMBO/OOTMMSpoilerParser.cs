using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using MathNet.Numerics;
using System.Runtime.CompilerServices;

namespace MMR_Tracker_V3.OtherGames.OOTMMRCOMBO
{
	internal class OOTMMSpoilerParser
	{
        public static string Hintnamedata = "{\"OOT RAVAGED_VILLAGE\":[\"OOT Kakariko Song Shadow\"],\"OOT FROGS_FINAL\":[\"OOT Zora River Frogs Game\"],\"OOT ZORA_KING\":[\"OOT Zora Domain Tunic\",\"OOT Zora Domain Eyeball Frog\"],\"OOT FISHING\":[\"OOT Fishing Pond Child\",\"OOT Fishing Pond Adult\"],\"OOT GERUDO_ARCHERY\":[\"OOT Gerudo Fortress Archery Reward 1\",\"OOT Gerudo Fortress Archery Reward 2\"],\"OOT HAUNTED_WASTELAND\":[\"OOT Haunted Wasteland Chest\"],\"OOT GANON_FAIRY\":[\"OOT Great Fairy Defense Upgrade\"],\"OOT TEMPLE_FIRE_HAMMER\":[\"OOT Fire Temple Hammer\"],\"OOT TEMPLE_FIRE_SCARECROW\":[\"OOT Fire Temple Scarecrow Chest\"],\"OOT GTG_WATER\":[\"OOT Gerudo Training Water\"],\"MM BANK_3\":[\"MM Clock Town Bank Reward 3\"],\"MM SOUND_CHECK\":[\"MM Milk Bar Troupe Leader Mask\"],\"MM COUPLE_MASK\":[\"MM Stock Pot Inn Couple's Mask\"],\"MM BOAT_ARCHERY\":[\"MM Tourist Information Boat Cruise\"],\"MM BUTLER_RACE\":[\"MM Deku Shrine Mask of Scents\"],\"MM DON_GERO_CHOIR\":[\"MM Mountain Village Frog Choir HP\"],\"MM GORON_RACE\":[\"MM Goron Race Reward\"],\"MM RANCH_DEFENSE\":[\"MM Romani Ranch Aliens\",\"MM Romani Ranch Cremia Escort\"],\"MM FISHERMAN_GAME\":[\"MM Great Bay Coast Fisherman HP\"],\"MM PINNACLE_ROCK_HP\":[\"MM Pinnacle Rock HP\"],\"MM OSH_CHEST\":[\"MM Ocean Spider House Chest HP\"],\"MM GRAVEYARD_NIGHT3\":[\"MM Beneath The Graveyard Dampe Chest\"],\"MM SONG_ELEGY\":[\"MM Ancient Castle of Ikana Song Emptiness\"],\"MM SECRET_SHRINE_WART_HP\":[\"MM Secret Shrine Boss 3 Chest\",\"MM Secret Shrine HP Chest\"]}";
        public enum HintType
        {
            ItemExact,
            ItemRegion,
            Hero,
            Foolish,
            none
        }
        public class SpoilerHintData
        {
            public HintType HintType;
            public string HintedLocationText;
            public string PrettyLocationText;
            public string[] HintedLocations;
            public string[] HintedItems;
        }
        public static void readAndApplySpoilerLog(LogicObjects.TrackerInstance Instance)
        {
            ResetInstanceData(Instance);
            Dictionary<string, string> LocationData = GetDictionaryFromSpoiler(Instance, "Location List");
            Dictionary<string, SpoilerHintData> HintData = GetDictionaryFromSpoiler(Instance, "Hints").ToDictionary(x => x.Key, x => ParseHintLine(x.Value));
            Dictionary<string, string> SettingData = GetDictionaryFromSpoiler(Instance, "Settings");
            Dictionary<string, int> StartingItemData = GetDictionaryFromSpoiler(Instance, "Starting Items").ToDictionary(x => x.Key, x => int.TryParse(x.Value, out int SIC) ? SIC : -1);
            Dictionary<string, string> ExitData = GetEntranceListFromSpoiler(Instance, "Entrances");
            List<string> TrickData = GetListFromSpoiler(Instance, "Tricks").Select(x => $"TRICK_{x}").ToList();
            List<string> JunkLocationData = GetListFromSpoiler(Instance, "Junk Locations");

            Debug.WriteLine("\nApplying Settings");
            ApplySpoilerSettings(Instance, SettingData);
            Debug.WriteLine("\nToggling Tricks");
            ToggleTricks(Instance, TrickData);
            Debug.WriteLine("\nApplying Starting Items");
            SetStartingItems(Instance, StartingItemData);
            Debug.WriteLine("\nPlacing Items at locations");
            PlaceItemsAtLocations(Instance, LocationData);
            Debug.WriteLine("\nApplying Junk Locations");
            SetjunkedLocations(Instance, JunkLocationData);
            Debug.WriteLine("\nApplying Hints");
            ApplyHintData(Instance, HintData);
            Debug.WriteLine("\nApplying Entrance Data");
            ApplyEntrances(Instance, ExitData);
        }

        private class EntranceDataContainer
        {
            public string Area { get; set; }
            public string Dest { get; set; }
        }

        private static void ApplyEntrances(LogicObjects.TrackerInstance instance, Dictionary<string, string> exitData)
        {
            foreach(var i in exitData)
            {
                EntranceDataContainer EntranceData = new() { Area = i.Key.Split('/')[0], Dest = i.Key.Split('/')[1] };
                EntranceDataContainer ExitData = new() { Area = i.Value.Split('/')[0], Dest =  i.Value.Split('/')[1] };

                TempNameFixing(EntranceData);
                TempNameFixing(ExitData);

                if (!instance.EntrancePool.AreaList.ContainsKey(EntranceData.Area))
                {
                    Debug.WriteLine($"Exit Area {EntranceData.Area} is not a valid Area");
                }
                else if (!instance.EntrancePool.AreaList[EntranceData.Area].RandomizableExits(instance).ContainsKey(EntranceData.Dest))
                {
                    Debug.WriteLine($"{EntranceData.Area} Does not contain a randomizable Entrance to {EntranceData.Dest}");
                }
                else if (!instance.EntrancePool.AreaList.ContainsKey(ExitData.Area))
                {
                    Debug.WriteLine($"Destination Area {ExitData.Area} is not a valid Area");
                    instance.EntrancePool.AreaList[EntranceData.Area].Exits[EntranceData.Dest].RandomizedState = MiscData.RandomizedState.Randomized;
                }
                else if (!instance.EntrancePool.AreaList[ExitData.Area].RandomizableExits(instance).ContainsKey(ExitData.Dest))
                {
                    Debug.WriteLine($"{ExitData.Dest} Does not contain a Randomizable exit from {ExitData.Area}");
                    instance.EntrancePool.AreaList[EntranceData.Area].Exits[EntranceData.Dest].RandomizedState = MiscData.RandomizedState.Randomized;
                }
                else
                {
                    instance.EntrancePool.AreaList[EntranceData.Area].Exits[EntranceData.Dest].RandomizedState = MiscData.RandomizedState.Randomized;
                    instance.EntrancePool.AreaList[EntranceData.Area].Exits[EntranceData.Dest].SpoilerDefinedDestinationExit = new EntranceData.EntranceRandoDestination
                    {
                        from = ExitData.Area,
                        region= ExitData.Dest,
                    };
                }
            }

            void TempNameFixing(EntranceDataContainer Data)
            {
                if (Data.Area == "OOT Fire Temple" && Data.Dest == "OOT Fire Temple Boss") { Data.Area = "OOT Fire Temple Pre-Boss"; }
                if (Data.Area == "OOT Jabu-Jabu Main" && Data.Dest == "OOT Jabu-Jabu Boss") { Data.Area = "OOT Jabu-Jabu Pre-Boss"; }
            }

        }

        private static void ApplyHintData(LogicObjects.TrackerInstance instance, Dictionary<string, SpoilerHintData> hintData)
        {
            foreach(var i in hintData)
            {
                if (!instance.HintPool.ContainsKey(i.Key)) { Debug.WriteLine($"{i.Key} Was not a valid hint locations!"); continue; }
                switch (i.Value.HintType)
                {
                    case HintType.ItemExact:
                        instance.HintPool[i.Key].HintText = $"{i.Value.PrettyLocationText} contains {string.Join(" and ", i.Value.HintedItems)}";
                        if (i.Value.HintedLocations.Length == i.Value.HintedItems.Length)
                        {
                            for (var hintind = 0; hintind < i.Value.HintedLocations.Length; hintind++)
                            {
                                instance.HintPool[i.Key].ParsedHintData.Add(i.Value.HintedLocations[hintind], i.Value.HintedItems[hintind]);
                            }
                        }
                        break;
                    case HintType.ItemRegion:
                        instance.HintPool[i.Key].HintText = $"{i.Value.PrettyLocationText} contains {string.Join(" and ", i.Value.HintedItems )}";
                        break;
                    case HintType.Hero:
                        instance.HintPool[i.Key].HintText = $"{i.Value.PrettyLocationText} is Way of the Hero";
                        break;
                    case HintType.Foolish:
                        instance.HintPool[i.Key].HintText = $"{i.Value.PrettyLocationText} is Foolish";
                        break;
                }
            }
        }

        private static void ToggleTricks(LogicObjects.TrackerInstance instance, List<string> trickData)
        {
            foreach(var i in instance.MacroPool.Values.Where(x => x.isTrick(instance)))
            {
                i.TrickEnabled = trickData.Contains(i.ID);
            }
        }

        private static void SetjunkedLocations(LogicObjects.TrackerInstance instance, List<string> junkLocationData)
        {
            foreach(var i in junkLocationData)
            {
                if (!instance.LocationPool.ContainsKey(i)) { Debug.WriteLine($"{i} Was not a valid Location"); continue; }
                instance.LocationPool[i].SetRandomizedState(MiscData.RandomizedState.ForcedJunk, instance);
            }
        }

        private static void PlaceItemsAtLocations(LogicObjects.TrackerInstance instance, Dictionary<string, string> LocationData)
        {
            List<LocationData.LocationObject> LocationFilled = new List<LocationData.LocationObject>();
            foreach(var i in LocationData)
            {
                var location = instance.GetLocationByID(i.Key);
                var Item = instance.GetItemToPlace(i.Value);
                string ItemID;
                if (location is null)
                {
                    Debug.WriteLine($"{i.Key} is not valid");
                    continue;
                }
                else if (Item is null)
                {
                    var ValidItems = instance.ItemPool.Values.Where(x => x.GetDictEntry(instance).SpoilerData.SpoilerLogNames.Contains(i.Value));
                    if (ValidItems.Any()) 
                    { 
                        Debug.WriteLine($"{i.Value} has been placed more times than is allowed!");
                        ItemID = ValidItems.First().Id;
                    }
                    else
                    {
                        Debug.WriteLine($"{i.Value} Is not a valid Item!");
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
            foreach(var i in instance.LocationPool.Values)
            {
                if (!LocationFilled.Contains(i))
                {
                    Debug.WriteLine($"{i.ID} was not found in the spoiler log. Assuming it's unrandomized");
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized, instance);
                }
            }
        }

        private static void SetStartingItems(LogicObjects.TrackerInstance instance, Dictionary<string, int> startingItemData)
        {
            foreach(var i in startingItemData)
            {
                if(i.Value < 1) { continue; }
                if (!instance.ItemPool.ContainsKey(i.Key))
                {
                    Debug.WriteLine($"{i.Key} is not a valid item");
                    continue;
                }
                for(var c = 0; c < i.Value; c++)
                {
                    if (instance.GetItemToPlace(i.Key) is not null)
                    {
                        instance.ItemPool[i.Key].AmountInStartingpool++;
                    }
                    else
                    {
                        Debug.WriteLine($"Could not place anymore {i.Key}");
                        break;
                    }
                }
            }
        }

        private static void ApplySpoilerSettings(LogicObjects.TrackerInstance instance, Dictionary<string, string> settingData)
        {
            foreach(var setting in settingData)
            {
                if (!instance.UserOptions.ContainsKey(setting.Key)) { continue; }
                else if (!instance.UserOptions[setting.Key].Values.ContainsKey(setting.Value)) { Debug.WriteLine($"Value {setting.Value} Was not Valid for setting {setting.Key}"); }
                else { instance.UserOptions[setting.Key].CurrentValue = setting.Value; }

                if (setting.Key == "mapCompassShuffle" && setting.Value == "starting")
                {
                    var MapsCompasses = instance.ItemPool.Values.Where(x =>
                    x.Id.StartsWith("MM_MAP_") ||
                    x.Id.StartsWith("OOT_MAP_") ||
                    x.Id.StartsWith("MM_COMPASS_") ||
                    x.Id.StartsWith("OOT_COMPASS_"));
                    foreach (var i in MapsCompasses) { i.AmountInStartingpool= 1; }
                }
            }
        }

        private static Dictionary<string, string> GetDictionaryFromSpoiler(LogicObjects.TrackerInstance Instance, string Start)
        {
            var Log = Instance.SpoilerLog.Log;
            bool AtReleventData = false;
            var SpoilerData = new Dictionary<string, string>();
            foreach (var x in Log)
            {
                if (x.Trim() == Start) { AtReleventData = true; continue; }
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

        private static Dictionary<string, string> GetEntranceListFromSpoiler(LogicObjects.TrackerInstance Instance, string Start)
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

        private static List<string> GetListFromSpoiler(LogicObjects.TrackerInstance Instance, string Start)
        {
            var Log = Instance.SpoilerLog.Log;
            bool AtReleventData = false;
            var EnabledTricks = new List<string>();
            foreach (var x in Log)
            {
                if (x.Trim() == Start) { AtReleventData = true; continue; }
                if (AtReleventData && !x.StartsWith(" ")) { break; }
                if (!AtReleventData || string.IsNullOrWhiteSpace(x)) { continue; }
                EnabledTricks.Add(x.Trim());
            }
            return EnabledTricks;
        }

        public static SpoilerHintData ParseHintLine(string line)
        {
            var HintNames = new Dictionary<string, string[]>();
            HintNames = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(Hintnamedata).ToDictionary(x => x.Key.Replace(" ", "_"), x => x.Value);
            SpoilerHintData result = new SpoilerHintData();
            string SpoilerHintType = line.Split(',')[0].Trim();
            result.HintType = OOTMMSpoilerParser.HintType.none;
            switch (SpoilerHintType)
            {
                case "Item-Exact":
                    result.HintType = HintType.ItemExact;
                    break;
                case "Item-Region":
                    result.HintType = HintType.ItemRegion;
                    break;
                case "Hero":
                    result.HintType = HintType.Hero;
                    break;
                case "Foolish":
                    result.HintType = HintType.Foolish;
                    break;
            }
            string LocationAnditemData = line.Replace($"{SpoilerHintType}, ", "");
            result.HintedLocationText = LocationAnditemData.Split(new[] { '(' }, 2)[0].Trim();
            result.PrettyLocationText = result.HintedLocationText.ToLower().Replace("_", " ");
            result.PrettyLocationText = Regex.Replace(result.PrettyLocationText, @"(^\w)|(\s\w)", m => m.Value.ToUpper()).Replace("Mm", "MM").Replace("Oot", "OOT");

            if (result.HintType == HintType.ItemExact || result.HintType == HintType.ItemRegion)
            {
                if (HintNames.ContainsKey(result.HintedLocationText))
                {
                    result.HintedLocations = HintNames[result.HintedLocationText];
                }
                else { result.HintedLocations = Array.Empty<string>(); }
                string itemData = LocationAnditemData.Replace($"{result.HintedLocationText} ", "");
                itemData = itemData[1..^1];
                result.HintedItems = itemData.Split(",").Select(x => x.Trim()).ToArray();
            }
            return result;
        }

        private static void ResetInstanceData(LogicObjects.TrackerInstance Instance)
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

    }
}
