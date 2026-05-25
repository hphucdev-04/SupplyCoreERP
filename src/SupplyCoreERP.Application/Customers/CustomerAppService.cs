using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Customers.Dtos;
using SupplyCoreERP.Enums.Partner;
using SupplyCoreERP.Partner.Customers;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Customers;

public class CustomerAppService : SupplyCore, ICustomerAppService
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
        IQueryable<Customer> query = await _customerRepository.GetQueryableAsync();
        Customer? customer = await query
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
        IQueryable<Customer> query = await _customerRepository.GetQueryableAsync();

        query = query
            .Include(x => x.City)
            .Include(x => x.PriceList)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x =>
                x.Name.Contains(input.Filter) ||
                x.Code.Contains(input.Filter) ||
                x.PhoneNumber.Contains(input.Filter))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);

        int totalCount = await query.CountAsync();
        List<Customer> items = await query
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
        Customer customer = await _customerManager.CreateAsync(
            input.Name, input.PhoneNumber, input.Email,
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
        Customer customer = await _customerRepository.GetAsync(id);

        await _customerManager.UpdateAsync(
            customer, input.Name, input.PhoneNumber, input.Email,
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
        Customer customer = await _customerRepository.GetAsync(id);
        customer.SetActive(!customer.IsActive);
        await _customerRepository.UpdateAsync(customer);
    }

    public async Task<CustomerSummaryDto> GetSummaryAsync()
    {
        IQueryable<Customer> query = await _customerRepository.GetQueryableAsync();

        CustomerSummaryDto? summary = query
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

