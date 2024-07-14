using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using TDMUtils;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;

namespace MMR_Tracker_V3.NetCode
{
    public class ConnectionHandling
    {
        public static void CloseServer(InstanceContainer InstanceContainer)
        {
            InstanceContainer.netConnection.CloseAndReset();
        }
        public static bool ConnectToWebServer(InstanceContainer InstanceContainer, string Address, int Port, int PlayerID, string Password, NetData.OnlineMode ClientMode, out List<string> Log)
        {
            List<string> ConnectionLog = [];
            try
            {
                InstanceContainer.netConnection.ServerConnection = new TcpClient(Address, Port);
                InstanceContainer.netConnection.ServerConnection.LingerState = new LingerOption(true, 0);
                NetData.NetPacket HandshakePacket = new NetData.NetPacket(PlayerID, NetData.PacketType.Handshake, Password, Mode: ClientMode);
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(HandshakePacket.ToFormattedJson());
                InstanceContainer.netConnection.ServerConnection.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
                byte[] buffer = new byte[InstanceContainer.netConnection.ServerConnection.ReceiveBufferSize];
                int bytesRead = InstanceContainer.netConnection.ServerConnection.GetStream().Read(buffer, 0, InstanceContainer.netConnection.ServerConnection.ReceiveBufferSize);
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                var ConfirmationHandshake = JsonConvert.DeserializeObject<NetData.NetPacket>(dataReceived);
                if (!ConfirmationHandshake.HandshakeResponse.ConnectionSuccess)
                {
                    ConnectionLog.Add("Failed to connect to server!");
                    CloseServer(InstanceContainer);
                    ConnectionLog.AddRange(ConfirmationHandshake.HandshakeResponse.ConnectionStatus.Split('\n'));
                    Log = ConnectionLog;
                    return false;
                }
                ConnectionLog.AddRange(ConfirmationHandshake.HandshakeResponse.ConnectionStatus.Split('\n'));
                InstanceContainer.netConnection.OnlineMode = ConfirmationHandshake.HandshakeResponse.ClientMode;
                InstanceContainer.netConnection.PlayerID = ConfirmationHandshake.HandshakeResponse.PlayerID;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                ConnectionLog.Add(e.Message);
                CloseServer(InstanceContainer);
                Log = ConnectionLog;
                return false;
            }
            Log = ConnectionLog;
            return true;
        }
    }
}
