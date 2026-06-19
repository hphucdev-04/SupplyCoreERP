using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Balances;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Inventory.Transactions;
using SupplyCoreERP.Partner.Customers;
using SupplyCoreERP.Procurement.PurchaseOrders;
using SupplyCoreERP.Procurement.PurchaseReturns;
using SupplyCoreERP.Sales.Orders;
using SupplyCoreERP.Sales.SalesRecalls;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Inventory.Balances;

public class InventoryBalanceManager : DomainService
{
    // Dependencies
    private readonly IRepository<InventoryBalance, Guid> _balanceRepo;
    private readonly IRepository<InventoryReservation, Guid> _reservationRepo;
    private readonly IRepository<InventoryTransaction, Guid> _transactionRepo;
    private readonly IRepository<InventoryTicketLine, Guid> _ticketLineRepo;
    private readonly IRepository<PurchaseOrder, Guid> _poRepo;
    private readonly IRepository<SalesOrder, Guid> _soRepo;
    private readonly IRepository<PurchaseReturn, Guid> _purchaseReturnRepo;
    private readonly IRepository<SalesRecall, Guid> _salesRecallRepo;
    private readonly IRepository<Customer, Guid> _customerRepo;

    // Constructor injection
    public InventoryBalanceManager(
        IRepository<InventoryBalance, Guid> balanceRepo,
        IRepository<InventoryReservation, Guid> reservationRepo,
        IRepository<InventoryTransaction, Guid> transactionRepo,
        IRepository<InventoryTicketLine, Guid> ticketLineRepo,
        IRepository<PurchaseOrder, Guid> poRepo,
        IRepository<SalesOrder, Guid> soRepo,
        IRepository<PurchaseReturn, Guid> purchaseReturnRepo,
        IRepository<SalesRecall, Guid> salesRecallRepo,
        IRepository<Customer, Guid> customerRepo)
    {
        _balanceRepo = balanceRepo;
        _reservationRepo = reservationRepo;
        _transactionRepo = transactionRepo;
        _ticketLineRepo = ticketLineRepo;
        _poRepo = poRepo;
        _soRepo = soRepo;
        _purchaseReturnRepo = purchaseReturnRepo;
        _salesRecallRepo = salesRecallRepo;
        _customerRepo = customerRepo;
    }

    private async Task<List<InventoryBalance>> GetBalancesWithDetailsAsync(
        IEnumerable<InventoryTicketDetail> details, Guid warehouseId)
    {
        List<Guid> productIds = details.Select(d => d.ProductId).Distinct().ToList();
        List<Guid> batchIds = details.Select(d => d.ProductBatchId).Distinct().ToList();

        var query = await _balanceRepo.WithDetailsAsync(x => x.BinBalances);

        return query.Where(x =>
            x.WarehouseId == warehouseId &&
            productIds.Contains(x.ProductId) &&
            batchIds.Contains(x.ProductBatchId))
            .ToList();
    }

    private async Task<(Guid? PartnerId, string? PartnerName, Guid? SourceDocId, string? SourceDocNumber)> GetPartnerAndDocInfoAsync(
        InventoryTicket ticket, Guid? referenceDocumentLineId)
    {
        if (!ticket.ReferenceDocumentId.HasValue)
        {
            return (null, null, null, null);
        }

        Guid refId = ticket.ReferenceDocumentId.Value;

        try
        {
            switch (ticket.Type)
            {
                case TicketType.GoodsReceipt: // Nhập mua hàng từ NCC
                    {
                        var poQuery = await _poRepo.WithDetailsAsync(x => x.Supplier);
                        var po = poQuery.FirstOrDefault(x => x.Id == refId);
                        if (po != null)
                        {
                            return (po.SupplierId, po.Supplier?.Name, po.Id, po.Code);
                        }
                    }
                    break;

                case TicketType.GoodsIssue: // Xuất bán hàng cho khách
                case TicketType.ReturnInward: // Khách trả hàng (mặc định map theo SalesOrder)
                    {
                        var soQuery = await _soRepo.WithDetailsAsync(x => x.Customer);
                        var so = soQuery.FirstOrDefault(x => x.Id == refId);
                        if (so != null)
                        {
                            return (so.CustomerId, so.Customer?.Name, so.Id, so.Code);
                        }
                    }
                    break;

                case TicketType.ReturnOutward: // Xuất trả hàng NCC
                    {
                        var prQuery = await _purchaseReturnRepo.WithDetailsAsync(x => x.Supplier);
                        var pr = prQuery.FirstOrDefault(x => x.Id == refId);
                        if (pr != null)
                        {
                            return (pr.SupplierId, pr.Supplier?.Name, pr.Id, pr.Code);
                        }
                    }
                    break;

                case TicketType.RecallReceipt: // Nhập thu hồi từ khách hàng
                    {
                        if (referenceDocumentLineId.HasValue)
                        {
                            var recallQuery = await _salesRecallRepo.WithDetailsAsync(x => x.Lines);
                            var recall = recallQuery.FirstOrDefault(x => x.Id == refId);

                            var line = recall?.Lines.FirstOrDefault(l => l.Id == referenceDocumentLineId.Value);
                            if (line != null)
                            {
                                var customer = await _customerRepo.FindAsync(line.CustomerId);
                                return (line.CustomerId, customer?.Name, recall.Id, recall.Code);
                            }
                        }
                    }
                    break;
            }
        }
        catch (Exception)
        {
            // Fallback phòng trường hợp DB chưa sẵn sàng hoặc lỗi truy vấn
            return (null, null, null, null);
        }

        return (null, null, null, null);
    }

