using MMR_Tracker_V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMUtils;
using static MMR_Tracker_V3.NetCode.NetData;

namespace WebServer
{
    internal class Utility
    {
        public static Dictionary<int, Dictionary<string, int>> GetItemsBelongingToPlayer(int PlayerID, HashSet<Guid> FromPlayers)
        {
            Dictionary<int, Dictionary<string, int>> MultiworldItemsForPlayer = [];
            foreach (var ClientID in FromPlayers)
            {
                var Client = ServerThread.Clients[ClientID];
                if (Client.MultiworldItemData.TryGetValue(PlayerID, out Dictionary<string, int> PlayerItems))
                {
                    MultiworldItemsForPlayer.Add(Client.PlayerID, PlayerItems);
                }
            }
            return MultiworldItemsForPlayer;
        }
        public static Dictionary<string, string> GetCheckedLocations(HashSet<Guid> FromPlayers)
        {
            Dictionary<string, string> AllCheckedLocations = [];
            foreach (var ClientID in FromPlayers)
            {
                var Client = ServerThread.Clients[ClientID];
                foreach (var l in Client.OnlineLocationData)
                {
                    AllCheckedLocations[l.Key] = l.Value;
                }
            }
            return AllCheckedLocations;
        }
        public static HashSet<Guid> GetPlayers(params int[] PlayerIDs)
        {
            HashSet<Guid> Whitelist = [];
            foreach (var i in ServerThread.Clients)
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
            HashSet<Guid> Whitelist = [];
            foreach (var i in ServerThread.Clients)
            {
                if (PlayerIDs.Contains(i.Value.PlayerID)) { continue; }
                Whitelist.Add(i.Key);
            }
            return Whitelist;
        }

        public static void SendChatToClients(ChatMessage Chat, HashSet<Guid> _PlayerToUpdate)
        {
            IEnumerable<ServerClient> ClientsToUpdate = ServerThread.Clients.Where(x => _PlayerToUpdate.Contains(x.Key)).Select(x => x.Value);
            NetPacket Update = new(-1, PacketType.ChatMessage)
            {
                ChatMessage = Chat
            };
            string PacketString = Update.ToFormattedJson();
            foreach (var Client in ClientsToUpdate)
            {
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(PacketString);
                Client.NetClient.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
            }
        }

        public static void SendCheckedLocationsToClients(HashSet<Guid> _PlayerToUpdate, HashSet<Guid> _PlayersToGetDataFrom)
        {
            HashSet<Guid> PlayerToUpdate = _PlayerToUpdate ?? [];
            HashSet<Guid> PlayersToGetDataFrom = _PlayersToGetDataFrom ?? [];
            IEnumerable<ServerClient> ClientsToUpdate = ServerThread.Clients.Where(x => PlayerToUpdate.Contains(x.Key)).Select(x => x.Value);
            NetPacket Update = new(-1, PacketType.OnlineSynedLocations)
            {
                LocationData = Utility.GetCheckedLocations(PlayersToGetDataFrom)
            };
            string PacketString = Update.ToFormattedJson();
            foreach (var Client in ClientsToUpdate)
            {
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(PacketString);
                Client.NetClient.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
            }
        }

        public static void SendMultiworldItemsToClients(HashSet<Guid> _PlayerToUpdate, HashSet<Guid> _PlayersToGetDataFrom)
        {
            HashSet<Guid> PlayerToUpdate = _PlayerToUpdate ?? [];
            HashSet<Guid> PlayersToGetDataFrom = _PlayersToGetDataFrom ?? [];
            IEnumerable<ServerClient> ClientsToUpdate = ServerThread.Clients.Where(x => PlayerToUpdate.Contains(x.Key)).Select(x => x.Value);
            foreach (var Client in ClientsToUpdate)
            {
                NetPacket Update = new(-1, PacketType.MultiWorldItems)
                {
                    ItemData = Utility.GetItemsBelongingToPlayer(Client.PlayerID, PlayersToGetDataFrom)
                };
                string PacketString = Update.ToFormattedJson();

                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(PacketString);
                Client.NetClient.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
            }
        }
    }
}
