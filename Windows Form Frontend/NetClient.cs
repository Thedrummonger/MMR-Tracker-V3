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
using static System.Runtime.InteropServices.JavaScript.JSType;
using MMR_Tracker_V3.SpoilerLogHandling;

namespace Windows_Form_Frontend
{
    public partial class NetClient : Form
    {
        MainInterface ParentWindowsForm;
        InstanceContainer InstanceContainer;
        ListenerThread ListenerThread;
        NetSessionData NetSessionData;
        ArchipelagoConnectionHandler archipelagoConnection;
        public NetClient(MainInterface testingForm, InstanceContainer C)
        {
            InstanceContainer = C;
            ParentWindowsForm = testingForm;
            InitializeComponent();
        }

        bool ModeUpdating = false;

        private OnlineMode CurrentMode = OnlineMode.None;
        private string ServerCache = "127.0.0.1";
        private int PortCache = 25570;
        private string APServerCache = "archipelago.gg";
        private int APPortCache = 38281;

        //Form Actions
        private void NetClient_Load(object sender, EventArgs e)
        {
            NetSessionData = new NetSessionData(txtServerAddress.Text, (int)nudPort.Value,
                (int)nudPlayer.Value, txtSlotID.Text, txtGameName.Text, txtPassword.Text,
                chkRecieveData.Checked, chkSendData.Checked, chkProcessData.Checked, chkAllowCheck.Checked,
                PrintToConsoleThreadSafe, UpdateUI, UpdateMainForm, InstanceContainer);
            ListenerThread = new ListenerThread(NetSessionData);
            archipelagoConnection = new ArchipelagoConnectionHandler(NetSessionData);
            ModeUpdating = true;
            LocationChecker.CheckStateChanged += ListenerThread.TrackerDataHandeling_CheckedObjectsUpdate;

            cmbGameType.DataSource = Utility.EnumAsArray<NetData.OnlineMode>();
            cmbGameType.SelectedIndexChanged += CmbGameType_SelectedIndexChanged;

            txtServerAddress.Text = ServerCache;
            nudPort.Value = PortCache;
            nudPlayer.Value = 1;

            txtGameName.Text = InstanceContainer.Instance.LogicDictionary.GameName;

            UpdateUI();

            ModeUpdating = false;
        }
        private void NetClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (InstanceContainer.netConnection.IsConnected())
            {
                var result = MessageBox.Show($"Closing this windows will disable the active connection, are you sure?", "Close Net Socket", MessageBoxButtons.YesNo);
                if (result != DialogResult.Yes) { e.Cancel = true; return; }
            }
            LocationChecker.CheckStateChanged -= ListenerThread.TrackerDataHandeling_CheckedObjectsUpdate;
            PrintToConsole($"Connection closed manually");
            ConnectionHandling.CloseServer(InstanceContainer);
            ParentWindowsForm.CurrentNetClientForm = null;
        }

        //UI Updates
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
            btnProcessData.Enabled = (ListenerThread.LocationDataToProcess.Any() || ListenerThread.ItemDataToProcess.Any() || IsArchipelago) && Connected;
            chkAllowCheck.Enabled = IsCoop;
            chkSendData.Enabled = !IsArchipelago && !IsNone;
            chkRecieveData.Enabled = !IsNone;
            chkProcessData.Enabled = !IsNone;
            if (chkShowPass.Checked) { txtPassword.PasswordChar = '\0'; }
            else { txtPassword.PasswordChar = '*'; }
        }
        public void UpdateMainForm()
        {
            MainInterface.InstanceContainer.logicCalculation.CalculateLogic();
            Invoke(new Action(() =>
            {
                MainInterface.CurrentProgram.PrintToListBox();
            }));
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
        private void PrintToConsole(string Content, object[] flags = null) { PrintToConsole(new string[] { Content }); }
        private void PrintToConsoleThreadSafe(string Content, object[] flags = null) 
        {
            Invoke(new Action(() =>
            {
                if (!chkRecieveData.Checked) { return; }
                PrintToConsole(Content);
            }));
        }

        //Setting Updates
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
            UpdateNetSessionParams();
        }
        private void chkOption_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUI();
            UpdateNetSessionParams();
        }
        private void textbox_TextChanged(object sender, EventArgs e)
        {
            UpdateNetSessionParams();
        }
        private void UpdateNetSessionParams()
        {
            NetSessionData.UpdateParams(txtServerAddress.Text, (int)nudPort.Value,
                (int)nudPlayer.Value, txtSlotID.Text, txtGameName.Text, txtPassword.Text,
                chkRecieveData.Checked, chkSendData.Checked, chkProcessData.Checked, chkAllowCheck.Checked);
        }

        //Buttons
        private void btnProcessData_Click(object sender, EventArgs e)
        {
            if (InstanceContainer.netConnection.OnlineMode == OnlineMode.Coop)
            {
                ListenerThread.ProcessSharedLocations();
            }
            else if (InstanceContainer.netConnection.OnlineMode == OnlineMode.Multiworld)
            {
                ListenerThread.ProcessMultiworldItems();
            }
            else if (InstanceContainer.netConnection.OnlineMode == OnlineMode.Archipelago)
            {
                archipelagoConnection.SyncWithArchipelagoData();
            }
            UpdateUI();
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
                PrintToConsole(["Closing Server", "Client Closed Manually"]);
                ConnectionHandling.CloseServer(InstanceContainer);
            }
            Debug.WriteLine("Connection Complete");
            UpdateUI();
        }

        //Misc
        private async void ConnectToWebServer()
        {
            var Connected = ConnectionHandling.ConnectToWebServer(
                InstanceContainer, txtServerAddress.Text, (int)nudPort.Value,
                (int)nudPlayer.Value, txtPassword.Text, CurrentMode, out List<string> Log);
            PrintToConsole(Log);
            UpdateUI();
            if (!Connected) { return; }
            string ExitReason = await ListenerThread.OpenListenThread();
            PrintToConsole(ExitReason);
            ConnectionHandling.CloseServer(InstanceContainer);
            UpdateUI();

        }
        private void ConnectToArchipelago()
        {
            var Connected = archipelagoConnection.Connect(out List<string> Log);
            PrintToConsole(Log);
            if (!Connected) { return ; }
            nudPlayer.Value = InstanceContainer.netConnection.ArchipelagoClient.Session.ConnectionInfo.Slot;
            if (InstanceContainer.Instance.SpoilerLog is null)
            {
                var Result = MessageBox.Show("Would you like to apply spoiler data?", "Import spoiler", MessageBoxButtons.YesNo);
                if (Result == DialogResult.Yes) 
                {
                    SpoilerTools.ApplySpoilerLog(InstanceContainer, WinFormUtils.SelectAndReadFile);
                }
            }
            archipelagoConnection.SyncWithArchipelagoData();
            ParentWindowsForm.UpdateUI();
            archipelagoConnection.ActivateListers();
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
                else if (this.ActiveControl.In(txtChatMessage, btnSendChat, lbConsole))
                {
                    btnSendChat_Click(sender, e);
                }
                e.Handled = true;
            }
        }



    }
}
