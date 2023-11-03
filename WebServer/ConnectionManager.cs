using MathNet.Numerics.Statistics;
using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.NetData;
using static WebServer.AsyncServerTest;

namespace WebServer
{
    internal class ConnectionManager
    {
        public static async void WaitForNewClient(TcpListener serverListenter, NetData.ConfigFile serverConfig)
        {
            while (true)
            {
                Console.WriteLine($"Awaiting Client");
                TcpClient client = await serverListenter.AcceptTcpClientAsync();
                #pragma warning disable CS4014
                Task.Run(() => ProcessNewClient(client, serverConfig));
                #pragma warning restore CS4014
            }
        }

        public static void ProcessNewClient(TcpClient client, NetData.ConfigFile serverConfig)
        {
            var NewNetClient = ReadHandshakePacket(client, serverConfig);
            if (NewNetClient is null) { SendConnectionConfirmation(client, null, "Bad Handshake Packet"); return; }
            if (!AuthenticateUser(NewNetClient, serverConfig)) { SendConnectionConfirmation(client, null, "Authentication Failed"); return; }
            if (!AddPlayerToClientList(NewNetClient)) { SendConnectionConfirmation(client, null, "Unknown Server error"); return; }

            string ConnectionSuccessMessage = $"Connected to web server\n" +
                $"PlayerID: {NewNetClient.PlayerID}\n" +
                $"ClientID: {NewNetClient.ClientID}\n" +
                $"Game Mode: {NewNetClient.ClientMode}";

            SendConnectionConfirmation(client, NewNetClient, ConnectionSuccessMessage);

            SendPlayerJoinedMessasge(NewNetClient);

            StartClientLoop(NewNetClient);

            RemovePlayerToClientList(NewNetClient);
        }

        private static void SendPlayerJoinedMessasge(ServerClient newNetClient)
        {
            var NotifyChatMessage = Guid.NewGuid();
            PlayerChat.Add(NotifyChatMessage, new NetData.ChatMessage(-1, NotifyChatMessage, $"Player {newNetClient.PlayerID} joined the server."));
            ConnectionManager.UpdateClients(new NetData.NetPacket(-1, NetData.PacketType.ChatMessage) { ChatMessage = PlayerChat.Last().Value }, Clients.Values.Where(x => x.PlayerID != newNetClient.PlayerID));
        }

        private static void SendConnectionConfirmation(TcpClient client, ServerClient? serverClient, string ConnectionStatus)
        {
            bool NoClient = serverClient is null;

            int PlayerID = NoClient ? -1 : serverClient.PlayerID;
            Guid ClientID = NoClient ? Guid.Empty : serverClient.ClientID;
            NetData.OnlineMode ClientMode = NoClient ? NetData.OnlineMode.None: serverClient.ClientMode;

            var confirmationPacket = new NetPacket(PlayerID, PacketType.Handshake);

            confirmationPacket.HandshakeResponse = new HandshakeResponse 
            { 
                PlayerID = PlayerID, 
                ClientID = ClientID, 
                ClientMode = ClientMode, 
                ConnectionStatus = ConnectionStatus, 
                ConnectionSuccess = !NoClient,
                ConnectedPlayers = NoClient ? Array.Empty<int>() : Clients.Where(x => x.Key != ClientID).Select(x => x.Value.PlayerID).ToArray(),
            };

            string DataToSend = confirmationPacket.ToFormattedJson();
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(DataToSend);
            client.GetStream().Write(bytesToSend, 0, bytesToSend.Length);

        }

        public static void StartClientLoop(NetData.ServerClient _ServerClient)
        {
            _ServerClient.EndPoint = _ServerClient.NetClient.Client.RemoteEndPoint as IPEndPoint;
            Console.WriteLine($"Player {_ServerClient.PlayerID} Client thread started");
            while (_ServerClient.NetClient.GetStream().CanRead && _ServerClient.NetClient.Connected)
            {
                string Message = ListenForMessage(_ServerClient);
                if (string.IsNullOrWhiteSpace(Message)) { break; } //Message is null if the read failes 
                NetPacket? packet = ParsePacket(Message, _ServerClient);
                if (packet is null) { continue; }

                Console.WriteLine($"Got {packet.packetType} Packet from player {_ServerClient.PlayerID} on {_ServerClient.GetIP()}:{_ServerClient.GetPort()}");

                if (packet.packetType == PacketType.ChatMessage)
                {
                    Console.WriteLine($"P{packet.PlayerID}: {packet.ChatMessage.Message}");
                }

                UpdateClients(packet, GetPlayersToUpdate(packet));

            }
        }

