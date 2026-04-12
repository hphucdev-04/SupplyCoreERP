using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Balances.Dtos;
using SupplyCoreERP.Enums.Balances;
using SupplyCoreERP.Inventories.Balances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Balances
{
	public class InventoryBalanceAppService : ApplicationService, IInventoryBalanceAppService
	{
		private readonly IRepository<InventoryBalance, Guid> _balanceRepo;
		private readonly IRepository<InventoryReservation, Guid> _reservationRepo;

		public InventoryBalanceAppService(IRepository<InventoryBalance, Guid> balanceRepo, IRepository<InventoryReservation, Guid> reservationRepo)
		{
			_balanceRepo = balanceRepo;
			_reservationRepo = reservationRepo;
		}

		public async Task<PagedResultDto<InventoryBalanceDto>> GetListAsync(GetInventoryBalanceListDto input)
		{
			// Include đến 4 bảng để lấy đủ tên (Warehouse, Bin, Product, Batch)
			var query = await _balanceRepo.WithDetailsAsync(x => x.Warehouse, x => x.Bin, x => x.Product, x => x.ProductBatch);

			query = query
				.WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId)
				.WhereIf(input.BinId.HasValue, x => x.BinId == input.BinId)
				.WhereIf(input.ProductId.HasValue, x => x.ProductId == input.ProductId)
				.WhereIf(!string.IsNullOrWhiteSpace(input.BatchNumber), x => x.ProductBatch.BatchNumber.Contains(input.BatchNumber))
				.WhereIf(input.HideZeroQuantity == true, x => x.Quantity > 0);

			// Logic lọc Cận Date: Giả sử cận date là dưới 180 ngày (6 tháng)
			if (input.IsNearExpiry == true)
			{
				var nearExpiryDate = DateTime.Now.AddDays(180);
				query = query.Where(x => x.ProductBatch.ExpiryDate <= nearExpiryDate && x.ProductBatch.ExpiryDate > DateTime.Now);
			}

			var totalCount = await AsyncExecuter.CountAsync(query);
			var items = await AsyncExecuter.ToListAsync(
				query.OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? "Warehouse.Name, Bin.Code" : input.Sorting)
					 .Skip(input.SkipCount)
					 .Take(input.MaxResultCount)
			);

			return new PagedResultDto<InventoryBalanceDto>(totalCount, ObjectMapper.Map<List<InventoryBalance>, List<InventoryBalanceDto>>(items));
		}

		public async Task<InventoryBalanceDetailDto> GetAsync(Guid id)
		{
			var query = await _balanceRepo.GetQueryableAsync();

			// Nối sâu vào City, Area, và Supplier
			query = query
				.Include(x => x.Warehouse).ThenInclude(w => w.City)
				.Include(x => x.Warehouse).ThenInclude(w => w.Area)
				.Include(x => x.Bin)
				.Include(x => x.Product)
				.Include(x => x.ProductBatch).ThenInclude(b => b.Supplier);

			var entity = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));
			if (entity == null) throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(InventoryBalance), id);

			return ObjectMapper.Map<InventoryBalance, InventoryBalanceDetailDto>(entity);
		}

		public async Task<PagedResultDto<InventoryReservationDto>> GetReservationListAsync(GetInventoryReservationListDto input)
		{
			var query = await _reservationRepo.GetQueryableAsync();

			// Bộ lọc đa năng (Multi-dimensional Filter)
			query = query
				.WhereIf(input.ReferenceDocumentId.HasValue, x => x.ReferenceDocumentId == input.ReferenceDocumentId)
				.WhereIf(!string.IsNullOrWhiteSpace(input.ReferenceDocumentNumber), x => x.ReferenceDocumentNumber.Contains(input.ReferenceDocumentNumber))
				.WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId)
				.WhereIf(input.BinId.HasValue, x => x.BinId == input.BinId)
				.WhereIf(input.ProductId.HasValue, x => x.ProductId == input.ProductId)
				.WhereIf(input.ProductBatchId.HasValue, x => x.ProductBatchId == input.ProductBatchId)
				.WhereIf(input.Status.HasValue, x => x.Status == input.Status);

			var totalCount = await AsyncExecuter.CountAsync(query);

			var items = await AsyncExecuter.ToListAsync(
				query.OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? "CreationTime DESC" : input.Sorting)
					 .Skip(input.SkipCount)
					 .Take(input.MaxResultCount)
			);

			return new PagedResultDto<InventoryReservationDto>(
				totalCount,
				ObjectMapper.Map<List<InventoryReservation>, List<InventoryReservationDto>>(items)
			);

		}
	}
}