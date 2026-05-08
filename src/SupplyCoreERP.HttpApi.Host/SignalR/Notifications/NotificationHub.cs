using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Authorization.Permissions;
using static SupplyCoreERP.Permissions.SupplyCoreERPPermissions;

namespace SupplyCoreERP.SignalR.Notifications;

[Authorize]
public class NotificationHub : AbpHub
{
    private readonly IPermissionChecker _permissionChecker;

    public NotificationHub(IPermissionChecker permissionChecker)
    {
        _permissionChecker = permissionChecker;
    }

    public override async Task OnConnectedAsync()
    {
        // Tất cả user join group Global
        await Groups.AddToGroupAsync(Context.ConnectionId, "Global");

        // Join từng permission-group nếu được cấp quyền
        string[] allPermissions =
        [
            Catalog.Medicine.Reject,
            Catalog.Medicine.Approve,
            // Thêm permission mới tại đây khi có feature mới
        ];

        foreach (var perm in allPermissions)
        {
            if (await _permissionChecker.IsGrantedAsync(perm))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, perm);
            }
        }

        await base.OnConnectedAsync();
    }
}
