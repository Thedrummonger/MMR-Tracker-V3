using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TDMUtils;

namespace MMR_Tracker_V3.SpoilerLogHandling.HardCodedParsers
{
    internal class OOTMMSpoilerLogTools
    {
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
            public HintType HintType { get; set; }
            public string PrettyLocationText { get; set; }
            public string[] HintedLocations { get; set; }
            public string[] HintedItemNames { get; set; }
            public string[] HintedItems { get; set; }
        }
        public static GenericSpoilerLog readAndApplySpoilerLog(InstanceData.TrackerInstance Temp, Dictionary<string, object> DataStore)
        {
            InstanceData.InstanceContainer container = new InstanceData.InstanceContainer();
            container.CopyAndLoadInstance(Temp);
            var Instance = container.Instance;
            ResetInstanceData(Instance);
            var log = DataStore["SpoilerLog"] as string[];
            Dictionary<string, string> LocationData = GetDictionaryFromSpoiler(log, "Location List", true);
            Dictionary<string, string> SettingData = GetDictionaryFromSpoiler(log, "Settings");
            Dictionary<string, int> StartingItemData = GetDictionaryFromSpoiler(log, "Starting Items").ToDictionary(x => x.Key, x => int.Parse(x.Value));
            Dictionary<string, string> ExitData = GetEntranceListFromSpoiler(log, "Entrances");
            List<string> HintData = GetListFromSpoiler(log, "Hints");
            List<string> WorldFlagData = GetListFromSpoiler(log, "World Flags");
            List<string> TrickData = [.. GetListFromSpoiler(log, "Tricks"), .. GetListFromSpoiler(log, "Glitches")];
            List<string> JunkLocationData = GetListFromSpoiler(log, "Junk Locations");
            List<string> MQDungeons = GetListFromSpoiler(log, "MQ Dungeons");
            List<string> AccessConditions = GetListFromSpoiler(log, "Special Conditions");

            Debug.WriteLine("\nApplying Settings");
            ApplySettings(Instance, SettingData);
            Debug.WriteLine("\nApplying World Flag Settings");
            ApplyWorldFlagSettings(Instance, WorldFlagData);
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
            return GenericSpoilerLog.CreateFromInstanceState(Instance);
        }

        private static void ApplyWorldFlagSettings(InstanceData.TrackerInstance instance, List<string> worldFlagData)
        {
            Dictionary<string, List<string>> SetOptions = new Dictionary<string, List<string>>();
            string CurrentSet = "";
            foreach (string key in worldFlagData)
            {
                if (key.EndsWith(':'))
                {
                    string BaseSetOption = key.TrimEnd(':');
                    CurrentSet = BaseSetOption;
                    SetOptions.SetIfEmpty(CurrentSet, new List<string>());
                }
                else if (key.StartsWith("- "))
                {
                    SetOptions[CurrentSet].Add(key[2..]);
                }
                else if (key.Contains(':'))
                {
                    string[] Data = key.Split(':').Select(x => x.Trim()).ToArray();
                    switch (Data[1])
                    {
                        case "all":
                            SetOptions.SetIfEmpty(Data[0], instance.MultiSelectOptions[Data[0]].ValueList.Keys.ToList());
                            break;
                        case "none":
                        case "random":
                            SetOptions.SetIfEmpty(Data[0], new List<string>());
                            break;
                    }
                }
            }
            foreach (var SetOption in SetOptions)
            {
                var MultiSelectOption = instance.MultiSelectOptions[SetOption.Key];
                MultiSelectOption.SetValues(SetOption.Value);
            }

        }

