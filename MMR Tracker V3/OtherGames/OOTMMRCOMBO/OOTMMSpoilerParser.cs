using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using MathNet.Numerics;

namespace MMR_Tracker_V3.OtherGames.OOTMMRCOMBO
{
	internal class OOTMMSpoilerParser
	{
        public static Dictionary<string, dynamic> HintNames = new Dictionary<string, dynamic>() {
            { "OOT_FROGS_FINAL", "OOT Zora River Frogs Game"},
            { "OOT_FISHING", new string[] { "OOT Fishing Pond Child", "OOT Fishing Pond Adult" }},
            { "OOT_RAVAGED_VILLAGE", "OOT Kakariko Song Shadow"},
            { "OOT_ZORA_KING", new string[]{ "OOT Zora Domain Tunic", "OOT Zora Domain Eyeball Frog"}},
            { "OOT_GANON_FAIRY", "OOT Great Fairy Defense Upgrade"},
            { "OOT_TEMPLE_FIRE_HAMMER", "OOT Fire Temple Hammer"},
            { "OOT_TEMPLE_FIRE_SCARECROW", "OOT Fire Temple Scarecrow Chest"},
            { "OOT_GTG_WATER", "OOT Gerudo Training Water"},
            { "OOT_HAUNTED_WASTELAND", "OOT Haunted Wasteland Chest"},
            { "OOT_GERUDO_ARCHERY", new string[] { "OOT Gerudo Fortress Archery Reward 1", "OOT Gerudo Fortress Archery Reward 2" }},
            { "MM_RANCH_DEFENSE", new string[]{ "MM Romani Ranch Aliens", "MM Romani Ranch Cremia Escort" }},
            { "MM_BUTLER_RACE", "MM Deku Shrine Mask of Scents"},
            { "MM_COUPLE_MASK", "MM Stock Pot Inn Couple's Mask"},
            { "MM_DON_GERO_CHOIR", "MM Mountain Village Frog Choir HP"},
            { "MM_GORON_RACE", "MM Goron Race Reward"},
            { "MM_GRAVEYARD_NIGHT3", "MM Beneath The Graveyard Dampe Chest"},
            { "MM_BANK_3", "MM Clock Town Bank Reward 3"},
            { "MM_SOUND_CHECK", "MM Milk Bar Troupe Leader Mask"},
            { "MM_BOAT_ARCHERY", "MM Tourist Information Boat Cruise"},
            { "MM_OSH_CHEST", "MM Ocean Spider House Chest HP"},
            { "MM_PINNACLE_ROCK_HP", "MM Pinnacle Rock HP"},
            { "MM_FISHERMAN_GAME", "MM Great Bay Coast Fisherman HP"},
            { "MM_SONG_ELEGY", "MM Ancient Castle of Ikana Song Emptiness"},
            { "MM_SECRET_SHRINE_HP", "MM Secret Shrine HP Chest"}
        };


        public static Dictionary<string, Dictionary<string, string>> SpolierNameChanges = new Dictionary<string, Dictionary<string, string>>
        {
            { "Exits", new Dictionary<string, string> {
                    { "OOT Dodongo Cavern Pre Boss", "OOT Dodongo Cavern Skull" }
            }}
        };
        public static void readAndApplySpoilerLog(LogicObjects.TrackerInstance Instance)
        {
            Instance.ToggleAllTricks(false);
            foreach (var i in Instance.EntrancePool.AreaList.SelectMany(x => x.Value.LoadingZoneExits))
            {
                i.Value.RandomizedState = MiscData.RandomizedState.Unrandomized;
            }
            foreach (var i in Instance.LocationPool.Values)
            {
                i.RandomizedState = MiscData.RandomizedState.Randomized;
            }
            foreach (var i in Instance.ItemPool.Values)
            {
                i.AmountInStartingpool = 0;
            }
            Dictionary<string, bool> spoilerFileLocation =
                new Dictionary<string, bool> { { "Settings", false }, { "Tricks", false }, { "Starting Items", false }, { "Junk Locations", false }, { "Entrances", false }, { "Hints", false } };
            foreach (var l in Instance.SpoilerLog.Log)
            {
                if (string.IsNullOrWhiteSpace(l)) { continue; }
                if (GetCurrentSpoilerLogline(l, spoilerFileLocation, out bool EOO)) 
                {
                    if (EOO) { break; }
                    continue;
                }
                if (!ApplySettings(l, spoilerFileLocation, Instance)) { continue; }
            }

            var ReleventData = GetReleventSpoilerLines(Instance.SpoilerLog.Log);
            foreach (var i in ReleventData)
            {
                var entryData = i.Split(':').Select(x => x.Trim()).ToArray();

                TrackerObjects.LocationData.LocationObject location = null;
                TrackerObjects.ItemData.ItemObject item = Instance.GetItemToPlace(entryData[1]);

                if (Instance.LocationPool.ContainsKey(entryData[0])) { location = Instance.LocationPool[entryData[0]]; }
                else { Debug.WriteLine($"{entryData[0]} was not a valid location!"); }
                if (item is null) { Debug.WriteLine($"{entryData[1]} was not a valid Item or no more of this could be placed!"); }
                if (location is not null) { location.Randomizeditem.SpoilerLogGivenItem = item?.Id ?? entryData[1]; }
            }
        }

