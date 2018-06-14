using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using SharpRaven;
using SharpRaven.Data;

namespace NetGrpcSentry
{
    /// <summary>
    /// Interceptor for logging exceptions into the Sentry
    /// </summary>
    public class SentryInterceptor : Interceptor
    {
        private readonly RavenClient _sentryClient;
        private readonly Breadcrumber _breadcrumber;

        /// <summary>
        /// General constructor for exception logging
        /// </summary>
        /// <param name="dsn">Sentry dsn</param>
        /// <param name="jsonPacketFactory">Sentry configuration</param>
        /// <param name="sentryRequestFactory">Sentry configuration</param>
        /// <param name="sentryUserFactory">Sentry configuration</param>
        public SentryInterceptor(string dsn, IJsonPacketFactory jsonPacketFactory = null,
            ISentryRequestFactory sentryRequestFactory = null, ISentryUserFactory sentryUserFactory = null)
        {
            _sentryClient = new RavenClient(dsn, jsonPacketFactory, sentryRequestFactory, sentryUserFactory);
            _breadcrumber = new Breadcrumber(_sentryClient);
        }

        /// <summary>
        /// General constructor for exception logging
        /// </summary>
        /// <param name="dsn">Sentry dsn</param>
        /// <param name="jsonPacketFactory">Sentry configuration</param>
        /// <param name="sentryRequestFactory">Sentry configuration</param>
        /// <param name="sentryUserFactory">Sentry configuration</param>
        public SentryInterceptor(Dsn dsn, IJsonPacketFactory jsonPacketFactory = null,
            ISentryRequestFactory sentryRequestFactory = null, ISentryUserFactory sentryUserFactory = null)
        {
            _sentryClient = new RavenClient(dsn, jsonPacketFactory, sentryRequestFactory, sentryUserFactory);
            _breadcrumber = new Breadcrumber(_sentryClient);
        }

        #region SERVER

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                return await continuation(request, context);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception e)
            {
                _breadcrumber.MessageBreadcrumb(request);
                _breadcrumber.ContextBreadcrumb(context);
                _breadcrumber.MethodBreadcrumb(continuation.Method);

                _sentryClient.Capture(new SentryEvent(e));

                throw;
            }
        }

        public override async Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                await continuation(request, responseStream, context);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception e)
            {
                _breadcrumber.MessageBreadcrumb(request);
                _breadcrumber.ContextBreadcrumb(context);
                _breadcrumber.MethodBreadcrumb(continuation.Method);

                _sentryClient.Capture(new SentryEvent(e));

                throw;
            }
        }

        public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream, ServerCallContext context,
            ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                return await continuation(requestStream, context);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception e)
            {
                _breadcrumber.ContextBreadcrumb(context);
                _breadcrumber.MethodBreadcrumb(continuation.Method);

                _sentryClient.Capture(new SentryEvent(e));

                throw;
            }
        }

        public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            IServerStreamWriter<TResponse> responseStream, ServerCallContext context,
            DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                await continuation(requestStream, responseStream, context);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception e)
            {
                _breadcrumber.ContextBreadcrumb(context);
                _breadcrumber.MethodBreadcrumb(continuation.Method);

                _sentryClient.Capture(new SentryEvent(e));

                throw;
            }
        }

        #endregion
    }
}
