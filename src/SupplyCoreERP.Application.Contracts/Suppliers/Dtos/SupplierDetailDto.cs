using System;
using SupplyCoreERP.Enums.Partner;

namespace SupplyCoreERP.Suppliers.Dtos;

public class SupplierDetailDto : SupplierDto
{
    public string? TaxCode { get; set; }
    public string? RepresentativeName { get; set; }
    public Gender? Gender { get; set; }
    public string? Note { get; set; }

    public string? Address { get; set; }
    public Guid? AreaId { get; set; }
    public string? AreaName { get; set; }

    public decimal DebtLimit { get; set; }
    public int PaymentTermDays { get; set; }
}
