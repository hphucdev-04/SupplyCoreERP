using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.Enums.Warehouses
{
	public enum InventoryTransactionType
	{
		PurchaseReceipt = 0,  // Nhập mua hàng
		SaleDelivery = 1,     // Xuất bán
		ReturnInward = 2,     // Khách trả hàng
		ReturnOutward = 3,    // Trả hàng NCC
		RecallReceipt = 4,    // Nhập thu hồi
		Disposal = 5,         // Xuất hủy
		AdjustmentIn = 6,     // Điều chỉnh tăng (Kiểm kê)
		AdjustmentOut = 7,    // Điều chỉnh giảm (Kiểm kê)
		TransferIn = 8,       // Nhận chuyển kho
		TransferOut = 9      // Xuất chuyển kho
	}
}
