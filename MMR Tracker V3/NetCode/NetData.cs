using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using static MMR_Tracker_V3.TrackerObjects.HintData;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;

namespace MMR_Tracker_V3.NetCode
{
    public class NetData
    {
        public const int DefaultProgramPort = 25570;
        public enum PacketType
        {
            Handshake,
            OnlineSynedLocations,
            MultiWorldItems,
            ChatMessage
        }
        public enum OnlineMode
        {
            [Description("None")]
            None = 0,
            [Description("Co-op")]
            Coop = 1,
            [Description("Multiworld")]
            Multiworld = 3,
            [Description("Archipelago")]
            Archipelago = 4
        }
        public class NetSessionData(
            string _ServerAddress, int _ServerPort,
            int _PlayerID, string _SlotID, string _GameName,
            string _Password,
            bool _ReceiveData, bool _SendData, bool _AutoProcessData, bool _AllowLocationChecking,
            Action<string, object[]> _Logger, Action _RefreshNetForm, Action _RefreshMainForm,
            InstanceContainer _InstanceContainer
        )
        {
            public string ServerAddress = _ServerAddress;
            public int ServerPort = _ServerPort;
            public int PlayerID = _PlayerID;
            public string SlotID = _SlotID;
            public string GameName = _GameName;
            public string Password = _Password;
            public bool ReceiveData = _ReceiveData;
            public bool SendData = _SendData;
            public bool AutoProcessData = _AutoProcessData;
            public bool AllowLocationChecking = _AllowLocationChecking;
            public Action<string, object[]> Logger = _Logger;
            public Action Refresh = _RefreshNetForm;
            public Action RefreshMainForm = _RefreshMainForm;
            public InstanceContainer InstanceContainer = _InstanceContainer;

            public void UpdateParams(
            string _ServerAddress, int _ServerPort,
            int _PlayerID, string _SlotID, string _GameName,
            string _Password,
            bool _ReceiveData, bool _SendData, bool _AutoProcessData, bool _AllowLocationChecking)
            {
                ServerAddress = _ServerAddress;
                ServerPort = _ServerPort;
                PlayerID = _PlayerID;
                SlotID = _SlotID;
                GameName = _GameName;
                Password = _Password;
                ReceiveData = _ReceiveData;
                SendData = _SendData;
                AutoProcessData = _AutoProcessData;
                AllowLocationChecking = _AllowLocationChecking;
            }
        }
        public class NetPacket(int _playerID, PacketType _packetType, string _Password = "", Dictionary<int, Dictionary<string, int>> _ItemData = null, NetData.OnlineMode Mode = OnlineMode.None)
        {
            public int PlayerID = _playerID;
            public string Password = _Password;
            public PacketType packetType = _packetType;
            public MiscData.CheckState ClientCheckAction;
            public int[] UpdateWhitelist = null;
            public Dictionary<string, string> LocationData = new Dictionary<string, string>();
            //Dictionary<PlayerID, Dictionary<ItemID, ItemAmount>>
            public Dictionary<int, Dictionary<string, int>> ItemData = _ItemData;
            public ChatMessage ChatMessage = null;
            public HandshakeResponse HandshakeResponse = null;

        }
        public class ChatMessage
        {
            public ChatMessage(int _PlayerID, Guid _guid, string _Message = "")
            {
                PlayerID = _PlayerID;
                guid = _guid;
                Message = _Message;
            }
            public int PlayerID;
            public Guid guid;
            public string Message;
        }
        public class ServerClient
        {
            public Guid ClientID;
            [JsonIgnore]
            public TcpClient NetClient;
            public NetPacket Handshake;
            [JsonIgnore]
            public IPEndPoint EndPoint;
            public int PlayerID;
            public OnlineMode ClientMode;
            public Dictionary<string, string> OnlineLocationData;
            public Dictionary<int, Dictionary<string, int>> MultiworldItemData;
            public IPAddress GetIP() { return EndPoint?.Address; }
            public int? GetPort() { return EndPoint?.Port; }
        }
        public class HandshakeResponse
        {
            public Guid ClientID;
            public int PlayerID;
            public OnlineMode ClientMode;
            public bool ConnectionSuccess;
            public string ConnectionStatus;
            public int[] ConnectedPlayers;
        }
        public class ConfigFile
        {
            public static string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recources", "ServerConfig.cfg");
            public IPAddress IPAddress = IPAddress.Any;
            public int Port = DefaultProgramPort;
            public OnlineMode ServerGameMode = OnlineMode.None;
            public List<IPAddress> IPBlacklist = new List<IPAddress>();
            public List<IPAddress> IPWhitelist = new List<IPAddress>();
            public bool RequireLogin = false;
            public Dictionary<int, string> UserLogins = new Dictionary<int, string>();
            public ConfigFile SetDefaultExamples()
            {
                IPBlacklist.Add(IPAddress.Parse("8.8.8.8"));
                IPBlacklist.Add(IPAddress.Parse("8.8.4.4"));
                for (var i = 1; i <= 4; i++)
                {
                    UserLogins.Add(i, $"Password{i}");
                }
                return this;
            }
            public static void VerifyConfig()
            {
                if (!File.Exists(ConfigFilePath)) { WriteNewConfig(); }
                else
                {
                    try { _ = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(ConfigFilePath), Utility.DefaultSerializerSettings); }
                    catch { WriteNewConfig(); }
                }
            }

