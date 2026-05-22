using System.Linq;
using AutoMapper;
using SupplyCoreERP.Suppliers.Dtos;

namespace SupplyCoreERP.Suppliers;

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
            .ForMember(dest => dest.DefaultUnitName, opt => opt.MapFrom(src => src.DefaultUnit != null ? src.DefaultUnit.Name : null))
            .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions));

        CreateMap<SupplierProductCondition, SupplierProductConditionDto>()
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : null));

        CreateMap<CreateUpdateSupplierProductConditionDto, SupplierProductCondition>();

        CreateMap<SupplierProduct, SupplierMedicineDto>()
            // Thông tin từ bảng Supplier
            .ForMember(dest => dest.SupplierCode, opt => opt.MapFrom(src => src.Supplier.Code))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name))
            .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.Supplier.CountryId))
            .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Supplier.Country.Name))

            // Thông tin từ bảng Unit
            .ForMember(dest => dest.DefaultUnitName, opt => opt.MapFrom(src => src.DefaultUnit.Name))

            // Lấy giá chuẩn và số lượng tối thiểu từ Condition khớp với DefaultUnitId
            .ForMember(dest => dest.StandardPrice, opt => opt.MapFrom(src =>
                src.Conditions != null && src.Conditions.Any(c => c.UnitId == src.DefaultUnitId)
                ? src.Conditions.First(c => c.UnitId == src.DefaultUnitId).StandardPrice
                : 0))
            .ForMember(dest => dest.MinOrderQuantity, opt => opt.MapFrom(src =>
                src.Conditions != null && src.Conditions.Any(c => c.UnitId == src.DefaultUnitId)
                ? src.Conditions.First(c => c.UnitId == src.DefaultUnitId).MinOrderQuantity
                : 0));
    }
}
