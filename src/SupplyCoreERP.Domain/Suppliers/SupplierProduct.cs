using System;
using System.Collections.Generic;
using System.Linq;
using SupplyCoreERP.BaseUnits;
using SupplyCoreERP.Products;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Suppliers;

/// <summary>
/// Danh mục sản phẩm theo từng nhà cung cấp.
/// Tương đương Purchasing Info Record trong SAP.
/// </summary>
public class SupplierProduct : AuditedEntity<Guid>
{
    public Guid SupplierId { get; private set; }
    public virtual Supplier Supplier { get; protected set; }

    public Guid ProductId { get; private set; }
    public virtual Product Product { get; protected set; }

    // ── Đơn vị mua mặc định của nhà cung cấp
    public Guid DefaultUnitId { get; private set; }
    public virtual BaseUnit DefaultUnit { get; protected set; }

    // ── Điều kiện mua chung
    public int LeadTimeDays { get; private set; }
    public bool IsPreferred { get; private set; }
    public bool IsActive { get; private set; }
    public string? Note { get; private set; }

    // ── Bảng giá và điều kiện chi tiết theo từng đơn vị tính (1-N)
    public virtual ICollection<SupplierProductCondition> Conditions { get; protected set; }

    protected SupplierProduct()
    {
        Conditions = new List<SupplierProductCondition>();
    }

    public SupplierProduct(
        Guid id,
        Guid supplierId,
        Guid productId,
        Guid defaultUnitId,
        int leadTimeDays,
        bool isPreferred = false,
        string? note = null) : base(id)
    {
        SupplierId = supplierId;
        ProductId = productId;
        DefaultUnitId = defaultUnitId;
        LeadTimeDays = Math.Max(0, leadTimeDays);
        IsPreferred = isPreferred;
        IsActive = true;
        Note = note;
        Conditions = new List<SupplierProductCondition>();
    }

    public void UpdateInfo(
        Guid defaultUnitId,
        int leadTimeDays,
        bool isPreferred,
        string? note)
    {
        DefaultUnitId = defaultUnitId;
        LeadTimeDays = Math.Max(0, leadTimeDays);
        IsPreferred = isPreferred;
        Note = note;
    }

    public void AddCondition(SupplierProductCondition condition)
    {
        Conditions.Add(condition);
    }

    public void RemoveCondition(Guid conditionId)
    {
        SupplierProductCondition? condition = Conditions.FirstOrDefault(c => c.Id == conditionId);
        if (condition != null)
        {
            Conditions.Remove(condition);
        }
    }

    public void ValidateConditions()
    {
        if (Conditions == null || !Conditions.Any())
        {
            return;
        }

        IEnumerable<IGrouping<Guid, SupplierProductCondition>> groups = Conditions.GroupBy(x => x.UnitId);

        foreach (IGrouping<Guid, SupplierProductCondition> group in groups)
        {
            // 1. Kiểm tra tính đồng nhất của Hệ số quy đổi
            var distinctFactors = group.Select(x => x.ConversionFactor).Distinct().ToList();
            if (distinctFactors.Count > 1)
            {
                throw new UserFriendlyException("Các mức quy cách của cùng một đơn vị tính bắt buộc phải sử dụng chung một hệ số quy đổi!");
            }

            // Sắp xếp các mức số lượng tối thiểu tăng dần
            var orderedConditions = group.OrderBy(x => x.MinOrderQuantity).ToList();

            for (int i = 1; i < orderedConditions.Count; i++)
            {
                SupplierProductCondition prev = orderedConditions[i - 1];
                SupplierProductCondition current = orderedConditions[i];

                // 2. Kiểm tra trùng MOQ
                if (current.MinOrderQuantity == prev.MinOrderQuantity)
                {
                    throw new UserFriendlyException($"Đơn vị tính đã được cấu hình mốc số lượng đặt tối thiểu là {current.MinOrderQuantity}!");
                }

                // 3. Quy tắc B: MOQ lớn hơn thì giá phải nhỏ hơn hoặc bằng
                if (current.StandardPrice > prev.StandardPrice)
                {
                    throw new UserFriendlyException($"Đơn giá thỏa thuận cho mốc số lượng đặt lớn hơn ({current.MinOrderQuantity}) phải nhỏ hơn hoặc bằng mức giá của mốc số lượng nhỏ hơn ({prev.StandardPrice:N0}đ)!");
                }
            }
        }
    }

    public void SetPreferred(bool isPreferred)
    {
        IsPreferred = isPreferred;
    }
    public void SetActive(bool active) => IsActive = active;
}

