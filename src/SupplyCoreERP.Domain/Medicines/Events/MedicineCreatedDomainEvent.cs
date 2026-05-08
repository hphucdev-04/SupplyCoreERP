using System;

namespace SupplyCoreERP.Medicines.Events;

public record MedicineCreatedDomainEvent(
    Guid MedicineId,
    string MedicineName,
    string MedicineCode
);
