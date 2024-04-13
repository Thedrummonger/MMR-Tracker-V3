using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.SpoilerLogHandling
{
    public static class SettingStringHandler
    {
        public static bool ApplyLocationString(string LocationString, InstanceData.TrackerInstance Instance)
        {
            var LocationPool = Instance.LocationPool.Values.Where(x => !x.GetDictEntry().IgnoreForSettingString ?? true).ToList();

            var ItemGroupCount = (int)Math.Ceiling(LocationPool.Count / 32.0);

            var RandomizedItemIndexs = ParseMMRSettingString(LocationString, ItemGroupCount);
            if (RandomizedItemIndexs == null) { return false; }

            int Index = 0;
            foreach (var i in LocationPool)
            {
                bool IsRandomized = RandomizedItemIndexs.Contains(Index);
                if (IsRandomized && i.IsUnrandomized())
                {
                    i.SetRandomizedState(RandomizedState.Randomized);
                }
                else if (!IsRandomized && !i.IsUnrandomized())
                {
                    i.SetRandomizedState(RandomizedState.Unrandomized);
                }
                Index++;
            }
            return true;
        }

        public static bool ApplyJunkString(string LocationString, InstanceData.TrackerInstance Instance)
        {
            var LocationPool = Instance.LocationPool.Values.Where(x => !x.GetDictEntry().IgnoreForSettingString ?? true).ToList();

            var ItemGroupCount = (int)Math.Ceiling(LocationPool.Count / 32.0);

            var JunkItemIndexes = ParseMMRSettingString(LocationString, ItemGroupCount);
            if (JunkItemIndexes == null) { return false; }

            int Index = 0;
            foreach (var i in LocationPool)
            {
                bool IsJunk = JunkItemIndexes.Contains(Index);
                if (IsJunk && i.IsRandomized())
                {
                    i.SetRandomizedState(RandomizedState.ForcedJunk);
                }
                else if (!IsJunk && i.IsJunk())
                {
                    i.SetRandomizedState(RandomizedState.Randomized);
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
                var DictEntry = i.GetDictEntry();
                bool ValidStartingItem = DictEntry.ValidStartingItem ?? true;
                if (!ValidStartingItem) { continue; }
                if (i.GetDictEntry().IgnoreForSettingString ?? false) { continue; }
                //This has to be the unedited max amount in world since the setting string should stay consistance even if an option changes this value
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
