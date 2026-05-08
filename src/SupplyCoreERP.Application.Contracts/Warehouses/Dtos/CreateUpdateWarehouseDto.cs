using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Warehouses.Dtos;

public class CreateUpdateWarehouseDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public Guid? CountryId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? AreaId { get; set; }

    [Range(100, 10000)]
    public int MapWidth { get; set; } = 1000;

    [Range(100, 10000)]
    public int MapLength { get; set; } = 1000;
}
