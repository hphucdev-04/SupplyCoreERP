using AutoMapper;
using SupplyCoreERP.BaseUnits.Dtos;

namespace SupplyCoreERP.BaseUnits
{
	public class BaseUnitAutoMapperProfile : Profile
	{
		public BaseUnitAutoMapperProfile()
		{
			CreateMap<BaseUnit, BaseUnitDto>();
			CreateMap<CreateUpdateBaseUnitDto, BaseUnit>();
		}
	}
}
