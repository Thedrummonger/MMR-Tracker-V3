using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames.PaperMarioRando
{
    internal class SpoilerLogParser
    {
        public class SpoilerEntrance
        {
            public string entrance;
            public string exit;
            public string direction;
        }
        public static string SettingWebPage = "https://paper-mario-randomizer-server.ue.r.appspot.com/randomizer_settings/";
        public static void ParseSpoiler(LogicObjects.TrackerInstance Instance)
        {
            var SpoilerLog = Instance.SpoilerLog.Log;

            string WebData = SettingWebPage + Path.GetFileNameWithoutExtension(Instance.SpoilerLog.FileName).Split('_')[0];
            
            Debug.WriteLine($"Getting settings from [{WebData}]");

            System.Net.WebClient wc = new System.Net.WebClient();
            byte[] raw = wc.DownloadData(WebData);
            string webString = System.Text.Encoding.UTF8.GetString(raw);

            Dictionary<string, object> Settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(webString);

            foreach (var i in Settings)
            {
                if (Instance.UserOptions.ContainsKey(i.Key)) 
                { 
                    Instance.UserOptions[i.Key].CurrentValue = i.Value.ToString().ToLower();
                    Debug.WriteLine($"Option {i.Key} = {i.Value}");  
                }
                else if (Instance.Variables.ContainsKey(i.Key))
                {
                    Instance.Variables[i.Key].Value = i.Value;
                    Debug.WriteLine($"Variable {i.Key} = {i.Value}");
                }
            }

            Dictionary<string, object> LogObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(string.Join("\r\n", SpoilerLog));

            foreach(var loc in Instance.EntrancePool.AreaList.Values.SelectMany(x => x.RandomizableExits(Instance)))
            {
                loc.Value.SpoilerDefinedDestinationExit = loc.Value.GetVanillaDestination();
            }

            foreach(var i in LogObject)
            {
                if (i.Key == "SeedHashItems" || i.Key == "difficulty" || i.Key == "move_costs" || i.Key == "superblocks") { continue; }
                else if (i.Key == "entrances")
                {
                    List<SpoilerEntrance> EntranceData = JsonConvert.DeserializeObject<List<SpoilerEntrance>>(JsonConvert.SerializeObject(i.Value));

                    foreach(var entrance in EntranceData)
                    {
                        var Ent = Instance.EntrancePool.AreaList.First(x => x.Key.Replace("'", "") == entrance.entrance).Value.RandomizableExits(Instance).First();
                        var Dest = Instance.EntrancePool.AreaList.First(x => x.Key.Replace("'", "") == entrance.exit).Value.RandomizableExits(Instance).First();
                        Ent.Value.SpoilerDefinedDestinationExit = Dest.Value.GetDestnationFromEntrancePair();
                        Dest.Value.SpoilerDefinedDestinationExit = Ent.Value.GetDestnationFromEntrancePair();
                    }
                }
                else if (i.Key == "sphere_log")
                {
                    Dictionary<string, object> Spheres = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(i.Value));
                    var StartingItems = Spheres["starting_items"];
                    List<string> StartingItemList = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(StartingItems));
                    foreach(var item in StartingItemList)
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
                    foreach(var Location in Locations)
                    {
                        if (!Instance.LocationPool.ContainsKey(Location.Key)) { throw new Exception($"{Location.Key} Is not a valid Location"); }

                        string SpoilerGivenItem = Location.Value.ToString();

                        if (SpoilerGivenItem.Contains("(") && (SpoilerGivenItem.SplitOnce('(', true)[1].Contains("coins") || SpoilerGivenItem.SplitOnce('(', true)[1].Contains(" sp)"))) { SpoilerGivenItem = SpoilerGivenItem.SplitOnce('(', true)[0].Trim(); }

                        string ItemID = null;
                        var ItemToPlace = Instance.GetItemToPlace(SpoilerGivenItem, false, true, true);
                        if (ItemToPlace is null && SpoilerGivenItem.StartsWith("TRAP")) { ItemID = SpoilerGivenItem; }
                        else if (ItemToPlace is not null) { ItemID = ItemToPlace.Id; }

                        if (ItemID is null) { throw new Exception($"{SpoilerGivenItem} Is not a valid Item\n{Location.Key}: {Location.Value}"); }

                        Instance.LocationPool[Location.Key].Randomizeditem.SpoilerLogGivenItem = ItemID;
                    }
                }
            }

            foreach(var i in Instance.LocationPool)
            {
                if (i.Value.Randomizeditem.SpoilerLogGivenItem == null && i.Value.SingleValidItem == null)
                {
                    Debug.WriteLine($"{i.Value.GetDictEntry(Instance).Area} - {i.Key} Did not have spoiler data");
                }
            }

        }
    }
}
