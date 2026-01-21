using SupplyCoreERP.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace SupplyCoreERP.Permissions;

public class SupplyCoreERPPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
		var catalogPermission = CreateGroupPermission(context, SupplyCoreERPPermissions.Catalog.GroupNameCatalog);
		AddCrudPermissions(catalogPermission, "Catalog", "Category");
		AddCrudPermissions(catalogPermission, "Catalog", "Medicine");

	}

	private void AddCrudPermissions(PermissionDefinition parent, string groupName, string entityName)
	{
		var basePermission = $"{groupName}.{entityName}";

		var entityPermission = parent.AddChild(
			basePermission,
			L($"Permission:{groupName}.{entityName}"));

		entityPermission.AddChild(
			$"{basePermission}.Create",
			L($"Permission:{groupName}.{entityName}.Create"));

		entityPermission.AddChild(
			$"{basePermission}.Update",
			L($"Permission:{groupName}.{entityName}.Update"));

		entityPermission.AddChild(
			$"{basePermission}.Delete",
			L($"Permission:{groupName}.{entityName}.Delete"));
	}

	private PermissionDefinition CreateGroupPermission(IPermissionDefinitionContext context, string groupName)
	{
		var group = context.AddGroup(groupName, L($"Permission:{groupName}"));
		return group.AddPermission(groupName, L($"Permission:{groupName}"));
	}

	private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SupplyCoreERPResource>(name);
    }
}
