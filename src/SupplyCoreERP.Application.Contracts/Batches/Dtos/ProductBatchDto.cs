using System;
using SupplyCoreERP.Enums.Warehouses;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Batches.Dtos;

public class ProductBatchDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductCode { get; set; }

    public string BatchNumber { get; set; }
    public DateTime ManufacturingDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }

    public BatchQAStatus Status { get; set; }

    public string? RegistrationNumber { get; set; }
}
