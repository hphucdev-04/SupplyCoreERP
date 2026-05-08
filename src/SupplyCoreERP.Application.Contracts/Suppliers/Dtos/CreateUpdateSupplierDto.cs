using System;
using System.ComponentModel.DataAnnotations;
using SupplyCoreERP.Enums.Partner;


namespace SupplyCoreERP.Suppliers.Dtos;

public class CreateUpdateSupplierDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    public string? TaxCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? RepresentativeName { get; set; }
    public Gender? Gender { get; set; }
    public string? Note { get; set; }

    public string? Address { get; set; }
    public Guid? CountryId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? AreaId { get; set; }

    public decimal DebtLimit { get; set; }
    public int PaymentTermDays { get; set; }

    public bool IsActive { get; set; } = true;
}
