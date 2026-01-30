using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.DosageForms.Dtos
{
	public class DosageFormDto : FullAuditedEntityDto<Guid>
	{
		public string Code { get; set; }
		public string Name { get; set; }
	}
}
