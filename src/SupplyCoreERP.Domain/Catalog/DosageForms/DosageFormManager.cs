using System;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.Medicines;
using SupplyCoreERP.Common.DocumentSequences;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;


namespace SupplyCoreERP.Catalog.DosageForms;

public class DosageFormManager : DomainService
{
    // Dependencies
    private readonly IRepository<DosageForm, Guid> _repository;
    private readonly IRepository<Medicine, Guid> _medicineRepository;
    private readonly IDocumentSequenceManager _documentSequenceManager;

    // Constructor injection
    public DosageFormManager(
        IRepository<DosageForm, Guid> repository,
        IRepository<Medicine, Guid> medicineRepository,
        IDocumentSequenceManager documentSequenceManager)
    {
        _repository = repository;
        _medicineRepository = medicineRepository;
        _documentSequenceManager = documentSequenceManager;
    }

    public async Task<DosageForm> CreateAsync(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));
        string normalizedName = name.Trim();

        string code = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeDosageForm);
        //Check trùng mã
        if (await _repository.AnyAsync(x => x.Code == code))
        {
            throw new BusinessException("SupplyCoreERP:InvalidDosageFormCode", $"Mã dạng bào chế '{code}' đã tồn tại!");
        }

        //Check trùng tên
        if (await _repository.AnyAsync(x => x.Name == normalizedName))
        {
            throw new BusinessException("SupplyCoreERP:InvalidDosageFormName", $"Tên dạng bào chế '{name}' đã tồn tại!");
        }

        return new DosageForm(GuidGenerator.Create(), code, normalizedName);
    }

    public async Task UpdateAsync(DosageForm entity, string newName)
    {
        Check.NotNull(entity, nameof(entity));
        Check.NotNullOrWhiteSpace(newName, nameof(newName));
        string normalizedName = newName.Trim();

        //Check trùng tên
        if (await _repository.AnyAsync(x => x.Name == normalizedName && x.Id != entity.Id))
        {
            throw new BusinessException("SupplyCoreERP:InvalidDosageFormName", $"Tên dạng bào chế '{newName}' đã được sử dụng!");
        }

        entity.Update(normalizedName);
    }

    public async Task DeleteAsync(DosageForm entity)
    {
        Check.NotNull(entity, nameof(entity));

        //KhÔng xóa nế u đang có thuốc dùng dạng này
        bool isInUse = await _medicineRepository.AnyAsync(x => x.DosageFormId == entity.Id);

        if (isInUse)
        {
            throw new BusinessException("SupplyCoreERP:DosageFormInUse", $"Không thể xóa '{entity.Name}' vì đang có thuốc sử dụng dạng bào chế này!");
        }

        await _repository.DeleteAsync(entity);
    }
}







