using AutoMapper;
using SupplyCoreERP.Suppliers.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.Suppliers
{
	public class SupplierAutoMapperProfile : Profile
	{
		public SupplierAutoMapperProfile()
		{
			CreateMap<Supplier, SupplierDto>()
				.ForMember(x => x.CountryName, opt => opt.MapFrom(s => s.Country != null ? s.Country.Name : null))
				.ForMember(x => x.CityName, opt => opt.MapFrom(s => s.City != null ? s.City.Name : null))
				.ForMember(x => x.AreaName, opt => opt.MapFrom(s => s.Area != null ? s.Area.Name : null));
			CreateMap<CreateUpdateSupplierDto, Supplier>();
			
		}
	}
}
