using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Prices
{
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

		public async Task<ProductPrice> CreatePriceAsync(
			Guid priceListId,
			Guid productId,
			Guid unitId,
			decimal price,
			int minQuantity = 1)
		{
			//Validate price
			if (price < 0)
			{
				throw new UserFriendlyException("Giá bán không được nhỏ hơn 0!");
			}

			//Kiểm tra bảng giá có tồn tại?
			var priceList = await _priceListRepository.FindAsync(priceListId);
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
	}
}