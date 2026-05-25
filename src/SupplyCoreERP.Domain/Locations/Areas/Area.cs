using System;
using System.Collections.Generic;
using System.Text;
using SupplyCoreERP.Locations.Cities;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Locations.Areas;

public class Area : FullAuditedAggregateRoot<Guid>
{
    public Guid CityId { get; private set; }
    public virtual City City { get; private set; }
    public string ZipCode { get; private set; }
    public string Name { get; private set; }

    private Area() { }
    public Area(Guid id, Guid cityId, string zipCode, string name) : base(id)
    {
        CityId = cityId;
        ZipCode = Check.NotNullOrWhiteSpace(zipCode, nameof(ZipCode), 10).ToUpper();
        Name = Check.NotNullOrWhiteSpace(name, nameof(Name), 100);
    }
}






