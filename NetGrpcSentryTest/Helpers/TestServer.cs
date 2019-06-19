using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using NetGrpcSentry;
using NetGrpcSentryTest.Service;

namespace NetGrpcSentryTest.Helpers
{
    public class TestServer : IDisposable
    {
        private readonly Server _server;

        public readonly int Port;

        public TestServer()
        {
            Port = PortFinder.FreeTcpPort();

            var dsn = Environment.GetEnvironmentVariable("NetGrpcSentry_DSN") ?? "http://test:test@localhost:65535/1";
            
            _server = new Server()
            {
                Services =
                {
                    TestService.BindService(new ServiceImp())
                        .Intercept(new SentryInterceptor(dsn))
                },
                Ports = {new ServerPort("localhost", Port, ServerCredentials.Insecure)}
            };

            _server.Start();
        }

        public void Dispose()
        {
            _server.ShutdownAsync().Wait();
        }
    }
}
