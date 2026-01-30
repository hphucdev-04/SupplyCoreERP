using SupplyCoreERP.Categories.Dtos;
using SupplyCoreERP.DosageForms.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.DosageForms
{
	public class DosageFormAppService : CrudAppService<
		DosageForm, 
		DosageFormDto, 
		Guid, 
		GetDosageFormListDto, 
		CreateUpdateDosageFormDto>, 
		IDosageFormAppService
	{
		private readonly DosageFormManager _dosageFormManager;
		public DosageFormAppService(
			IRepository<DosageForm, Guid> repository,
			DosageFormManager dosageFormManager)
			: base(repository)
		{
			_dosageFormManager = dosageFormManager;
		}

		public override async Task<DosageFormDto> CreateAsync(CreateUpdateDosageFormDto input)
		{
			// Manager check và tạo entity 
			var entity = await _dosageFormManager.CreateAsync(input.Code, input.Name);
			//Repository save vào DB
			await Repository.InsertAsync(entity);

			return ObjectMapper.Map<DosageForm, DosageFormDto>(entity);
		}

		public override async Task<DosageFormDto> UpdateAsync(Guid id, CreateUpdateDosageFormDto input)
		{
			var entity = await Repository.GetAsync(id);
			// Manager để đảm bảo tính hợp lệ của entity
			await _dosageFormManager.UpdateAsync(entity, input.Code, input.Name);
			//Repository update vào DB
			await Repository.UpdateAsync(entity);

			return ObjectMapper.Map<DosageForm, DosageFormDto>(entity);
		}

		public override async Task DeleteAsync(Guid id)
		{
			var entity = await Repository.GetAsync(id);
			await _dosageFormManager.DeleteAsync(entity);
		}

		protected override async Task<IQueryable<DosageForm>> CreateFilteredQueryAsync(GetDosageFormListDto input)
		{
			var query = await base.CreateFilteredQueryAsync(input);

			if (!input.Filter.IsNullOrWhiteSpace())
			{
				query = query.Where(x => x.Name.ToLower().Contains(input.Filter.ToLower()));
			}

			return query;
		}
	}
}
