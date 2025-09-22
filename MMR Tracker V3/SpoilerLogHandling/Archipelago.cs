using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MMR_Tracker_V3.SpoilerLogHandling
{
    public class Archipelago
    {
        public class GenericAPSpoiler
        {
            public List<(string Location, string Item, int Player)> Locations = [];
            public Dictionary<string, object> SlotData = [];
        };
        public static GenericAPSpoiler CreateGenericSpoilerLog(InstanceData.InstanceContainer IC)
        {
            GenericAPSpoiler genericAPSpoiler = new GenericAPSpoiler();
            var APClient = IC.netConnection.ArchipelagoClient;
            var Data = APClient.Session.Locations.ScoutLocationsAsync(APClient.Session.Locations.AllLocations.ToArray()).Result;
            foreach (var Entry in Data)
            {
                string LocationName = Entry.Value.LocationName;
                string ItemName = Entry.Value.ItemName;
                genericAPSpoiler.Locations.Add((LocationName, ItemName, Entry.Value.Player));
            }
            genericAPSpoiler.SlotData = APClient.GetLoginSuccessInfo().SlotData;

            return genericAPSpoiler;
        }
        public static bool IsGenericAPSpoiler(InstanceData.TrackerInstance instance)
        {
            return IsGenericAPSpoiler(instance, out _);
        }
        public static bool IsGenericAPSpoiler(InstanceData.TrackerInstance instance, out Archipelago.GenericAPSpoiler Log)
        {
            Log = null;
            if (instance.GetParentContainer().netConnection.OnlineMode != NetCode.NetData.OnlineMode.Archipelago) { return false; }
            try
            {
                Archipelago.GenericAPSpoiler genericAPSpoiler =
                    JsonConvert.DeserializeObject<Archipelago.GenericAPSpoiler>(string.Join(" ", instance.SpoilerLog.Log));
                var LocationData = genericAPSpoiler.Locations;
                var SlotData = genericAPSpoiler.SlotData;
                if (LocationData.Count < 1 || SlotData.Count < 1) { return false; }
                Log = genericAPSpoiler;
            }
            catch { return false; }
            return true;
        }
    }
}
