using MMR_Tracker_V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
