using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MessageSenderServer
{
    internal class Server : IDisposable
    {
        private const int MAX_USER_CONNECTED = 7;
        public event EventHandler<Exception> AcceptClientEXP;
        public event EventHandler WaitingConnect;
        public event EventHandler<Socket> UserConnected;
        public event EventHandler<Socket> UserDisconnected;
        public event EventHandler<Socket> AcceptMessage;

        private int _port;
        private bool _server_connected;
        public List<Socket> _clients = new List<Socket> { };
        Socket s_socket;
        private Server(int port)
        {
            _port = port;
        }
        public static Server Acept(int InterPort)
        {
            return new Server(InterPort);
        }
        public void Start()
        {
            s_socket = new Socket(SocketType.Stream, ProtocolType.IP);
            s_socket.Bind(new IPEndPoint(IPAddress.Loopback, _port));
            s_socket.Listen(MAX_USER_CONNECTED);
            _server_connected = true;

            StartClientTask();
        }
        public void StartClientTask()
        {
            Task.Run(() => ClientWork());
        }
        public void ClientWork()
        {
            Socket ClientSocket = null;
            WaitingConnect?.Invoke(this, EventArgs.Empty);
            try
            {
                ClientSocket = s_socket.Accept();
            }
            catch (ObjectDisposedException ex)
            {
                AcceptClientEXP?.Invoke(this, ex);
            }
            catch (Exception ex)
            {
                AcceptClientEXP.Invoke(this, ex);
            }

            UserConnected?.Invoke(this, ClientSocket);
            StartClientTask();

            Stream st = new MemoryStream();
            byte[] buff = new byte[4096];
            st.Read(buff, 0, buff.Length);
            BinaryWriter tr = new BinaryWriter(st);
            tr.Write(DB_Chat.getChat());
            tr.Flush();
            st.Seek(0, SeekOrigin.Begin);
            st.Read(buff, 0, buff.Length);


            ClientSocket.Send(buff);

            while (ClientSocket.Available == 0)
            {
                Thread.Sleep(150);
            }
            AcceptMessage?.Invoke(this, ClientSocket);
            Thread.CurrentThread.Join();
            StartClientTask();
            //.Invoke(this, ClientSocket);
            //ClientSocket.Close();
        }
        public void AddUserMessageToDB(string message)
        {
            DB_Chat.AddMessage(message);
        }
        public void AddUserSocket(Socket user)
        {
            _clients.Add(user);
        }
        public void RemoveUserSocket(Socket user)
        {
            foreach(var users in _clients)
            {
                users.Dispose();
            }
            _clients.Remove(user);
        }
        public void Stop()
        {
            _server_connected = false;
            s_socket?.Close();
        }
        public void Dispose()
        {
            Stop();
            s_socket?.Dispose();
        }
        public Socket GetSocket
        {
            get
            {
                return this.s_socket;
            }
        }
    }
}
