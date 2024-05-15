using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMUtils;

namespace MMR_Tracker_V3.SpoilerLogHandling.HardCodedParsers
{
    internal class TPR
    {
        public class TPRSpoilerLog
        {
            public string[] requiredDungeons { get; set; }
            public Dictionary<string, string> itemPlacements { get; set; }
            public Dictionary<string, object> settings { get; set; }
        }
        public static GenericSpoilerLog readAndApplySpoilerLog(InstanceData.TrackerInstance I, Dictionary<string, object> DataStore)
        {
            GenericSpoilerLog Result = new GenericSpoilerLog();
            var SpoilerLog = DataStore["SpoilerLog"] as string[];
            var SpoilerData = JsonConvert.DeserializeObject<TPRSpoilerLog>(string.Join("", SpoilerLog));
            foreach (var i in SpoilerData.settings)
            {
                if (I.ChoiceOptions.ContainsKey(i.Key))
                {
                    Result.ChoiceOptionAssignment.Add(i.Key, i.Value.ToString());
                }
                else if (I.ToggleOptions.ContainsKey(i.Key))
                {
                    Result.ToggleOptionAssignment.Add(i.Key, bool.Parse(i.Value.ToString()));
                }
                else if (I.IntOptions.ContainsKey(i.Key))
                {
                    Result.IntOptionAssignment.Add(i.Key, int.Parse(i.Value.ToString()));
                }
                if (i.Key == "startingItems")
                {
                    Debug.WriteLine(i.Value.GetType());
                    foreach (var item in TDMUtils.Utility.SerializeConvert<List<string>>(i.Value))
                    {
                        var ItemObj = I.GetItemByID(item);
                        if (ItemObj is null)
                        {
                            Debug.WriteLine($"{i.Value} was not a valid Starting Item!");
                            continue;
                        }
                        Result.StartingItems.SetIfEmpty(ItemObj.ID, 0);
                        Result.StartingItems[ItemObj.ID]++;
                    }
                }
                if (i.Key == "excludedChecks")
                {
                    foreach (var Location in TDMUtils.Utility.SerializeConvert<List<string>>(i.Value))
                    {
                        var LocationObj = I.GetLocationByID(Location);
                        if (LocationObj is null)
                        {
                            Debug.WriteLine($"{i.Value} was not a valid Junk Location!");
                            continue;
                        }
                        Result.JunkLocations.Add(LocationObj.ID);
                    }
                }
            }
            foreach (var i in SpoilerData.itemPlacements)
            {
                var Location = I.GetLocationByID(i.Key);
                var item = I.GetItemByID(i.Value);
                if (i.Value == "Vanilla" && Location is not null) 
                {
                    Result.JunkLocations.Add(Location.ID);
                    continue; 
                }
                if (Location is null)
                {
                    Debug.WriteLine($"{i.Key} was not a valid location!");
                    continue;
                }
                if (item is null)
                {
                    Debug.WriteLine($"{i.Value} was not a valid Item!");
                }
                Result.LocationAssignment.Add(Location.ID, item?.ID ?? i.Value);
            }
            foreach (var i in I.LocationPool.Values)
            {
                if (!Result.LocationAssignment.ContainsKey(i.ID) && i.IsRandomized() && string.IsNullOrWhiteSpace(i.SingleValidItem))
                {
                    Debug.WriteLine($"{i.ID} Was not found in the spoiler log!");
                }
            }
            return Result;
        }
    }
}
