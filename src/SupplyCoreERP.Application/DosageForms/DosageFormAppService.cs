using System;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.DosageForms;
using SupplyCoreERP.DosageForms.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.DosageForms;

public class DosageFormAppService : CrudAppService<
    DosageForm,
    DosageFormDto,
    Guid,
    GetDosageFormListDto,
    CreateUpdateDosageFormDto>,
    IDosageFormAppService
{
    private readonly DosageFormManager _dosageFormManager;
    public DosageFormAppService(
        IRepository<DosageForm, Guid> repository,
        DosageFormManager dosageFormManager)
        : base(repository)
    {
        _dosageFormManager = dosageFormManager;
    }

    public override async Task<DosageFormDto> CreateAsync(CreateUpdateDosageFormDto input)
    {
        DosageForm entity = await _dosageFormManager.CreateAsync(input.Name);
        await Repository.InsertAsync(entity);

        return ObjectMapper.Map<DosageForm, DosageFormDto>(entity);
    }

    public override async Task<DosageFormDto> UpdateAsync(Guid id, CreateUpdateDosageFormDto input)
    {
        DosageForm entity = await Repository.GetAsync(id);

        await _dosageFormManager.UpdateAsync(entity, input.Name);
        await Repository.UpdateAsync(entity);

        return ObjectMapper.Map<DosageForm, DosageFormDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        DosageForm entity = await Repository.GetAsync(id);
        await _dosageFormManager.DeleteAsync(entity);
    }

    protected override async Task<IQueryable<DosageForm>> CreateFilteredQueryAsync(GetDosageFormListDto input)
    {
        IQueryable<DosageForm> query = await base.CreateFilteredQueryAsync(input);

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            query = query.Where(x => x.Name.ToLower().Contains(input.Filter.ToLower()));
        }

        return query;
    }
}

