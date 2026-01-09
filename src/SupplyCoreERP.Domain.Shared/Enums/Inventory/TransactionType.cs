namespace SupplyCoreERP.Enums.Inventory
{
	public enum TransactionType
	{
		Import = 1,         // Nhập mua (Tăng kho)
		Export = 2,         // Xuất bán (Giảm kho)
		Transfer = 3,       // Chuyển kho (Giảm A Tăng B)
		AdjustmentUp = 4,   // Kiểm kê thừa (Tăng kho)
		AdjustmentDown = 5, // Kiểm kê thiếu (Giảm kho)
		Scrap = 6           // Hủy hàng (Giảm kho)
	}

}