        public static bool ApplySettings(string l, Dictionary<string, bool> spoilerFileLocation, LogicObjects.TrackerInstance Instance)
        {
            string Line = l.Trim();
            if (spoilerFileLocation["Settings"])
            {
                var SettingLineData = Line.Split(":").Select(x => x.Trim()).ToArray();
                if (SettingLineData.Length < 2) { return false; }
                if (Instance.UserOptions.ContainsKey(SettingLineData[0]))
                {
                    Instance.UserOptions[SettingLineData[0]].CurrentValue = SettingLineData[1];
                }
                else if (SettingLineData[0] == "erBoss" && SettingLineData[1] != "none")
                {
                    foreach (var i in Instance.EntrancePool.AreaList.SelectMany(x => x.Value.LoadingZoneExits))
                    {
                        i.Value.RandomizedState = MiscData.RandomizedState.Randomized;
                    }
                }
                else if(SettingLineData[0] == "mapCompassShuffle" && SettingLineData[1] == "starting")
                {
                    var MapsCompasses = Instance.ItemPool.Values.Where(x => 
                    x.Id.StartsWith("MM_MAP_") || 
                    x.Id.StartsWith("OOT_MAP_") || 
                    x.Id.StartsWith("MM_COMPASS_") || 
                    x.Id.StartsWith("OOT_COMPASS_"));

                    foreach(var i in MapsCompasses) { i.AmountInStartingpool= 1; }
                }
            }
            if (spoilerFileLocation["Tricks"])
            {
                var TrickName = "TRICK_" + Line;
                var trick = Instance.MacroPool.Values.FirstOrDefault(x => x.isTrick(Instance) && x.ID == TrickName);
                if (trick == null) { }
                if (trick is not null) { trick.TrickEnabled = true; }
                else { Debug.WriteLine($"{TrickName} Could notbe found in the trick list!"); }
            }
            if (spoilerFileLocation["Starting Items"])
            {
                var startingItemData = Line.Split(":").Select(x => x.Trim()).ToArray();
                if (startingItemData.Length < 2 || !int.TryParse(startingItemData[1], out int tr)) { return false; }
                if (tr < 1) { return false; }
                for (var i = 0; i < tr; i++)
                {
                    var ValidItem = Instance.GetItemToPlace(startingItemData[0], true, true);
                    if (ValidItem is not null) { ValidItem.AmountInStartingpool++; }
                    else { Debug.WriteLine($"{startingItemData[1]} Could not be made a starting item!"); }
                }
            }
            if (spoilerFileLocation["Junk Locations"])
            {
                var junkLOC = Instance.LocationPool.Values.FirstOrDefault(x => x.ID == Line);
                if (junkLOC is not null) { junkLOC.SetRandomizedState(MiscData.RandomizedState.ForcedJunk, Instance); }
                else { Debug.WriteLine($"{Line} Was not a valid Location!"); }
            }
            if (spoilerFileLocation["Hints"])
            {
                var pieces = Line.Split(new[] { ':' }, 2).Select(x => x.Trim()).ToArray();
                var GossipStone = pieces[0];
                if (!Instance.HintPool.ContainsKey(GossipStone)) { return false; }
                var GossipStoneEntry = Instance.HintPool[GossipStone];
                var HintText = pieces[1].Split(new[] { ',' }, 2).Select(x => x.Trim()).ToArray();
                var HintType = HintText[0];
                var HintData = HintText[1].Replace(")", "").Split("(").Select(x => x.Trim()).ToArray();
                var HintLocation = HintData[0];
                var HintItem = HintData.Count() > 1 ? HintData[1] : string.Empty;
                string PrettyLocationName = HintData[0].ToLower().Replace("_", " ");
                PrettyLocationName = Regex.Replace(PrettyLocationName, @"(^\w)|(\s\w)", m => m.Value.ToUpper()).Replace("Mm", "MM").Replace("Oot", "OOT");
                switch (HintType)
                {
                    case "Item-Exact":
                        var hintLocationsDYN = !HintNames.ContainsKey(HintLocation) ? null : HintNames[HintLocation];

                        List<string> HintedLocations = new();
                        List<string> HintedItems = HintItem.Split(",").Select(x => x.Trim()).ToList();

                        if (hintLocationsDYN is not null)
                        {
                            if (hintLocationsDYN is string) { HintedLocations.Add(hintLocationsDYN); }
                            else { HintedLocations.AddRange(hintLocationsDYN); }
                        }
                        GossipStoneEntry.SpoilerHintText = $"{PrettyLocationName} contains {string.Join(" and ", HintedItems.Select(x => GetItemName(x, Instance)))}";

                        if (HintedLocations.Any() && HintedLocations.Count == HintedItems.Count)
                        {
                            for (var c = 0; c < HintedItems.Count; c++)
                            {
                                if (Instance.GetLocationByID(HintedLocations[c]) != null)
                                {
                                    GossipStoneEntry.ParsedHintData[HintedLocations[c]] = HintedItems[c];
                                }
                            }
                        }
                        break;
                    case "Item-Region":
                        GossipStoneEntry.SpoilerHintText = $"{PrettyLocationName} contains {GetItemName(HintItem, Instance)}";
                        break;
                    case "Hero":
                        GossipStoneEntry.SpoilerHintText = $"{PrettyLocationName} is Way of the Hero";
                        break;
                    case "Foolish":
                        GossipStoneEntry.SpoilerHintText = $"{PrettyLocationName} is Foolish";
                        break;
                }
            }
            if (spoilerFileLocation["Entrances"])
            {
                string NewLine = Line;

                foreach (var i in SpolierNameChanges["Exits"])
                {
                    NewLine = NewLine.Replace(i.Key, i.Value);
                }

                var EntranceData = NewLine.Split(new string[] { "->" }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
                var Source = EntranceData[0].Split('/');
                var Destination = EntranceData[1].Split('/');
                Debug.WriteLine($"[{Source[0]}] [{Source[1]}]");
                if (Instance.EntrancePool.AreaList.ContainsKey(Source[0]) &&
                    Instance.EntrancePool.AreaList[Source[0]].LoadingZoneExits.ContainsKey(Source[1]) &&
                    Instance.EntrancePool.AreaList.ContainsKey(Destination[0]) &&
                    Instance.EntrancePool.AreaList[Destination[0]].LoadingZoneExits.ContainsKey(Destination[1]))
                {
                    Instance.EntrancePool.AreaList[Source[0]].LoadingZoneExits[Source[1]].SpoilerDefinedDestinationExit = new EntranceData.EntranceRandoDestination
                    {
                        region = Destination[1],
                        from = Destination[0]
                    };
                }
                else { Debug.WriteLine($"{NewLine} Could not be mapped to an entrance!"); }
            }
            return true;
        }

        public static string GetItemName(string Item, LogicObjects.TrackerInstance Instance)
        {
            var LogicItem = Instance.GetItemByID(Item);
            if (LogicItem is null) { return Item; }
            var dictEntry = LogicItem.GetDictEntry(Instance);
            if (dictEntry is null) { return Item; }
            return dictEntry.GetName(Instance);
        }

        public static bool GetCurrentSpoilerLogline(string l, Dictionary<string, bool> spoilerFileLocation, out bool EndOfOptions)
        {
            EndOfOptions = false;
            if (l == "Settings")
            {
                foreach (var k in spoilerFileLocation.Keys) { spoilerFileLocation[k] = false; }
                spoilerFileLocation["Settings"] = true;
                return true;
            }
            if (l == "Tricks")
            {
                foreach (var k in spoilerFileLocation.Keys) { spoilerFileLocation[k] = false; }
                spoilerFileLocation["Tricks"] = true;
                return true;
            }
            if (l == "Starting Items")
            {
                foreach (var k in spoilerFileLocation.Keys) { spoilerFileLocation[k] = false; }
                spoilerFileLocation["Starting Items"] = true;
                return true;
            }
            if (l == "Junk Locations")
            {
                foreach (var k in spoilerFileLocation.Keys) { spoilerFileLocation[k] = false; }
                spoilerFileLocation["Junk Locations"] = true;
                return true;
            }
            if (l == "Entrances")
            {
                foreach (var k in spoilerFileLocation.Keys) { spoilerFileLocation[k] = false; }
                spoilerFileLocation["Entrances"] = true;
                return true;
            }
            if (l == "Foolish Regions")
            {
                foreach (var k in spoilerFileLocation.Keys) { spoilerFileLocation[k] = false; }
                return true;
            }
            if (l == "Hints")
            {
                foreach (var k in spoilerFileLocation.Keys) { spoilerFileLocation[k] = false; }
                spoilerFileLocation["Hints"] = true;
                return true;
            }
            if (l == "Sphere 0") { EndOfOptions = true; return true; }
            return false;
        }

        public static List<string> GetReleventSpoilerLines(string[] Data)
        {
            var ReleventSpoilerlines = new List<string>();
            bool AtSphereData = false;
            bool CurrentLineEmpty = false;
            bool AtLocationData = false;
            foreach (var x in Data)
            {
                bool SphereLine;
                if (x.StartsWith("Sphere ")) { AtSphereData = true; SphereLine = true; }
                else { SphereLine = false; }
                bool PreviousLineEmpty = CurrentLineEmpty;
                if (string.IsNullOrWhiteSpace(x)) { CurrentLineEmpty = true; }
                else { CurrentLineEmpty = false; }
                if (!CurrentLineEmpty && !SphereLine && PreviousLineEmpty && AtSphereData) { AtLocationData = true; }
                if (AtLocationData) { ReleventSpoilerlines.Add(x); }
            }
            return ReleventSpoilerlines;
        }


    }
}
