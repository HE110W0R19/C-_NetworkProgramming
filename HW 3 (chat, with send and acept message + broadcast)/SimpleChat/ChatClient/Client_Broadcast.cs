using ImageChat.Contains;
using System;
using System.Collections.Generic;

//ВСЕ сделано по мотивам кода преподователя 

namespace ChatClient
{
    internal class Client_Broadcast : IDisposable
    {
        private List<string> _servers;
        private readonly ServerLocatorSenderService _serverLocatorSenderService;
        private readonly ServerLocatorReceiverService _serverLocatorReceiverService;

        public Client_Broadcast()
        {
            _servers = new List<string>();
            _serverLocatorReceiverService = new ServerLocatorReceiverService(TimeSpan.FromSeconds(10));
            _serverLocatorSenderService = new ServerLocatorSenderService(
                Constants.ServerLocatorBroadcastDatagramSendTimeout,
                Constants.ServerLocatorBroadcastPort,
                _serverLocatorReceiverService.UdpPort);
        }

        public List<string> getServerList()
        {
            return _serverLocatorReceiverService.getServerList;
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
        }

        public void Dispose()
        {
            _serverLocatorSenderService?.Dispose();
            _serverLocatorReceiverService?.Dispose();
        }
    }
}
