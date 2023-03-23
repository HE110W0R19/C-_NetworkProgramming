using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChatUser
{
    internal class ChatUser : IDisposable
    {
        private string _username;
        private int _port;
        private bool _connected;
        private Socket _UserSocket;
        public string UserMess = "Hello World!";
        private ChatUser(int port, string UserName)
        {
            _port = port;
            _username = UserName;
            _UserSocket = new Socket(SocketType.Stream, ProtocolType.IP);
        }
        public static ChatUser CreateUser(int port, string UserName)
        {
            return new ChatUser(port, UserName);
        }
        public void AcceptMessage()
        {
            byte[] buffer = new byte[4096];
            var chatBytes = _UserSocket.Receive(buffer);
            var ms = new MemoryStream();
            ms.Write(buffer, 0, chatBytes);
            TextReader tr = new StreamReader(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var chatContent = tr.ReadToEnd();

            Console.WriteLine(chatContent.TrimEnd('\0'));
            //_UserSocket.Send(System.Text.Encoding.ASCII.
            //    GetBytes(UserMess));
        }
        public void SendMessage() 
        {
            Stream st = new MemoryStream();
            byte[] buff = new byte[4096];
            BinaryWriter tr = new BinaryWriter(st);
            Console.WriteLine();
            string message = Convert.ToString(Console.ReadLine());
            tr.Write(message);
            tr.Flush();
            st.Seek(0, SeekOrigin.Begin);
            st.Read(buff, 0, buff.Length);
            _UserSocket.Send(buff);

            DisconnectUser(_UserSocket);
        }
        public void DisconnectUser(Socket user_socket)
        {
            user_socket.Disconnect(false);
            Console.WriteLine("You disconnected~");
        }
        public void StopUser()
        {
            _connected = false;
            _UserSocket?.Close();
        }
        public void ConnectUser()
        {
            _connected = true;
            _UserSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));
        }
        public void Dispose()
        {
            StopUser();
            _UserSocket?.Dispose();
        }

        static void Main(string[] args)
        {
            using (var user = ChatUser.CreateUser(18181, "User_1"))
            {
                user.ConnectUser();
                Console.WriteLine(user._UserSocket.ToString());
                Thread UserThread = new Thread(new ThreadStart(() =>
                {
                    user.AcceptMessage();
                    user.SendMessage();
                }));
                UserThread.Start();
                
                Console.ReadKey();
                Thread.Sleep(20000);
                user.StopUser();
                UserThread.Abort();
            }
        }
    }
}
