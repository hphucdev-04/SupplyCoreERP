using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyCoreERP.Suppliers.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Suppliers;

public interface ISupplierAppService : IApplicationService
{
    #region Supplier 
    Task<SupplierDetailDto> GetAsync(Guid id);
    Task<PagedResultDto<SupplierDto>> GetListAsync(GetSupplierListDto input);
    Task<SupplierDetailDto> CreateAsync(CreateUpdateSupplierDto input);
    Task<SupplierDetailDto> UpdateAsync(Guid id, CreateUpdateSupplierDto input);
    Task DeleteAsync(Guid id);
    Task ToggleActiveAsync(Guid id);
    #endregion

    #region Supplier Product
    Task<PagedResultDto<SupplierProductDto>> GetProductListAsync(Guid supplierId, GetSupplierProductListDto input);
    Task<PagedResultDto<SupplierMedicineDto>> GetSupplierListAsync(Guid productId, GetSupplierMedicineListDto input);
    Task<SupplierProductDto> AddProductAsync(Guid supplierId, CreateUpdateSupplierProductDto input);
    Task<SupplierProductDto> UpdateProductAsync(Guid supplierId, Guid productId, CreateUpdateSupplierProductDto input);
    Task RemoveProductAsync(Guid supplierId, Guid productId);
    Task ToggleProductActiveAsync(Guid supplierId, Guid productId);
    Task<List<SourcingSuggestionDto>> GetSourcingSuggestionsAsync(List<Guid> productIds);
    #endregion
}

