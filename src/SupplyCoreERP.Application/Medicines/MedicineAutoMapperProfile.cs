using AutoMapper;
using SupplyCoreERP.Medicines.Dtos;
using SupplyCoreERP.Products;

namespace SupplyCoreERP.Medicines;

public class MedicineAutoMapperProfile : Profile
{
    public MedicineAutoMapperProfile()
    {
        CreateMap<Medicine, MedicineDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.ManufacturerName, o => o.MapFrom(s => s.Manufacturer.Name))
            .ForMember(d => d.BaseUnitName, o => o.MapFrom(s => s.BaseUnit.Name))
            .ForMember(d => d.DosageFormName, o => o.MapFrom(s => s.DosageForm.Name))
            .ForMember(d => d.OriginCountryName, o => o.MapFrom(s => s.Manufacturer.Country.Name))
            .ForMember(d => d.OriginCountryISO, o => o.MapFrom(s => s.Manufacturer.Country.ISO))
            .ForMember(d => d.RegistrationNumber, o => o.MapFrom(s => s.GetCurrentRegistration() != null ? s.GetCurrentRegistration().RegistrationNumber : string.Empty))
            .ForMember(d => d.RegistrationValidFrom, o => o.MapFrom(s => s.GetCurrentRegistration() != null ? s.GetCurrentRegistration().ValidFrom : null))
            .ForMember(d => d.RegistrationValidTo, o => o.MapFrom(s => s.GetCurrentRegistration() != null ? s.GetCurrentRegistration().ValidTo : null))
            .ForMember(d => d.RegistrationNote, o => o.MapFrom(s => s.GetCurrentRegistration() != null ? s.GetCurrentRegistration().Note : string.Empty));

        CreateMap<Medicine, MedicineDetailDto>()
            .IncludeBase<Medicine, MedicineDto>()
            .ForMember(d => d.OriginCountryId, o => o.MapFrom(s => s.Manufacturer.Country.Id));

        CreateMap<MedicineRegistration, MedicineRegistrationDto>();

        CreateMap<MedicineIngredient, MedicineIngredientDto>()
            .ForMember(d => d.ActiveIngredientName, o => o.MapFrom(s => s.ActiveIngredient.Name))
            .ForMember(d => d.ActiveIngredientCode, o => o.MapFrom(s => s.ActiveIngredient.Code));


        CreateMap<ProductUnit, MedicineUnitDto>()
            .ForMember(d => d.UnitName, o => o.MapFrom(s => s.Unit.Name));
    }
}
