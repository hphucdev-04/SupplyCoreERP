namespace SupplyCoreERP.Enums.Orders;

public enum PurchaseReturnRequestStatus
{
    Draft = 1,           // Nháp (Đang soạn)
    PendingApproval = 2, // Chờ duyệt
    Approved = 3,        // Đã duyệt (Đồng ý gom/tách đơn con)
    Rejected = 4,        // Từ chối duyệt
    Processed = 5        // Đã xử lý (Đã gom nhóm & sinh ra các PurchaseReturn con thành công)
}
