using System;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.Medicines;
using SupplyCoreERP.Common.DocumentSequences;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;


namespace SupplyCoreERP.Catalog.ActiveIngredients;

public class ActiveIngredientManager : DomainService
{
    // Dependencies
    private readonly IRepository<ActiveIngredient, Guid> _repository;
    private readonly IRepository<MedicineIngredient, Guid> _medIngredientRepo;
    private readonly IDocumentSequenceManager _documentSequenceManager;

    // Constructor injection
    public ActiveIngredientManager(
        IRepository<ActiveIngredient, Guid> repository,
        IRepository<MedicineIngredient, Guid> medIngredientRepo,
        IDocumentSequenceManager documentSequenceManager
        )
    {
        _repository = repository;
        _medIngredientRepo = medIngredientRepo;
        _documentSequenceManager = documentSequenceManager;
    }

    public async Task<ActiveIngredient> CreateAsync(string name)
    {
        string code = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeIngredient);

        if (await _repository.AnyAsync(x => x.Code == code))
        {
            throw new BusinessException("SupplyCoreERP:InvalidActiveIngredientCode", $"Mã hoạt chất '{code}' đã tồn tại!");
        }

        return new ActiveIngredient(GuidGenerator.Create(), code, name);
    }

    public async Task UpdateAsync(ActiveIngredient entity, string newName)
    {
        entity.Update(newName);
    }

    public async Task DeleteAsync(ActiveIngredient entity)
    {
        //Check xem hoá chất có đang được sử dụng trong thuốc nào không, nếu có thì không cho xóa
        bool isUsed = await _medIngredientRepo.AnyAsync(x => x.ActiveIngredientId == entity.Id);

        if (isUsed)
        {
            throw new BusinessException("SupplyCoreERP:ActiveIngredientInUse", $"Hoạt chất '{entity.Name}' đang được sử dụng trong một số thuốc, không thể xóa.");
        }

        await _repository.DeleteAsync(entity);
    }
}







