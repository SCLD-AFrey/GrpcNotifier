using System;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using GrpcNotifier.Common;
using GrpcNotifier.Client.Common;

namespace GrpcNotifier.Client.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var originId = Guid.NewGuid().ToString();

            System.Console.WriteLine($"Joined as {originId}");
            var notificationServiceClient = new NotificationServiceClient();
            var consoleLock = new object();

            // subscribe (asynchronous)
            _ = notificationServiceClient.NotificationLogs()
                .ForEachAsync(x =>
                {
                    // if the user is writing something, wait until it finishes.
                    lock (consoleLock)
                    {
                        System.Console.WriteLine($"{x.At.ToDateTime().ToString("HH:mm:ss")} {x.OriginId}: {x.Content}");
                    }
                });

            // write
            while (true)
            {
                var key = System.Console.ReadKey();

                // A key input starts writing mode
                lock (consoleLock)
                {
                    var content = key.KeyChar + System.Console.ReadLine();

                    notificationServiceClient.Write(new NotificationLog
                    {
                        OriginId = originId,
                        Content = content,
                        At = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())
                    }).Wait();
                }
            }
        }
    }
}