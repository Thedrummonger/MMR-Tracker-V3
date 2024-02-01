using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.SpoilerLogImporter
{
    public class TPRSpoilerLogTools
    {
        public class TPRSpoilerLog
        {
            public string[] requiredDungeons { get; set; }
            public Dictionary<string, string> itemPlacements { get; set; }
            public Dictionary<string, object> settings { get; set; }
        }
        public static void readAndApplySpoilerLog(TrackerObjects.InstanceData.TrackerInstance Instance)
        {
            var SpoilerData = JsonConvert.DeserializeObject<TPRSpoilerLog>(string.Join("", Instance.SpoilerLog.Log));
            foreach (var i in SpoilerData.settings)
            {
                if (Instance.ChoiceOptions.ContainsKey(i.Key))
                {
                    Instance.ChoiceOptions[i.Key].SetValue(i.Value.ToString());
                }
                else if (Instance.ToggleOptions.ContainsKey(i.Key))
                {
                    Instance.ToggleOptions[i.Key].SetValue(bool.Parse(i.Value.ToString()));
                }
                else if (Instance.IntOptions.ContainsKey(i.Key))
                {
                    Instance.IntOptions[i.Key].SetValue(int.Parse(i.Value.ToString()));
                }
                if (i.Key == "startingItems")
                {
                    Debug.WriteLine(i.Value.GetType());
                    foreach (var item in Utility.SerializeConvert<List<string>>(i.Value))
                    {
                        var ItemObj = Instance.GetItemByID(item);
                        if (ItemObj is null)
                        {
                            Debug.WriteLine($"{i.Value} was not a valid Starting Item!");
                            continue;
                        }
                        ItemObj.AmountInStartingpool++;
                    }
                }
                if (i.Key == "excludedChecks")
                {
                    foreach (var Location in Utility.SerializeConvert<List<string>>(i.Value))
                    {
                        var LocationObj = Instance.GetLocationByID(Location);
                        if (LocationObj is null)
                        {
                            Debug.WriteLine($"{i.Value} was not a valid Junk Location!");
                            continue;
                        }
                        LocationObj.SetRandomizedState(MiscData.RandomizedState.ForcedJunk);
                    }
                }
            }
            foreach (var i in SpoilerData.itemPlacements)
            {
                var Location = Instance.GetLocationByID(i.Key);
                var item = Instance.GetItemByID(i.Value);
                if (i.Value == "Vanilla" && Location is not null) { Location.SetRandomizedState(MiscData.RandomizedState.ForcedJunk); continue; }
                if (Location is null)
                {
                    Debug.WriteLine($"{i.Key} was not a valid location!");
                    continue;
                }
                if (item is null)
                {
                    Debug.WriteLine($"{i.Value} was not a valid Item!");
                }
                else if (Instance.GetItemToPlace(i.Value) == null)
                {
                    Debug.WriteLine($"{i.Value} was placed more times than allowed!");
                }
                Location.Randomizeditem.SpoilerLogGivenItem = item?.ID ?? i.Value;
            }
            foreach (var i in Instance.LocationPool.Values)
            {
                if (string.IsNullOrWhiteSpace(i.Randomizeditem.SpoilerLogGivenItem) && i.IsRandomized() && string.IsNullOrWhiteSpace(i.SingleValidItem))
                {
                    Debug.WriteLine($"{i.ID} Was not found in the spoiler log!");
                }
            }
        }
    }
}
