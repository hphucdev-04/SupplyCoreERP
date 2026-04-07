using SupplyCoreERP.Enums.Partner;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Countries;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Suppliers
{
	public class Supplier : FullAuditedAggregateRoot<Guid>
	{
		public string Code { get; private set; }
		public string Name { get; private set; }
		public string? TaxCode { get; private set; }
		public string? PhoneNumber { get; private set; }
		public string? Email { get; private set; }
		public string? RepresentativeName { get; private set; }
		public Gender? Gender { get; private set; }
		public string? Note { get; private set; }
		public bool IsActive { get; private set; }
		public string? Address { get; private set; }

		public decimal DebtLimit { get; private set; }
		public int PaymentTermDays { get; private set; }
		public decimal CurrentDebt { get; private set; }

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
			string? taxCode,
			string? phoneNumber,
			string? email,
			string? representativeName,
			string? note,
			string? address,
			Guid? countryId,
			Guid? cityId,
			Guid? areaId,
			Gender? gender,
			decimal debtLimit = 0,
			int paymentTermDays = 0)
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
			Gender = gender;

			SetLocation(address, countryId, cityId, areaId);
			SetDebtInfo(debtLimit, paymentTermDays);
		}

		public void UpdateInfo(
			string name,
			Gender? gender,
			string? taxCode,
			string? phoneNumber,
			string? email,
			string? representativeName,
			string? note)
		{
			SetName(name);
			TaxCode = taxCode;
			PhoneNumber = phoneNumber;
			Email = email;
			RepresentativeName = representativeName;
			Note = note;
			Gender = gender;
		}

		public void SetLocation(string? address, Guid? countryId, Guid? cityId, Guid? areaId)
		{
			Address = address;
			CountryId = countryId;
			CityId = cityId;
			AreaId = areaId;
		}

		public void SetDebtInfo(decimal debtLimit, int paymentTermDays)
		{
			DebtLimit = debtLimit >= 0 ? debtLimit : 0;
			PaymentTermDays = paymentTermDays >= 0 ? paymentTermDays : 0;
		}

		public void AddDebt(decimal amount)
		{
			if (amount <= 0) throw new ArgumentException("Số tiền ghi nợ phải lớn hơn 0");
			if (DebtLimit > 0 && (CurrentDebt + amount > DebtLimit))
				throw new UserFriendlyException($"Vượt quá hạn mức nợ cho phép ({DebtLimit:N0}).");
			CurrentDebt += amount;
		}

		public void PayDebt(decimal amount)
		{
			if (amount <= 0) throw new ArgumentException("Số tiền thanh toán phải lớn hơn 0");
			CurrentDebt -= amount;
		}

		public void SetActive(bool isActive) => IsActive = isActive;
		private void SetCode(string code) => Code = Check.NotNullOrWhiteSpace(code, nameof(Code), 50).Trim().ToUpper();
		private void SetName(string name) => Name = Check.NotNullOrWhiteSpace(name, nameof(Name), 255).Trim();

		public void UpdateCode(string newCode)
		{
			SetCode(newCode);
		}
	}
}