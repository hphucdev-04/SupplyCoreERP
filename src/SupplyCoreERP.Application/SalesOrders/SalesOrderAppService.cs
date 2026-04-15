using AutoMapper.Internal.Mappers;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Customers;
using SupplyCoreERP.Inventories.Tickets;
using SupplyCoreERP.Sales.Orders;
using SupplyCoreERP.SalesOrders.Dtos;
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

namespace SupplyCoreERP.SalesOrders
{
	public class SalesOrderAppService : ApplicationService, ISalesOrderAppService
	{
		// Dependencies
		private readonly IRepository<SalesOrder, Guid> _orderRepo;
		private readonly IRepository<InventoryTicket, Guid> _ticketRepo;         
		private readonly IRepository<InventoryTicketDetail, Guid> _ticketDetailRepo; 
		private readonly IRepository<Customer, Guid> _customerRepo;
		private readonly SalesOrderManager _orderManager;

		// DI
		public SalesOrderAppService(
			IRepository<SalesOrder, Guid> orderRepo,
			IRepository<InventoryTicket, Guid> ticketRepo,
			IRepository<InventoryTicketDetail, Guid> ticketDetailRepo,
			IRepository<Customer, Guid> customerRepo,
			SalesOrderManager orderManager)
		{
			_orderRepo = orderRepo;
			_ticketRepo = ticketRepo;
			_ticketDetailRepo = ticketDetailRepo;
			_customerRepo = customerRepo;
			_orderManager = orderManager;
		}

		#region SaleOrder
		public async Task<PagedResultDto<SalesOrderDto>> GetListAsync(GetSalesOrderListDto input)
		{
			var query = await _orderRepo.GetQueryableAsync();

			query = query
				.Include(x => x.Customer)
				.Include(x => x.Warehouse);

			query = query
				.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Code.Contains(input.Filter) || x.Customer.Name.Contains(input.Filter))
				.WhereIf(input.CustomerId.HasValue, x => x.CustomerId == input.CustomerId)
				.WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId)
				.WhereIf(input.Status.HasValue, x => x.Status == input.Status);

			var totalCount = await AsyncExecuter.CountAsync(query);

			query = query
				.OrderBy(input.Sorting ?? nameof(SalesOrder.CreationTime) + " DESC")
				.PageBy(input);

			var items = await AsyncExecuter.ToListAsync(query);

			var dtos = ObjectMapper.Map<List<SalesOrder>, List<SalesOrderDto>>(items);
			return new PagedResultDto<SalesOrderDto>(totalCount, dtos);
		}

		public async Task<SalesOrderDto> GetAsync(Guid id)
		{
			var query = await _orderRepo.GetQueryableAsync();

			var entity = await query
				.Include(x => x.Customer)
				.Include(x => x.Warehouse)
				.Include(x => x.Details).ThenInclude(d => d.Product)
				.Include(x => x.Details).ThenInclude(d => d.Unit)
				.FirstOrDefaultAsync(x => x.Id == id);

			if (entity == null) throw new EntityNotFoundException(typeof(SalesOrder), id);

			return ObjectMapper.Map<SalesOrder, SalesOrderDto>(entity);
		}

		public async Task<SalesOrderDto> CreateAsync(CreateSalesOrderDto input)
		{
			var entity = await _orderManager.CreateOrderAsync(input.CustomerId, input.WarehouseId, input.OrderDate, input.ExpectedDeliveryDate, input.DueDate, input.Note);

			await _orderRepo.InsertAsync(entity);

			return ObjectMapper.Map<SalesOrder, SalesOrderDto>(entity);
		}

		public async Task<SalesOrderDto> UpdateAsync(Guid id, UpdateSalesOrderDto input)
		{
			var entity = await _orderRepo.GetAsync(id);

			await _orderManager.UpdateOrderAsync(entity, input.WarehouseId, input.ExpectedDeliveryDate, input.DueDate, input.Note);
			await _orderRepo.UpdateAsync(entity);

			return ObjectMapper.Map<SalesOrder, SalesOrderDto>(entity);
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

		#region SaleOrder Details
		public async Task AddDetailAsync(Guid orderId, AddSalesOrderDetailDto input)
		{
			var query = await _orderRepo.GetQueryableAsync();
			var entity = await query.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == orderId);
			if (entity == null) throw new EntityNotFoundException(typeof(SalesOrder), orderId);

			await _orderManager.AddDetailAsync(entity, input.ProductId, input.UnitId, input.ConversionFactor, input.Quantity, input.DiscountRate, input.TaxRate);
			await _orderRepo.UpdateAsync(entity);
		}

		public async Task UpdateDetailAsync(Guid orderId, Guid detailId, UpdateSalesOrderDetailDto input)
		{
			var query = await _orderRepo.GetQueryableAsync();
			var entity = await query.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == orderId);
			if (entity == null) throw new EntityNotFoundException(typeof(SalesOrder), orderId);

			await _orderManager.UpdateDetailAsync(entity, detailId, input.Quantity, input.DiscountRate, input.TaxRate);
			await _orderRepo.UpdateAsync(entity);
		}

		public async Task RemoveDetailAsync(Guid orderId, Guid detailId)
		{
			var query = await _orderRepo.GetQueryableAsync();
			var entity = await query.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == orderId);
			if (entity == null) throw new EntityNotFoundException(typeof(SalesOrder), orderId);

			await _orderManager.RemoveDetailAsync(entity, detailId);
			await _orderRepo.UpdateAsync(entity);
		}
		#endregion

		#region Workflow
		public async Task SendToApproveAsync(Guid id)
		{
			var query = await _orderRepo.GetQueryableAsync();
			var entity = await query.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id)
				?? throw new EntityNotFoundException(typeof(SalesOrder), id);

			await _orderManager.SendToApproveAsync(entity);
			await _orderRepo.UpdateAsync(entity);
		}

		public async Task ApproveAsync(Guid id)
		{
			var query = await _orderRepo.GetQueryableAsync();
			var entity = await query.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id)
				?? throw new EntityNotFoundException(typeof(SalesOrder), id);

			// Manager validate tồn kho + tạo ticket + chạy FEFO 
			var (ticket, fefoDetails) = await _orderManager.ApproveAsync(entity);

			await _ticketRepo.InsertAsync(ticket);
			if (fefoDetails.Any())
				await _ticketDetailRepo.InsertManyAsync(fefoDetails);
			await _orderRepo.UpdateAsync(entity);
		}

		public async Task CompleteAsync(Guid id)
		{
			var entity = await _orderRepo.GetAsync(id);

			var customer = await _orderManager.CompleteAsync(entity);

			await _customerRepo.UpdateAsync(customer);
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