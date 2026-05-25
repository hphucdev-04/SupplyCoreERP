using System;
using System.Collections.Generic;
using System.Text;
using SupplyCoreERP.Enums.Balances;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Balances.Dtos;

public class GetInventoryReservationListDto : PagedAndSortedResultRequestDto
{
    // TÃ¬m theo ÄÆ¡n hÃ ng / Phiáº¿u kho
    public Guid? ReferenceDocumentId { get; set; }
    public string? ReferenceDocumentNumber { get; set; }

    // TÃ¬m theo Tá»“n kho
    public Guid? WarehouseId { get; set; }
    public Guid? BinId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProductBatchId { get; set; }

    // Tráº¡ng thÃ¡i giá»¯ chá»—
    public ReservationStatus? Status { get; set; }
}

