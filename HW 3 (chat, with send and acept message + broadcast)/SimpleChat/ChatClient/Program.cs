using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Utilities;

namespace ChatClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var user_broadcast = new Client_Broadcast())
            {
                searchServers(user_broadcast, 10000);

                Console.Clear();

                var allServers = user_broadcast.getServerList();

                Console.WriteLine("Founded ports: ");
                foreach (var server in allServers)
                {
                    Console.WriteLine($"...port: {server}");
                }
                Console.WriteLine("\n");

                Console.WriteLine("Enter server port to connect: ");
                int serverPortToConnect = Convert.ToInt32(Console.ReadLine());

                var socket = ConnectClientToServer(new IPEndPoint(IPAddress.Loopback, serverPortToConnect));

                var chatContent = ReceiveChatContent(socket);
                ShowChatContent(chatContent);

                //::::::::::::::::::::::::::::::::::::::
                //Enter and send user name to server
                var name = GetClientName();
                SendNameToServer(socket, name);
                Console.Clear();
                //:::::::::::::::::::::::::::::::::::::

                //::::::::::::::::::::::::::::::::::::::
                //Enter messages and refresh chat content
                //hread refreshChatContentThr =
                //new Thread(new ThreadStart(() => { refreshChatContent(socket, chatContent); }));
                //::::::::::::::::::::::::::::::::::::::

                //::::::::::::::::::::::::::::::::::::::
                //Send messages
                Thread messageSenderThr =
                    new Thread(new ThreadStart(() => { messageWork(socket, chatContent); }));
                //::::::::::::::::::::::::::::::::::::::::

                messageSenderThr.Start();
                //refreshChatContentThr.Start();
            }
        }

        private static void messageWork(Socket socket, string chat_content)
        {
            string message = "";
            while (message != "/stop")
            {
                chat_content = ReceiveChatContent(socket);
                ShowChatContent(chat_content);

                message = GetClientMessage();
                SendMessageToServer(socket, message);
                Console.Clear();
            }
            DisconnectClientFromServer(socket);

            Thread.Sleep(TimeSpan.FromSeconds(10));

            DisposeClientSocket(socket);
        }

        //private static void refreshChatContent(Socket socket, string chatSaver)
        //{
        //    while (true)
        //    {
        //        if (socket.Available > 0)
        //        {
        //            chatSaver = ReceiveChatContent(socket);
        //            ShowChatContent(chatSaver);
        //        }
        //    }
        //}

        private static void searchServers(Client_Broadcast client, int milliseconds)
        {
            client.Start();
            Thread.Sleep(milliseconds);
            client.Stop();
        }

        private static void DisposeClientSocket(Socket socket)
        {
            socket.Close();
            socket.Dispose();
        }

        private static void DisconnectClientFromServer(Socket socket)
        {
            socket.Disconnect(false);
            Console.WriteLine("Client disconnected from server");
        }

        private static void SendNameToServer(Socket socket, string name)
        {
            Console.WriteLine("Sending Name to server");
            SocketUtility.SendString(socket, name,
                () => { Console.WriteLine($"Send string to server data check client side exception"); });
            Console.WriteLine("Name sent to server");
        }

        private static void SendMessageToServer(Socket socket, string message)
        {
            Console.WriteLine("Sending Message to server");
            SocketUtility.SendString(socket, message,
                () => { Console.WriteLine($"Send string to server data check client side exception"); });
            Console.WriteLine("Message sent to server");
        }

        private static string GetClientMessage()
        {
            Console.Write("Your message:");
            var message = Console.ReadLine();
            return message;
        }
        private static string GetClientName()
        {
            Console.Write("Your name: ");
            var user_name = Console.ReadLine();
            return user_name;
        }

        private static void ShowChatContent(string chatContent)
        {
            Console.WriteLine("---------------Chat content--------------------");
            Console.WriteLine(chatContent);
            Console.WriteLine("------------End of chat content----------------");
            Console.WriteLine();
        }

        private static string ReceiveChatContent(Socket socket)
        {
            string chatContent = SocketUtility.ReceiveString(socket,
                () => { Console.WriteLine($"Receive string size check from server client side exception"); },
                () => { Console.WriteLine($"Receive string data check from server client side exception"); });
            return chatContent;
        }

        private static Socket ConnectClientToServer(IPEndPoint serverEndPoint)
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.IP);

            socket.Connect(serverEndPoint);

            Console.WriteLine($"Client connected Local {socket.LocalEndPoint} Remote {socket.RemoteEndPoint}");

            return socket;
        }
    }
}
