using System;
using System.Collections.Generic;
using System.Text;
using SupplyCoreERP.DosageForms.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.DosageForms;

public interface IDosageFormAppService : ICrudAppService<
    DosageFormDto,
    Guid,
    GetDosageFormListDto,
    CreateUpdateDosageFormDto>
{
}
