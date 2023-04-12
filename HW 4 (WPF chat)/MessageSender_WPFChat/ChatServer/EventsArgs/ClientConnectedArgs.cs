using System.Net.Sockets;

namespace ChatServer.EventsArgs
{
    public class ClientConnectedArgs
    {
        public Socket ServerSocket { get; }
        public Socket ClientSocket { get; }
        
        public static ClientConnectedArgs Create(Socket serverSocket, Socket clientSocket)
        {
            return new ClientConnectedArgs(serverSocket, clientSocket);
        }
        
        private ClientConnectedArgs(Socket serverSocket, Socket clientSocket)
        {
            ServerSocket = serverSocket;
            ClientSocket = clientSocket;
        }
    }
}