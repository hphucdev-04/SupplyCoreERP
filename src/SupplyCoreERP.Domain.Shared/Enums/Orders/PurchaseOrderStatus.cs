namespace SupplyCoreERP.Enums.Orders;

public enum PurchaseOrderStatus
{
    Draft = 1,           // Nháp (Đang soạn)
    PendingApproval = 2, // Chờ duyệt (Giám đốc duyệt chi)
    Approved = 3,        // Đã duyệt (Được phép gửi cho Supplier)
    Receiving = 4,       // Đang nhận hàng (Đã tạo phiếu kho, chờ nhập)
    Completed = 5,       // Hoàn tất (Đã nhập đủ hàng và xuất hóa đơn)
    Canceled = 6         // Hủy bỏ
}
