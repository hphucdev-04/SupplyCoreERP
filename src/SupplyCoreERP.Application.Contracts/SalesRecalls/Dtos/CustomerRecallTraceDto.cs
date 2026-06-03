using System;

namespace SupplyCoreERP.SalesRecalls.Dtos;

public class CustomerRecallTraceDto
{
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }

    public Guid SalesOrderId { get; set; }
    public string SalesOrderCode { get; set; }
    public DateTime SalesOrderDate { get; set; }

    public Guid ProductBatchId { get; set; }
    public string BatchNumber { get; set; }

    public decimal Quantity { get; set; }
    public string UnitName { get; set; }
}
