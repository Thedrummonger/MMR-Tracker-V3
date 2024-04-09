using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.OptionData;

namespace MMR_Tracker_V3.SpoilerLogImporter
{
    internal class WWRSpoilerLogTools
    {
        public static void ParseSpoiler(InstanceData.TrackerInstance Instance)
        {
            if (SpoilerLogTools.IsGenericAPSpoiler(Instance)) { ParseAPSpoiler(Instance); }
            else { ParseStandardSpoiler(Instance); }
        }

        private static void ParseStandardSpoiler(InstanceData.TrackerInstance instance)
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

        private static void ParseAPSpoiler(InstanceData.TrackerInstance Instance)
        {
            Archipelago.GenericAPSpoiler SpoilerLog = JsonConvert.DeserializeObject<Archipelago.GenericAPSpoiler>(string.Join(" ", Instance.SpoilerLog.Log));
            foreach (var i in SpoilerLog.Locations)
            {
                var Location = Instance.GetLocationByID(i.Location);

                if (Location is not null)
                {
                    Location.Randomizeditem.SpoilerLogGivenItem = i.Item;
                    Location.Randomizeditem.OwningPlayer = i.Player;
                }
            }
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
                else if (ProgressionSettings.TryGetValue(i.Key, out string[] JunkType) && !i.Value.IsTruthy()) { JunkChecks(JunkType, Instance); }
                else if (EntranceRandoSetting.TryGetValue(i.Key, out string EntranceType) && !i.Value.IsTruthy()) { UnrandoEntrances(EntranceType, Instance); }
                else if (i.Key.In("progression_triforce_charts")) { JunkSunkenTreasureChecks(true, Instance); }
                else if (i.Key.In("progression_treasure_charts")) { JunkSunkenTreasureChecks(false, Instance); }
            }
            foreach (var i in Instance.LocationPool.Values.Where(x => string.IsNullOrWhiteSpace(x.Randomizeditem.SpoilerLogGivenItem)))
            {
                if (i.GetDictEntry().SpoilerData.Tags.Contains("Consumables only")) { i.SetRandomizedState(MiscData.RandomizedState.ForcedJunk); }
                else if (i.GetDictEntry().SpoilerData.Tags.Contains("No progression")) { i.SetRandomizedState(MiscData.RandomizedState.ForcedJunk); }
                else if (i.GetDictEntry().ValidItemTypes.Contains("Entrances")) { continue; }
                else
                {
                    Debug.WriteLine($"{i.GetName()} Had no item data");
                }
            }
        }
        private static void JunkSunkenTreasureChecks(bool Triforce, InstanceData.TrackerInstance Instance)
        {
            foreach (var i in Instance.LocationPool.Values)
            {
                if ("Sunken Treasure".In(i.GetDictEntry().SpoilerData.Tags) && i.GetDictEntry().OriginalItem.Contains("Triforce Shard") == Triforce) 
                { 
                    i.SetRandomizedState(MiscData.RandomizedState.ForcedJunk); 
                }
            }
        }

        private static void JunkChecks(string[] Tags, InstanceData.TrackerInstance Instance)
        {
            foreach(var i in Instance.LocationPool.Values)
            {
                if (Tags.Intersect(i.GetDictEntry().SpoilerData.Tags).Any()) { i.SetRandomizedState(MiscData.RandomizedState.ForcedJunk); }
            }
        }

        private static void UnrandoEntrances(string Tag, InstanceData.TrackerInstance Instance)
        {
            foreach (var i in Instance.LocationPool.Values)
            {
                if (Tag.In(i.GetDictEntry().SpoilerData.Tags)) { i.SetRandomizedState(MiscData.RandomizedState.Unrandomized); }
            }
        }
    }
}
