using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Continents;
using SupplyCoreERP.Locations.Countries;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.MasterData
{
	public class Manufacturer : FullAuditedAggregateRoot<Guid>
	{
		public string Name { get; private set; } 
		public string Address { get; private set; }

		public Guid ContinentId { get; private set; }
		public virtual Continent Continent { get; private set; }

		public Guid CountryId { get; private set; }
		public virtual Country Country { get; private set; }

		public Guid CityId { get; private set; }
		public virtual City City { get; private set; }

		public Guid AreaId { get; private set; }
		public virtual Area Area { get; private set; }


		private Manufacturer() { }

		public Manufacturer(Guid id, string name, string address, Guid continentId, Guid countryId, Guid cityId, Guid areaId)
			: base(id)
		{
			Name = Check.NotNullOrWhiteSpace(name, nameof(Name), 255);
			Address = Check.NotNullOrWhiteSpace(address, nameof(Address), 500);
			ContinentId = continentId;
			CountryId = countryId;
			CityId = cityId;
			AreaId = areaId;
		}

		public void SetAddress(string address, Guid continentId, Guid countryId, Guid cityId, Guid areaId)
		{
			Address = Check.NotNullOrWhiteSpace(address, nameof(Address), 500);
			ContinentId = continentId;
			CountryId = countryId;
			CityId = cityId;
			AreaId = areaId;
		}
	}
}