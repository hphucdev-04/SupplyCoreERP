namespace SupplyCoreERP.Enums.Orders;

public enum RecallLevel
{
    Level1 = 1, // Mức độ 1: Nguy cơ tổn thương nghiêm trọng hoặc tử vong (Hạn thu hồi tối đa 3 ngày)
    Level2 = 2, // Mức độ 2: Không bảo đảm hiệu quả điều trị hoặc nguy cơ không an toàn (Hạn thu hồi tối đa 15 ngày)
    Level3 = 3  // Mức độ 3: Ít nghiêm trọng, vi phạm nhãn mác cảm quan (Hạn thu hồi tối đa 30 ngày)
}
