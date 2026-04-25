using AutoMapper;
using SupplyCoreERP.Prices.Dtos;

namespace SupplyCoreERP.Prices
{
    public class PriceAutoMapperProfile : Profile
    {
        public PriceAutoMapperProfile()
        {
            CreateMap<PriceList, PriceListDto>();

            CreateMap<ProductPrice, ProductPriceDto>()
                .ForMember(dest => dest.PriceListName, opt => opt.MapFrom(src => src.PriceList.Name))
                .ForMember(dest => dest.PriceListCode, opt => opt.MapFrom(src => src.PriceList.Code))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.PriceList.Currency))
                .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit.Name));
        }
    }
}
