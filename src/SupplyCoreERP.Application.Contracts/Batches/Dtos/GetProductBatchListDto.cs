using System;
using SupplyCoreERP.Enums.Warehouses;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Batches.Dtos;

public class GetProductBatchListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? SupplierId { get; set; }
    public BatchQAStatus? Status { get; set; }
}
