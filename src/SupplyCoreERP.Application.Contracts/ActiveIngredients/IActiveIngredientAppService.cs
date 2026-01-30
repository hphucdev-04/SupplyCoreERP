using SupplyCoreERP.ActiveIngredients.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.ActiveIngredients
{
	public interface IActiveIngredientAppService : ICrudAppService<
		ActiveIngredientDto,
		Guid,
		GetActiveIngredientListDto,
		CreateUpdateActiveIngredientDto
		>
	{
	}
}
