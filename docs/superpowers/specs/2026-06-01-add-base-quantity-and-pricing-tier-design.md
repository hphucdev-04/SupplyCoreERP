# Design Specification: Add BaseQuantity & Implement Pricing Tier Tiered Logic

**Date:** 2026-06-01  
**Author:** Antigravity  
**Status:** Approved  

---

## 1. Goal

This specification addresses two key enhancements to the SupplyCoreERP system:
1. **Backend:** Bổ sung trường `BaseQuantity` vào thực thể `InventoryTicketLine` để tự động lưu trữ số lượng quy đổi về đơn vị gốc (`BaseUnit`), đảm bảo chuẩn DDD và toàn vẹn dữ liệu tồn kho.
2. **Frontend:** Giải quyết vấn đề hiển thị trùng lặp đơn vị tính trên Drawer thêm dòng đơn mua hàng (`Add Product` drawer của `PurchaseOrder`), đồng thời tự động cập nhật đơn giá theo chính sách giá bậc thang (Price Tier) dựa trên số lượng tối thiểu đặt hàng (MQT / MOQ) mà người dùng nhập trên UI.

---

## 2. Backend Design

### 2.1. Entity: `InventoryTicketLine`
Bổ sung trường `BaseQuantity` với cơ chế đóng gói hoàn toàn trong Entity (Phương án B1). Số lượng gốc luôn bằng `Quantity * ConversionFactor`.

* **File:** `src/SupplyCoreERP.Domain/Inventory/Tickets/InventoryTicketLine.cs`
* **Changes:**
  * Thêm trường `public decimal BaseQuantity { get; private set; }`.
  * Cập nhật Constructor gán `BaseQuantity = quantity * conversionFactor;`.
  * Cập nhật phương thức `UpdateQuantity` gán `BaseQuantity = quantity * ConversionFactor;`.

```csharp
public class InventoryTicketLine : AuditedEntity<Guid>
{
    // ... Existing properties ...
    
    public decimal BaseQuantity { get; private set; }

    public InventoryTicketLine(
        Guid id,
        Guid ticketId,
        Guid productId,
        Guid unitId,
        int conversionFactor,
        Guid? referenceDocumentLineId,
        decimal quantity) : base(id)
    {
        TicketId = ticketId;
        ProductId = productId;
        UnitId = unitId;
        ConversionFactor = conversionFactor;
        ReferenceDocumentLineId = referenceDocumentLineId;
        Quantity = quantity;
        BaseQuantity = quantity * conversionFactor; // Auto-calculated
        Details = new List<InventoryTicketDetail>();
    }

    public void UpdateQuantity(decimal quantity)
    {
        Quantity = quantity;
        BaseQuantity = quantity * ConversionFactor; // Auto-calculated
    }
}
```

### 2.2. DTO: `InventoryTicketLineDto`
Bổ sung trường để đồng bộ dữ liệu API từ Backend sang Frontend.

* **File:** `src/SupplyCoreERP.Application.Contracts/Tickets/Dtos/InventoryTicketLineDto.cs`
* **Changes:**
  * Bổ sung `public decimal BaseQuantity { get; set; }`.

```csharp
public class InventoryTicketLineDto : AuditedEntityDto<Guid>
{
    // ... Existing properties ...
    public decimal BaseQuantity { get; set; }
}
```

### 2.3. Entity Framework Core Mapping
Cấu hình kiểu dữ liệu chính xác cho cơ sở dữ liệu PostgreSQL.

* **File:** `src/SupplyCoreERP.EntityFrameworkCore/EntityFrameworkCore/SupplyCoreERPDbContext.cs`
* **Changes:**
  * Thêm dòng mapping: `b.Property(x => x.BaseQuantity).HasColumnType("decimal(18, 2)");`

```csharp
builder.Entity<InventoryTicketLine>(b =>
{
    // ... Existing configs ...
    b.Property(x => x.BaseQuantity).HasColumnType("decimal(18, 2)");
});
```

---

## 3. Frontend (Angular) Design

### 3.1. Unique Unit Items Dropdown
Loại bỏ trùng lặp khi load đơn vị tính từ chính sách giá (`conditions`) của Nhà cung cấp - Sản phẩm.

