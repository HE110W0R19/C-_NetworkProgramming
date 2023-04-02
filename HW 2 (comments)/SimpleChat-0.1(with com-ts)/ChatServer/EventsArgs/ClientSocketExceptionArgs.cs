using System;
using System.Net.Sockets;

namespace ChatServer.EventsArgs
{
    public class ClientSocketExceptionArgs
    {
        public Exception Exception { get; }
        public Socket ClientSocket { get; }

        public static ClientSocketExceptionArgs Create(Exception exception, Socket clientSocket)
        {
            return new ClientSocketExceptionArgs(exception, clientSocket);
        }

        private ClientSocketExceptionArgs(Exception exception, Socket clientSocket)
        {
            Exception = exception;
            ClientSocket = clientSocket;
        }
    }
}