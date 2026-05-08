using AutoMapper;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Continents;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Locations.Dtos;

namespace SupplyCoreERP.Locations;

public class LocationAutoMapperProfile : Profile
{
    public LocationAutoMapperProfile()
    {
        CreateMap<Continent, ContinentDto>();
        CreateMap<Country, CountryDto>();
        CreateMap<City, CityDto>();
        CreateMap<Area, AreaDto>();
    }
}
