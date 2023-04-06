using ChatClient;
using ImageChat.Contains;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

//Сделано по мотивам кода преподователя 

namespace ChatServer
{
    public class ServerLocatorService : IDisposable
    {
        private readonly int _serverServicePort;
        public Socket server;
        private readonly ServerLocatorSenderService _serverLocatorSenderService;
        private readonly ServerLocatorReceiverService _serverLocatorReceiverService;

        public ServerLocatorService(int serverServicePort)
        {
            _serverServicePort = serverServicePort;
            _serverLocatorSenderService = new ServerLocatorSenderService(TimeSpan.FromSeconds(10));
            _serverLocatorReceiverService = new ServerLocatorReceiverService(
                Constants.ServerLocatorBroadcastDatagramReceiveTimeout);

            _serverLocatorReceiverService.OnBroadcastMessageReceived +=
                ServerLocatorReceiverService_OnBroadcastMessageReceived;
        }

        private void ServerLocatorReceiverService_OnBroadcastMessageReceived(object sender, string e)
        {
            Console.WriteLine($@"{DateTime.Now.ToLongTimeString()} -> [ServerLocatorSenderService] " +
                              $@"Server received broadcast message [{e}]");
            var data = e.Split(new[] { '[', ']', ':' }, StringSplitOptions.RemoveEmptyEntries);
            var clientIp = data[0];
            var clientPort = data[1];
            var clientRequest = data[2];

            var clientEndPoint = new IPEndPoint(IPAddress.Parse(clientIp), Convert.ToInt32(clientPort));

            switch (clientRequest)
            {
                case "Get image chat server IP&Port":
                    _serverLocatorSenderService.SendInfo(clientEndPoint,
                        $"[{Program._serverPort}]Server info"
                    );

                    break;
                default:
                    Console.WriteLine($@"{DateTime.Now.ToLongTimeString()} -> [ServerLocatorSenderService] " +
                                      @"Unknown command received from broadcast");
                    break;

            }
        }

        public void Start()
        {
            _serverLocatorSenderService.Start();
            _serverLocatorReceiverService.Start();
        }

        public void Stop()
        {
            _serverLocatorSenderService.Stop();
            _serverLocatorReceiverService.Stop();
            Dispose();
        }

        public void Dispose()
        {
            _serverLocatorSenderService?.Dispose();
            _serverLocatorReceiverService?.Dispose();
        }
    }
}
