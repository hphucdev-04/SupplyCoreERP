using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Warehouses;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Inventory.Tickets;

public interface ITicketManager : IDomainService
{
    Task<bool> HasStatusAsync(Guid referenceId, ApprovalStatus status);

    // Ticket
    Task<InventoryTicket> CreateTicketAsync(
        TicketType type,
        Guid warehouseId,
        Guid? referenceDocumentId,
        string? referenceDocumentNumber,
        string? note);

    void UpdateTicket(InventoryTicket ticket, string? note);
    Task ValidateBeforeDeleteAsync(InventoryTicket ticket);

    // Ticket Line
    Task<InventoryTicketLine> CreateTicketLineAsync(
        InventoryTicket ticket,
        Guid productId,
        Guid? referenceDocumentLineId,
        decimal quantity,
        Guid? unitId = null,
        int? conversionFactor = null);

    void UpdateLineQuantity(InventoryTicket ticket, InventoryTicketLine line, decimal newQuantity);

    // Ticket Detail
    Task<InventoryTicketDetail> CreateTicketDetailAsync(
        InventoryTicket ticket,
        InventoryTicketLine line,
        Guid productId,
        Guid productBatchId,
        Guid binId,
        Guid unitId,
        int conversionFactor,
        decimal quantity);

    Task UpdateDetailQuantityAsync(
        InventoryTicket ticket,
        InventoryTicketLine line,
        InventoryTicketDetail detail,
        decimal actualQuantity);

    Task RemoveTicketDetailAsync(
        InventoryTicket ticket,
        InventoryTicketLine line,
        InventoryTicketDetail detail);

    // Workflow
    Task SendToApproveAsync(InventoryTicket ticket);
    Task RejectTicketAsync(InventoryTicket ticket, string rejectReason);
    Task ExecuteTicketAsync(InventoryTicket ticket);

    // FEFO
    Task AllocateFEFOForLineAsync(InventoryTicket ticket, InventoryTicketLine line);
}
