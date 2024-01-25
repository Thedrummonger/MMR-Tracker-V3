using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MMR_Tracker_V3
{
    public class PriceRando
    {
        public static List<string> GetAllWalletLogicEntries(InstanceData.TrackerInstance instance)
        {
            bool CurrentInitializedState = instance.PriceData.Initialized;
            instance.PriceData.Initialized = false;

            List<string> WalletEntries = new();
            foreach (var i in instance.ItemPool)
            {
                if (i.Value.GetDictEntry().WalletCapacity != null)
                {
                    WalletEntries.Add(i.Key);
                }
            }
            foreach (var i in instance.MacroPool)
            {
                if (i.Value.GetDictEntry().WalletCapacity != null)
                {
                    WalletEntries.Add(i.Key);
                }
            }

            while (true)
            {
                if (!ScanMacros()) { break; }
            }

            instance.PriceData.Initialized = CurrentInitializedState;

            return WalletEntries;

            bool ScanMacros()
            {
                bool NewWalletFound = false;
                foreach (var i in instance.MacroPool)
                {
                    if (WalletEntries.Contains(i.Key)) { continue; }
                    var Logic = instance.GetLogic(i.Key);
                    if (IsWalletmacro(Logic))
                    {
                        WalletEntries.Add(i.Key);
                        NewWalletFound = true;
                    }
                }
                return NewWalletFound;
            }

            bool IsWalletmacro(MMRData.JsonFormatLogicItem Logic)
            {
                if (!Logic.RequiredItems.Any() && !Logic.ConditionalItems.Any()) { return false; }
                foreach (var i in Logic.RequiredItems)
                {
                    if (!WalletEntries.Contains(i)) { return false; }
                }
                foreach (var cond in Logic.ConditionalItems)
                {
                    foreach (var i in cond)
                    {
                        if (!WalletEntries.Contains(i)) { return false; }
                    }
                }
                return true;
            }

        }

        public static bool TestForPriceData(dynamic Object)
        {
            try
            {
                Object.GetPrice(out int p, out char c);
                Debug.WriteLine("Had Price Function");
                return true;
            }
            catch (Exception e)
            {

                Debug.WriteLine($"NOT Had Price Function {e}");
                return false;
            }
        }
    }
}
