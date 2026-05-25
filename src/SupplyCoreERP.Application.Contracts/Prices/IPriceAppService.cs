using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyCoreERP.Prices.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Prices;

public interface IPriceAppService : IApplicationService
{
    Task<List<PriceListDto>> GetPriceListsAsync();
    Task<List<ProductPriceDto>> GetByProductAsync(Guid productId);
    Task CreateAsync(CreateUpdateProductPriceDto input);
    Task UpdateAsync(Guid id, CreateUpdateProductPriceDto input);
    Task DeleteAsync(Guid id);
}

