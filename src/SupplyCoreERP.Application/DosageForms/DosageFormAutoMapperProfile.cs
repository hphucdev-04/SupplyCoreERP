using AutoMapper;
using SupplyCoreERP.DosageForms.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.DosageForms
{
	public class DosageFormAutoMapperProfile : Profile
	{
		public DosageFormAutoMapperProfile()
		{
			CreateMap<DosageForm, DosageFormDto>();
			CreateMap<CreateUpdateDosageFormDto, DosageForm>();
		}
	}
}
