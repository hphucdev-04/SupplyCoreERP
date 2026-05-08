using System;
using System.Threading.Tasks;
using SupplyCoreERP.Medicines.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace SupplyCoreERP.Medicines;

public interface IMedicineAppService : IApplicationService
{
    //Medicines
    Task<PagedResultDto<MedicineDto>> GetListAsync(GetMedicineListDto input);
    Task<MedicineDetailDto> GetAsync(Guid id);
    Task<MedicineDetailDto> CreateAsync(CreateUpdateMedicineDto input);
    Task<MedicineDetailDto> UpdateAsync(Guid id, CreateUpdateMedicineDto input);
    Task DeleteAsync(Guid id);
    Task<MedicineSummaryDto> GetSummaryAsync();

    //Workflow
    Task ApproveAsync(Guid id);
    Task RejectAsync(Guid id);
    Task ToggleActiveAsync(Guid id);

    //Ingredients
    Task AddIngredientAsync(Guid id, CreateUpdateMedicineIngredientDto input);
    Task RemoveIngredientAsync(Guid id, Guid activeIngredientId);

    //Units
    Task AddUnitAsync(Guid id, CreateUpdateMedicineUnitDto input);
    Task UpdateUnitAsync(Guid id, Guid unitId, CreateUpdateMedicineUnitDto input);
    Task RemoveUnitAsync(Guid id, Guid unitId);

    //Export Excel
    Task<IRemoteStreamContent> GetListAsExcelFileAsync(GetMedicineListDto input);

    //Import Excel
    Task ImportExcelAsync(IRemoteStreamContent file);
    Task<IRemoteStreamContent> GetImportTemplateAsync();
}
