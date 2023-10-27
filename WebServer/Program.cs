using MathNet.Symbolics;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MMR_Tracker_V3.TrackerObjects;
using FParsec;
using MMR_Tracker_V3;

namespace WebServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //The the config file values
            NetData.ConfigFile.VerifyConfig();
            NetData.ConfigFile cfg = JsonConvert.DeserializeObject<NetData.ConfigFile>(File.ReadAllText(NetData.ConfigFile.ConfigFilePath), MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions)??new NetData.ConfigFile().SetDefaultExamples();

            //If the program was started with arguments, scan then for an IP and/or Port and override the config if found
            NetData.ParseNetServerArgs(args, out IPAddress? ArgsIP, out int ArgsPort);
            if (ArgsIP is not null) { cfg.IPAddress = ArgsIP; }
            if (ArgsPort > -1) { cfg.Port = ArgsPort; }

            //Sanitize the IPAdress and Port
            if (cfg.IPAddress is null || cfg.IPAddress.ToString() == "0.0.0.0") { cfg.IPAddress = IPAddress.Any; }
            if (cfg.Port < 0) { cfg.Port = NetData.DefaultProgramPort; }

            //Create and start the server
            HttpServer server = new HttpServer(cfg);
            //server.Start();

            AsyncServerTest.test(cfg);
        }
    }
    internal class AsyncServerTest
    {
        public class MMRTServerClient
        {
            public TcpClient NetClient;
            public IPEndPoint EndPoint;
            public int PlayerID;
            public Dictionary<int, Dictionary<string, int>> ItemData = new Dictionary<int, Dictionary<string, int>>();
        }
        public static Dictionary<Guid, MMRTServerClient> Clients = new Dictionary<Guid, MMRTServerClient>();

        public static NetData.ConfigFile serverConfig;

        public static void test(NetData.ConfigFile cfg)
        {
            serverConfig = cfg;
            var serverListenter = new TcpListener(cfg.IPAddress, cfg.Port);
            serverListenter.Start();

            WaitForNewClient(serverListenter);

            while (true)
            {
                Console.WriteLine($"Do Other Program Stuff");
                string Stuff = Console.ReadLine();
                if (Stuff == "send")
                {
                    foreach (var i in Clients.Values)
                    {
                        string DataToSend = $"Server Data:\n{Clients.ToDictionary(x => x.Value.PlayerID, x => x.Value.ItemData).ToFormattedJson()}";
                        byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(DataToSend);
                        i.NetClient.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
                    }
                }
                else if (Stuff == "Clients")
                {
                    Console.WriteLine("Connected Clients:\n" + string.Join("\n", Clients.Select(x => $"{x.Key}|{x.Value.EndPoint?.Address}")));
                }
                else
                {
                    Console.WriteLine($"You ran Manual command {Stuff}");
                }
            }

        }

        public static async void WaitForNewClient(TcpListener serverListenter)
        {
            while (true)
            {
                Console.WriteLine($"Awaiting Client");
                TcpClient client = await serverListenter.AcceptTcpClientAsync();
                StartClientLoop(new MMRTServerClient() { NetClient = client });
            }
        }
        public static async void StartClientLoop(MMRTServerClient _ServerClient)
        {
            Guid guid = Guid.NewGuid();
            _ServerClient.EndPoint = _ServerClient.NetClient.Client.RemoteEndPoint as IPEndPoint;
            var localAddress = _ServerClient.EndPoint?.Address;
            var localPort = _ServerClient.EndPoint?.Port;
            try
            {
                byte[] HandshakeBuffer = new byte[_ServerClient.NetClient.ReceiveBufferSize];
                int HandshakeBytes = _ServerClient.NetClient.GetStream().Read(HandshakeBuffer, 0, _ServerClient.NetClient.ReceiveBufferSize);
                string HandShake = Encoding.ASCII.GetString(HandshakeBuffer, 0, HandshakeBytes);
                NetData.NetPacket HandshakePacket = JsonConvert.DeserializeObject<NetData.NetPacket>(HandShake);
                if (!AuthenticateUser(HandshakePacket)) { return; }
                _ServerClient.PlayerID = HandshakePacket.PlayerID;
            }
            catch
            {
                Console.WriteLine("Failed to parse Handshake Packet");
                return;
            }

            Console.WriteLine($"Player {_ServerClient.PlayerID} Connect from {localAddress}:{localPort}");
            Clients.Add(guid, _ServerClient);
            while (true)
            {
                string dataReceived;
                try
                {
                    byte[] buffer = new byte[_ServerClient.NetClient.ReceiveBufferSize];
                    int bytesRead = await _ServerClient.NetClient.GetStream().ReadAsync(buffer, 0, _ServerClient.NetClient.ReceiveBufferSize);
                    dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                }
                catch
                {
                    Console.WriteLine($"Client {localAddress}:{localPort} Disconnected");
                    Clients.Remove(guid);
                    break;
                }

                try
                {
                    NetData.NetPacket Packet = JsonConvert.DeserializeObject<NetData.NetPacket>(dataReceived);
                    if (Packet.PlayerID != _ServerClient.PlayerID) 
                    {
                        Console.WriteLine($"Packet for player {Packet.PlayerID} recieved on player {_ServerClient.PlayerID} client thread");
                        continue;
                    }
                    Console.WriteLine($"Recieved Packet from player {Packet.PlayerID}");
                    _ServerClient.ItemData = Packet.ItemData;
                }
                catch
                {
                    Console.WriteLine($"Failed to parse Packet from {_ServerClient.PlayerID}");
                }
            }
        }

        private static bool AuthenticateUser(NetData.NetPacket mMRTPacket)
        {
            if (mMRTPacket.PlayerID < 0 || Clients.Any(x => x.Value.PlayerID == mMRTPacket.PlayerID)) { Console.WriteLine($"{mMRTPacket.PlayerID} was Invalid"); return false; }
            if (!serverConfig.PlayersRequirePassword) { return true; }
            if (!serverConfig.playerPasswords.ContainsKey(mMRTPacket.PlayerID)) { Console.WriteLine($"Player {mMRTPacket.PlayerID} was not entered in user list"); return false; }
            if (serverConfig.playerPasswords[mMRTPacket.PlayerID] != mMRTPacket.Password) { Console.WriteLine($"Incorrect Password for Player {mMRTPacket.PlayerID}"); return false; }
            return true;
        }

        private static NetData.NetPacket GetItemsBelongingToPlayer(int PlayerID)
        {
            NetData.NetPacket ReturnPacket = new NetData.NetPacket(PlayerID, _ItemData: new Dictionary<int, Dictionary<string, int>>());
            foreach (var i in Clients)
            {
                if (i.Value.PlayerID == PlayerID) { continue; }
                if (i.Value.ItemData.ContainsKey(PlayerID))
                {
                    ReturnPacket.ItemData.Add(i.Value.PlayerID, i.Value.ItemData[PlayerID]);
                }
                if (i.Value.ItemData.ContainsKey(-1))
                {
                    ReturnPacket.ItemData.Add(-1, i.Value.ItemData[PlayerID]);
                }
            }
            return ReturnPacket;
        }
    }
}