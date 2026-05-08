using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SupplyCoreERP.DocumentSequences;
using SupplyCoreERP.Products;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

namespace SupplyCoreERP.BaseUnits;

public class BaseUnitManager : DomainService
{
    private readonly IRepository<BaseUnit, Guid> _repository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<ProductUnit, Guid> _productUnitRepository;
    private readonly DocumentSequenceManager _documentSequenceManager;

    public BaseUnitManager(
        IRepository<BaseUnit, Guid> repository,
        IRepository<Product, Guid> productRepository,
        IRepository<ProductUnit, Guid> productUnitRepository,
        DocumentSequenceManager documentSequenceManager
        )
    {
        _repository = repository;
        _productRepository = productRepository;
        _productUnitRepository = productUnitRepository;
        _documentSequenceManager = documentSequenceManager;
    }

    public async Task<BaseUnit> CreateAsync(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));

        var code = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeUnit);

        if (await _repository.AnyAsync(x => x.Code == code))
        {
            throw new UserFriendlyException($"Mã đơn vị '{code}' đã tồn tại!");
        }

        return new BaseUnit(GuidGenerator.Create(), code, name);
    }

    public async Task UpdateAsync(BaseUnit unit, string newName)
    {
        Check.NotNull(unit, nameof(unit));
        Check.NotNullOrWhiteSpace(newName, nameof(newName));
        unit.Update(newName);
    }

    public async Task DeleteAsync(BaseUnit unit)
    {
        Check.NotNull(unit, nameof(unit));

        //Check sản phẩm nào dùng làm Unit gốc (BaseUnit) không?
        var isUsedAsBase = await _productRepository.AnyAsync(x => x.BaseUnitId == unit.Id);
        if (isUsedAsBase)
        {
            throw new UserFriendlyException($"Không thể xóa đơn vị '{unit.Name}' vì đang là đơn vị gốc của một số sản phẩm!");
        }

        //Check sản phẩm nào dùng làm Unit quy đổi (trong bảng ProductUnit) không?
        var isUsedAsConversion = await _productUnitRepository.AnyAsync(x => x.UnitId == unit.Id);
        if (isUsedAsConversion)
        {
            throw new UserFriendlyException($"Không thể xóa đơn vị '{unit.Name}' vì đang được dùng làm đơn vị quy đổi!");
        }

        await _repository.DeleteAsync(unit);
    }
}
