using SupplyCoreERP.Enums.Notificaitons;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Notifications
{
    public class Notification : CreationAuditedAggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
        public string Title { get; private set; }
        public string Body { get; private set; }
        public NotificationType NotificationType { get; private set; }
        public string? ActionUrl { get; private set; }
        public bool IsRead { get; private set; }
        public DateTime? ReadAt { get; private set; }
        public bool IsEmailSent { get; private set; }

        protected Notification() { }

        public Notification(
         Guid id, Guid userId,
         string title, string body,
         NotificationType notificationType,
         string? actionUrl = null)
         : base(id)
        {
            UserId = userId;
            Title = Check.NotNullOrWhiteSpace(title, nameof(title), 255);
            Body = Check.NotNullOrWhiteSpace(body, nameof(body), 1024);
            NotificationType = notificationType;
            ActionUrl = actionUrl;
            IsRead = false;
            IsEmailSent = false;
        }

        public void MarkAsRead()
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }

    }
}
