using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
            public string PrettyLocationText;
            public string[] HintedLocations;
            public string[] HintedItemNames;
            public string[] HintedItems;
        }
        public static void readAndApplySpoilerLog(LogicObjects.TrackerInstance Instance)
        {
            ResetInstanceData(Instance);
            Dictionary<string, string> LocationData = GetDictionaryFromSpoiler(Instance, "Location List");
            Dictionary<string, string> SettingData = GetDictionaryFromSpoiler(Instance, "Settings");
            Dictionary<string, int> StartingItemData = GetDictionaryFromSpoiler(Instance, "Starting Items").ToDictionary(x => x.Key, x => int.TryParse(x.Value, out int SIC) ? SIC : -1);
            Dictionary<string, string> ExitData = GetEntranceListFromSpoiler(Instance, "Entrances");
            List<string> HintData = GetListFromSpoiler(Instance, "Hints");
            List<string> TrickData = GetListFromSpoiler(Instance, "Tricks");
            List<string> JunkLocationData = GetListFromSpoiler(Instance, "Junk Locations");
            List<string> MQDungeons = GetListFromSpoiler(Instance, "MQ Dungeons");
            List<string> AccessConditions = GetListFromSpoiler(Instance, "Special Conditions");

            Debug.WriteLine("\nApplying Settings");
            ApplySpoilerSettings(Instance, SettingData);
            Debug.WriteLine("\nApplying Master Quest Data");
            HandleMQChecks(Instance, MQDungeons);
            Debug.WriteLine("\nToggling Tricks");
            ToggleTricks(Instance, TrickData);
            Debug.WriteLine("\nApplying Starting Items");
            SetStartingItems(Instance, StartingItemData);
            Debug.WriteLine("\nPlacing Items at locations");
            PlaceItemsAtLocations(Instance, LocationData);
            Debug.WriteLine("\nApplying Junk Locations");
            SetjunkedLocations(Instance, JunkLocationData);
            Debug.WriteLine("\nApplying Hints");
            ApplyHintData(Instance, ParseHintData(Instance, HintData));
            Debug.WriteLine("\nApplying Entrance Data");
            ApplyEntrances(Instance, ExitData);
            Debug.WriteLine("\nSetting Special Conditions");
            ToggleSpecialConditions(Instance, AccessConditions);
        }

        private static void ToggleSpecialConditions(LogicObjects.TrackerInstance instance, List<string> AccessConditions)
        {

            bool atBridge = false;
            bool atMoon = false;
            bool atLACS = false;
            bool atGBK = false;
            foreach (var i in AccessConditions)
            {
                if (i == "BRIDGE:") { atBridge = true; atMoon = false; atLACS = false; atGBK = false; continue; }
                if (i == "MOON:") { atBridge = false; atMoon = true; atLACS = false; atGBK = false; continue; }
                if (i == "LACS:") { atBridge = false; atMoon = false; atLACS = true; atGBK = false; continue; }
                if (i == "GANON_BK:") { atBridge = false; atMoon = false; atLACS = false; atGBK = true; continue; }
                if (string.IsNullOrWhiteSpace(i)) { return; }
                var Data = i.Split(':').Select(x => x.Trim()).ToArray();
                string ID = "";
                if (atBridge) { ID = $"bridge_{Data[0]}"; }
                else if (atMoon) { ID = $"moon_{Data[0]}"; }
                else if (atLACS) { ID = $"lacs_{Data[0]}"; }
                else if (atGBK) { ID = $"ganon_bk_{Data[0]}"; }
                if (instance.UserOptions.ContainsKey(ID)) { instance.UserOptions[ID].CurrentValue = Data[1]; }
                else if (instance.Variables.ContainsKey(ID))
                {
                    if (int.TryParse(Data[1], out int count)) { instance.Variables[ID].Value = count; }
                    else { instance.Variables[ID].Value = Data[1]; }
                }
                else { Debug.WriteLine($"{ID} was not an option or variable"); }
            }

        }

        private class EntranceDataContainer
        {
            public string Area { get; set; }
            public string Dest { get; set; }
        }

        private static void HandleMQChecks(LogicObjects.TrackerInstance instance, List<string> mQDungeons)
        {
            foreach(var i in instance.UserOptions.Where(x => x.Value.SubCategory == "Master Quest Dungeons"))
            {
                bool ISMQ = mQDungeons.Any(x => i.Key.ToLower().StartsWith(x.ToLower()));
                i.Value.CurrentValue = ISMQ.ToString().ToLower();

                string DungeonCode = i.Key[..^2];

                Debug.WriteLine($"{DungeonCode} MQ {ISMQ}");
                string OppositeLogic = $"option{{{i.Key}, {(!ISMQ).ToString().ToLower()}}}";
                var JunkChecks = instance.LocationPool.Values.Where(x => instance.GetLogic(x.ID).RequiredItems.Contains(OppositeLogic));
                foreach(var Check in JunkChecks)
                {
                    Check.SetRandomizedState(MiscData.RandomizedState.ForcedJunk, instance);
                }
            }
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
                        instance.HintPool[i.Key].SpoilerHintText = $"{i.Value.PrettyLocationText} contains {string.Join(" and ", i.Value.HintedItems )}";
                        break;
                    case HintType.Hero:
                        instance.HintPool[i.Key].SpoilerHintText = $"{i.Value.PrettyLocationText} is Way of the Hero";
                        break;
                    case HintType.Foolish:
                        instance.HintPool[i.Key].SpoilerHintText = $"{i.Value.PrettyLocationText} is Foolish";
                        break;
                }
            }
            foreach(var i in instance.HintPool)
            {
                if (string.IsNullOrWhiteSpace(i.Value.SpoilerHintText))
                {
                    Debug.WriteLine($"Hint {i.Key} has no spoiler data, assuming junk");
                    i.Value.RandomizedState = MiscData.RandomizedState.ForcedJunk;
                }
            }
        }

        private static void ToggleTricks(LogicObjects.TrackerInstance instance, List<string> trickData)
        {
            var allTricks = instance.MacroPool.Values.Where(x => x.isTrick(instance)).ToArray();
            foreach (var i in instance.MacroPool.Values.Where(x => x.isTrick(instance))) { i.TrickEnabled = false; }
            foreach (var i in trickData)
            {
                var CurrentTrick = allTricks.FirstOrDefault(x => x.GetDictEntry(instance).Name == i);
                if (CurrentTrick is null) { Debug.WriteLine($"{i} is not a valid trick"); }
                else { CurrentTrick.TrickEnabled = true;}
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
                string ItemName = instance.ItemPool.ContainsKey(i.Value) ? instance.ItemPool[i.Value].GetDictEntry(instance).SpoilerData.SpoilerLogNames.First() : i.Value; 
                var Item = instance.GetItemToPlace(ItemName);
                string ItemID;
                if (location is null)
                {
                    Debug.WriteLine($"{i.Key} is not valid");
                    continue;
                }
                else if (Item is null)
                {
                    var ValidItems = instance.ItemPool.Values.Where(x => x.GetDictEntry(instance).SpoilerData.SpoilerLogNames.Contains(ItemName));
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
                if (string.IsNullOrWhiteSpace(i.Randomizeditem.SpoilerLogGivenItem) && i.RandomizedState == MiscData.RandomizedState.Randomized)
                {
                    Debug.WriteLine($"{i.ID} was randomized but had no spoiler data. Assuming it's unrandomized");
                    if (i.SingleValidItem is not null) { i.SetRandomizedState(MiscData.RandomizedState.UnrandomizedManual, instance); }
                    else { i.SetRandomizedState(MiscData.RandomizedState.Unrandomized, instance); }
                }
            }
        }

        private static void SetStartingItems(LogicObjects.TrackerInstance instance, Dictionary<string, int> startingItemData)
        {
            foreach(var i in startingItemData)
            {
                if(i.Value < 1) { continue; }
                var item = instance.ItemPool.Values.FirstOrDefault(x => x.Id == i.Key || x.GetDictEntry(instance).SpoilerData.SpoilerLogNames.Contains(i.Key));
                if (item is null)
                {
                    Debug.WriteLine($"{i.Key} is not a valid item");
                    continue;
                }
                for(var c = 0; c < i.Value; c++)
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
                if (AtReleventData && !x.StartsWith(" ") && !string.IsNullOrWhiteSpace(x)) { break; }
                if (!AtReleventData || string.IsNullOrWhiteSpace(x)) { continue; }
                EnabledTricks.Add(x.Trim());
            }
            return EnabledTricks;
        }

        private static Dictionary<string, SpoilerHintData> ParseHintData(LogicObjects.TrackerInstance Instance, List<string> hintData)
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
                else if (CurrentType == HintType.Hero) { HeroHints.Add(x); }
                else if (CurrentType == HintType.Foolish) { FoolishHints.Add(x); }
                else if (CurrentType == HintType.ItemExact) { ExactHints.Add(x); }
                else if (CurrentType == HintType.ItemRegion) { RegionHints.Add(x); }
            }
            string CurrentKey = "";
            foreach(var x in HeroHints)
            {
                if (CurrentKey == "") { CurrentKey = x; }
                else
                {
                    Result[CurrentKey] = new SpoilerHintData { HintType= HintType.Hero, PrettyLocationText = $"{x.StringSplit("    ")[0]}" };
                    CurrentKey = "";
                }
            }
            CurrentKey = "";
            foreach (var x in FoolishHints)
            {
                if (CurrentKey != "")
                {
                    Result[CurrentKey] = new SpoilerHintData { HintType= HintType.Foolish, PrettyLocationText = $"{x}" };
                    CurrentKey = "";
                }
                else { CurrentKey = x; }
            }
            CurrentKey = "";
            foreach (var x in RegionHints)
            {
                if (CurrentKey != "")
                {
                    var data = x.Split(new[] { '(' }, 2);
                    data[1] = data[1][..^1];
                    Result[CurrentKey] = new SpoilerHintData { HintType= HintType.Hero, PrettyLocationText = data[0], HintedItems = data[1].Split(",") };
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
                        HintType= HintType.ItemExact, 
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
