using System;
using System.ComponentModel.DataAnnotations;
using SupplyCoreERP.Enums.Medicines;

namespace SupplyCoreERP.Medicines.Dtos;

public class CreateUpdateMedicineDto
{

    [Required, StringLength(255)]
    public string Name { get; set; }
    [Required]
    public Guid CategoryId { get; set; }
    [Required]
    public Guid ManufacturerId { get; set; }
    [Required]
    public Guid BaseUnitId { get; set; }
    [Required]
    public Guid DosageFormId { get; set; }

    [StringLength(50)]
    public string RegistrationNumber { get; set; }

    public DateTime? RegistrationValidFrom { get; set; }
    public DateTime? RegistrationValidTo { get; set; }
    public string? RegistrationNote { get; set; }

    public UsageRoute UsageRoute { get; set; } = UsageRoute.Oral;
    public StorageCondition StorageCondition { get; set; } = StorageCondition.Normal;
    public bool IsPrescriptionDrug { get; set; }
    public bool IsActive { get; set; } = true;
}

