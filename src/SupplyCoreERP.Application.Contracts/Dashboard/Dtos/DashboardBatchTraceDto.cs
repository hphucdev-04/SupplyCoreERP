using System;
using System.Collections.Generic;

namespace SupplyCoreERP.Dashboard.Dtos;

public class DashboardBatchTraceDto
{
    public Guid BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string MedicineCode { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public DateTime ManufacturingDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public decimal TotalOnHand { get; set; }
    public decimal TotalReserved { get; set; }

    public List<DashboardBatchTraceBalanceDto> Balances { get; set; } = new();
    public List<DashboardBatchTraceReceiptDto> Receipts { get; set; } = new();
    public List<DashboardBatchTraceDeliveryDto> Deliveries { get; set; } = new();
    public List<DashboardBatchTraceOtherDto> OtherTransactions { get; set; } = new();
}

public class DashboardBatchTraceBalanceDto
{
    public string WarehouseName { get; set; } = string.Empty;
    public string BinCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public class DashboardBatchTraceReceiptDto
{
    public string SupplierName { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public string PoNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Quantity { get; set; }
}

public class DashboardBatchTraceDeliveryDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public string SoNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Quantity { get; set; }
}

public class DashboardBatchTraceOtherDto
{
    public string TransactionType { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Quantity { get; set; }
    public string Note { get; set; } = string.Empty;
}
