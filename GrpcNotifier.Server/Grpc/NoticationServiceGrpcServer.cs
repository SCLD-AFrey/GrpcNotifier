using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Interceptors;
using GrpcNotifier.Common;
using GrpcNotifier.Server.Infrastructure;
using GrpcNotifier.Server.Model;

namespace GrpcNotifier.Server.Rpc
{
    public class NoticationServiceGrpcServer
    {
        [Export(typeof(IService))]
        public class NotificationServiceGrpcServer : Notification.NotificationBase, IService
        {
            private const int Port = 50052;
            private readonly Empty m_empty = new();
            private readonly Grpc.Core.Server m_server;

            [Import] private Logger m_logger;

            [Import] private NotifcationService m_notificationService;

            public NotificationServiceGrpcServer()
            {
                // Locate required files and set true to enable SSL
                //var secure = false;

                m_server = new Grpc.Core.Server
                {
                    Services =
                    {
                        Notification.BindService(this)
                            .Intercept(new IpAddressAuthenticator())
                    },
                    Ports =
                    {
                        new ServerPort("localhost", Port, ServerCredentials.Insecure)
                    }
                };
            }

            public void Start()
            {
                m_server.Start();

                m_logger.Info("Started.");
            }

            public override async Task Subscribe(Empty request, IServerStreamWriter<NotificationLog> responseStream,
                ServerCallContext context)
            {
                var peer = context.Peer; // keep peer information because it is not available after disconnection
                m_logger.Info($"{peer} subscribes.");

                context.CancellationToken.Register(() => m_logger.Info($"{peer} cancels subscription."));

                // Completing the method means disconnecting the stream by server side.
                // If subscribing IObservable, you have to block this method after the subscription.
                // I prefer converting IObservable to IAsyncEnumerable to consume the sequense here
                // because gRPC interface is in IAsyncEnumerable world.
                // Note that the chat service model itself is in IObservable world
                // because chat is naturally recognized as an event sequence.

                try
                {
                    await m_notificationService.GetChatLogsAsObservable()
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(async x => await responseStream.WriteAsync(x), context.CancellationToken)
                        .ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    m_logger.Info($"{peer} unsubscribed.");
                }
            }

            public override Task<Empty> Write(NotificationLog request, ServerCallContext context)
            {
                m_logger.Info($"{context.Peer} {request}");

                m_notificationService.Add(request);

                return Task.FromResult(m_empty);
            }
        }
    }
}