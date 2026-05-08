using System;
using System.Threading.Tasks;
using SupplyCoreERP.Batches.Dtos;
using SupplyCoreERP.Warehouses.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Batches;

public interface IProductBatchAppService : IApplicationService
{
    Task<PagedResultDto<ProductBatchDto>> GetListAsync(GetProductBatchListDto input);
    Task<ProductBatchDto> GetAsync(Guid id);
    Task<ProductBatchDto> CreateAsync(CreateUpdateProductBatchDto input);
    Task<ProductBatchDto> UpdateAsync(Guid id, CreateUpdateProductBatchDto input);
    Task DeleteAsync(Guid id);

    Task ApproveQAAsync(Guid id);
    Task RejectQAAsync(Guid id);
    Task RecallAsync(Guid id);
}
