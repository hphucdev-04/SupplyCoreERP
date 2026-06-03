# Đặc Tả Thiết Kế UI Angular — Phân Hệ PurchaseReturns & SalesRecalls

Tài liệu đặc tả thiết kế chi tiết (Spec) cho giao diện người dùng Angular của hai phân hệ **PurchaseReturns (Trả hàng mua)** và **SalesRecalls (Thu hồi hàng bán - Luật Dược)** trong dự án SupplyCoreERP. Thiết kế tuân thủ mô hình 2 cấp (List → Detail), sử dụng `app-drawer` có sẵn trong thư mục `shared` và đồng bộ 100% với hệ thống Design System toàn cục thông qua `styles.scss`.

---

## 1. Cấu Trúc Thư Mục & Đăng Ký Định Tuyến (Routing)

### 1.1. Cấu trúc thư mục đề xuất
Cả hai phân hệ sẽ được triển khai độc lập trong thư mục `angular/src/app/orders` để phản ánh đúng Bounded Context và thuận tiện phân quyền:

```text
angular/src/app/orders/
├── purchase-returns/
│   ├── purchase-returns.component.ts             # Danh sách phiếu trả hàng mua
│   ├── purchase-returns.component.html
│   ├── purchase-returns.component.scss           # Chỉ chứa custom style đặc thù (nếu có)
│   └── purchase-return-details/
│       ├── purchase-return-details.component.ts   # Chi tiết phiếu trả + Drawer Bottom chọn PO Line
│       ├── purchase-return-details.component.html
│       └── purchase-return-details.component.scss
└── sales-recalls/
    ├── sales-recalls.component.ts                 # Danh sách quyết định thu hồi
    ├── sales-recalls.component.html
    ├── sales-recalls.component.scss
    └── sales-recall-details/
        ├── sales-recall-details.component.ts      # Chi tiết quyết định + Drawer Bottom truy vết
        ├── sales-recall-details.component.html
        └── sales-recall-details.component.scss
```

