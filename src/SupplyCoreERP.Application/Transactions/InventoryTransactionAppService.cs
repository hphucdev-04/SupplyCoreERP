using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Inventories.Transactions;
using SupplyCoreERP.Transactions.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Transactions
{
	public class InventoryTransactionAppService : ApplicationService, IInventoryTransactionAppService
	{
		private readonly IRepository<InventoryTransaction, Guid> _transactionRepo;

		public InventoryTransactionAppService(IRepository<InventoryTransaction, Guid> transactionRepo)
		{
			_transactionRepo = transactionRepo;
		}

		public async Task<PagedResultDto<InventoryTransactionDto>> GetListAsync(GetInventoryTransactionListDto input)
		{
			var query = await _transactionRepo.GetQueryableAsync();

			// 1. Chỉ Include Master Data
			query = query
				.Include(x => x.Warehouse)
				.Include(x => x.Product)
				.Include(x => x.ProductBatch)
				.Include(x => x.Bin);

			// 2. Lọc dữ liệu
			query = query
				.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x =>
					x.Product.Name.Contains(input.Filter) ||
					x.Product.Code.Contains(input.Filter) ||
					x.ProductBatch.BatchNumber.Contains(input.Filter) ||
					// Search thẳng vào cột String (rất nhanh)
					(x.ReferenceDocumentNumber != null && x.ReferenceDocumentNumber.Contains(input.Filter)))
				.WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId)
				.WhereIf(input.ProductId.HasValue, x => x.ProductId == input.ProductId)
				.WhereIf(input.ProductBatchId.HasValue, x => x.ProductBatchId == input.ProductBatchId)
				.WhereIf(input.BinId.HasValue, x => x.BinId == input.BinId)
				.WhereIf(input.ReferenceDocumentId.HasValue, x => x.ReferenceDocumentId == input.ReferenceDocumentId)
				.WhereIf(input.TransactionType.HasValue, x => x.TransactionType == input.TransactionType)
				.WhereIf(input.FromDate.HasValue, x => x.CreationTime >= input.FromDate.Value)
				.WhereIf(input.ToDate.HasValue, x => x.CreationTime <= input.ToDate.Value);

			var totalCount = await AsyncExecuter.CountAsync(query);

			var items = await AsyncExecuter.ToListAsync(
				query.OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? "CreationTime DESC" : input.Sorting)
					 .Skip(input.SkipCount)
					 .Take(input.MaxResultCount)
			);

			return new PagedResultDto<InventoryTransactionDto>(
				totalCount,
				ObjectMapper.Map<List<InventoryTransaction>, List<InventoryTransactionDto>>(items)
			);
		}
	}
}