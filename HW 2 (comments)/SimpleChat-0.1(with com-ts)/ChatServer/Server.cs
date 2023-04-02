using ChatServer.EventsArgs;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Utilities;

namespace ChatServer
{
    internal class Server : IDisposable
    {
        //Максимальное кол-во юзеров, которые могут ожидать подключение 
        private const int MAX_CLIENTS_WAITING_FOR_CONNECT = 5;

        //vvv Событие для обработки ошибок клиента
        public event EventHandler<Exception> AcceptClientException;
        //vvv Событие для обработки ошибки с отправкой данных клиенту
        public event EventHandler<ClientSocketExceptionArgs> SendDataToClientException;
        //vvv Событие для обработки ошибки с получением данных от клиента
        public event EventHandler<ClientSocketExceptionArgs> ReceiveDataFromClientException;
        //vvv Событие для отправки содержимого чата клиенту
        public event EventHandler<ClientSocketEventArgs> ChatContentSentToClient;
        //vvv Событие для ожидания подключения клиента к серваку 
        public event EventHandler<ServerSocketEventArgs> WaitingForClientConnect;
        //vvv Событие для ожидания данных от клиента
        public event EventHandler<ClientSocketEventArgs> WaitingDataFromClient;
        //vvv Событие для реакции на подключение клиента
        public event EventHandler<ClientConnectedArgs> ClientConnected;
        //vvv Событие на получение сообщения от клиента
        public event EventHandler<string> ClientMessageReceived;
        //vvv Событие на отключение клиента
        public event EventHandler<ClientSocketEventArgs> ClientDisconnected;

        private int _serverPort; //Порт сервака
        private Socket _serverSocket; //Сокет сервака
        private bool _isServerAlive; //Работает ли сервак 

        //vvv Статический метод который принимает порт и создает сокет
        public static Server Initialise(int listeningPort)
        {
            return new Server(listeningPort);
        }

        //vvv Конструктор Сервака (получает порт)
        private Server(int serverPort)
        {
            _serverPort = serverPort;
        }

        //Метод для запуска сервака
        public void Start()
        {
            _serverSocket = new Socket(SocketType.Stream, ProtocolType.IP); //Инициализация сокета сервака
            _serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, _serverPort)); //Биндим сокет сервака 
            _serverSocket.Listen(MAX_CLIENTS_WAITING_FOR_CONNECT); //Ставим прослушивание и указываем максимальное кол-во клиентов
            _isServerAlive = true; //Указываем что сервак работает

            //вызываем метод для работы с клиентом после получения коннекта
            StartClientTask(); 
        }

        // Сам метод для работы с клиентом
        private void StartClientTask()
        {
            Task.Run(() => ClientWorker());
        }

        // Остановка работы сервака
        public void Stop()
        {
            _isServerAlive = false; //Ставим значение что сервак не"живой"
            _serverSocket.Close();
        }
        //Перегружаем Dispose из IDisposable
        public void Dispose()
        {
            Stop(); //Вызываем метод для остановки сервера 
            _serverSocket?.Dispose(); //Кидаем сокет сервака в лопасти garbage collector-а :)
        }

        private void ClientWorker()
        {
            //Принимаем сокет клиента
            Socket clientSocket = AcceptClient();

            StartClientTask();//Запускаем работу с клиентом

            //Отправляем содержимое чата клиенту
            SendChatContentToClient(clientSocket);
            //Ожидаем сообщение от клиента
            WaitForDataFromClientAvailable(clientSocket);
            //Записываем сообщение клиента в переменную 
            var chatMessage = ReceiveChatMessageFromClient(clientSocket);
            //добавляем сообщение в БД
            ChatDatabase.AddMessage(chatMessage);
            //Отключаем одноразового клиента 
            DisconnectClient(clientSocket);
        }

        //Метод для отключки клиента 
        private void DisconnectClient(Socket clientSocket)
        {
            clientSocket.Disconnect(false); //Закрываем подключение сокета 
            ClientDisconnected?.Invoke(this, ClientSocketEventArgs.Create(clientSocket));
            clientSocket.Close(); //закрываем сокет клиента 
            clientSocket.Dispose(); //Избавляемся от сокета клиента 
        }

        //Получаем сообщение от клиента
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

        //Ожидаем данные от клиента 
        private void WaitForDataFromClientAvailable(Socket clientSocket)
        {
            WaitingDataFromClient?.Invoke(this, ClientSocketEventArgs.Create(clientSocket));
            SocketUtility.WaitDataFromSocket(clientSocket);
        }

        //Отправляем содержимое чата клиенту
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

        //Принимаем сокет клиента
        private Socket AcceptClient()
        {

            Socket clientSocket = null; //создаем временную переменную для записи сокета клиента

            WaitingForClientConnect?.Invoke(this, ServerSocketEventArgs.Create(_serverSocket));

            try
            {
                clientSocket = _serverSocket.Accept(); //пытаемя ацептнуть сокет клиента из сокета сервака
            }
            //Ловим исключение от клиента
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
