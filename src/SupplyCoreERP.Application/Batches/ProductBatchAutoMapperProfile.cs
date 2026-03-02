using AutoMapper;
using SupplyCoreERP.Batches.Dtos;
using SupplyCoreERP.Inventories.Batches;

namespace SupplyCoreERP.Batches
{
	public class ProductBatchAutoMapperProfile : Profile
	{
		public ProductBatchAutoMapperProfile()
		{
			CreateMap<ProductBatch, ProductBatchDto>()
				.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
				.ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null));

		}
	}
}
