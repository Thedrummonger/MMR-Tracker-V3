using MathNet.Numerics.Statistics;
using Microsoft.VisualBasic;
using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.NetCode.NetData;
using MMR_Tracker_V3.NetCode;
using System.Net;
using Archipelago.MultiClient.Net.Packets;
using MMR_Tracker_V3.SpoilerLogImporter;

namespace Windows_Form_Frontend
{
    public partial class NetClient : Form
    {
        MainInterface ParentForm;
        InstanceContainer InstanceContainer;
        public NetClient(MainInterface testingForm, InstanceContainer C)
        {
            InstanceContainer = C;
            ParentForm = testingForm;
            InitializeComponent();
        }

        bool ModeUpdating = false;
        private void NetClient_Load(object sender, EventArgs e)
        {
            ModeUpdating = true;
            LocationChecker.CheckStateChanged += TrackerDataHandeling_CheckedObjectsUpdate;

            cmbGameType.DataSource = Utility.EnumAsArray<NetData.OnlineMode>();
            cmbGameType.SelectedIndexChanged += CmbGameType_SelectedIndexChanged;

            txtServerAddress.Text = ServerCache;
            nudPort.Value = PortCache;
            nudPlayer.Value = 1;

            txtGameName.Text = InstanceContainer.Instance.LogicDictionary.GameName;

            UpdateUI();

            ModeUpdating = false;
        }

        private OnlineMode CurrentMode = OnlineMode.None;
        private string ServerCache = "127.0.0.1";
        private int PortCache = 25570;
        private string APServerCache = "archipelago.gg";
        private int APPortCache = 38281;
        private void CmbGameType_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnlineMode NewMode = (OnlineMode)cmbGameType.SelectedItem;

            if (CurrentMode != OnlineMode.Archipelago && NewMode == OnlineMode.Archipelago)
            {
                ServerCache = txtServerAddress.Text;
                PortCache = (int)nudPort.Value;
                txtServerAddress.Text = APServerCache;
                nudPort.Value = APPortCache;
            }
            else if (CurrentMode == OnlineMode.Archipelago && NewMode != OnlineMode.Archipelago)
            {
                APServerCache = txtServerAddress.Text;
                APPortCache = (int)nudPort.Value;
                txtServerAddress.Text = ServerCache;
                nudPort.Value = PortCache;
            }

            UpdateUI();
            CurrentMode = (OnlineMode)cmbGameType.SelectedItem;
        }

