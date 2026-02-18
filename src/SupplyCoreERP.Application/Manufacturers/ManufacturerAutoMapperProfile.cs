using AutoMapper;
using SupplyCoreERP.Manufacturers.Dtos;

namespace SupplyCoreERP.Manufacturers
{
	public class ManufacturerAutoMapperProfile : Profile
	{
		public ManufacturerAutoMapperProfile()
		{
			CreateMap<Manufacturer, ManufacturerDto>();
			CreateMap<CreateUpdateManufacturerDto, Manufacturer>();
		}
	}
}
