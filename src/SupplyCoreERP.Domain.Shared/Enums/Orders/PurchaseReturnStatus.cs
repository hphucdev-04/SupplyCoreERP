namespace SupplyCoreERP.Enums.Orders;

public enum PurchaseReturnStatus
{
    Draft = 1,           // Nháp (Đang soạn)
    PendingApproval = 2, // Chờ duyệt
    Approved = 3,        // Đã duyệt (Chờ xuất kho trả hàng)
    Returning = 4,       // Đang trả hàng (Đã sinh phiếu kho xuất trả, chờ thực thi)
    Completed = 5,       // Hoàn tất (Đã thực thi xuất kho và trừ nợ NCC)
    Rejected = 6         // Từ chối duyệt
}
