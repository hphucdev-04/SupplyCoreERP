namespace SupplyCoreERP.Enums.Orders;

public enum PurchaseReturnType
{
    Defective = 1, // Bể vỡ / Lỗi do Nhà cung cấp (Khấu hao bắt buộc = 0%)
    Commercial = 2 // Trả hàng thương mại / Đổi date (Khấu hao tự do)
}
