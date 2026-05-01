using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Prices.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Prices
{
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

        public async Task CreateAsync(CreateUpdateProductPriceDto input)
        {
            ProductPrice entity = await _priceManager.CreatePriceAsync(
                input.PriceListId,
                input.ProductId,
                input.UnitId,
                input.Price,
                input.MinQuantity
            );

            await _productPriceRepo.InsertAsync(entity);
        }

        public async Task UpdateAsync(Guid id, CreateUpdateProductPriceDto input)
        {
            ProductPrice entity = await _productPriceRepo.GetAsync(id);

            // Update chỉ cập nhật Giá và Số lượng
            // KHÔNG cập nhật PriceListId, ProductId, UnitId dù DTO có gửi lên
            // (Nếu muốn đổi mấy cái đó, user phải xóa đi tạo mới)

            entity.UpdatePrice(input.Price);
            // Nếu bạn chưa viết hàm UpdatePrice trong Entity thì dùng: entity.Price = input.Price;

            // Cập nhật số lượng min (nếu có logic này)
            // entity.MinQuantity = input.MinQuantity; 

            await _productPriceRepo.UpdateAsync(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _productPriceRepo.DeleteAsync(id);
        }
    }
}
