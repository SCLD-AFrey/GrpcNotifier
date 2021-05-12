using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcNotifier.Common;

namespace GrpcNotifier.Client
{
    public class NotificationServiceClient
    {
        private readonly Notification.NotificationClient m_client;

        public NotificationServiceClient()
        {
            m_client = new Notification.NotificationClient(
                new Channel("localhost", 50052, ChannelCredentials.Insecure));
        }

        public async Task Write(NotificationLog notificationLog)
        {
            await m_client.WriteAsync(notificationLog);
        }

        public IAsyncEnumerable<NotificationLog> NotificationLogs()
        {
            var call = m_client.Subscribe(new Empty());

            // I do not want to expose gRPC such as IAsyncStreamReader or AsyncServerStreamingCall.
            // I also do not want to bother user of this class with asking to dispose the call object.

            return call.ResponseStream
                .ToAsyncEnumerable()
                .Finally(() => call.Dispose());
        }
    }
}