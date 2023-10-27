using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class NetData
    {
        public const int DefaultProgramPort = 25570;
        public enum ConnectionResult
        {
            Success,
            Blacklisted,
            NonWhitelisted,
            Unknown
        }
        public class NetPacket
        {
            public NetPacket(int _playerID, string _Password = "", Dictionary<int, Dictionary<string, int>> _ItemData = null)
            {
                PlayerID = _playerID;
                Password = _Password;
                ItemData = _ItemData;
            }
            public int PlayerID;
            public string Password;
            //Dictionary<PlayerID, Dictionary<ItemID, ItemAmount>>
            public Dictionary<int,Dictionary<string, int>> ItemData;
            public string TestString = "Hello";
        }
        public class ConfigFile
        {
            public static string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recources", "ServerConfig.cfg");
            public IPAddress IPAddress = IPAddress.Any;
            public int Port = DefaultProgramPort;
            public List<IPAddress> IPBlacklist = new List<IPAddress>();
            public List<IPAddress> IPWhitelist = new List<IPAddress>();
            public bool PlayersRequirePassword = false;
            public Dictionary<int, string> playerPasswords = new Dictionary<int, string>();
            public ConfigFile SetDefaultExamples()
            {
                IPBlacklist.Add(System.Net.IPAddress.Parse("8.8.8.8"));
                IPBlacklist.Add(System.Net.IPAddress.Parse("8.8.4.4"));
                for (var i = 1; i <= 4; i++)
                {
                    playerPasswords.Add(i, $"Password{i}");
                }
                return this;
            }
            public static void VerifyConfig()
            {
                if (!File.Exists(ConfigFilePath)) { WriteNewConfig(); }
                else
                {
                    try { _ = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(ConfigFilePath), MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions); }
                    catch { WriteNewConfig(); }
                }
            }

            public static void WriteNewConfig()
            {
                ConfigFile configFile = new ConfigFile().SetDefaultExamples();
                File.WriteAllText(ConfigFilePath, MMR_Tracker_V3.Utility.ToFormattedJson(configFile));
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
            IP = null;
            var Segments = Input.Split('.');
            if (Segments.Length != 4) { return false; }
            if (!IPAddress.TryParse(Input, out IP)) { return false; }
            return true;
        }
    }
}
