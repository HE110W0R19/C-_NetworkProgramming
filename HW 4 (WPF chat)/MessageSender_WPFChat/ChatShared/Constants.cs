using System;

namespace ImageChat.Contains
{
    public static class Constants
    {
        public static int ServerLocatorBroadcastPort => 11111;
        public static int[] ServerLocatorUdpPorts => new[]
            {21210, 21211, 21212, 21213, 21214, 21215, 21216, 21217, 21218, 21219};
        public static int[] ServerTcpPorts => new[]
            {11110, 11111, 11112, 11113, 11114, 11115, 11116, 11117, 11118, 11119};
        public static int UdpDatagramSize => 1024;
        public static TimeSpan ServerLocatorBroadcastDatagramSendTimeout => TimeSpan.FromSeconds(1);
        public static TimeSpan ServerLocatorBroadcastDatagramReceiveTimeout => TimeSpan.FromSeconds(0.3f);
    }
}