using SupplyCoreERP.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SupplyCoreERP.Permissions;

public class SupplyCoreERPPermissionDefinitionProvider : PermissionDefinitionProvider
{
	public override void Define(IPermissionDefinitionContext context)
	{
		//Tạo Group chính: Catalog
		var catalogGroup = context.AddGroup(SupplyCoreERPPermissions.Catalog.GroupNameCatalog, L("Permission:Catalog"));

		//Category
		AddCrudPermissions(catalogGroup, SupplyCoreERPPermissions.Catalog.Category.Default, "Category");

		//BaseUnit
		AddCrudPermissions(catalogGroup, SupplyCoreERPPermissions.Catalog.BaseUnit.Default, "BaseUnit");

		//DosageForm
		AddCrudPermissions(catalogGroup, SupplyCoreERPPermissions.Catalog.DosageForm.Default, "DosageForm");

		//ActiveIngredient
		AddCrudPermissions(catalogGroup, SupplyCoreERPPermissions.Catalog.ActiveIngredient.Default, "ActiveIngredient");

		//Manufacturer
		AddCrudPermissions(catalogGroup, SupplyCoreERPPermissions.Catalog.Manufacturer.Default, "Manufacturer");

		//Medicine 
		var medicinePerm = AddCrudPermissions(catalogGroup, SupplyCoreERPPermissions.Catalog.Medicine.Default, "Medicine");
		//Thêm quyền Approve 
		medicinePerm.AddChild(
			SupplyCoreERPPermissions.Catalog.Medicine.Approve,
			L("Permission:Catalog.Medicine.Approve")
		);
	}

	/// <summary>
	/// Hàm helper tạo bộ quyền chuẩn: Xem, Thêm, Sửa, Xóa
	/// </summary>
	/// <returns>Trả về permission cha (Xem) để có thể add thêm child nếu cần</returns>
	private PermissionDefinition AddCrudPermissions(PermissionGroupDefinition group, string permissionName, string localizationKey)
	{
		//Quyền mặc định (VIEW/XEM)
		var parentPermission = group.AddPermission(
			permissionName,
			L($"Permission:Catalog.{localizationKey}")
		);

		//Quyền CREATE
		parentPermission.AddChild(
			$"{permissionName}.Create",
			L($"Permission:Catalog.{localizationKey}.Create")
		);

		//Quyền UPDATE
		parentPermission.AddChild(
			$"{permissionName}.Update",
			L($"Permission:Catalog.{localizationKey}.Update")
		);

		//Quyền DELETE
		parentPermission.AddChild(
			$"{permissionName}.Delete",
			L($"Permission:Catalog.{localizationKey}.Delete")
		);

		return parentPermission;
	}

	private static LocalizableString L(string name)
	{
		return LocalizableString.Create<SupplyCoreERPResource>(name);
	}
}