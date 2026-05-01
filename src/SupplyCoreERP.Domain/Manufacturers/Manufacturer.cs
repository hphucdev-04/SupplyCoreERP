using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Continents;
using SupplyCoreERP.Locations.Countries;
using System;
using System.Net;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Manufacturers
{
	public class Manufacturer : FullAuditedAggregateRoot<Guid>
	{
		public string Code { get; private set; }
		public string Name { get; private set; } 

		public Guid ContinentId { get; private set; }
		public virtual Continent Continent { get; private set; }

		public Guid CountryId { get; private set; }
		public virtual Country Country { get; private set; }



		private Manufacturer() { }

		public Manufacturer(Guid id, string code, string name, Guid continentId, Guid countryId)
			: base(id)
		{
			Code = Check.NotNullOrWhiteSpace(code, nameof(Code), 50);
            Name = Check.NotNullOrWhiteSpace(name, nameof(Name), 255);
			ContinentId = continentId;
			CountryId = countryId;
		}

		public void Update(string name, Guid continentId, Guid countryId)
		{
			Name = Check.NotNullOrWhiteSpace(name, nameof(Name), 255);
			ContinentId = continentId;
			CountryId = countryId;
		}
	}
}