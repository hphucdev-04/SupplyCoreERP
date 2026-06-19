using AutoMapper;
using SupplyCoreERP.Inventory.Transactions;
using SupplyCoreERP.Transactions.Dtos;

namespace SupplyCoreERP;

public class SupplyCoreERPApplicationAutoMapperProfile : Profile
{
    public SupplyCoreERPApplicationAutoMapperProfile()
    {
        CreateMap<InventoryTransaction, InventoryTransactionDto>()
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : null))
            .ForMember(dest => dest.BatchNumber, opt => opt.MapFrom(src => src.ProductBatch != null ? src.ProductBatch.BatchNumber : null))
            .ForMember(dest => dest.BinCode, opt => opt.MapFrom(src => src.Bin != null ? src.Bin.Code : null))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.QuantityChanged));
    }
}

