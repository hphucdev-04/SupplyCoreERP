using System;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Common.DocumentSequences;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Catalog.BaseUnits;

public class BaseUnitManager : DomainService
{
    // Dependencies
    private readonly IRepository<BaseUnit, Guid> _repository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<ProductUnit, Guid> _productUnitRepository;
    private readonly IDocumentSequenceManager _documentSequenceManager;

    // Constructor injection
    public BaseUnitManager(
        IRepository<BaseUnit, Guid> repository,
        IRepository<Product, Guid> productRepository,
        IRepository<ProductUnit, Guid> productUnitRepository,
        IDocumentSequenceManager documentSequenceManager
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

        string code = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeUnit);

        if (await _repository.AnyAsync(x => x.Code == code))
        {
            throw new BusinessException("SupplyCoreERP:InvalidBaseUnitCode", $"Mã đơn vị '{code}' đã tồn tại!");
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

        //Check sản phẩm nào đang dùng đơn vị này làm đơn vị gốc (BaseUnit) không?
        bool isUsedAsBase = await _productRepository.AnyAsync(x => x.BaseUnitId == unit.Id);
        if (isUsedAsBase)
        {
            throw new BusinessException("SupplyCoreERP:BaseUnitInUse", $"Không thể xóa đơn vị '{unit.Name}' vì đang là đơn vị gốc của một số sản phẩm!");
        }

        //Check sản phẩm nào đang dùng làm Unit quy đổi (trong bảng ProductUnit) không?
        bool isUsedAsConversion = await _productUnitRepository.AnyAsync(x => x.UnitId == unit.Id);
        if (isUsedAsConversion)
        {
            throw new BusinessException("SupplyCoreERP:BaseUnitInUse", $"Không thể xóa đơn vị '{unit.Name}' vì đang được sử dụng làm đơn vị quy đổi!");
        }

        await _repository.DeleteAsync(unit);
    }
}







