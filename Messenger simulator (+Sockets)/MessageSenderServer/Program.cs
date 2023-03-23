using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace MessageSenderServer
{
    internal class Program
    {
        static Server serv = null;
        public static event EventHandler<string> AcceptMessage;
        static void Main(string[] args)
        {
            using (serv = Server.Acept(18181))
            {
                serv.AcceptClientEXP += Serv_AcceptClientEXP;
                serv.WaitingConnect += Serv_WaitingConnect;
                serv.UserConnected += Serv_UserConnected;
                serv.UserDisconnected += Serv_UserDisconnected;
                serv.AcceptMessage += Serv_AcceptMessage;

                serv.Start();
                serv.ClientWork();
                Console.ReadKey();
                serv.Stop();
            }
        }

        private static void Serv_AcceptMessage(object sender, Socket e)
        {
            byte[] buffer = new byte[4096];
            var CharByte = e.Receive(buffer);
            if (CharByte != 0)
            {
                var MS = new MemoryStream();
                MS.Write(buffer, 0, CharByte);
                TextReader textReader = new StreamReader(MS);
                MS.Seek(0, SeekOrigin.Begin);
                var ChatContent = textReader.ReadToEnd();
                Console.WriteLine($"Chat content: {ChatContent.TrimEnd('\0')}");
                DB_Chat.AddMessage(ChatContent.TrimEnd('\0'));
            }
        }

        private static void Serv_WaitingConnect(object sender, EventArgs e)
        {
            Console.WriteLine("Server waiting connect!");
        }

        private static void Serv_UserConnected(object sender, Socket e)
        {
            serv.AddUserSocket(e);
            Console.WriteLine($"User: {e.RemoteEndPoint} - connected!");
        }

        private static void Serv_UserDisconnected(object sender, Socket e)
        {
            serv.RemoveUserSocket(e);
            Console.WriteLine($"User: {e.RemoteEndPoint} - disconnected!");
        }

        private static void Serv_AcceptClientEXP(object sender, Exception e)
        {
            Console.WriteLine("Client Exception! " + e.ToString());
        }
    }
}
