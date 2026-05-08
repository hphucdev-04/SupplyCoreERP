using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using SupplyCoreERP.Orders.PO;
using SupplyCoreERP.PurchaseOrders.Dtos;

namespace SupplyCoreERP.PurchaseOrders;

public class PurchaseOrderAutoMapperProfile : Profile
{
    public PurchaseOrderAutoMapperProfile()
    {
        CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null));

        CreateMap<PurchaseOrderDetail, PurchaseOrderDetailDto>()
            .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : null))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : null));
    }
}
