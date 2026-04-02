using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Orders;
using SupplyCoreERP.PO;
using SupplyCoreERP.PO.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Purchasing.Orders
{
	public class POAppService : ApplicationService, IPOAppService
	{
		private readonly IRepository<PurchaseOrder, Guid> _poRepository;
		private readonly PurchaseOrderManager _poManager;

		public POAppService(
			IRepository<PurchaseOrder, Guid> poRepo,
			PurchaseOrderManager poManager)
		{
			_poRepository = poRepo;
			_poManager = poManager;
		}

		private async Task<PurchaseOrder> GetOrderWithDetailsAsync(Guid orderId)
		{
			var query = await _poRepository.WithDetailsAsync(x => x.Details);
			var order = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == orderId));

			if (order == null) throw new UserFriendlyException("Không tìm thấy Đơn mua hàng!");

			return order;
		}

		public async Task<PagedResultDto<PurchaseOrderDto>> GetListAsync(GetPurchaseOrderListDto input)
		{
			var query = await _poRepository.WithDetailsAsync(x => x.Supplier);

			query = query
				.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Code.Contains(input.Filter))
				.WhereIf(input.SupplierId.HasValue, x => x.SupplierId == input.SupplierId)
				.WhereIf(input.Status.HasValue, x => x.Status == input.Status);

			var totalCount = await AsyncExecuter.CountAsync(query);
			var items = await AsyncExecuter.ToListAsync(
				query.OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? "CreationTime DESC" : input.Sorting)
					 .Skip(input.SkipCount)
					 .Take(input.MaxResultCount)
			);

			return new PagedResultDto<PurchaseOrderDto>(
				totalCount,
				ObjectMapper.Map<List<PurchaseOrder>, List<PurchaseOrderDto>>(items));
		}

		public async Task<PurchaseOrderDto> GetAsync(Guid id)
		{
			var query = await _poRepository.GetQueryableAsync();

			query = query
				.Include(x => x.Supplier)
				.Include(x => x.Details).ThenInclude(d => d.Product)
				.Include(x => x.Details).ThenInclude(d => d.Unit);

			var order = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));
			if (order == null) throw new UserFriendlyException("Không tìm thấy Đơn mua hàng!");

			return ObjectMapper.Map<PurchaseOrder, PurchaseOrderDto>(order);
		}

		public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto input)
		{
			var order = await _poManager.CreateOrderAsync(
				input.SupplierId, input.OrderDate, input.ExpectedDeliveryDate, input.Note);

			await _poRepository.InsertAsync(order);
			return ObjectMapper.Map<PurchaseOrder, PurchaseOrderDto>(order);
		}

		public async Task<PurchaseOrderDto> UpdateAsync(Guid id, UpdatePurchaseOrderDto input)
		{
			var order = await _poRepository.GetAsync(id);

			await _poManager.UpdateOrderAsync(order, input.ExpectedDeliveryDate, input.Note);

			await _poRepository.UpdateAsync(order);
			return await GetAsync(id);
		}

		public async Task DeleteAsync(Guid id)
		{
			var order = await _poRepository.GetAsync(id);

			await _poManager.CheckBeforeDeleteAsync(order);

			await _poRepository.DeleteAsync(order);
		}

		public async Task<PurchaseOrderDto> AddDetailAsync(Guid orderId, AddPurchaseOrderDetailDto input)
		{
			var order = await GetOrderWithDetailsAsync(orderId);

			await _poManager.AddDetailAsync(
				order, input.ProductId, input.UnitId, input.ConversionFactor,
				input.Quantity, input.UnitPrice, input.TaxRate);

			await _poRepository.UpdateAsync(order);
			return await GetAsync(orderId);
		}

		public async Task<PurchaseOrderDto> UpdateDetailAsync(Guid orderId, Guid detailId, UpdatePurchaseOrderDetailDto input)
		{
			var order = await GetOrderWithDetailsAsync(orderId);

			await _poManager.UpdateDetailAsync(order, detailId, input.Quantity, input.UnitPrice, input.TaxRate);

			await _poRepository.UpdateAsync(order);
			return await GetAsync(orderId);
		}

		public async Task<PurchaseOrderDto> RemoveDetailAsync(Guid orderId, Guid detailId)
		{
			var order = await GetOrderWithDetailsAsync(orderId);

			await _poManager.RemoveDetailAsync(order, detailId);

			await _poRepository.UpdateAsync(order);
			return await GetAsync(orderId);
		}

		public async Task SendToApproveAsync(Guid id)
		{
			var order = await GetOrderWithDetailsAsync(id);
			await _poManager.SendToApproveAsync(order);
			await _poRepository.UpdateAsync(order);
		}

		public async Task ApproveAsync(Guid id)
		{
			var order = await GetOrderWithDetailsAsync(id);
			await _poManager.ApproveAsync(order);
			await _poRepository.UpdateAsync(order);
		}

		public async Task CancelAsync(Guid id, string reason)
		{
			var order = await GetOrderWithDetailsAsync(id);
			await _poManager.CancelAsync(order, reason);
			await _poRepository.UpdateAsync(order);
		}
	}
}