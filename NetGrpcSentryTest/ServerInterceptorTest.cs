using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using NetGrpcSentryTest.Helpers;
using NetGrpcSentryTest.Service;
using NUnit.Framework;

namespace NetGrpcSentryTest
{
    [TestFixture]
    public class ServerInterceptorTest
    {
        private TestServer _server;
        private TestService.TestServiceClient _client;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _server = new TestServer();
            _client = TestClient.Create(_server.Port);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _server.Dispose();
        }

        [Test]
        public void Unary_Exception()
        {
            // Arrange, Act, Assert
            Assert.Throws<RpcException>(() =>
            {
                _client.Unary(new Request() {Message = "this is request message"},
                    new Metadata() {new Metadata.Entry("MetadataKey", "value")});
            });
        }

        [Test]
        public void ClientStreaming_Exception()
        {
            // Arrange
            using (var call = _client.ClientStream(new Metadata() {new Metadata.Entry("MetadataKey", "value")}))
            {
                // Act
                call.RequestStream.WriteAsync(new Request() {Message = "this is request message"}).Wait();
                call.RequestStream.CompleteAsync().Wait();

                // Assert
                Assert.ThrowsAsync<RpcException>(async() =>
                {
                    var result = await call.ResponseAsync;
                });
            }
        }

        [Test]  
        public async Task ServerStreaming_Exception()
        {
            // Arrange
            using (var call = _client.ServerStream(new Request() {Message = "this is request message"},
                new Metadata() {new Metadata.Entry("MetadataKey", "value")}))
            {
                // Act
                while (await call.ResponseStream.MoveNext(CancellationToken.None))
                {
                    var result = call.ResponseStream.Current;
                }

                // Assert
            }
        }

        [Test]
        public async Task DuplexStreaming_Exception()
        {
            // Arrange
            using (var call = _client.DuplexStream(new Metadata() { new Metadata.Entry("MetadataKey", "value") }))
            {
                // Act
                var responseReaderTask = Task.Run(async () =>
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        var result = call.ResponseStream.Current;
                    }
                });

                await call.RequestStream.WriteAsync(new Request() { Message = "this is request message" });
                await call.RequestStream.CompleteAsync();

                await responseReaderTask;

                // Assert
            }
        }
    }
}
