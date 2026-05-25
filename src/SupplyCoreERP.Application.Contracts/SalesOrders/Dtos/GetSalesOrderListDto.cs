using System;
using System.Collections.Generic;
using System.Text;
using SupplyCoreERP.Enums.Orders;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.SalesOrders.Dtos;

public class GetSalesOrderListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? WarehouseId { get; set; }
    public SalesOrderStatus? Status { get; set; }
}

