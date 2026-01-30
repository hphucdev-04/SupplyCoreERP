using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.ActiveIngredients.Dtos
{
	public class ActiveIngredientDto : FullAuditedEntityDto<Guid>
	{
		public string Code { get; set; }
		public string Name { get; set; }
	}
}
