using ChatClient;
using ChatServer.EventsArgs;
using System;
using System.Net;

namespace ChatServer
{
    internal class Program
    {
        public static int _serverPort;
        static void Main(string[] args)
        {
            Console.WriteLine("Enter server port: ");
            _serverPort = Convert.ToInt32(Console.ReadLine());
            ServerService _serverService = new ServerService(TimeSpan.FromSeconds(10));
            using (var _serverLocatorService = new ServerLocatorService(_serverService.TcpPort))
            {
                using (var server = Server.Initialise(_serverPort))
                {
                    server.AcceptClientException += Server_AcceptClientException;
                    server.WaitingForClientConnect += Server_WaitingForClientConnect;
                    server.ClientConnected += Server_ClientConnected;
                    server.SendDataToClientException += Server_SendDataToClientException;
                    server.ChatContentSentToClient += Server_ChatContentSentToClient;
                    server.WaitingDataFromClient += Server_WaitingDataFromClient;
                    server.ReceiveDataFromClientException += Server_ReceiveDataFromClientException;
                    server.ClientMessageReceived += ServerClientMessageReceived;
                    server.ClientNameReceived += ServerClientNameReceived;
                    server.ClientDisconnected += Server_ClientDisconnected;
                    server.Start();

                    _serverLocatorService.Start();

                    Console.ReadLine();
                    _serverLocatorService.Stop();
                    server.Stop();
                }
            }
        }

        private static void Server_ClientDisconnected(object sender, ClientSocketEventArgs e)
        {
            Console.WriteLine("Client with " +
                              $"local IP v4 address {GetIpV4Address(e.ClientSocket.LocalEndPoint)} " +
                              $"and remote IP v4 address {GetIpV4Address(e.ClientSocket.RemoteEndPoint)} disconnected.");
        }

        private static void ServerClientMessageReceived(object sender, string e)
        {
            Console.WriteLine($"Client message [{e}] received");
        }
        private static void ServerClientNameReceived(object sender, string e)
        {
            Console.WriteLine($"Client name [{e}] received");
        }

        private static void Server_ReceiveDataFromClientException(object sender, ClientSocketExceptionArgs e)
        {
            Console.WriteLine("Receiving data from client with " +
                              $"local IP v4 address {GetIpV4Address(e.ClientSocket.LocalEndPoint)} " +
                              $"and remote IP v4 address {GetIpV4Address(e.ClientSocket.RemoteEndPoint)} " +
                              $"caused exception [{e.Exception.Message}] on server side.");
        }

        private static void Server_WaitingDataFromClient(object sender, ClientSocketEventArgs e)
        {
            Console.WriteLine("Waiting data from client with " +
                              $"local IP v4 address {GetIpV4Address(e.ClientSocket.LocalEndPoint)} " +
                              $"and remote IP v4 address {GetIpV4Address(e.ClientSocket.RemoteEndPoint)}.");
        }

        private static void Server_ChatContentSentToClient(object sender, ClientSocketEventArgs e)
        {
            Console.WriteLine("Chat content sent to client with " +
                              $"local IP v4 address {GetIpV4Address(e.ClientSocket.LocalEndPoint)} " +
                              $"and remote IP v4 address {GetIpV4Address(e.ClientSocket.RemoteEndPoint)}.");
        }

        private static void Server_SendDataToClientException(object sender, ClientSocketExceptionArgs e)
        {
            Console.WriteLine("Send data to client with " +
                              $"local IP v4 address {GetIpV4Address(e.ClientSocket.LocalEndPoint)} " +
                              $"and remote IP v4 address {GetIpV4Address(e.ClientSocket.RemoteEndPoint)} " +
                              $"caused exception [{e.Exception.Message}] on server side.");
        }

        private static void Server_ClientConnected(object sender, ClientConnectedArgs e)
        {
            Console.WriteLine($"Client with local IP v4 address {GetIpV4Address(e.ClientSocket.LocalEndPoint)} " +
                              $"and remote IP v4 address {GetIpV4Address(e.ClientSocket.RemoteEndPoint)} " +
                              $"connected to server with local IP v4 address {GetIpV4Address(e.ServerSocket.LocalEndPoint)}.");
        }

        private static string GetIpV4Address(EndPoint endPoint)
        {
            var ipEndPoint = (IPEndPoint)endPoint;
            var ip = ipEndPoint.Address.MapToIPv4().ToString();
            var port = ipEndPoint.Port;
            return $"[{ip}]:{port}";
        }

        private static void Server_WaitingForClientConnect(object sender, ServerSocketEventArgs e)
        {
            Console.WriteLine($"Server with local IP v4 address {GetIpV4Address(e.ServerSocket.LocalEndPoint)} " +
                              "waiting for client connection.");
        }

        private static void Server_AcceptClientException(object sender, Exception e)
        {
            Console.WriteLine($"Server caused exception while client accept [{e.Message}].");
        }
    }
}
