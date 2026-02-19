using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Suppliers.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Suppliers
{
	public class SupplierAppService : ApplicationService, ISupplierAppService
	{
		private readonly IRepository<Supplier, Guid> _supplierRepository;
		private readonly SupplierManager _supplierManager;

		public SupplierAppService(IRepository<Supplier, Guid> supplierRepository, SupplierManager supplierManager)
		{
			_supplierRepository = supplierRepository;
			_supplierManager = supplierManager;
		}

		public async Task<PagedResultDto<SupplierDto>> GetListAsync(GetSupplierListDto input)
		{
			var query = await _supplierRepository.GetQueryableAsync();

			query = query
				.Include(x => x.Country)
				.Include(x => x.City)
				.Include(x => x.Area)
				.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x =>
					x.Name.Contains(input.Filter) ||
					x.Code.Contains(input.Filter) ||
					x.PhoneNumber.Contains(input.Filter))
				.WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);

			var totalCount = await query.CountAsync();
			var items = await query
				.OrderBy(input.Sorting ?? "CreationTime DESC")
				.Skip(input.SkipCount)
				.Take(input.MaxResultCount)
				.ToListAsync();

			return new PagedResultDto<SupplierDto>(
				totalCount,
				ObjectMapper.Map<List<Supplier>, List<SupplierDto>>(items)
			);
		}

		public async Task<SupplierDto> CreateAsync(CreateUpdateSupplierDto input)
		{
			var supplier = await _supplierManager.CreateAsync(
				input.Code, input.Name, input.TaxCode, input.PhoneNumber, input.Email,
				input.RepresentativeName, input.Note,
				input.Address, input.CountryId, input.CityId, input.AreaId,
				input.DebtLimit, input.PaymentTermDays
			);

			await _supplierRepository.InsertAsync(supplier);
			return ObjectMapper.Map<Supplier, SupplierDto>(supplier);
		}

		public async Task<SupplierDto> UpdateAsync(Guid id, CreateUpdateSupplierDto input)
		{
			var supplier = await _supplierRepository.GetAsync(id);

			await _supplierManager.UpdateAsync(
				supplier, input.Name, input.TaxCode, input.PhoneNumber, input.Email,
				input.RepresentativeName, input.Note,
				input.Address, input.CountryId, input.CityId, input.AreaId,
				input.DebtLimit, input.PaymentTermDays
			);

			await _supplierRepository.UpdateAsync(supplier);
			return ObjectMapper.Map<Supplier, SupplierDto>(supplier);
		}

		public async Task DeleteAsync(Guid id)
		{
			await _supplierManager.DeleteAsync(id);
		}

		public async Task ToggleActiveAsync(Guid id)
		{
			var supplier = await _supplierRepository.GetAsync(id);
			supplier.SetActive(!supplier.IsActive);
			await _supplierRepository.UpdateAsync(supplier);
		}
	}
}