using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.Enums.Orders;

public enum SalesOrderStatus
{
    Draft = 1,           // Nháp (Sale đang lên đơn)
    PendingApproval = 2, // Chờ duyệt (Kế toán duyệt công nợ / Giám đốc duyệt giá)
    Approved = 3,        // Đã duyệt (Đẩy xuống Kho chờ xuất hàng)
    Delivering = 4,      // Đang giao hàng (Kho đã xuất, Shipper đang đi giao)
    Completed = 5,       // Hoàn tất (Khách đã nhận đủ hàng)
    Canceled = 6         // Hủy bỏ
}
