using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Balances;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Inventory.Transactions;
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

    // Constructor injection
    public InventoryBalanceManager(
        IRepository<InventoryBalance, Guid> balanceRepo,
        IRepository<InventoryReservation, Guid> reservationRepo,
        IRepository<InventoryTransaction, Guid> transactionRepo)
    {
        _balanceRepo = balanceRepo;
        _reservationRepo = reservationRepo;
        _transactionRepo = transactionRepo;
    }

    private async Task<Dictionary<(Guid BinId, Guid BatchId), InventoryBalance>> GetBalancesAsync(
        IEnumerable<InventoryTicketDetail> details, Guid warehouseId)
    {
        List<Guid> binIds = details.Select(d => d.BinId).Distinct().ToList();
        List<Guid> batchIds = details.Select(d => d.ProductBatchId).Distinct().ToList();

        List<InventoryBalance> balances = await _balanceRepo.GetListAsync(x =>
            x.WarehouseId == warehouseId && binIds.Contains(x.BinId) && batchIds.Contains(x.ProductBatchId));

        return balances.ToDictionary(b => (b.BinId, b.ProductBatchId));
    }

    public async Task LockStockAsync(InventoryTicket ticket, IEnumerable<InventoryTicketDetail> details)
    {
        Dictionary<(Guid BinId, Guid BatchId), InventoryBalance> balances = await GetBalancesAsync(details, ticket.WarehouseId);
        List<InventoryReservation> reservations = new();
        List<InventoryBalance> balancesToUpdate = new();

        foreach (InventoryTicketDetail item in details)
        {
            if (!balances.TryGetValue((item.BinId, item.ProductBatchId), out InventoryBalance? balance) || balance.AvailableQuantity < item.BaseQuantity)
            {
                throw new BusinessException("SupplyCoreERP:OutOfStock", $"Không đủ tồn kho dành cho sản phẩm ID {item.ProductId}!");
            }

            balance.LockStock(item.BaseQuantity);
            balancesToUpdate.Add(balance);

            reservations.Add(new InventoryReservation(
                GuidGenerator.Create(), ticket.Id, ticket.TicketNumber,
                ticket.WarehouseId, item.BinId, item.ProductId, item.ProductBatchId, item.BaseQuantity));
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

        List<Guid> binIds = activeReservations.Select(x => x.BinId).Distinct().ToList();
        List<Guid> batchIds = activeReservations.Select(x => x.ProductBatchId).Distinct().ToList();

        List<InventoryBalance> balances = await _balanceRepo.GetListAsync(x =>
            x.WarehouseId == ticket.WarehouseId && binIds.Contains(x.BinId) && batchIds.Contains(x.ProductBatchId));

        List<InventoryBalance> balancesToUpdate = new();

        foreach (InventoryReservation res in activeReservations)
        {
            res.Cancel();
            InventoryBalance? balance = balances.FirstOrDefault(x => x.BinId == res.BinId && x.ProductBatchId == res.ProductBatchId);
            if (balance != null)
            {
                balance.UnlockStock(res.ReservedQuantity);
                if (!balancesToUpdate.Contains(balance))
                {
                    balancesToUpdate.Add(balance);
                }
            }
        }

        await _reservationRepo.UpdateManyAsync(activeReservations);
        if (balancesToUpdate.Any())
        {
            await _balanceRepo.UpdateManyAsync(balancesToUpdate);
        }
    }

    public async Task AdjustLockAsync(InventoryTicket ticket, Guid binId, Guid productId, Guid productBatchId, decimal baseQtyDiff)
    {
        if (baseQtyDiff == 0)
        {
            return;
        }

        InventoryBalance? balance = await _balanceRepo.FirstOrDefaultAsync(x => x.BinId == binId && x.ProductBatchId == productBatchId);

        if (baseQtyDiff > 0) // Lock thêm
        {
            if (balance == null || balance.AvailableQuantity < baseQtyDiff)
            {
                throw new BusinessException("SupplyCoreERP:OutOfStock", $"Không đủ tồn khả dụng");
            }

            balance.LockStock(baseQtyDiff);
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
                await _reservationRepo.InsertAsync(new InventoryReservation(
                    GuidGenerator.Create(), ticket.Id, ticket.TicketNumber, ticket.WarehouseId, binId, productId, productBatchId, baseQtyDiff));
            }
        }
        else // Nhả bớt look
        {
            decimal unlockAmount = Math.Abs(baseQtyDiff);
            if (balance != null)
            {
                balance.UnlockStock(unlockAmount);
                await _balanceRepo.UpdateAsync(balance);
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
        Dictionary<(Guid BinId, Guid BatchId), InventoryBalance> balances = await GetBalancesAsync(details, ticket.WarehouseId);
        List<InventoryTransaction> transactions = new();

        List<InventoryReservation> activeReservations = isIssue
            ? await _reservationRepo.GetListAsync(x => x.ReferenceDocumentId == ticket.Id && x.Status == ReservationStatus.Active)
            : new List<InventoryReservation>();

        List<InventoryBalance> balancesToInsert = new();
        List<InventoryBalance> balancesToUpdate = new();

        foreach (InventoryTicketDetail item in details)
        {
            balances.TryGetValue((item.BinId, item.ProductBatchId), out InventoryBalance? balance);

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
                    balance.UnlockStock(relatedRes.ReservedQuantity);
                }

                balance.RemoveStock(item.BaseQuantity);
                if (!balancesToUpdate.Contains(balance))
                {
                    balancesToUpdate.Add(balance);
                }
            }
            else // Nhập kho
            {
                if (balance == null)
                {
                    balance = new InventoryBalance(GuidGenerator.Create(), ticket.WarehouseId, item.BinId, item.ProductId, item.ProductBatchId, item.BaseQuantity);
                    balances.Add((item.BinId, item.ProductBatchId), balance);
                    balancesToInsert.Add(balance);
                }
                else
                {
                    balance.AddStock(item.BaseQuantity);
                    if (!balancesToUpdate.Contains(balance) && !balancesToInsert.Contains(balance))
                    {
                        balancesToUpdate.Add(balance);
                    }
                }
            }

            transactions.Add(new InventoryTransaction(
                GuidGenerator.Create(), ticket.WarehouseId, item.BinId, item.ProductId, item.ProductBatchId,
                transType, item.BaseQuantity, balance.Quantity, ticket.Id, ticket.TicketNumber, ticket.Note));
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

        if (transactions.Any())
        {
            await _transactionRepo.InsertManyAsync(transactions);
        }
    }
}






