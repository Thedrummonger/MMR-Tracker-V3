using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    public class HttpServer
    {
        private readonly TcpListener serverListenter;
        private readonly NetData.ConfigFile serverConfig;

        public HttpServer(NetData.ConfigFile serverConfig)
        {
            this.serverListenter = new TcpListener(serverConfig.IPAddress, serverConfig.Port);
            this.serverConfig = serverConfig;
        }

        public void Start()
        {
            while (true)
            {
                string Status = serverConfig.IPAddress == IPAddress.Any ? "Port " : $"{serverConfig.IPAddress}:";
                Debug.WriteLine($"Server started on {serverConfig.IPAddress}:{serverConfig.Port}.");
                Console.WriteLine($"Server started on {Status}{serverConfig.Port}.");
                serverListenter.Start();

                //---incoming client connected---
                TcpClient client = serverListenter.AcceptTcpClient();
                var localEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                var localAddress = localEndPoint?.Address;
                var localPort = localEndPoint?.Port;
                Console.WriteLine($"Connection Established\n{localAddress??IPAddress.None}:{localPort??0}");

                var ConnectionResult = ConnectionAllowed(localEndPoint);
                if (ConnectionResult == NetData.ConnectionResult.Success)
                {
                    ParseRequest(client);
                }
                else
                {
                    Console.WriteLine($"Server Refused Connection from {localAddress}. {ConnectionResult}");
                }

                client.Close();
                serverListenter.Stop();
            }
        }

        private NetData.ConnectionResult ConnectionAllowed(IPEndPoint? localEndPoint)
        {
            var localAddress = localEndPoint?.Address;
            if (localAddress is null) { return NetData.ConnectionResult.Unknown; }
            if (serverConfig.IPWhitelist.Any() && !serverConfig.IPWhitelist.Contains(localAddress)) { return NetData.ConnectionResult.NonWhitelisted; }
            if (serverConfig.IPBlacklist.Any() && serverConfig.IPBlacklist.Contains(localAddress)) { return NetData.ConnectionResult.Blacklisted; }
            return NetData.ConnectionResult.Success;
        }

        public static string TestCommand(string Command)
        {
            switch (Command.ToLower())
            {
                case "check": return "Checking Item";
                case "mark": return "Marking Item";
                case "uncheck": return "Unchecking Item";
            }
            return "Unknown Command!";
        }

        public void ParseRequest(TcpClient client)
        {
            try
            {
                //---get the incoming data through a network stream---
                NetworkStream nwStream = client.GetStream();
                nwStream.ReadTimeout = 1000;
                byte[] buffer = new byte[client.ReceiveBufferSize];

                //---read incoming stream---
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
                Console.WriteLine($"BytesRead {bytesRead}");

                //---convert the data received into a string---
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received data, parsing...");

                string DataToSend = ParseNetPacket(dataReceived);

                if (DataToSend is not null)
                {
                    byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(DataToSend);
                    //---write back the text to the client---
                    Console.WriteLine($"Sending return data.");
                    nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                }
                else
                {
                    Console.WriteLine($"Recieved Bad Packet");
                }
            }
            catch (Exception e) { Console.WriteLine(e); }
        }

        public string? ParseNetPacket(string dataReceived)
        {
            NetData.NetPacket MMRTPacket;
            try { MMRTPacket = JsonConvert.DeserializeObject<NetData.NetPacket>(dataReceived); }
            catch { return ReturnErrorPacket($"Could not Parse packet"); }
            if (MMRTPacket is null) { return ReturnErrorPacket($"Could not Parse packet"); }

            if (!AuthenticateUser(MMRTPacket)) { return ReturnErrorPacket("Authentication Failed"); }
            ReadAndStoreitemData(MMRTPacket);
            return GetReturnData(MMRTPacket);
        }
        private string? ReturnErrorPacket(string Error)
        {
            Console.WriteLine(Error);
            var ReturnPacket = new NetData.NetPacket(-1);
            ReturnPacket.TestString = Error;
            return JsonConvert.SerializeObject(ReturnPacket);
        }

        private string? GetReturnData(NetData.NetPacket mMRTPacket)
        {
            //Get the items that other players have found and return it to the player
            var ReturnPacket = new NetData.NetPacket(mMRTPacket.PlayerID);
            ReturnPacket.TestString = mMRTPacket.TestString;
            return JsonConvert.SerializeObject(ReturnPacket);
        }

        private bool AuthenticateUser(NetData.NetPacket mMRTPacket)
        {
            if (!serverConfig.PlayersRequirePassword) { return true; }
            if (!serverConfig.playerPasswords.ContainsKey(mMRTPacket.PlayerID)) { Console.WriteLine($"Player {mMRTPacket.PlayerID} was not entered in user list"); return false; }
            if (serverConfig.playerPasswords[mMRTPacket.PlayerID] != mMRTPacket.Password) { Console.WriteLine($"Incorrect Password for Player {mMRTPacket.PlayerID}"); return false; }
            return true;
        }

        private void ReadAndStoreitemData(NetData.NetPacket mMRTPacket)
        {
            //Store The users sent items to the server database
        }
    }
}
