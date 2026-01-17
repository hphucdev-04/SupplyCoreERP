using SupplyCoreERP.Locations.Cities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

namespace SupplyCoreERP.Locations.Areas
{
	public class AreaManager : DomainService
	{
		private readonly IRepository<Area, Guid> _areaRepository;
		private readonly IRepository<City, Guid> _cityRepository;

		public AreaManager(
			IRepository<Area, Guid> areaRepository,
			IRepository<City, Guid> cityRepository)
		{
			_areaRepository = areaRepository;
			_cityRepository = cityRepository;
		}

		public async Task<Area> CreateAsync(Guid cityId, string zipCode, string name)
		{
			Check.NotNullOrWhiteSpace(name, nameof(name));

			//Check Thành phố
			if (!await _cityRepository.AnyAsync(x => x.Id == cityId))
			{
				throw new UserFriendlyException("Thành phố không tồn tại!");
			}

			//Check trùng tên 
			if (await _areaRepository.AnyAsync(x => x.CityId == cityId && x.Name == name))
			{
				throw new UserFriendlyException($"Khu vực '{name}' đã tồn tại trong thành phố này!");
			}

			return new Area(GuidGenerator.Create(), cityId, zipCode, name);
		}
	}
}