        public static IEnumerable<ServerClient> GetPlayersToUpdate(NetPacket ServerPacket)
        {
            return Clients.Values.Where(x => 
            x.PlayerID != ServerPacket.PlayerID && 
            (ServerPacket.UpdateWhitelist is null || ServerPacket.UpdateWhitelist.Contains(x.PlayerID)));
        }

        public static void UpdateClients(NetPacket packet, IEnumerable<ServerClient> _PlayerToUpdate)
        {
            Dictionary<Guid, Dictionary<int, Dictionary<string, int>>> PlayerMultiworldItemData = new Dictionary<Guid, Dictionary<int, Dictionary<string, int>>>();
            NetPacket Update = new NetPacket(-1, packet.packetType);
            switch (packet.packetType)
            {
                case PacketType.ChatMessage:
                    var MostRecentchat = PlayerChat.Last().Value;
                    Update.ChatMessage = MostRecentchat;
                    break;
                case PacketType.OnlineSynedLocations:
                    Update.LocationData = AsyncServerTest.GetCheckedLocations();
                    break;
                case PacketType.MultiWorldItems:
                    foreach(var k in Clients) { PlayerMultiworldItemData[k.Key] = AsyncServerTest.GetItemsBelongingToPlayer(k.Value.PlayerID); }
                    break;
                default:
                    return;
            };
            string PacketString = Update.ToFormattedJson();
            foreach (var Client in _PlayerToUpdate)
            {
                if (packet.packetType == PacketType.MultiWorldItems) 
                {
                    Update.ItemData = PlayerMultiworldItemData[Client.ClientID];
                    PacketString = Update.ToFormattedJson();
                }
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(PacketString);
                Client.NetClient.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
            }
        }

        public static NetPacket? ParsePacket(string dataReceived, ServerClient _ServerClient)
        {
            NetPacket Packet;
            try
            {
                Packet = JsonConvert.DeserializeObject<NetData.NetPacket>(dataReceived);
                if (Packet.PlayerID != _ServerClient.PlayerID)
                {
                    Console.WriteLine($"Packet for player {Packet.PlayerID} recieved on player {_ServerClient.PlayerID} client thread");
                    return null;
                }
                switch (Packet.packetType)
                {
                    case PacketType.ChatMessage: 
                        PlayerChat.Add(Packet.ChatMessage.guid, Packet.ChatMessage);
                        break;
                    case PacketType.OnlineSynedLocations:
                        _ServerClient.OnlineLocationData = Packet.LocationData;
                        break;
                    case PacketType.MultiWorldItems:
                        _ServerClient.MultiworldItemData = Packet.ItemData;
                        break;
                    default:
                        return null;
                };
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to parse Packet from {_ServerClient.PlayerID} {e.Message}\n{dataReceived}");
                return null;
            }
            return Packet;
        }

        public static string ListenForMessage(NetData.ServerClient _ServerClient)
        {
            string dataReceived;
            try
            {
                byte[] buffer = new byte[_ServerClient.NetClient.ReceiveBufferSize];
                int bytesRead = _ServerClient.NetClient.GetStream().Read(buffer, 0, _ServerClient.NetClient.ReceiveBufferSize);
                dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            }
            catch
            {
                return null;
            }
            return dataReceived;
        }

