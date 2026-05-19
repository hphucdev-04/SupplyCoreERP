using System;
using System.Collections.Generic;
using System.Text;
using SupplyCoreERP.Enums.Balances;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Balances.Dtos;

public class InventoryReservationDto : CreationAuditedEntityDto<Guid>
{
    public Guid ReferenceDocumentId { get; set; }
    public string ReferenceDocumentNumber { get; set; }

    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; }

    public Guid BinId { get; set; }
    public string BinCode { get; set; }

    public Guid ProductId { get; set; }
    public Guid ProductBatchId { get; set; }

    public decimal ReservedQuantity { get; set; }
    public ReservationStatus Status { get; set; }
}
