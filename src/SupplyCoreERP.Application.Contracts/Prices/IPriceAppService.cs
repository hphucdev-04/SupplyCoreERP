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
    Task<ProductCostReferenceDto> GetCostReferenceAsync(Guid productId, Guid unitId);
    Task<ProductPriceDto> CreateAsync(CreateUpdateProductPriceDto input);
    Task<ProductPriceDto> UpdateAsync(Guid id, CreateUpdateProductPriceDto input);
    Task DeleteAsync(Guid id);
}