        public static NetData.ServerClient? ReadHandshakePacket(TcpClient client, NetData.ConfigFile serverConfig)
        {
            byte[] HandshakeBuffer = new byte[client.ReceiveBufferSize];
            int HandshakeBytes = client.GetStream().Read(HandshakeBuffer, 0, client.ReceiveBufferSize);
            string HandShake = Encoding.ASCII.GetString(HandshakeBuffer, 0, HandshakeBytes);

            NetData.ServerClient _ServerClient = new NetData.ServerClient() 
            { 
                NetClient = client, 
                ClientMode = serverConfig.ServerGameMode, 
                ClientID = Guid.NewGuid(),
                EndPoint = client.Client.RemoteEndPoint as IPEndPoint
            };
            try
            {
                _ServerClient.Handshake = JsonConvert.DeserializeObject<NetData.NetPacket>(HandShake);
                if (_ServerClient.Handshake.packetType != PacketType.Handshake)
                {
                    Console.WriteLine($"{_ServerClient.GetIP()} Connection packet was not handshake"); 
                    return null; }
                _ServerClient.PlayerID = _ServerClient.Handshake.PlayerID;
                Console.WriteLine($"Player {_ServerClient.PlayerID} Connect from {_ServerClient.GetIP()}:{_ServerClient.GetPort()}");
            }
            catch
            {
                Console.WriteLine("Failed to parse Handshake Packet");
                return null;
            }
            return _ServerClient;
        }

        public static bool AddPlayerToClientList(NetData.ServerClient client)
        {
            if (Clients.ContainsKey(client.ClientID)) 
            {
                Console.WriteLine($"Client UUID {client.ClientID} already existed, this should not have happened!");
                return false; 
            }
            Clients.Add(client.ClientID, client);
            return true;
        }

        public static bool RemovePlayerToClientList(NetData.ServerClient client)
        {
            Console.WriteLine($"Client {client.GetIP()}:{client.GetPort()} Disconnected");
            if (client.NetClient.Connected) { client.NetClient.Close(); }
            if (!Clients.ContainsKey(client.ClientID)) { throw new Exception($"Could not remove player {client.PlayerID} [{client.ClientID}] from client list. Something bad happend!"); }
            Clients.Remove(client.ClientID);
            return true;
        }

        private static bool AuthenticateUser(NetData.ServerClient Client, ConfigFile serverConfig)
        {
            if (Client.PlayerID < 0) { Console.WriteLine($"{Client.PlayerID} was Invalid"); return false; }
            if (Clients.Any(x => x.Value.PlayerID == Client.PlayerID)) { Console.WriteLine($"A player is already connected with ID {Client.PlayerID}"); return false; }
            if (!ConnectionAllowed(Client, serverConfig)) { return false; }
            if (!UserLogin(Client, serverConfig)) { return false; }
            return true;
        }
        private static bool UserLogin(NetData.ServerClient Client, ConfigFile serverConfig)
        {
            if (!serverConfig.RequireLogin) { return true; }
            if (!serverConfig.UserLogins.ContainsKey(Client.PlayerID)) { Console.WriteLine($"Player {Client.PlayerID} was not entered in user list"); return false; }
            if (serverConfig.UserLogins[Client.PlayerID] != Client.Handshake.Password) { Console.WriteLine($"Incorrect Password for Player {Client.PlayerID}"); return false; }
            return true;
        }
        private static bool ConnectionAllowed(NetData.ServerClient Client, ConfigFile serverConfig)
        {
            bool HasWhitelist = serverConfig.IPWhitelist is not null && serverConfig.IPWhitelist.Any();
            bool HasBalcklist = serverConfig.IPBlacklist is not null && serverConfig.IPBlacklist.Any();
            var localAddress = Client.EndPoint?.Address;

            #pragma warning disable CS8602
            if (!HasBalcklist && !HasWhitelist) { return true; };
            if (localAddress is null) { Console.WriteLine($"Client Connected with invalid IP (P{Client.PlayerID})"); return false; }
            if (HasWhitelist && !serverConfig.IPWhitelist.Contains(localAddress)) { Console.WriteLine($"{localAddress} was not whitelisted (P{Client.PlayerID})."); return false; }
            if (HasBalcklist && serverConfig.IPBlacklist.Contains(localAddress)) { Console.WriteLine($"{localAddress} was blacklisted (P{Client.PlayerID})."); return false; }
            return true;
        }
    }
}
