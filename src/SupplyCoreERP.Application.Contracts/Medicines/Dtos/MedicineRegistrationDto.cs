using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Medicines.Dtos;

public class MedicineRegistrationDto : EntityDto<Guid>
{
    public string RegistrationNumber { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
    public DateTime CreationTime { get; set; }
}
