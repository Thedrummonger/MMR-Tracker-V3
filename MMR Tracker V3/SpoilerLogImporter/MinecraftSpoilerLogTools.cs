using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.SpoilerLogImporter
{
    internal class MinecraftSpoilerLogTools
    {
        public static void ParseSpoiler(InstanceData.TrackerInstance Instance)
        {
            Archipelago.GenericAPSpoiler SpoilerLog = JsonConvert.DeserializeObject<Archipelago.GenericAPSpoiler>(string.Join(" ", Instance.SpoilerLog.Log));
            //Apply Location Data
            foreach(var i in SpoilerLog.Locations)
            {
                var Location = Instance.GetLocationByID(i.Location);
                if (Location is not null) { 
                    Location.Randomizeditem.SpoilerLogGivenItem = i.Item;
                    Location.Randomizeditem.OwningPlayer = i.Player;
                }
            }
            //Apply Entrance Data
            if (SpoilerLog.SlotData.TryGetValue("structures", out object EntMapObj))
            {
                var EntranceMap = Utility.SerializeConvert<Dictionary<string, string>>(EntMapObj);
                foreach (var Entry in EntranceMap)
                {
                    var exit = Instance.ExitPool.Values.First(x => x.GetDictEntry().Area == Entry.Key);
                    var Dest = Instance.ExitPool.Values.First(x => x.GetDictEntry().Exit == Entry.Value).AsDestination();
                    exit.SpoilerDefinedDestinationExit = Dest;
                }
            }
            //Apply Setting Data
            foreach (var i in SpoilerLog.SlotData)
            {
                if (Instance.IntOptions.TryGetValue(i.Key, out OptionData.IntOption intOption))
                {
                    intOption.SetDynValue(i.Value);
                }
                else if (Instance.ToggleOptions.TryGetValue(i.Key, out OptionData.ToggleOption toggleOption))
                {
                    toggleOption.SetDynValue(i.Value);
                }
                else if (Instance.ChoiceOptions.TryGetValue(i.Key, out OptionData.ChoiceOption choiceOption))
                {
                    choiceOption.SetDynValue(i.Value);
                }
            }
            //Apply Junk Locations
            List<string> JunkLocationTypes = [];
            if (SpoilerLog.SlotData.TryGetValue("include_postgame_advancements", out object tv1) && !tv1.IsTruthy())
            {
                if (Instance.ChoiceOptions["required_bosses"].Value.In("ender_dragon", "both"))
                {
                    JunkLocationTypes.Add("ender_dragon");
                }
                if (Instance.ChoiceOptions["required_bosses"].Value.In("wither", "both"))
                {
                    JunkLocationTypes.Add("wither");
                }
            }
            if (SpoilerLog.SlotData.TryGetValue("include_hard_advancements", out object tv2) && !tv2.IsTruthy())
            {
                JunkLocationTypes.Add("hard");
            }
            if (SpoilerLog.SlotData.TryGetValue("include_unreasonable_advancements", out object tv3) && !tv3.IsTruthy())
            {
                JunkLocationTypes.Add("unreasonable");
            }
            foreach(var loc in Instance.LocationPool.Values)
            {
                if (loc.GetDictEntry().SpoilerData.Tags.Intersect(JunkLocationTypes).Any())
                {
                    loc.SetRandomizedState(MiscData.RandomizedState.ForcedJunk);
                }
            }
        }
    }
}
