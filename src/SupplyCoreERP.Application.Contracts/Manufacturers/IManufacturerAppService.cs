using SupplyCoreERP.Manufacturers.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Manufacturers
{
	public interface IManufacturerAppService :
		ICrudAppService<
			ManufacturerDto,
			Guid,
			GetManufacturerListDto,
			CreateUpdateManufacturerDto>
	{
		
	}
}
