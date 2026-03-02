using SupplyCoreERP.Balances;
using SupplyCoreERP.Balances.Dtos;
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

		public InventoryBalanceAppService(IRepository<InventoryBalance, Guid> balanceRepo)
		{
			_balanceRepo = balanceRepo;
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
	}
}