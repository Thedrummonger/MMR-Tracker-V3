using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MMRTrackerWebServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            HttpServer server = new HttpServer("127.0.0.1", 25568);
            server.Start();
        }
    }

    public class HttpServer
    {
        private readonly IPAddress ipAddress;
        private readonly int port;
        private readonly TcpListener serverListenter;

        public HttpServer(string ipAddress, int port)
        {
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;

            this.serverListenter = new TcpListener(this.ipAddress, port);
        }

        public void Start()
        {
            while (true)
            {
                Console.WriteLine($"Server started on port {port}.");
                serverListenter.Start();

                //---incoming client connected---
                TcpClient client = serverListenter.AcceptTcpClient();

                //---get the incoming data through a network stream---
                NetworkStream nwStream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];

                //---read incoming stream---
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                //---convert the data received into a string---
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received : " + dataReceived);

                string DataToSend = TestCommand(dataReceived);

                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(DataToSend);

                //---write back the text to the client---
                Console.WriteLine("Sending back : " + DataToSend);
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                client.Close();
                serverListenter.Stop();
            }
        }

        public static string TestCommand(string Command)
        {
            switch (Command.ToLower())
            {
                case "check": return "Checking Item";
                case "mark": return "Marking Item";
                case "uncheck": return "Unchecking Item";
            }
            return "Unknown Command!";
        }
    }
}