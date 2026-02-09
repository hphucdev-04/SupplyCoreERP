using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Areas;

namespace SupplyCoreERP.Suppliers
{
	public class Supplier : FullAuditedAggregateRoot<Guid>
	{
		public string Code { get; private set; }
		public string Name { get; private set; }
		public string TaxCode { get; private set; }
		public string PhoneNumber { get; private set; }
		public string Email { get; private set; }
		public string RepresentativeName { get; private set; }
		public string Note { get; private set; }
		public bool IsActive { get; private set; }
		public string Address { get; private set; } 

		public Guid? CountryId { get; private set; }
		public virtual Country Country { get; private set; }

		public Guid? CityId { get; private set; }
		public virtual City City { get; private set; }

		public Guid? AreaId { get; private set; }
		public virtual Area Area { get; private set; }

		private Supplier() { }

		public Supplier(
			Guid id,
			string code,
			string name,
			string taxCode,
			string phoneNumber,
			string email,
			string representativeName,
			string note,
			string address,
			Guid? countryId,
			Guid? cityId,
			Guid? areaId)
			: base(id)
		{
			SetCode(code);
			SetName(name);
			TaxCode = taxCode;
			PhoneNumber = phoneNumber;
			Email = email;
			RepresentativeName = representativeName;
			Note = note;
			IsActive = true;

			SetLocation(address, countryId, cityId, areaId);
		}

		public void UpdateInfo(
			string name,
			string taxCode,
			string phoneNumber,
			string email,
			string representativeName,
			string note)
		{
			SetName(name);
			TaxCode = taxCode;
			PhoneNumber = phoneNumber;
			Email = email;
			RepresentativeName = representativeName;
			Note = note;
		}

		public void SetLocation(string address, Guid? countryId, Guid? cityId, Guid? areaId)
		{
			Address = address;
			CountryId = countryId;
			CityId = cityId;
			AreaId = areaId;
		}

		public void SetActive(bool isActive)
		{
			IsActive = isActive;
		}

		private void SetCode(string code)
		{
			Code = Check.NotNullOrWhiteSpace(code, nameof(Code), 50).Trim().ToUpper();
		}

		private void SetName(string name)
		{
			Name = Check.NotNullOrWhiteSpace(name, nameof(Name), 255).Trim();
		}
	}
}