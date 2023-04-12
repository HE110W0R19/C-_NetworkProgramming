using ChatServer.EventsArgs;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Utilities;

namespace ChatServer
{
    internal class Server : IDisposable
    {
        private const int MAX_CLIENTS_WAITING_FOR_CONNECT = 5;

        public event EventHandler<Exception> AcceptClientException;
        public event EventHandler<ClientSocketExceptionArgs> SendDataToClientException;
        public event EventHandler<ClientSocketExceptionArgs> ReceiveDataFromClientException;
        public event EventHandler<ClientSocketEventArgs> ChatContentSentToClient;
        public event EventHandler<ServerSocketEventArgs> WaitingForClientConnect;
        public event EventHandler<ClientSocketEventArgs> WaitingDataFromClient;
        public event EventHandler<ClientConnectedArgs> ClientConnected;
        public event EventHandler<string> ClientMessageReceived;
        public event EventHandler<string> ClientNameReceived;
        public event EventHandler<ClientSocketEventArgs> ClientDisconnected;

        private List<Socket> _clientSockets = new List<Socket>();

        private int _serverPort;
        private Socket _serverSocket;
        private bool _isServerAlive;

        public static Server Initialise(int listeningPort)
        {
            return new Server(listeningPort);
        }

        private Server(int serverPort)
        {
            _serverPort = serverPort;
        }

        public void Start()
        {
            _serverSocket = new Socket(SocketType.Stream, ProtocolType.IP);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, _serverPort));
            _serverSocket.Listen(MAX_CLIENTS_WAITING_FOR_CONNECT);
            _isServerAlive = true;

            StartClientTask();
        }

        private void StartClientTask()
        {
            Task.Run(() => ClientWorker());
        }

        public void Stop()
        {
            _isServerAlive = false;
            _serverSocket.Close();
        }

        public void Dispose()
        {
            Stop();
            _serverSocket?.Dispose();
        }

        private void ClientWorker()
        {
            Socket clientSocket = AcceptClient();
            _clientSockets.Add(clientSocket);

            string chatMessage = "";

            SendChatContentToClient(clientSocket);
            WaitForDataFromClientAvailable(clientSocket);
            var chatName = ReceiveChatUserNameFromClient(clientSocket);

            while (chatMessage != "/stop")
            {
                StartClientTask();
                SendChatContentToClient(clientSocket);
                WaitForDataFromClientAvailable(clientSocket);
                chatMessage = ReceiveChatMessageFromClient(clientSocket);
                ChatDatabase.AddMessage(chatName, chatMessage);

                SendChatContentToClient(clientSocket);
            }
            DisconnectClient(clientSocket);
        }

        private void DisconnectClient(Socket clientSocket)
        {
            clientSocket.Disconnect(false);
            ClientDisconnected?.Invoke(this, ClientSocketEventArgs.Create(clientSocket));
            _clientSockets?.Remove(clientSocket);
            clientSocket.Close();
            clientSocket.Dispose();
        }

        private string ReceiveChatMessageFromClient(Socket clientSocket)
        {
            var chatMessage = SocketUtility.ReceiveString(clientSocket, () =>
                {
                    ReceiveDataFromClientException?.Invoke(this,
                        ClientSocketExceptionArgs.Create(
                            new Exception("Retrieving string size from socket check fail"),
                            clientSocket
                        )
                    );
                },
                () =>
                {
                    ReceiveDataFromClientException?.Invoke(this,
                        ClientSocketExceptionArgs.Create(
                            new Exception("Retrieving string from socket check fail"),
                            clientSocket
                        )
                    );
                });
            ClientMessageReceived?.Invoke(this, chatMessage);
            return chatMessage;
        }
        private string ReceiveChatUserNameFromClient(Socket clientSocket)
        {
            var chatName = SocketUtility.ReceiveString(clientSocket, () =>
            {
                ReceiveDataFromClientException?.Invoke(this,
                    ClientSocketExceptionArgs.Create(
                        new Exception("Retrieving string size from socket check fail"),
                        clientSocket
                    )
                );
            },
                () =>
                {
                    ReceiveDataFromClientException?.Invoke(this,
                        ClientSocketExceptionArgs.Create(
                            new Exception("Retrieving string from socket check fail"),
                            clientSocket
                        )
                    );
                });
            ClientNameReceived?.Invoke(this, chatName);
            return chatName;
        }

        private void WaitForDataFromClientAvailable(Socket clientSocket)
        {
            WaitingDataFromClient?.Invoke(this, ClientSocketEventArgs.Create(clientSocket));
            SocketUtility.WaitDataFromSocket(clientSocket);
        }

        private void SendChatContentToClient(Socket clientSocket)
        {
            SocketUtility.SendString(clientSocket, ChatDatabase.GetChat(),
                () =>
                {
                    SendDataToClientException?.Invoke(this,
                        ClientSocketExceptionArgs.Create(
                            new Exception("Preparation data for socket send check fail"),
                            clientSocket
                        )
                    );
                });
            ChatContentSentToClient?.Invoke(this, ClientSocketEventArgs.Create(clientSocket));
        }

        private Socket AcceptClient()
        {

            Socket clientSocket = null;

            WaitingForClientConnect?.Invoke(this, ServerSocketEventArgs.Create(_serverSocket));

            try
            {
                clientSocket = _serverSocket.Accept();
            }
            catch (SocketException ex)
            {
                AcceptClientException?.Invoke(this, ex);
            }
            catch (ObjectDisposedException ex)
            {
                AcceptClientException?.Invoke(this, ex);
            }
            catch (InvalidOperationException ex)
            {
                AcceptClientException?.Invoke(this, ex);
            }

            ClientConnected?.Invoke(this, ClientConnectedArgs.Create(_serverSocket, clientSocket));

            return clientSocket;
        }
    }
}
