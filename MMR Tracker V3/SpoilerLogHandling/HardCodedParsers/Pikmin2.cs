using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TDMUtils;
using static MMR_Tracker_V3.SpoilerLogHandling.Archipelago;

namespace MMR_Tracker_V3.SpoilerLogHandling.HardCodedParsers
{
    internal class Pikmin2
    {
        public static GenericSpoilerLog ReadSpoiler(InstanceData.TrackerInstance Instance, Dictionary<string, object> DataStore)
        {
            GenericSpoilerLog Result = new GenericSpoilerLog();
            if (!DataStore.ContainsKey("Archipelago") || DataStore["Archipelago"] is not GenericAPSpoiler SpoilerLog) { return null; }

            string[] entry = DataStore["APPatchFile"] as string[];
            var PatchFile = JsonConvert.DeserializeObject<PatchFile>(string.Join(Environment.NewLine, entry));

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

            foreach(var i in PatchFile.Caves)
            {
                var exit = Instance.ExitPool.Values.First(x => x.ExitID == i.Key);
                var Dest = Instance.ExitPool.Values.First(x => x.ExitID == i.Value).AsDestination();
                Result.EntranceAssignments.Add(exit.ID, (Dest.from, Dest.region));
            }

            if (Instance.ChoiceOptions["onion_shuffle"].GetDynValue(PatchFile.OnionShuffle) != "vanilla")
                Result.LocationAssignment.Add("VoR Onion", ToOnion(PatchFile.OnionLocations["VoR"]));

            if (Instance.ChoiceOptions["onion_shuffle"].GetDynValue(PatchFile.OnionShuffle) != "vanilla" && Instance.ChoiceOptions["onion_shuffle"].GetDynValue(PatchFile.OnionShuffle) != "in_pool")
            {
                Result.LocationAssignment.Add("AW Onion", ToOnion(PatchFile.OnionLocations["AW"]));
                Result.LocationAssignment.Add("PP Onion", ToOnion(PatchFile.OnionLocations["PP"]));
            }

            Debug.WriteLine($"Patch\n{entry.ToFormattedJson()}");

            Result.ChoiceOptionAssignment.Add("win_condition", Instance.ChoiceOptions["win_condition"].GetDynValue(PatchFile.WinCondition));
            Result.ChoiceOptionAssignment.Add("shuffle_caves", Instance.ChoiceOptions["shuffle_caves"].GetDynValue(PatchFile.CaveRando));
            Result.ChoiceOptionAssignment.Add("cave_keys", Instance.ChoiceOptions["cave_keys"].GetDynValue(PatchFile.CaveKeys));
            Result.ChoiceOptionAssignment.Add("boss_rando", Instance.ChoiceOptions["boss_rando"].GetDynValue(PatchFile.BossRando));
            Result.ChoiceOptionAssignment.Add("onion_shuffle", Instance.ChoiceOptions["onion_shuffle"].GetDynValue(PatchFile.OnionShuffle));
            Result.ChoiceOptionAssignment.Add("progressive_globes", Instance.ChoiceOptions["progressive_globes"].GetDynValue(PatchFile.ProgressiveGlobes));
            //Result.ChoiceOptionAssignment.Add("weapons_in_pool", Instance.ChoiceOptions["weapons_in_pool"].GetDynValue(PatchFile.)); No in the patch file, TODO later
            Result.IntOptionAssignment.Add("poko_amount", PatchFile.PokoAmount);
            Result.IntOptionAssignment.Add("treasure_amount", PatchFile.TreasureAmount);
            Result.IntOptionAssignment.Add("debt", PatchFile.Debt);

            return Result;
        }
        static string ToOnion(string word) => char.ToUpper(word[0]) + word[1..] + " Onion";


        private class PatchFile
        {
            [JsonProperty("seed")] public string Seed { get; set; }
            [JsonProperty("slot")] public int Slot { get; set; }
            [JsonProperty("slot_name")] public string SlotName { get; set; }
            [JsonProperty("items")] public Dictionary<string, string> Items { get; set; }
            [JsonProperty("starter_items")] public List<string> StarterItems { get; set; }
            [JsonProperty("win_condition")] public int WinCondition { get; set; }
            [JsonProperty("poko_amount")] public int PokoAmount { get; set; }
            [JsonProperty("treasure_amount")] public int TreasureAmount { get; set; }
            [JsonProperty("caves")] public Dictionary<string, string> Caves { get; set; }
            [JsonProperty("cave_rando")] public int CaveRando { get; set; }
            [JsonProperty("death_link")] public int DeathLink { get; set; }
            [JsonProperty("boss_rando")] public int BossRando { get; set; }
            [JsonProperty("onion_shuffle")] public int OnionShuffle { get; set; }
            [JsonProperty("onion_locations")] public Dictionary<string, string> OnionLocations { get; set; }
            [JsonProperty("debt")] public int Debt { get; set; }
            [JsonProperty("progressive_globes")] public int ProgressiveGlobes { get; set; }
            [JsonProperty("cave_keys")] public int CaveKeys { get; set; }
        }
    }
}
