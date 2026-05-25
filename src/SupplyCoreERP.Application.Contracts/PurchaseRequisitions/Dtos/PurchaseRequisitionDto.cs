using System;
using System.Collections.Generic;
using SupplyCoreERP.Enums.Orders;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.PurchaseRequisitions.Dtos;

public class PurchaseRequisitionDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; }
    public DateTime RequestedDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public PurchaseRequisitionStatus Status { get; set; }
    public string? Note { get; set; }
    public List<PurchaseRequisitionLineDto> Lines { get; set; }
    public List<RelatedPurchaseOrderDto> RelatedOrders { get; set; }
}

