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
using YamlDotNet.Core.Tokens;
using static MMR_Tracker_V3.SpoilerLogHandling.Archipelago;

namespace MMR_Tracker_V3.SpoilerLogHandling.HardCodedParsers
{
    internal class BanjoTooie
    {
        public static GenericSpoilerLog ReadSpoiler(InstanceData.TrackerInstance Instance, Dictionary<string, object> DataStore)
        {
            GenericSpoilerLog Result = new GenericSpoilerLog();
            if (!DataStore.ContainsKey("Archipelago") || DataStore["Archipelago"] is not GenericAPSpoiler SpoilerLog) { return null; }
            if (DataStore.TryGetValue("APPlayerFile", out object PF) && PF is string[] APPlayerFile && APPlayerFile.Any())
            {
                var DataFileJsonString = Utility.ConvertYamlStringToJsonString(string.Join(Environment.NewLine, APPlayerFile));
                var DataFile = JsonConvert.DeserializeObject<Dictionary<string, object>>(DataFileJsonString);
                var PMRData = DataFile["Banjo-Tooie"].SerializeConvert<Dictionary<string, object>>();
                foreach (var entry in PMRData)
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
                        object finalval = int.TryParse(entry.Value.ToString(), out int Intval) ? Intval : entry.Value.ToString();
                        Result.ChoiceOptionAssignment.Add(choiceOption.ID, choiceOption.GetDynValue(finalval));
                    }
                }
            }
            //Apply Location Data
            foreach (var i in SpoilerLog.Locations)
            {
                var Location = Instance.LocationPool.Values.First(x => x.GetDictEntry().SpoilerData.NetIDs.Contains(i.Location));
                if (Location is not null)
                {
                    Result.LocationPlayerAssignment.Add(Location.ID, i.Player);
                    if (Instance.GetParentContainer().netConnection.PlayerID == i.Player)
                    {
                        var item = Instance.ItemPool.Values.First(x => x.GetDictEntry().SpoilerData.NetIDs.Contains(i.Item));
                        Result.LocationAssignment.Add(Location.ID, item.ID);
                    }
                    else
                    {
                        Result.LocationAssignment.Add(Location.ID, i.Item);
                    }
                }
            }
            return Result;
        }
    }
}
