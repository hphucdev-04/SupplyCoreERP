using SupplyCoreERP.Enums.Notificaitons;
using SupplyCoreERP.Notifications.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using static SupplyCoreERP.Permissions.SupplyCoreERPPermissions;

namespace SupplyCoreERP.Notifications;

public class NotificationAppService : SupplyCore, INotificationAppService
{
    private readonly NotificationManager _notificationManager;
    private readonly IRepository<Notification, Guid> _notificationRepo;
    private readonly IRepository<UserNotification, Guid> _userNotifRepo;
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionChecker _permissionChecker;

    public NotificationAppService(
        NotificationManager notificationManager,
        IRepository<Notification, Guid> notificationRepo,
        IRepository<UserNotification, Guid> userNotifRepo,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
    {
        _notificationManager = notificationManager;
        _notificationRepo = notificationRepo;
        _userNotifRepo = userNotifRepo;
        _currentUser = currentUser;
        _permissionChecker = permissionChecker;
    }

    public async Task<NotificationDto> CreateGlobalAsync(
        string title, string content, NotificationSeverity severity)
    {
        Notification notification = _notificationManager
            .CreateForGlobal(title, content, severity);

        await _notificationRepo.InsertAsync(notification, autoSave: true);

        return ObjectMapper.Map<Notification, NotificationDto>(notification);
    }

    public async Task<NotificationDto> CreateForPermissionAsync(
        string title, string content,
        NotificationSeverity severity,
        List<string> targetPermissions)
    {
        Notification notification = _notificationManager
            .CreateForPermission(title, content, severity, targetPermissions);

        await _notificationRepo.InsertAsync(notification, autoSave: true);

        return ObjectMapper.Map<Notification, NotificationDto>(notification);
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public async Task<PagedResultDto<NotificationDto>> GetListAsync(GetNotificationListDto input)
    {
        List<string> grantedPerms = new();
        foreach (string perm in new[]
        {
            Catalog.Medicine.Approve,
            Catalog.Medicine.Reject
        })
            if (await _permissionChecker.IsGrantedAsync(perm))
                grantedPerms.Add(perm);

        IQueryable<Notification> query = (await _notificationRepo.GetQueryableAsync())
            .Where(n =>
                n.Level == NotificationLevel.Global ||
                (n.Level == NotificationLevel.Permission
                 && n.TargetPermissions.Any(p => grantedPerms.Contains(p))));

        if (input.Level.HasValue)
            query = query.Where(n => n.Level == input.Level.Value);

        query = query.OrderByDescending(n => n.CreationTime);

        int total = await AsyncExecuter.CountAsync(query);

        List<Notification> items = await AsyncExecuter.ToListAsync(query.PageBy(input));

        List<NotificationDto> dtos = ObjectMapper
            .Map<List<Notification>, List<NotificationDto>>(items);

        // Gán IsRead
        Guid userId = _currentUser.GetId();
        List<Guid> notifIds = items.Select(x => x.Id).ToList();

        HashSet<Guid> readIds = (await _userNotifRepo
            .GetListAsync(x => x.UserId == userId && notifIds.Contains(x.NotificationId)))
            .Where(x => x.IsRead)
            .Select(x => x.NotificationId)
            .ToHashSet();

        foreach (NotificationDto dto in dtos)
            dto.IsRead = readIds.Contains(dto.Id);

        if (input.IsRead.HasValue)
            dtos = dtos.Where(x => x.IsRead == input.IsRead.Value).ToList();

        return new PagedResultDto<NotificationDto>(total, dtos);
    }

    public async Task MarkReadAsync(Guid notificationId)
        => await _notificationManager.MarkReadAsync(notificationId, _currentUser.GetId());

    public async Task MarkAllReadAsync(List<Guid> ids)
        => await _notificationManager.MarkManyReadAsync(ids, _currentUser.GetId());
}