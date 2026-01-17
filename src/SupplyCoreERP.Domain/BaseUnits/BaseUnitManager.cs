using SupplyCoreERP.Products;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

namespace SupplyCoreERP.BaseUnits
{
	public class BaseUnitManager : DomainService
	{
		private readonly IRepository<BaseUnit, Guid> _repository;
		private readonly IRepository<Product, Guid> _productRepository;
		private readonly IRepository<ProductUnit, Guid> _productUnitRepository;
		public BaseUnitManager(
			IRepository<BaseUnit, Guid> repository,
			IRepository<Product, Guid> productRepository,
			IRepository<ProductUnit, Guid> productUnitRepository)
		{
			_repository = repository;
			_productRepository = productRepository;
			_productUnitRepository = productUnitRepository;
		}

		public async Task<BaseUnit> CreateAsync(string code, string name)
		{
			Check.NotNullOrWhiteSpace(code, nameof(code));
			Check.NotNullOrWhiteSpace(name, nameof(name));

			var normalizedCode = code.Trim().ToUpper();

			if (await _repository.AnyAsync(x => x.Code == normalizedCode))
				throw new UserFriendlyException($"Mã đơn vị '{code}' đã tồn tại!");

			return new BaseUnit(GuidGenerator.Create(), normalizedCode, name);
		}

		public async Task UpdateAsync(BaseUnit unit, string newCode, string newName)
		{
			Check.NotNull(unit, nameof(unit));
			Check.NotNullOrWhiteSpace(newCode, nameof(newCode));
			Check.NotNullOrWhiteSpace(newName, nameof(newName));

			var normalizedCode = newCode.Trim().ToUpper();

			// Check trùng mã 
			if (await _repository.AnyAsync(x => x.Code == normalizedCode && x.Id != unit.Id))
			{
				throw new UserFriendlyException($"Mã đơn vị '{newCode}' đã được sử dụng!");
			}

			unit.Update(normalizedCode, newName);
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
}
