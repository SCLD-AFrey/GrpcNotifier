using System.Collections.Generic;
using GrpcNotifier.Common;

namespace GrpcNotifier.Server.Model
{
    public interface INotificationLogRepository
    {
        void Add(NotificationLog chatLog);
        IEnumerable<NotificationLog> GetAll();
    }
}