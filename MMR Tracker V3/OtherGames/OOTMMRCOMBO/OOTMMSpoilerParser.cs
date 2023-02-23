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
        public static string Hintnamedata = "{\r\n  \"OOT RAVAGED_VILLAGE\": [\r\n    \"OOT Kakariko Song Shadow\"\r\n  ],\r\n  \"OOT FROGS_FINAL\": [\r\n    \"OOT Zora River Frogs Game\"\r\n  ],\r\n  \"OOT ZORA_KING\": [\r\n    \"OOT Zora Domain Tunic\",\r\n    \"OOT Zora Domain Eyeball Frog\"\r\n  ],\r\n  \"OOT FISHING\": [\r\n    \"OOT Fishing Pond Child\",\r\n    \"OOT Fishing Pond Adult\"\r\n  ],\r\n  \"OOT GERUDO_ARCHERY\": [\r\n    \"OOT Gerudo Fortress Archery Reward 1\",\r\n    \"OOT Gerudo Fortress Archery Reward 2\"\r\n  ],\r\n  \"OOT HAUNTED_WASTELAND\": [\r\n    \"OOT Haunted Wasteland Chest\"\r\n  ],\r\n  \"OOT GANON_FAIRY\": [\r\n    \"OOT Great Fairy Defense Upgrade\"\r\n  ],\r\n  \"OOT TEMPLE_FIRE_HAMMER\": [\r\n    \"OOT Fire Temple Hammer\"\r\n  ],\r\n  \"OOT TEMPLE_FIRE_SCARECROW\": [\r\n    \"OOT Fire Temple Scarecrow Chest\"\r\n  ],\r\n  \"OOT GTG_WATER\": [\r\n    \"OOT Gerudo Training Water\"\r\n  ],\r\n  \"MM BANK_3\": [\r\n    \"MM Clock Town Bank Reward 3\"\r\n  ],\r\n  \"MM SOUND_CHECK\": [\r\n    \"MM Milk Bar Troupe Leader Mask\"\r\n  ],\r\n  \"MM COUPLE_MASK\": [\r\n    \"MM Stock Pot Inn Couple's Mask\"\r\n  ],\r\n  \"MM BOAT_ARCHERY\": [\r\n    \"MM Tourist Information Boat Cruise\"\r\n  ],\r\n  \"MM BUTLER_RACE\": [\r\n    \"MM Deku Shrine Mask of Scents\"\r\n  ],\r\n  \"MM DON_GERO_CHOIR\": [\r\n    \"MM Mountain Village Frog Choir HP\"\r\n  ],\r\n  \"MM GORON_RACE\": [\r\n    \"MM Goron Race Reward\"\r\n  ],\r\n  \"MM RANCH_DEFENSE\": [\r\n    \"MM Romani Ranch Aliens\",\r\n    \"MM Romani Ranch Cremia Escort\"\r\n  ],\r\n  \"MM FISHERMAN_GAME\": [\r\n    \"MM Great Bay Coast Fisherman HP\"\r\n  ],\r\n  \"MM PINNACLE_ROCK_HP\": [\r\n    \"MM Pinnacle Rock HP\"\r\n  ],\r\n  \"MM OSH_CHEST\": [\r\n    \"MM Ocean Spider House Chest HP\"\r\n  ],\r\n  \"MM GRAVEYARD_NIGHT3\": [\r\n    \"MM Beneath The Graveyard Dampe Chest\"\r\n  ],\r\n  \"MM SONG_ELEGY\": [\r\n    \"MM Ancient Castle of Ikana Song Emptiness\"\r\n  ],\r\n  \"MM SECRET_SHRINE_WART_HP\": [\r\n    \"MM Secret Shrine Boss 4 Chest\",\r\n    \"MM Secret Shrine HP Chest\"\r\n  ]\r\n}";

        public static void readAndApplySpoilerLog(LogicObjects.TrackerInstance Instance)
        {
            Instance.ToggleAllTricks(false);
            foreach (var i in Instance.EntrancePool.AreaList.SelectMany(x => x.Value.LoadingZoneExits))
            {
                i.Value.RandomizedState = MiscData.RandomizedState.Unrandomized;
            }
            foreach (var i in Instance.LocationPool.Values)
            {
                i.SetRandomizedState(MiscData.RandomizedState.Randomized, Instance);
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
                if (GetCurrentSettingLine(l, spoilerFileLocation, out bool EOO)) 
                {
                    if (EOO) { break; }
                    continue;
                }
                if (!ApplySettings(l, spoilerFileLocation, Instance)) { continue; }
            }

            List<LocationData.LocationObject> LocationsInspoilerLog= new List<LocationData.LocationObject>();

            var ReleventData = GetReleventSpoilerLines(Instance.SpoilerLog.Log);
            foreach (var i in ReleventData)
            {
                var entryData = i.Split(':').Select(x => x.Trim()).ToArray();

                var location = Instance.LocationPool.ContainsKey(entryData[0]) ? Instance.LocationPool[entryData[0]] : null;

                if (location is not null)
                {
                    LocationsInspoilerLog.Add(location);
                    ItemData.ItemObject item = Instance.GetItemToPlace(entryData[1]);
                    if (item is null) { Debug.WriteLine($"{entryData[1]} was not a valid Item or no more of this could be placed!"); }
                    location.Randomizeditem.SpoilerLogGivenItem = item?.Id ?? entryData[1];
                }
                else
                {
                    Debug.WriteLine($"{entryData[0]} was not a valid location!");
                }
            }

            List<string> ManualTrackItems = new()
            {
                "OOT_GS_TOKEN",
                "MM_GS_TOKEN_OCEAN",
                "MM_GS_TOKEN_SWAMP",
                "MM_STRAY_FAIRY_GB",
                "MM_STRAY_FAIRY_SH",
                "MM_STRAY_FAIRY_ST",
                "MM_STRAY_FAIRY_WF"
            };

            foreach(var i in Instance.LocationPool.Values)
            {
                if (!LocationsInspoilerLog.Contains(i)) 
                {
                    if (ManualTrackItems.Contains(i.GetDictEntry(Instance).OriginalItem))
                    {
                        i.SetRandomizedState(MiscData.RandomizedState.UnrandomizedManual, Instance);
                    }
                    else
                    {
                        i.SetRandomizedState(MiscData.RandomizedState.Unrandomized, Instance);
                    }
                }
                
            }

            Instance.StaticOptions.EntranceRandoFeatures = false;

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
                        //For now just randomize them if their found in the spoiler log
                        //i.Value.RandomizedState = MiscData.RandomizedState.Randomized;
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
                var HintData = ParseHintLine(Line);
                if (HintData is null) { return false; }
                if (!Instance.HintPool.TryGetValue(HintData.HintStoneName, out var GossipStoneEntry))
                {
                    Debug.WriteLine($"{HintData.HintStoneName} Was not a valid Hint Entry");
                    return false;
                }
                
                switch (HintData.HintType)
                {
                    case HintType.ItemRegion:
                        GossipStoneEntry.SpoilerHintText = $"{HintData.PrettyLocationText} contains {string.Join(" and ", HintData.HintedItems)}";
                        break;
                    case HintType.ItemExact:
                        if (HintData.HintedLocations.Length == HintData.HintedItems.Length && HintData.HintType == HintType.ItemExact)
                        {
                            for(var i = 0; i < HintData.HintedLocations.Length; i++)
                            {
                                var Item = Instance.ItemPool.Values.FirstOrDefault(x => x.GetDictEntry(Instance).SpoilerData.SpoilerLogNames.Contains(HintData.HintedItems[i]));
                                var Location = Instance.LocationPool.Values.FirstOrDefault(x => x.GetDictEntry(Instance).SpoilerData.SpoilerLogNames.Contains(HintData.HintedLocations[i]));
                                if (Item is not null && Location is not null)
                                {
                                    GossipStoneEntry.ParsedHintData.Add(Location.ID, Item.Id);
                                }
                            }
                        }
                        GossipStoneEntry.SpoilerHintText = $"{HintData.PrettyLocationText} contains {string.Join(" and ", HintData.HintedItems)}";
                        break;
                    case HintType.Hero:
                        GossipStoneEntry.SpoilerHintText = $"{HintData.PrettyLocationText} is Way of the Hero";
                        break;
                    case HintType.Foolish:
                        GossipStoneEntry.SpoilerHintText = $"{HintData.PrettyLocationText} is Foolish";
                        break;
                }
            }
            if (spoilerFileLocation["Entrances"])
            {
                string NewLine = Line;

                var EntranceData = NewLine.Split(new string[] { "->" }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
                var Source = EntranceData[0].Split('/');
                var Destination = EntranceData[1].Split('/');

                Instance.EntrancePool.EntranceIsValid(Source[0], Source[1], false, out bool SourceArea, out bool SourceExit);
                Instance.EntrancePool.EntranceIsValid(Destination[0], Destination[1], false, out bool DestArea, out bool DestExit);

                if (SourceArea && SourceExit && DestArea && DestExit)
                {
                    var CurExit = Instance.EntrancePool.AreaList[Source[0]].LoadingZoneExits[Source[1]];
                    CurExit.SpoilerDefinedDestinationExit = new EntranceData.EntranceRandoDestination
                    {
                        region = Destination[1],
                        from = Destination[0]
                    };
                    CurExit.RandomizedState = MiscData.RandomizedState.Randomized;
                    //Probaly not needed but just incase they remove dungeon => overworld from the log
                    var PairArea = CurExit.EntrancePair?.Area;
                    var PairExit = CurExit.EntrancePair?.Exit;
                    if (PairArea is not null && PairExit is not null)
                    {
                        Instance.EntrancePool.AreaList[PairArea].LoadingZoneExits[PairExit].RandomizedState = MiscData.RandomizedState.Randomized;
                    }
                }
                else 
                { 
                    Debug.WriteLine($"{NewLine} Could not be mapped to an entrance!");
                    Debug.WriteLine($"{Source[0]}: {SourceArea}");
                    Debug.WriteLine($"{Source[1]}: {SourceExit}");
                    Debug.WriteLine($"{Destination[0]}: {DestArea}");
                    Debug.WriteLine($"{Destination[1]}: {DestExit}");
                }
            }
            return true;
        }

        public class SpoilerHintData
        {
            public string HintStoneName;
            public HintType HintType;
            public string HintedLocationText;
            public string PrettyLocationText;
            public string[] HintedLocations;
            public string[] HintedItems;
        }

        public enum HintType
        {
            ItemExact,
            ItemRegion,
            Hero,
            Foolish,
            none
        }

        public static SpoilerHintData ParseHintLine(string line)
        {
            var HintNames = new Dictionary<string, string[]>();
            HintNames = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(Hintnamedata);
            SpoilerHintData result = new SpoilerHintData();
            if (string.IsNullOrEmpty(line) || line.StartsWith("=")) { return null; }  
            result.HintStoneName = line.Split(':')[0];
            string SpoilerHintType = line.Split(':')[1].Split(',')[0].Trim();
            result.HintType = OOTMMSpoilerParser.HintType.none;
            switch (SpoilerHintType)
            {
                case "Item-Exact":
                    result.HintType = HintType.ItemExact;
                    break;
                case "Item-Region":
                    result.HintType = HintType.ItemRegion;
                    break;
                case "Hero":
                    result.HintType = HintType.Hero;
                    break;
                case "Foolish":
                    result.HintType = HintType.Foolish;
                    break;
            }
            string LocationAnditemData = line.Replace($"{result.HintStoneName}: {SpoilerHintType}, ", "");
            result.HintedLocationText = LocationAnditemData.Split(new[] { '(' }, 2)[0].Trim();
            result.PrettyLocationText = result.HintedLocationText.ToLower().Replace("_", " ");
            result.PrettyLocationText = Regex.Replace(result.PrettyLocationText, @"(^\w)|(\s\w)", m => m.Value.ToUpper()).Replace("Mm", "MM").Replace("Oot", "OOT");
            
            if (result.HintType == HintType.ItemExact || result.HintType == HintType.ItemRegion)
            {
                if (HintNames.ContainsKey(result.HintedLocationText))
                {
                    result.HintedLocations = HintNames[result.HintedLocationText];
                }
                else { result.HintedLocations = Array.Empty<string>(); }
                string itemData = LocationAnditemData.Replace($"{result.HintedLocationText} ", "");
                itemData = itemData[1..^1];
                result.HintedItems = itemData.Split(",").Select(x => x.Trim()).ToArray();
            }
            return result;
        }


        public static bool GetCurrentSettingLine(string l, Dictionary<string, bool> spoilerFileLocation, out bool EndOfOptions)
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
            if (l == "Spheres") { EndOfOptions = true; return true; }
            return false;
        }

        public static List<string> GetReleventSpoilerLines(string[] Data)
        {
            bool AtLocationData = false;
            var ReleventSpoilerlines = new List<string>();
            foreach (var x in Data)
            {
                if (x == "Location List") { AtLocationData = true; continue; }
                if (!AtLocationData || string.IsNullOrWhiteSpace(x)) { continue; }
                var LineParts = x.Split(':').Select(x => x.Trim()).ToArray();
                if (LineParts.Count() > 1 && !string.IsNullOrWhiteSpace(LineParts[1]))
                {
                    ReleventSpoilerlines.Add(x);
                }
            }
            return ReleventSpoilerlines;
            bool AtSphereData = false;
            bool CurrentLineEmpty = false;
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
