using AutoMapper;
using SupplyCoreERP.Categories.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.Categories
{
	public class CategoryApplicationAutoMapperProfile : Profile
	{
		public CategoryApplicationAutoMapperProfile()
		{
			CreateMap<Category, CategoryDto>();
			CreateMap<CreateUpdateCategoryDto, Category>();
		}
	}
}
