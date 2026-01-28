using SupplyCoreERP.BaseUnits.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.BaseUnits
{
	public class BaseUnitAppService : CrudAppService<
		BaseUnit,
		BaseUnitDto,
		Guid,
		GetBaseUnitListDto,
		CreateUpdateBaseUnitDto>, 
		IBaseUnitAppService
	{
		private readonly BaseUnitManager _baseUnitManager;
	}
}
