using AutoMapper;
using SupplyCoreERP.Procurement.PurchaseRequisitions;
using SupplyCoreERP.PurchaseRequisitions.Dtos;

namespace SupplyCoreERP.PurchaseRequisitions;

public class PurchaseRequisitionAutoMapperProfile : Profile
{
    public PurchaseRequisitionAutoMapperProfile()
    {
        CreateMap<PurchaseRequisition, PurchaseRequisitionDto>()
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null));
        CreateMap<PurchaseRequisitionLine, PurchaseRequisitionLineDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : null))
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : null));
    }
}

