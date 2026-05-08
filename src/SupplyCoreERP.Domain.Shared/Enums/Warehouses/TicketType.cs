namespace SupplyCoreERP.Enums.Warehouses;

public enum TicketType
{
    GoodsReceipt = 0,    // Phiếu Nhập kho (Mua từ NCC)
    GoodsIssue = 1,      // Phiếu Xuất kho (Bán cho Khách)
    ReturnInward = 2,    // Phiếu Khách trả hàng (Nhập lại)
    ReturnOutward = 3,   // Phiếu Trả NCC (Xuất đi)
    RecallReceipt = 4,   // Phiếu Nhập thu hồi hàng lỗi
    DisposalIssue = 5    // Phiếu Xuất hủy (Hàng hỏng, hết date)
}
