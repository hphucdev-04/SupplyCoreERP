using System;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.BaseUnits.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.BaseUnits;

public class BaseUnitAppService : CrudAppService<
    BaseUnit,
    BaseUnitDto,
    Guid,
    GetBaseUnitListDto,
    CreateUpdateBaseUnitDto>,
    IBaseUnitAppService
{
    private readonly BaseUnitManager _baseUnitManager;

    public BaseUnitAppService(
        IRepository<BaseUnit, Guid> repository,
        BaseUnitManager baseUnitManager)
        : base(repository)
    {
        _baseUnitManager = baseUnitManager;
    }

    public override async Task<BaseUnitDto> CreateAsync(CreateUpdateBaseUnitDto input)
    {
        // Manager check và tạo entity 
        BaseUnit unit = await _baseUnitManager.CreateAsync(input.Name);
        //Repository save vào DB
        await Repository.InsertAsync(unit);

        return ObjectMapper.Map<BaseUnit, BaseUnitDto>(unit);
    }

    public override async Task<BaseUnitDto> UpdateAsync(Guid id, CreateUpdateBaseUnitDto input)
    {
        BaseUnit unit = await Repository.GetAsync(id);
        // Manager để đảm bảo tính hợp lệ của entity
        await _baseUnitManager.UpdateAsync(unit, input.Name);
        //Repository update vào DB
        await Repository.UpdateAsync(unit);

        return ObjectMapper.Map<BaseUnit, BaseUnitDto>(unit);
    }

    public override async Task DeleteAsync(Guid id)
    {
        BaseUnit unit = await Repository.GetAsync(id);
        await _baseUnitManager.DeleteAsync(unit);
    }


    protected override async Task<IQueryable<BaseUnit>> CreateFilteredQueryAsync(GetBaseUnitListDto input)
    {
        IQueryable<BaseUnit> query = await base.CreateFilteredQueryAsync(input);

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            query = query.Where(x => x.Name.ToLower().Contains(input.Filter.ToLower()));
        }

        return query;
    }
}
