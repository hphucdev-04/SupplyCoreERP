namespace SupplyCoreERP.Enums.Orders
{
	public enum OrderStatus { 
		New = 0, // Đơn hàng mới tạo
		Confirmed = 1, // Đơn hàng đã được xác nhận
		Shipping = 2, // Đơn hàng đang trong quá trình vận chuyển
		Completed = 3, // Đơn hàng đã hoàn thành
		Cancelled = -1 // Đơn hàng đã bị hủy
	}

}
