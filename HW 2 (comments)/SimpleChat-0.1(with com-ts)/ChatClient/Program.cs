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
            //коннектим юзера к серваку
            var socket = ConnectClientToServer(new IPEndPoint(IPAddress.Loopback, 10111));

            //получаем содержимое чата в переменную
            var chatContent = ReceiveChatContent(socket);

            //Выводим содержимое чата из переменной
            ShowChatContent(chatContent);

            //получаем сообщение от юзера
            var message = GetClientMessage();

            //Кидаем наше сообщение на сокет серваку
            SendMessageToServer(socket, message);
            
            /*
             * Потенциально будет нужна в ходе дальнейшей разработки
             * В текущей версии строку ожидания Enter заменяет ожидание в
             * 1 секунду ниже
             */
            //WaitForEnterPressedToCloseApplication();

            //Отключаем юзера от сервака
            DisconnectClientFromServer(socket);
            
            //ожидаем 1 секунду
            Thread.Sleep(TimeSpan.FromSeconds(1));
            
            //отключаем сокет клиента и выходим из чата
            DisposeClientSocket(socket);
        }

        //Отключаем сокет юзера 
        private static void DisposeClientSocket(Socket socket)
        {
            socket.Close();
            socket.Dispose();
        }

        //Отключаем клиента от сервака 
        private static void DisconnectClientFromServer(Socket socket)
        {
            socket.Disconnect(false);
            Console.WriteLine("Client disconnected from server");
        }

        //Ожидаем нажатия Enter-а для выхода из чата
        private static void WaitForEnterPressedToCloseApplication()
        {
            Console.Write("Press [Enter] to close client console application");
            Console.ReadLine();
        }

        //Метод для отправки сообщения на сервак
        private static void SendMessageToServer(Socket socket, string message)
        {
            Console.WriteLine("Sending message to server");
            SocketUtility.SendString(socket, message,
                () => { Console.WriteLine($"Send string to server data check client side exception"); });
            Console.WriteLine("Message sent to server");
        }

        //Метод для ввода сообщения с консоли
        private static string GetClientMessage()
        {
            Console.Write("Your message:");
            var message = Console.ReadLine();
            return message;
        }

        //Вывод содержимого чата
        private static void ShowChatContent(string chatContent)
        {
            Console.WriteLine("---------------Chat content--------------------");
            Console.WriteLine(chatContent);
            Console.WriteLine("------------End of chat content----------------");
            Console.WriteLine();
        }

        //получаем содержимое чата
        private static string ReceiveChatContent(Socket socket)
        {
            string chatContent = SocketUtility.ReceiveString(socket,
                () => { Console.WriteLine($"Receive string size check from server client side exception"); },
                () => { Console.WriteLine($"Receive string data check from server client side exception"); });
            return chatContent;
        }

        //коннектим юзера на сервак
        private static Socket ConnectClientToServer(IPEndPoint serverEndPoint)
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.IP);//создаем соет юзера 
            
            socket.Connect(serverEndPoint);//коннектим сокед через эндпоинт сервака

            //выводим инфу о подключении
            Console.WriteLine($"Client connected Local {socket.LocalEndPoint} Remote {socket.RemoteEndPoint}");
            
            return socket;
        }
    }
}
