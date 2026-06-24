using System;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Notificaitons;
using SupplyCoreERP.Inventory.Tickets.Events;
using SupplyCoreERP.Notifications.Dtos;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace SupplyCoreERP.Notifications.Handlers;

public class InventoryTicketExecutedNotificationHandler
    : ILocalEventHandler<InventoryTicketExecutedDomainEvent>, ITransientDependency
{
    private readonly INotificationRealTime _notificationRealTime;

    public InventoryTicketExecutedNotificationHandler(INotificationRealTime notificationRealTime)
    {
        _notificationRealTime = notificationRealTime;
    }

    public async Task HandleEventAsync(InventoryTicketExecutedDomainEvent eventData)
    {
        // Bắn tín hiệu SignalR cập nhật Dashboard
        await _notificationRealTime.SendToGlobalAsync(new NotificationDto
        {
            Id = Guid.NewGuid(),
            Title = "[System] InventoryChanged",
            Content = $"Phiếu kho ID {eventData.TicketId} đã thực thi.",
            Severity = NotificationSeverity.Info,
            Level = NotificationLevel.Global,
            CreationTime = DateTime.UtcNow
        });
    }
}
