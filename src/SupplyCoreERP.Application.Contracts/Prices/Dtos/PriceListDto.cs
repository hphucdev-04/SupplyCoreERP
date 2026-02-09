using SupplyCoreERP.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Prices.Dtos
{
	public class PriceListDto : EntityDto<Guid>
	{
		public string Code { get; set; }
		public string Name { get; set; }
		public CurrencyType Currency { get; set; }
		public bool IsBase { get; set; }
	}
}
