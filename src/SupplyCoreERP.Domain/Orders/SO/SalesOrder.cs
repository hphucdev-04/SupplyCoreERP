using System;
using System.Collections.Generic;
using System.Linq;
using SupplyCoreERP.Customers;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Inventories.Warehouses;
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

    public virtual ICollection<SalesOrderDetail> Details { get; private set; }

    protected SalesOrder() { Details = new List<SalesOrderDetail>(); }

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
        Details = new List<SalesOrderDetail>();
    }

    public void UpdateInfo(Guid warehouseId, DateTime? expectedDeliveryDate, DateTime? dueDate, string? note)
    {
        if (Status != SalesOrderStatus.Draft && Status != SalesOrderStatus.PendingApproval)
        {
            throw new UserFriendlyException("Chỉ có thể sửa đơn bán khi đang ở trạng thái Nháp hoặc Chờ duyệt.");
        }

        WarehouseId = warehouseId;
        ExpectedDeliveryDate = expectedDeliveryDate;
        DueDate = dueDate;
        Note = note;
    }

    #region SaleOrder Detials
    public SalesOrderDetail AddDetail(Guid id, Guid productId, Guid unitId, int conversionFactor, decimal quantity, decimal unitPrice, decimal discountRate, decimal taxRate)
    {
        if (Status != SalesOrderStatus.Draft && Status != SalesOrderStatus.PendingApproval)
        {
            throw new UserFriendlyException("Chỉ được thêm chi tiết khi đơn đang Nháp hoặc Chờ duyệt.");
        }

        var detail = new SalesOrderDetail(id, Id, productId, unitId, conversionFactor, quantity, unitPrice, discountRate, taxRate);
        Details.Add(detail);

        RecalculateTotal();
        return detail;
    }

    public void UpdateDetail(Guid detailId, decimal quantity, decimal unitPrice, decimal discountRate, decimal taxRate)
    {
        if (Status != SalesOrderStatus.Draft && Status != SalesOrderStatus.PendingApproval)
        {
            throw new UserFriendlyException("Không thể sửa chi tiết khi đơn đã duyệt.");
        }

        SalesOrderDetail? detail = Details.FirstOrDefault(x => x.Id == detailId);
        if (detail == null)
        {
            throw new UserFriendlyException("Không tìm thấy dòng chi tiết.");
        }

        detail.UpdateInfo(quantity, unitPrice, discountRate, taxRate);
        RecalculateTotal();
    }

    public void RemoveDetail(Guid detailId)
    {
        if (Status != SalesOrderStatus.Draft && Status != SalesOrderStatus.PendingApproval)
        {
            throw new UserFriendlyException("Không thể xóa chi tiết khi đơn đã duyệt.");
        }

        SalesOrderDetail? detail = Details.FirstOrDefault(x => x.Id == detailId);
        if (detail == null)
        {
            throw new UserFriendlyException("Không tìm thấy dòng chi tiết.");
        }

        Details.Remove(detail);
        RecalculateTotal();
    }
    #endregion

    #region Work flow
    public void SendToApprove()
    {
        if (!Details.Any())
        {
            throw new UserFriendlyException("Đơn bán hàng chưa có sản phẩm nào!");
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
            throw new UserFriendlyException(
                "Chỉ có thể bắt đầu giao hàng khi đơn đang ở trạng thái Đã duyệt!");
        }

        Status = SalesOrderStatus.Delivering;
    }
    public void Complete()
    {
        Status = SalesOrderStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == SalesOrderStatus.Completed)
        {
            throw new UserFriendlyException("Đơn đã giao xong, không thể hủy!");
        }

        if (Status == SalesOrderStatus.Delivering)
        {
            throw new UserFriendlyException("Hàng đang giao, Kho phải thu hồi trước khi hủy!");
        }

        Status = SalesOrderStatus.Canceled;
    }
    #endregion

    #region Helper 
    private void RecalculateTotal()
    {
        SubTotal = Details.Sum(x => x.TotalPrice);
        DiscountAmount = Details.Sum(x => x.DiscountAmount);
        TaxAmount = Details.Sum(x => x.TaxAmount);
        TotalAmount = SubTotal - DiscountAmount + TaxAmount;
    }
    #endregion
}
