namespace SupplyCoreERP.Enums.Orders;

public enum PurchaseRequisitionStatus
{
    Draft = 1,           // Nháp 
    PendingApproval = 2, // Chờ duyệt
    Approved = 3,        // Đã duyệt (Sẵn sàng để chuyển sang PO)
    Rejected = 4,        // Từ chối duyệt
    PartialOrdered = 5,  // Đã đặt hàng một phần (Một số dòng đã sang PO)
    Ordered = 6,         // Đã đặt hàng hết (Tất cả dòng đã sang PO)
    Canceled = 7         // Hủy bỏ
}
