using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SupplyCoreERP.Locations.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Locations;

public interface ILocationAppService : IApplicationService
{
    Task<ListResultDto<ContinentDto>> GetContinentsAsync();
    Task<ListResultDto<CountryDto>> GetCountriesByContinentAsync(Guid continentId);
    Task<ListResultDto<CountryDto>> GetAllCountriesAsync();
    Task<ListResultDto<CityDto>> GetCitiesByCountryAsync(Guid countryId);
    Task<ListResultDto<CityDto>> GetAllCitiesAsync();
    Task<ListResultDto<AreaDto>> GetAreasByCityAsync(Guid cityId);
}

