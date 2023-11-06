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
using MathNet.Numerics.Statistics;

namespace WebServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //The the config file values
            NetData.ConfigFile.VerifyConfig();
            NetData.ConfigFile cfg = JsonConvert.DeserializeObject<NetData.ConfigFile>(File.ReadAllText(NetData.ConfigFile.ConfigFilePath), MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions)??new NetData.ConfigFile().SetDefaultExamples();

            if (cfg.ServerGameMode == NetData.OnlineMode.None)
            {
                Console.WriteLine("Select Game Mode");
                Console.WriteLine("1. Co-Op");
                Console.WriteLine("2. MultiWorld");
                selectGM:
                var gmSelect = Console.ReadKey();
                switch (gmSelect.Key)
                {
                    case ConsoleKey.NumPad1:
                    case ConsoleKey.D1:
                        cfg.ServerGameMode = NetData.OnlineMode.Coop; break;
                    case ConsoleKey.NumPad2:
                    case ConsoleKey.D2:
                        cfg.ServerGameMode = NetData.OnlineMode.Multiworld; break;
                    default:
                        goto selectGM;
                }
                Console.Clear();
            }

            Console.WriteLine($"Game mode set to {cfg.ServerGameMode}");

            //If the program was started with arguments, scan then for an IP and/or Port and override the config if found
            NetData.ParseNetServerArgs(args, out IPAddress? ArgsIP, out int ArgsPort);
            if (ArgsIP is not null) { cfg.IPAddress = ArgsIP; }
            if (ArgsPort > -1) { cfg.Port = ArgsPort; }

            //Sanitize the IPAdress and Port
            if (cfg.IPAddress is null || cfg.IPAddress.ToString() == "0.0.0.0") { cfg.IPAddress = IPAddress.Any; }
            if (cfg.Port < 0) { cfg.Port = NetData.DefaultProgramPort; }

            //Create and start the server
            AsyncServerTest.test(cfg);
        }
    }
    internal class AsyncServerTest
    {
        public static Dictionary<Guid, NetData.ServerClient> Clients = new Dictionary<Guid, NetData.ServerClient>();
        public static Dictionary<Guid, NetData.ChatMessage> PlayerChat = new Dictionary<Guid, NetData.ChatMessage>();
        public static void test(NetData.ConfigFile cfg)
        {
            var serverListenter = new TcpListener(cfg.IPAddress, cfg.Port);
            serverListenter.Start();

            string IpString = cfg.IPAddress.ToString() == "0.0.0.0" ? "*:" : $"IP {cfg.IPAddress}:";
            Console.WriteLine($"Server active on {IpString}{cfg.Port}");

            ConnectionManager.WaitForNewClient(serverListenter, cfg);

            while (true)
            {
                string command = Console.ReadLine().ToLower();
                if (command == "clients")
                {
                    Console.WriteLine($"Connected {cfg.ServerGameMode} Clients:\n" + string.Join("\n", Clients.Select(x => $"{x.Key}|{x.Value.EndPoint?.Address}")));
                }
                else if (Stuff == "debug")
                {
                    Console.WriteLine(Clients.ToFormattedJson());
                }
                else
                {
                    var ServerChatGuid = Guid.NewGuid();
                    PlayerChat.Add(ServerChatGuid, new NetData.ChatMessage(-1, ServerChatGuid, Stuff));
                    ConnectionManager.UpdateClients(new NetData.NetPacket(-1, NetData.PacketType.ChatMessage) { ChatMessage = PlayerChat.Last().Value });
                }
            }
        }

        public static Dictionary<int, Dictionary<string, int>> GetItemsBelongingToPlayer(int PlayerID)
        {
            Dictionary<int, Dictionary<string, int>> MultiworldItemsForPlayer = new();
            foreach (var i in Clients)
            {
                if (i.Value.PlayerID == PlayerID) { continue; }
                if (i.Value.MultiworldItemData.ContainsKey(PlayerID))
                {
                    MultiworldItemsForPlayer.Add(i.Value.PlayerID, i.Value.MultiworldItemData[PlayerID]);
                }
                if (i.Value.MultiworldItemData.ContainsKey(-1))
                {
                    MultiworldItemsForPlayer.Add(-1, i.Value.MultiworldItemData[PlayerID]);
                }
            }
            return MultiworldItemsForPlayer;
        }
        public static Dictionary<string, string> GetCheckedLocations()
        {
            Dictionary<string, string> AllCheckedLocations = new Dictionary<string, string>();
            foreach (var c in Clients)
            {
                foreach(var l in c.Value.OnlineLocationData)
                {
                    AllCheckedLocations[l.Key] = l.Value;
                }
            }
            return AllCheckedLocations;
        }

    }
}