    public async Task LockStockAsync(InventoryTicket ticket, IEnumerable<InventoryTicketDetail> details)
    {
        List<InventoryBalance> balances = await GetBalancesWithDetailsAsync(details, ticket.WarehouseId);
        List<InventoryReservation> reservations = new();
        List<InventoryBalance> balancesToUpdate = new();

        // Load trước danh sách ticket lines để tránh n+1 queries
        var lineIds = details.Select(d => d.TicketLineId).Distinct().ToList();
        List<InventoryTicketLine> ticketLines = await _ticketLineRepo.GetListAsync(x => lineIds.Contains(x.Id));

        foreach (InventoryTicketDetail item in details)
        {
            var balance = balances.FirstOrDefault(x => x.ProductId == item.ProductId && x.ProductBatchId == item.ProductBatchId);
            var binBalance = balance?.BinBalances.FirstOrDefault(x => x.BinId == item.BinId);
            
            if (balance == null || binBalance == null || binBalance.AvailableQuantity < item.BaseQuantity)
            {
                throw new BusinessException("SupplyCoreERP:OutOfStock", $"Không đủ tồn kho khả dụng dành cho sản phẩm ID {item.ProductId} tại kệ chỉ định!");
            }

            balance.LockStock(item.BinId, item.BaseQuantity);
            if (!balancesToUpdate.Contains(balance))
            {
                balancesToUpdate.Add(balance);
            }

            // Tìm line để lấy ReferenceDocumentLineId
            InventoryTicketLine? line = ticketLines.FirstOrDefault(l => l.Id == item.TicketLineId);
            (Guid? partnerId, string? partnerName, Guid? sourceDocId, string? sourceDocNumber) = await GetPartnerAndDocInfoAsync(ticket, line?.ReferenceDocumentLineId);

            reservations.Add(new InventoryReservation(
                GuidGenerator.Create(), ticket.Id, ticket.TicketNumber,
                ticket.WarehouseId, item.BinId, item.ProductId, item.ProductBatchId, item.BaseQuantity,
                partnerId, partnerName, sourceDocId, sourceDocNumber));
        }

        await _balanceRepo.UpdateManyAsync(balancesToUpdate);
        await _reservationRepo.InsertManyAsync(reservations);
    }

