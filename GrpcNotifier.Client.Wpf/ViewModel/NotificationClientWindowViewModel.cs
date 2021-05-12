using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using Google.Protobuf.WellKnownTypes;
using GrpcNotifier.Common;
using Prism.Commands;
using Prism.Mvvm;

namespace GrpcNotifier.Client.Wpf.ViewModel
{
    public class NotificationClientWindowViewModel : BindableBase
    {
        private readonly object m_notificationHistoryLockObject = new();
        private readonly NotificationServiceClient m_notificationService = new();
        private string m_originId = Guid.NewGuid().ToString();

        public NotificationClientWindowViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(NotificationHistory, m_notificationHistoryLockObject);

            WriteCommand = new DelegateCommand<string>(WriteCommandExecute);

            StartReadingChatServer();
        }

        public ObservableCollection<string> NotificationHistory { get; } = new();

        public string Origin
        {
            get => m_originId;
            set => SetProperty(ref m_originId, value);
        }

        public DelegateCommand<string> WriteCommand { get; }

        private void StartReadingChatServer()
        {
            var cts = new CancellationTokenSource();
            _ = m_notificationService.NotificationLogs()
                .ForEachAsync(
                    x => NotificationHistory.Add($"{x.At.ToDateTime().ToString("HH:mm:ss")} {x.OriginId}: {x.Content}"),
                    cts.Token);

            Application.Current.Exit += (_, __) => cts.Cancel();
        }

        private async void WriteCommandExecute(string content)
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