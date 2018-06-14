using System;
using System.Net;
using System.Net.Sockets;

namespace NetGrpcSentryTest.Helpers
{
    public class PortFinder : TcpListener, IDisposable
    {
        public PortFinder(int port) : base(port)
        {
            Start();
        }

        public PortFinder(IPAddress localaddr, int port) : base(localaddr, port)
        {
            Start();
        }

        public PortFinder(IPEndPoint localEp) : base(localEp)
        {
            Start();
        }

        public void Dispose()
        {
            Stop();
        }

        public static int FreeTcpPort()
        {
            using (var tcpListener = new PortFinder(IPAddress.Loopback, 0))
            {
                return ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            }
        }
    }
}
