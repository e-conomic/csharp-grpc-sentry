using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Grpc.Core;
using SharpRaven;
using SharpRaven.Data;

namespace NetGrpcSentry
{
    public class Breadcrumber
    {
        private readonly RavenClient _sentryClient;

        public Breadcrumber(RavenClient sentryClient)
        {
            _sentryClient = sentryClient;
        }

        public void GeneralInformationBreadcrumb()
        {
            _sentryClient.AddTrail(new Breadcrumb("General Information")
            {
                Message = "General information for the exception",
                Data = new Dictionary<string, string>()
                {
                    {nameof(DateTime), DateTime.Now.ToLongDateString()}
                }
            });
        }

        public void MessageBreadcrumb<T>(T message)
        {
            _sentryClient.AddTrail(new Breadcrumb("Message")
            {
                Message = "Server received following message",
                Level = BreadcrumbLevel.Info,
                Data = new Dictionary<string, string>()
                {
                    {"message", message.ToString()}
                }
            });
        }

        public void ContextBreadcrumb(ServerCallContext context)
        {
            var data = new Dictionary<string, string>()
            {
                {nameof(ServerCallContext.Host), context.Host},
                {nameof(ServerCallContext.Peer), context.Peer},
                {nameof(ServerCallContext.Deadline), context.Deadline.ToLongDateString()},
                {nameof(AuthContext.IsPeerAuthenticated), context.AuthContext.IsPeerAuthenticated ? "True" : "False"},
                {nameof(AuthContext.PeerIdentityPropertyName), context.AuthContext.PeerIdentityPropertyName},
                {nameof(AuthContext.PeerIdentity), string.Join(",", context.AuthContext.PeerIdentity.Select(p => $"{p.Name} : {p.Value}")) },
                {nameof(AuthContext.Properties), string.Join(",", context.AuthContext.PeerIdentity.Select(p => $"{p.Name} : {p.Value}")) }
            };

            foreach (var header in context.RequestHeaders)
            {
                data.Add(header.Key, header.Value);
            }

            _sentryClient.AddTrail(new Breadcrumb(nameof(ServerCallContext))
            {
                Message = "Context for a server-side call",
                Level = BreadcrumbLevel.Info,
                Data = data
            });
        }

        public void MethodBreadcrumb(MethodInfo method)
        {
            _sentryClient.AddTrail(new Breadcrumb("Method")
            {
                Message = "Following method was invoked on the server",
                Level = BreadcrumbLevel.Info,
                Data = new Dictionary<string, string>()
                {
                    {nameof(MethodInfo.Name), method.Name}
                }
            });
        }
    }
}
