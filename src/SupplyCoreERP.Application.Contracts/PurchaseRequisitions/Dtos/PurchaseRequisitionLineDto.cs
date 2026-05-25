using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.PurchaseRequisitions.Dtos;

public class PurchaseRequisitionLineDto : AuditedEntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public Guid UnitId { get; set; }
    public string UnitName { get; set; }
    public decimal Quantity { get; set; }
    public decimal OrderedQuantity { get; set; }
    public string? Note { get; set; }
}

