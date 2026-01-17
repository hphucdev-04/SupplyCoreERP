using SupplyCoreERP.Locations.Continents;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

namespace SupplyCoreERP.Locations.Countries
{
	public class CountryManager : DomainService
	{
		private readonly IRepository<Country, Guid> _countryRepository;
		private readonly IRepository<Continent, Guid> _continentRepository;

		public CountryManager(
			IRepository<Country, Guid> countryRepository,
			IRepository<Continent, Guid> continentRepository)
		{
			_countryRepository = countryRepository;
			_continentRepository = continentRepository;
		}

		public async Task<Country> CreateAsync(Guid continentId, string iso, string name)
		{
			Check.NotNullOrWhiteSpace(iso, nameof(iso));
			Check.NotNullOrWhiteSpace(name, nameof(name));

			//Check Châu lục tồn tại
			if (!await _continentRepository.AnyAsync(x => x.Id == continentId))
			{
				throw new UserFriendlyException("Châu lục không tồn tại!");
			}

			//Check trùng mã ISO 
			if (await _countryRepository.AnyAsync(x => x.ISO == iso))
			{
				throw new UserFriendlyException($"Mã quốc gia '{iso}' đã tồn tại!");
			}

			return new Country(GuidGenerator.Create(), continentId, iso, name);
		}
	}
}
