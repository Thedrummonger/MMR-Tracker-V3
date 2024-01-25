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
using FParsec;
using MMR_Tracker_V3;
using MathNet.Numerics.Statistics;
using MMR_Tracker_V3.TrackerObjects;

namespace WebServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //The the config file values
            NetData.ConfigFile.VerifyConfig();
            NetData.ConfigFile cfg = JsonConvert.DeserializeObject<NetData.ConfigFile>(File.ReadAllText(NetData.ConfigFile.ConfigFilePath), MMR_Tracker_V3.Utility.DefaultSerializerSettings)??new NetData.ConfigFile().SetDefaultExamples();

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

            //Sanitize the IPAddress and Port
            if (cfg.IPAddress is null || cfg.IPAddress.ToString() == "0.0.0.0") { cfg.IPAddress = IPAddress.Any; }
            if (cfg.Port < 0) { cfg.Port = NetData.DefaultProgramPort; }

            //Create and start the server
            ServerThread.StartServer(cfg);
        }
    }
    internal class ServerThread
    {
        public static Dictionary<Guid, NetData.ServerClient> Clients = [];
        public static Dictionary<Guid, NetData.ChatMessage> PlayerChat = [];
        public static void StartServer(NetData.ConfigFile cfg)
        {
            var serverListener = new TcpListener(cfg.IPAddress, cfg.Port);
            serverListener.Start();

            string IpString = cfg.IPAddress.ToString() == "0.0.0.0" ? "*:" : $"IP {cfg.IPAddress}:";
            Console.WriteLine($"Server active on {IpString}{cfg.Port}");

            //Start a new thread to listen for new clients
            ClientListener.WaitForNewClient(serverListener, cfg);

            //Main loop for server commands
            while (true)
            {
                string command = Console.ReadLine()?.ToLower()??String.Empty;
                if (command == "clients")
                {
                    Console.WriteLine($"Connected {cfg.ServerGameMode} Clients:\n" + string.Join("\n", Clients.Select(x => $"{x.Key}|{x.Value.EndPoint?.Address}")));
                }
                else if (command == "debug")
                {
                    Console.WriteLine(Clients.ToFormattedJson());
                }
                else if (!String.IsNullOrWhiteSpace(command))
                {
                    var ServerChatGuid = Guid.NewGuid();
                    PlayerChat.Add(ServerChatGuid, new NetData.ChatMessage(-1, ServerChatGuid, command));
                    Utility.SendChatToClients(PlayerChat.Last().Value, Utility.GetPlayersExcept());
                }
            }
        }

    }
}