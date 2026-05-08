using System;
using SupplyCoreERP.Enums.Warehouses;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Transactions.Dtos;

public class GetInventoryTransactionListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProductBatchId { get; set; }
    public Guid? BinId { get; set; }
    public Guid? ReferenceDocumentId { get; set; }
    public InventoryTransactionType? TransactionType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
