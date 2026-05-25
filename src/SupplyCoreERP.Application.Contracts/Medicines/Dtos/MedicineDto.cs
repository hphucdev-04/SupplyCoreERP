using System;
using SupplyCoreERP.Enums.Medicines;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Medicines.Dtos;

public class MedicineDto : EntityDto<Guid>
{
    public string Code { get; set; }
    public string Name { get; set; }

    public Guid BaseUnitId { get; set; }
    public string CategoryName { get; set; }
    public string ManufacturerName { get; set; }
    public string BaseUnitName { get; set; }
    public string DosageFormName { get; set; }
    public string OriginCountryName { get; set; }
    public string OriginCountryISO { get; set; }

    public string RegistrationNumber { get; set; }
    public DateTime? RegistrationValidFrom { get; set; }
    public DateTime? RegistrationValidTo { get; set; }
    public string? RegistrationNote { get; set; }
    public StorageCondition StorageCondition { get; set; }
    public MedicineStatus Status { get; set; }
    public bool IsActive { get; set; }
    public bool HasTransactions { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }

}

