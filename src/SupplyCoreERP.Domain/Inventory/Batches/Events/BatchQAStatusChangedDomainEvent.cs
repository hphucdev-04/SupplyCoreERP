using System;
using SupplyCoreERP.Enums.Warehouses;

namespace SupplyCoreERP.Inventory.Batches.Events;

public record BatchQAStatusChangedDomainEvent(
    Guid BatchId,
    string BatchNumber,
    Guid ProductId,
    BatchQAStatus OldStatus,
    BatchQAStatus NewStatus
);
