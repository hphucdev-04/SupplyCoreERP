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
using Volo.Abp.Domain.Entities;
using SupplyCoreERP.Enums.Partner;

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

		#region Customer
		public async Task<CustomerDetailDto> GetAsync(Guid id)
		{
			var query = await _customerRepository.GetQueryableAsync();
			var customer = await query
				.Include(x => x.Country)
				.Include(x => x.City)
				.Include(x => x.Area)
				.Include(x => x.PriceList) 
				.FirstOrDefaultAsync(x => x.Id == id);

			if (customer == null)
			{
				throw new EntityNotFoundException(typeof(Customer), id);
			}

			return ObjectMapper.Map<Customer, CustomerDetailDto>(customer);
		}

		public async Task<PagedResultDto<CustomerDto>> GetListAsync(GetCustomerListDto input)
		{
			var query = await _customerRepository.GetQueryableAsync();

			query = query
				.Include(x => x.City)
				.Include(x => x.PriceList) 
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

		public async Task<CustomerDetailDto> CreateAsync(CreateUpdateCustomerDto input)
		{
			var customer = await _customerManager.CreateAsync(
				input.Code, input.Name, input.PhoneNumber, input.Email,
				input.RepresentativeName, input.Gender, input.Type, input.TaxCode,
				input.Address, input.CountryId, input.CityId, input.AreaId,
				input.Note, input.DebtLimit, input.PaymentTermDays,
				input.PriceListId 
			);
			customer.SetActive(input.IsActive);

			await _customerRepository.InsertAsync(customer);
			return ObjectMapper.Map<Customer, CustomerDetailDto>(customer);
		}

		public async Task<CustomerDetailDto> UpdateAsync(Guid id, CreateUpdateCustomerDto input)
		{
			var customer = await _customerRepository.GetAsync(id);

			await _customerManager.UpdateAsync(
				customer, input.Code, input.Name, input.PhoneNumber, input.Email,
				input.RepresentativeName, input.Gender, input.Type, input.TaxCode,
				input.Address, input.CountryId, input.CityId, input.AreaId,
				input.Note, input.DebtLimit, input.PaymentTermDays,
				input.PriceListId 
			);

			customer.SetActive(input.IsActive);
			await _customerRepository.UpdateAsync(customer);
			return ObjectMapper.Map<Customer, CustomerDetailDto>(customer);
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

		public async Task<CustomerSummaryDto> GetSummaryAsync()
		{
			var query = await _customerRepository.GetQueryableAsync();

			var summary = query
				.GroupBy(x => 1)
				.Select(g => new CustomerSummaryDto
				{
					TotalCount = g.Count(),
					TotalActive = g.Count(x => x.IsActive),
					TotalInactive = g.Count(x => !x.IsActive),
					TotalOrganization = g.Count(x => x.Type == CustomerType.Organization),
					TotalIndividual = g.Count(x => x.Type == CustomerType.Individual)
				})
				.FirstOrDefault();

			return summary ?? new CustomerSummaryDto();
		}
		#endregion
	}
}