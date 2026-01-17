using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Continents;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.MasterData;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;


namespace SupplyCoreERP.Manufacturers
{
	public class ManufacturerManager : DomainService
	{
		private readonly IRepository<Manufacturer, Guid> _manufacturerRepository;
		private readonly IRepository<Continent, Guid> _continentRepository;
		private readonly IRepository<Country, Guid> _countryRepository;
		private readonly IRepository<City, Guid> _cityRepository;
		private readonly IRepository<Area, Guid> _areaRepository;

		public ManufacturerManager(
			IRepository<Manufacturer, Guid> manufacturerRepository,
			IRepository<Continent, Guid> continentRepository,
			IRepository<Country, Guid> countryRepository,
			IRepository<City, Guid> cityRepository,
			IRepository<Area, Guid> areaRepository)
		{
			_manufacturerRepository = manufacturerRepository;
			_continentRepository = continentRepository;
			_countryRepository = countryRepository;
			_cityRepository = cityRepository;
			_areaRepository = areaRepository;
		}

		public async Task<Manufacturer> CreateAsync(
			string name,
			string address,
			Guid continentId,
			Guid countryId,
			Guid cityId,
			Guid areaId)
		{
			//Check ID các bảng địa lý
			if (!await _continentRepository.AnyAsync(x => x.Id == continentId))
				throw new UserFriendlyException("Châu lục không tồn tại.");

			if (!await _countryRepository.AnyAsync(x => x.Id == countryId))
				throw new UserFriendlyException("Quốc gia không tồn tại.");

			if (!await _cityRepository.AnyAsync(x => x.Id == cityId))
				throw new UserFriendlyException("Thành phố không tồn tại.");

			if (!await _areaRepository.AnyAsync(x => x.Id == areaId))
				throw new UserFriendlyException("Khu vực không tồn tại.");

			//Check trùng tên nhà sản xuất
			if (await _manufacturerRepository.AnyAsync(x => x.Name == name))
			{
				throw new UserFriendlyException($"Nhà sản xuất '{name}' đã tồn tại.");
			}

			return new Manufacturer(
				GuidGenerator.Create(),
				name,
				address,
				continentId,
				countryId,
				cityId,
				areaId
			);
		}
	}
}
