using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Tickets.Dtos;

public class UpdateInventoryTicketDto
{
    [MaxLength(1000)]
    public string? Note { get; set; }
}

