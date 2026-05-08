using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Locations.Continents;

public class Continent : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; private set; }

    private Continent() { }
    public Continent(Guid id, string name) : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(Name), 100);
    }
}
