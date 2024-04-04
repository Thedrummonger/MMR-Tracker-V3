using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.NetCode.NetData;

namespace WebServer
{
    internal class ClientThread
    {

        public static void StartClientLoop(ServerClient _ServerClient)
        {
            _ServerClient.EndPoint = _ServerClient.NetClient.Client.RemoteEndPoint as IPEndPoint;
            Console.WriteLine($"Player {_ServerClient.PlayerID} Client thread started");
            while (_ServerClient.NetClient.GetStream().CanRead && _ServerClient.NetClient.Connected)
            {
                string Message = ListenForMessage(_ServerClient);
                if (string.IsNullOrWhiteSpace(Message)) { break; } //Message is null if the read fails 
                NetPacket? packet = ParsePacket(Message, _ServerClient);
                if (packet is null) { continue; }

                Console.WriteLine($"Got {packet.packetType} Packet from player {_ServerClient.PlayerID} on {_ServerClient.GetIP()}:{_ServerClient.GetPort()}");
                SendClientResponse(_ServerClient, packet);
            }
        }

        public static string? ListenForMessage(ServerClient _ServerClient)
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

        public static NetPacket? ParsePacket(string dataReceived, ServerClient _ServerClient)
        {
            NetPacket Packet;
            try
            {
                Packet = JsonConvert.DeserializeObject<NetPacket>(dataReceived);
                if (Packet is null)
                {
                    Console.WriteLine($"Failed to parse Packet from {_ServerClient.PlayerID}\n{dataReceived}");
                    return null;
                }
                if (Packet.PlayerID != _ServerClient.PlayerID)
                {
                    Console.WriteLine($"Packet for player {Packet.PlayerID} received on player {_ServerClient.PlayerID} client thread");
                    return null;
                }
                switch (Packet.packetType)
                {
                    case PacketType.ChatMessage:
                        ServerThread.PlayerChat.Add(Packet.ChatMessage.guid, Packet.ChatMessage);
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

        public static void SendClientResponse(ServerClient _ServerClient, NetPacket packet)
        {
            HashSet<Guid> PlayersToUpdate = Utility.GetPlayersExcept(_ServerClient.PlayerID);
            if (packet.UpdateWhitelist is not null)
            {
                PlayersToUpdate = Utility.GetPlayers(packet.UpdateWhitelist);
            }

            switch (packet.packetType)
            {
                case PacketType.OnlineSynedLocations:
                    Utility.SendCheckedLocationsToClients(PlayersToUpdate, Utility.GetPlayers(_ServerClient.PlayerID));
                    break;
                case PacketType.MultiWorldItems:
                    Utility.SendMultiworldItemsToClients(PlayersToUpdate, Utility.GetPlayers(_ServerClient.PlayerID));
                    break;
                case PacketType.ChatMessage:
                    Console.WriteLine($"P{packet.PlayerID}: {packet.ChatMessage.Message}");
                    Utility.SendChatToClients(packet.ChatMessage, PlayersToUpdate);
                    break;
            }
        }
    }
}
