using SupplyCoreERP.Batches.Dtos;
using SupplyCoreERP.Inventories.Batches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Batches
{
	public class ProductBatchAppService : ApplicationService, IProductBatchAppService
	{
		private readonly IRepository<ProductBatch, Guid> _batchRepo;
		private readonly BatchManager _batchManager;

		public ProductBatchAppService(IRepository<ProductBatch, Guid> batchRepo, BatchManager batchManager)
		{
			_batchRepo = batchRepo;
			_batchManager = batchManager;
		}

		public async Task<PagedResultDto<ProductBatchDto>> GetListAsync(GetProductBatchListDto input)
		{
			var query = await _batchRepo.WithDetailsAsync(x => x.Product, x => x.Supplier);

			query = query
				.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.BatchNumber.Contains(input.Filter))
				.WhereIf(input.ProductId.HasValue, x => x.ProductId == input.ProductId)
				.WhereIf(input.SupplierId.HasValue, x => x.SupplierId == input.SupplierId)
				.WhereIf(input.Status.HasValue, x => x.Status == input.Status);

			var totalCount = await AsyncExecuter.CountAsync(query);
			var items = await AsyncExecuter.ToListAsync(
				query.OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? "CreationTime DESC" : input.Sorting)
					 .Skip(input.SkipCount)
					 .Take(input.MaxResultCount)
			);

			return new PagedResultDto<ProductBatchDto>(totalCount, ObjectMapper.Map<List<ProductBatch>, List<ProductBatchDto>>(items));
		}

		public async Task<ProductBatchDto> GetAsync(Guid id)
		{
			// Lấy kèm bảng liên kết để AutoMapper không bị null tên Product/Supplier
			var query = await _batchRepo.WithDetailsAsync(x => x.Product, x => x.Supplier);
			var batch = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));
			return ObjectMapper.Map<ProductBatch, ProductBatchDto>(batch);
		}

		public async Task<ProductBatchDto> CreateAsync(CreateUpdateProductBatchDto input)
		{
			var batch = await _batchManager.CreateAsync(input.ProductId, input.BatchNumber, input.ManufacturingDate, input.ExpiryDate, input.SupplierId);
			await _batchRepo.InsertAsync(batch);
			return ObjectMapper.Map<ProductBatch, ProductBatchDto>(batch);
		}

		public async Task<ProductBatchDto> UpdateAsync(Guid id, CreateUpdateProductBatchDto input)
		{
			await _batchManager.UpdateAsync(id, input.ManufacturingDate, input.ExpiryDate, input.SupplierId);
			var batch = await _batchRepo.GetAsync(id);
			return ObjectMapper.Map<ProductBatch, ProductBatchDto>(batch);
		}

		public async Task DeleteAsync(Guid id) => await _batchManager.DeleteAsync(id);

		// CÁC HÀM DUYỆT QA (Thay đổi Status)
		public async Task ApproveQAAsync(Guid id)
		{
			var batch = await _batchRepo.GetAsync(id);
			batch.ApproveQA();
			await _batchRepo.UpdateAsync(batch);
		}

		public async Task RejectQAAsync(Guid id)
		{
			var batch = await _batchRepo.GetAsync(id);
			batch.RejectQA();
			await _batchRepo.UpdateAsync(batch);
		}

		public async Task RecallAsync(Guid id)
		{
			var batch = await _batchRepo.GetAsync(id);
			batch.Recall();
			await _batchRepo.UpdateAsync(batch);
		}
	}
}