using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using SupplyCoreERP.Sales.Orders;
using SupplyCoreERP.SalesOrders.Dtos;

namespace SupplyCoreERP.SalesOrders;

public class SalesOrderAutoMapperProfile : Profile
{
    public SalesOrderAutoMapperProfile()
    {
        CreateMap<SalesOrder, SalesOrderDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null));

        CreateMap<SalesOrderDetail, SalesOrderDetailDto>()
            .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : null))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : null));
    }
}
