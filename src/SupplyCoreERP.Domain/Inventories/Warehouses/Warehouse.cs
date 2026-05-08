using System;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Countries;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventories.Warehouses;

public class Warehouse : FullAuditedAggregateRoot<Guid>
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? Address { get; private set; }
    public Guid? CountryId { get; private set; }
    public virtual Country Country { get; protected set; }
    public Guid? CityId { get; private set; }
    public virtual City City { get; protected set; }
    public Guid? AreaId { get; private set; }
    public virtual Area Area { get; protected set; }

    public int MapWidth { get; private set; }
    public int MapLength { get; private set; }
    public ApprovalStatus Status { get; private set; }
    public bool IsActive { get; private set; }

    protected Warehouse() { }

    public Warehouse(Guid id, string code, string name, string? address, Guid? countryId, Guid? cityId, Guid? areaId, int mapWidth = 1000, int mapLength = 1000) : base(id)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(Code), 50).ToUpper();
        Name = Check.NotNullOrWhiteSpace(name, nameof(Name), 255);
        Address = address;
        CountryId = countryId;
        CityId = cityId;
        AreaId = areaId;
        MapWidth = mapWidth;
        MapLength = mapLength;
        Status = ApprovalStatus.Pending;
        IsActive = true;
    }

    public void UpdateInfo(string name, string? address, Guid? countryId, Guid? cityId, Guid? areaId)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(Name), 255);
        Address = address;
        CountryId = countryId;
        CityId = cityId;
        AreaId = areaId;
    }

    public void UpdateMapSize(int width, int length)
    {
        MapWidth = width > 0 ? width : 1000;
        MapLength = length > 0 ? length : 1000;
    }

    public void Approve() => Status = ApprovalStatus.Approved;
    public void Reject() => Status = ApprovalStatus.Rejected;
    public void SetActive(bool isActive) => IsActive = isActive;
}
