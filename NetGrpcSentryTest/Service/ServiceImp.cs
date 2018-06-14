using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace NetGrpcSentryTest.Service
{
    public class ServiceImp : TestService.TestServiceBase
    {
        public override Task<Response> Unary(Request request, ServerCallContext context)
        {
            return Task.FromException<Response>(new DivideByZeroException("Dummy exception"));
        }   
        
        public override Task<Response> ClientStream(IAsyncStreamReader<Request> requestStream, ServerCallContext context)
        {
            return Task.FromException<Response>(new DivideByZeroException("Dummy exception"));
        }

        public override Task ServerStream(Request request, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            return Task.FromException(new DivideByZeroException("Dummy exception"));
        }

        public override Task DuplexStream(IAsyncStreamReader<Request> requestStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            return Task.FromException(new DivideByZeroException("Dummy exception"));
        }
    }
}
