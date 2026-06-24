using System;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Enums.Warehouses;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Warehouses.Dtos;

public class BinDto : EntityDto<Guid>
{
    public Guid WarehouseId { get; set; }
    public Guid ZoneId { get; set; }
    public string ZoneName { get; set; }
    public ZoneType ZoneType { get; set; }
    public StorageCondition ZoneStorageCondition { get; set; }

    public string Code { get; set; }

    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int Width { get; set; }
    public int Length { get; set; }
    public float Rotation { get; set; }

    public int MaxSKU { get; set; }
    public int Height { get; set; }
    public decimal MaxVolume { get; set; }
    public bool IsBlocked { get; set; }
}

