using MathNet.Numerics;
using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.HintData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.SpoilerLogImporter
{
    public static class MMRSpoilerLogTools
    {
        public static void ReadAndApplySpoilerLog(InstanceContainer Container)
        {
            int SettingLineIndex = -1;
            int SeedIndex = -1;
            int BlitzStartingItemsIndex = -1;
            int EntranceIndex = -1;
            int ItemLocationIndex = -1;
            int CostIndex = -1;
            int GossipIndex = -1;
            int PlaythroughIndex = -1;

            int CurInd = 0;
            foreach(var i in Container.Instance.SpoilerLog.Log)
            {
                if (i.StartsWith("Settings:")) { SettingLineIndex = CurInd; }
                else if (i.StartsWith("Seed:")) { SeedIndex = CurInd; }
                else if (i.Trim().StartsWith("Blitz Starting Items")) { BlitzStartingItemsIndex = CurInd; }
                else if (i.Trim().StartsWith("Entrance") && i.Contains("Destination")) { EntranceIndex = CurInd; }
                else if (i.Trim().StartsWith("Location") && i.Contains("Item")) { ItemLocationIndex = CurInd; }
                else if (i.Trim().StartsWith("Name") && i.Contains("Cost")) { CostIndex = CurInd; }
                else if (i.Trim().StartsWith("Gossip Stone ") && i.Contains("Message")) { GossipIndex = CurInd; }
                else if (i.Trim().StartsWith("Playthrough")) { GossipIndex = CurInd; }
                CurInd++;
            }
            int SettingIndexEnd = SeedIndex - 1;
            int BlitzStartingItemsEnd = EntranceIndex < 0 ? ItemLocationIndex -1 : EntranceIndex;
            int EntranceIndexEnd = ItemLocationIndex - 1;
            int ItemLocationIndexEnd = CostIndex < 0 ? GossipIndex -1 : CostIndex - 1;
            int GossipIndexEnd = PlaythroughIndex -1;

            var SettingLines = Container.Instance.SpoilerLog.Log.ToList().GetRange((SettingLineIndex+1)..(SettingIndexEnd-1));
            var SettingString = $"{{{string.Join(" ", SettingLines)}}}";
            Dictionary<string, object> Settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(SettingString);

            List<string> ItemLocations = Container.Instance.SpoilerLog.Log.ToList().GetRange((ItemLocationIndex+1)..(ItemLocationIndexEnd));
            List<string> JunkLocations = GetJunkLocations(ItemLocations);
            List<Tuple<string, string>> ItemLocationDict = GetItemLocationData(ItemLocations);

            Dictionary<string, string> EntranceLocationDict = new Dictionary<string, string>();
            if (EntranceIndex > -1)
            {
                List<string> EntrancesList = Container.Instance.SpoilerLog.Log.ToList().GetRange((EntranceIndex+1)..(EntranceIndexEnd));
                EntranceLocationDict = GetEntranceLocationData(EntrancesList);
            }

            List<string> BlitzStartingItems = new List<string>();
            if (BlitzStartingItemsIndex > -1)
            {
                BlitzStartingItems = Container.Instance.SpoilerLog.Log.ToList().GetRange((BlitzStartingItemsIndex+1)..(BlitzStartingItemsEnd)).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            }

            /* The randomizer will rename these locations based on boss door rando, since the tracker can't really support live location renaming based
             * on what item is where, I'll just manually swap them when reading the spoiler log.
             * 
             * TODO hopefully find a better way to do this mess
             * 
            HeartContainerWoodfall gets renamed based on AreaOdolwasLair
            HeartContainerSnowhead gets renamed based on AreaGohtsLair
            HeartContainerGreatBay gets renamed based on AreaGyorgsLair
            HeartContainerStoneTower gets renamed based on AreaTwinmoldsLair
            RemainsOdolwa gets renamed based on AreaOdolwasLair
            RemainsGoht gets renamed based on AreaGohtsLair
            RemainsGyorg gets renamed based on AreaGyorgsLair
            RemainsTwinmold gets renamed based on AreaTwinmoldsLair
            */
            Dictionary<string, string> RenamedHeartContainerChecks = new Dictionary<string, string>()
            {
                { "HeartContainerWoodfall", "AreaOdolwasLair" },
                { "HeartContainerSnowhead", "AreaGohtsLair" },
                { "HeartContainerGreatBay", "AreaGyorgsLair" },
                { "HeartContainerStoneTower", "AreaTwinmoldsLair" }
            };
            Dictionary<string, string> RenamedRemainsChecks = new Dictionary<string, string>()
            {
                { "RemainsOdolwa", "AreaOdolwasLair" },
                { "RemainsGoht", "AreaGohtsLair" },
                { "RemainsGyorg", "AreaGyorgsLair" },
                { "RemainsTwinmold", "AreaTwinmoldsLair" }
            };

            ResetInstance(Container.Instance);

            ApplySettings(Container.Instance, Settings);

            foreach(var trick in Settings.GetValueAs<string, string[]>("EnabledTricks"))
            {
                var Trick = Container.Instance.GetMacroByID(trick);
                if (Trick is null) { Debug.WriteLine($"trick {trick} was not valid"); continue; }
                if (!Trick.isTrick(Container.Instance)) { Debug.WriteLine($"macro {trick} was not a trick"); continue; }
                Trick.TrickEnabled = true;
            }

            Container.logicCalculation.CompileOptionActionEdits();

            SettingStringHandler.ApplyLocationString(Settings.GetValueAs<string, string>("CustomItemListString"), Container.Instance);
            SettingStringHandler.ApplyJunkString(Settings.GetValueAs<string, string>("CustomJunkLocationsString"), Container.Instance);
            SettingStringHandler.ApplyStartingItemString(Settings.GetValueAs<string, string>("CustomStartingItemListString"), Container.Instance);

            //Keep track of boss door locations even if they aren't randomized
            Dictionary<string, string> BossDoorMapping = new Dictionary<string, string>()
            {
                { "AreaOdolwasLair", "AreaOdolwasLair" },
                { "AreaGohtsLair", "AreaGohtsLair" },
                { "AreaGyorgsLair", "AreaGyorgsLair" },
                { "AreaTwinmoldsLair", "AreaTwinmoldsLair" }
            };
            foreach(var E in EntranceLocationDict)
            {
                var Entrance = Container.Instance.LocationPool.First(x => x.Value.GetDictEntry(Container.Instance).SpoilerData.SpoilerLogNames.Contains(E.Key)).Value;
                var Destination = Container.Instance.ItemPool.First(x => x.Value.GetDictEntry(Container.Instance).SpoilerData.SpoilerLogNames.Contains(E.Value)).Value;

                Entrance.SetRandomizedState(RandomizedState.Randomized, Container.Instance);
                Entrance.Randomizeditem.SpoilerLogGivenItem = Destination.Id;
                if (BossDoorMapping.ContainsKey(Entrance.ID)) { BossDoorMapping[Entrance.ID] = Destination.Id; }
            }

            foreach(var i in Container.Instance.LocationPool.Values)
            {
                if (i.IsUnrandomized(UnrandState.Any))
                {
                    var OriginalItem = Container.Instance.GetItemByID(i.GetDictEntry(Container.Instance).OriginalItem);
                    if (OriginalItem.AmountInStartingpool > 0)
                    {
                        Debug.WriteLine($"{i.ID}: {OriginalItem.Id} Was unrandomized but Item was starting item. Forcing Junk");
                        i.SetRandomizedState(RandomizedState.ForcedJunk, Container.Instance);
                    }
                }
            }

            foreach (var i in ItemLocationDict)
            {
                var PossibleLocations = Container.Instance.LocationPool.Where(x => x.Value.GetDictEntry(Container.Instance).GetName(Container.Instance) == i.Item1);
                PossibleLocations = PossibleLocations.Where(x => string.IsNullOrWhiteSpace(x.Value.Randomizeditem.SpoilerLogGivenItem));
                if (!PossibleLocations.Any())
                {
                    Debug.WriteLine($"No Location {i.Item1} has not been placed. Assuming Duplicate?");
                    continue;
                }
                var SpoilerLocation = PossibleLocations.First().Value;
                var Item = Container.Instance.GetItemToPlace(i.Item2, DoNameEdits: true);

                if (RenamedHeartContainerChecks.ContainsKey(SpoilerLocation.ID))
                {
                    var BossDoorAtCheck = RenamedHeartContainerChecks[SpoilerLocation.ID];
                    var NewBossDoor = BossDoorMapping[BossDoorAtCheck];
                    var NewLocation = RenamedHeartContainerChecks.First(x => x.Value == NewBossDoor);
                    Debug.WriteLine($"Location {SpoilerLocation.ID} Was changed to {NewLocation.Key}");
                    SpoilerLocation = Container.Instance.GetLocationByID(NewLocation.Key);
                }
                else if (RenamedRemainsChecks.ContainsKey(SpoilerLocation.ID))
                {
                    var BossDoorAtCheck = RenamedRemainsChecks[SpoilerLocation.ID];
                    var NewBossDoor = BossDoorMapping[BossDoorAtCheck];
                    var NewLocation = RenamedRemainsChecks.First(x => x.Value == NewBossDoor);
                    Debug.WriteLine($"Location {SpoilerLocation.ID} Was changed to {NewLocation.Key}");
                    SpoilerLocation = Container.Instance.GetLocationByID(NewLocation.Key);
                }

                if (Item is null)
                {
                    Debug.WriteLine($"No more {i.Item2} Could Be Placed. Assuming Junk?");
                    SpoilerLocation.Randomizeditem.SpoilerLogGivenItem = $"FAKE: {i.Item2}";
                }
                else
                {
                    SpoilerLocation.Randomizeditem.SpoilerLogGivenItem = Item.Id;
                }
            }


            if (BlitzStartingItems.Any())
            {
                HandleBlitzJunkedDungeons(Container.Instance, EntranceLocationDict, BlitzStartingItems);
            }

            foreach (var i in Container.Instance.LocationPool.Values)
            {
                if (i.RandomizedState != RandomizedState.Randomized) { continue; }
                if (!string.IsNullOrWhiteSpace(i.Randomizeditem.SpoilerLogGivenItem)) { continue; }

                Debug.WriteLine($"{i.ID} Was Randomized with no Spoiler Data and Could not be Unrandomized");
            }

            foreach (var item in BlitzStartingItems)
            {
                var Item = Container.Instance.GetItemToPlace(item, DoNameEdits: true);
                //Item.AmountInStartingpool = 1;
            }


        }

        private static void HandleBlitzJunkedDungeons(InstanceData.TrackerInstance instance, Dictionary<string, string> entranceLocationDict, List<string> blitzStartingItems)
        {
            HashSet<string> LockedByBlitzCache = new HashSet<string>();
            HashSet<string> InaccessableItems = GetBlitzDungeons(instance, blitzStartingItems, out HashSet<string> BlitzStartingItems);
            HashSet<string> InaccessableMacros = new HashSet<string>();
            HashSet<string> InaccessableLocations = new HashSet<string>();

            var Actions = instance.GetOptionActions();

            bool ItemsToBeFound = true;
            while (ItemsToBeFound)
            {
                ItemsToBeFound = CheckForLockedLogic();
            }
            Debug.WriteLine($"InaccessableItems\n{InaccessableItems.ToFormattedJson()}");
            Debug.WriteLine($"InaccessableMacros\n{InaccessableMacros.ToFormattedJson()}");
            Debug.WriteLine($"InaccessableLocations\n{InaccessableLocations.ToFormattedJson()}");
            Debug.WriteLine($"BlitzStartingItems\n{BlitzStartingItems.ToFormattedJson()}");
            Debug.WriteLine($"LockedByBlitzCache\n{LockedByBlitzCache.ToFormattedJson()}");

            foreach (var Location in InaccessableLocations)
            {
                var Loc = instance.GetLocationByID(Location);
                if (Loc.IsRandomized() && !string.IsNullOrWhiteSpace(Loc.Randomizeditem.SpoilerLogGivenItem) && !Loc.ID.EndsWith("Lair"))
                {
                    throw new Exception($"{Loc.ID} Was randomized and given {Loc.Randomizeditem.SpoilerLogGivenItem}");
                }
                Loc.SetRandomizedState(RandomizedState.ForcedJunk, instance);
            }
            foreach(var Item in BlitzStartingItems)
            {
                var item = instance.GetItemByID(Item);
                if (item.AmountInStartingpool > 0)
                {
                    //throw new Exception($"{item.Id} Was already a starting item?");
                }
                item.AmountInStartingpool = 1;
            }


            bool CheckForLockedLogic()
            {
                bool FoundLockedItem = false;
                foreach(var i in instance.LocationPool)
                {
                    if (InaccessableLocations.Contains(i.Key)) { continue; }
                    var Logic = instance.GetLogic(i.Key, true, Actions);
                    if (Logic.RequiredItems.Any(x => LockedByBlitz(x)))
                    {
                        InaccessableLocations.Add(i.Key);
                        CommitLockedLocation(i);
                        FoundLockedItem = true;
                    }
                    if (Logic.ConditionalItems.Any())
                    {
                        var NewConditionals = Logic.ConditionalItems.Where(x => !x.Any(x => LockedByDefault(x)));
                        if (NewConditionals.Any() && NewConditionals.All(set => set.Any(x => LockedByBlitz(x))))
                        {
                            InaccessableLocations.Add(i.Key);
                            CommitLockedLocation(i);
                            FoundLockedItem = true;
                        }
                    }
                }
                foreach(var i in instance.MacroPool)
                {
                    if (InaccessableMacros.Contains(i.Key)) { continue; }
                    var Logic = instance.GetLogic(i.Key, true, Actions);
                    if (Logic.RequiredItems.Any(x => LockedByBlitz(x)))
                    {
                        InaccessableMacros.Add(i.Key);
                        FoundLockedItem = true;
                    }
                    if (Logic.ConditionalItems.Any())
                    {
                        var NewConditionals = Logic.ConditionalItems.Where(x => !x.Any(x => LockedByDefault(x)));
                        if (NewConditionals.Any() && NewConditionals.All(set => set.Any(x => LockedByBlitz(x))))
                        {
                            InaccessableMacros.Add(i.Key);
                            FoundLockedItem = true;
                        }
                    }
                }
                return FoundLockedItem;
            }

            void CommitLockedLocation(KeyValuePair<string, LocationData.LocationObject> i)
            {
                if (i.Value.IsUnrandomized())
                {
                    string OriginalItem = i.Value.GetItemAtCheck(instance);
                    var OriginalItemObj = instance.GetItemByID(OriginalItem);
                    if (OriginalItemObj.ValidStartingItem(instance)) { BlitzStartingItems.Add(OriginalItem); }
                    else { InaccessableItems.Add(OriginalItem); }
                }
                else if (i.Key.StartsWith("BottleCatch"))
                {
                    string RandomizedItem = i.Value.GetItemAtCheck(instance);
                    var OriginalItemObj = instance.GetItemByID(RandomizedItem);
                    InaccessableItems.Add(OriginalItemObj.Id);
                }
                else if (i.Value.GetDictEntry(instance).ValidItemTypes.Contains("DungeonEntrance") || i.Value.GetDictEntry(instance).ValidItemTypes.Contains("BossDoorEntrance"))
                {
                    string RandomizedItem = i.Value.GetItemAtCheck(instance);
                    var OriginalItemObj = instance.GetItemByID(RandomizedItem);

                    InaccessableItems.Add(OriginalItemObj.Id);
                }
            }

            bool LockedByBlitz(string Item)
            {
                if (LockedByBlitzCache.Contains(Item)) { return true; }
                if (LogicEditing.IsLogicFunction(Item, out string Func, out string Param))
                {
                    var Params = Param.Split(",").Select(x => x.Trim()).ToArray();
                    if (Func.In("check", "available") && (InaccessableLocations.Contains(Params[0]) || InaccessableMacros.Contains(Params[0]))) {
                        LockedByBlitzCache.Add(Item);
                        return true; 
                    }
                    else { return false; }
                }
                instance.MultipleItemEntry(Item, out string LogicItem, out int Amount);
                bool Literal = LogicItem.IsLiteralID(out LogicItem);
                var type = instance.GetItemEntryType(LogicItem, Literal, out _);
                if (InaccessableMacros.Contains(LogicItem)) { LockedByBlitzCache.Add(Item); return true; }
                if (InaccessableItems.Contains(LogicItem)) { LockedByBlitzCache.Add(Item); return true; }
                return false;
            }

            bool LockedByDefault(string Item)
            {
                if (LogicEditing.IsLogicFunction(Item, out string Func, out string Param))
                {
                    //Debug.WriteLine($"Item Was Function {Func}");
                    if (Func.In("contains", "trick", "setting", "option", "rand", "randomized")) 
                    {
                        //Debug.WriteLine($"Parsing {Func} Function");
                        LogicEditing.CheckLogicFunction(instance, Item, new List<string>(), out bool FunctionEntryValid);
                        //Debug.WriteLine($"Valid? {FunctionEntryValid}");
                        return !FunctionEntryValid;
                    }
                    else { return false; }
                }
                instance.MultipleItemEntry(Item, out string LogicItem, out int Amount);
                bool Literal = LogicItem.IsLiteralID(out LogicItem);
                var type = instance.GetItemEntryType(LogicItem, Literal, out object obj);
                if (type == LogicEntryType.macro && ((MacroObject)obj).isTrick(instance) && !((MacroObject)obj).TrickEnabled) { return true; }
                if (type == LogicEntryType.Bool && !bool.Parse(LogicItem)) { return true; }
                return false;
            }

        }

        private static HashSet<string> GetBlitzDungeons(InstanceData.TrackerInstance instance, List<string> blitzStartingItems, out HashSet<string> BlitzStartingItems)
        {
            BlitzStartingItems = new HashSet<string>();
            var remainsInLairs = new Dictionary<string, string>
            {
                { "RemainsOdolwa", "AreaOdolwasLair" },
                { "RemainsGoht", "AreaGohtsLair" },
                { "RemainsGyorg", "AreaGyorgsLair" },
                { "RemainsTwinmold", "AreaTwinmoldsLair" },
            };
            var lairsInDungeons = new Dictionary<string, string[]>
            {
                { "AreaOdolwasLair", new string[] { "AreaWoodFallTempleAccess" } },
                { "AreaGohtsLair", new string[] { "AreaSnowheadTempleAccess" } },
                { "AreaGyorgsLair", new string[] { "AreaGreatBayTempleAccess" } },
                { "AreaTwinmoldsLair", new string[] { "AreaInvertedStoneTowerTempleAccess", "AreaStoneTowerTempleAccess" } },
            };

            List<string> BlitzDungeons = new List<string>();

            foreach(string item in blitzStartingItems)
            {
                var BlitzStartingItem = instance.GetItemToPlace(item, IgnoreMaxAmount: true);
                if (!remainsInLairs.ContainsKey(BlitzStartingItem.Id)) { continue; }
                BlitzStartingItems.Add(BlitzStartingItem.Id);
                string Lair = remainsInLairs[BlitzStartingItem.Id];

                var NewLairLocation = instance.LocationPool.Values.FirstOrDefault(x => (x.Randomizeditem.SpoilerLogGivenItem??"") == Lair);
                Lair = NewLairLocation is null ? Lair : NewLairLocation.ID;

                string[] Temples = lairsInDungeons[Lair];
                BlitzDungeons.AddRange(Temples);
            }
            return BlitzDungeons.Distinct().ToHashSet();

        }

        private static Dictionary<string, string> GetEntranceLocationData(List<string> entrancesList)
        {
            Dictionary<string, string> EntranceDict = new Dictionary<string, string>();
            foreach (var Entrnace in entrancesList)
            {
                if (!Entrnace.Contains("->")) { continue; }
                string[] Data = Entrnace.Split("->");
                string Location = Data[0].Trim();
                string Item = Data[1].Trim();
                EntranceDict.Add(Location, Item);
            }
            return EntranceDict;
        }

        private static void ResetInstance(InstanceData.TrackerInstance instance)
        {
            instance.ToggleAllTricks(false);
            foreach (var i in instance.LocationPool)
            {
                i.Value.SetRandomizedState(RandomizedState.Randomized, instance);
                if (i.Value.GetDictEntry(instance).ValidItemTypes.Contains("DungeonEntrance") || i.Value.GetDictEntry(instance).ValidItemTypes.Contains("BossDoorEntrance"))
                {
                    i.Value.SetRandomizedState(RandomizedState.Unrandomized, instance);
                }
            }
            foreach (var i in instance.ItemPool)
            {
                i.Value.AmountInStartingpool = 0;
            }
        }

        private static List<Tuple<string, string>> GetItemLocationData(List<string> itemLocations)
        {
            List<Tuple<string, string>> ItemLocationDict = new List<Tuple<string, string>>();
            foreach (var ItemLocation in itemLocations)
            {
                if (!ItemLocation.Contains("->")) { continue; }
                string[] Data = ItemLocation.Split("->");
                string Location = Data[0].Trim();
                if (Location.StartsWith("- ")) { continue; }
                string Item = Data[1].Replace("*", "").Replace("^", "").Trim();
                ItemLocationDict.Add(new(Location, Item));
            }
            return ItemLocationDict;
        }

        private static List<string> GetJunkLocations(List<string> itemLocations)
        {
            List<string> locations = new List<string>();
            foreach (var ItemLocation in itemLocations)
            {
                if (!ItemLocation.Contains("->")) { continue; }
                string[] Data = ItemLocation.Split("->");
                string Location = Data[0].Trim();
                if (Location.StartsWith("- ")) { locations.Add(Location[2..]); }
            }
            return locations;
        }

        private static void ApplySettings(InstanceData.TrackerInstance instance, Dictionary<string, object> Settings)
        {
            List<string> AllOptions = instance.ChoiceOptions.Keys.Concat(instance.MultiSelectOptions.Keys).Concat(instance.ToggleOptions.Keys).Concat(instance.IntOptions.Keys).ToList();
            List<string> ValidSpoilerOptions = new List<string>();

            foreach (var i in Settings)
            {
                if (instance.ChoiceOptions.ContainsKey(i.Key))
                {
                    ValidSpoilerOptions.Add(i.Key);
                    var Option = instance.ChoiceOptions[i.Key];
                    string Value = i.Value.SerializeConvert<string>();
                    if (!Option.ValueList.ContainsKey(Value)) { throw new Exception($"{Option.ID} Did not contain Value {Value}"); }
                    Option.SetValue(Value);
                }
                else if (instance.MultiSelectOptions.ContainsKey(i.Key))
                {
                    ValidSpoilerOptions.Add(i.Key);
                    var Option = instance.MultiSelectOptions[i.Key];
                    Option.EnabledValues.Clear();
                    string[] Values = i.Value.SerializeConvert<string>().Split(",").Select(x => x.Trim()).ToArray();
                    foreach (var Value in Values)
                    {
                        if (!Option.ValueList.ContainsKey(Value)) { continue; }
                        Option.ToggleValue(Value);
                    }
                }
                else if (instance.ToggleOptions.ContainsKey(i.Key))
                {
                    ValidSpoilerOptions.Add(i.Key);
                    var Option = instance.ToggleOptions[i.Key];
                    bool Value = i.Value.SerializeConvert<bool>();
                    Option.SetValue(Value);
                }
                else if (instance.IntOptions.ContainsKey(i.Key))
                {
                    ValidSpoilerOptions.Add(i.Key);
                    var Option = instance.IntOptions[i.Key];
                    int Value = i.Value.SerializeConvert<int>();
                    if (Value > Option.Max || Value < Option.Min) { throw new Exception($"{Option.ID} valid range {Option.Max}<>{Option.Min} did not include {Value}"); }
                    Option.SetValue(Value);
                }
            }

            var MissingSettings = AllOptions.Where(x => !ValidSpoilerOptions.Contains(x));
            Debug.WriteLine(MissingSettings.ToFormattedJson());
        }
    }

    public static class SettingStringHandler
    {
        public static bool ApplyLocationString(string LocationString, InstanceData.TrackerInstance Instance)
        {
            var LocationPool = Instance.LocationPool.Values.Where(x => !x.GetDictEntry(Instance).IgnoreForSettingString ?? true).ToList();

            var ItemGroupCount = (int)Math.Ceiling(LocationPool.Count / 32.0);

            var RandomizedItemIndexs = ParseMMRSettingString(LocationString, ItemGroupCount);
            if (RandomizedItemIndexs == null) { return false; }

            int Index = 0;
            foreach (var i in LocationPool)
            {
                bool IsRandomized = RandomizedItemIndexs.Contains(Index);
                if (IsRandomized && i.IsUnrandomized())
                {
                    i.SetRandomizedState(RandomizedState.Randomized, Instance);
                }
                else if (!IsRandomized && !i.IsUnrandomized())
                {
                    i.SetRandomizedState(RandomizedState.Unrandomized, Instance);
                }
                Index++;
            }
            return true;
        }

        public static bool ApplyJunkString(string LocationString, InstanceData.TrackerInstance Instance)
        {
            var LocationPool = Instance.LocationPool.Values.Where(x => !x.GetDictEntry(Instance).IgnoreForSettingString ?? true).ToList();

            var ItemGroupCount = (int)Math.Ceiling(LocationPool.Count / 32.0);

            var JunkItemIndexes = ParseMMRSettingString(LocationString, ItemGroupCount);
            if (JunkItemIndexes == null) { return false; }

            int Index = 0;
            foreach (var i in LocationPool)
            {
                bool IsJunk = JunkItemIndexes.Contains(Index);
                if (IsJunk && i.IsRandomized())
                {
                    i.SetRandomizedState(RandomizedState.ForcedJunk, Instance);
                }
                else if (!IsJunk && i.IsJunk())
                {
                    i.SetRandomizedState(RandomizedState.Randomized, Instance);
                }
                Index++;
            }
            return true;
        }

        public static bool ApplyStartingItemString(string ItemString, InstanceData.TrackerInstance Instance)
        {
            var StartingItems = GetStartingItemList(Instance);

            var ItemGroupCount = (int)Math.Ceiling(StartingItems.Count / 32.0);

            var StartingItemIndexes = ParseMMRSettingString(ItemString, ItemGroupCount);
            if (StartingItemIndexes == null) { return false; }

            foreach (var i in StartingItems.Distinct())
            {
                i.AmountInStartingpool = 0;
            }

            int Index = 0;
            foreach (var i in StartingItems)
            {
                bool AddStartingItem = StartingItemIndexes.Contains(Index);
                if (AddStartingItem) { i.AmountInStartingpool++; }
                Index++;
            }
            return true;
        }

        public static List<ItemData.ItemObject> GetStartingItemList(InstanceData.TrackerInstance Instance)
        {
            List<ItemData.ItemObject> StartingItems = new List<ItemData.ItemObject>();
            foreach (var i in Instance.ItemPool.Values)
            {
                var DictEntry = i.GetDictEntry(Instance);
                bool ValidStartingItem = DictEntry.ValidStartingItem ?? true;
                if (!ValidStartingItem) { continue; }
                if (i.GetDictEntry(Instance).IgnoreForSettingString ?? false) { continue; }
                //This has to be the unedited max amount in world since the setting string should stay consistance even if an option changes this value
                int MaxInWorld = DictEntry.MaxAmountInWorld??-1;
                if (MaxInWorld > 5 || MaxInWorld < 0) { MaxInWorld = 5; }

                for (var j = 0; j < MaxInWorld; j++)
                {
                    StartingItems.Add(i);
                }
            }
            return StartingItems;
        }

        public static string CreateSettingString(IEnumerable<object> MasterList, IEnumerable<object> SubList)
        {
            var ItemGroupCount = (int)Math.Ceiling(MasterList.Count() / 32.0);

            int[] n = new int[ItemGroupCount];
            string[] ns = new string[ItemGroupCount];
            foreach (var item in SubList)
            {
                var i = MasterList.ToList().IndexOf(item);
                int j = i / 32;
                int k = i % 32;
                n[j] |= 1 << k;
                ns[j] = Convert.ToString(n[j], 16);
            }
            return string.Join("-", ns.Reverse());
        }

        public static List<int> ParseMMRSettingString(string SettingString, int ItemCount)
        {
            var result = new List<int>();
            if (string.IsNullOrWhiteSpace(SettingString)) { Debug.WriteLine("String Empty"); return result; }

            result.Clear();
            string[] Sections = SettingString.Split('-');
            int[] NewSections = new int[ItemCount];
            if (Sections.Length != NewSections.Length) { Debug.WriteLine($"{Sections.Length} != {NewSections.Length}"); return null; }

            try
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    if (Sections[ItemCount - 1 - i] != "") { NewSections[i] = Convert.ToInt32(Sections[ItemCount - 1 - i], 16); }
                }
                for (int i = 0; i < 32 * ItemCount; i++)
                {
                    int j = i / 32;
                    int k = i % 32;
                    if ((NewSections[j] >> k & 1) > 0) { result.Add(i); }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"It broke {e.Message}");
                return null;
            }
            return result;
        }
    }
}
