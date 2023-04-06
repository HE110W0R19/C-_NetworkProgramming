using ImageChat.Shared;
using ImageChat.Contains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Utilities;

namespace ChatClient
{
    public class ServerLocatorReceiverService : BaseThread
    {
        public event EventHandler<string> OnBroadcastMessageReceived;
        private readonly int _bindingPort;

        public ServerLocatorReceiverService(TimeSpan loopDelay) : base(loopDelay)
        {
            _bindingPort = Constants.ServerLocatorBroadcastPort;
        }

        protected override Socket CreateServiceSocket()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.EnableBroadcast = true;
            socket.Bind(new IPEndPoint(IPAddress.Any, _bindingPort));


            return socket;
        }

        protected override void ServiceWorkerLoop(Socket serviceSocket)
        {
            if (serviceSocket.Available == 0)
            {
                return;
            }

            var message = ReceiveMessage(serviceSocket).Result;

            OnBroadcastMessageReceived?.Invoke(this, message);
        }

        private static async Task<string> ReceiveMessage(Socket serviceSocket)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[Constants.UdpDatagramSize]);

            await serviceSocket.ReceiveAsync(buffer, SocketFlags.None);

            return SocketUtility.GetStringFromDatagram(buffer.Array);
        }
    }

    public class ServerLocatorSenderService : BaseThread
    {
        private readonly Queue<KeyValuePair<IPEndPoint, string>> _messagesToSend;
        private readonly object _messagesToSendLockObject;

        public ServerLocatorSenderService(TimeSpan loopDelay) : base(loopDelay)
        {
            _messagesToSend = new Queue<KeyValuePair<IPEndPoint, string>>();
            _messagesToSendLockObject = new object();
        }

        public void SendInfo(IPEndPoint targetLocatorServiceEndPoint, string message)
        {
            lock (_messagesToSendLockObject)
            {
                _messagesToSend.Enqueue(new KeyValuePair<IPEndPoint, string>(targetLocatorServiceEndPoint, message));
            }

            Console.WriteLine($@"Message[{message}] with target endpoint" +
                              $@"[{targetLocatorServiceEndPoint.Address.MapToIPv4()}:" +
                              $@"{targetLocatorServiceEndPoint.Port}] enqueued to send messages queue.");
        }

        protected override Socket CreateServiceSocket()
        {
            var socket = new Socket(SocketType.Dgram, ProtocolType.Udp);

            socket.EnableBroadcast = true;

            return socket;
        }

        protected override void ServiceWorkerLoop(Socket serviceSocket)
        {
            lock (_messagesToSendLockObject)
            {
                if (!_messagesToSend.Any())
                {
                    return;
                }

                StartSendMessages(serviceSocket).Wait();
            }

            Console.WriteLine(@"All enqueue messages sent.");
        }

        private async Task StartSendMessages(Socket serviceSocket)
        {
            while (_messagesToSend.Any())
            {
                var messageToSend = _messagesToSend.Dequeue();
                var datagramArray =
                    SocketUtility.PrepareDatagramForSendingString(
                        Constants.UdpDatagramSize,
                        messageToSend.Value,
                        () => throw new ArgumentOutOfRangeException(
                            $"Can not send string, data size exceeds datagram size")
                    );
                var datagram = new ArraySegment<byte>(datagramArray);

                await serviceSocket.SendToAsync(datagram, SocketFlags.None, messageToSend.Key);
            }
        }

    }

    public class ServerService : BaseThread
    {
        public int TcpPort { get; }

        public ServerService(TimeSpan loopDelay) : base(loopDelay)
        {
            Random randomGenerator = new Random();

            TcpPort = Constants.ServerTcpPorts[randomGenerator.Next(0, Constants.ServerTcpPorts.Length)];
        }

        protected override Socket CreateServiceSocket()
        {
            throw new NotImplementedException();
        }

        protected override void ServiceWorkerLoop(Socket serviceSocket)
        {
            throw new NotImplementedException();
        }
    }
}