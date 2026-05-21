# Thiết Kế Hệ Thống Quy Đổi Đơn Vị Và Ràng Buộc Đơn Vị Gốc (Unit Conversion & Base Unit Constraint)

Tài liệu thiết kế kỹ thuật chi tiết nhằm tối ưu hóa thuật toán quy đổi đơn vị tính, thiết lập các cơ chế kiểm soát chặt chẽ ràng buộc đơn vị gốc (`BaseUnitId`) trên toàn hệ thống backend của SupplyCoreERP.

---

## 1. Giới thiệu & Bối cảnh (Introduction & Context)

Trong hệ thống ERP và Quản lý chuỗi cung ứng dược phẩm (SupplyCoreERP), tính toàn vẹn của số liệu tồn kho và lịch sử giao dịch là yếu tố sống còn. Hiện tại, hệ thống đang tồn tại hai rủi ro lớn về mặt kiến trúc:
1. **Thiếu kiểm soát thay đổi đơn vị gốc:** Sản phẩm (`Product`) cho phép cập nhật `BaseUnitId` trực tiếp mà không kiểm tra xem sản phẩm đó đã phát sinh số dư tồn kho (`InventoryBalance`), dòng phiếu kho (`InventoryTicketLine`), dòng đơn mua hàng (`PurchaseOrderLine`), hoặc dòng đơn bán hàng (`SalesOrderLine`) chưa. Điều này dẫn đến nguy cơ sai lệch ý nghĩa số liệu lịch sử cực kỳ nghiêm trọng.
2. **Quy đổi đơn vị phân tán (Inline Calculation):** Các phép toán nhân/chia quy đổi số lượng giữa đơn vị phụ (`ProductUnit`) và đơn vị gốc (`BaseUnit`) được thực hiện thủ công rải rác ở tầng Application và Domain, dẫn đến sự thiếu nhất quán trong việc xử lý phần thập phân và làm tròn số (`Rounding Policy`).

---

## 2. Mục tiêu (Objectives)

* **Bảo vệ tính toàn vẹn dữ liệu giao dịch:** Chặn hoàn toàn việc thay đổi `BaseUnitId` đối với bất kỳ sản phẩm nào đã phát sinh giao dịch hoặc số dư tồn kho trong DB (bất kể số dư hiện tại bằng 0 hay lớn hơn 0).
* **Độc bản đơn vị:** Đảm bảo `BaseUnitId` của sản phẩm không trùng với bất kỳ đơn vị quy đổi phụ nào đang hoạt động trong danh sách `Units`.
* **Quy chuẩn hóa thuật toán quy đổi:** Xây dựng một dịch vụ miền tập trung (`UnitConversionManager`) chịu trách nhiệm thực hiện toàn bộ các tính toán quy đổi đơn vị tính với độ chính xác cao và chính sách làm tròn thống nhất.

---

## 3. Ràng buộc Tầng Domain (Domain Layer Invariants & Validations)

