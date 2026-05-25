using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SupplyCoreERP.Common.Notifications;
using SupplyCoreERP.Enums.Notificaitons;
using SupplyCoreERP.Notifications.Dtos;
using SupplyCoreERP.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

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

    [RemoteService(false)]
    public async Task<NotificationDto> CreateGlobalAsync(
        string title, string content, NotificationSeverity severity)
    {
        Notification notification = _notificationManager
            .CreateForGlobal(title, content, severity);

        await _notificationRepo.InsertAsync(notification, autoSave: true);

        return ObjectMapper.Map<Notification, NotificationDto>(notification);
    }

    [RemoteService(false)]
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

    [Authorize]
    public async Task<PagedResultDto<NotificationDto>> GetListAsync(GetNotificationListDto input)
    {
        Guid userId = _currentUser.GetId();

        List<string> grantedPerms = new();
        foreach (string? perm in new[]
        {
            SupplyCoreERPPermissions.Catalog.Medicine.Approve,
            SupplyCoreERPPermissions.Catalog.Medicine.Reject
        })
        {
            if (await _permissionChecker.IsGrantedAsync(perm))
            {
                grantedPerms.Add(perm);
            }
        }

        HashSet<Guid> deletedIds = (await _userNotifRepo
            .GetListAsync(x => x.UserId == userId && x.IsDelete))
            .Select(x => x.NotificationId)
            .ToHashSet();

        IQueryable<Notification> query = (await _notificationRepo.GetQueryableAsync())
            .Where(n => n.Level == NotificationLevel.Global || n.Level == NotificationLevel.Permission);

        if (input.Level.HasValue)
        {
            query = query.Where(n => n.Level == input.Level.Value);
        }

        query = query.OrderByDescending(n => n.CreationTime);

        List<Notification> allCandidates = await AsyncExecuter.ToListAsync(query);

        List<Notification> filtered = allCandidates
            .Where(n => !deletedIds.Contains(n.Id))
            .Where(n =>
                n.Level == NotificationLevel.Global ||
                (n.Level == NotificationLevel.Permission
                 && n.TargetPermissions.Any(p => grantedPerms.Contains(p))))
            .ToList();

        int total = filtered.Count;

        List<Notification> items = filtered
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        List<NotificationDto> dtos = ObjectMapper
            .Map<List<Notification>, List<NotificationDto>>(items);

        List<Guid> notifIds = items.Select(x => x.Id).ToList();

        HashSet<Guid> readIds = (await _userNotifRepo
            .GetListAsync(x => x.UserId == userId && notifIds.Contains(x.NotificationId)))
            .Where(x => x.IsRead)
            .Select(x => x.NotificationId)
            .ToHashSet();

        foreach (NotificationDto dto in dtos)
        {
            dto.IsRead = readIds.Contains(dto.Id);
        }

        if (input.IsRead.HasValue)
        {
            dtos = dtos.Where(x => x.IsRead == input.IsRead.Value).ToList();
        }

        return new PagedResultDto<NotificationDto>(total, dtos);
    }

    [Authorize]
    public async Task MarkReadAsync(Guid notificationId)
        => await _notificationManager.MarkReadAsync(notificationId, _currentUser.GetId());

    [Authorize]
    public async Task MarkAllReadAsync(List<Guid> ids)
        => await _notificationManager.MarkManyReadAsync(ids, _currentUser.GetId());
    [Authorize]
    public async Task MarkDeleteAsync(Guid notificationId)
        => await _notificationManager.MarkDeleteAsync(notificationId, _currentUser.GetId());
    [Authorize]
    public async Task MarkAllDeleteAsync(List<Guid> ids)
        => await _notificationManager.MarkManyDeleteAsync(ids, _currentUser.GetId());
}

