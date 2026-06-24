using System;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Partner.Suppliers;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Sales.PriceLists;

public class PriceManager : DomainService
{
    // Dependencies
    private readonly IRepository<ProductPrice, Guid> _productPriceRepository;
    private readonly IRepository<PriceList, Guid> _priceListRepository;
    private readonly IRepository<SupplierProductCondition, Guid> _supplierProductConditionRepository;

    // Constructor injection
    public PriceManager(
        IRepository<ProductPrice, Guid> productPriceRepository,
        IRepository<PriceList, Guid> priceListRepository,
        IRepository<SupplierProductCondition, Guid> supplierProductConditionRepository)
    {
        _productPriceRepository = productPriceRepository;
        _priceListRepository = priceListRepository;
        _supplierProductConditionRepository = supplierProductConditionRepository;
    }

    public async Task<ProductPrice> CreatePriceAsync(Guid priceListId, Guid productId, Guid unitId, decimal price, int minQuantity = 1)
    {
        if (price < 0)
        {
            throw new BusinessException("SupplyCoreERP:InvalidPrice", "Giá bán không được nhỏ hơn 0!");
        }

        PriceList? priceList = await _priceListRepository.FindAsync(priceListId);
        if (priceList == null)
        {
            throw new BusinessException("SupplyCoreERP:PriceListNotFound", "Bảng giá không tồn tại!");
        }

        bool exists = await _productPriceRepository.AnyAsync(x =>
            x.PriceListId == priceListId &&
            x.ProductId == productId &&
            x.UnitId == unitId &&
            x.MinQuantity == minQuantity
        );

        if (exists)
        {
            throw new BusinessException("SupplyCoreERP:PriceAlreadyExists", $"Đã tồn tại giá cho đơn vị này trong bảng giá '{priceList.Name}'!");
        }

        return new ProductPrice(
            GuidGenerator.Create(),
            priceListId,
            productId,
            unitId,
            price,
            minQuantity
        );
    }

    public async Task<decimal> GetOfficialPriceAsync(Guid? appliedPriceListId, Guid productId, Guid unitId, decimal quantity)
    {
        IQueryable<ProductPrice> priceQuery = await _productPriceRepository.GetQueryableAsync();
        IQueryable<PriceList> listQuery = await _priceListRepository.GetQueryableAsync();

        var allPricesForProduct = (from p in priceQuery
                                   join l in listQuery on p.PriceListId equals l.Id
                                   where l.IsActive
                                         && p.ProductId == productId
                                         && p.UnitId == unitId
                                   select new { p.Price, p.MinQuantity, p.PriceListId, l.IsBase })
                                  .ToList();

        if (appliedPriceListId.HasValue)
        {
            var targetPrice = allPricesForProduct
                .Where(x => x.PriceListId == appliedPriceListId.Value && x.MinQuantity <= quantity)
                .OrderByDescending(x => x.MinQuantity)
                .FirstOrDefault();

            if (targetPrice != null)
            {
                return targetPrice.Price;
            }
        }

        // FALLBACK - isBase = true
        var basePrice = allPricesForProduct
            .Where(x => x.IsBase && x.MinQuantity <= quantity)
            .OrderByDescending(x => x.MinQuantity)
            .FirstOrDefault();

        if (basePrice != null)
        {
            return basePrice.Price;
        }

        throw new BusinessException("SupplyCoreERP:PriceNotFound", "Sản phẩm này chưa được thiết lập giá bán cho mức số lượng bạn chọn!");
    }

    /// <summary>
    /// Tra giá nhập chuẩn thấp nhất (Min StandardPrice) của tất cả nhà cung cấp cho một sản phẩm + đơn vị.
    /// Dùng để cảnh báo khi giá bán thấp hơn giá nhập.
    /// </summary>
    /// <returns>Giá nhập chuẩn thấp nhất, hoặc null nếu không có dữ liệu tham chiếu.</returns>
    public async Task<decimal?> GetLowestPurchasePriceAsync(Guid productId, Guid unitId)
    {
        IQueryable<SupplierProductCondition> query = await _supplierProductConditionRepository.GetQueryableAsync();

        // Filter theo cả ProductId + UnitId — dùng 1 biến để tránh MinAsync throw trên sequence rỗng
        IQueryable<SupplierProductCondition> filtered = query.Where(c =>
            c.SupplierProduct.ProductId == productId &&
            c.UnitId == unitId);

        bool hasAny = await AsyncExecuter.AnyAsync(filtered);
        if (!hasAny) return null;

        return await AsyncExecuter.MinAsync(filtered, c => c.StandardPrice);
    }
}
