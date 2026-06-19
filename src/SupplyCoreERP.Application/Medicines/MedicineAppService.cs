using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Catalog.Medicines;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Medicines.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Medicines;

public class MedicineAppService : SupplyCore, IMedicineAppService
{
    private readonly IRepository<Medicine, Guid> _medicineRepo;
    private readonly IMedicineManager _medicineManager;
    private readonly ProductManager _productManager;

    private readonly MedicineExcelService _excelService;

    public MedicineAppService(
        IRepository<Medicine, Guid> medicineRepo,
        IMedicineManager medicineManager,
        ProductManager productManager,
        MedicineExcelService excelService
        )
    {
        _medicineRepo = medicineRepo;
        _medicineManager = medicineManager;
        _productManager = productManager;
        _excelService = excelService;
    }
    #region Medicine
    public async Task<PagedResultDto<MedicineDto>> GetListAsync(GetMedicineListDto input)
    {
        //Queryable
        IQueryable<Medicine> query = await _medicineRepo.GetQueryableAsync();

        //JOIN Báº¢NG 
        query = query
            .Include(x => x.Category)
            .Include(x => x.Manufacturer).ThenInclude(m => m.Country)
            .Include(x => x.BaseUnit)
            .Include(x => x.DosageForm)
            .Include(x => x.Registrations);

        //Filter Logic
        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Name.Contains(input.Filter) || x.Code.Contains(input.Filter))
            .WhereIf(input.CategoryId.HasValue, x => x.CategoryId == input.CategoryId)
            .WhereIf(input.ManufacturerId.HasValue, x => x.ManufacturerId == input.ManufacturerId);

        //Sort & Paging
        int totalCount = await AsyncExecuter.CountAsync(query);

        query = query
            .OrderBy(input.Sorting ?? nameof(Medicine.CreationTime) + " DESC")
            .PageBy(input);

        // Load vÃ o RAM
        List<Medicine> items = await AsyncExecuter.ToListAsync(query);

        //Map to DTO 
        List<MedicineDto> dtos = ObjectMapper.Map<List<Medicine>, List<MedicineDto>>(items);

