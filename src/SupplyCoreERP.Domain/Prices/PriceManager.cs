using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Prices;

public class PriceManager : DomainService
{
    private readonly IRepository<ProductPrice, Guid> _productPriceRepository;
    private readonly IRepository<PriceList, Guid> _priceListRepository;

    public PriceManager(
        IRepository<ProductPrice, Guid> productPriceRepository,
        IRepository<PriceList, Guid> priceListRepository)
    {
        _productPriceRepository = productPriceRepository;
        _priceListRepository = priceListRepository;
    }

    public async Task<ProductPrice> CreatePriceAsync(Guid priceListId, Guid productId, Guid unitId, decimal price, int minQuantity = 1)
    {
        if (price < 0)
        {
            throw new UserFriendlyException("Giá bán không được nhỏ hơn 0!");
        }

        PriceList? priceList = await _priceListRepository.FindAsync(priceListId);
        if (priceList == null)
        {
            throw new UserFriendlyException("Bảng giá không tồn tại!");
        }

        //Một bảng giá + Một thuốc + Một đơn vị + Một mức số lượng -> Chỉ có 1 giá duy nhất
        var exists = await _productPriceRepository.AnyAsync(x =>
            x.PriceListId == priceListId &&
            x.ProductId == productId &&
            x.UnitId == unitId &&
            x.MinQuantity == minQuantity
        );

        if (exists)
        {
            throw new UserFriendlyException($"Đã tồn tại giá cho đơn vị này trong bảng giá '{priceList.Name}'!");
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

        // 1. TÌM TRONG BẢNG GIÁ ĐƯỢC CHỈ ĐỊNH (CỦA KHÁCH)
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

        // 2. FALLBACK - RỚT XUỐNG BẢNG GIÁ CHUẨN (IsBase = true)
        var basePrice = allPricesForProduct
            .Where(x => x.IsBase && x.MinQuantity <= quantity)
            .OrderByDescending(x => x.MinQuantity)
            .FirstOrDefault();

        if (basePrice != null)
        {
            return basePrice.Price;
        }

        throw new UserFriendlyException("Sản phẩm này chưa được thiết lập giá bán cho mức số lượng bạn chọn!");
    }

}
