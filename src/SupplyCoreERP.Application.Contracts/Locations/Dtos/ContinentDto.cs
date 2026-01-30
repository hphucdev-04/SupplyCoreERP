using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Locations.Dtos
{
	public class ContinentDto : EntityDto<Guid>
	{
		public string Name { get; set; }
	}
}
