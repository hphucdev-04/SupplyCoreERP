using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.Enums.Warehouses
{
	public enum ApprovalStatus
	{
		Draft = 0,       // Bản nháp (Đang soạn hoặc do hệ thống tự sinh ra)
		Pending = 1,     // Chờ duyệt
		Approved = 2,    // Đã duyệt (Đối với Kho: Được hoạt động. Đối với Phiếu: Đã xuất/nhập thành công)
		Rejected = 3     // Bị từ chối / Hủy bỏ
	}
}
