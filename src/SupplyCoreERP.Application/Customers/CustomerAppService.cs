using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Customers.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;


namespace SupplyCoreERP.Customers
{
	public class CustomerAppService : ApplicationService, ICustomerAppService
	{
		private readonly IRepository<Customer, Guid> _customerRepository;
		private readonly CustomerManager _customerManager;

		public CustomerAppService(
			IRepository<Customer, Guid> customerRepository,
			CustomerManager customerManager)
		{
			_customerRepository = customerRepository;
			_customerManager = customerManager;
		}

		public async Task<PagedResultDto<CustomerDto>> GetListAsync(GetCustomerListDto input)
		{
			var query = await _customerRepository.GetQueryableAsync();

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

			return new PagedResultDto<CustomerDto>(
				totalCount,
				ObjectMapper.Map<List<Customer>, List<CustomerDto>>(items)
			);
		}

		public async Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input)
		{
			var customer = await _customerManager.CreateAsync(
				input.Code, input.Name, input.PhoneNumber, input.Email,
				input.DateOfBirth, input.Gender, input.Type, input.TaxCode,
				input.Address, input.CountryId, input.CityId, input.AreaId,
				input.DebtLimit, input.PaymentTermDays
			);

			await _customerRepository.InsertAsync(customer);
			return ObjectMapper.Map<Customer, CustomerDto>(customer);
		}

		public async Task<CustomerDto> UpdateAsync(Guid id, CreateUpdateCustomerDto input)
		{
			var customer = await _customerRepository.GetAsync(id);

			await _customerManager.UpdateAsync(
				customer, input.Name, input.PhoneNumber, input.Email,
				input.DateOfBirth, input.Gender, input.Type, input.TaxCode,
				input.Address, input.CountryId, input.CityId, input.AreaId,
				input.DebtLimit, input.PaymentTermDays
			);

			await _customerRepository.UpdateAsync(customer);
			return ObjectMapper.Map<Customer, CustomerDto>(customer);
		}

		public async Task DeleteAsync(Guid id)
		{
			await _customerManager.DeleteAsync(id);
		}

		public async Task ToggleActiveAsync(Guid id)
		{
			var customer = await _customerRepository.GetAsync(id);
			customer.SetActive(!customer.IsActive);
			await _customerRepository.UpdateAsync(customer);
		}
	}
}
