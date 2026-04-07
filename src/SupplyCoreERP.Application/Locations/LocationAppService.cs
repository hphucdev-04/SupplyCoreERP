using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Continents;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Locations.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Locations
{
	public class LocationAppService : ApplicationService, ILocationAppService
	{
		private readonly IRepository<Continent, Guid> _continentRepository;
		private readonly IRepository<Country, Guid> _countryRepository;
		private readonly IRepository<City, Guid> _cityRepository;
		private readonly IRepository<Area, Guid> _areaRepository;

		public LocationAppService(
			IRepository<Continent, Guid> continentRepository,
			IRepository<Country, Guid> countryRepository,
			IRepository<City, Guid> cityRepository,
			IRepository<Area, Guid> areaRepository)
		{
			_continentRepository = continentRepository;
			_countryRepository = countryRepository;
			_cityRepository = cityRepository;
			_areaRepository = areaRepository;
		}

		public async Task<ListResultDto<ContinentDto>> GetContinentsAsync()
		{
			var items = await _continentRepository.GetListAsync();

			return new ListResultDto<ContinentDto>(
				ObjectMapper.Map<List<Continent>, List<ContinentDto>>(
					items.OrderBy(x => x.Name).ToList()
				)
			);
		}

		public async Task<ListResultDto<CountryDto>> GetCountriesByContinentAsync(Guid continentId)
		{
			var items = await _countryRepository.GetListAsync(x => x.ContinentId == continentId);

			return new ListResultDto<CountryDto>(
				ObjectMapper.Map<List<Country>, List<CountryDto>>(
					items.OrderBy(x => x.Name).ToList()
				)
			);
		}

		public async Task<ListResultDto<CountryDto>> GetAllCountriesAsync()
		{
			var items = await _countryRepository.GetListAsync();

			return new ListResultDto<CountryDto>(
				ObjectMapper.Map<List<Country>, List<CountryDto>>(
					items.OrderBy(x => x.Name).ToList()
				)
			);
		}

		public async Task<ListResultDto<CityDto>> GetCitiesByCountryAsync(Guid countryId)
		{
			var items = await _cityRepository.GetListAsync(x => x.CountryId == countryId);

			return new ListResultDto<CityDto>(
				ObjectMapper.Map<List<City>, List<CityDto>>(
					items.OrderBy(x => x.Name).ToList()
				)
			);
		}

		public async Task<ListResultDto<CityDto>> GetAllCitiesAsync()
		{
			var items = await _cityRepository.GetListAsync();

			return new ListResultDto<CityDto>(
				ObjectMapper.Map<List<City>, List<CityDto>>(
					items.OrderBy(x => x.Name).ToList()
				)
			);
		}

		public async Task<ListResultDto<AreaDto>> GetAreasByCityAsync(Guid cityId)
		{
			var items = await _areaRepository.GetListAsync(x => x.CityId == cityId);

			return new ListResultDto<AreaDto>(
				ObjectMapper.Map<List<Area>, List<AreaDto>>(
					items.OrderBy(x => x.Name).ToList()
				)
			);
		}
	}
}