### 3.1. Cải tiến Thực thể `Product.cs`
Bổ sung kiểm tra điều kiện ràng buộc trong phương thức cập nhật của [Product.cs](file:///D:/ProjectOwner/SupplyCoreERP/src/SupplyCoreERP.Domain/Products/Product.cs). Khi cập nhật thông tin sản phẩm, đơn vị gốc mới không được trùng với các đơn vị quy đổi phụ hiện có trong danh sách `Units`.

```csharp
public void UpdateInfo(string name, Guid categoryId, Guid manufacturerId, Guid baseUnitId)
{
    if (Units.Any(u => u.UnitId == baseUnitId))
    {
        throw new BusinessException("SupplyCoreERP:DuplicateBaseUnitInUnits", "Đơn vị gốc không được trùng với các đơn vị quy đổi phụ đang có.");
    }
    SetName(name);
    CategoryId = categoryId;
    ManufacturerId = manufacturerId;
    BaseUnitId = baseUnitId;
}
```

### 3.2. Bổ sung Xác thực Giao dịch trong `ProductManager.cs`
[ProductManager.cs](file:///D:/ProjectOwner/SupplyCoreERP/src/SupplyCoreERP.Domain/Products/ProductManager.cs) chịu trách nhiệm kiểm tra lịch sử giao dịch thực tế của sản phẩm trên cơ sở dữ liệu.

* **Các Repository liên quan:**
  * `IRepository<InventoryBalance, Guid>`
  * `IRepository<InventoryTicketLine, Guid>`
  * `IRepository<PurchaseOrderLine, Guid>`
  * `IRepository<SalesOrderLine, Guid>`

* **Phương thức kiểm tra:**
```csharp
public async Task ValidateBaseUnitChangeAsync(Product product, Guid newBaseUnitId)
{
    Check.NotNull(product, nameof(product));

    // Chỉ thực hiện truy vấn DB khi thực sự có sự thay đổi BaseUnitId
    if (product.BaseUnitId == newBaseUnitId)
    {
        return;
    }

    // 1. Kiểm tra bảng số dư tồn kho
    if (await _balanceRepo.AnyAsync(x => x.ProductId == product.Id))
    {
        throw new BusinessException("SupplyCoreERP:CannotChangeBaseUnitWithTransactions", "Không thể thay đổi đơn vị gốc vì sản phẩm đã phát sinh số dư tồn kho.");
    }

    // 2. Kiểm tra bảng dòng phiếu kho
    if (await _ticketLineRepo.AnyAsync(x => x.ProductId == product.Id))
    {
        throw new BusinessException("SupplyCoreERP:CannotChangeBaseUnitWithTransactions", "Không thể thay đổi đơn vị gốc vì sản phẩm đã phát sinh phiếu kho.");
    }

    // 3. Kiểm tra bảng dòng đơn mua hàng
    if (await _poLineRepo.AnyAsync(x => x.ProductId == product.Id))
    {
        throw new BusinessException("SupplyCoreERP:CannotChangeBaseUnitWithTransactions", "Không thể thay đổi đơn vị gốc vì sản phẩm đã phát sinh dòng đơn mua hàng.");
    }

    // 4. Kiểm tra bảng dòng đơn bán hàng
    if (await _soLineRepo.AnyAsync(x => x.ProductId == product.Id))
    {
        throw new BusinessException("SupplyCoreERP:CannotChangeBaseUnitWithTransactions", "Không thể thay đổi đơn vị gốc vì sản phẩm đã phát sinh dòng đơn bán hàng.");
    }
}
```

### 3.3. Tích hợp trong `MedicineManager.cs`
Khi cập nhật thông tin thuốc thông qua [MedicineManager.cs](file:///D:/ProjectOwner/SupplyCoreERP/src/SupplyCoreERP.Domain/Medicines/MedicineManager.cs), hệ thống sẽ gọi trực tiếp phương thức kiểm tra của `ProductManager` trước khi lưu thay đổi:

```csharp
public async Task UpdateAsync(
    Medicine medicine,
    string name,
    Guid categoryId,
    Guid manufacturerId,
    Guid baseUnitId,
    Guid dosageFormId,
    string regNumber,
    UsageRoute usageRoute,
    StorageCondition storageCondition,
    bool isPrescriptionDrug,
    DateTime? regValidFrom = null,
    DateTime? regValidTo = null,
    string? regNote = null)
{
    Check.NotNull(medicine, nameof(medicine));
    await ValidateForeignKeysAsync(categoryId, manufacturerId, baseUnitId, dosageFormId);

    // Kích hoạt kiểm soát BaseUnit
    await _productManager.ValidateBaseUnitChangeAsync(medicine, baseUnitId);

    medicine.UpdateInfo(name, categoryId, manufacturerId, baseUnitId);
    
    // ... logic cập nhật SĐK và Pharma Info ...
}
```

---

## 4. Dịch vụ Quy đổi Tập trung (Centralized Unit Conversion Domain Service)

Tạo mới dịch vụ miền chịu trách nhiệm tính toán tại đường dẫn: `src/SupplyCoreERP.Domain/Products/UnitConversionManager.cs`.

```csharp
using System;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Products;

public class UnitConversionManager : DomainService
{
    /// <summary>
    /// Quy đổi từ một đơn vị bất kỳ về số lượng của Đơn vị gốc (BaseUnit).
    /// Phép tính: BaseQuantity = Quantity × ConversionFactor
    /// </summary>
    public decimal ConvertToBaseQuantity(Product product, Guid sourceUnitId, decimal quantity)
    {
        Check.NotNull(product, nameof(product));

        if (sourceUnitId == Guid.Empty)
        {
            throw new BusinessException("SupplyCoreERP:InvalidUnitId", "Đơn vị tính nguồn không hợp lệ.");
        }

        if (sourceUnitId == product.BaseUnitId)
        {
            return quantity;
        }

        ProductUnit? productUnit = product.Units.FirstOrDefault(u => u.UnitId == sourceUnitId);
        if (productUnit == null)
        {
            throw new BusinessException(
                "SupplyCoreERP:UnitNotFound", 
                $"Đơn vị tính với Id '{sourceUnitId}' không thuộc danh sách quy đổi của sản phẩm '{product.Name}'."
            );
        }

        return quantity * productUnit.ConversionFactor;
    }

    /// <summary>
    /// Quy đổi từ số lượng Đơn vị gốc (BaseUnit) sang một đơn vị phụ đích.
    /// Phép tính: TargetQuantity = BaseQuantity ÷ ConversionFactor
    /// </summary>
    public decimal ConvertFromBaseQuantity(Product product, Guid targetUnitId, decimal baseQuantity, int decimals = 4)
    {
        Check.NotNull(product, nameof(product));

        if (targetUnitId == Guid.Empty)
        {
            throw new BusinessException("SupplyCoreERP:InvalidUnitId", "Đơn vị tính đích không hợp lệ.");
        }

        if (targetUnitId == product.BaseUnitId)
        {
            return baseQuantity;
        }

        ProductUnit? productUnit = product.Units.FirstOrDefault(u => u.UnitId == targetUnitId);
        if (productUnit == null)
        {
            throw new BusinessException(
                "SupplyCoreERP:UnitNotFound", 
                $"Đơn vị tính với Id '{targetUnitId}' không thuộc danh sách quy đổi của sản phẩm '{product.Name}'."
            );
        }

        decimal rawResult = baseQuantity / productUnit.ConversionFactor;

        // Sử dụng làm tròn toán học chuẩn thương mại (Away From Zero) để tránh mất mát dữ liệu số thập phân
        return Math.Round(rawResult, decimals, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Quy đổi chéo giữa hai đơn vị tính bất kỳ của sản phẩm.
    /// </summary>
    public decimal ConvertBetweenUnits(Product product, Guid sourceUnitId, Guid targetUnitId, decimal quantity, int decimals = 4)
    {
        Check.NotNull(product, nameof(product));

        decimal baseQty = ConvertToBaseQuantity(product, sourceUnitId, quantity);
        return ConvertFromBaseQuantity(product, targetUnitId, baseQty, decimals);
    }

    /// <summary>
    /// Lấy hệ số quy đổi (ConversionFactor) của một đơn vị so với Đơn vị gốc.
    /// </summary>
    public int GetConversionFactor(Product product, Guid unitId)
    {
        Check.NotNull(product, nameof(product));

        if (unitId == product.BaseUnitId)
        {
            return 1;
        }

        ProductUnit? productUnit = product.Units.FirstOrDefault(u => u.UnitId == unitId);
        if (productUnit == null)
        {
            throw new BusinessException(
                "SupplyCoreERP:UnitNotFound", 
                $"Đơn vị tính với Id '{unitId}' không thuộc danh sách quy đổi của sản phẩm '{product.Name}'."
            );
        }

        return productUnit.ConversionFactor;
    }
}
```

---

## 5. Tích hợp Hệ thống & Quản lý lỗi (System Integration & Error Codes)

### 5.1. Tích hợp mã nguồn
* **Tầng Domain:** `TicketManager.cs` sẽ được tiêm `UnitConversionManager` để quy đổi chéo số lượng chi tiết phiếu kho sang `BaseQuantity` trước khi cập nhật số lượng tồn kho khả dụng.
* **Tầng Application:** `InventoryTicketAppService.cs` sử dụng `UnitConversionManager` để thực hiện phép chia phân bổ tồn kho khả dụng ngược lại đơn vị hiển thị trên phiếu kho mà không sợ sai lệch làm tròn số.

### 5.2. Tiêu chuẩn hóa Mã lỗi
Các mã lỗi nghiệp vụ được cấu hình phục vụ dịch tự động (Localization):

* **`SupplyCoreERP:CannotChangeBaseUnitWithTransactions`**: Trả về mã lỗi 403 Forbidden khi người dùng thay đổi đơn vị gốc của sản phẩm đã phát sinh giao dịch tồn kho/mua bán.
* **`SupplyCoreERP:DuplicateBaseUnitInUnits`**: Trả về mã lỗi 400 Bad Request khi đơn vị gốc mới trùng với đơn vị phụ trong bảng quy đổi.
* **`SupplyCoreERP:UnitNotFound`**: Trả về mã lỗi 404 Not Found khi đơn vị tính yêu cầu quy đổi không tồn tại trong cấu hình sản phẩm.
* **`SupplyCoreERP:InvalidUnitId`**: Trả về mã lỗi 400 Bad Request khi truyền mã đơn vị rỗng.

---

## 6. Kế hoạch Kiểm thử & Xác thực (Verification & Testing Plan)

### 6.1. Kiểm thử Tích hợp (Integration Tests)
Viết các bài test trong dự án `EntityFrameworkCore.Tests` sử dụng DB SQLite in-memory:
* **Test Case 1:** Đổi thành công `BaseUnitId` của sản phẩm vừa tạo mới (chưa có bất kỳ dòng giao dịch hay số dư tồn kho nào).
* **Test Case 2:** Đổi `BaseUnitId` của sản phẩm đã có bản ghi tồn kho (`InventoryBalance`) -> Kiểm tra xem hệ thống có quăng lỗi `CannotChangeBaseUnitWithTransactions` hay không.
* **Test Case 3:** Đổi `BaseUnitId` của sản phẩm đã có phiếu kho (`InventoryTicketLine`) -> Đảm bảo chặn thành công.
* **Test Case 4:** Cập nhật `BaseUnitId` trùng với một `UnitId` phụ đang có trong danh sách đơn vị quy đổi -> Phải ném lỗi `DuplicateBaseUnitInUnits`.

### 6.2. Kiểm thử Đơn vị (Unit Tests)
Viết các bài test cho dịch vụ `UnitConversionManager` độc lập:
* **Test Case 1:** Quy đổi số lượng từ đơn vị gốc về chính nó -> Đảm bảo giá trị không thay đổi.
* **Test Case 2:** Quy đổi từ đơn vị phụ (ví dụ: Hộp x50) về đơn vị gốc -> Đảm bảo nhân chính xác.
* **Test Case 3:** Quy đổi từ đơn vị gốc ngược lại đơn vị phụ -> Kiểm tra tính chính xác của phép chia và cơ chế làm tròn `MidpointRounding.AwayFromZero` với số thập phân lẻ (ví dụ: 1/3, 2/3).
* **Test Case 4:** Quy đổi đơn vị không tồn tại trên cấu hình sản phẩm -> Đảm bảo quăng ngoại lệ `UnitNotFound`.
