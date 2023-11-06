using MMR_Tracker_V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.NetData;

namespace WebServer
{
    internal class Utility
    {
        public static Dictionary<int, Dictionary<string, int>> GetItemsBelongingToPlayer(int PlayerID, HashSet<Guid> FromPlayers)
        {
            Dictionary<int, Dictionary<string, int>> MultiworldItemsForPlayer = new();
            foreach (var ClientID in FromPlayers)
            {
                var Client = AsyncServerTest.Clients[ClientID];
                if (Client.MultiworldItemData.ContainsKey(PlayerID))
                {
                    MultiworldItemsForPlayer.Add(Client.PlayerID, Client.MultiworldItemData[PlayerID]);
                }
            }
            return MultiworldItemsForPlayer;
        }
        public static Dictionary<string, string> GetCheckedLocations(HashSet<Guid> FromPlayers)
        {
            Dictionary<string, string> AllCheckedLocations = new Dictionary<string, string>();
            foreach (var ClientID in FromPlayers)
            {
                var Client = AsyncServerTest.Clients[ClientID];
                foreach (var l in Client.OnlineLocationData)
                {
                    AllCheckedLocations[l.Key] = l.Value;
                }
            }
            return AllCheckedLocations;
        }
        public static HashSet<Guid> GetPlayers(params int[] PlayerIDs)
        {
            HashSet<Guid> Whitelist = new HashSet<Guid>();
            foreach (var i in AsyncServerTest.Clients)
            {
                if (PlayerIDs.Contains(i.Value.PlayerID))
                {
                    Whitelist.Add(i.Key);
                }
            }
            return Whitelist;
        }
        public static HashSet<Guid> GetPlayersExcept(params int[] PlayerIDs)
        {
            HashSet<Guid> Whitelist = new HashSet<Guid>();
            foreach (var i in AsyncServerTest.Clients)
            {
                if (PlayerIDs.Contains(i.Value.PlayerID)) { continue; }
                Whitelist.Add(i.Key);
            }
            return Whitelist;
        }

        public static void SendChatToClients(ChatMessage Chat, HashSet<Guid> _PlayerToUpdate)
        {
            IEnumerable<ServerClient> ClientsToUpdate = AsyncServerTest.Clients.Where(x => _PlayerToUpdate.Contains(x.Key)).Select(x => x.Value);
            NetPacket Update = new NetPacket(-1, PacketType.ChatMessage);
            Update.ChatMessage = Chat;
            string PacketString = Update.ToFormattedJson();
            foreach (var Client in ClientsToUpdate)
            {
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(PacketString);
                Client.NetClient.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
            }
        }

        public static void SendCheckedLocationsToClients(HashSet<Guid> _PlayerToUpdate, HashSet<Guid> _PlayersToGetDataFrom)
        {
            HashSet<Guid> PlayerToUpdate = _PlayerToUpdate??new HashSet<Guid>();
            HashSet<Guid> PlayersToGetDataFrom = _PlayersToGetDataFrom??new HashSet<Guid>();
            IEnumerable<ServerClient> ClientsToUpdate = AsyncServerTest.Clients.Where(x => PlayerToUpdate.Contains(x.Key)).Select(x => x.Value);
            NetPacket Update = new NetPacket(-1, PacketType.OnlineSynedLocations);
            Update.LocationData = Utility.GetCheckedLocations(PlayersToGetDataFrom);
            string PacketString = Update.ToFormattedJson();
            foreach (var Client in ClientsToUpdate)
            {
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(PacketString);
                Client.NetClient.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
            }
        }

        public static void SendMultiworldItemsToClients(HashSet<Guid> _PlayerToUpdate, HashSet<Guid> _PlayersToGetDataFrom)
        {
            HashSet<Guid> PlayerToUpdate = _PlayerToUpdate??new HashSet<Guid>();
            HashSet<Guid> PlayersToGetDataFrom = _PlayersToGetDataFrom??new HashSet<Guid>();
            IEnumerable<ServerClient> ClientsToUpdate = AsyncServerTest.Clients.Where(x => PlayerToUpdate.Contains(x.Key)).Select(x => x.Value);
            foreach (var Client in ClientsToUpdate)
            {
                NetPacket Update = new NetPacket(-1, PacketType.MultiWorldItems);
                Update.ItemData = Utility.GetItemsBelongingToPlayer(Client.PlayerID, PlayersToGetDataFrom);
                string PacketString = Update.ToFormattedJson();

                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(PacketString);
                Client.NetClient.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
            }
        }
    }
}
