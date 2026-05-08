using System;
using SupplyCoreERP.Enums.Medicines;

namespace SupplyCoreERP.Medicines.Events;

public record MedicineStatusChangedDomainEvent
(
    Guid MedicineId,
    string MedicineName,
    string MedicineCode,
    MedicineStatus NewStatus
);
