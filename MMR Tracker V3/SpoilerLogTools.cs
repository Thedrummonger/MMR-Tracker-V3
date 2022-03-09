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
            Debug.WriteLine($"Applying String {LocationString}");
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
                    i.RandomizedState = MiscData.RandomizedState.Randomized;
                }
                else if (!IsRandomized && !i.IsUnrandomized())
                {
                    i.RandomizedState = MiscData.RandomizedState.Unrandomized;
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
                if (IsJunk && i.RandomizedState == MiscData.RandomizedState.Randomized)
                {
                    i.RandomizedState = MiscData.RandomizedState.ForcedJunk;
                }
                else if (!IsJunk && i.RandomizedState == MiscData.RandomizedState.ForcedJunk)
                {
                    i.RandomizedState = MiscData.RandomizedState.Randomized;
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
                if (line.Trim().StartsWith("Entrance")) { CurrentSection = "Entrance"; continue; }
                if (line.Trim().StartsWith("Location")) { CurrentSection = "Location"; continue; }
                if (line.Trim().StartsWith("Gossip Stone") && line.Contains("Message")) { CurrentSection = "Gossip"; continue; }
                if (line.Trim().StartsWith("Name") && line.Contains("Cost")) { CurrentSection = "Price"; continue; }

                if (CurrentSection == "Settings")
                {
                    SettingString += line;
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(line) && line.Contains("->"))
                {
                    var Entry = line.Split(new string[] { "->" }, StringSplitOptions.None);
                    if (CurrentSection == "Entrance")
                    {
                        spoilerLog.DungeonLog.Add(Entry[0].Trim(), Entry[1].Trim());
                    }
                    else if (CurrentSection == "Location")
                    {
                        spoilerLog.LocationLog.Add(Entry[0].Trim(), Entry[1].Trim());
                    }
                    else if (CurrentSection == "Price")
                    {
                        spoilerLog.PriceLog.Add(Entry[0].Trim(), int.Parse(Entry[1].Trim()));
                    }
                    else if (CurrentSection == "Gossip")
                    {
                        spoilerLog.GossipLog.Add(Entry[0].Trim(), Entry[1].Trim());
                    }
                }
            }
            spoilerLog.GameplaySettings = Newtonsoft.Json.JsonConvert.DeserializeObject<MMRData.GameplaySettings>(SettingString);
            return spoilerLog;
        }

        public static void ApplyRandoSettings(this LogicObjects.TrackerInstance instance, MMRData.SpoilerLogData Log)
        {
            ApplyLocationString(Log.GameplaySettings.CustomItemListString, instance);
            ApplyJunkString(Log.GameplaySettings.CustomJunkLocationsString, instance);
            ApplyStartingItemString(Log.GameplaySettings.CustomStartingItemListString, instance);
            foreach (var i in instance.MacroPool.Values.Where(x => x.isTrick(instance)))
            {
                i.TrickEnabled = Log.GameplaySettings.EnabledTricks.Contains(i.ID);
            }

            List<string> StartingItems = new() { "StartingSword", "StartingShield", "StartingHeartContainer1", "StartingHeartContainer2", "MaskDeku" };
            if (Log.GameplaySettings.AddSongs) { StartingItems.Add("SongHealing"); }
            foreach (var i in StartingItems)
            {
                if (Log.GameplaySettings.StartingItemMode != "None") { break; }
                var Entry = instance.GetLocationByID(i);
                if (!Entry.IsUnrandomized()) { Entry.RandomizedState = MiscData.RandomizedState.ForcedJunk; }
            }

            instance.GetLocationByID("AreaWoodFallTempleAccess").RandomizedState = Log.GameplaySettings.RandomizeDungeonEntrances ? MiscData.RandomizedState.Randomized : MiscData.RandomizedState.Unrandomized;
            instance.GetLocationByID("AreaSnowheadTempleAccess").RandomizedState = Log.GameplaySettings.RandomizeDungeonEntrances ? MiscData.RandomizedState.Randomized : MiscData.RandomizedState.Unrandomized;
            instance.GetLocationByID("AreaGreatBayTempleAccess").RandomizedState = Log.GameplaySettings.RandomizeDungeonEntrances ? MiscData.RandomizedState.Randomized : MiscData.RandomizedState.Unrandomized;
            instance.GetLocationByID("AreaInvertedStoneTowerTempleAccess").RandomizedState = Log.GameplaySettings.RandomizeDungeonEntrances ? MiscData.RandomizedState.Randomized : MiscData.RandomizedState.Unrandomized;

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
            foreach(var i in instance.HintPool)
            {
                i.Value.RandomizedState = Log.GameplaySettings.GossipHintStyle == "Default" ? MiscData.RandomizedState.ForcedJunk : MiscData.RandomizedState.Randomized;
            }

        }
    }
}
