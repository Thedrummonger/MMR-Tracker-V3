using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using TDMUtils;
using static MMR_Tracker_V3.NetCode.NetData;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MMR_Tracker_V3.NetCode
{
    public class ConnectionHandling
    {
        public static void CloseServer(InstanceContainer InstanceContainer)
        {
            InstanceContainer.netConnection.CloseAndReset();
        }
        public static bool ConnectToWebServer(NetData.NetSessionData sessionData, NetData.OnlineMode ClientMode, out List<string> Log)
        {
            var IC = sessionData.InstanceContainer;
            List<string> ConnectionLog = [];
            try
            {
                IC.netConnection.ServerConnection = new TcpClient(sessionData.ServerAddress, sessionData.ServerPort);
                IC.netConnection.ServerConnection.LingerState = new LingerOption(true, 0);
                NetData.NetPacket HandshakePacket = new NetData.NetPacket(sessionData.PlayerID, NetData.PacketType.Handshake, sessionData.Password, Mode: ClientMode);
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(HandshakePacket.ToFormattedJson());
                IC.netConnection.ServerConnection.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
                byte[] buffer = new byte[IC.netConnection.ServerConnection.ReceiveBufferSize];
                int bytesRead = IC.netConnection.ServerConnection.GetStream().Read(buffer, 0, IC.netConnection.ServerConnection.ReceiveBufferSize);
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                var ConfirmationHandshake = JsonConvert.DeserializeObject<NetData.NetPacket>(dataReceived);
                if (!ConfirmationHandshake.HandshakeResponse.ConnectionSuccess)
                {
                    ConnectionLog.Add("Failed to connect to server!");
                    CloseServer(IC);
                    ConnectionLog.AddRange(ConfirmationHandshake.HandshakeResponse.ConnectionStatus.Split('\n'));
                    Log = ConnectionLog;
                    return false;
                }
                ConnectionLog.AddRange(ConfirmationHandshake.HandshakeResponse.ConnectionStatus.Split('\n'));
                IC.netConnection.OnlineMode = ConfirmationHandshake.HandshakeResponse.ClientMode;
                IC.netConnection.PlayerID = ConfirmationHandshake.HandshakeResponse.PlayerID;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                ConnectionLog.Add(e.Message);
                CloseServer(IC);
                Log = ConnectionLog;
                return false;
            }
            Log = ConnectionLog;
            return true;
        }
        public static bool ConnectToAPServer(NetData.NetSessionData Data, out List<string> Log)
        {
            Log = new List<string>();
            string ServerAddress = $"{Data.ServerAddress}:{Data.ServerPort}";
            Data.InstanceContainer.netConnection.ArchipelagoClient =
            new ArchipelagoConnector(Data.GameName, Data.SlotID, Data.Password, ServerAddress);

            if (!Data.InstanceContainer.netConnection.ArchipelagoClient.WasConnectionSuccess(out string[] Error))
            {
                Log.Add(string.Join("\n", Error));
                Data.InstanceContainer.netConnection.ArchipelagoClient = null;
                return false;
            }
            var APClient = Data.InstanceContainer.netConnection.ArchipelagoClient;
            var ConnectionInfo = APClient.GetLoginSuccessInfo();
            Log.Add($"Connected to {ServerAddress}");
            Data.InstanceContainer.netConnection.OnlineMode = OnlineMode.Archipelago;
            Data.InstanceContainer.netConnection.PlayerID = Data.InstanceContainer.netConnection.ArchipelagoClient.Session.ConnectionInfo.Slot;
            Data.InstanceContainer.netConnection.SlotID = Data.SlotID;
            Data.InstanceContainer.netConnection.GameName = Data.GameName;

            Data.InstanceContainer.netConnection.PlayerNames = APClient.Session.Players.AllPlayers.ToDictionary(x => x.Slot, x => x.Name);

            return true;
        }
    }
}
