using System.Net.Sockets;

namespace ChatServer.EventsArgs
{
    public class ClientSocketEventArgs
    {
        public Socket ClientSocket { get; }
        
        public static ClientSocketEventArgs Create(Socket clientSocket)
        {
            return new ClientSocketEventArgs(clientSocket);
        }
        
        private ClientSocketEventArgs(Socket clientSocket)
        {
            ClientSocket = clientSocket;
        }

        
    }
}