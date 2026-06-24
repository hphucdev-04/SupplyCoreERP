namespace SupplyCoreERP.Permissions;

public static class SupplyCoreERPPermissions
{
    // Core Permission
    public const string GroupName = "SupplyCoreERP";

    #region Catalog Permission
    public static class Catalog
    {
        public const string GroupNameCatalog = "Catalog";

        //Category Permisstion
        public static class Category
        {
            public const string Default = GroupNameCatalog + ".Category";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
        }

        //Medicine Permisstion
        public static class Medicine
        {
            public const string Default = GroupNameCatalog + ".Medicine";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
            public const string Approve = Default + ".Approve";
            public const string Reject = Default + ".Reject";
        }

        //BaseUnit Permisstion
        public static class BaseUnit
        {
            public const string Default = GroupNameCatalog + ".BaseUnit";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
        }

        //DosageForm Permisstion
        public static class DosageForm
        {
            public const string Default = GroupNameCatalog + ".DosageForm";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
        }

        //ActiveIngredient Permisstion
        public static class ActiveIngredient
        {
            public const string Default = GroupNameCatalog + ".ActiveIngredient";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
        }

        //Manufacturer Permisstion
        public static class Manufacturer
        {
            public const string Default = GroupNameCatalog + ".Manufacturer";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
        }
    }
    #endregion

    #region Partner Permission
    public static class Partner
    {
        public const string GroupNamePartner = "Partner";

        // Customer Permisstion
        public static class Customer
        {
            public const string Default = GroupNamePartner + ".Customer";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
        }

        // Supplier Permisstion
        public static class Supplier
        {
            public const string Default = GroupNamePartner + ".Supplier";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
        }
    }
    #endregion

    #region Inventory Permission
    public static class Inventory
    {
        public const string GroupNameInventory = "Inventory";

        // Warehouse Permission
        public static class Warehouse
        {
            public const string Default = GroupNameInventory + ".Warehouse";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
            public const string Approve = Default + ".Approve";
            public const string Reject = Default + ".Reject";
            public const string ZoneTransfer = Default + ".ZoneTransfer";
        }

        // Batch Permission
        public static class Batch
        {
            public const string Default = GroupNameInventory + ".Batch";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
        }

        // Ticket Permission
        public static class Ticket
        {
            public const string Default = GroupNameInventory + ".Ticket";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
            public const string Approve = Default + ".Approve";
            public const string Reject = Default + ".Reject";
        }
    }
    #endregion

    #region Order Permission
    public static class Order
    {
        public const string GroupNameOrder = "Order";

        // PurchaseOrder Permission
        public static class PurchaseOrder
        {
            public const string Default = GroupNameOrder + ".PurchaseOrder";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
            public const string Approve = Default + ".Approve";
            public const string Reject = Default + ".Reject";
        }

        // PurchaseRequisition Permission
        public static class PurchaseRequisition
        {
            public const string Default = GroupNameOrder + ".PurchaseRequisition";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
            public const string Approve = Default + ".Approve";
            public const string Reject = Default + ".Reject";
        }

        // Sale Order Permission
        public static class SaleOrder
        {
            public const string Default = GroupNameOrder + ".SaleOrder";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
            public const string Approve = Default + ".Approve";
            public const string Reject = Default + ".Reject";
            public const string OverrideUnitPrice = Default + ".OverrideUnitPrice";
        }

        // PurchaseReturn Permission
        public static class PurchaseReturn
        {
            public const string Default = GroupNameOrder + ".PurchaseReturn";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
            public const string Approve = Default + ".Approve";
            public const string Reject = Default + ".Reject";
        }

        // PurchaseReturnRequest Permission
        public static class PurchaseReturnRequest
        {
            public const string Default = GroupNameOrder + ".PurchaseReturnRequest";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
            public const string Approve = Default + ".Approve";
            public const string Reject = Default + ".Reject";
        }

        // SalesRecall Permission
        public static class SalesRecall
        {
            public const string Default = GroupNameOrder + ".SalesRecall";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
            public const string Approve = Default + ".Approve";
            public const string Reject = Default + ".Reject";
        }
    }
    #endregion
}

