using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Products;

public class ProductManager : DomainService
{
    private readonly IRepository<Product, Guid> _productRepository;

    public ProductManager(IRepository<Product, Guid> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task CheckCodeAsync(string code, Guid? excludeId = null)
    {
        Check.NotNullOrWhiteSpace(code, nameof(code));
        var normalizedCode = code.Trim().ToUpper();


        // Check Code
        if (await _productRepository.AnyAsync(x => x.Code == normalizedCode && x.Id != excludeId))
        {
            throw new UserFriendlyException($"Mã sản phẩm '{code}' đã tồn tại!");
        }
    }
}
