# Đặc Tả Thiết Kế Hệ Thống & Giao Diện UI Angular — PurchaseReturnRequest & SalesRecalls

Tài liệu đặc tả thiết kế chi tiết (Spec) tích hợp thực thể mới **PurchaseReturnRequest (Yêu cầu trả hàng mua)** cấp cao, cơ chế tự động **Grouping & Splitting** (Gom nhóm & Tách phiếu) khi duyệt, và xây dựng giao diện Angular cho các phân hệ **PurchaseReturnRequest** và **SalesRecalls (Thu hồi hàng bán - Luật Dược)** trong hệ thống SupplyCoreERP.

---

## 1. PHẦN I: THIẾT KẾ KIẾN TRÚC HỆ THỐNG BACKEND (REFACTOR)

Để giải quyết bài toán gom nhiều PO của cùng một Supplier trong 1 phiếu vận chuyển/xuất kho vật lý mà vẫn bảo toàn 100% tính nguồn gốc 1-1 của đợt nhập hàng phục vụ kế toán cấn trừ nợ và kho, hệ thống bổ sung thực thể cấp cao **`PurchaseReturnRequest`**.

### 1.1. Thực thể `PurchaseReturnRequest` (Yêu cầu trả hàng - Cấp Master)
*   **Mục đích:** Đóng vai trò là "giỏ gom" yêu cầu trả hàng của cùng một nhà cung cấp từ một kho vật lý, đồng thời quản lý quy trình phê duyệt nội bộ.
*   **Sơ đồ Db Mapping:**
    ```text
    [PurchaseReturnRequest]
    ├── Id (Guid - PK)
    ├── Code (String - Unique) — Mã tự sinh PRQ-YYYYMMDD-XXXX
    ├── SupplierId (Guid - FK to Supplier)
    ├── WarehouseId (Guid - FK to Warehouse)
    ├── ReturnType (Enum - Defective / Commercial)
    ├── RequestDate (DateTime)
    ├── Status (Enum - Draft / PendingApproval / Approved / Rejected / Processed)
    └── Note (String?)
    ```

### 1.2. Thực thể `PurchaseReturnRequestLine` (Cấp Detail Line)
*   **Mục đích:** Lưu trữ chi tiết từng mặt hàng yêu cầu trả và liên kết nguồn gốc của nó.
*   **Sơ đồ Db Mapping:**
    ```text
    [PurchaseReturnRequestLine]
    ├── Id (Guid - PK)
    ├── PurchaseReturnRequestId (Guid - FK to PurchaseReturnRequest)
    ├── ProductId (Guid - FK to Product)
    ├── UnitId (Guid - FK to Unit)
    ├── ConversionFactor (Int)
    ├── PurchaseOrderId (Guid - FK to PurchaseOrder)
    ├── PurchaseOrderLineId (Guid - FK to PurchaseOrderLine)
    ├── Quantity / BaseQuantity (Decimal)
    ├── OriginalUnitPrice (Decimal - Giá mua gốc)
    ├── DepreciationRate (Decimal - Tỷ lệ khấu hao)
    └── TaxRate (Decimal)
    ```

### 1.3. Ràng buộc nghiệp vụ ở Tầng Domain (`PurchaseReturnRequestManager.cs`)
*   **Ràng buộc Khấu hao theo Loại trả hàng:**
    *   Nếu `ReturnType` = `Defective` (Bể vỡ/Lỗi do NCC) $\rightarrow$ Tỷ lệ khấu hao `DepreciationRate` bắt buộc phải truyền vào là `0`. Nếu truyền khác `0` $\rightarrow$ Báo lỗi `BusinessException`: *"Hàng lỗi bể vỡ do nhà cung cấp không được phép tính khấu hao!"*.
    *   Nếu `ReturnType` = `Commercial` (Thương mại) $\rightarrow$ Cho phép truyền `DepreciationRate` tự do (từ 0% đến 100%) thỏa thuận.
*   **Ràng buộc số lượng (Cumulative Limit Check):**
    *   Tổng số lượng xuất trả (`Quantity`) của dòng PO gốc này (`PurchaseOrderLineId`) trên toàn hệ thống không được vượt quá số lượng đã nhận kho thực tế (`ReceivedQuantity`) của dòng PO đó.

