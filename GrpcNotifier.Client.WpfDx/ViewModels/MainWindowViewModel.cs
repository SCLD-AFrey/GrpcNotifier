using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using Google.Protobuf.WellKnownTypes;
using GrpcNotifier.Common;

namespace GrpcNotifier.Client.WpfDx.ViewModels
{
    [MetadataType(typeof(MetaData))]
    public class MainWindowViewModel
    {
        public class MetaData : IMetadataProvider<MainWindowViewModel>
        {
            void IMetadataProvider<MainWindowViewModel>.BuildMetadata
                (MetadataBuilder<MainWindowViewModel> p_builder)
            {
            }
        }

        #region Constructors

        protected MainWindowViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(NotificationHistory, m_notificationHistoryLockObject);
            StartReadingNotificationServer();
        }

        public static MainWindowViewModel Create()
        {
            return ViewModelSource.Create(() => new MainWindowViewModel());
        }

        #endregion

        #region Fields and Properties

        private readonly object m_notificationHistoryLockObject = new();
        private readonly NotificationServiceClient m_notificationService = new();
        private string m_originId = Guid.NewGuid().ToString();
        public ObservableCollection<string> NotificationHistory { get; } = new();

        #endregion

        #region Methods

        private void StartReadingNotificationServer()
        {
            var cts = new CancellationTokenSource();
            _ = m_notificationService.NotificationLogs()
                .ForEachAsync(
                    x => NotificationHistory.Add($"{x.At.ToDateTime().ToString("HH:mm:ss")} {x.OriginId}: {x.Content}"),
                    cts.Token);

            Application.Current.Exit += (_, __) => cts.Cancel();
        }
        
        #endregion
    }
}