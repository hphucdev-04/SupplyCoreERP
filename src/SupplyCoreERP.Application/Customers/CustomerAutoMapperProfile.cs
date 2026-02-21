using AutoMapper;
using SupplyCoreERP.Customers.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.Customers
{
	public class CustomerAutoMapperProfile : Profile
	{
		public CustomerAutoMapperProfile()
		{
			CreateMap<Customer, CustomerDto>()
				.ForMember(x => x.CityName, opt => opt.MapFrom(c => c.City != null ? c.City.Name : null));

			CreateMap<Customer, CustomerDetailDto>()
				.IncludeBase<Customer, CustomerDto>()
				.ForMember(x => x.CountryName, opt => opt.MapFrom(c => c.Country != null ? c.Country.Name : null))
				.ForMember(x => x.AreaName, opt => opt.MapFrom(c => c.Area != null ? c.Area.Name : null));

			CreateMap<CreateUpdateCustomerDto, Customer>();
		}
	}
}
