using AutoMapper.Internal.Mappers;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Inventories.Tickets;
using SupplyCoreERP.Orders.PO;
using SupplyCoreERP.PurchaseOrders.Dtos;
using SupplyCoreERP.Suppliers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.PurchaseOrders
{
	public class PurchaseOrderAppService : ApplicationService, IPurchaseOrderAppService
	{
        // Dependencies
        private readonly IRepository<PurchaseOrder, Guid> _orderRepo;
		private readonly IRepository<InventoryTicket, Guid> _ticketRepo;   
		private readonly IRepository<Supplier, Guid> _supplierRepo;
		private readonly PurchaseOrderManager _orderManager;
   

		// DI
        public PurchaseOrderAppService(
		IRepository<PurchaseOrder, Guid> orderRepo,
		IRepository<InventoryTicket, Guid> ticketRepo,
		IRepository<Supplier, Guid> supplierRepo,
		PurchaseOrderManager orderManager)
		{
			_orderRepo = orderRepo;
			_ticketRepo = ticketRepo;
			_supplierRepo = supplierRepo;
			_orderManager = orderManager;
		}

        #region Purchase Order
        public async Task<PagedResultDto<PurchaseOrderDto>> GetListAsync(GetPurchaseOrderListDto input)
		{
			var query = await _orderRepo.GetQueryableAsync();

			query = query
				.Include(x => x.Supplier)
				.Include(x => x.Warehouse);

			query = query
				.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Code.Contains(input.Filter) || x.Supplier.Name.Contains(input.Filter))
				.WhereIf(input.SupplierId.HasValue, x => x.SupplierId == input.SupplierId)
				.WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId)
				.WhereIf(input.Status.HasValue, x => x.Status == input.Status);

			var totalCount = await AsyncExecuter.CountAsync(query);

			query = query
				.OrderBy(input.Sorting ?? nameof(PurchaseOrder.CreationTime) + " DESC")
				.PageBy(input);

			var items = await AsyncExecuter.ToListAsync(query);

			var dtos = ObjectMapper.Map<List<PurchaseOrder>, List<PurchaseOrderDto>>(items);
			return new PagedResultDto<PurchaseOrderDto>(totalCount, dtos);
		}

		public async Task<PurchaseOrderDto> GetAsync(Guid id)
		{
			var query = await _orderRepo.GetQueryableAsync();

			var entity = await query
				.Include(x => x.Supplier)
				.Include(x => x.Warehouse)
				.Include(x => x.Details).ThenInclude(d => d.Product)
				.Include(x => x.Details).ThenInclude(d => d.Unit)
				.FirstOrDefaultAsync(x => x.Id == id);

			if (entity == null) throw new EntityNotFoundException(typeof(PurchaseOrder), id);

			return ObjectMapper.Map<PurchaseOrder, PurchaseOrderDto>(entity);
		}

		public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto input)
		{
			var entity = await _orderManager.CreateOrderAsync(input.SupplierId, input.WarehouseId, input.OrderDate, input.ExpectedDeliveryDate, input.DueDate, input.Note);

			await _orderRepo.InsertAsync(entity);

			return ObjectMapper.Map<PurchaseOrder, PurchaseOrderDto>(entity);
		}

		public async Task<PurchaseOrderDto> UpdateAsync(Guid id, UpdatePurchaseOrderDto input)
		{
			var entity = await _orderRepo.GetAsync(id);

			await _orderManager.UpdateOrderAsync(entity, input.WarehouseId, input.ExpectedDeliveryDate, input.DueDate, input.Note);
			await _orderRepo.UpdateAsync(entity);

			return ObjectMapper.Map<PurchaseOrder, PurchaseOrderDto>(entity);
		}

		public async Task DeleteAsync(Guid id)
		{
			var query = await _orderRepo.GetQueryableAsync();
			var entity = await query.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id);

			if (entity != null)
			{
				await _orderManager.CheckBeforeDeleteAsync(entity);
				await _orderRepo.DeleteAsync(entity);
			}
		}
		#endregion

		#region Purchase Detail
		public async Task AddDetailAsync(Guid orderId, AddPurchaseOrderDetailDto input)
		{
			var query = await _orderRepo.GetQueryableAsync();
			var entity = await query.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == orderId);
			if (entity == null) throw new EntityNotFoundException(typeof(PurchaseOrder), orderId);

			await _orderManager.AddDetailAsync(entity, input.ProductId, input.UnitId, input.ConversionFactor, input.Quantity, input.UnitPrice, input.TaxRate);
			await _orderRepo.UpdateAsync(entity);
		}

		public async Task UpdateDetailAsync(Guid orderId, Guid detailId, UpdatePurchaseOrderDetailDto input)
		{
			var query = await _orderRepo.GetQueryableAsync();
			var entity = await query.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == orderId);
			if (entity == null) throw new EntityNotFoundException(typeof(PurchaseOrder), orderId);

			await _orderManager.UpdateDetailAsync(entity, detailId, input.Quantity, input.UnitPrice, input.TaxRate);
			await _orderRepo.UpdateAsync(entity);
		}

		public async Task RemoveDetailAsync(Guid orderId, Guid detailId)
		{
			var query = await _orderRepo.GetQueryableAsync();
			var entity = await query.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == orderId);
			if (entity == null) throw new EntityNotFoundException(typeof(PurchaseOrder), orderId);

			await _orderManager.RemoveDetailAsync(entity, detailId);
			await _orderRepo.UpdateAsync(entity);
		}
		#endregion

		#region Workflow
		public async Task SendToApproveAsync(Guid id)
		{
			var query = await _orderRepo.GetQueryableAsync();
			var entity = await query.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id)
				?? throw new EntityNotFoundException(typeof(PurchaseOrder), id);

			await _orderManager.SendToApproveAsync(entity);
			await _orderRepo.UpdateAsync(entity);
		}

		public async Task ApproveAsync(Guid id)
		{
			var query = await _orderRepo.GetQueryableAsync();
			var entity = await query.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id)
				?? throw new EntityNotFoundException(typeof(PurchaseOrder), id);

			var ticket = await _orderManager.ApproveAsync(entity);

			await _ticketRepo.InsertAsync(ticket);
			await _orderRepo.UpdateAsync(entity);
		}

		public async Task CompleteAsync(Guid id)
		{
			var entity = await _orderRepo.GetAsync(id);

			var supplier = await _orderManager.CompleteAsync(entity);

			await _supplierRepo.UpdateAsync(supplier);
			await _orderRepo.UpdateAsync(entity);
		}

		public async Task CancelAsync(Guid id, string reason)
		{
			var entity = await _orderRepo.GetAsync(id);
			await _orderManager.CancelAsync(entity, reason);
			await _orderRepo.UpdateAsync(entity);
		}
		#endregion
	}
}