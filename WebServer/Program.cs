using MathNet.Symbolics;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MMR_Tracker_V3.TrackerObjects;

namespace WebServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //The the config file values
            NetData.ConfigFile.VerifyConfig();
            NetData.ConfigFile cfg = JsonConvert.DeserializeObject<NetData.ConfigFile>(File.ReadAllText(NetData.ConfigFile.ConfigFilePath), MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions)??new NetData.ConfigFile().SetDefaultExamples();

            //If the program was started with arguments, scan then for an IP and/or Port and override the config if found
            NetData.ParseNetServerArgs(args, out IPAddress? ArgsIP, out int ArgsPort);
            if (ArgsIP is not null) { cfg.IPAddress = ArgsIP; }
            if (ArgsPort > -1) { cfg.Port = ArgsPort; }

            //Sanitize the IPAdress and Port
            if (cfg.IPAddress is null || cfg.IPAddress.ToString() == "0.0.0.0") { cfg.IPAddress = IPAddress.Any; }
            if (cfg.Port < 0) { cfg.Port = NetData.DefaultProgramPort; }

            //Create and start the server
            HttpServer server = new HttpServer(cfg);
            server.Start();
        }
    }
}