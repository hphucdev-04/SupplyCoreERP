namespace SupplyCoreERP.Enums.Orders;

public enum SalesRecallStatus
{
    Draft = 1,           // Nháp (Đang soạn)
    PendingApproval = 2, // Chờ duyệt
    Approved = 3,        // Đã duyệt (Chờ nhập kho thu hồi)
    Recalling = 4,       // Đang thu hồi (Đã sinh phiếu kho nhập thu hồi, chờ thực thi)
    Completed = 5,       // Hoàn tất (Đã thực thi nhập kho, giảm nợ khách hàng, khóa thuốc, thu hồi lô)
    Rejected = 6         // Từ chối duyệt
}