### 1.4. Thuật toán Gom nhóm & Tách phiếu tự động khi Approved
Khi `PurchaseReturnRequest` được chuyển trạng thái sang **`Approved`**, Backend Domain Service tự động chạy logic Grouping & Splitting:
1.  **Group by `PurchaseOrderId`:** Duyệt qua danh sách `Lines` của phiếu yêu cầu để gom nhóm các dòng có cùng `PurchaseOrderId`.
2.  **Sinh đơn con (`PurchaseReturn`):** Với mỗi nhóm (tương ứng với 1 `PurchaseOrderId` cụ thể):
    *   Sinh 1 phiếu `PurchaseReturn` con liên kết 1-1 với `PurchaseOrderId` đó ở Master (sử dụng cấu trúc `PurchaseReturn` 1-1 hiện có của Backend).
    *   Với mỗi `PurchaseReturnRequestLine` thuộc nhóm, tạo tương ứng 1 `PurchaseReturnLine` con gắn với `PurchaseOrderLineId` gốc.
3.  **Cập nhật trạng thái:** Cập nhật trạng thái `PurchaseReturnRequest` mẹ sang `Processed` và ghi nhận liên kết chéo (Related Tickets).

---

## 2. PHẦN II: THIẾT KẾ CHI TIẾT GIAO DIỆN ANGULAR (FRONTEND)

### 2.1. Cấu trúc thư mục định tuyến mới
```text
angular/src/app/orders/
├── purchase-returns/
│   ├── purchase-returns.component.ts         # Danh sách Yêu cầu trả hàng (PRQ)
│   ├── purchase-returns.component.html
│   └── purchase-return-details/
│       ├── purchase-return-details.component.ts   # Chi tiết Yêu cầu (PRQ) + Drawer Bottom chọn PO Lines
│       └── purchase-return-details.component.html
└── sales-recalls/
    ├── sales-recalls.component.ts             # Danh sách Quyết định thu hồi
    └── sales-recall-details/
        ├── sales-recall-details.component.ts      # Chi tiết quyết định + Drawer Bottom truy vết
        └── sales-recall-details.component.html
```

### 2.2. Đăng ký Định Tuyến (`orders.routes.ts`)
```typescript
  // Purchase Returns Request (Yêu cầu trả hàng mua)
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
```

---

## 3. PHÂN HỆ PURCHASERETURNS (YÊU CẦU TRẢ HÀNG MUA - GOM PO)

### 3.1. Màn hình danh sách (`PurchaseReturnsComponent`)
*   **Bảng dữ liệu:** Mã phiếu yêu cầu (`PRQ-YYYYMMDD-XXXX` - link click), Nhà cung cấp, Kho xuất, Loại trả hàng (Tag Bể vỡ/Lỗi NCC hoặc Tag Thương mại), Ngày lập, Trạng thái (`Draft`, `PendingApproval`, `Approved`, `Rejected`, `Processed`).
*   **Nút "Tạo yêu cầu" (Drawer bên phải - `<app-drawer position="right" width="md">`):**
    *   **Thứ tự các trường:**
        1.  Chọn **Kho xuất trả (`WarehouseId`)** $\rightarrow$ Bắt buộc chọn đầu tiên.
        2.  Chọn **Nhà cung cấp (`SupplierId`)** $\rightarrow$ Bắt buộc chọn.
        3.  Chọn **Loại trả hàng (`ReturnType`)** $\rightarrow$ Dropdown chọn `Bể vỡ / Lỗi do Nhà cung cấp (Defective)` hoặc `Trả hàng thương mại (Commercial)`.
        4.  Ngày yêu cầu (`RequestDate`), Ghi chú (`Note`).
    *   Bấm Lưu $\rightarrow$ Hệ thống tạo Master Yêu cầu nháp, đóng Drawer và chuyển sang trang Chi tiết Yêu cầu.

### 3.2. Màn hình chi tiết (`PurchaseReturnDetailsComponent`)
*   **Master Info Card:** Hiển thị Kho, Nhà cung cấp, Loại trả hàng, Ngày lập, Trạng thái.
*   **Bảng dòng chi tiết yêu cầu (`Lines`):** hiển thị sản phẩm, số lượng yêu cầu trả, đơn giá mua gốc, tỷ lệ khấu hao, đơn giá trả sau khấu hao, thành tiền và **Số PO gốc của dòng hàng**.
*   **Nút hành động:** 📥 **"Chọn dòng hàng mua"** (Chỉ hiển thị khi ở trạng thái `Draft`).
*   **Chứng từ liên quan Widget:** 
    *   Hiển thị danh sách các phiếu xuất trả thực tế (`PurchaseReturn` con) và phiếu xuất kho (`InventoryTicket`) tự động sinh ra sau khi yêu cầu được duyệt và xử lý tách đơn.

