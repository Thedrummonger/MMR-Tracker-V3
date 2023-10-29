using Microsoft.VisualBasic;
using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows_Form_Frontend;
using static MMR_Tracker_V3.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.NetData;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TestingForm
{
    public partial class NetClient : Form
    {
        TestingForm ParentForm;
        InstanceContainer InstanceContainer;
        public NetClient(TestingForm testingForm, InstanceContainer C)
        {
            InstanceContainer = C;
            ParentForm = testingForm;
            InitializeComponent();
        }
        public TcpClient? ServerConnection = null;

        bool ModeUpdating = false;
        private void NetClient_Load(object sender, EventArgs e)
        {
            ModeUpdating = true;
            MMR_Tracker_V3.TrackerDataHandeling.CheckedObjectsUpdate += TrackerDataHandeling_CheckedObjectsUpdate;
            Array EnumArray = Enum.GetValues(typeof(NetData.OnlineMode));

            txtServerAddress.Text = "127.0.0.1";
            nudPort.Value = 25570;
            nudPlayer.Value = 1;

            UpdateUI();

            ModeUpdating = false;
        }

        private void NetClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show($"Closing this windows will disable the we socket, are you sure?", "Close Net Socket", MessageBoxButtons.YesNo);
            if (result != DialogResult.Yes) { e.Cancel = true; return; }
            MMR_Tracker_V3.TrackerDataHandeling.CheckedObjectsUpdate -= TrackerDataHandeling_CheckedObjectsUpdate;
            TestingForm.CurrentNetClientForm = null;
            CloseServer("Client Closed Manually");
            ParentForm.UpdateDebugActions();
        }

        public void CloseServer(string Reason = "Unknown")
        {
            bool ServerConnectionNull = ServerConnection is null;

            if (ServerConnectionNull) { return; }
            
            Debug.WriteLine($"Server Disconnected\n{Reason}".Split('\n'));
            PrintToConsole($"Server Disconnected\n{Reason}".Split('\n'));
            MainInterface.InstanceContainer.OnlineMode = NetData.OnlineMode.None;
            if (ServerConnection is not null && ServerConnection.Connected)
            {
                ServerConnection.GetStream().Close();
                ServerConnection.Close();
            }
            ServerConnection = null;
        }

        private void PrintToConsole(IEnumerable<string> Content)
        {
            lbConsole.BeginUpdate();
            foreach (string ContentItem in Content)
            {
                lbConsole.Items.Add(ContentItem);
            }
            lbConsole.EndUpdate();
            lbConsole.TopIndex = lbConsole.Items.Count - 1;
        }
        private void PrintToConsole(string Content) { PrintToConsole(new string[] { Content }); }

        private void TrackerDataHandeling_CheckedObjectsUpdate(List<object> arg1, MMR_Tracker_V3.InstanceData.TrackerInstance arg2)
        {
            var LocationsUpdated = arg1.Where(x => x is LocationData.LocationObject).Select(x => (LocationData.LocationObject)x);
            if (!LocationsUpdated.Any(x => x.CheckState == MiscData.CheckState.Checked)) { return; }
            if (ServerConnection is null || !ServerConnection.Connected) { return; }
            if (!chkSendData.Checked) { return; }

            var packet = CreateCheckedItemPacket(InstanceContainer.OnlineMode, arg2);
            if (packet == null) { return; }

            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(packet.ToFormattedJson());
            ServerConnection.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
        }

        private NetData.NetPacket? CreateCheckedItemPacket(NetData.OnlineMode onlineMode, MMR_Tracker_V3.InstanceData.TrackerInstance arg2)
        {
            var packetType = onlineMode switch
            {
                NetData.OnlineMode.Online => NetData.PacketType.OnlineSynedLocations,
                NetData.OnlineMode.Coop => NetData.PacketType.OnlineSynedLocations,
                NetData.OnlineMode.Multiworld => NetData.PacketType.OnlineSynedLocations,
                _ => NetData.PacketType.None,
            };
            if (packetType == NetData.PacketType.None) { return null; }
            NetData.NetPacket packet = new NetData.NetPacket((int)nudPlayer.Value, packetType, txtPassword.Text);
            packet.ItemData = onlineMode == NetData.OnlineMode.Multiworld ? GetMultiworldItemsToSend(arg2) : new Dictionary<int, Dictionary<string, int>>();
            packet.LcationData = onlineMode != NetData.OnlineMode.Multiworld ? getCheckedLocations(arg2) : new Dictionary<string, string>();
            return packet;
        }

        public static Dictionary<int, Dictionary<string, int>> GetMultiworldItemsToSend(MMR_Tracker_V3.InstanceData.TrackerInstance instance)
        {
            Dictionary<int, Dictionary<string, int>> PlayersSentItem = new Dictionary<int, Dictionary<string, int>>();
            foreach (var Item in instance.ItemPool)
            {
                if (!Item.Value.AmountSentToPlayer.Any()) { continue; }

                foreach (var Player in Item.Value.AmountSentToPlayer)
                {
                    if (Player.Value < 1) { continue; }
                    if (!PlayersSentItem.ContainsKey(Player.Key)) { PlayersSentItem[Player.Key] = new Dictionary<string, int>(); }
                    PlayersSentItem[Player.Key][Item.Key] = Player.Value;
                }
            }
            return PlayersSentItem;
        }

        public static Dictionary<string, string> getCheckedLocations(MMR_Tracker_V3.InstanceData.TrackerInstance instance)
        {
            return instance.LocationPool.Values.Where(x => x.CheckState == MiscData.CheckState.Checked).ToDictionary(x => x.ID, x => x.Randomizeditem.Item);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (ServerConnection is null || !ServerConnection.Connected)
            {
                if (!NetData.IsIpAddress(txtServerAddress.Text, out _)) { return; }
                ConnectToWebServer();
            }
            else
            {
                CloseServer("Client Closed Manually");
            }
            UpdateUI();
        }

        public void UpdateUI()
        {
            bool Connected = ServerConnection is not null && ServerConnection.Connected;
            //btnConnect.Enabled = !Connected;
            btnConnect.Text = Connected ? "Disconnect" : "Connect";
            txtServerAddress.Enabled = !Connected;
            nudPort.Enabled = !Connected;
            nudPlayer.Enabled = !Connected;
            txtPassword.Enabled = !Connected;
            txtChatMessage.Enabled = Connected;
            btnSendChat.Enabled = Connected;
        }

        private async void ConnectToWebServer()
        {
            int PlayerID = (int)nudPlayer.Value;
            try
            {
                ServerConnection = new TcpClient(txtServerAddress.Text, (int)nudPort.Value);
                ServerConnection.LingerState = new LingerOption(true, 0);
                NetData.NetPacket HandshakePacket = new NetData.NetPacket(PlayerID, NetData.PacketType.None, txtPassword.Text);
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(HandshakePacket.ToFormattedJson());
                ServerConnection.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
                byte[] buffer = new byte[ServerConnection.ReceiveBufferSize];
                int bytesRead = ServerConnection.GetStream().Read(buffer, 0, ServerConnection.ReceiveBufferSize);
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                var ConfirmationHandshake = JsonConvert.DeserializeObject<NetData.NetPacket>(dataReceived);
                if (!ConfirmationHandshake.HandshakeResponse.ConnectionSuccess)
                {
                    PrintToConsole("Failed to connect to server!");
                    PrintToConsole(ConfirmationHandshake.HandshakeResponse.ConnectionStatus.Split('\n'));
                    ServerConnection = null;
                    return;
                }
                PrintToConsole(ConfirmationHandshake.HandshakeResponse.ConnectionStatus.Split('\n'));
                InstanceContainer.OnlineMode = ConfirmationHandshake.HandshakeResponse.ClientMode;
            }
            catch (Exception e)
            {
                PrintToConsole("Failed to connect to server!");
                PrintToConsole(e.Message);
                ServerConnection = null;
                return;
            }
            UpdateUI();
            string ExitReason = await OpenListenThread();
            CloseServer(ExitReason);
            UpdateUI();

        }
        private async Task<string> OpenListenThread()
        {
            string ExitReason = "Unknown";
            while (ServerConnection is not null && ServerConnection.Connected)
            {
                Debug.WriteLine("Listening for data");
                string dataReceived;
                try
                {
                    byte[] buffer = new byte[ServerConnection.ReceiveBufferSize];
                    int bytesRead = await ServerConnection.GetStream().ReadAsync(buffer, 0, ServerConnection.ReceiveBufferSize);
                    dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                }
                catch (Exception e) { ExitReason = "Server Closed"; break; }

                NetData.NetPacket packet;
                try { packet = JsonConvert.DeserializeObject<NetData.NetPacket>(dataReceived); }
                catch { ExitReason = "Bad Packet From Server"; break; }

                if (packet is null) { ExitReason = "Bad Packet From Server"; break; }
                HandlePacket(packet);
            }
            Debug.WriteLine("Closing Listen Thread");
            return ExitReason;
        }

        private void HandlePacket(NetData.NetPacket? packet)
        {
            if (packet.packetType != NetData.PacketType.ChatMessage && !chkRecieveData.Checked) { return; }
            Debug.WriteLine($"Got Packet of type {packet.packetType}");
            switch (packet.packetType)
            {
                case NetData.PacketType.OnlineSynedLocations:
                    break;
                case NetData.PacketType.MultiWorldItems:
                    break;
                case NetData.PacketType.ChatMessage:
                    string PlayerName = packet.ChatMessage.PlayerID < 0 ? "Server" : $"Player {packet.ChatMessage.PlayerID}";
                    PrintToConsole($"{PlayerName}: {packet.ChatMessage.Message}");
                    break;
                default: 
                    break;
            }
        }

        private void btnSendChat_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtChatMessage.Text)) { return; }
            if (ServerConnection is null || !ServerConnection.Connected) { return; }

            NetData.NetPacket packet = new NetData.NetPacket((int)nudPlayer.Value, PacketType.ChatMessage, txtPassword.Text);
            Guid ChatGuid = Guid.NewGuid();
            packet.ChatMessage = new ChatMessage((int)nudPlayer.Value, ChatGuid, txtChatMessage.Text);
            PrintToConsole($"Player {nudPlayer.Value} (You): {packet.ChatMessage.Message}");
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(packet.ToFormattedJson());
            ServerConnection.GetStream().Write(bytesToSend, 0, bytesToSend.Length);

            txtChatMessage.Text = string.Empty;
        }
    }
}
