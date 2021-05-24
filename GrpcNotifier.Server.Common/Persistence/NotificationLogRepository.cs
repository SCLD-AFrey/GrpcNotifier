using System.Collections.Generic;
using System.ComponentModel.Composition;
using GrpcNotifier.Common;
using GrpcNotifier.Server.Common.Model;

namespace GrpcNotifier.Server.Common.Persistence
{
    [Export(typeof(INotificationLogRepository))]
    public class NotificationLogRepository : INotificationLogRepository
    {
        private readonly List<NotificationLog> m_storage = new(); // dummy on memory storage

        public void Add(NotificationLog chatLog)
        {
            m_storage.Add(chatLog);
        }

        public IEnumerable<NotificationLog> GetAll()
        {
            return m_storage.AsReadOnly();
        }
    }
}