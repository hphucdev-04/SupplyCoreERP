using AutoMapper;
using SupplyCoreERP.Procurement.PurchaseReturns;
using SupplyCoreERP.PurchaseReturns.Dtos;

namespace SupplyCoreERP.PurchaseReturns;

public class PurchaseReturnAutoMapperProfile : Profile
{
    public PurchaseReturnAutoMapperProfile()
    {
        // PurchaseReturn Mappings
        CreateMap<PurchaseReturn, PurchaseReturnDto>()
            .ForMember(dest => dest.PurchaseOrderCode, opt => opt.MapFrom(src => src.PurchaseOrder != null ? src.PurchaseOrder.Code : null))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
            .ForMember(dest => dest.SupplierCode, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Code : null))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
            .ForMember(dest => dest.WarehouseCode, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Code : null))
            .ForMember(dest => dest.RelatedTickets, opt => opt.Ignore());

        CreateMap<PurchaseReturnLine, PurchaseReturnLineDto>()
            .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : null))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : null));
    }
}