### 3.3. Drawer Bottom chọn dòng hàng (`app-drawer position="bottom" height="lg"`)
*   Khi bấm **"Chọn dòng hàng mua"**:
    *   Hệ thống gọi API load tất cả các dòng hàng đã nhận (`PurchaseOrderLine`) của **Nhà cung cấp** tại **Kho xuất** được chọn.
    *   **Bảng hiển thị:**
        *   Cột Checkbox chọn dòng.
        *   **Mã đơn mua hàng gốc (`PurchaseOrderCode`)** $\rightarrow$ Hiển thị rõ để thủ kho biết dòng này thuộc PO nào.
        *   Sản phẩm & Số lô.
        *   Số lượng đã nhận gốc (`ReceivedQuantity`).
        *   Đơn giá mua gốc (`UnitPrice`).
        *   **Số lượng xuất trả lần này (Input):** Nhập số lượng trả.
        *   **Tỷ lệ khấu hao (`DepreciationRate` - Input):**
            *   *Nếu `ReturnType` là `Defective` (Bể vỡ):* Trường này bị **Khóa cứng (Disabled)** và tự động set bằng `0`.
            *   *Nếu `ReturnType` là `Commercial` (Thương mại):* Trường này được **Mở khóa (Enabled)** để người dùng tự do nhập.
    *   Bấm Lưu $\rightarrow$ Lưu hàng loạt dòng yêu cầu (`PurchaseReturnRequestLine`), reload lại trang chi tiết.

---

## 4. PHÂN HỆ SALESRECALLS (THU HỒI HÀNG BÁN - LUẬT DƯỢC)

### 4.1. Màn hình danh sách (`SalesRecallsComponent`)
*   **Bảng dữ liệu:** Mã phiếu (`RC-YYYYMMDD-XXXX` - link), Số quyết định, Sản phẩm & Số lô, Mức độ thu hồi (Level 1: Đỏ, Level 2: Cam, Level 3: Vàng), Hạn chót tuân thủ (`Deadline`), Cảnh báo trễ hạn (`IsOverdue` nhấp nháy đỏ rực), Trạng thái.
*   **Nút "Tạo quyết định" (Drawer phải):** Nhập thông tin Quyết định thu hồi (Sản phẩm, Số lô, Số quyết định, Mức độ thu hồi, Kho nhận).

### 4.2. Màn hình chi tiết (`SalesRecallDetailsComponent`)
*   **Master Info Card & Cảnh báo:** Hiển thị viền màu theo mức độ khẩn cấp, đếm ngược hạn chót và cảnh báo trễ hạn.
*   **Realtime Progress Bar:** Hiển thị tỷ lệ % phần trăm thu hồi thực tế: `Đã thu hồi / Tổng số lượng thuốc lỗi đã phát hành ra thị trường`.
*   **Drawer Bottom Truy vết:** Gọi API `TraceCustomersByBatchAsync` load tất cả các đơn bán hàng lịch sử đã bán lô thuốc lỗi này.
    *   Hiển thị danh sách khách hàng, mã đơn bán, số lượng đã giao gốc.
    *   Thủ kho tick chọn khách hàng, nhập số lượng thực tế đã thu hồi thành công $\rightarrow$ Lưu hàng loạt để đưa vào phiếu thu hồi và cập nhật thanh tiến độ thực tế.

---

## 5. DANH SÁCH KIỂM TRA SPEC (SPEC SELF-REVIEW)

1.  **Placeholder scan:** 100% không chứa các nhãn tạm "TBD", "TODO". Các enums và DTOs được ánh xạ rõ ràng.
2.  **Tính nhất quán:** Logic Grouping & Splitting được định nghĩa toán học rõ ràng, đảm bảo không sinh rác hệ thống.
3.  **Quy chuẩn ERP:** Quy trình "Yêu cầu (PRQ) $\rightarrow$ Phê duyệt $\rightarrow$ Thực thi (RO)" được thiết kế chặt chẽ và nhất quán với phân hệ PR/PO hiện tại của SupplyCoreERP.