    public async Task UnlockStockAsync(InventoryTicket ticket)
    {
        List<InventoryReservation> activeReservations = await _reservationRepo.GetListAsync(x =>
            x.ReferenceDocumentId == ticket.Id && x.Status == ReservationStatus.Active);

        if (!activeReservations.Any())
        {
            return;
        }

        List<Guid> productIds = activeReservations.Select(x => x.ProductId).Distinct().ToList();
        List<Guid> batchIds = activeReservations.Select(x => x.ProductBatchId).Distinct().ToList();

        var query = await _balanceRepo.WithDetailsAsync(x => x.BinBalances);
        List<InventoryBalance> balances = query.Where(x =>
            x.WarehouseId == ticket.WarehouseId && productIds.Contains(x.ProductId) && batchIds.Contains(x.ProductBatchId)).ToList();

        List<InventoryBalance> balancesToUpdate = new();
        List<InventoryBalance> balancesToDelete = new();

        foreach (InventoryReservation res in activeReservations)
        {
            res.Cancel();
            InventoryBalance? balance = balances.FirstOrDefault(x => x.ProductId == res.ProductId && x.ProductBatchId == res.ProductBatchId);
            if (balance != null)
            {
                balance.UnlockStock(res.BinId, res.ReservedQuantity);
                
                if (balance.Quantity == 0 && balance.LockedQuantity == 0)
                {
                    if (!balancesToDelete.Contains(balance))
                    {
                        balancesToDelete.Add(balance);
                    }
                    balancesToUpdate.Remove(balance);
                }
                else
                {
                    if (!balancesToUpdate.Contains(balance) && !balancesToDelete.Contains(balance))
                    {
                        balancesToUpdate.Add(balance);
                    }
                }
            }
        }

        await _reservationRepo.UpdateManyAsync(activeReservations);
        if (balancesToUpdate.Any())
        {
            await _balanceRepo.UpdateManyAsync(balancesToUpdate);
        }
        if (balancesToDelete.Any())
        {
            await _balanceRepo.DeleteManyAsync(balancesToDelete);
        }
    }

    public async Task AdjustLockAsync(InventoryTicket ticket, Guid binId, Guid productId, Guid productBatchId, decimal baseQtyDiff)
    {
        if (baseQtyDiff == 0)
        {
            return;
        }

        var query = await _balanceRepo.WithDetailsAsync(x => x.BinBalances);
        InventoryBalance? balance = query.FirstOrDefault(x => 
            x.WarehouseId == ticket.WarehouseId && x.ProductId == productId && x.ProductBatchId == productBatchId);

        if (baseQtyDiff > 0) // Lock thêm
        {
            var binBalance = balance?.BinBalances.FirstOrDefault(x => x.BinId == binId);
            if (balance == null || binBalance == null || binBalance.AvailableQuantity < baseQtyDiff)
            {
                throw new BusinessException("SupplyCoreERP:OutOfStock", $"Không đủ tồn khả dụng");
            }

            balance.LockStock(binId, baseQtyDiff);
            await _balanceRepo.UpdateAsync(balance);

            // Ưu tiên tìm reservation đang có để tăng, nếu không có mới tạo mới
            InventoryReservation? existingRes = await _reservationRepo.FirstOrDefaultAsync(x =>
                x.ReferenceDocumentId == ticket.Id && x.Status == ReservationStatus.Active && x.BinId == binId && x.ProductBatchId == productBatchId);

            if (existingRes != null)
            {
                existingRes.IncreaseQuantity(baseQtyDiff);
                await _reservationRepo.UpdateAsync(existingRes);
            }
            else
            {
                // Tìm line tương ứng để lấy ReferenceDocumentLineId
                InventoryTicketLine? ticketLine = await _ticketLineRepo.FirstOrDefaultAsync(l => l.TicketId == ticket.Id && l.ProductId == productId);
                (Guid? partnerId, string? partnerName, Guid? sourceDocId, string? sourceDocNumber) = await GetPartnerAndDocInfoAsync(ticket, ticketLine?.ReferenceDocumentLineId);

                await _reservationRepo.InsertAsync(new InventoryReservation(
                    GuidGenerator.Create(), ticket.Id, ticket.TicketNumber, ticket.WarehouseId, binId, productId, productBatchId, baseQtyDiff,
                    partnerId, partnerName, sourceDocId, sourceDocNumber));
            }
        }
        else // Nhả bớt lock
        {
            decimal unlockAmount = Math.Abs(baseQtyDiff);
            if (balance != null)
            {
                balance.UnlockStock(binId, unlockAmount);
                
                if (balance.Quantity == 0 && balance.LockedQuantity == 0)
                {
                    await _balanceRepo.DeleteAsync(balance);
                }
                else
                {
                    await _balanceRepo.UpdateAsync(balance);
                }
            }

            List<InventoryReservation> activeReservations = await _reservationRepo.GetListAsync(x =>
                x.ReferenceDocumentId == ticket.Id && x.Status == ReservationStatus.Active && x.BinId == binId && x.ProductBatchId == productBatchId);

            foreach (InventoryReservation res in activeReservations)
            {
                if (unlockAmount <= 0)
                {
                    break;
                }

                decimal deduction = Math.Min(res.ReservedQuantity, unlockAmount);
                res.DecreaseQuantity(deduction);
                unlockAmount -= deduction;
            }
            await _reservationRepo.UpdateManyAsync(activeReservations);
        }
    }

