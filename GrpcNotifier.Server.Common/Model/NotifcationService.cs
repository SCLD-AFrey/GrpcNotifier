using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GrpcNotifier.Common;
using GrpcNotifier.Server.Common.Infrastructure;

namespace GrpcNotifier.Server.Common.Model
{
    [Export]
    public class NotifcationService
    {
        [Import] private Logger m_logger;

        [Import] private INotificationLogRepository m_repository;

        private event Action<NotificationLog> Added;

        public void Add(NotificationLog notificationLog)
        {
            m_logger.Info($"{notificationLog}");

            m_repository.Add(notificationLog);
            Added?.Invoke(notificationLog);
        }

        public IObservable<NotificationLog> GetChatLogsAsObservable()
        {
            var oldLogs = m_repository.GetAll().ToObservable();
            var newLogs = Observable.FromEvent<NotificationLog>(x => Added += x, x => Added -= x);

            return oldLogs.Concat(newLogs);
        }
    }
}