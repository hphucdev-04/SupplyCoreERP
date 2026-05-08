using System;
using System.ComponentModel.DataAnnotations;
using SupplyCoreERP.Enums.Warehouses;


namespace SupplyCoreERP.Tickets.Dtos;

public class CreateInventoryTicketDto
{
    [Required]
    public TicketType Type { get; set; }

    [Required]
    public Guid WarehouseId { get; set; }

    public Guid? ReferenceDocumentId { get; set; }
    public string? ReferenceDocumentNumber { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }
}