        private static void ApplyEntrances(InstanceData.TrackerInstance instance, Dictionary<string, string> exitData)
        {
            var AllRandomizableExits = instance.GetAllRandomizableExits();
            foreach (var Line in exitData)
            {
                string Exit = Line.Key.TrimSplit("(")[1][..^1];
                string Dest = Line.Value.TrimSplit("(")[1][..^1];
                var SpoilerEntrance = AllRandomizableExits.First(x => x.GetDictEntry().SpoilerData.SpoilerLogNames.Contains(Exit));
                var SpoilerExit = AllRandomizableExits.First(x => x.GetDictEntry().SpoilerData.SpoilerLogNames.Contains(Dest));
                SpoilerEntrance.RandomizedState = MiscData.RandomizedState.Randomized;
                SpoilerEntrance.SpoilerDefinedDestinationExit = new EntranceData.EntranceRandoDestination { region = SpoilerExit.ExitID, from = SpoilerExit.ParentAreaID };
            }
        }

        private static void HandleMQChecks(InstanceData.TrackerInstance instance, List<string> mQDungeons)
        {
            var MQSetting = instance.MultiSelectOptions["MasterQuest"];
            MQSetting.SetValues(mQDungeons);
            foreach (var i in MQSetting.ValueList.Keys)
            {
                bool ISMQ = MQSetting.EnabledValues.Contains(i);

                Debug.WriteLine($"{i} MQ {ISMQ}");
                string OppositeLogic = ISMQ ? $"setting{{MasterQuest, {i}, False}}" : $"setting{{MasterQuest, {i}, True}}";
                var JunkChecks = instance.LocationPool.Values.Where(x => instance.GetLogic(x.ID).RequiredItems.Contains(OppositeLogic));
                foreach (var Check in JunkChecks)
                {
                    Check.SetRandomizedState(MiscData.RandomizedState.ForcedJunk);
                }
            }
        }

        private static void SetjunkedLocations(InstanceData.TrackerInstance instance, List<string> junkLocationData)
        {
            foreach (var i in junkLocationData)
            {
                if (!instance.LocationPool.ContainsKey(i)) { Debug.WriteLine($"{i} Was not a valid Location"); continue; }
                instance.LocationPool[i].SetRandomizedState(MiscData.RandomizedState.ForcedJunk);
            }
        }