        private void NetClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(InstanceContainer.netConnection.ServerConnection is not null && InstanceContainer.netConnection.ServerConnection.Connected)
            {
                var result = MessageBox.Show($"Closing this windows will disable the active connection, are you sure?", "Close Net Socket", MessageBoxButtons.YesNo);
                if (result != DialogResult.Yes) { e.Cancel = true; return; }
            }
            LocationChecker.CheckStateChanged -= TrackerDataHandeling_CheckedObjectsUpdate;
            CloseServer("Client Closed Manually");
            ParentForm.CurrentNetClientForm = null;
        }

        public void CloseServer(string Reason = "Unknown", bool WasConnected = true)
        {
            if (WasConnected)
            {
                Debug.WriteLine($"Server Disconnected");
                PrintToConsole($"Server Disconnected");
            }
            Debug.WriteLine($"{Reason}");
            PrintToConsole($"{Reason}");

            if (InstanceContainer.netConnection.ServerConnection is not null && InstanceContainer.netConnection.ServerConnection.Connected)
            {
                InstanceContainer.netConnection.ServerConnection.GetStream().Close();
                InstanceContainer.netConnection.ServerConnection.Close();
            }
            if (InstanceContainer.netConnection.ArchipelagoClient is not null)
            {
                InstanceContainer.netConnection.ArchipelagoClient.Session.Socket.DisconnectAsync();
            }
            InstanceContainer.netConnection.Reset();

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

        private void TrackerDataHandeling_CheckedObjectsUpdate(List<object> ObjectsUpdated, MMR_Tracker_V3.TrackerObjects.InstanceData.TrackerInstance Instance)
        {
            var LocationsUpdated = ObjectsUpdated.Where(x => x is LocationData.LocationObject lo && (lo.Randomizeditem.OwningPlayer > -1) || InstanceContainer.netConnection.OnlineMode != OnlineMode.Multiworld)
                .Select(x => (LocationData.LocationObject)x);
            if (InstanceContainer.netConnection.IsConnected()) { return; }
            if (!chkSendData.Checked) { return; }

            NetData.NetPacket? packet;
            switch (InstanceContainer.netConnection.OnlineMode)
            {
                case OnlineMode.Multiworld:
                    packet = CreateMultiWorldPacket(Instance, LocationsUpdated);
                    break;
                case OnlineMode.Coop:
                    packet = CreateCoopPacket(Instance, LocationsUpdated);
                    break;
                default: return;
            }
            if (packet == null) { return; }

            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(packet.ToFormattedJson());
            InstanceContainer.netConnection.ServerConnection.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
        }

        private NetData.NetPacket? CreateCoopPacket(MMR_Tracker_V3.TrackerObjects.InstanceData.TrackerInstance arg2, IEnumerable<LocationData.LocationObject> locationsUpdated)
        {
            NetData.NetPacket packet = new NetData.NetPacket((int)nudPlayer.Value, NetData.PacketType.OnlineSynedLocations, txtPassword.Text);
            packet.LocationData = getCheckedLocations(arg2);
            packet.UpdateWhitelist = locationsUpdated.Any(x => x.CheckState == MiscData.CheckState.Checked) ? null : new int[] { -1 };
            return packet;
        }
        private NetData.NetPacket? CreateMultiWorldPacket(MMR_Tracker_V3.TrackerObjects.InstanceData.TrackerInstance arg2, IEnumerable<LocationData.LocationObject> locationsUpdated)
        {
            NetData.NetPacket packet = new NetData.NetPacket((int)nudPlayer.Value, NetData.PacketType.MultiWorldItems, txtPassword.Text);
            packet.ItemData = GetMultiworldItemsToSend(arg2);
            packet.UpdateWhitelist = locationsUpdated.Select(x => x.Randomizeditem.OwningPlayer).Distinct().ToArray();
            return packet;
        }

        public static Dictionary<int, Dictionary<string, int>> GetMultiworldItemsToSend(MMR_Tracker_V3.TrackerObjects.InstanceData.TrackerInstance instance)
        {
            Dictionary<int, Dictionary<string, int>> PlayersSentItem = new Dictionary<int, Dictionary<string, int>>();
            foreach (var Item in instance.ItemPool)
            {
                if (!Item.Value.AmountSentToPlayer.Any()) { continue; }

                foreach (var Player in Item.Value.AmountSentToPlayer)
                {
                    if (Player.Value < 1) { continue; }
                    if (!PlayersSentItem.ContainsKey(Player.Key)) { PlayersSentItem[Player.Key] = new Dictionary<string, int>(); }
                    PlayersSentItem[Player.Key][Item.Value.GetDictEntry().GetName()] = Player.Value;
                }
            }
            return PlayersSentItem;
        }

        public static Dictionary<string, string> getCheckedLocations(MMR_Tracker_V3.TrackerObjects.InstanceData.TrackerInstance instance)
        {
            return instance.LocationPool.Values.Where(x => x.CheckState == MiscData.CheckState.Checked).ToDictionary(x => x.ID, x => x.Randomizeditem.Item);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!InstanceContainer.netConnection.IsConnected())
            {
                if (!NetData.IsIpAddress(txtServerAddress.Text, out IPAddress ParsedAddress))
                {
                    PrintToConsole($"{txtServerAddress.Text} Was not a valid IP address");
                    return; 
                }
                PrintToConsole($"Connecting to server {ParsedAddress}:{nudPort.Value}");
                lbConsole.Refresh();
                if ((OnlineMode)cmbGameType.SelectedItem == OnlineMode.Archipelago) { ConnectToArchipelago(); }
                else { ConnectToWebServer(); }
            }
            else
            {
                CloseServer("Client Closed Manually");
            }
            Debug.WriteLine("Connection Complete");
            UpdateUI();
        }

        public void UpdateUI()
        {
            bool Connected = InstanceContainer.netConnection.IsConnected();
            bool IsNone = (OnlineMode)cmbGameType.SelectedItem == OnlineMode.None;
            bool IsArchipelago = (OnlineMode)cmbGameType.SelectedItem == OnlineMode.Archipelago;
            bool IsCoop = (OnlineMode)cmbGameType.SelectedItem == OnlineMode.Coop;
            bool IsMultworld = (OnlineMode)cmbGameType.SelectedItem == OnlineMode.Multiworld;
            //btnConnect.Enabled = !Connected;
            btnConnect.Text = Connected ? "Disconnect" : "Connect";
            btnConnect.Enabled = !IsNone;
            txtServerAddress.Enabled = !Connected && !IsNone;
            nudPort.Enabled = !Connected && !IsNone;
            nudPlayer.Enabled = !Connected && (IsCoop || IsMultworld);
            txtGameName.Enabled = !Connected && IsArchipelago;
            txtSlotID.Enabled = !Connected && IsArchipelago;
            txtPassword.Enabled = !Connected && !IsNone;
            cmbGameType.Enabled = !Connected;
            txtChatMessage.Enabled = Connected;
            btnSendChat.Enabled = Connected;
            btnProcessData.Enabled = (LocationDataToProcess.Any() || ItemDataToProcess.Any() || IsArchipelago) && Connected;
            chkAllowCheck.Enabled = IsCoop;
            chkSendData.Enabled = !IsArchipelago && !IsNone;
            chkRecieveData.Enabled = !IsNone;
            chkProcessData.Enabled = !IsNone;
            if (chkShowPass.Checked) { txtPassword.PasswordChar = '\0'; }
            else { txtPassword.PasswordChar = '*'; }
        }

        private async void ConnectToWebServer()
        {
            int PlayerID = (int)nudPlayer.Value;
            try
            {
                NetData.OnlineMode ClientMode = (NetData.OnlineMode)cmbGameType.SelectedItem;
                InstanceContainer.netConnection.ServerConnection = new TcpClient(txtServerAddress.Text, (int)nudPort.Value);
                InstanceContainer.netConnection.ServerConnection.LingerState = new LingerOption(true, 0);
                NetData.NetPacket HandshakePacket = new NetData.NetPacket(PlayerID, NetData.PacketType.Handshake, txtPassword.Text, Mode: ClientMode);
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(HandshakePacket.ToFormattedJson());
                InstanceContainer.netConnection.ServerConnection.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
                byte[] buffer = new byte[InstanceContainer.netConnection.ServerConnection.ReceiveBufferSize];
                int bytesRead = InstanceContainer.netConnection.ServerConnection.GetStream().Read(buffer, 0, InstanceContainer.netConnection.ServerConnection.ReceiveBufferSize);
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                var ConfirmationHandshake = JsonConvert.DeserializeObject<NetData.NetPacket>(dataReceived);
                if (!ConfirmationHandshake.HandshakeResponse.ConnectionSuccess)
                {
                    CloseServer("Failed to connect to server!", false);
                    PrintToConsole(ConfirmationHandshake.HandshakeResponse.ConnectionStatus.Split('\n'));
                    return;
                }
                PrintToConsole(ConfirmationHandshake.HandshakeResponse.ConnectionStatus.Split('\n'));
                InstanceContainer.netConnection.OnlineMode = ConfirmationHandshake.HandshakeResponse.ClientMode;
                InstanceContainer.netConnection.PlayerID = ConfirmationHandshake.HandshakeResponse.PlayerID;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                CloseServer(e.Message, false);
                return;
            }
            UpdateUI();
            string ExitReason = await OpenListenThread();
            CloseServer(ExitReason);
            UpdateUI();

        }

        private void ConnectToArchipelago()
        {
            string ServerAddress = $"{txtServerAddress.Text}:{nudPort.Value}";
            InstanceContainer.netConnection.ArchipelagoClient =
                new ArchipelagoConnector(txtGameName.Text, txtSlotID.Text, txtPassword.Text, ServerAddress);

            if (!InstanceContainer.netConnection.ArchipelagoClient.WasConnectionSuccess(out string[] Error))
            {
                PrintToConsole(Error);
                InstanceContainer.netConnection.ArchipelagoClient = null;
                return;
            }
            var APClient = InstanceContainer.netConnection.ArchipelagoClient;
            var ConnectionInfo = APClient.GetLoginSuccessInfo();
            PrintToConsole([$"Connected to {ServerAddress}"]);
            InstanceContainer.netConnection.OnlineMode = OnlineMode.Archipelago;
            nudPlayer.Value = InstanceContainer.netConnection.ArchipelagoClient.Session.ConnectionInfo.Slot;
            InstanceContainer.netConnection.PlayerID = InstanceContainer.netConnection.ArchipelagoClient.Session.ConnectionInfo.Slot;
            InstanceContainer.netConnection.SlotID = txtSlotID.Text;
            InstanceContainer.netConnection.GameName = txtGameName.Text;

            InstanceContainer.netConnection.PlayerNames = APClient.Session.Players.AllPlayers.ToDictionary(x => x.Slot, x => x.Name);
            //Has to go Before event listeners are defined

            if (InstanceContainer.Instance.SpoilerLog is null)
            {
                var SpoilerLog = MMR_Tracker_V3.SpoilerLogImporter.Archipelago.CreateGenericSpoilerLog(InstanceContainer);
                var LogImported = SpoilerLogTools.ImportSpoilerLog(SpoilerLog, "", InstanceContainer);
                InstanceContainer.logicCalculation.CalculateLogic();
            }
            ParentForm.UpdateUI();
            SyncWithArchipelagoData();

            APClient.Session.Items.ItemReceived += ArchipelagoItemReceived;
            APClient.Session.Locations.CheckedLocationsUpdated += ArchipelagoLocationChecked;
            APClient.Session.MessageLog.OnMessageReceived += ArchipelagoChatMessageReceived;

        }

        private void ArchipelagoChatMessageReceived(Archipelago.MultiClient.Net.MessageLog.Messages.LogMessage message)
        {
            string Chat = message.ToString();
            Invoke(new Action(() => { 
                if (!chkRecieveData.Checked) { return; }
                PrintToConsole(Chat); 
            }));
        }

        private void ArchipelagoLocationChecked(System.Collections.ObjectModel.ReadOnlyCollection<long> newCheckedLocations)
        {
            Debug.WriteLine("ArchipelagoLocationChecked");
            Invoke(new Action(() => {
                if (!chkRecieveData.Checked) { return; }
                SyncWithArchipelagoData();
            }));
        }

        private void ArchipelagoItemReceived(Archipelago.MultiClient.Net.Helpers.ReceivedItemsHelper helper)
        {
            Debug.WriteLine("ArchipelagoItemReceived");
            Invoke(new Action(() => {
                if (!chkRecieveData.Checked) { return; }
                SyncWithArchipelagoData();
            }));
        }

        private void SyncWithArchipelagoData()
        {
            var Instance = InstanceContainer.Instance;
            var Sess = InstanceContainer.netConnection.ArchipelagoClient.Session;
            var AllItems = Sess.Items.AllItemsReceived.Select(x => (Sess.Items.GetItemName(x.Item), Sess.Locations.GetLocationNameFromId(x.Location), x.Player)).ToArray();
            var AllLocations = Sess.Locations.AllLocationsChecked.Select(x => Sess.Locations.GetLocationNameFromId(x)).ToArray();
            foreach (var i in InstanceContainer.Instance.ItemPool.Values) { i.AmountAquiredOnline = []; }
            List<LocationData.LocationObject> ToCheck = new List<LocationData.LocationObject>();
            foreach(var Entry in AllItems)
            {
                bool IsLocal = Entry.Player == InstanceContainer.netConnection.PlayerID;
                var location = IsLocal ? Instance.GetLocationByID(Entry.Item2) : null;
                var Item = Instance.GetItemByID(Entry.Item1);
                if (!IsLocal && Item is not null) { 
                    Item.AmountAquiredOnline.SetIfEmpty(Entry.Player, 0);
                    Item.AmountAquiredOnline[Entry.Player]++;
                }
                else if (IsLocal && location is not null && location.CheckState != MiscData.CheckState.Checked)
                {
                    location.Randomizeditem.Item = Entry.Item1;
                    ToCheck.Add(location);
                }
            }
            foreach(var Entry in AllLocations)
            {
                var location = Instance.GetLocationByID(Entry);
                if (location is null) { continue; }
                if (location.GetItemAtCheck() == null) { location.Randomizeditem.Item = "Archipelago Item"; }
                ToCheck.Add(location);
            }
            LocationChecker.CheckSelectedItems(ToCheck, InstanceContainer, new CheckItemSetting(MiscData.CheckState.Checked));
            InstanceContainer.logicCalculation.CalculateLogic();
            ParentForm.PrintToListBox();
        }

        private async Task<string> OpenListenThread()
        {
            string ExitReason = "Unknown";
            while (InstanceContainer.netConnection.ServerConnection is not null && InstanceContainer.netConnection.ServerConnection.Connected)
            {
                Debug.WriteLine("Listening for data");
                string dataReceived;
                try
                {
                    byte[] buffer = new byte[InstanceContainer.netConnection.ServerConnection.ReceiveBufferSize];
                    int bytesRead = await InstanceContainer.netConnection.ServerConnection.GetStream().ReadAsync(buffer, 0, InstanceContainer.netConnection.ServerConnection.ReceiveBufferSize);
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
                    ParseSharedLocationData(packet);
                    break;
                case NetData.PacketType.MultiWorldItems:
                    ParseMultiWorldData(packet);
                    break;
                case NetData.PacketType.ChatMessage:
                    string PlayerName = packet.ChatMessage.PlayerID < 0 ? "Server" : $"Player {packet.ChatMessage.PlayerID}";
                    PrintToConsole($"{PlayerName}: {packet.ChatMessage.Message}");
                    break;
                default: 
                    break;
            }
        }

        private void btnProcessData_Click(object sender, EventArgs e)
        {
            if (InstanceContainer.netConnection.OnlineMode == OnlineMode.Coop)
            {
                ProcessSharedLocations();
            }
            else if (InstanceContainer.netConnection.OnlineMode == OnlineMode.Multiworld)
            {
                ProcessMultiworldItems();
            }
            else if (InstanceContainer.netConnection.OnlineMode == OnlineMode.Archipelago)
            {
                SyncWithArchipelagoData();
            }
            UpdateUI();
        }

        private Dictionary<int, Dictionary<string, int>> ItemDataToProcess = new Dictionary<int, Dictionary<string, int>>();

        private void ParseMultiWorldData(NetPacket packet)
        {
            foreach(var PlayerData in packet.ItemData)
            {
                foreach(var ItemData in PlayerData.Value)
                {
                    ItemDataToProcess.SetIfEmpty(PlayerData.Key, new Dictionary<string, int>());
                    ItemDataToProcess[PlayerData.Key][ItemData.Key] = ItemData.Value;
                }
            }
            if (chkProcessData.Checked)
            {
                ProcessMultiworldItems();
            }
            UpdateUI();
        }

        private void ProcessMultiworldItems()
        {
            foreach(var item in InstanceContainer.Instance.ItemPool)
            {
                foreach(var Player in ItemDataToProcess.Keys)
                {
                    if (item.Value.AmountAquiredOnline.ContainsKey(Player)) { item.Value.AmountAquiredOnline.Remove(Player); }
                }
            }

            foreach(var players in ItemDataToProcess)
            {
                foreach(var items in players.Value)
                {
                    var ValidItem = InstanceContainer.Instance.GetItemToPlace(items.Key, false, true);
                    if (ValidItem is null) { continue; }
                    ValidItem.AmountAquiredOnline.SetIfEmpty(players.Key, 0);
                    ValidItem.AmountAquiredOnline[players.Key]++;
                }
            }
            ItemDataToProcess.Clear();
            MainInterface.InstanceContainer.logicCalculation.CalculateLogic();
            MainInterface.CurrentProgram.UpdateUI();
        }

        private Dictionary<string, string> LocationDataToProcess = new Dictionary<string, string>();

        private void ParseSharedLocationData(NetPacket packet)
        {
            foreach(var i in packet.LocationData.Keys)
            {
                LocationDataToProcess[i] = packet.LocationData[i];
            }
            if (chkProcessData.Checked)
            {
                ProcessSharedLocations();
            }
            UpdateUI();
        }

        private void ProcessSharedLocations()
        {
            MiscData.CheckState CheckAction = chkAllowCheck.Checked ? MiscData.CheckState.Checked : MiscData.CheckState.Marked;

            List<LocationData.LocationObject> LocationList = new List<LocationData.LocationObject>();
            foreach (var i in LocationDataToProcess)
            {
                if (!InstanceContainer.Instance.LocationPool.ContainsKey(i.Key)) { continue; }
                var Location = InstanceContainer.Instance.LocationPool[i.Key];
                if (Location.CheckState == MiscData.CheckState.Checked || Location.CheckState == CheckAction) { continue; }
                LocationList.Add(Location);
            }

            var CheckObjectOptions = new CheckItemSetting(CheckAction).SetEnforceMarkAction(true).SetCheckUnassignedLocations((O, C) =>
            {
                List<ManualCheckObjectResult> Results = [];
                foreach (var obj in O)
                {
                    LocationData.LocationObject location = obj as LocationData.LocationObject;
                    Results.Add(new ManualCheckObjectResult().SetLocation(location, LocationDataToProcess[location.ID]));
                }
                return Results;
            });

            LocationChecker.SetLocationsCheckState(LocationList, InstanceContainer, CheckObjectOptions);

            LocationDataToProcess.Clear();
            MainInterface.InstanceContainer.logicCalculation.CalculateLogic();
            MainInterface.CurrentProgram.UpdateUI();

        }

        private void btnSendChat_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtChatMessage.Text)) { return; }
            if (!InstanceContainer.netConnection.IsConnected()) { return; }
            if ((OnlineMode)cmbGameType.SelectedItem == OnlineMode.Archipelago)
            {
                InstanceContainer.netConnection.ArchipelagoClient.Session.Socket.SendPacket(new SayPacket() { Text = txtChatMessage.Text });
            }
            else
            {

                NetData.NetPacket packet = new NetData.NetPacket((int)nudPlayer.Value, PacketType.ChatMessage, txtPassword.Text);
                Guid ChatGuid = Guid.NewGuid();
                packet.ChatMessage = new ChatMessage((int)nudPlayer.Value, ChatGuid, txtChatMessage.Text);
                PrintToConsole($"Player {nudPlayer.Value} (You): {packet.ChatMessage.Message}");
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(packet.ToFormattedJson());
                InstanceContainer.netConnection.ServerConnection.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
            }
            txtChatMessage.Text = string.Empty;
        }

        private void chkOption_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
            Debug.WriteLine(this.ActiveControl.Name.ToString());
        }

        private void NetClient_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                bool Connected = InstanceContainer.netConnection.ServerConnection is not null && InstanceContainer.netConnection.ServerConnection.Connected;
                if (this.ActiveControl.In(txtServerAddress, nudPort, nudPlayer, txtPassword, chkShowPass, chkRecieveData, chkSendData, chkProcessData, chkAllowCheck) && !Connected)
                {
                    btnConnect_Click(sender, e);
                    this.ActiveControl = txtChatMessage;
                }
                else if(this.ActiveControl.In(txtChatMessage, btnSendChat, lbConsole))
                {
                    btnSendChat_Click(sender, e);
                }
                e.Handled = true;
            }
        }
    }
}
