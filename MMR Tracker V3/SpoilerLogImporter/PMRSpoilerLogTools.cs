using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.SpoilerLogImporter
{
    internal class PMRSpoilerLogTools
    {
        public class SpoilerEntrance
        {
            public string entrance;
            public string exit;
            public string direction;
        }
        public static string SettingWebPage = "https://paper-mario-randomizer-server.ue.r.appspot.com/randomizer_settings/";
        public static void ParseSpoiler(TrackerObjects.InstanceData.TrackerInstance Instance)
        {
            var SpoilerLog = Instance.SpoilerLog.Log;

            string WebData = SettingWebPage + Path.GetFileNameWithoutExtension(Instance.SpoilerLog.FileName).Split('_')[0];

            Debug.WriteLine($"Getting settings from [{WebData}]");

            System.Net.WebClient wc = new System.Net.WebClient();
            byte[] raw = wc.DownloadData(WebData);
            string webString = Encoding.UTF8.GetString(raw);

            Dictionary<string, object> Settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(webString);

            foreach (var i in Settings)
            {
                if (Instance.ToggleOptions.ContainsKey(i.Key))
                {
                    Instance.ToggleOptions[i.Key].SetValue(bool.Parse(i.Value.ToString()));
                    Debug.WriteLine($"Option {i.Key} = {i.Value}");
                }
                else if (Instance.ChoiceOptions.ContainsKey(i.Key))
                {
                    Instance.ChoiceOptions[i.Key].SetValue(i.Value.ToString());
                    Debug.WriteLine($"Variable {i.Key} = {i.Value}");
                }
                else if (Instance.IntOptions.ContainsKey(i.Key))
                {
                    Instance.IntOptions[i.Key].SetValue(int.Parse(i.Value.ToString()));
                    Debug.WriteLine($"Variable {i.Key} = {i.Value}");
                }
            }

            var KootShuffle = int.Parse(Settings["IncludeFavorsMode"].ToString());
            var KootCoinShuffle = bool.Parse(Settings["IncludeCoinsFavors"].ToString());
            var LetterShuffle = int.Parse(Settings["IncludeLettersMode"].ToString());
            var DOJOShuffle = bool.Parse(Settings["IncludeDojo"].ToString());
            var RadioShuffle = bool.Parse(Settings["IncludeRadioTradeEvent"].ToString());
            var OverWorldCoinShuffle = bool.Parse(Settings["IncludeCoinsOverworld"].ToString());
            var BushTreeCoinShuffle = bool.Parse(Settings["IncludeCoinsFoliage"].ToString());
            var BlockCoinShuffle = bool.Parse(Settings["IncludeCoinsBlocks"].ToString());
            var ShuffleSuperMulti = bool.Parse(Settings["ShuffleBlocks"].ToString());
            var PartnerUpgradeShuffle = int.Parse(Settings["PartnerUpgradeShuffle"].ToString());

            foreach (var i in Instance.LocationPool.Values)
            {
                var Dict = i.GetDictEntry();

                i.SetRandomizedState(MiscData.RandomizedState.Randomized);

                if (Dict.ValidItemTypes.Contains("MultiCoinBlock") && !ShuffleSuperMulti)
                {
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized);
                }
                if (Dict.ValidItemTypes.Contains("SuperBlock") && !ShuffleSuperMulti && PartnerUpgradeShuffle == 0)
                {
                    i.SetRandomizedState(MiscData.RandomizedState.UnrandomizedManual);
                }

                if (Dict.SpoilerData.SpoilerLogNames.Contains("KootFavor") && KootShuffle < 1)
                {
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized);
                }
                if (Dict.SpoilerData.SpoilerLogNames.Contains("KootFavorKey") && KootShuffle < 2)
                {
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized);
                }
                if (Dict.SpoilerData.SpoilerLogNames.Contains("KootFavorCoin") && (!KootCoinShuffle || KootShuffle < 1))
                {
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized);
                }

                if (Dict.SpoilerData.SpoilerLogNames.Contains("LetterChain") && LetterShuffle < 3)
                {
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized);
                }
                if (Dict.SpoilerData.SpoilerLogNames.Contains("ChainLetterFinal") && LetterShuffle < 2)
                {
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized);
                }
                if (Dict.SpoilerData.SpoilerLogNames.Contains("SimpleLetter") && LetterShuffle < 1)
                {
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized);
                }

                if (Dict.SpoilerData.SpoilerLogNames.Contains("DOJO") && !DOJOShuffle)
                {
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized);
                }
                if (Dict.SpoilerData.SpoilerLogNames.Contains("RadioTrade") && !RadioShuffle)
                {
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized);
                }

                if (Dict.SpoilerData.SpoilerLogNames.Contains("TreeBushCoin") && !BushTreeCoinShuffle)
                {
                    if (Dict.OriginalItem != "Coin") { throw new Exception($"Why {Dict.ID}"); }
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized);
                }
                if (Dict.SpoilerData.SpoilerLogNames.Contains("OverworldCoin") && !OverWorldCoinShuffle)
                {
                    if (Dict.OriginalItem != "Coin") { throw new Exception($"Why {Dict.ID}"); }
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized);
                }
                if (Dict.SpoilerData.SpoilerLogNames.Contains("BlockCoin") && !BlockCoinShuffle)
                {
                    if (Dict.OriginalItem != "Coin") { throw new Exception($"Why {Dict.ID}"); }
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized);
                }

            }

            Dictionary<string, object> LogObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(string.Join("\r\n", SpoilerLog));

            Dictionary<string, string[]> SuperBlocksLocations = new Dictionary<string, string[]>();

            foreach (var loc in Instance.GetAllRandomizableExits())
            {
                loc.SpoilerDefinedDestinationExit = loc.GetVanillaDestination();
            }

            foreach (var i in LogObject)
            {
                if (i.Key == "SeedHashItems" || i.Key == "difficulty" || i.Key == "move_costs") { continue; }
                else if (i.Key == "superblocks")
                {
                    SuperBlocksLocations = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(JsonConvert.SerializeObject(i.Value));
                }
                else if (i.Key == "entrances")
                {
                    List<SpoilerEntrance> EntranceData = JsonConvert.DeserializeObject<List<SpoilerEntrance>>(JsonConvert.SerializeObject(i.Value));

                    foreach (var entrance in EntranceData)
                    {
                        Debug.WriteLine($"Checking Entrance {entrance.entrance}");
                        var Ent = Instance.AreaPool.First(x => x.Key.Replace("'", "") == entrance.entrance).Value.GetAllRandomizableExits().First();
                        var Dest = Instance.AreaPool.First(x => x.Key.Replace("'", "") == entrance.exit).Value.GetAllRandomizableExits().First();
                        Ent.Value.SpoilerDefinedDestinationExit = Dest.Value.EntrancePair.AsDestination();
                        Dest.Value.SpoilerDefinedDestinationExit = Ent.Value.EntrancePair.AsDestination();
                    }
                }
                else if (i.Key == "sphere_log")
                {
                    Dictionary<string, object> Spheres = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(i.Value));
                    var StartingItems = Spheres["starting_items"];
                    List<string> StartingItemList = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(StartingItems));
                    foreach (var item in StartingItemList)
                    {
                        string StartingItem = item.Replace("*", "");

                        var ItemToPlace = Instance.GetItemToPlace(StartingItem, false, true, true);

                        if (ItemToPlace is null) { throw new Exception($"{StartingItem} Is not a valid Item\n{item}"); }

                        ItemToPlace.AmountInStartingpool++;
                    }
                }
                else
                {
                    Dictionary<string, string> Locations = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(i.Value));
                    foreach (var Location in Locations)
                    {
                        //Get Spoiler Location
                        if (!Instance.LocationPool.ContainsKey(Location.Key)) { throw new Exception($"{Location.Key} Is not a valid Location"); }
                        var Loc = Instance.LocationPool[Location.Key];
                        //Check if the location is unrandomized
                        bool CheckUnrand = Loc.IsUnrandomized(MiscData.UnrandState.Any);

                        string SpoilerGivenItem = Location.Value.ToString();

                        //Remove coin/starpoint costs from item
                        if (SpoilerGivenItem.Contains('(') && (SpoilerGivenItem.SplitOnce('(', true).Item2.Contains("coins") || SpoilerGivenItem.SplitOnce('(', true).Item2.Contains(" sp)")))
                        {
                            SpoilerGivenItem = SpoilerGivenItem.SplitOnce('(', true).Item1.Trim();
                        }

                        //If the location is unrandomized, Ensure it contains its vanilla item in the spoiler log
                        if (CheckUnrand)
                        {
                            var OriginalItem = Instance.ItemPool[Loc.GetDictEntry().OriginalItem];
                            if (OriginalItem.ID != SpoilerGivenItem && OriginalItem.GetDictEntry().Name != SpoilerGivenItem)
                            {
                                throw new Exception($"{Loc.ID} Was unrandomized but did not contain its vanilla item\n{Location.Key}: {Location.Value}\n{JsonConvert.SerializeObject(Loc, Utility.DefaultSerializerSettings)}");
                            }
                            continue;
                        }

                        //Serach for the item in the item pool
                        var ItemToPlaceAny = Instance.GetItemToPlace(SpoilerGivenItem, false, true, true, false, true);
                        //search for an item in the item pool that can be placed
                        var ItemToPlace = Instance.GetItemToPlace(SpoilerGivenItem, false, true, true);

                        //If no item was found, check if the spoiler item is a trap
                        if (ItemToPlace is null && SpoilerGivenItem.StartsWith("TRAP"))
                        {
                            Instance.LocationPool[Location.Key].Randomizeditem.SpoilerLogGivenItem = SpoilerGivenItem;
                            continue;
                        }

                        //Check if the item is not valid or if no more of that item can be placed.
                        if (ItemToPlaceAny is null) { throw new Exception($"{SpoilerGivenItem} Is not a valid Item\n{Location.Key}: {Location.Value}"); }
                        else if (ItemToPlace is null) { throw new Exception($"No More {SpoilerGivenItem} could be placed\n{Location.Key}: {Location.Value}\n"); }

                        Instance.LocationPool[Location.Key].Randomizeditem.SpoilerLogGivenItem = ItemToPlace.ID;
                    }
                }
            }

            Utility.PrintObjectToConsole(SuperBlocksLocations);

            foreach (var i in Instance.LocationPool)
            {
                if (i.Value.Randomizeditem.SpoilerLogGivenItem == null && i.Value.SingleValidItem == null && i.Value.IsRandomized())
                {
                    if (i.Value.GetDictEntry().ValidItemTypes.Any(x => x.In("MultiCoinBlock", "SuperBlock")))
                    {
                        bool ContainedSuperBlock =
                            SuperBlocksLocations.ContainsKey(i.Value.GetDictEntry().Area) &&
                            SuperBlocksLocations[i.Value.GetDictEntry().Area].Any(x => i.Key.StartsWith(x));
                        i.Value.Randomizeditem.SpoilerLogGivenItem = ContainedSuperBlock ? "PartnerUp1" : "Coin";
                    }
                    else
                    {
                        Debug.WriteLine($"{i.Value.GetDictEntry().Area} - {i.Key} Did not have spoiler data");
                    }
                }
            }

        }
    }
}
