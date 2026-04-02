using AutoMapper;
using SupplyCoreERP.Orders;
using SupplyCoreERP.PO.Dtos;
using SupplyCoreERP.Purchasing.Orders;

namespace SupplyCoreERP.PO
{
	public class POAutoMapperProfile : Profile
	{
		public POAutoMapperProfile() 
		{
			CreateMap<PurchaseOrder, PurchaseOrderDto>()
				.ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name));

			CreateMap<PurchaseOrderDetail, PurchaseOrderDetailDto>()
				.ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product.Code))
				.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
				.ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit.Name));
		}
	}
}