            public static void WriteNewConfig()
            {
                ConfigFile configFile = new ConfigFile().SetDefaultExamples();
                File.WriteAllText(ConfigFilePath, configFile.ToFormattedJson());
            }
        }

        public class NetConnection
        {
            public NetData.OnlineMode OnlineMode { get; set; } = NetData.OnlineMode.None;
            public TcpClient ServerConnection { get; set; } = null;
            public ArchipelagoConnector ArchipelagoClient { get; set; } = null;
            public int PlayerID { get; set; } = -1;
            public string SlotID { get; set; } = string.Empty;
            public string GameName { get; set; } = string.Empty;
            public Dictionary<int, string> PlayerNames { get; set; } = [];
            public List<RemoteLocationHint> RemoteHints { get; set; } = [];
            public void Reset()
            {
                OnlineMode = NetData.OnlineMode.None;
                ServerConnection = null;
                ArchipelagoClient = null;
                PlayerID = -1;
                SlotID = string.Empty;
                GameName = string.Empty;
            }
            public bool IsConnected()
            {
                bool ServerConnected = ServerConnection is not null && ServerConnection.Connected;
                bool ArchipelagoConnected = ArchipelagoClient is not null && ArchipelagoClient.WasConnectionSuccess(out _);

                return ServerConnected || ArchipelagoConnected;
            }
        }
        public static void ParseNetServerArgs(string[] args, out IPAddress IP, out int Port)
        {
            IP = null;
            Port = -1;
            if (args.Length < 1) { return; }
            foreach (var arg in args)
            {
                if (IP is null && IsIpAddress(arg, out IPAddress ArgIP)) { IP = ArgIP; }
                if (Port < 0 && int.TryParse(arg, out int ArgPort)) { Port = ArgPort; }
                if (IP is not null && Port > -1) { return; }
            }
        }

        public static bool IsIpAddress(string Input, out IPAddress IP)
        {
            bool WasIP = true;
            IP = null;
            var Segments = Input.Split('.');
            if (Segments.Length != 4) { WasIP = false; }
            if (!IPAddress.TryParse(Input, out IP)) { WasIP = false; }
            if (!WasIP)
            {
                IPAddress[] addresslist = Dns.GetHostAddresses(Input);
                if (addresslist.Length != 0)
                {
                    WasIP = true;
                    IP = addresslist.First();
                }
            }
            return WasIP;
        }
    }
}
