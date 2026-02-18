namespace SupplyCoreERP.Enums.Inventory
{
	public enum TransactionStatus { 
		Draft = 0, // Nháp (Chưa ảnh hưởng tồn kho)
		Approved = 1, // Đã duyệt
		Cancelled = -1 // Đã hủy
	}

}
