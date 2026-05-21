using System.CodeDom;
using Volo.Abp.Identity;

namespace SupplyCoreERP;

public static class SupplyCoreERPConsts
{
    public const string DbTablePrefix = "App";
    public const string? DbSchema = null;
    public const string AdminEmailDefaultValue = IdentityDataSeedContributor.AdminEmailDefaultValue;
    public const string AdminPasswordDefaultValue = IdentityDataSeedContributor.AdminPasswordDefaultValue;

    // Document types
    public const string DocumentTypeSalesOrder = "SO";
    public const string DocumentTypePurchaseOrder = "PO";
    public const string DocumentTypePurchaseRequisition = "PR";
    public const string DocumentTypeBatch = "BA";
    public const string DocumentTypeManufacturer = "MA";
    public const string DocumentTypeWarehouse = "WH";
    public const string DocumentTypeZone = "ZN";
    public const string DocumentTypeBin = "BN";
    public const string DocumentTypeMedicine = "MD";
    public const string DocumentTypeSupplier = "SP";
    public const string DocumentTypeCustomer = "CU";
    public const string DocumentTypeIngredient = "IG";
    public const string DocumentTypeUnit = "UN";
    public const string DocumentTypeDosageForm = "DF";
    public const string DocumentTypeInventoryTicket = "IT";

}
