using AutoMapper;
using SupplyCoreERP.Inventories.Tickets;
using SupplyCoreERP.Tickets.Dtos;
using System;

namespace SupplyCoreERP.Tickets
{
	public class InventoryTicketAutoMapperProfile : Profile
	{
		public InventoryTicketAutoMapperProfile()
		{
			CreateMap<InventoryTicket, InventoryTicketDto>()
				.ForMember(dest => dest.WarehouseName,
					opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null));

			CreateMap<InventoryTicketDetail, InventoryTicketDetailDto>()
				// Product
				.ForMember(dest => dest.ProductName,
					opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
				.ForMember(dest => dest.ProductCode,
					opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : null))
				.ForMember(dest => dest.BaseUnitName,
					opt => opt.MapFrom(src => src.Product != null && src.Product.BaseUnit != null
						? src.Product.BaseUnit.Name : null))
				// Batch
				.ForMember(dest => dest.BatchNumber,
					opt => opt.MapFrom(src => src.ProductBatch != null ? src.ProductBatch.BatchNumber : null))
				.ForMember(dest => dest.ExpiryDate,
					opt => opt.MapFrom(src => src.ProductBatch != null ? src.ProductBatch.ExpiryDate : (DateTime?)null))
				// Bin
				.ForMember(dest => dest.BinCode,
					opt => opt.MapFrom(src => src.Bin != null ? src.Bin.Code : null))
				// Unit
				.ForMember(dest => dest.UnitName,
					opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : null))
				// ConversionFactor ánh xạ thẳng từ entity
				.ForMember(dest => dest.ConversionFactor,
					opt => opt.MapFrom(src => src.ConversionFactor));
		}
	}
}