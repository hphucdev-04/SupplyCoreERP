using System;
using SupplyCoreERP.Enums.PriceList;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Prices;

public class PriceList : FullAuditedAggregateRoot<Guid>
{
    public string Code { get; private set; }
    public string Name { get; private set; }

    public CurrencyType Currency { get; private set; } // Loại tiền tệ

    //Dùng làm giá Fallback khi không tìm thấy giá ở bảng khác
    public bool IsBase { get; private set; }
    public bool IsActive { get; set; }

    protected PriceList() { }

    public PriceList(Guid id, string code, string name, bool isBase, CurrencyType currency = CurrencyType.VND)
        : base(id)
    {
        Code = code;
        Name = name;
        IsBase = isBase;
        Currency = currency;
        IsActive = true;
    }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}
