using System;
using AutoMapper;
using SupplyCoreERP.Dashboard.Dtos;
using SupplyCoreERP.Inventory.Balances;
using SupplyCoreERP.Inventory.Warehouses;

namespace SupplyCoreERP.Dashboard;

public class DashboardAutoMapperProfile : Profile
{
    public DashboardAutoMapperProfile()
    {
        CreateMap<Warehouse, DashboardWarehouseCapacityDto>()
            .ForMember(dest => dest.WarehouseId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.CapacityPercent, opt => opt.Ignore());

        CreateMap<InventoryBalance, DashboardExpiredBatchDto>()
            .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.BatchNumber, opt => opt.MapFrom(src => src.ProductBatch.BatchNumber))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse.Name))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ProductBatch.ExpiryDate))
            .ForMember(dest => dest.DaysRemaining, opt => opt.MapFrom(src => (src.ProductBatch.ExpiryDate.Date - DateTime.Now.Date).Days));
    }
}
