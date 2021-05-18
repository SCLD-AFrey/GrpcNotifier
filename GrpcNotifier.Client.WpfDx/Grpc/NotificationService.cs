using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using DevExpress.Mvvm;
using Google.Protobuf.WellKnownTypes;
using GrpcNotifier.Common;

namespace GrpcNotifier.Client.WpfDx.Grpc
{
    public class NotificationService : BindableBase
    {
        private readonly object m_notificationHistoryLockObject = new();
        private static readonly NotificationServiceClient m_notificationService = new();
        private static string m_originId = Guid.NewGuid().ToString();
        
        public NotificationService()
        {

        }
        
        public static ObservableCollection<string> NotificationHistory { get; } = new();
        public string Origin
        {
            get => m_originId;
            set => m_originId = value;
        }
        public DelegateCommand<string> WriteCommand { get; }
        
        
        public static void StartReadingNotificationServer()
        {
            var cts = new CancellationTokenSource();
            _ = m_notificationService.NotificationLogs()
                .ForEachAsync(
                    x => NotificationHistory.Add($"{x.At.ToDateTime().ToString("HH:mm:ss")} {x.OriginId}: {x.Content}"),
                    cts.Token);

            Application.Current.Exit += (_, __) => cts.Cancel();
        }

        public static async void WriteCommandExecute(string content)
        {
            await m_notificationService.Write(new NotificationLog
            {
                OriginId = m_originId,
                Content = content,
                At = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())
            });
        }
    }
}