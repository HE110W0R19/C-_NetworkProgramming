using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ImageChat.Shared
{
    public abstract class BaseThread : IDisposable
    {
        private readonly TimeSpan _loopDelay;
        private bool _isStarted;
        private readonly Thread _serviceThread;

        protected BaseThread(TimeSpan loopDelay)
        {
            _loopDelay = loopDelay;
            _isStarted = false;
            _serviceThread = new Thread(ServiceWorker);
        }

        protected abstract Socket CreateServiceSocket();

        public virtual void Start()
        {
            _isStarted = true;

            _serviceThread.Start();
        }

        public virtual void Stop()
        {
            _isStarted = false;

            _serviceThread.Abort();
        }

        protected virtual void ServiceWorker()
        {
            using (var serviceSocket = CreateServiceSocket())
            {
                while (_isStarted)
                {
                    ServiceWorkerLoop(serviceSocket);

                    Task.Delay(_loopDelay).Wait();
                }

                serviceSocket.Close();
            }
        }

        protected abstract void ServiceWorkerLoop(Socket serviceSocket);

        public virtual void Dispose()
        {
            Stop();
        }
    }
}