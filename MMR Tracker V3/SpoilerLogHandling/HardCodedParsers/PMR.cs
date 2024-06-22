using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TDMUtils;

namespace MMR_Tracker_V3.SpoilerLogHandling.HardCodedParsers
{
    internal class PMR
    {
        public static GenericSpoilerLog ParseSpoiler(InstanceData.TrackerInstance Instance, Dictionary<string, object> DataStore)
        {
            if (DataStore.ContainsKey("Archipelago"))
            {
                return ParseAPSpoiler(Instance, DataStore);
            }
            else
            {
                return ParseSpoilerLogFile(Instance, DataStore);
            }
        }

        private static GenericSpoilerLog ParseSpoilerLogFile(InstanceData.TrackerInstance instance, Dictionary<string, object> dataStore)
        {
            return null;
        }

        private static GenericSpoilerLog ParseAPSpoiler(InstanceData.TrackerInstance Instance, Dictionary<string, object> DataStore)
        {
            GenericSpoilerLog Result = new();
            if (DataStore["Archipelago"] is not Archipelago.GenericAPSpoiler genericAPSpoiler) { return null; }
            if (DataStore.TryGetValue("APPlayerFile", out object PF) && PF is string[] APPlayerFile && APPlayerFile.Any())
            {
                var DataFileJsonString = DataFileUtilities.ConvertYamlStringToJsonString(string.Join(Environment.NewLine, APPlayerFile));
                var DataFile = JsonConvert.DeserializeObject<Dictionary<string, object>>(DataFileJsonString);
                var PMRData = DataFile["Paper Mario"].SerializeConvert<Dictionary<string, object>>();
                foreach(var entry in PMRData)
                {
                    if (Instance.IntOptions.TryGetValue(entry.Key, out OptionData.IntOption intOption))
                    {
                        Result.IntOptionAssignment.Add(intOption.ID, entry.Value.AsIntValue());
                    }
                    else if (Instance.ToggleOptions.TryGetValue(entry.Key, out OptionData.ToggleOption toggleOption))
                    {
                        Result.ToggleOptionAssignment.Add(toggleOption.ID, entry.Value.IsTruthy());
                    }
                    else if (Instance.ChoiceOptions.TryGetValue(entry.Key, out OptionData.ChoiceOption choiceOption))
                    {
                        Result.ChoiceOptionAssignment.Add(choiceOption.ID, choiceOption.GetDynValue(entry.Value));
                    }
                }
            }

            foreach (var entry in genericAPSpoiler.Locations)
            {
                var Location = Instance.LocationPool.First(x => x.Value.GetDictEntry().SpoilerData.NetIDs.Contains(entry.Location));
                if (Instance.GetParentContainer().netConnection.PlayerID != entry.Player)
                {
                    Result.LocationAssignment.Add(Location.Key, entry.Item);
                    Result.LocationPlayerAssignment.Add(Location.Key, entry.Player);
                }
                else
                {
                    var Item = Instance.ItemPool.First(x => x.Value.GetDictEntry().SpoilerData.NetIDs.Contains(entry.Item));
                    Result.LocationAssignment.Add(Location.Key, Item.Key);
                }
            }
            foreach(var i in Instance.LocationPool)
            {
                if (!Result.ItemHasSpoilerData(i.Key))
                {
                    Result.UnrandomizedLocations.Add(i.Key);
                }
            }
            return Result;
        }
    }
}
