using MMR_Tracker_V3.TrackerObjects;
using CLIFrontEnd;
using Windows_Form_Frontend;
using System.Diagnostics;
using MMR_Tracker_V3.TrackerObjectExtentions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MathNet.Numerics.Statistics;
using System.Security.Policy;
using System.IO;
using Octokit;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using MathNet.Numerics;
using System;
using MMR_Tracker_V3;
using FParsec;

namespace TestingForm
{
    public partial class TestingForm : Form
    {
        public static TestingForm CurrentForm;
        public TestingForm()
        {
            CurrentForm = this;
            CLITracker.HideConsole = CLITrackerTesting.HideCLI;
            InitializeComponent();
        }

        private void MainInterface_Load(object sender, EventArgs e)
        {
            Utility.ValidateDevFiles();
            UpdateDebugActions();
            DLLImport.AllocConsole();
            DLLImport.ShowWindow(DLLImport.GetConsoleWindow(), DLLImport.SW_HIDE);
        }

        public class DevAction
        {
            public string Name;
            public Action action;
            public Func<bool>? Conitional;
            public Action? RefreshAction;
            public DevAction(string _Name, Action _action, Action? _RefreshAction = null, Func<bool>? _Conitional = null)
            {
                Name = _Name; action = _action; Conitional = _Conitional; RefreshAction = _RefreshAction;
            }
            public void Run() 
            { 
                action(); 
                if (RefreshAction is not null) { RefreshAction(); }
            }
            public override string ToString()
            {
                return Name;
            }
        }

        public void UpdateDebugActions()
        {
            listBox1.Items.Clear();
            //Debug Action Name, Debug action Code, Show action Check, Refresh After activation

            List<DevAction> DevFunctions = new List<DevAction>()
            {
                new DevAction("Open WinForm Tracker Debug", WinFormTesting.ActivateWinFormInterface, UpdateDebugActions, () => !WinFormTesting.WinformLoaded()),
                new DevAction("Open CLI Tracker Debug", CLITrackerTesting.OpenCLITracker, UpdateDebugActions),
                new DevAction("Save Tracker State", WinFormTesting.SaveWinformTrackerState, UpdateDebugActions, WinFormTesting.CanSaveWinformTrackerState),
                new DevAction("Load Tracker State", WinFormTesting.LoadWinformTrackerState, UpdateDebugActions, WinFormTesting.CanLoadWinformTrackerState),
                new DevAction("Print Selected Object to Console", WinFormTesting.PrintWinformSelectedObject, UpdateDebugActions, () => WinFormTesting.LastSelectedObject is not null),
                new DevAction("Create TPR Data", GameFileCreation.TPRCreateData, UpdateDebugActions),
                new DevAction("Create OOTMM Data", GameFileCreation.OOTMMCreateData, UpdateDebugActions),
                new DevAction("Create PMR Data", GameFileCreation.PMRCreateData, UpdateDebugActions),
                new DevAction("Connect To Async Web Server P1", () => ConnectToAsyncWebServer(1), UpdateDebugActions, () => { return !(Asyncclient?.Connected??false); }),
                new DevAction("Connect To Async Web Server P2", () => ConnectToAsyncWebServer(2), UpdateDebugActions, () => { return !(Asyncclient?.Connected??false); }),
                new DevAction("Connect To Async Web Server P3", () => ConnectToAsyncWebServer(3), UpdateDebugActions, () => { return !(Asyncclient?.Connected??false); }),
                new DevAction("Send To Async Web Server", SendDataToServer, null, () => { return Asyncclient?.Connected??false; } ),
            };

            foreach (var Function in DevFunctions)
            {
                if (Function.Conitional is not null && !Function.Conitional()) { continue; }
                listBox1.Items.Add(Function);
            }
        }

        TcpClient Asyncclient = null;
        int PlayerID = -1;
        private async void ConnectToAsyncWebServer(int AsPlayer)
        {
            try
            {
                Asyncclient = new TcpClient("127.0.0.1", 25570);
                NetData.NetPacket HandshakePacket = new NetData.NetPacket(AsPlayer, NetData.PacketType.None, $"Password{AsPlayer}");
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(HandshakePacket.ToFormattedJson());
                Asyncclient.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
                byte[] buffer = new byte[Asyncclient.ReceiveBufferSize];
                int bytesRead = Asyncclient.GetStream().Read(buffer, 0, Asyncclient.ReceiveBufferSize);
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                var ConfirmationHandshake = JsonConvert.DeserializeObject<NetData.NetPacket>(dataReceived);
                if (!ConfirmationHandshake.HandshakeResponse.ConnectionSuccess)
                {
                    MessageBox.Show($"Failed to connect to server!\n{ConfirmationHandshake.HandshakeResponse.ConnectionStatus}");
                    return;
                }
                MessageBox.Show(ConfirmationHandshake.HandshakeResponse.ConnectionStatus);
                PlayerID = AsPlayer;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to connect to server!\n{e.Message}");
                Asyncclient = null;
                return;
            }
            await OpenListenThread();

        }

        bool Recieving = true;

        private async Task OpenListenThread()
        {
            string ExitReason = "Unknown";
            while (Asyncclient?.Connected??false && Recieving)
            {
                try
                {
                    byte[] buffer = new byte[Asyncclient.ReceiveBufferSize];
                    int bytesRead = await Asyncclient.GetStream().ReadAsync(buffer, 0, Asyncclient.ReceiveBufferSize);
                    if (!Recieving) { break; }
                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Debug.WriteLine($"Server Data: {dataReceived}");
                    NetData.NetPacket packet = JsonConvert.DeserializeObject<NetData.NetPacket>(dataReceived);
                    if (packet.packetType == NetData.PacketType.ChatMessage)
                    {
                        string Player = packet.ChatMessage.PlayerID < 0 ? "Server" : $"Player {packet.ChatMessage.PlayerID}";
                        listBox1.Items.Add($"{Player}: {packet.ChatMessage.Message}");
                    }
                }
                catch (Exception e)
                {
                    ExitReason = e.Message;
                    break;
                }
            }
            Console.WriteLine($"Server Disconnected\n{ExitReason}");
            Asyncclient = null;
            UpdateDebugActions();
        }

        private void SendDataToServer()
        {
            string Message = Interaction.InputBox("Send Chat");
            NetData.NetPacket packet = new NetData.NetPacket(PlayerID, NetData.PacketType.ChatMessage);
            packet.ChatMessage = new NetData.ChatMessage(PlayerID, Guid.NewGuid(), Message);
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(packet.ToFormattedJson());
            Asyncclient.GetStream().Write(bytesToSend, 0, bytesToSend.Length);

            listBox1.Items.Add($"Player {PlayerID}: {Message}");
        }

        private void LB_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is DevAction DevAction) 
            {
                DevAction.Run();
            }
        }
    }
}