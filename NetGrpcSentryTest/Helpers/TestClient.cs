using System;
using Grpc.Core;
using NetGrpcSentryTest.Service;

namespace NetGrpcSentryTest.Helpers
{
    public static class TestClient
    {
        public static TestService.TestServiceClient Create(int port)
        {
            var channel = new Channel("localhost", port, ChannelCredentials.Insecure);
            return new TestService.TestServiceClient(channel);
        }
    }
}
