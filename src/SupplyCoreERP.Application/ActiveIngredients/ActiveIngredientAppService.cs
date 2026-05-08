using System;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.ActiveIngredients.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.ActiveIngredients;

public class ActiveIngredientAppService : CrudAppService<
    ActiveIngredient,
    ActiveIngredientDto,
    Guid,
    GetActiveIngredientListDto,
    CreateUpdateActiveIngredientDto>,
    IActiveIngredientAppService
{
    private readonly ActiveIngredientManager _activeIngredientManager;
    public ActiveIngredientAppService(
        IRepository<ActiveIngredient, Guid> repository,
        ActiveIngredientManager activeIngredientManager)
        : base(repository)
    {
        _activeIngredientManager = activeIngredientManager;
    }

    public override async Task<ActiveIngredientDto> CreateAsync(CreateUpdateActiveIngredientDto input)
    {
        //Manager check và tạo entity
        ActiveIngredient ingredient = await _activeIngredientManager.CreateAsync(input.Name);
        //Repository save vào DB
        await Repository.InsertAsync(ingredient);
        return ObjectMapper.Map<ActiveIngredient, ActiveIngredientDto>(ingredient);
    }

    public override async Task<ActiveIngredientDto> UpdateAsync(Guid id, CreateUpdateActiveIngredientDto input)
    {
        // Manager để đảm bảo tính hợp lệ của entity
        ActiveIngredient ingredient = await Repository.GetAsync(id);
        //Repository update vào DB
        await _activeIngredientManager.UpdateAsync(ingredient, input.Name);
        return ObjectMapper.Map<ActiveIngredient, ActiveIngredientDto>(ingredient);
    }

    public override async Task DeleteAsync(Guid id)
    {
        ActiveIngredient ingredient = await Repository.GetAsync(id);
        await _activeIngredientManager.DeleteAsync(ingredient);
    }

    protected override async Task<IQueryable<ActiveIngredient>> CreateFilteredQueryAsync(GetActiveIngredientListDto input)
    {
        IQueryable<ActiveIngredient> query = await base.CreateFilteredQueryAsync(input);

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            query = query.Where(x => x.Name.ToLower().Contains(input.Filter.ToLower()));
        }

        return query;
    }
}
