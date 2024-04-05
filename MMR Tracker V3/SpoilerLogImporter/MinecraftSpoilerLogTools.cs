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
        public static void ParseSpoiler(TrackerObjects.InstanceData.TrackerInstance Instance)
        {
            Archipelago.GenericAPSpoiler SpoilerLog = JsonConvert.DeserializeObject<Archipelago.GenericAPSpoiler>(string.Join(" ", Instance.SpoilerLog.Log));
            foreach(var i in SpoilerLog.Locations)
            {
                var Location = Instance.GetLocationByID(i.Location);

                if (Location is not null) { 
                    Location.Randomizeditem.SpoilerLogGivenItem = i.Item;
                    Location.Randomizeditem.OwningPlayer = i.Player;
                }
            }
            foreach(var i in SpoilerLog.SlotData)
            {
                if (i.Key == "structures")
                {
                    var EntranceMap = Utility.SerializeConvert<Dictionary<string, string>>(i.Value);
                    foreach(var Entry in EntranceMap)
                    {
                        var exit = Instance.ExitPool.Values.First(x => x.GetDictEntry().Area == Entry.Key);
                        var Dest = Instance.ExitPool.Values.First(x => x.GetDictEntry().Exit == Entry.Value).AsDestination();
                        exit.SpoilerDefinedDestinationExit = Dest;
                    }
                }
                else if (Instance.IntOptions.TryGetValue(i.Key, out OptionData.IntOption intOption))
                {
                    if (i.Value is int i1) { intOption.SetValue(i1); }
                    else if (i.Value is Int64 i64) { intOption.SetValue(Convert.ToInt32(i64)); }
                }
                else if (Instance.ToggleOptions.TryGetValue(i.Key, out OptionData.ToggleOption toggleOption))
                {
                    bool Value;
                    if (i.Value is bool boolVal) { Value = boolVal; }
                    else if (i.Value is int IntBoolVal) { Value = IntBoolVal > 0; }
                    else if (i.Value is string StringBoolVal) { Value = bool.Parse(StringBoolVal); }
                    else { Value = false; }
                    toggleOption.SetValue(Value);
                }
                else if (Instance.ChoiceOptions.TryGetValue(i.Key, out OptionData.ChoiceOption choiceOption))
                {
                    string Value;
                    if (i.Value is Int64 IntBoolVal) { Value = choiceOption.ValueList.Keys.ToArray()[IntBoolVal]; }
                    else if (i.Value is string StringBoolVal) { Value = StringBoolVal; }
                    else { Value = ""; }
                    choiceOption.SetValue(Value);
                }
            }
        }
    }
}
