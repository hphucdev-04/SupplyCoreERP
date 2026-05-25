using AutoMapper;
using SupplyCoreERP.Catalog.DosageForms;
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

