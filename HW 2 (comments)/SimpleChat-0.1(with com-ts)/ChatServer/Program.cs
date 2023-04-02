using System;
using System.Net;
using ChatServer.EventsArgs;

namespace ChatServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Создаем сервак с портом 10111
            using (var server = Server.Initialise(10111))
            {
                //Присваеваем методы к ивентам
                server.AcceptClientException += Server_AcceptClientException;
                server.WaitingForClientConnect += Server_WaitingForClientConnect;
                server.ClientConnected += Server_ClientConnected;
                server.SendDataToClientException += Server_SendDataToClientException;
                server.ChatContentSentToClient += Server_ChatContentSentToClient;
                server.WaitingDataFromClient += Server_WaitingDataFromClient;
                server.ReceiveDataFromClientException += Server_ReceiveDataFromClientException;
                server.ClientMessageReceived += ServerClientMessageReceived;
                server.ClientDisconnected += Server_ClientDisconnected;
                server.Start(); //запускаем сервак

                Console.ReadLine(); //Читаем строку чтобы сервак не накрывался сразу
                server.Stop(); // Стопаем сервак
            }
        }

        //Выводим инфу о отключенном юзере
        private static void Server_ClientDisconnected(object sender, ClientSocketEventArgs e)
        {
            Console.WriteLine("Client with " +
                              $"local IP v4 address {GetIpV4Address(e.ClientSocket.LocalEndPoint)} " +
                              $"and remote IP v4 address {GetIpV4Address(e.ClientSocket.RemoteEndPoint)} disconnected.");
        }

        //Выводим инфу о полученном сообщении от клиента 
        private static void ServerClientMessageReceived(object sender, string e)
        {
            Console.WriteLine($"Client message [{e}] received");
        }

        //Выводим инфу о полученных данниых от клиена
        private static void Server_ReceiveDataFromClientException(object sender, ClientSocketExceptionArgs e)
        {
            Console.WriteLine("Receiving data from client with " +
                              $"local IP v4 address {GetIpV4Address(e.ClientSocket.LocalEndPoint)} " +
                              $"and remote IP v4 address {GetIpV4Address(e.ClientSocket.RemoteEndPoint)} " +
                              $"caused exception [{e.Exception.Message}] on server side.");
        }

        //Выводим инфу о ожидании данных от клиента
        private static void Server_WaitingDataFromClient(object sender, ClientSocketEventArgs e)
        {
            Console.WriteLine("Waiting data from client with " +
                              $"local IP v4 address {GetIpV4Address(e.ClientSocket.LocalEndPoint)} " +
                              $"and remote IP v4 address {GetIpV4Address(e.ClientSocket.RemoteEndPoint)}.");
        }

        //Выводим инфу о отправленном содержимом чата
        private static void Server_ChatContentSentToClient(object sender, ClientSocketEventArgs e)
        {
            Console.WriteLine("Chat content sent to client with " +
                              $"local IP v4 address {GetIpV4Address(e.ClientSocket.LocalEndPoint)} " +
                              $"and remote IP v4 address {GetIpV4Address(e.ClientSocket.RemoteEndPoint)}.");
        }

        //Выводим инфу о пойманном эксепшине от получения данных от клиента
        private static void Server_SendDataToClientException(object sender, ClientSocketExceptionArgs e)
        {
            Console.WriteLine("Send data to client with " +
                              $"local IP v4 address {GetIpV4Address(e.ClientSocket.LocalEndPoint)} " +
                              $"and remote IP v4 address {GetIpV4Address(e.ClientSocket.RemoteEndPoint)} " +
                              $"caused exception [{e.Exception.Message}] on server side.");
        }

        //Выводим инфу о подключившемся клиенте
        private static void Server_ClientConnected(object sender, ClientConnectedArgs e)
        {
            Console.WriteLine($"Client with local IP v4 address {GetIpV4Address(e.ClientSocket.LocalEndPoint)} " +
                              $"and remote IP v4 address {GetIpV4Address(e.ClientSocket.RemoteEndPoint)} " +
                              $"connected to server with local IP v4 address {GetIpV4Address(e.ServerSocket.LocalEndPoint)}.");
        }

        //Получаем айпишник из эндпоинта
        private static string GetIpV4Address(EndPoint endPoint)
        {
            var ipEndPoint = (IPEndPoint)endPoint;
            var ip = ipEndPoint.Address.MapToIPv4().ToString();
            var port = ipEndPoint.Port;
            return $"[{ip}]:{port}";
        }

        //Выводим статус сервака (Ожидание подключения) 
        private static void Server_WaitingForClientConnect(object sender, ServerSocketEventArgs e)
        {
            Console.WriteLine($"Server with local IP v4 address {GetIpV4Address(e.ServerSocket.LocalEndPoint)} " +
                              "waiting for client connection.");
        }

        //Выводим инфу о ошибки получении юзенра
        private static void Server_AcceptClientException(object sender, Exception e)
        {
            Console.WriteLine($"Server caused exception while client accept [{e.Message}].");
        }
    }
}
