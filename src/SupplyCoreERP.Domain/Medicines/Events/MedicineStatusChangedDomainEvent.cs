using SupplyCoreERP.Enums.Medicines;
using System;

namespace SupplyCoreERP.Medicines.Events
{
    public record MedicineStatusChangedDomainEvent
    (
        Guid MedicineId,
        string MedicineName,
        string MedicineCode,
        MedicineStatus NewStatus
    );
}
