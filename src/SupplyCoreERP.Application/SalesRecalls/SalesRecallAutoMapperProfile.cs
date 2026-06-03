using AutoMapper;
using SupplyCoreERP.Sales.SalesRecalls;
using SupplyCoreERP.SalesRecalls.Dtos;

namespace SupplyCoreERP.SalesRecalls;

public class SalesRecallAutoMapperProfile : Profile
{
    public SalesRecallAutoMapperProfile()
    {
        CreateMap<SalesRecall, SalesRecallDto>()
            .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : null))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.BatchNumber, opt => opt.MapFrom(src => src.ProductBatch != null ? src.ProductBatch.BatchNumber : null))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
            .ForMember(dest => dest.WarehouseCode, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Code : null))
            .ForMember(dest => dest.RelatedTickets, opt => opt.Ignore());

        CreateMap<SalesRecallLine, SalesRecallLineDto>()
            .ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Code : null))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
            .ForMember(dest => dest.SalesOrderCode, opt => opt.MapFrom(src => src.SalesOrder != null ? src.SalesOrder.Code : null))
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : null));
    }
}
