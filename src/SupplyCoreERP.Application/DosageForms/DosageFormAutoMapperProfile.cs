using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using SupplyCoreERP.DosageForms.Dtos;

namespace SupplyCoreERP.DosageForms;

public class DosageFormAutoMapperProfile : Profile
{
    public DosageFormAutoMapperProfile()
    {
        CreateMap<DosageForm, DosageFormDto>();
        CreateMap<CreateUpdateDosageFormDto, DosageForm>();
    }
}
