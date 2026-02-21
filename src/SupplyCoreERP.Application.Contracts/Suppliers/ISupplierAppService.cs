using SupplyCoreERP.Suppliers.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Suppliers
{
	public interface ISupplierAppService : IApplicationService
	{
		Task<SupplierDetailDto> GetAsync(Guid id);
		Task<PagedResultDto<SupplierDto>> GetListAsync(GetSupplierListDto input);
		Task<SupplierDetailDto> CreateAsync(CreateUpdateSupplierDto input);
		Task<SupplierDetailDto> UpdateAsync(Guid id, CreateUpdateSupplierDto input);
		Task DeleteAsync(Guid id);
		Task ToggleActiveAsync(Guid id);
	}
}
