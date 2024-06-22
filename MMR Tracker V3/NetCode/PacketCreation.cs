using MMR_Tracker_V3.TrackerObjects;
using System.Collections.Generic;
using System.Linq;

namespace MMR_Tracker_V3.NetCode
{
    public class PacketCreation
    {
        public static NetData.NetPacket? CreateCoopPacket(int PlayerID, string Password, InstanceData.TrackerInstance arg2, IEnumerable<LocationData.LocationObject> locationsUpdated)
        {
            NetData.NetPacket packet = new(PlayerID, NetData.PacketType.OnlineSynedLocations, Password)
            {
                LocationData = getCheckedLocations(arg2),
                UpdateWhitelist = locationsUpdated.Any(x => x.CheckState == MiscData.CheckState.Checked) ? null : [-1]
            };
            return packet;
        }
        public static NetData.NetPacket? CreateMultiWorldPacket(int PlayerID, string Password, InstanceData.TrackerInstance arg2, IEnumerable<LocationData.LocationObject> locationsUpdated)
        {
            NetData.NetPacket packet = new(PlayerID, NetData.PacketType.MultiWorldItems, Password)
            {
                ItemData = GetMultiworldItemsToSend(arg2),
                UpdateWhitelist = locationsUpdated.Select(x => x.Randomizeditem.OwningPlayer).Distinct().ToArray()
            };
            return packet;
        }
        public static Dictionary<string, string> getCheckedLocations(InstanceData.TrackerInstance instance)
        {
            return instance.LocationPool.Values
                .Where(x => x.CheckState == MiscData.CheckState.Checked)
                .ToDictionary(x => x.ID, x => x.Randomizeditem.Item);
        }

        public static Dictionary<int, Dictionary<string, int>> GetMultiworldItemsToSend(InstanceData.TrackerInstance instance)
        {
            Dictionary<int, Dictionary<string, int>> PlayersSentItem = [];
            foreach (var Item in instance.ItemPool)
            {
                if (!Item.Value.AmountSentToPlayer.Any()) { continue; }

                foreach (var Player in Item.Value.AmountSentToPlayer)
                {
                    if (Player.Value < 1) { continue; }
                    if (!PlayersSentItem.ContainsKey(Player.Key)) { PlayersSentItem[Player.Key] = []; }
                    PlayersSentItem[Player.Key][Item.Value.GetDictEntry().GetName()] = Player.Value;
                }
            }
            return PlayersSentItem;
        }
    }
}
