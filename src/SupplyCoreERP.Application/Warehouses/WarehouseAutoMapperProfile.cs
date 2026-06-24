using AutoMapper;
using SupplyCoreERP.Inventory.Warehouses;
using SupplyCoreERP.Warehouses.Dtos;

namespace SupplyCoreERP.Warehouses;

public class WarehouseAutoMapperProfile : Profile
{
    public WarehouseAutoMapperProfile()
    {
        // Warehouse mappings
        CreateMap<Warehouse, WarehouseDto>()
            .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City != null ? src.City.Name : null))
            .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Area != null ? src.Area.Name : null))
            .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country != null ? src.Country.Name : null));

        // Zone mappings
        CreateMap<Zone, ZoneDto>();

        // Bin mappings
        CreateMap<Bin, BinDto>()
            .ForMember(dest => dest.ZoneName, opt => opt.MapFrom(src => src.Zone != null ? src.Zone.Name : null))
            .ForMember(dest => dest.ZoneType, opt => opt.MapFrom(src => src.Zone != null ? src.Zone.Type : default))
            .ForMember(dest => dest.ZoneStorageCondition, opt => opt.MapFrom(src => src.Zone.StorageCondition));
    }
}

