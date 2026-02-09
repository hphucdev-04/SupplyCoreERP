using AutoMapper;
using SupplyCoreERP.Medicines.Dtos;
using SupplyCoreERP.Products;

namespace SupplyCoreERP.Medicines
{
	public class MedicineAutoMapperProfile : Profile	
	{
		public MedicineAutoMapperProfile()
		{
			CreateMap<Medicine, MedicineDto>()
				.ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
				.ForMember(d => d.ManufacturerName, o => o.MapFrom(s => s.Manufacturer.Name))
				.ForMember(d => d.BaseUnitName, o => o.MapFrom(s => s.BaseUnit.Name))
				.ForMember(d => d.DosageFormName, o => o.MapFrom(s => s.DosageForm.Name))
				.ForMember(d => d.OriginCountryName, o => o.MapFrom(s => s.Manufacturer.Country.Name)); 

			CreateMap<Medicine, MedicineDetailDto>()
				.IncludeBase<Medicine, MedicineDto>();

			CreateMap<MedicineIngredient, MedicineIngredientDto>()
				.ForMember(d => d.ActiveIngredientName, o => o.MapFrom(s => s.ActiveIngredient.Name))
				.ForMember(d => d.ActiveIngredientCode, o => o.MapFrom(s => s.ActiveIngredient.Code));

			CreateMap<ProductUnit, MedicineUnitDto>()
				.ForMember(d => d.UnitName, o => o.MapFrom(s => s.Unit.Name));
		}
	}
}
