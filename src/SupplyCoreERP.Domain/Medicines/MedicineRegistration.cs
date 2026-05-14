using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Medicines;

/// <summary>
/// Lưu trữ lịch sử số đăng ký (SĐK) của thuốc.
/// Hỗ trợ trường hợp một thuốc được gia hạn hoặc thay đổi SĐK theo thời gian.
/// </summary>
public class MedicineRegistration : FullAuditedEntity<Guid>
{
    public Guid MedicineId { get; private set; }
    
    public string RegistrationNumber { get; private set; }
    
    public DateTime? ValidFrom { get; private set; }
    
    public DateTime? ValidTo { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public string? Note { get; private set; }

    protected MedicineRegistration() { }

    public MedicineRegistration(
        Guid id,
        Guid medicineId,
        string registrationNumber,
        DateTime? validFrom = null,
        DateTime? validTo = null,
        bool isActive = true,
        string? note = null) : base(id)
    {
        MedicineId = medicineId;
        SetRegistrationNumber(registrationNumber);
        ValidFrom = validFrom;
        ValidTo = validTo;
        IsActive = isActive;
        Note = note;

        if (validFrom.HasValue && validTo.HasValue && validTo < validFrom)
        {
            throw new BusinessException("SupplyCoreERP:InvalidDateRange", "Ngày hết hạn không được nhỏ hơn ngày hiệu lực.");
        }
    }

    public void SetRegistrationNumber(string registrationNumber)
    {
        RegistrationNumber = Check.NotNullOrWhiteSpace(registrationNumber, nameof(RegistrationNumber), 100).Trim().ToUpper();
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void UpdateValidity(DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue && to < from)
        {
            throw new BusinessException("SupplyCoreERP:InvalidDateRange", "Ngày hết hạn không được nhỏ hơn ngày hiệu lực.");
        }
        ValidFrom = from;
        ValidTo = to;
    }

    public void SetNote(string? note)
    {
        Note = note;
    }
}
