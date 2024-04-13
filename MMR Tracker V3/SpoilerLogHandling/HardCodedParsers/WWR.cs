using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.SpoilerLogHandling.HardCodedParsers
{
    internal class WWR
    {
        public static GenericSpoilerLog ParseSpoiler(InstanceData.TrackerInstance Instance, Dictionary<string, object> DataStore)
        {
            if (DataStore.ContainsKey("Archipelago")) { return ParseAPSpoiler(Instance, DataStore); }
            else { return ParseStandardSpoiler(Instance, DataStore); }
        }

        private static GenericSpoilerLog ParseStandardSpoiler(InstanceData.TrackerInstance instance, Dictionary<string, object> dataStore)
        {
            throw new NotImplementedException();
        }

        static Dictionary<string, string[]> ProgressionSettings = new()
        {
            { "progression_dungeons", ["Dungeon"] },
            { "progression_tingle_chests", ["Tingle Chest"] },
            { "progression_dungeon_secrets", ["Dungeon Secret"] },
            { "progression_puzzle_secret_caves", ["Puzzle Secret Cave"] },
            { "progression_combat_secret_caves", ["Combat Secret Cave"] },
            { "progression_savage_labyrinth", ["Savage Labyrinth"] },
            { "progression_great_fairies", ["Great Fairy"] },
            { "progression_short_sidequests", ["Short Sidequest"] },
            { "progression_long_sidequests", ["Long Sidequest"] },
            { "progression_spoils_trading", ["Spoils Trading"] },
            { "progression_minigames", ["Minigame"] },
            { "progression_battlesquid", ["Battlesquid"] },
            { "progression_free_gifts", ["Free Gift"] },
            { "progression_mail", ["Mail"] },
            { "progression_platforms_rafts", ["Platform", "Raft"] },
            { "progression_submarines", ["Submarine"] },
            { "progression_eye_reef_chests", ["Eye Reef Chest"] },
            { "progression_big_octos_gunboats", ["Big Octo", "Gunboat"] },
            { "progression_expensive_purchases", ["Expensive Purchase"] },
            { "progression_island_puzzles", ["Island Puzzle"] },
            { "progression_misc", ["Other Chest", "Misc"] },
        };
        static Dictionary<string, string> EntranceRandoSetting = new()
        {

            { "randomize_dungeon_entrances", "Dungeon Entrance" },
            { "randomize_secret_cave_entrances", "Secret Cave Entrance" },
            { "randomize_miniboss_entrances", "MiniBoss Entrance" },
            { "randomize_boss_entrances", "Boss Entrance" },
            { "randomize_secret_cave_inner_entrances", "Inner Cave Entrance" },
            { "randomize_fairy_fountain_entrances", "Fairy Fountain Entrance" },
        };

        private static GenericSpoilerLog ParseAPSpoiler(InstanceData.TrackerInstance Instance, Dictionary<string, object> dataStore)
        {
            GenericSpoilerLog Result = new GenericSpoilerLog();
            Archipelago.GenericAPSpoiler SpoilerLog = dataStore["Archipelago"] as Archipelago.GenericAPSpoiler;
            foreach (var i in SpoilerLog.Locations)
            {
                var Location = Instance.GetLocationByID(i.Location);

                if (Location is not null)
                {
                    Result.LocationAssignment.Add(Location.ID, i.Item);
                    Result.LocationPlayerAssignment.Add(Location.ID, i.Player);
                }
            }
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
                else if (ProgressionSettings.TryGetValue(i.Key, out string[] JunkType) && !i.Value.IsTruthy()) { JunkChecks(JunkType, Instance, Result); }
                else if (EntranceRandoSetting.TryGetValue(i.Key, out string EntranceType) && !i.Value.IsTruthy()) { UnrandoEntrances(EntranceType, Instance, Result); }
                else if (i.Key.In("progression_triforce_charts")) { JunkSunkenTreasureChecks(true, Instance, Result); }
                else if (i.Key.In("progression_treasure_charts")) { JunkSunkenTreasureChecks(false, Instance, Result); }
            }
            foreach (var i in Instance.LocationPool.Values.Where(x => 
                !Result.LocationAssignment.ContainsKey(x.ID) && 
                !Result.JunkLocations.Contains(x.ID) && 
                !Result.UnrandomizedLocations.Contains(x.ID) &&
                !Result.ManualLocations.Contains(x.ID) &&
                string.IsNullOrWhiteSpace(x.SingleValidItem)))
            {
                if (i.GetDictEntry().SpoilerData.Tags.Contains("Consumables only")) { Result.JunkLocations.Add(i.ID); }
                else if (i.GetDictEntry().SpoilerData.Tags.Contains("No progression")) { Result.JunkLocations.Add(i.ID); }
                else if (i.GetDictEntry().ValidItemTypes.Contains("Entrances")) { continue; }
                else
                {
                    Debug.WriteLine($"{i.GetName()} Had no item data");
                }
            }
            return Result;
        }
        private static void JunkSunkenTreasureChecks(bool Triforce, InstanceData.TrackerInstance Instance, GenericSpoilerLog Result)
        {
            foreach (var i in Instance.LocationPool.Values)
            {
                if ("Sunken Treasure".In(i.GetDictEntry().SpoilerData.Tags) && i.GetDictEntry().OriginalItem.Contains("Triforce Shard") == Triforce)
                {
                    Result.JunkLocations.Add(i.ID);
                }
            }
        }

        private static void JunkChecks(string[] Tags, InstanceData.TrackerInstance Instance, GenericSpoilerLog Result)
        {
            foreach (var i in Instance.LocationPool.Values)
            {
                if (Tags.Intersect(i.GetDictEntry().SpoilerData.Tags).Any()) { Result.JunkLocations.Add(i.ID); }
            }
        }

        private static void UnrandoEntrances(string Tag, InstanceData.TrackerInstance Instance, GenericSpoilerLog Result)
        {
            foreach (var i in Instance.LocationPool.Values)
            {
                if (Tag.In(i.GetDictEntry().SpoilerData.Tags)) { Result.UnrandomizedLocations.Add(i.ID); }
            }
        }
    }
}
