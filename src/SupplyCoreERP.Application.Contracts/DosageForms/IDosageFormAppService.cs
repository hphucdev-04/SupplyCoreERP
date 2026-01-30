using SupplyCoreERP.DosageForms.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.DosageForms
{
	public interface IDosageFormAppService : ICrudAppService<
		DosageFormDto, 
		Guid, 
		GetDosageFormListDto, 
		CreateUpdateDosageFormDto>
	{
	}
}
