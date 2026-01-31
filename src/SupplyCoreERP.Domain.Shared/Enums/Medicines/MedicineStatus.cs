namespace SupplyCoreERP.Enums.Medicines
{
	public enum MedicineStatus
	{
		Pending = 1,  // Chờ duyệt (Mới tạo)
		Approved = 2, // Đã duyệt (Được phép sử dụng)
		Rejected = 3  // Từ chối (Sai thông tin, trả về sửa)
	}
}
