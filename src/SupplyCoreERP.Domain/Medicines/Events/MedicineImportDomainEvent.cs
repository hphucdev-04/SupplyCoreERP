using System;
using System.Collections.Generic;

namespace SupplyCoreERP.Medicines.Events;

public record MedicineImportDomainEvent(
    List<MedicineImportedItem> Items
);

public record MedicineImportedItem(
    Guid MedicineId,
    string MedicineName,
    string MedicineCode
);
