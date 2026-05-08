using System;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Enums.Warehouses;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Warehouses.Dtos;

public class ZoneDto : EntityDto<Guid>
{
    public Guid WarehouseId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public ZoneType Type { get; set; }
    public StorageCondition StorageCondition { get; set; }
    public string Color { get; set; }

    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int Width { get; set; }
    public int Length { get; set; }
    public float Rotation { get; set; }
}
