using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public static class SpoilerLogTools
    {

        public static bool ApplyLocationString(string LocationString, LogicObjects.TrackerInstance Instance)
        {
            var LocationPool = Instance.LocationPool.Values.Where(x => !x.GetDictEntry(Instance).IgnoreForSettingString ?? true).ToList();

            var ItemGroupCount = (int)Math.Ceiling(LocationPool.Count / 32.0);

            var RandomizedItemIndexs = Utility.ParseLocationAndJunkSettingString(LocationString, ItemGroupCount, "Location");
            if (RandomizedItemIndexs == null) { return false; }

            int Index = 0;
            foreach (var i in LocationPool)
            {
                bool IsRandomized = RandomizedItemIndexs.Contains(Index);
                if (IsRandomized && i.IsUnrandomized())
                {
                    i.SetRandomizedState(MiscData.RandomizedState.Randomized, Instance);
                }
                else if (!IsRandomized && !i.IsUnrandomized())
                {
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized, Instance);
                }
                Index++;
            }
            return true;
        }

        public static bool ApplyJunkString(string LocationString, LogicObjects.TrackerInstance Instance)
        {
            var LocationPool = Instance.LocationPool.Values.Where(x => !x.GetDictEntry(Instance).IgnoreForSettingString ?? true).ToList();

            var ItemGroupCount = (int)Math.Ceiling(LocationPool.Count / 32.0);

            var JunkItemIndexes = Utility.ParseLocationAndJunkSettingString(LocationString, ItemGroupCount, "Location");
            if (JunkItemIndexes == null) { return false; }

            int Index = 0;
            foreach (var i in LocationPool)
            {
                bool IsJunk = JunkItemIndexes.Contains(Index);
                if (IsJunk && i.IsRandomized())
                {
                    i.SetRandomizedState(MiscData.RandomizedState.ForcedJunk, Instance);
                }
                else if (!IsJunk && i.IsJunk())
                {
                    i.SetRandomizedState(MiscData.RandomizedState.Randomized, Instance);
                }
                Index++;
            }
            return true;
        }

        public static bool ApplyStartingItemString(string ItemString, LogicObjects.TrackerInstance Instance)
        {
            var StartingItems = GetStartingItemList(Instance);

            var ItemGroupCount = (int)Math.Ceiling(StartingItems.Count / 32.0);

            var StartingItemIndexes = Utility.ParseLocationAndJunkSettingString(ItemString, ItemGroupCount, "Location");
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

        public static List<ItemData.ItemObject> GetStartingItemList(LogicObjects.TrackerInstance Instance)
        {
            List<ItemData.ItemObject> StartingItems = new List<ItemData.ItemObject>();
            foreach (var i in Instance.ItemPool.Values)
            {
                var DictEntry = i.GetDictEntry(Instance);
                bool ValidStartingItem = DictEntry.ValidStartingItem ?? true;
                if (!ValidStartingItem) { continue; }
                int MaxInWorld = DictEntry.MaxAmountInWorld ?? -1;
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
                n[j] |= (int)(1 << k);
                ns[j] = Convert.ToString(n[j], 16);
            }
            return string.Join("-", ns.Reverse());
        }

        public static MMRData.SpoilerLogData ReadSpoilerLog(string[] File)
        {
            MMRData.SpoilerLogData spoilerLog = new MMRData.SpoilerLogData();
            string CurrentSection = "";
            string SettingString = "{";
            foreach(var line in File)
            {
                if (line.Trim().StartsWith("Settings:")) { CurrentSection = "Settings"; continue; }
                if (line.Trim().StartsWith("Seed:")) { CurrentSection = "Seed"; continue; }
                if (line.Trim().StartsWith("Entrance") && line.Contains("Destination")) { CurrentSection = "Dungeon"; continue; }
                if (line.Trim().StartsWith("Location") && line.Contains("Item")) { CurrentSection = "Location"; continue; }
                if (line.Trim().StartsWith("Gossip Stone") && line.Contains("Message")) { CurrentSection = "Gossip"; continue; }
                if (line.Trim().StartsWith("Name") && line.Contains("Cost")) { CurrentSection = "Price"; continue; }

                if (CurrentSection == "Settings")
                {
                    SettingString += line;
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(line) && line.Contains("->"))
                {
                    var Entry = line.Split(new string[] { "->" }, StringSplitOptions.None).Select(x => x.Replace("*","").Replace("^", "")).ToArray();
                    if ((CurrentSection == "Location" || CurrentSection == "Dungeon") && !spoilerLog.LocationLog.ContainsKey(Entry[0].Trim()))
                    {
                        spoilerLog.LocationLog.Add(Entry[0].Trim(), Entry[1].Trim());
                    }
                    else if (CurrentSection == "Price" && !spoilerLog.PriceLog.ContainsKey(Entry[0].Trim()))
                    {
                        spoilerLog.PriceLog.Add(Entry[0].Trim(), int.Parse(Entry[1].Trim()));
                    }
                    else if (CurrentSection == "Gossip" && !spoilerLog.GossipLog.ContainsKey(Entry[0].Trim()))
                    {
                        spoilerLog.GossipLog.Add(Entry[0].Trim(), Entry[1].Trim());
                    }
                }
            }
            spoilerLog.GameplaySettings = Newtonsoft.Json.JsonConvert.DeserializeObject<MMRData.GameplaySettings>(SettingString);
            return spoilerLog;
        }

        public static void ApplyMMRandoSettings(this LogicObjects.TrackerInstance instance, MMRData.SpoilerLogData Log)
        {
            //Apply setting strings and enable tricks
            Debug.WriteLine($"Applying Setting Strings");
            ApplyLocationString(Log.GameplaySettings.CustomItemListString, instance);
            ApplyJunkString(Log.GameplaySettings.CustomJunkLocationsString, instance);
            ApplyStartingItemString(Log.GameplaySettings.CustomStartingItemListString, instance);
            Debug.WriteLine($"setting Tricks");
            foreach (var i in instance.MacroPool.Values.Where(x => x.isTrick(instance)))
            {
                i.TrickEnabled = Log.GameplaySettings.EnabledTricks.Contains(i.ID);
            }
            //If hints are vanilla no need to track them
            Debug.WriteLine($"Enabling Hints");
            foreach (var i in instance.HintPool)
            {
                i.Value.RandomizedState = Log.GameplaySettings.GossipHintStyle == "Default" ? MiscData.RandomizedState.ForcedJunk : MiscData.RandomizedState.Randomized;
            }
            //If no StartingItemMode = None, starting locations should be junk. SongHealing is only junked if songs are mixed with items
            Debug.WriteLine($"Handeling MM Starting Locations");
            List<string> StartingItems = new() { "StartingSword", "StartingShield", "StartingHeartContainer1", "StartingHeartContainer2", "MaskDeku" };
            if (Log.GameplaySettings.AddSongs) { StartingItems.Add("SongHealing"); }
            foreach (var i in StartingItems)
            {
                if (Log.GameplaySettings.StartingItemMode != "None") { break; }
                var Entry = instance.GetLocationByID(i);
                if (!Entry.IsUnrandomized()) { Entry.SetRandomizedState(MiscData.RandomizedState.ForcedJunk, instance); }
            }

            Debug.WriteLine($"Handeling Dungeon Entrances");
            //Dungeon entrances are not tracked in the CustomItemListString and should instead be randomized based on the RandomizeDungeonEntrances setting
            var DungeoneState = Log.GameplaySettings.RandomizeDungeonEntrances ? MiscData.RandomizedState.Randomized : MiscData.RandomizedState.Unrandomized;
            instance.GetLocationByID("AreaWoodFallTempleAccess").SetRandomizedState(DungeoneState, instance);
            instance.GetLocationByID("AreaSnowheadTempleAccess").SetRandomizedState(DungeoneState, instance);
            instance.GetLocationByID("AreaGreatBayTempleAccess").SetRandomizedState(DungeoneState, instance);
            instance.GetLocationByID("AreaInvertedStoneTowerTempleAccess").SetRandomizedState(DungeoneState, instance);

            Debug.WriteLine($"Handeling Options");
            //These options are not hard coded to the tracker, but set them if they exist.
            if (instance.UserOptions.ContainsKey("ProgressiveItems"))
            {
                instance.UserOptions["ProgressiveItems"].CurrentValue = Log.GameplaySettings.ProgressiveUpgrades ? "enabled" : "disabled";
            }
            if (instance.UserOptions.ContainsKey("SmallKeysy"))
            {
                instance.UserOptions["SmallKeysy"].CurrentValue = Log.GameplaySettings.SmallKeyMode.Contains("DoorsOpen") ? "enabled" : "disabled";
            }
            if (instance.UserOptions.ContainsKey("BossKeysy"))
            {
                instance.UserOptions["BossKeysy"].CurrentValue = Log.GameplaySettings.BossKeyMode.Contains("DoorsOpen") ? "enabled" : "disabled";
            }
            if (instance.UserOptions.ContainsKey("BYOA"))
            {
                instance.UserOptions["BYOA"].CurrentValue = Log.GameplaySettings.ByoAmmo ? "enabled" : "disabled";
            }

            Debug.WriteLine($"Getting all Logic Items");
            var AllLogicItems = instance.LogicFile.GetAllItemsUsedInLogic();
            Debug.WriteLine($"Making Unrandomized Items Manual and Junking Unrandomized Starting Items");
            foreach (var i in instance.LocationPool)
            {   
                //Any unrandomized locations with items that effect logic should be made Manual to track obtaining the item and updating logic
                if (i.Value.IsUnrandomized(1) && AllLogicItems.Contains(i.Value.GetDictEntry(instance).OriginalItem))
                {
                    i.Value.SetRandomizedState(MiscData.RandomizedState.UnrandomizedManual, instance);
                }
                //If a location is unrandomized, but it's unrandomized item is a starting item it will contain junk. This is MMR sepcific
                if (i.Value.IsUnrandomized() && instance.GetItemByID(i.Value.GetDictEntry(instance).OriginalItem).AmountInStartingpool > 0)
                {
                    if (StartingItems.Contains(i.Key)) { continue; } //Don't do this for starting items since you automatcially get them
                    i.Value.SetRandomizedState(MiscData.RandomizedState.ForcedJunk, instance);
                }
            }
        }

        public static void ApplyMMRandoSpoilerLog(this LogicObjects.TrackerInstance instance, MMRData.SpoilerLogData Log)
        {
            foreach(var i in instance.LocationPool.Values)
            {
                var DictEntry = i.GetDictEntry(instance);
                var MatchingLocation = Log.LocationLog.Where(x => DictEntry.SpoilerData.SpoilerLogNames.Contains(x.Key));
                if (!MatchingLocation.Any()) 
                {
                    if (i.IsRandomized())
                    {
                        Debug.WriteLine($"{i.ID} was not found in the spoiler log and was randomized");
                    }
                    continue; 
                }
                var ItemName = MatchingLocation.First().Value;
                var MatchingItems = instance.ItemPool.Values.Where(x => x.GetDictEntry(instance).SpoilerData.SpoilerLogNames.Contains(ItemName));
                var PlaceablegItems = MatchingItems.Where(x => x.CanBePlaced(instance));
                if (!MatchingItems.Any())
                {
                    Debug.WriteLine($"{ItemName} Could not placed at {i.ID}: No items match name {ItemName}");
                    continue;
                }
                else if (!PlaceablegItems.Any())
                {
                    Debug.WriteLine($"{ItemName} Could not placed at {i.ID}: No items {ItemName} could be placed");
                    continue;
                }
                i.Randomizeditem.SpoilerLogGivenItem = PlaceablegItems.First().Id;
            }
            foreach (var i in instance.LocationPool.Values)
            {
                var DictEntry = i.GetDictEntry(instance);
                var MatchingLocations = Log.PriceLog.Where(x => DictEntry.SpoilerData.PriceDataNames.Contains(x.Key));
                if (!MatchingLocations.Any())
                {
                    //Debug.WriteLine($"{i.ID} was not found in the Price log");
                    continue;
                }
                i.CheckPrice = MatchingLocations.Select(x => x.Value).Min();
                Debug.WriteLine($"{i.ID} was assigned a price of {i.CheckPrice}");
            }
            foreach (var i in instance.HintPool.Values)
            {
                var DictEntry = i.GetDictEntry(instance);
                var MatchingLocation = Log.GossipLog.Where(x => DictEntry.SpoilerData.SpoilerLogNames.Contains(x.Key));
                if (!MatchingLocation.Any())
                {
                    Debug.WriteLine($"{i.ID} was not found in the hint log");
                    continue;
                }
                i.SpoilerHintText = MatchingLocation.First().Value;
            }
        }

        public static void RemoveSpoilerData(this LogicObjects.TrackerInstance instance)
        {
            foreach (var i in instance.LocationPool.Values)
            {
                i.Randomizeditem.SpoilerLogGivenItem = null;
                i.CheckPrice = -1;
            }
            foreach (var i in instance.HintPool.Values)
            {
                i.SpoilerHintText = null;
            }
        }
    }
}
