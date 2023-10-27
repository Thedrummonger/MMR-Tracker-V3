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
                new DevAction("Open NetClient", OpenNetClient, UpdateDebugActions),
                new DevAction("Test Web Server", SendServerREquest, UpdateDebugActions),
            };

            foreach (var Function in DevFunctions)
            {
                if (Function.Conitional is not null && !Function.Conitional()) { continue; }
                listBox1.Items.Add(Function);
            }
        }

        private void SendServerREquest()
        {
            //---data to send to the server---
            string TestText = Interaction.InputBox("Send Command to Server");

            string textToSend = JsonConvert.SerializeObject(new NetData.NetPacket(2, "Password2") { TestString = TestText });
            TcpClient client = null;
            try
            {
                //---create a TCPClient object at the IP and port no.---
                client = new TcpClient("SERVER IP", 25570);
                NetworkStream nwStream = client.GetStream();
                nwStream.ReadTimeout = 1000;
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);

                //---send the text---
                Debug.WriteLine("Sending : " + textToSend);
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                //---read back the text---
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);

                string ResponseText = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);

                NetData.NetPacket ReturnPacket = new NetData.NetPacket(0);
                try { ReturnPacket = JsonConvert.DeserializeObject<NetData.NetPacket>(ResponseText); }
                catch (Exception e) { ReturnPacket = new NetData.NetPacket(0); ReturnPacket.TestString = e.Message; }

                Debug.WriteLine($"Received: Player {ReturnPacket.PlayerID} | {ReturnPacket.TestString}");
                MessageBox.Show($"Player {ReturnPacket.PlayerID} | {ReturnPacket.TestString}");
                client.Close();
            }
            catch (ArgumentNullException ane)
            {
                client?.Close();
                MessageBox.Show($"ArgumentNullException : {ane.Message}");
            }
            catch (SocketException se)
            {
                client?.Close();
                MessageBox.Show($"SocketException : {se.Message}");
            }
            catch (IOException Ie)
            {
                client?.Close();
                MessageBox.Show($"IOException : {Ie.Message}");
            }
            catch (Exception e)
            {
                client?.Close();
                MessageBox.Show($"Unexpected exception : {e.Message}");
            }
        }

        private void LB_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is DevAction DevAction) 
            {
                DevAction.Run();
            }
        }

        private void OpenNetClient()
        {
            NetClient netClient = new NetClient();
            netClient.Show();
        }
    }
}