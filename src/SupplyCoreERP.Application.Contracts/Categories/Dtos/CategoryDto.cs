using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Categories.Dtos
{
	public class CategoryDto : AuditedEntityDto<Guid>
	{
		public string Code { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}
}
