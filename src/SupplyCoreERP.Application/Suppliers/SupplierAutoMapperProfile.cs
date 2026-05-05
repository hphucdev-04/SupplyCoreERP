using AutoMapper;
using SupplyCoreERP.Suppliers.Dtos;

namespace SupplyCoreERP.Suppliers
{
    public class SupplierAutoMapperProfile : Profile
    {
        public SupplierAutoMapperProfile()
        {
            CreateMap<Supplier, SupplierDto>()
                .ForMember(x => x.CityName, opt => opt.MapFrom(s => s.City != null ? s.City.Name : null));

            CreateMap<Supplier, SupplierDetailDto>()
                .IncludeBase<Supplier, SupplierDto>() // Kế thừa việc map CityName ở trên
                .ForMember(x => x.CountryName, opt => opt.MapFrom(s => s.Country != null ? s.Country.Name : null))
                .ForMember(x => x.AreaName, opt => opt.MapFrom(s => s.Area != null ? s.Area.Name : null));

            CreateMap<CreateUpdateSupplierDto, Supplier>();

            CreateMap<SupplierProduct, SupplierProductDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.DefaultUnitName, opt => opt.MapFrom(src => src.DefaultUnit != null ? src.DefaultUnit.Name : null));
        }
    }
}
