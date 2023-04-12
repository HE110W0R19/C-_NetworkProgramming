using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ImageChat.Shared;
using ImageChat.Contains;
using Utilities;
using System.Collections.Generic;

namespace ChatClient
{
    public class ServerLocatorSenderService : BaseThread
    {
        private readonly IPEndPoint _broadcastIpEndPoint;
        private readonly byte[] _broadcastDatagram;

        public ServerLocatorSenderService(TimeSpan loopDelay, int broadcastPort, int receiverPort) : base(loopDelay)
        {
            IPAddress broadcastAddress = CreateBroadcastAddress();

            _broadcastIpEndPoint = new IPEndPoint(broadcastAddress, broadcastPort);

            _broadcastDatagram =
                SocketUtility.PrepareDatagramForSendingString(
                    Constants.UdpDatagramSize,
                    $"[{GetLocalIpAddress()}:{receiverPort}]Get image chat server IP&Port",
                    () => throw new ArgumentOutOfRangeException(
                        $"Can not send string [Follow the white rabbit!], data size exceeds datagram size")
                );
        }

        protected override Socket CreateServiceSocket()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.EnableBroadcast = true;

            return socket;
        }

        protected override void ServiceWorkerLoop(Socket serviceSocket)
        {
            serviceSocket.BeginSendTo(_broadcastDatagram, 0, Constants.UdpDatagramSize, SocketFlags.None,
                _broadcastIpEndPoint, SendToCallback, serviceSocket);
        }

        private void SendToCallback(IAsyncResult asyncData)
        {
            var serviceSocket = (Socket)asyncData.AsyncState;

            try
            {
                serviceSocket.EndSendTo(asyncData);
            }
            catch (ObjectDisposedException)// callback called while dispose/close call
            {
            }

            Console.WriteLine($@"{DateTime.Now.ToLongTimeString()} -> [ServerLocatorSenderService] " +
                              "broadcast message sent to image chat server.");
        }

        private static IPAddress CreateBroadcastAddress()
        {
            var localIpAddress = GetLocalIpAddress();

            var localIpAddressNumbers = localIpAddress.Split('.');

            localIpAddressNumbers[3] = "255";

            var remoteIpAddressInString = localIpAddressNumbers
                .Aggregate("", (acc, value) => $"{acc}.{value}")
                .Substring(1);

            var broadcastAddress = IPAddress.Parse(remoteIpAddressInString);

            return broadcastAddress;
        }

        private static string GetLocalIpAddress()
        {
            return Dns
                .GetHostEntry(Dns.GetHostName())
                .AddressList
                .First(x => x.AddressFamily == AddressFamily.InterNetwork)
                .ToString();
        }
    }

    public class ServerLocatorReceiverService : BaseThread
    {
        private List<string> _servers;

        public int UdpPort { get; }

        public List<string> getServerList
        {
            get { return _servers; }
        }

        public ServerLocatorReceiverService(TimeSpan loopDelay) : base(loopDelay)
        {
            _servers = new List<string>();
            Random randomGenerator = new Random();
            UdpPort = Constants.ServerLocatorUdpPorts[randomGenerator.Next(0, Constants.ServerLocatorUdpPorts.Length)];
        }

        protected override Socket CreateServiceSocket()
        {
            var socket = new Socket(SocketType.Dgram, ProtocolType.Udp);

            socket.Bind(new IPEndPoint(IPAddress.Any, UdpPort));

            return socket;
        }

        protected override void ServiceWorkerLoop(Socket serviceSocket)
        {
            if (serviceSocket.Available > 0)
            {
                Task.Delay(TimeSpan.FromSeconds(10));
            }
            var message = ReceiveMessage(serviceSocket).Result;

            var data = message.Split(new[] { '[', ']'}, StringSplitOptions.RemoveEmptyEntries);
            var serverPort = data[0];

            _servers.Add(serverPort);
        }
        private static async Task<string> ReceiveMessage(Socket serviceSocket)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[Constants.UdpDatagramSize]);

            await serviceSocket.ReceiveAsync(buffer, SocketFlags.None);

            return SocketUtility.GetStringFromDatagram(buffer.Array);
        }
    }
}