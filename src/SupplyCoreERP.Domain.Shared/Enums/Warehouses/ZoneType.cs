using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.Enums.Warehouses;

public enum ZoneType
{
    Storage = 0,        // Khu vực lưu trữ (Kệ hàng)
    Inbound = 1,        // Khu vực Nhập hàng (Dock In)
    Outbound = 2,       // Khu vực Xuất hàng (Dock Out)
    Staging = 3,        // Khu vực Soạn hàng / Đóng gói
    Quarantine = 4,     // Khu vực Biệt trữ / Hàng lỗi
    ForkliftParking = 5, // Bãi đỗ xe nâng
    Office = 6           // Văn phòng
}
