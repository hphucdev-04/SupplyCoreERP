using System;
using SupplyCoreERP.Enums.Partner;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Customers.Dtos;

public class CustomerDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string? PhoneNumber { get; set; }
    public CustomerType Type { get; set; }
    public string? CityName { get; set; }
    public decimal CurrentDebt { get; set; }
    public bool IsActive { get; set; }
}
