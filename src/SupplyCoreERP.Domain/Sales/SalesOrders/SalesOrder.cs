using System;
using System.Collections.Generic;
using System.Linq;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Inventory.Warehouses;
using SupplyCoreERP.Partner.Customers;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Sales.Orders;

public class SalesOrder : FullAuditedAggregateRoot<Guid>
{
    public string Code { get; private set; }
    public Guid CustomerId { get; private set; }
    public virtual Customer Customer { get; private set; }

    public DateTime OrderDate { get; private set; }
    public DateTime? ExpectedDeliveryDate { get; private set; }
    public DateTime? DueDate { get; private set; }
    public SalesOrderStatus Status { get; private set; }

    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }

    public string? Note { get; private set; }
    public Guid WarehouseId { get; private set; }
    public virtual Warehouse Warehouse { get; private set; }

    public virtual ICollection<SalesOrderLine> Lines { get; private set; }

    protected SalesOrder() { Lines = new List<SalesOrderLine>(); }

    public SalesOrder(Guid id, string code, Guid customerId, Guid warehouseId, DateTime orderDate, DateTime? expectedDeliveryDate, DateTime? dueDate, string? note) : base(id)
    {
        Code = code;
        CustomerId = customerId;
        WarehouseId = warehouseId;
        OrderDate = orderDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
        DueDate = dueDate;
        Note = note;
        Status = SalesOrderStatus.Draft;
        SubTotal = 0;
        DiscountAmount = 0;
        TaxAmount = 0;
        TotalAmount = 0;
        Lines = new List<SalesOrderLine>();
    }

    public void UpdateInfo(Guid warehouseId, DateTime? expectedDeliveryDate, DateTime? dueDate, string? note)
    {
        if (Status != SalesOrderStatus.Draft && Status != SalesOrderStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidPrice", "Giá bán không được nhỏ hơn 0!");
        }

        WarehouseId = warehouseId;
        ExpectedDeliveryDate = expectedDeliveryDate;
        DueDate = dueDate;
        Note = note;
    }

    #region SaleOrder Lines
    public SalesOrderLine AddLine(Guid id, Guid productId, Guid unitId, int conversionFactor, decimal quantity, decimal unitPrice, decimal discountRate, decimal taxRate)
    {
        if (Status != SalesOrderStatus.Draft && Status != SalesOrderStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidPrice", "Giá bán không được nhỏ hơn 0!");
        }

        SalesOrderLine line = new(id, Id, productId, unitId, conversionFactor, quantity, unitPrice, discountRate, taxRate);
        Lines.Add(line);

        RecalculateTotal();
        return line;
    }

    public void UpdateLine(Guid lineId, decimal quantity, decimal unitPrice, decimal discountRate, decimal taxRate)
    {
        if (Status != SalesOrderStatus.Draft && Status != SalesOrderStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidPrice", "Giá bán không được nhỏ hơn 0!");
        }

        SalesOrderLine? line = Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new BusinessException("SupplyCoreERP:InvalidPrice", "Giá bán không được nhỏ hơn 0!");
        }

        line.UpdateInfo(quantity, unitPrice, discountRate, taxRate);
        RecalculateTotal();
    }

    public void RemoveLine(Guid lineId)
    {
        if (Status != SalesOrderStatus.Draft && Status != SalesOrderStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidPrice", "Giá bán không được nhỏ hơn 0!");
        }

        SalesOrderLine? line = Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new BusinessException("SupplyCoreERP:InvalidPrice", "Giá bán không được nhỏ hơn 0!");
        }

        Lines.Remove(line);
        RecalculateTotal();
    }
    #endregion

    #region Work flow
    public void SendToApprove()
    {
        if (!Lines.Any())
        {
            throw new BusinessException("SupplyCoreERP:InvalidPrice", "Giá bán không được nhỏ hơn 0!");
        }

        Status = SalesOrderStatus.PendingApproval;
    }
    public void Approve()
    {
        Status = SalesOrderStatus.Approved;
    }
    public void StartDelivering()
    {
        if (Status != SalesOrderStatus.Approved)
        {
            throw new BusinessException("SupplyCoreERP:InvalidPrice", "Giá bán không được nhỏ hơn 0!");
        }

        Status = SalesOrderStatus.Delivering;
    }
    public void Complete()
    {
        Status = SalesOrderStatus.Completed;
    }
    #endregion

    #region Helper 
    private void RecalculateTotal()
    {
        SubTotal = Lines.Sum(x => x.TotalPrice);
        DiscountAmount = Lines.Sum(x => x.DiscountAmount);
        TaxAmount = Lines.Sum(x => x.TaxAmount);
        TotalAmount = SubTotal - DiscountAmount + TaxAmount;
    }
    #endregion
}






