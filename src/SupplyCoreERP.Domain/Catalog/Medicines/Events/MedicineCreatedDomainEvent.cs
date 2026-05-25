using System;

namespace SupplyCoreERP.Catalog.Medicines.Events;

public record MedicineCreatedDomainEvent(
    Guid MedicineId,
    string MedicineName,
    string MedicineCode
);







