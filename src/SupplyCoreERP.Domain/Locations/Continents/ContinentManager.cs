using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

namespace SupplyCoreERP.Locations.Continents
{
	public class ContinentManager : DomainService
	{
		private readonly IRepository<Continent, Guid> _continentRepository;

		public ContinentManager(IRepository<Continent, Guid> continentRepository)
		{
			_continentRepository = continentRepository;
		}

		public async Task<Continent> CreateAsync(string name)
		{
			Check.NotNullOrWhiteSpace(name, nameof(name));

			// Check trùng tên
			if (await _continentRepository.AnyAsync(x => x.Name == name))
			{
				throw new UserFriendlyException($"Châu lục '{name}' đã tồn tại!");
			}

			return new Continent(GuidGenerator.Create(), name);
		}
	}
}
