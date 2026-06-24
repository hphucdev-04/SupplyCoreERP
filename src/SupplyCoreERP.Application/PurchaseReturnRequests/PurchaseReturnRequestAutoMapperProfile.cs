using AutoMapper;
using SupplyCoreERP.Procurement.PurchaseReturnRequests;
using SupplyCoreERP.PurchaseReturnRequests.Dtos;

namespace SupplyCoreERP.PurchaseReturnRequests;

public class PurchaseReturnRequestAutoMapperProfile : Profile
{
    public PurchaseReturnRequestAutoMapperProfile()
    {
        CreateMap<PurchaseReturnRequest, PurchaseReturnRequestDto>()
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
            .ForMember(dest => dest.WarehouseCode, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Code : null))
            .ForMember(dest => dest.RelatedTickets, opt => opt.Ignore());

        CreateMap<PurchaseReturnRequestLine, PurchaseReturnRequestLineDto>()
            .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : null))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : null))
            .ForMember(dest => dest.PurchaseOrderCode, opt => opt.Ignore());
    }
}
