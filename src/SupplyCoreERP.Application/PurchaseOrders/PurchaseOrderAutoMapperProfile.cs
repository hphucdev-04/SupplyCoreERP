using AutoMapper;
using SupplyCoreERP.Procurement.PurchaseOrders;
using SupplyCoreERP.PurchaseOrders.Dtos;

namespace SupplyCoreERP.PurchaseOrders;

public class PurchaseOrderAutoMapperProfile : Profile
{
    public PurchaseOrderAutoMapperProfile()
    {
        CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
            .ForMember(dest => dest.SupplierCode, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Code : null))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
            .ForMember(dest => dest.WarehouseCode, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Code : null))
            .ForMember(dest => dest.PurchaseRequisitionCode, opt => opt.MapFrom(src => src.PurchaseRequisition != null ? src.PurchaseRequisition.Code : null));

        CreateMap<PurchaseOrderLine, PurchaseOrderLineDto>()
            .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : null))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : null))
            .ForMember(dest => dest.BaseUnitName, opt => opt.MapFrom(src => src.Product != null && src.Product.BaseUnit != null ? src.Product.BaseUnit.Name : null));
    }
}

