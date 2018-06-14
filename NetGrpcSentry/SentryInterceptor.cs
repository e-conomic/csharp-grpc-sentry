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

        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            return continuation(request, context).ContinueWith(task =>
            {
                if (task.Exception == null)
                {
                    return task.Result;
                }

                if (task.Exception != null &&
                    task.Exception.InnerExceptions.All(exception => exception is RpcException))
                {
                    return task.Result;
                }
                
                _breadcrumber.MessageBreadcrumb(request);
                _breadcrumber.ContextBreadcrumb(context);
                _breadcrumber.MethodBreadcrumb(continuation.Method);
                
                var exceptions = task.Exception.InnerExceptions.Where(e => !(e.InnerException is RpcException));
                foreach (var exception in exceptions)
                {
                    _sentryClient.Capture(new SentryEvent(exception));
                }

                throw task.Exception;

            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            return continuation(request, responseStream, context).ContinueWith(task =>
            {
                if (task.Exception == null)
                {
                    return;
                }

                if (task.Exception != null &&
                    task.Exception.InnerExceptions.All(exception => exception is RpcException))
                {
                    return;
                }

                _breadcrumber.MessageBreadcrumb(request);
                _breadcrumber.ContextBreadcrumb(context);
                _breadcrumber.MethodBreadcrumb(continuation.Method);

                var exceptions = task.Exception.InnerExceptions.Where(e => !(e.InnerException is RpcException));
                foreach (var exception in exceptions)
                {
                    _sentryClient.Capture(new SentryEvent(exception));
                }

            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream, ServerCallContext context,
            ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            return continuation(requestStream, context).ContinueWith(task =>
            {
                if (task.Exception == null)
                {
                    return task.Result;
                }

                if (task.Exception != null &&
                    task.Exception.InnerExceptions.All(exception => exception is RpcException))
                {
                    return task.Result;
                }
                
                _breadcrumber.ContextBreadcrumb(context);
                _breadcrumber.MethodBreadcrumb(continuation.Method);

                var exceptions = task.Exception.InnerExceptions.Where(e => !(e.InnerException is RpcException));
                foreach (var exception in exceptions)
                {
                    _sentryClient.Capture(new SentryEvent(exception));
                }

                return task.Result;

            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            IServerStreamWriter<TResponse> responseStream, ServerCallContext context,
            DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            return continuation(requestStream, responseStream, context).ContinueWith(task =>
            {
                if (task.Exception == null)
                {
                    return;
                }

                if (task.Exception != null &&
                    task.Exception.InnerExceptions.All(exception => exception is RpcException))
                {
                    return;
                }

                _breadcrumber.ContextBreadcrumb(context);
                _breadcrumber.MethodBreadcrumb(continuation.Method);

                var exceptions = task.Exception.InnerExceptions.Where(e => !(e.InnerException is RpcException));
                foreach (var exception in exceptions)
                {
                    _sentryClient.Capture(new SentryEvent(exception));
                }

            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        #endregion
    }
}
