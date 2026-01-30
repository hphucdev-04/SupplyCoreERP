using AutoMapper;
using SupplyCoreERP.Manufacturers.Dtos;
using SupplyCoreERP.MasterData;
using System;
using System.Collections.Generic;
using System.Text;

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
