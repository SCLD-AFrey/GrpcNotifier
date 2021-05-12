using System;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using GrpcNotifier.Common;

namespace GrpcNotifier.Client.Core
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var originId = Guid.NewGuid().ToString();

            Console.WriteLine($"Joined as {originId}");
            var notificationServiceClient = new NotificationServiceClient();
            var consoleLock = new object();

            // subscribe (asynchronous)
            _ = notificationServiceClient.NotificationLogs()
                .ForEachAsync(x =>
                {
                    // if the user is writing something, wait until it finishes.
                    lock (consoleLock)
                    {
                        Console.WriteLine($"{x.At.ToDateTime().ToString("HH:mm:ss")} {x.OriginId}: {x.Content}");
                    }
                });

            // write
            while (true)
            {
                var key = Console.ReadKey();

                // A key input starts writing mode
                lock (consoleLock)
                {
                    var content = key.KeyChar + Console.ReadLine();

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