* **File:** `angular/src/app/orders/purchaseorders/purchaseorder-details/purchaseorder-details.component.ts`
* **Changes in `onMedicineChange(medicineId)`:**
  Sử dụng `Map` để gom nhóm theo `unitId` và giữ lại duy nhất một tùy chọn cho mỗi đơn vị tính.

```typescript
const uniqueUnitsMap = new Map<string, ProductUnitLookup>();
sp.conditions.forEach(c => {
  if (!uniqueUnitsMap.has(c.unitId)) {
    uniqueUnitsMap.set(c.unitId, {
      unitId: c.unitId,
      unitName: c.unitName,
      conversionFactor: c.conversionFactor || 1,
      isBaseUnit: c.unitId === sp.defaultUnitId,
    });
  }
});
this.units = Array.from(uniqueUnitsMap.values());
```

### 3.2. Tiered Pricing Logic (updatePriceTier)
Xây dựng logic tự động dò tìm đơn giá thỏa thuận phù hợp nhất với số lượng đặt hàng (`quantity`) hiện tại.

* **File:** `angular/src/app/orders/purchaseorders/purchaseorder-details/purchaseorder-details.component.ts`
* **New Method `updatePriceTier()`:**
  * Lọc toàn bộ mốc giá bậc thang của đơn vị tính đang chọn.
  * Sắp xếp các mốc giá theo thứ tự MQT (MOQ) giảm dần.
  * Tìm mốc đầu tiên thỏa mãn điều kiện `quantity >= minOrderQuantity`.
  * Nếu không thỏa mãn bất kỳ mốc nào (số lượng quá nhỏ), fallback về mốc MOQ nhỏ nhất.

```typescript
updatePriceTier() {
  const qty = this.detailForm.get('quantity')?.value || 0;
  const unitId = this.detailForm.get('unitId')?.value;
  
  if (this.isAutoFilled && this.activeConditions.length > 0 && unitId) {
    const unitConditions = this.activeConditions.filter(c => c.unitId === unitId);
    
    if (unitConditions.length > 0) {
      const matchedCond = unitConditions
        .sort((a, b) => (b.minOrderQuantity || 0) - (a.minOrderQuantity || 0))
        .find(c => qty >= (c.minOrderQuantity || 0));
      
      if (matchedCond) {
        const unitPrice = matchedCond.standardPrice || matchedCond.lastPurchasePrice || 0;
        this.detailForm.patchValue({ unitPrice: unitPrice }, { emitEvent: false });
      } else {
        const minCond = unitConditions.sort((a, b) => (a.minOrderQuantity || 0) - (b.minOrderQuantity || 0))[0];
        if (minCond) {
          const unitPrice = minCond.standardPrice || minCond.lastPurchasePrice || 0;
          this.detailForm.patchValue({ unitPrice: unitPrice }, { emitEvent: false });
        }
      }
    }
  }
}
```

### 3.3. Form Value Changes Subscription (Reactive Auto-pricing)
Sử dụng Angular Reactive Subscription để tự động tính toán lại đơn giá mỗi khi `quantity` hoặc `unitId` thay đổi.

* **File:** `angular/src/app/orders/purchaseorders/purchaseorder-details/purchaseorder-details.component.ts`
* **Changes in `buildForms()`:**
  Đăng ký lắng nghe Reactive thay đổi giá trị.

```typescript
this.detailForm.get('quantity')?.valueChanges.subscribe(() => {
  this.updateQuantityPreview();
  this.updatePriceTier();
});

this.detailForm.get('unitId')?.valueChanges.subscribe(() => {
  this.updatePriceTier();
});
```

---

## 4. Implementation Checklist

- [ ] **Step 1:** Cập nhật Entity `InventoryTicketLine.cs` để thêm `BaseQuantity` và tự động tính toán.
- [ ] **Step 2:** Cập nhật DTO `InventoryTicketLineDto.cs` và cấu hình EF Core trong `SupplyCoreERPDbContext.cs`.
- [ ] **Step 3:** Tạo mới migration `AddBaseQuantityToTicketLine` và áp dụng vào DB.
- [ ] **Step 4:** Cập nhật file Angular Component `purchaseorder-details.component.ts` để lọc trùng dropdown đơn vị.
- [ ] **Step 5:** Triển khai phương thức `updatePriceTier()` và Reactive Forms `valueChanges` subscription trong Angular Component.
- [ ] **Step 6:** Thực hiện kiểm thử toàn bộ quy trình trên Frontend (Drawer Add Product).
