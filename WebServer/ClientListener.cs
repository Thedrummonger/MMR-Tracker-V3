using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.DataStructure.NetData;
using static WebServer.ServerThread;
using Newtonsoft.Json;
using MMR_Tracker_V3;

namespace WebServer
{
    internal class ClientListener
    {
        public static async void WaitForNewClient(TcpListener serverListener, ConfigFile serverConfig)
        {
            while (true)
            {
                Console.WriteLine($"Awaiting Client");
                TcpClient client = await serverListener.AcceptTcpClientAsync();
                #pragma warning disable CS4014
                //start a new thread to handle the newly connected client
                Task.Run(() => ProcessNewClient(client, serverConfig));
                #pragma warning restore CS4014
            }
        }

        public static void ProcessNewClient(TcpClient client, ConfigFile serverConfig)
        {
            var NewNetClient = ReadHandshakePacket(client, serverConfig);
            if (NewNetClient is null) { SendConnectionConfirmation(client, null, "Bad Handshake Packet"); return; }
            if (!ClientAuthentication.AuthenticateUser(NewNetClient, serverConfig)) { SendConnectionConfirmation(client, null, "Authentication Failed"); return; }
            if (!AddPlayerToClientList(NewNetClient)) { SendConnectionConfirmation(client, null, "Unknown Server error"); return; }

            string ConnectionSuccessMessage = $"Connected to web server\n" +
                $"PlayerID: {NewNetClient.PlayerID}\n" +
                $"ClientID: {NewNetClient.ClientID}\n" +
                $"Game Mode: {NewNetClient.ClientMode}";

            SendConnectionConfirmation(client, NewNetClient, ConnectionSuccessMessage);

            SendPlayerJoinedMessage(NewNetClient);

            ClientThread.StartClientLoop(NewNetClient);

            //When the client loop exits, the client has disconnected
            RemovePlayerToClientList(NewNetClient);
        }

        private static void SendPlayerJoinedMessage(ServerClient newNetClient)
        {
            var NotifyChatMessage = Guid.NewGuid();
            PlayerChat.Add(NotifyChatMessage, new ChatMessage(-1, NotifyChatMessage, $"Player {newNetClient.PlayerID} joined the server."));
            Utility.SendChatToClients(PlayerChat.Last().Value, Utility.GetPlayersExcept(newNetClient.PlayerID));
        }

        private static void SendConnectionConfirmation(TcpClient client, ServerClient? serverClient, string ConnectionStatus)
        {
            bool NoClient = serverClient is null;

            int PlayerID = NoClient ? -1 : serverClient.PlayerID;
            Guid ClientID = NoClient ? Guid.Empty : serverClient.ClientID;
            OnlineMode ClientMode = NoClient ? OnlineMode.None : serverClient.ClientMode;

            var confirmationPacket = new NetPacket(-1, PacketType.Handshake)
            {
                HandshakeResponse = new HandshakeResponse
                {
                    PlayerID = PlayerID,
                    ClientID = ClientID,
                    ClientMode = ClientMode,
                    ConnectionStatus = ConnectionStatus,
                    ConnectionSuccess = !NoClient,
                    ConnectedPlayers = NoClient ? [] : Clients.Where(x => x.Key != ClientID).Select(x => x.Value.PlayerID).ToArray(),
                }
            };

            string DataToSend = confirmationPacket.ToFormattedJson();
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(DataToSend);
            client.GetStream().Write(bytesToSend, 0, bytesToSend.Length);

        }

        public static bool AddPlayerToClientList(ServerClient client)
        {
            if (Clients.ContainsKey(client.ClientID))
            {
                Console.WriteLine($"Client UUID {client.ClientID} already existed, this should not have happened!");
                return false;
            }
            Clients.Add(client.ClientID, client);
            return true;
        }

        public static bool RemovePlayerToClientList(ServerClient client)
        {
            Console.WriteLine($"Client {client.GetIP()}:{client.GetPort()} Disconnected");
            if (client.NetClient.Connected) { client.NetClient.Close(); }
            if (!Clients.ContainsKey(client.ClientID)) { throw new Exception($"Could not remove player {client.PlayerID} [{client.ClientID}] from client list. Something bad happened!"); }
            Clients.Remove(client.ClientID);
            return true;
        }

        public static ServerClient? ReadHandshakePacket(TcpClient client, ConfigFile serverConfig)
        {
            byte[] HandshakeBuffer = new byte[client.ReceiveBufferSize];
            int HandshakeBytes = client.GetStream().Read(HandshakeBuffer, 0, client.ReceiveBufferSize);
            string HandShake = Encoding.ASCII.GetString(HandshakeBuffer, 0, HandshakeBytes);

            ServerClient _ServerClient = new()
            {
                NetClient = client,
                ClientMode = serverConfig.ServerGameMode,
                ClientID = Guid.NewGuid(),
                EndPoint = client.Client.RemoteEndPoint as IPEndPoint
            };
            try
            {
                _ServerClient.Handshake = JsonConvert.DeserializeObject<NetPacket>(HandShake);
                if (_ServerClient.Handshake is null || _ServerClient.Handshake.packetType != PacketType.Handshake)
                {
                    Console.WriteLine($"{_ServerClient.GetIP()} Connection packet was not handshake");
                    return null;
                }
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
    }
}
