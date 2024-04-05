using Archipelago.MultiClient.Net.Packets;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;

namespace MMR_Tracker_V3.SpoilerLogImporter
{
    public class Archipelago
    {
        public class GenericAPSpoiler
        {
            public List<(string Location, string Item, int Player)> Locations = [];
            public Dictionary<string, object> SlotData = [];
        };
        public static string CreateGenericSpoilerLog(InstanceData.InstanceContainer IC)
        {
            GenericAPSpoiler genericAPSpoiler = new GenericAPSpoiler();
            var APClient = IC.netConnection.ArchipelagoClient;
            var Data = APClient.Session.Locations.ScoutLocationsAsync(APClient.Session.Locations.AllLocations.ToArray()).Result;
            foreach (var Entry in Data.Locations)
            {
                string LocationName = APClient.Session.Locations.GetLocationNameFromId(Entry.Location);
                string ItemName = APClient.Session.Items.GetItemName(Entry.Item);
                genericAPSpoiler.Locations.Add((LocationName, ItemName, Entry.Player));
            }
            genericAPSpoiler.SlotData = APClient.GetLoginSuccessInfo().SlotData;
            
            return genericAPSpoiler.ToFormattedJson();
        }
    }
}
