﻿using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.datamodel;

namespace MMR_Tracker_V3.OtherGames.OOTMMV2
{
    internal class ImportSpoilerLog
    {
        public static void readAndApplySpoilerLog(LogicObjects.TrackerInstance Instance)
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
            Debug.WriteLine("\nApplying Junk Locations");
            SetjunkedLocations(Instance, JunkLocationData);
            Debug.WriteLine("\nApplying Entrance Data");
            ApplyEntrances(Instance, ExitData);

            Debug.WriteLine("\nSetting Special Conditions");
            ToggleSpecialConditions(Instance, AccessConditions);

        }

        private static void ApplyEntrances(LogicObjects.TrackerInstance instance, Dictionary<string, string> exitData)
        {
            var AllRandomizableExits = instance.EntrancePool.AreaList.Values.SelectMany(x => x.RandomizableExits(instance));
            foreach (var Entrance in exitData)
            {
                Debug.WriteLine($"{Entrance.Key}: {Entrance.Value}");
                var SpoilerEntrance = AllRandomizableExits.First(x => x.Value.GetDictEntry(instance).SpoilerData.SpoilerLogNames.Contains(Entrance.Key));
                var SpoilerExit = AllRandomizableExits.First(x => x.Value.GetDictEntry(instance).SpoilerData.SpoilerLogNames.Contains(Entrance.Value));
                SpoilerEntrance.Value.RandomizedState = MiscData.RandomizedState.Randomized;
                SpoilerEntrance.Value.SpoilerDefinedDestinationExit = new EntranceData.EntranceRandoDestination { region = SpoilerExit.Value.ID, from = SpoilerExit.Value.ParentAreaID };
            }
        }

        private static void HandleMQChecks(LogicObjects.TrackerInstance instance, List<string> mQDungeons)
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

        private static void SetjunkedLocations(LogicObjects.TrackerInstance instance, List<string> junkLocationData)
        {
            foreach (var i in junkLocationData)
            {
                if (!instance.LocationPool.ContainsKey(i)) { Debug.WriteLine($"{i} Was not a valid Location"); continue; }
                instance.LocationPool[i].SetRandomizedState(MiscData.RandomizedState.ForcedJunk, instance);
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
                else { CurrentTrick.TrickEnabled = true; }
            }
        }

        private static void PlaceItemsAtLocations(LogicObjects.TrackerInstance instance, Dictionary<string, string> LocationData)
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

        private static void ToggleSpecialConditions(LogicObjects.TrackerInstance instance, List<string> AccessConditions)
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

        private static void ApplySettings(LogicObjects.TrackerInstance instance, Dictionary<string, string> settingData)
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
                    else if (Int64.TryParse(setting.Value, out Int64 IntVal)) { instance.Variables[setting.Key].Value = IntVal; }
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
                    foreach (var i in MapsCompasses) { i.AmountInStartingpool= 1; }
                }
                if (setting.Key == "tingleShuffle" && setting.Value == "starting")
                {
                    var MapsCompasses = instance.ItemPool.Values.Where(x => x.Id.StartsWith("MM_WORLD_MAP_"));
                    foreach (var i in MapsCompasses) { i.AmountInStartingpool= 1; }
                }
            }
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

        private static Dictionary<string, string> GetDictionaryFromSpoiler(LogicObjects.TrackerInstance Instance, string Start, bool Fuzzy = false)
        {
            var Log = Instance.SpoilerLog.Log;
            bool AtReleventData = false;
            var SpoilerData = new Dictionary<string, string>();
            foreach (var x in Log)
            {
                if (x.Trim() == Start || (Fuzzy && x.Trim().StartsWith(Start))) { AtReleventData = true; continue; }
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

        private static void SetStartingItems(LogicObjects.TrackerInstance instance, Dictionary<string, int> startingItemData)
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
    }
}