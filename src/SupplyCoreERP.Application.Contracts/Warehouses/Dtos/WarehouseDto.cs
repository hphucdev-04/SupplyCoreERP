using System;
using SupplyCoreERP.Enums.Warehouses;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Warehouses.Dtos;

public class WarehouseDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string? Address { get; set; }


    public Guid? CountryId { get; set; }
    public string CountryName { get; set; }
    public Guid? CityId { get; set; }
    public string? CityName { get; set; }
    public Guid? AreaId { get; set; }
    public string? AreaName { get; set; }

    public int MapWidth { get; set; }
    public int MapLength { get; set; }

    public ApprovalStatus Status { get; set; }
    public bool IsActive { get; set; }
}

