using System;
using AutoMapper;
using SupplyCoreERP.Balances.Dtos;
using SupplyCoreERP.Inventories.Balances;

namespace SupplyCoreERP.Balances;

public class InventoryBalanceAutoMapperProfile : Profile
{
    public InventoryBalanceAutoMapperProfile()
    {
        CreateMap<InventoryBalance, InventoryBalanceDto>()
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
            .ForMember(dest => dest.BinCode, opt => opt.MapFrom(src => src.Bin != null ? src.Bin.Code : null))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.BatchNumber, opt => opt.MapFrom(src => src.ProductBatch != null ? src.ProductBatch.BatchNumber : null))
            .ForMember(dest => dest.BaseUnitName, opt => opt.MapFrom(src => src.Product != null && src.Product.BaseUnit != null ? src.Product.BaseUnit.Name : null));

        // Map Detail 
        CreateMap<InventoryBalance, InventoryBalanceDetailDto>()
            .IncludeBase<InventoryBalance, InventoryBalanceDto>()
            .ForMember(dest => dest.WarehouseAddress, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Address : null))
            .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.Warehouse != null && src.Warehouse.City != null ? src.Warehouse.City.Name : null))
            .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Warehouse != null && src.Warehouse.Area != null ? src.Warehouse.Area.Name : null))

            .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : null))
            .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ProductBatch != null ? (DateTime?)src.ProductBatch.ExpiryDate : null))
            .ForMember(dest => dest.ManufacturingDate, opt => opt.MapFrom(src => src.ProductBatch != null ? (DateTime?)src.ProductBatch.ManufacturingDate : null))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.ProductBatch != null && src.ProductBatch.Supplier != null ? src.ProductBatch.Supplier.Name : null)); ;

        CreateMap<InventoryReservation, InventoryReservationDto>()
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
            .ForMember(dest => dest.BinCode, opt => opt.MapFrom(src => src.Bin != null ? src.Bin.Code : null));
    }
}
