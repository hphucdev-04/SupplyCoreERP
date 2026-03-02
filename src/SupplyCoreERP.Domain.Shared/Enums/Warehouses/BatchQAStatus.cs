using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.Enums.Warehouses
{
	public enum BatchQAStatus
	{
		PendingQA = 0,   // Biệt trữ: Chờ kiểm nghiệm (Nằm trong kho nhưng FEFO KHÔNG được bốc)
		Approved = 1,    // Đạt chuẩn: Cho phép xuất bán
		Rejected = 2,    // Rớt kiểm định: Chờ xuất trả NCC
		Recalled = 3,    // Bị thu hồi khẩn cấp: Ngay lập tức đóng băng xuất bán
		Expired = 4      // Đã hết hạn (Hệ thống tự động quét mỗi ngày để cập nhật)
	}
}
