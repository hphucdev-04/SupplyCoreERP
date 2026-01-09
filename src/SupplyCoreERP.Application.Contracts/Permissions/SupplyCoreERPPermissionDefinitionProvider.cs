using SupplyCoreERP.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace SupplyCoreERP.Permissions;

public class SupplyCoreERPPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(SupplyCoreERPPermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(SupplyCoreERPPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SupplyCoreERPResource>(name);
    }
}