### 1.2. Cấu hình Routing (`orders.routes.ts`)
Đăng ký các route mới trong [orders.routes.ts](file:///D:/ProjectOwner/SupplyCoreERP/angular/src/app/orders/orders.routes.ts) để quản lý Lazy Loading:

```typescript
import { Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';

export const ORDER_ROUTES: Routes = [
  // ... các route có sẵn (saleorders, purchaseorders, purchaserequisitions) ...

  // ── Purchase Returns (Trả hàng mua)
  {
    path: 'purchasereturns',
    loadComponent: () =>
      import('./purchase-returns/purchase-returns.component').then(m => m.PurchaseReturnsComponent),
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Procurement.PurchaseReturns' }
  },
  {
    path: 'purchasereturns/details/:id',
    loadComponent: () =>
      import('./purchase-returns/purchase-return-details/purchase-return-details.component').then(
        m => m.PurchaseReturnDetailsComponent,
      ),
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Procurement.PurchaseReturns' }
  },

  // ── Sales Recalls (Thu hồi hàng bán - Luật Dược)
  {
    path: 'salesrecalls',
    loadComponent: () =>
      import('./sales-recalls/sales-recalls.component').then(m => m.SalesRecallsComponent),
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Sales.SalesRecalls' }
  },
  {
    path: 'salesrecalls/details/:id',
    loadComponent: () =>
      import('./sales-recalls/sales-recall-details/sales-recall-details.component').then(
        m => m.SalesRecallDetailsComponent,
      ),
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Sales.SalesRecalls' }
  },
];
```

---

## 2. Đồng Bộ Hóa Phong Cách Thiết Kế (Global Design System)

Sử dụng trực tiếp các class được định nghĩa sẵn trong [styles.scss](file:///D:/ProjectOwner/SupplyCoreERP/angular/src/styles.scss) để đảm bảo tính đồng nhất 100%:

| Thành phần UI | Class tiện ích trong `styles.scss` | Mục đích sử dụng |
| :--- | :--- | :--- |
| **Tiêu đề trang** | `.ph-page-title` | Tiêu đề chính màn hình danh sách và chi tiết |
| **Tiêu đề phân khu** | `.ph-section-title` | Tiêu đề bảng dòng hàng, tiêu đề Drawer |
| **Nhãn trường nhập** | `.ph-label`, `.ph-label--required` | Nhãn form, tự động thêm dấu sao đỏ nếu bắt buộc |
| **Xem chi tiết** | `.ph-field-meta`, `.ph-field-value` | Hiển thị nhãn nhỏ in hoa và giá trị trường trên trang Chi tiết |
| **Liên kết** | `.ph-link` | Dùng cho mã phiếu, mã đơn hàng tham chiếu |
| **Màu chữ trạng thái** | `.ph-text-muted`, `.ph-text-danger`, `.ph-text-success` | Hiển thị trạng thái quá hạn, số tiền, thành công |
| **Nút chính (Primary)** | `.ph-btn-primary` | Nút Lưu, Thực thi, Duyệt (màu xanh lá Pharmacy `#00B37E`) |
| **Nút hủy / outline** | `.ph-btn-outline` | Nút Hủy, Quay lại, Quay về danh sách |
| **Nút nguy hiểm** | `.ph-btn-danger` | Nút Hủy phiếu, xóa dòng |

---

## 3. Thiết Kế Chi Tiết Phân Hệ PurchaseReturns (Trả Hàng Mua)

### 3.1. Màn hình danh sách (`PurchaseReturnsComponent`)
*   **Tiêu đề:** Sử dụng `.ph-page-title` hiển thị "Quản lý Trả Hàng Mua".
*   **Bảng dữ liệu:**
    *   `Mã phiếu` (`Code`): Sử dụng class `.ph-link`. Khi click sẽ chuyển hướng sang trang chi tiết của phiếu.
    *   `Nhà cung cấp`: Hiển thị tên nhà cung cấp.
    *   `Kho xuất trả`: Tên kho xuất hàng.
    *   `Ngày trả`: Định dạng ngày chuẩn.
    *   `Trạng thái`: Hiển thị dạng Tag với các màu sắc:
        *   `Draft`: Tag xám.
        *   `Approved`: Tag xanh dương.
        *   `Executed`: Tag xanh lá.
        *   `Cancelled`: Tag đỏ.
    *   `Tổng tiền`: Căn lề phải, định dạng tiền tệ đậm.
*   **Nút "Tạo phiếu" (`.ph-btn-primary`):** 
    *   Khi click, mở Drawer bên phải (`<app-drawer position="right" width="md" [isOpen]="isCreateDrawerOpen" title="Tạo Phiếu Trả Hàng Mua">`).
    *   **Form master:** Người dùng chọn Nhà cung cấp (`SupplierId` - Dropdown), chọn Kho xuất (`WarehouseId` - Dropdown), Ngày trả, Ghi chú.
    *   **Lưu:** Hệ thống gọi API `CreateAsync` ở Backend tạo phiếu nháp, đóng Drawer và chuyển hướng sang trang Chi tiết Phiếu vừa tạo.

### 3.2. Màn hình chi tiết (`PurchaseReturnDetailsComponent`)
*   **Master Info Card:**
    *   Bên trái: Hiển thị các trường bằng `.ph-field-meta` và `.ph-field-value` (Mã phiếu, Nhà cung cấp, Kho xuất, Ngày trả, Tổng tiền).
    *   Hiển thị **Đơn mua hàng gốc** (`PurchaseOrderCode`) dưới dạng `.ph-link`. Nhấp vào sẽ chuyển tiếp sang trang chi tiết của Đơn mua hàng gốc.
*   **Bảng dòng chi tiết hàng trả (`Lines`):**
    *   Hiển thị danh sách sản phẩm, số lô, số lượng trả, đơn giá trả, thành tiền và dòng đơn mua gốc liên kết.
    *   Nút hành động: 📥 **"Chọn dòng hàng mua"** (Chỉ hiển thị khi trạng thái là `Draft`).
*   **Khu vực "Chứng từ liên quan" (Related Tickets Widget):**
    *   Hiển thị danh sách các phiếu kho (`InventoryTicket` - Phiếu xuất kho trả nhà cung cấp) tự động sinh ra khi thực thi phiếu trả hàng.
    *   Thông tin: Số phiếu (link click xem chi tiết), Loại phiếu (`InventoryIssue`), Trạng thái (`Approved`), Ngày tạo.
*   **Nút hành động master:**
    *   `Approve`: Duyệt phiếu.
    *   `Execute`: Thực thi xuất kho thực tế & tự động kích hoạt Event giảm nợ nhà cung cấp.

### 3.3. Drawer Bottom chọn dòng hàng (`app-drawer position="bottom" height="lg"`)
*   Khi người dùng bấm **"Chọn dòng hàng mua"** từ màn hình chi tiết, hệ thống mở Drawer trượt từ dưới lên.
*   **API nguồn:** Gọi API lấy danh sách các dòng hàng đã nhận từ Đơn mua hàng gốc / các đơn mua hàng trước đó của Nhà cung cấp này mà chưa hoàn trả hết.
*   **Bảng hiển thị:**
    *   Cột Checkbox chọn dòng.
    *   Mã đơn mua gốc (`PurchaseOrderCode`).
    *   Sản phẩm & Số lô.
    *   Số lượng mua gốc & Số lượng đã trả trước đây.
    *   **Số lượng tối đa còn có thể trả** (`DeliveredQuantity` - `ReturnedQuantity`).
    *   **Số lượng trả lần này** (Trường Input, tự động validate không cho vượt quá Số lượng tối đa).
*   **Cơ chế "Chọn lần nào Done lần đó":** Người dùng chọn dòng, nhập số lượng trả, bấm **Lưu** $\rightarrow$ Hệ thống lưu vào database, đóng Drawer và cập nhật lại trang Chi tiết. Các dòng hàng đã trả đủ số lượng sẽ tự động ẩn khỏi danh sách trong Drawer Bottom ở các lần sau.

---

## 4. Thiết Kế Chi Tiết Phân Hệ SalesRecalls (Thu Hồi Hàng Bán - Luật Dược)

### 4.1. Màn hình danh sách (`SalesRecallsComponent`)
*   **Tiêu đề:** "Quản lý Quyết định Thu hồi thuốc" (Sử dụng `.ph-page-title`).
*   **Bảng dữ liệu:**
    *   `Mã phiếu` (`Code`): Link click xem chi tiết.
    *   `Số quyết định`: Số công văn ban hành của Cục quản lý Dược.
    *   `Sản phẩm & Số lô`: Tên thuốc lỗi và số lô cụ thể cần thu hồi khẩn cấp.
    *   `Mức độ thu hồi` (`RecallLevel`):
        *   🚨 **Mức độ 1 (Khẩn cấp):** Tag đỏ đậm nhấp nháy, viền đỏ, chữ trắng (Hạn 3 ngày).
        *   ⚠️ **Mức độ 2 (Nghiêm trọng):** Tag cam (Hạn 15 ngày).
        *   📋 **Mức độ 3 (Thông thường):** Tag vàng/xám (Hạn 30 ngày).
    *   `Hạn chót tuân thủ` (`Deadline`): Định dạng ngày giờ cụ thể.
    *   `Cảnh báo trễ hạn` (`IsOverdue`): Nếu quyết định chưa hoàn tất và đã quá `Deadline`, hiển thị nhãn **TRỄ HẠN TUÂN THỦ** nhấp nháy đỏ rực bằng `.ph-text-danger`.
    *   `Trạng thái`: `Draft`, `Active` (Đang thu hồi), `Completed` (Đã thu hồi xong).
*   **Nút "Tạo quyết định thu hồi" (`.ph-btn-primary`):** Mở Drawer bên phải nhập thông tin master (Sản phẩm, Số lô, Số quyết định, Mức độ thu hồi, Kho nhận). Bấm Lưu sẽ lưu thông tin Master nháp và chuyển sang trang Chi tiết.

### 4.2. Màn hình chi tiết (`SalesRecallDetailsComponent`)
*   **Master Info Card:**
    *   Hiển thị thông tin Sản phẩm, Số lô lỗi, Ngày quyết định, Mức độ thu hồi, Hạn chót `Deadline` và trạng thái `IsOverdue`.
    *   **Thanh tiến độ thu hồi thực tế (Progress Bar):**
        *   Hiển thị phần trăm trực quan: `Tổng số lượng thu hồi thực tế` / `Tổng số lượng đã phát hành ra thị trường`.
        *   Ví dụ: **Đã thu hồi thành công 85% (850 / 1,000 hộp)**.
*   **Bảng chi tiết các dòng thu hồi (`Lines`):**
    *   Hiển thị danh sách khách hàng (`CustomerId`), mã đơn bán hàng gốc (`SalesOrderId`), số lượng giao gốc, và số lượng thực tế đã thu hồi thành công.
    *   Nút hành động: 🔍 **"Truy vết & Gợi ý Khách hàng"** (Drawer Bottom).
*   **Khu vực "Chứng từ liên quan" (Related Tickets Widget):**
    *   Hiển thị danh sách các phiếu nhập kho thu hồi thuốc lỗi trả về (`InventoryReceipt`) được sinh ra tự động.
    *   Thông tin: Số phiếu (link click xem chi tiết), Loại phiếu (`InventoryReceipt`), Trạng thái (`Approved`), Ngày tạo.
*   **Nút hành động master:**
    *   `Execute` (Thực thi thu hồi): Hoàn tất quyết định thu hồi, tự động khóa lô hàng lỗi trong kho và giảm công nợ khách hàng mua phải lô lỗi.

### 4.3. Drawer Bottom truy vết khách hàng (`app-drawer position="bottom" height="lg"`)
*   Khi người dùng bấm **"Truy vết & Gợi ý Khách hàng"**, hệ thống mở Drawer trượt từ dưới lên.
*   **API nguồn:** Gọi API `TraceCustomersByBatchAsync(productId, batchId)` từ Backend.
*   **Bảng hiển thị:**
    *   Checkbox chọn Khách hàng cần thu hồi.
    *   Tên Khách hàng & Mã đơn bán hàng gốc.
    *   Số lượng thuốc lỗi đã giao thực tế cho khách hàng đó.
    *   **Số lượng đã thu hồi thực tế** (Trường Input, mặc định gợi ý bằng số lượng đã giao gốc, tự động validate khống chế không được vượt quá số lượng giao gốc).
*   **Xác nhận:** Bấm Lưu sẽ chèn các dòng được chọn vào danh sách chi tiết của quyết định thu hồi trên màn hình chính và tự động tính toán lại thanh tiến độ.

---

## 5. Danh Sách Kiểm Tra Spec (Spec Self-Review)

1.  **Placeholder scan:** 100% không chứa các nhãn tạm "TBD", "TODO", các trường dữ liệu và class CSS đều được ánh xạ rõ ràng từ `styles.scss`.
2.  **Tính nhất quán:** Cách đặt tên biến DTO, API và luồng cấu trúc 2 cấp hoàn toàn đồng bộ giữa hai phân hệ và nhất quán với phân hệ `PurchaseOrder` thực tế trong codebase.
3.  **Ranh giới Bounded Context:** Tách biệt rõ ràng Route, Module và phân quyền ABP Guard cho từng phân hệ, phản ánh đúng đặc thù nghiệp vụ.
4.  **Độ tin cậy:** Khống chế kiểm soát định mức tối đa (Cumulative Limit Check) được cài đặt trực quan ngay trên Input của Drawer Bottom.
