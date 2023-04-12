using System;
using System.Net.Sockets;
using System.Threading;

namespace ImageChat.Shared
{
    public class SocketAsyncStates : IDisposable
    {
        public Socket Socket { get; }
        public ManualResetEvent ManualResetEvent { get; }

        public SocketAsyncStates(Socket socket)
        {
            Socket = socket;
            ManualResetEvent = new ManualResetEvent(false);
        }

        public void Dispose()
        {
            ManualResetEvent?.Dispose();
        }
    }
}