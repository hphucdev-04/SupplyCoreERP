using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Balances.Dtos;

public class GetInventoryBalanceListDto : PagedAndSortedResultRequestDto
{
    public Guid? WarehouseId { get; set; }
    public Guid? BinId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProductBatchId { get; set; }
    public string? BatchNumber { get; set; }
    public bool? IsNearExpiry { get; set; } // Lá»c thuá»‘c sáº¯p háº¿t háº¡n (VD: CÃ²n dÆ°á»›i 6 thÃ¡ng)
    public bool? HideZeroQuantity { get; set; } = true; // Máº·c Ä‘á»‹nh áº©n cÃ¡c ká»‡ Ä‘Ã£ háº¿t hÃ ng
}

