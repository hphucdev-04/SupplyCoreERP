using SupplyCoreERP.Locations.Continents;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Locations.Countries
{
	public class Country : FullAuditedAggregateRoot<Guid>
	{
		public Guid ContinentId { get; private set; }
		public virtual Continent Continent { get; private set; }
		public string ISO { get; private set; } 
		public string Name { get; private set; }


		private Country() { }
		public Country(Guid id, Guid continentId, string iso, string name) : base(id)
		{
			ContinentId = continentId;
			ISO = Check.NotNullOrWhiteSpace(iso, nameof(ISO), 3).ToUpper();
			Name = Check.NotNullOrWhiteSpace(name, nameof(Name), 100);
		}
	}
}
