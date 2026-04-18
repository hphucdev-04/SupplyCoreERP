using SupplyCoreERP.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SupplyCoreERP.Permissions;

public class SupplyCoreERPPermissionDefinitionProvider : PermissionDefinitionProvider
{
	public override void Define(IPermissionDefinitionContext context)
	{
        #region Catalog Permission
        // Parent Group
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
		var medicinePermision = AddCrudPermissions(catalogGroup, SupplyCoreERPPermissions.Catalog.Medicine.Default, "Medicine");
			// Add Child
			medicinePermision.AddChild(
				SupplyCoreERPPermissions.Catalog.Medicine.Approve,
				L("Permission:Catalog.Medicine.Approve")
			);
			medicinePermision.AddChild(
				SupplyCoreERPPermissions.Catalog.Medicine.Reject,
				L("Permission:Catalog.Medicine.Reject")
			);
        #endregion

        #region Partner Permission
        // Parent Group
        var partnerGroup = context.AddGroup(SupplyCoreERPPermissions.Partner.GroupNamePartner, L("Permission:Partner"));

		// Customer
        AddCrudPermissions(partnerGroup, SupplyCoreERPPermissions.Partner.Customer.Default, "Customer");

		// Supplier
        AddCrudPermissions(partnerGroup, SupplyCoreERPPermissions.Partner.Supplier.Default, "Supplier");
        #endregion

        #region Inventory Permission
        // Parent Group
        var inventoryGroup = context.AddGroup(SupplyCoreERPPermissions.Inventory.GroupNameInventory, L("Permission:Inventory"));

		// Warehouse
        var warehousePermission = AddCrudPermissions(inventoryGroup, SupplyCoreERPPermissions.Inventory.Warehouse.Default, "Warehouse");
			// Add Child
			warehousePermission.AddChild(
				SupplyCoreERPPermissions.Inventory.Warehouse.Approve,
				L("Permission:Inventory.Warehous.Approve")
			);
			warehousePermission.AddChild(
				SupplyCoreERPPermissions.Inventory.Warehouse.Reject,
				L("Permission:Inventory.Warehous.Reject")
			);

        // Batch
        AddCrudPermissions(inventoryGroup, SupplyCoreERPPermissions.Inventory.Warehouse.Default, "Batch");

		// Ticket
        var ticketPermission = AddCrudPermissions(inventoryGroup, SupplyCoreERPPermissions.Inventory.Warehouse.Default, "Ticket");
			// Add Child
			ticketPermission.AddChild(
					SupplyCoreERPPermissions.Inventory.Ticket.Approve,
					L("Permission:Inventory.Ticket.Approve")
			);
			ticketPermission.AddChild(
				SupplyCoreERPPermissions.Inventory.Ticket.Reject,
				L("Permission:Inventory.Ticket.Reject")
			);
        #endregion

        #region Order Permission
        // Parent Group
        var orderGroup = context.AddGroup(SupplyCoreERPPermissions.Order.GroupNameOrder, L("Permission:Order"));

        // PurchaseOrder
        var purchaseOrderPermission = AddCrudPermissions(orderGroup, SupplyCoreERPPermissions.Order.PurchaseOrder.Default, "PurchaseOrder");
			// Add Child
			purchaseOrderPermission.AddChild(
					SupplyCoreERPPermissions.Order.PurchaseOrder.Approve,
					L("Permission:Order.PurchaseOrder.Approve")
			);
			purchaseOrderPermission.AddChild(
				SupplyCoreERPPermissions.Order.PurchaseOrder.Reject,
				L("Permission:Order.PurchaseOrder.Reject")
			);

        // SaleOrder
        var saleOrderPermisson = AddCrudPermissions(orderGroup, SupplyCoreERPPermissions.Order.SaleOrder.Default, "SaleOrder");
			// Add Child
			saleOrderPermisson.AddChild(
					SupplyCoreERPPermissions.Order.SaleOrder.Approve,
					L("Permission:Order.SaleOrder.Approve")
			);
			saleOrderPermisson.AddChild(
				SupplyCoreERPPermissions.Order.SaleOrder.Reject,
				L("Permission:Order.SaleOrder.Reject")
			);
        #endregion


    }

    /// <summary>
    ///	CRUD Permission Initialization Method (can add child)
    /// </summary>
	/// <param>
	/// group: parent permission
	/// permissionName: permission key
	/// localizationKey: localizaion key 
	/// </param>
    /// <returns>
	/// permission child belong parent
	/// </returns>
    private PermissionDefinition AddCrudPermissions(PermissionGroupDefinition group, string permissionName, string localizationKey)
	{
		// Default (View) 
		var parentPermission = group.AddPermission(
			permissionName,
			L($"Permission:Catalog.{localizationKey}")
		);

		// Create
		parentPermission.AddChild(
			$"{permissionName}.Create",
			L($"Permission:Catalog.{localizationKey}.Create")
		);

		// Update
		parentPermission.AddChild(
			$"{permissionName}.Update",
			L($"Permission:Catalog.{localizationKey}.Update")
		);

		// Delete
		parentPermission.AddChild(
			$"{permissionName}.Delete",
			L($"Permission:Catalog.{localizationKey}.Delete")
		);

		return parentPermission;
	}

    // Localization Mapping
    private static LocalizableString L(string name)
	{
		return LocalizableString.Create<SupplyCoreERPResource>(name);
	}
}