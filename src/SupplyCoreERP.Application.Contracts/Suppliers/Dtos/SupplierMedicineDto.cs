using System;

namespace SupplyCoreERP.Suppliers.Dtos;

public class SupplierMedicineDto
{
    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; }
    public string SupplierName { get; set; }
    public Guid? CountryId { get; set; }
    public string? CountryName { get; set; }

    public decimal StandardPrice { get; set; }
    public int LeadTimeDays { get; set; }
    public decimal MinOrderQuantity { get; set; }
    public string DefaultUnitName { get; set; }
    public bool IsPreferred { get; set; }
}
