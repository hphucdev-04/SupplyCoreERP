using SupplyCoreERP.Enums.Partner;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Prices; 
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Customers
{
	public class Customer : FullAuditedAggregateRoot<Guid>
	{
		public string Code { get; private set; }
		public string Name { get; private set; }
		public string? PhoneNumber { get; private set; }
		public string? Email { get; private set; }
		public string? RepresentativeName { get; private set; }
		public Gender? Gender { get; private set; }
		public CustomerType Type { get; private set; }
		public string? TaxCode { get; private set; }
		public string? Note { get; private set; }
		public bool IsActive { get; private set; }

		public string? Address { get; private set; }
		public Guid? CountryId { get; private set; }
		public virtual Country Country { get; private set; }
		public Guid? CityId { get; private set; }
		public virtual City City { get; private set; }
		public Guid? AreaId { get; private set; }
		public virtual Area Area { get; private set; }

		public decimal DebtLimit { get; private set; }
		public int PaymentTermDays { get; private set; }
		public decimal CurrentDebt { get; private set; }

		// ==========================================
		// THÊM: BẢNG GIÁ ÁP DỤNG CHO KHÁCH HÀNG
		// ==========================================
		public Guid? PriceListId { get; private set; }
		public virtual PriceList PriceList { get; private set; }

		private Customer() { }

		public Customer(
			Guid id, string code, string name, string? phoneNumber, string? email,
			string? representativeName, Gender? gender, CustomerType type, string? taxCode,
			string? address, Guid? countryId, Guid? cityId, Guid? areaId, string? note,
			decimal debtLimit = 0, int paymentTermDays = 0, Guid? priceListId = null) // <--- Thêm tham số
			: base(id)
		{
			SetCode(code);
			SetName(name);
			PhoneNumber = phoneNumber;
			Email = email;
			RepresentativeName = representativeName;
			Gender = gender;
			Type = type;
			TaxCode = taxCode;
			Note = note;
			IsActive = true;

			SetLocation(address, countryId, cityId, areaId);
			SetDebtInfo(debtLimit, paymentTermDays);
			SetPriceList(priceListId); // <--- Gọi hàm set
		}

		public void UpdateInfo(
			string name, string? phoneNumber, string? email, string? representativeName,
			Gender? gender, CustomerType type, string? taxCode, string? note)
		{
			SetName(name);
			PhoneNumber = phoneNumber;
			Email = email;
			RepresentativeName = representativeName;
			Gender = gender;
			Type = type;
			TaxCode = taxCode;
			Note = note;
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

		// Hàm riêng set bảng giá
		public void SetPriceList(Guid? priceListId)
		{
			PriceListId = priceListId;
		}

		public void AddDebt(decimal amount)
		{
			if (amount <= 0) throw new ArgumentException("Số tiền ghi nợ phải lớn hơn 0");

			if (DebtLimit > 0 && (CurrentDebt + amount > DebtLimit))
			{
				throw new UserFriendlyException(
					$"Giao dịch thất bại! Tổng nợ ({CurrentDebt + amount:N0}) " +
					$"vượt quá hạn mức nợ cho phép của Khách hàng này ({DebtLimit:N0}).");
			}
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
	}
}