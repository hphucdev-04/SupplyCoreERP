using SupplyCoreERP.Enums.Partner;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Countries;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Customers
{
	public class Customer : FullAuditedAggregateRoot<Guid>
	{
		public string Code { get; private set; }
		public string Name { get; private set; }
		public string PhoneNumber { get; private set; }
		public string Email { get; private set; }
		public DateTime? DateOfBirth { get; private set; }
		public Gender Gender { get; private set; }
		public CustomerType Type { get; private set; }
		public string TaxCode { get; private set; }
		public bool IsActive { get; private set; }

		public string Address { get; private set; }
		public Guid? CountryId { get; private set; }
		public virtual Country Country { get; private set; }
		public Guid? CityId { get; private set; }
		public virtual City City { get; private set; }
		public Guid? AreaId { get; private set; }
		public virtual Area Area { get; private set; }

		private Customer() { }

		public Customer(
			Guid id,
			string code,
			string name,
			string phoneNumber,
			string email,
			DateTime? dob,
			Gender gender,
			CustomerType type,
			string taxCode,
			string address,
			Guid? countryId,
			Guid? cityId,
			Guid? areaId)
			: base(id)
		{
			SetCode(code);
			SetName(name);
			PhoneNumber = phoneNumber;
			Email = email;
			DateOfBirth = dob;
			Gender = gender;
			Type = type;
			TaxCode = taxCode;
			IsActive = true;

			SetLocation(address, countryId, cityId, areaId);
		}

		public void UpdateInfo(
			string name,
			string phoneNumber,
			string email,
			DateTime? dob,
			Gender gender,
			CustomerType type,
			string taxCode)
		{
			SetName(name);
			PhoneNumber = phoneNumber;
			Email = email;
			DateOfBirth = dob;
			Gender = gender;
			Type = type;
			TaxCode = taxCode;
		}

		public void SetLocation(string address, Guid? countryId, Guid? cityId, Guid? areaId)
		{
			Address = address;
			CountryId = countryId;
			CityId = cityId;
			AreaId = areaId;
		}

		public void SetActive(bool isActive) => IsActive = isActive;
		private void SetCode(string code) => Code = Check.NotNullOrWhiteSpace(code, nameof(Code), 50).Trim().ToUpper();
		private void SetName(string name) => Name = Check.NotNullOrWhiteSpace(name, nameof(Name), 255).Trim();
	}
}