using System.Net.Sockets;

namespace ChatServer.EventsArgs
{
    public class ServerSocketEventArgs
    {
        public Socket ServerSocket { get; }
        
        public static ServerSocketEventArgs Create(Socket serverSocket)
        {
            return new ServerSocketEventArgs(serverSocket);
        }
        
        private ServerSocketEventArgs(Socket serverSocket)
        {
            ServerSocket = serverSocket;
        }

        
    }
}