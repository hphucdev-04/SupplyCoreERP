namespace SupplyCoreERP.Enums.Medicines
{
	public enum MedicineStatus
	{
		Pending = 0,  // Chờ duyệt (Mới tạo)
		Approved = 1, // Đã duyệt (Được phép sử dụng)
		Rejected = 2  // Từ chối (Sai thông tin, trả về sửa)
	}
}
