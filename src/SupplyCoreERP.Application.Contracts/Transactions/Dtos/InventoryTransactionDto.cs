using System;
using System.Collections.Generic;
using System.Text;
using SupplyCoreERP.Enums.Warehouses;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Transactions.Dtos;

public class InventoryTransactionDto : CreationAuditedEntityDto<Guid>
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; }

    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductCode { get; set; }

    public Guid ProductBatchId { get; set; }
    public string BatchNumber { get; set; }

    public Guid BinId { get; set; }
    public string BinCode { get; set; }

    public InventoryTransactionType TransactionType { get; set; }

    public decimal Quantity { get; set; }
    public decimal BalanceAfterTransaction { get; set; }

    public Guid? ReferenceDocumentId { get; set; }
    public string? ReferenceDocumentNumber { get; set; }

    public string Note { get; set; }
}
