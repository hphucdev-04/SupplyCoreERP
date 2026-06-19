# Tài liệu Thiết kế: Tái thiết kế Tồn kho, Giữ hàng và Lịch sử Giao dịch

- **Ngày tạo:** 15/06/2026
- **Tác giả:** Antigravity AI Assistant
- **Dự án:** SupplyCoreERP
- **Công nghệ:** ABP Framework (.NET 10.0), PostgreSQL (Neon Cloud), Angular (Route-based)

---

## 1. Mục tiêu và Phạm vi

### Mục tiêu:
1.  **Dọn dẹp tồn kho bằng 0:** Tự động xóa thực thể [InventoryBalance](file:///D:/ProjectOwner/SupplyCoreERP/src/SupplyCoreERP.Domain/Inventory/Balances/InventoryBalance.cs) khi cả số lượng thực tế (`Quantity`) và số lượng bị khóa (`LockedQuantity`) giảm về 0, giải phóng dung lượng DB PostgreSQL và tối ưu hiển thị.
2.  **Trực quan hóa lượng hàng bị khóa (Lock Stock):** Cập nhật thực thể [InventoryReservation](file:///D:/ProjectOwner/SupplyCoreERP/src/SupplyCoreERP.Domain/Inventory/Balances/InventoryReservation.cs) để lưu trữ trực tiếp thông tin Đối tác (Khách hàng) và Đơn hàng gốc (Sales Order, v.v.). Cho phép xem chi tiết ai đang giữ số lượng hàng bị khóa tại cấp độ từng dòng tồn kho.
3.  **Lịch sử giao dịch chi tiết (Inventory Ledger):** Tái thiết kế [InventoryTransaction](file:///D:/ProjectOwner/SupplyCoreERP/src/SupplyCoreERP.Domain/Inventory/Transactions/InventoryTransaction.cs) để lưu vết snapshot bất biến bao gồm: Đối tác (Partner) thực hiện giao dịch, Chứng từ gốc (PO, SO, v.v.) và Phiếu kho trực tiếp (Ticket).
4.  **Tái thiết kế Frontend (Angular):**
    *   Chuyển đổi giao diện Chi tiết tồn kho (`BalanceDetails`) từ Modal dạng popup thành một **Trang (Page) với Route riêng biệt** để đồng bộ UI với các module khác (như `SupplierDetails`).
    *   Tích hợp Tab cấp trang trên màn hình Tồn kho chính để gộp chung danh sách tồn kho hiện tại và nhật ký giao dịch toàn kho.
    *   Tuân thủ nghiêm ngặt quy tắc sử dụng style: Ưu tiên tối đa các class global trong `styles.scss` toàn cục, chỉ cấu hình thêm css đặc thù trong file SCSS của component khi thực sự cần thiết.

---

## 2. Thiết kế Kiến trúc Backend (Domain & DB)

### 2.1. Thay đổi Thực thể (Entities)

#### [InventoryReservation.cs](file:///D:/ProjectOwner/SupplyCoreERP/src/SupplyCoreERP.Domain/Inventory/Balances/InventoryReservation.cs)
Bổ sung các trường thông tin snapshot của Đối tác và Chứng từ gốc:
```csharp
public Guid? PartnerId { get; private set; }
public string? PartnerName { get; private set; }

public Guid? SourceDocumentId { get; private set; }
public string? SourceDocumentNumber { get; private set; }
```

#### [InventoryTransaction.cs](file:///D:/ProjectOwner/SupplyCoreERP/src/SupplyCoreERP.Domain/Inventory/Transactions/InventoryTransaction.cs)
Bổ sung các trường thông tin snapshot tương tự:
```csharp
public Guid? PartnerId { get; private set; }
public string? PartnerName { get; private set; }

public Guid? SourceDocumentId { get; private set; }
public string? SourceDocumentNumber { get; private set; }
```

### 2.2. Logic Nghiệp vụ (Domain Services)

#### [InventoryBalanceManager.cs](file:///D:/ProjectOwner/SupplyCoreERP/src/SupplyCoreERP.Domain/Inventory/Balances/InventoryBalanceManager.cs)

1.  **Dọn dẹp tồn kho = 0:**
    Trong phương thức `ExecuteStockMovementAsync`, sau khi thực hiện trừ kho:
    ```csharp
    balance.RemoveStock(item.BaseQuantity);

    if (balance.Quantity == 0 && balance.LockedQuantity == 0)
    {
        balancesToDelete.Add(balance);
        balancesToUpdate.Remove(balance);
    }
    ```
    Cuối phương thức, thực hiện xóa hàng loạt:
    ```csharp
    if (balancesToDelete.Any())
    {
        await _balanceRepo.DeleteManyAsync(balancesToDelete);
    }
    ```

2.  **Lan truyền thông tin Snapshot khi giữ hàng và tạo giao dịch:**
    *   Khi `LockStockAsync` chạy, copy `PartnerId`, `PartnerName`, `SourceDocumentId`, `SourceDocumentNumber` từ `InventoryTicket` sang `InventoryReservation`.
    *   Khi `ExecuteStockMovementAsync` chạy, copy các trường tương tự từ `InventoryTicket` sang `InventoryTransaction`.

---

## 3. Thiết kế Frontend (Angular)

### 3.1. Routing (`inventories.routes.ts`)
Đăng ký route chi tiết tồn kho mới dạng Page:
```typescript
{
  path: 'balances/details/:id',
  loadComponent: () =>
    import('./balances/balance-details/balance-details.component').then(
      m => m.BalanceDetailsComponent,
    ),
}
```

### 3.2. Cấu trúc Component Chi tiết Tồn kho (`BalanceDetailsComponent`)

*   **TypeScript (`balance-details.component.ts`):**
    *   Kế thừa `OnInit`, `OnDestroy`.
    *   Sử dụng `ActivatedRoute` để lấy `id` từ url parameter trong `ngOnInit()`.
    *   Tích hợp `RoutesService` để cập nhật Breadcrumb động của ABP Layout.
    *   Cung cấp phương thức `goBack()` điều hướng về `/inventories/balances`.

*   **HTML (`balance-details.component.html`):**
    *   Sử dụng cấu trúc bố cục trang tiêu chuẩn, bỏ hoàn toàn các thẻ modal.
    *   Gồm nút Quay lại (`goBack()`), Page Header, và cụm Tab điều hướng ("Tổng quan" & "Lịch sử biến động").
    *   **Tab Tổng quan:** Hiển thị thông số tồn kho, thông tin lô hàng, vị trí lưu trữ và bảng chi tiết giữ hàng **Locked Reservations** (chỉ hiển thị khi `detail.lockedQuantity > 0`).
    *   **Tab Lịch sử biến động:** Nhúng component `<app-transactions [isEmbedded]="true">` đã được lọc theo sản phẩm, lô và vị trí kệ hiện tại.

### 3.3. Cấu trúc Trang danh sách chính (`BalancesComponent`)

*   **HTML (`balances.component.html`):**
    *   Sắp xếp 2 Tab lớn cấp trang: "Số dư tồn kho hiện tại" và "Nhật ký giao dịch toàn kho".
    *   Tab 1 hiển thị bộ lọc và bảng Grid số dư tồn kho (nhấp nút **View** sẽ điều hướng sang route chi tiết).
    *   Tab 2 hiển thị toàn bộ lịch sử giao dịch bằng cách nhúng `<app-transactions [isEmbedded]="false">`.

### 3.4. Component Lịch sử Giao dịch (`TransactionsComponent`)
*   Cải tiến cột **Document** để hiển thị đồng thời Số phiếu kho và Số đơn hàng gốc (PO/SO) ngay bên dưới.
*   Bổ sung cột **Đối tác** để hiển thị tên Nhà cung cấp hoặc Khách hàng trực tiếp trên lưới dữ liệu.

### 3.5. Quy tắc Styling
*   Ưu tiên sử dụng các class CSS/SCSS đã có sẵn trong `styles.scss` global của dự án để đảm bảo tính nhất quán giao diện (ví dụ các class `.ph-card`, `.ph-badge`, `.ph-filter-bar`, v.v.).
*   Không viết lại các thuộc tính CSS cơ bản trong component nếu global đã hỗ trợ. Chỉ viết code styling đặc thù trong file SCSS cục bộ của component khi cần tùy biến layout riêng biệt của trang chi tiết tồn kho.

---

## 4. Kế hoạch Kiểm thử (Testing)

1.  **Kiểm thử đơn vị & Tích hợp (Backend):**
    *   Verify phương thức xuất kho làm giảm tồn kho về 0: kiểm tra bản ghi `InventoryBalance` tương ứng phải biến mất khỏi cơ sở dữ liệu.
    *   Verify việc điền dữ liệu snapshot: kiểm tra các thuộc tính `PartnerName` và `SourceDocumentNumber` trên `InventoryReservation` và `InventoryTransaction` được lưu trữ chính xác sau khi tạo phiếu.
2.  **Kiểm thử giao diện (Frontend):**
    *   Verify điều hướng: click nút View trên bảng grid tồn kho phải chuyển hướng URL sang `/inventories/balances/details/{id}` và breadcrumb cập nhật đúng.
    *   Verify thông tin giữ hàng: khi mở chi tiết dòng tồn kho có `lockedQuantity > 0`, bảng locked reservations phải hiện đầy đủ thông tin khách hàng và số SO liên quan.
    *   Verify tab cấp trang: chuyển đổi tab chính ở màn hình tồn kho hoạt động mượt mà và tải dữ liệu chính xác.
