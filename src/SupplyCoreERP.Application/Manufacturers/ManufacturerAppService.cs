

using SupplyCoreERP.Locations.Continents;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Manufacturers.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Manufacturers
{
	public class ManufacturerAppService :
		CrudAppService<
			Manufacturer,
			ManufacturerDto,
			Guid,
			GetManufacturerListDto,
			CreateUpdateManufacturerDto>,
		IManufacturerAppService
	{
		private readonly ManufacturerManager _manager;
		private readonly IRepository<Country, Guid> _countryRepository;
		private readonly IRepository<Continent, Guid> _continentRepository;

		public ManufacturerAppService(
			IRepository<Manufacturer, Guid> repository,
			ManufacturerManager manager,
			IRepository<Country, Guid> countryRepository,
			IRepository<Continent, Guid> continentRepository)
			: base(repository)
		{
			_manager = manager;
			_countryRepository = countryRepository;
			_continentRepository = continentRepository;
		}

		protected override async Task<IQueryable<Manufacturer>> CreateFilteredQueryAsync(GetManufacturerListDto input)
		{
			var query = await base.CreateFilteredQueryAsync(input);

			if (!input.Filter.IsNullOrWhiteSpace())
			{
				query = query.Where(x => x.Name.ToLower().Contains(input.Filter.ToLower()));
			}

			return query;
		}

		public override async Task<PagedResultDto<ManufacturerDto>> GetListAsync(GetManufacturerListDto input)
		{
			var query = await CreateFilteredQueryAsync(input);
			var totalCount = await AsyncExecuter.CountAsync(query);

			var entities = await AsyncExecuter.ToListAsync(
				query.OrderByDescending(e => e.CreationTime) 
					 .PageBy(input)
			);

			//Map sang DTO cơ bản
			var dtos = ObjectMapper.Map<List<Manufacturer>, List<ManufacturerDto>>(entities);

			//Lấy danh sách ID cần tìm tên (Để tối ưu query, tránh N+1)
			var countryIds = dtos.Select(x => x.CountryId).Distinct().ToArray();
			var continentIds = dtos.Select(x => x.ContinentId).Distinct().ToArray();

			//Truy vấn lấy tên
			var countries = await _countryRepository.GetListAsync(x => countryIds.Contains(x.Id));
			var continents = await _continentRepository.GetListAsync(x => continentIds.Contains(x.Id));

			//Gán tên vào DTO
			foreach (var dto in dtos)
			{
				dto.CountryName = countries.FirstOrDefault(x => x.Id == dto.CountryId)?.Name;
				dto.ContinentName = continents.FirstOrDefault(x => x.Id == dto.ContinentId)?.Name;
			}

			return new PagedResultDto<ManufacturerDto>(totalCount, dtos);
		}

		public override async Task<ManufacturerDto> CreateAsync(CreateUpdateManufacturerDto input)
		{
			var entity = await _manager.CreateAsync(
				input.Name,
				input.ContinentId,
				input.CountryId
			);

			await Repository.InsertAsync(entity);

			return ObjectMapper.Map<Manufacturer, ManufacturerDto>(entity);
		}

		public override async Task<ManufacturerDto> UpdateAsync(Guid id, CreateUpdateManufacturerDto input)
		{
			var entity = await Repository.GetAsync(id);

			await _manager.UpdateAsync(
				entity,
				input.Name,
				input.ContinentId,
				input.CountryId
			);

			await Repository.UpdateAsync(entity);

			return ObjectMapper.Map<Manufacturer, ManufacturerDto>(entity);
		}

		public override async Task DeleteAsync(Guid id)
		{
			var entity = await Repository.GetAsync(id);
			await _manager.DeleteAsync(entity);
		}
	}
}