        return new PagedResultDto<MedicineDto>(totalCount, dtos);
    }

    public async Task<MedicineDetailDto> GetAsync(Guid id)
    {
        IQueryable<Medicine> query = await _medicineRepo.GetQueryableAsync();

        Medicine? entity = await query
            .Include(x => x.Category)
            .Include(x => x.Manufacturer).ThenInclude(m => m.Country)
            .Include(x => x.BaseUnit)
            .Include(x => x.DosageForm)
            .Include(x => x.Registrations)
            .Include(x => x.Ingredients).ThenInclude(i => i.ActiveIngredient)
            .Include(x => x.Units).ThenInclude(u => u.Unit)

            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(Medicine), id);
        }

        MedicineDetailDto dto = ObjectMapper.Map<Medicine, MedicineDetailDto>(entity);
        dto.HasTransactions = await _productManager.HasTransactionsAsync(id);
        return dto;
    }


    public async Task<MedicineDetailDto> CreateAsync(CreateUpdateMedicineDto input)
    {
        Medicine entity = await _medicineManager.CreateAsync(
            input.Name,
            input.CategoryId,
            input.ManufacturerId,
            input.BaseUnitId,
            input.DosageFormId,
            input.RegistrationNumber,
            input.UsageRoute,
            input.StorageCondition,
            input.IsPrescriptionDrug,
            input.BaseUnitVolume,
            input.RegistrationValidFrom,
            input.RegistrationValidTo,
            input.RegistrationNote
        );

        await _medicineRepo.InsertAsync(entity);

        MedicineDetailDto dto = ObjectMapper.Map<Medicine, MedicineDetailDto>(entity);
        dto.HasTransactions = false;
        return dto;
    }

    public async Task<MedicineDetailDto> UpdateAsync(Guid id, CreateUpdateMedicineDto input)
    {
        IQueryable<Medicine> query = await _medicineRepo.GetQueryableAsync();
        Medicine? entity = await query.Include(x => x.Registrations).FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(Medicine), id);
        }

        await _medicineManager.UpdateAsync(
            entity,
            input.Name,
            input.CategoryId,
            input.ManufacturerId,
            input.BaseUnitId,
            input.DosageFormId,
            input.RegistrationNumber,
            input.UsageRoute,
            input.StorageCondition,
            input.IsPrescriptionDrug,
            input.BaseUnitVolume,
            input.RegistrationValidFrom,
            input.RegistrationValidTo,
            input.RegistrationNote
        );

        entity.SetActive(input.IsActive);

        await _medicineRepo.UpdateAsync(entity);
        MedicineDetailDto dto = ObjectMapper.Map<Medicine, MedicineDetailDto>(entity);
        dto.HasTransactions = await _productManager.HasTransactionsAsync(id);
        return dto;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _medicineRepo.DeleteAsync(id);
    }

    public async Task ApproveAsync(Guid id)
    {
        Medicine entity = await _medicineRepo.GetAsync(id);
        entity.Approve();
        await _medicineRepo.UpdateAsync(entity);
    }

    public async Task RejectAsync(Guid id)
    {
        Medicine entity = await _medicineRepo.GetAsync(id);
        entity.Reject();
        await _medicineRepo.UpdateAsync(entity);
    }

    public async Task ToggleActiveAsync(Guid id)
    {
        Medicine entity = await _medicineRepo.GetAsync(id);
        entity.SetActive(!entity.IsActive);
        await _medicineRepo.UpdateAsync(entity);
    }

    public async Task<MedicineSummaryDto> GetSummaryAsync()
    {
        IQueryable<Medicine> query = await _medicineRepo.GetQueryableAsync();

        MedicineSummaryDto? summary = await query
            .GroupBy(x => 1)
            .Select(g => new MedicineSummaryDto
            {
                TotalCount = g.Count(),
                TotalActive = g.Count(x => x.IsActive),
                TotalInactive = g.Count(x => !x.IsActive),
                TotalApproved = g.Count(x => x.Status == MedicineStatus.Approved),
                TotalPending = g.Count(x => x.Status == MedicineStatus.Pending),
                TotalRejected = g.Count(x => x.Status == MedicineStatus.Rejected)
            })
            .FirstOrDefaultAsync();

        return summary ?? new MedicineSummaryDto();
    }


    #endregion

    #region Ingredients
    public async Task AddIngredientAsync(Guid id, CreateUpdateMedicineIngredientDto input)
    {
        IQueryable<Medicine> query = await _medicineRepo.GetQueryableAsync();
        Medicine? medicine = await query
            .Include(x => x.Ingredients)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (medicine == null)
        {
            throw new EntityNotFoundException(typeof(Medicine), id);
        }

        await _medicineManager.AddIngredientAsync(medicine, input.ActiveIngredientId);

        await _medicineRepo.UpdateAsync(medicine);
    }

    public async Task RemoveIngredientAsync(Guid id, Guid activeIngredientId)
    {
        IQueryable<Medicine> query = await _medicineRepo.GetQueryableAsync();
        Medicine? medicine = await query
            .Include(x => x.Ingredients)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (medicine == null)
        {
            throw new EntityNotFoundException(typeof(Medicine), id);
        }

        await _medicineManager.RemoveIngredientAsync(medicine, activeIngredientId);
        await _medicineRepo.UpdateAsync(medicine);
    }
    #endregion

    #region Units
    public async Task AddUnitAsync(Guid id, CreateUpdateMedicineUnitDto input)
    {
        IQueryable<Medicine> query = await _medicineRepo.GetQueryableAsync();

        Medicine? medicine = await query
            .Include(x => x.Units)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (medicine == null)
        {
            throw new EntityNotFoundException(typeof(Medicine), id);
        }

        await _medicineManager.AddUnitAsync(medicine, input.UnitId, input.ConversionFactor, input.Level, input.Volume);

        await _medicineRepo.UpdateAsync(medicine);
    }

    public async Task UpdateUnitAsync(Guid id, Guid unitId, CreateUpdateMedicineUnitDto input)
    {
        IQueryable<Medicine> query = await _medicineRepo.GetQueryableAsync();

        Medicine? medicine = await query
            .Include(x => x.Units)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (medicine == null)
        {
            throw new EntityNotFoundException(typeof(Medicine), id);
        }

        await _medicineManager.UpdateUnitAsync(medicine, unitId, input.ConversionFactor, input.Level, input.Volume);

        await _medicineRepo.UpdateAsync(medicine);
    }

    public async Task RemoveUnitAsync(Guid id, Guid unitId)
    {
        IQueryable<Medicine> query = await _medicineRepo.GetQueryableAsync();

        Medicine? medicine = await query
            .Include(x => x.Units)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (medicine == null)
        {
            throw new EntityNotFoundException(typeof(Medicine), id);
        }

        await _medicineManager.RemoveUnitAsync(medicine, unitId);
        await _medicineRepo.UpdateAsync(medicine);
    }
    #endregion

    #region Registration
    public async Task<List<MedicineRegistrationDto>> GetRegistrationsAsync(Guid id)
    {
        IQueryable<Medicine> query = await _medicineRepo.GetQueryableAsync();
        Medicine? medicine = await query.Include(x => x.Registrations).FirstOrDefaultAsync(x => x.Id == id);

        if (medicine == null)
        {
            throw new EntityNotFoundException(typeof(Medicine), id);
        }

        return ObjectMapper.Map<List<MedicineRegistration>, List<MedicineRegistrationDto>>(medicine.Registrations.ToList());
    }

    public async Task AddRegistrationAsync(Guid id, AddMedicineRegistrationDto input)
    {
        IQueryable<Medicine> query = await _medicineRepo.GetQueryableAsync();
        Medicine? medicine = await query.Include(x => x.Registrations).FirstOrDefaultAsync(x => x.Id == id);

        if (medicine == null)
        {
            throw new EntityNotFoundException(typeof(Medicine), id);
        }

        await _medicineManager.AddRegistrationAsync(
            medicine,
            input.RegistrationNumber,
            input.ValidFrom,
            input.ValidTo,
            input.Note
        );

        await _medicineRepo.UpdateAsync(medicine);
    }
    #endregion

    #region Excel
    public Task ImportExcelAsync(IRemoteStreamContent file)
    {
        return _excelService.ImportExcelAsync(file);
    }

    public Task<IRemoteStreamContent> GetImportTemplateAsync()
    {
        return _excelService.GetImportTemplateAsync();
    }

    public Task<IRemoteStreamContent> GetListAsExcelFileAsync(GetMedicineListDto input)
    {
        return _excelService.GetListAsExcelFileAsync(input);
    }
    #endregion
}

