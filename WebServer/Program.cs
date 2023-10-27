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
            server.Start();
        }
    }
    internal class AsyncServerTest
    {
        public class MMRTServerClient
        {
            public TcpClient NetClient;
            public NetworkStream NetworkStream;
            public IPEndPoint EndPoint;
        }
        public static Dictionary<Guid, MMRTServerClient> Clients = new Dictionary<Guid, MMRTServerClient>();
        public static void Main(string[] args)
        {
            test(IPAddress.Parse("127.0.0.1"), 25570);
        }

        public static void test(IPAddress IP, int Port)
        {
            var serverListenter = new TcpListener(IP, Port);
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
                        string DataToSend = $"Server Ping!";
                        byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(DataToSend);
                        i.NetworkStream.Write(bytesToSend, 0, bytesToSend.Length);
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
            Console.WriteLine($"Client {localAddress}:{localPort} Connected");
            _ServerClient.NetworkStream = _ServerClient.NetClient.GetStream();
            Clients.Add(guid, _ServerClient);
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[_ServerClient.NetClient.ReceiveBufferSize];
                    int bytesRead = await _ServerClient.NetworkStream.ReadAsync(buffer, 0, _ServerClient.NetClient.ReceiveBufferSize);
                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Client {localAddress}:{localPort} Sent Data {dataReceived}");
                    string DataToSend = $"I got [{dataReceived}]";
                    byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(DataToSend);
                    _ServerClient.NetworkStream.Write(bytesToSend, 0, bytesToSend.Length);
                }
                catch
                {
                    Console.WriteLine($"Client {localAddress}:{localPort} Disconnected");
                    Clients.Remove(guid);
                    break;
                }
            }
        }
    }
}