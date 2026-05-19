using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.Enums.Warehouses;

public enum ApprovalStatus
{
    Draft = 0,       // Bản nháp
    Pending = 1,     // Chờ duyệt
    Approved = 2,    // Đã duyệt
    Rejected = 3     // Bị từ chối / Hủy bỏ
}
