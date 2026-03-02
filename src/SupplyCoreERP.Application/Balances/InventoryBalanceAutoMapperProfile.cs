using AutoMapper;
using SupplyCoreERP.Balances.Dtos;
using SupplyCoreERP.Inventories.Balances;
using System;

namespace SupplyCoreERP.Balances
{
	public class InventoryBalanceAutoMapperProfile : Profile
	{
		public InventoryBalanceAutoMapperProfile()
		{
			CreateMap<InventoryBalance, InventoryBalanceDto>()
				.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
				.ForMember(dest => dest.BinCode, opt => opt.MapFrom(src => src.Bin != null ? src.Bin.Code : null))
				.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
				.ForMember(dest => dest.BatchNumber, opt => opt.MapFrom(src => src.ProductBatch != null ? src.ProductBatch.BatchNumber : null))
				.ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ProductBatch != null ? (DateTime?)src.ProductBatch.ExpiryDate : null));
		}
	}
}