        private static void ToggleTricks(InstanceData.TrackerInstance instance, List<string> trickData)
        {
            var allTricks = instance.MacroPool.Values.Where(x => x.isTrick()).ToArray();
            foreach (var i in instance.MacroPool.Values.Where(x => x.isTrick())) { i.TrickEnabled = false; }
            foreach (var i in trickData)
            {
                var CurrentTrick = allTricks.FirstOrDefault(x => x.GetDictEntry().Name == i);
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
                string ItemName = instance.ItemPool.ContainsKey(i.Value) ? instance.ItemPool[i.Value].GetDictEntry().SpoilerData.SpoilerLogNames.First() : i.Value;
                var Item = instance.GetItemToPlace(ItemName);
                string ItemID;
                if (location is null)
                {
                    Debug.WriteLine($"Location {i.Key} is not valid");
                    continue;
                }
                else if (Item is null)
                {
                    var ValidItems = instance.ItemPool.Values.Where(x => x.GetDictEntry().SpoilerData.SpoilerLogNames.Contains(ItemName));
                    if (ValidItems.Any())
                    {
                        Debug.WriteLine($"Item {i.Value} has been placed more times than is allowed!");
                        ItemID = ValidItems.First().ID;
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
                    ItemID = Item.ID;
                }
                location.Randomizeditem.SpoilerLogGivenItem = ItemID;
                LocationFilled.Add(location);
            }
            foreach (var i in instance.LocationPool.Values)
            {
                if (string.IsNullOrWhiteSpace(i.Randomizeditem.SpoilerLogGivenItem) && i.RandomizedState == MiscData.RandomizedState.Randomized)
                {
                    Debug.WriteLine($"{i.ID} was randomized but had no spoiler data. Assuming it's unrandomized");
                    if (i.SingleValidItem is not null) { i.SetRandomizedState(MiscData.RandomizedState.UnrandomizedManual); }
                    else { i.SetRandomizedState(MiscData.RandomizedState.Unrandomized); }
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
                if (instance.ChoiceOptions.ContainsKey(ID)) { instance.ChoiceOptions[ID].SetValue(Data[1]); }
                else if (instance.ToggleOptions.ContainsKey(ID)) { instance.ToggleOptions[ID].SetValue(bool.Parse(Data[1])); }
                else if (instance.IntOptions.ContainsKey(ID)) { instance.IntOptions[ID].SetValue(int.Parse(Data[1])); }
                else { Debug.WriteLine($"{ID} was not an option or variable"); }
            }

        }

        private static void ApplySettings(InstanceData.TrackerInstance instance, Dictionary<string, string> settingData)
        {
            foreach (var setting in settingData)
            {
                if (instance.ChoiceOptions.ContainsKey(setting.Key))
                {
                    instance.ChoiceOptions[setting.Key].SetValue(setting.Value);
                }
                else if (instance.ToggleOptions.ContainsKey(setting.Key))
                {
                    instance.ToggleOptions[setting.Key].SetValue(bool.Parse(setting.Value));
                }
                else if (instance.IntOptions.ContainsKey(setting.Key))
                {
                    instance.IntOptions[setting.Key].SetValue(int.Parse(setting.Value));
                }
                else
                {
                    throw new Exception($"Setting {setting.Key} did not exist");
                    Debug.WriteLine($"Setting {setting.Key} did not exist");
                }

                if (setting.Key == "mapCompassShuffle" && setting.Value == "starting")
                {
                    var MapsCompasses = instance.ItemPool.Values.Where(x =>
                    x.ID.StartsWith("MM_MAP_") ||
                    x.ID.StartsWith("OOT_MAP_") ||
                    x.ID.StartsWith("MM_COMPASS_") ||
                    x.ID.StartsWith("OOT_COMPASS_"));
                    foreach (var i in MapsCompasses) { i.AmountInStartingpool = 1; }
                }
                if (setting.Key == "tingleShuffle" && setting.Value == "starting")
                {
                    var MapsCompasses = instance.ItemPool.Values.Where(x => x.ID.StartsWith("MM_WORLD_MAP_"));
                    foreach (var i in MapsCompasses) { i.AmountInStartingpool = 1; }
                }
                if (setting.Key == "strayFairyChestShuffle" && setting.Value == "starting")
                {
                    //instance.ItemPool.Values.First(x => x.ID == "MM_STRAY_FAIRY_WF").AmountInStartingpool += 3;
                    //instance.ItemPool.Values.First(x => x.ID == "MM_STRAY_FAIRY_SH").AmountInStartingpool += 7;
                    //instance.ItemPool.Values.First(x => x.ID == "MM_STRAY_FAIRY_GB").AmountInStartingpool += 6;
                    //instance.ItemPool.Values.First(x => x.ID == "MM_STRAY_FAIRY_ST").AmountInStartingpool += 15;
                }
                if (setting.Key == "strayFairyOtherShuffle" && (setting.Value == "starting" || setting.Value == "removed"))
                {
                    //instance.ItemPool.Values.First(x => x.ID == "MM_STRAY_FAIRY_WF").AmountInStartingpool += 12;
                    //instance.ItemPool.Values.First(x => x.ID == "MM_STRAY_FAIRY_SH").AmountInStartingpool += 8;
                    //instance.ItemPool.Values.First(x => x.ID == "MM_STRAY_FAIRY_GB").AmountInStartingpool += 9;
                }

            }
            JunkLocationBySetting("shuffleFreeRupeesOot", "rupee", "OOT");
            JunkLocationBySetting("shuffleFreeRupeesMm", "rupee", "MM");
            JunkLocationBySetting("shuffleFreeHeartsOot", "heart", "OOT");
            JunkLocationBySetting("shuffleFreeHeartsMm", "heart", "MM");
            JunkLocationBySetting("shufflePotsOot", "pot", "OOT");
            JunkLocationBySetting("shufflePotsMm", "pot", "MM");
            JunkLocationBySetting("shuffleGrassOot", "grass", "OOT");
            JunkLocationBySetting("shuffleGrassMm", "grass", "MM");
            JunkLocationBySetting("pondFishShuffle", "fish", "OOT");
            JunkLocationBySetting("fairyFountainFairyShuffleOot", "fairy", "OOT");
            JunkLocationBySetting("fairyFountainFairyShuffleMm", "fairy", "MM");
            JunkLocationBySetting("fairySpotShuffleOot", "fairy_spot", "OOT");
            void JunkLocationBySetting(string optionKey, string tag, string GameCode)
            {
                var option = instance.ToggleOptions[optionKey];
                if (option.GetValue() == option.Enabled) { return; }
                foreach (var location in instance.LocationPool.Where(x =>
                    x.Value.GetDictEntry().SpoilerData.Tags.Contains(tag) && x.Key.StartsWith(GameCode)))
                {
                    location.Value.SetRandomizedState(MiscData.RandomizedState.Unrandomized);
                }
            }
        }

        private static void ResetInstanceData(InstanceData.TrackerInstance Instance)
        {
            Instance.GetParentContainer().logicCalculation.ResetAutoObtainedItems();
            Instance.ToggleAllTricks(false);
            foreach (var i in Instance.GetAllRandomizableExits())
            {
                i.RandomizedState = MiscData.RandomizedState.Unrandomized;
            }
            foreach (var i in Instance.LocationPool.Values)
            {
                i.SetRandomizedState(MiscData.RandomizedState.Randomized);
            }
            foreach (var i in Instance.ItemPool.Values)
            {
                i.AmountInStartingpool = 0;
            }
        }

        private static Dictionary<string, string> GetDictionaryFromSpoiler(string[] Log, string Start, bool Fuzzy = false)
        {
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

        private static Dictionary<string, string> GetEntranceListFromSpoiler(string[] Log, string Start)
        {
            bool AtReleventData = false;
            var SpoilerData = new Dictionary<string, string>();
            foreach (var x in Log)
            {
                if (x.Trim() == Start) { AtReleventData = true; continue; }
                if (AtReleventData && !x.StartsWith(" ") && !string.IsNullOrWhiteSpace(x)) { break; }
                if (!AtReleventData || string.IsNullOrWhiteSpace(x)) { continue; }
                var LineParts = x.Split("->").Select(x => x.Trim()).ToArray();
                if (LineParts.Count() > 1 && !string.IsNullOrWhiteSpace(LineParts[1]))
                {
                    SpoilerData[LineParts[0]] = LineParts[1];
                }
            }
            return SpoilerData;
        }

        private static List<string> GetListFromSpoiler(string[] Log, string Start)
        {
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
                var item = instance.ItemPool.Values.FirstOrDefault(x => x.ID == i.Key || x.GetDictEntry().SpoilerData.SpoilerLogNames.Contains(i.Key));
                if (item is null)
                {
                    Debug.WriteLine($"{i.Key} is not a valid item");
                    continue;
                }
                for (var c = 0; c < i.Value; c++)
                {
                    if (instance.GetItemToPlace(item.ID) is not null)
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
                    Result[CurrentKey] = new SpoilerHintData { HintType = HintType.Hero, PrettyLocationText = $"{x.Split("    ")[0]}" };
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
                        var ValidItem = Instance.ItemPool.Values.FirstOrDefault(x => x.GetDictEntry().SpoilerData.SpoilerLogNames.Contains(item));
                        ItemIDs.Add(ValidItem is null ? item : ValidItem.ID);
                    }
                    foreach (var item in Items)
                    {
                        ItemNames.Add(Instance.ItemPool.ContainsKey(item) ? Instance.ItemPool[item].GetDictEntry().GetName() : item);
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
