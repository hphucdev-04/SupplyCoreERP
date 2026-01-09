namespace SupplyCoreERP.Enums.Finance
{
	public enum InvoiceStatus { 
		Unpaid = 0, // Chưa thanh toán
		PartiallyPaid = 1, // Thanh toán một phần
		Paid = 2, // Đã thanh toán
		Overdue = 3, // Quá hạn thanh toán
		Cancelled = -1 // Hủy hóa đơn
	}

}
