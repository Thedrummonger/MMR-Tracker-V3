using MMR_Tracker_V3.NetCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.NetCode.NetData;

namespace WebServer
{
    internal class ClientAuthentication
    {

        public static bool AuthenticateUser(NetData.ServerClient Client, ConfigFile serverConfig)
        {
            if (Client.PlayerID < 0) { Console.WriteLine($"{Client.PlayerID} was Invalid"); return false; }
            if (ServerThread.Clients.Any(x => x.Value.PlayerID == Client.PlayerID)) { Console.WriteLine($"A player is already connected with ID {Client.PlayerID}"); return false; }
            if (!ConnectionAllowed(Client, serverConfig)) { return false; }
            if (!UserLogin(Client, serverConfig)) { return false; }
            return true;
        }
        private static bool UserLogin(NetData.ServerClient Client, ConfigFile serverConfig)
        {
            if (!serverConfig.RequireLogin) { return true; }
            if (!serverConfig.UserLogins.TryGetValue(Client.PlayerID, out string? Password)) { Console.WriteLine($"Player {Client.PlayerID} was not entered in user list"); return false; }
            if (Password != Client.Handshake.Password) { Console.WriteLine($"Incorrect Password for Player {Client.PlayerID}"); return false; }
            return true;
        }
        private static bool ConnectionAllowed(NetData.ServerClient Client, ConfigFile serverConfig)
        {
            bool HasWhitelist = serverConfig.IPWhitelist is not null && serverConfig.IPWhitelist.Count != 0;
            bool HasBacklist = serverConfig.IPBlacklist is not null && serverConfig.IPBlacklist.Count != 0;
            var localAddress = Client.EndPoint?.Address;

            #pragma warning disable CS8602
            if (!HasBacklist && !HasWhitelist) { return true; };
            if (localAddress is null) { Console.WriteLine($"Client Connected with invalid IP (P{Client.PlayerID})"); return false; }
            if (HasWhitelist && !serverConfig.IPWhitelist.Contains(localAddress)) { Console.WriteLine($"{localAddress} was not whitelisted (P{Client.PlayerID})."); return false; }
            if (HasBacklist && serverConfig.IPBlacklist.Contains(localAddress)) { Console.WriteLine($"{localAddress} was blacklisted (P{Client.PlayerID})."); return false; }
            return true;
        }
    }
}
