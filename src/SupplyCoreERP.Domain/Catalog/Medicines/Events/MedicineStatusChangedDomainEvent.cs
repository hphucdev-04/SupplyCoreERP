using System;
using SupplyCoreERP.Enums.Medicines;

namespace SupplyCoreERP.Catalog.Medicines.Events;

public record MedicineStatusChangedDomainEvent
(
    Guid MedicineId,
    string MedicineName,
    string MedicineCode,
    MedicineStatus NewStatus
);







