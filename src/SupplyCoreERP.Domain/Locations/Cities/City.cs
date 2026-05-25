using System;
using System.Collections.Generic;
using System.Text;
using SupplyCoreERP.Locations.Countries;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Locations.Cities;

public class City : FullAuditedAggregateRoot<Guid>
{
    public Guid CountryId { get; private set; }
    public virtual Country Country { get; private set; }
    public string Name { get; private set; }

    private City() { }
    public City(Guid id, Guid countryId, string name) : base(id)
    {
        CountryId = countryId;
        Name = Check.NotNullOrWhiteSpace(name, nameof(Name), 100);
    }
}






