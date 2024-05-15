using MathNet.Numerics;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMUtils;
using static MMR_Tracker_V3.SpoilerLogHandling.Archipelago;

namespace MMR_Tracker_V3.SpoilerLogHandling.HardCodedParsers
{
    internal class Minecraft
    {
        public static GenericSpoilerLog ReadSpoiler(InstanceData.TrackerInstance Instance, Dictionary<string, object> DataStore)
        {
            GenericSpoilerLog Result = new GenericSpoilerLog();
            if (!DataStore.ContainsKey("Archipelago") || DataStore["Archipelago"] is not GenericAPSpoiler SpoilerLog) { return null; }

            //Apply Location Data
            foreach (var i in SpoilerLog.Locations)
            {
                var Location = Instance.GetLocationByID(i.Location);
                if (Location is not null)
                {
                    Result.LocationAssignment.Add(Location.ID, i.Item);
                    Result.LocationPlayerAssignment.Add(Location.ID, i.Player);
                }
            }
            //Apply Entrance Data
            if (SpoilerLog.SlotData.TryGetValue("shuffle_structures", out object st) && !st.IsTruthy())
            {
                foreach (var ent in Instance.ExitPool.Values) 
                {
                    Result.UnrandomizedEntrances.Add(ent.ID);
                }

            }
            else if (SpoilerLog.SlotData.TryGetValue("structures", out object EntMapObj))
            {
                var EntranceMap = Utility.SerializeConvert<Dictionary<string, string>>(EntMapObj);
                foreach (var Entry in EntranceMap)
                {
                    var exit = Instance.ExitPool.Values.First(x => x.GetDictEntry().Area == Entry.Key);
                    var Dest = Instance.ExitPool.Values.First(x => x.GetDictEntry().Exit == Entry.Value).AsDestination();
                    Result.EntranceAssignments.Add(exit.ID, (Dest.from, Dest.region));
                }
            }
            //Apply Setting Data
            foreach (var i in SpoilerLog.SlotData)
            {
                if (Instance.IntOptions.TryGetValue(i.Key, out OptionData.IntOption intOption))
                {
                    Result.IntOptionAssignment.Add(intOption.ID, i.Value.AsIntValue());
                }
                else if (Instance.ToggleOptions.TryGetValue(i.Key, out OptionData.ToggleOption toggleOption))
                {
                    Result.ToggleOptionAssignment.Add(toggleOption.ID, i.Value.IsTruthy());
                }
                else if (Instance.ChoiceOptions.TryGetValue(i.Key, out OptionData.ChoiceOption choiceOption))
                {
                    Result.ChoiceOptionAssignment.Add(choiceOption.ID, choiceOption.GetDynValue(i.Value));
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
            foreach (var loc in Instance.LocationPool.Values)
            {
                if (loc.GetDictEntry().SpoilerData.Tags.Intersect(JunkLocationTypes).Any())
                {
                    Result.JunkLocations.Add(loc.ID);
                }
            }
            return Result;
        }
    }
}
