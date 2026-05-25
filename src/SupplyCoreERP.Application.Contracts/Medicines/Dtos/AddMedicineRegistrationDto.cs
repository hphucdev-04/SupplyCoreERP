using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Medicines.Dtos;

public class AddMedicineRegistrationDto
{
    [Required]
    [StringLength(100)]
    public string RegistrationNumber { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public string? Note { get; set; }
}

