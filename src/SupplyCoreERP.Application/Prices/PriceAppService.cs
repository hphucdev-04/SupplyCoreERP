using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Prices.Dtos;
using SupplyCoreERP.Sales.PriceLists;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Prices;

public class PriceAppService : SupplyCore, IPriceAppService
{
    private readonly IRepository<PriceList, Guid> _priceListRepo;
    private readonly IRepository<ProductPrice, Guid> _productPriceRepo;
    private readonly PriceManager _priceManager;

    public PriceAppService(
        IRepository<PriceList, Guid> priceListRepo,
        IRepository<ProductPrice, Guid> productPriceRepo,
        PriceManager priceManager)
    {
        _priceListRepo = priceListRepo;
        _productPriceRepo = productPriceRepo;
        _priceManager = priceManager;
    }

    public async Task<List<PriceListDto>> GetPriceListsAsync()
    {
        IQueryable<PriceList> query = await _priceListRepo.GetQueryableAsync();
        List<PriceList> list = await AsyncExecuter.ToListAsync(
            query
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code));

        return ObjectMapper.Map<List<PriceList>, List<PriceListDto>>(list);
    }

    public async Task<List<ProductPriceDto>> GetByProductAsync(Guid productId)
    {
        IQueryable<ProductPrice> query = await _productPriceRepo.GetQueryableAsync();
        List<ProductPrice> prices = await query
            .Include(x => x.PriceList)
            .Include(x => x.Unit)
            .Where(x => x.ProductId == productId)
            .OrderBy(x => x.PriceList.Code)
            .ToListAsync();

        return ObjectMapper.Map<List<ProductPrice>, List<ProductPriceDto>>(prices);
    }

    public async Task<ProductCostReferenceDto> GetCostReferenceAsync(Guid productId, Guid unitId)
    {
        decimal? lowestPurchasePrice = await _priceManager.GetLowestPurchasePriceAsync(productId, unitId);

        return new ProductCostReferenceDto
        {
            ProductId = productId,
            UnitId = unitId,
            LowestPurchasePrice = lowestPurchasePrice
        };
    }

    public async Task<ProductPriceDto> CreateAsync(CreateUpdateProductPriceDto input)
    {
        ProductPrice entity = await _priceManager.CreatePriceAsync(
            input.PriceListId,
            input.ProductId,
            input.UnitId,
            input.Price,
            input.MinQuantity
        );

        await _productPriceRepo.InsertAsync(entity, autoSave: true);

        IQueryable<ProductPrice> query = await _productPriceRepo.GetQueryableAsync();
        ProductPrice? saved = await query
            .Include(x => x.PriceList)
            .Include(x => x.Unit)
            .FirstOrDefaultAsync(x => x.Id == entity.Id);

        if (saved == null)
        {
            throw new UserFriendlyException("Không thể tải lại bản ghi giá bán vừa tạo.");
        }

        ProductPriceDto dto = ObjectMapper.Map<ProductPrice, ProductPriceDto>(saved);

        decimal? lowestPurchasePrice = await _priceManager.GetLowestPurchasePriceAsync(input.ProductId, input.UnitId);
        if (lowestPurchasePrice.HasValue && input.Price < lowestPurchasePrice.Value)
        {
            dto.BelowCostWarning = $"Giá bán ({input.Price:N0}) thấp hơn giá nhập chuẩn thấp nhất ({lowestPurchasePrice.Value:N0}).";
        }

        return dto;
    }

    public async Task<ProductPriceDto> UpdateAsync(Guid id, CreateUpdateProductPriceDto input)
    {
        ProductPrice entity = await _productPriceRepo.GetAsync(id);
        entity.UpdatePrice(input.Price);
        await _productPriceRepo.UpdateAsync(entity);

        // Reload với navigation properties để map đầy đủ DTO
        IQueryable<ProductPrice> query = await _productPriceRepo.GetQueryableAsync();
        ProductPrice? saved = await query
            .Include(x => x.PriceList)
            .Include(x => x.Unit)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (saved == null)
        {
            throw new UserFriendlyException("Không thể tải lại bản ghi giá bán sau khi cập nhật.");
        }

        ProductPriceDto dto = ObjectMapper.Map<ProductPrice, ProductPriceDto>(saved);

        // Dùng entity.ProductId / entity.UnitId vì UpdateSalesOrderLineDto không mang 2 field này
        decimal? lowestPurchasePrice = await _priceManager.GetLowestPurchasePriceAsync(entity.ProductId, entity.UnitId);
        if (lowestPurchasePrice.HasValue && input.Price < lowestPurchasePrice.Value)
        {
            dto.BelowCostWarning = $"Giá bán ({input.Price:N0}) thấp hơn giá nhập chuẩn thấp nhất ({lowestPurchasePrice.Value:N0}).";
        }

        return dto;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _productPriceRepo.DeleteAsync(id);
    }
}