    public async Task ExecuteStockMovementAsync(InventoryTicket ticket, IEnumerable<InventoryTicketDetail> details, InventoryTransactionType transType, bool isIssue)
    {
        List<InventoryBalance> balances = await GetBalancesWithDetailsAsync(details, ticket.WarehouseId);
        List<InventoryTransaction> transactions = new();

        List<InventoryReservation> activeReservations = isIssue
            ? await _reservationRepo.GetListAsync(x => x.ReferenceDocumentId == ticket.Id && x.Status == ReservationStatus.Active)
            : new List<InventoryReservation>();

        List<InventoryBalance> balancesToInsert = new();
        List<InventoryBalance> balancesToUpdate = new();
        List<InventoryBalance> balancesToDelete = new();

        // Load trước danh sách ticket lines
        var lineIds = details.Select(d => d.TicketLineId).Distinct().ToList();
        List<InventoryTicketLine> ticketLines = await _ticketLineRepo.GetListAsync(x => lineIds.Contains(x.Id));

        foreach (InventoryTicketDetail item in details)
        {
            var balance = balances.FirstOrDefault(x => x.ProductId == item.ProductId && x.ProductBatchId == item.ProductBatchId);

            if (isIssue)
            {
                if (balance == null)
                {
                    throw new BusinessException("SupplyCoreERP:OutOfStock", "Không có tồn kho khả dụng");
                }

                InventoryReservation? relatedRes = activeReservations.FirstOrDefault(x => x.BinId == item.BinId && x.ProductBatchId == item.ProductBatchId && x.Status == ReservationStatus.Active);
                if (relatedRes != null)
                {
                    relatedRes.Complete();
                    balance.UnlockStock(item.BinId, relatedRes.ReservedQuantity);
                }

                balance.RemoveStock(item.BinId, item.BaseQuantity);

                if (balance.Quantity == 0 && balance.LockedQuantity == 0)
                {
                    if (!balancesToDelete.Contains(balance))
                    {
                        balancesToDelete.Add(balance);
                    }
                    balancesToUpdate.Remove(balance);
                }
                else
                {
                    if (!balancesToUpdate.Contains(balance) && !balancesToDelete.Contains(balance))
                    {
                        balancesToUpdate.Add(balance);
                    }
                }
            }
            else // Nhập kho
            {
                if (balance == null)
                {
                    balance = new InventoryBalance(GuidGenerator.Create(), ticket.WarehouseId, item.ProductId, item.ProductBatchId);
                    balance.AddStock(item.BinId, item.BaseQuantity, GuidGenerator.Create());
                    
                    balances.Add(balance);
                    balancesToInsert.Add(balance);
                }
                else
                {
                    balance.AddStock(item.BinId, item.BaseQuantity, GuidGenerator.Create());
                    if (!balancesToUpdate.Contains(balance) && !balancesToInsert.Contains(balance))
                    {
                        balancesToUpdate.Add(balance);
                    }
                }
            }

            // Tìm line để lấy ReferenceDocumentLineId
            InventoryTicketLine? line = ticketLines.FirstOrDefault(l => l.Id == item.TicketLineId);
            (Guid? partnerId, string? partnerName, Guid? sourceDocId, string? sourceDocNumber) = await GetPartnerAndDocInfoAsync(ticket, line?.ReferenceDocumentLineId);

            transactions.Add(new InventoryTransaction(
                GuidGenerator.Create(), ticket.WarehouseId, item.BinId, item.ProductId, item.ProductBatchId,
                transType, item.BaseQuantity, balance.Quantity, ticket.Id, ticket.TicketNumber, ticket.Note,
                partnerId, partnerName, sourceDocId, sourceDocNumber));
        }

        if (activeReservations.Any())
        {
            await _reservationRepo.UpdateManyAsync(activeReservations);
        }

        if (balancesToInsert.Any())
        {
            await _balanceRepo.InsertManyAsync(balancesToInsert);
        }

        if (balancesToUpdate.Any())
        {
            await _balanceRepo.UpdateManyAsync(balancesToUpdate);
        }

        if (balancesToDelete.Any())
        {
            await _balanceRepo.DeleteManyAsync(balancesToDelete);
        }

        if (transactions.Any())
        {
            await _transactionRepo.InsertManyAsync(transactions);
        }
    }
}






