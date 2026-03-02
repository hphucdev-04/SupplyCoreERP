using AutoMapper;
using SupplyCoreERP.Inventories.Tickets;
using SupplyCoreERP.Tickes.Dtos;
using SupplyCoreERP.Tickets.Dtos;

namespace SupplyCoreERP.Tickets
{
	public class InventoryTicketAutoMapperProfile : Profile
	{
		public InventoryTicketAutoMapperProfile()
		{
			CreateMap<InventoryTicket, InventoryTicketDto>()
				.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null));

			CreateMap<InventoryTicketDetail, InventoryTicketDetailDto>()
				.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
				.ForMember(dest => dest.BatchNumber, opt => opt.MapFrom(src => src.ProductBatch != null ? src.ProductBatch.BatchNumber : null))
				.ForMember(dest => dest.BinCode, opt => opt.MapFrom(src => src.Bin != null ? src.Bin.Code : null));
		}
	}
}
