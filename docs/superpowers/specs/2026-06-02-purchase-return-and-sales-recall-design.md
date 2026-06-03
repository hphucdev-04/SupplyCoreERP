# Đặc Tả Thiết Kế Hệ Thống & Giao Diện UI Angular — PurchaseReturns & SalesRecalls

Tài liệu đặc tả thiết kế chi tiết (Spec) tích hợp tái cấu trúc (refactor) Backend và xây dựng giao diện Angular cho hai phân hệ **PurchaseReturns (Trả hàng mua)** và **SalesRecalls (Thu hồi hàng bán - Luật Dược)** trong hệ thống SupplyCoreERP. 

Thiết kế này đảm bảo giải quyết trọn vẹn kịch bản **gom nhiều PO của cùng một Nhà cung cấp** vào chung một phiếu xuất kho vật lý ở Master, đồng thời bảo toàn 100% tính nguồn gốc chứng từ và kiểm soát tài chính ở cấp dòng hàng.

---

## 1. PHẦN I: TÁI CẤU TRÚC BACKEND (REFACTORING ARCHITECTURE)

### 1.1. Thực thể `PurchaseReturn` (Aggregate Root - Master)
*   **Thay đổi cấu trúc:** Loại bỏ thuộc tính `PurchaseOrderId` ở cấp Master (bảng `PurchaseReturn`) để giải phóng liên kết 1-1 ở cấp độ phiếu, cho phép một phiếu trả hàng gom được nhiều PO của cùng một nhà cung cấp.
*   **Thêm thuộc tính mới:**
    *   `ReturnType` (Enum): Phân loại loại hình trả hàng.
        *   `Defective = 1` (Bể vỡ / Lỗi do Nhà cung cấp - Không tính khấu hao).
        *   `Commercial = 2` (Trả hàng thương mại / Đổi date - Khấu hao tự do).
*   **Sơ đồ Db Mapping mới:**
    ```text
    [PurchaseReturn]
    ├── Id (Guid - PK)
    ├── Code (String - Unique)
    ├── SupplierId (Guid - FK to Supplier)
    ├── WarehouseId (Guid - FK to Warehouse)
    ├── ReturnType (Enum - Defective / Commercial)
    ├── ReturnDate (DateTime)
    ├── Status (Enum - Draft / PendingApproval / Approved / Returning / Completed / Rejected)
    ├── SubTotal / TaxAmount / TotalAmount (Decimal)
    └── Note (String?)
    ```

### 1.2. Thực thể `PurchaseReturnLine` (Detail Lines)
*   **Thêm thuộc tính mới:**
    *   `PurchaseOrderId` (Guid): Liên kết tới Đơn mua hàng gốc của dòng này.
    *   `PurchaseOrderLineId` (Guid): Liên kết tới dòng PO gốc cụ thể để đối chiếu giá và kiểm soát số lượng.
*   **Sơ đồ Db Mapping mới:**
    ```text
    [PurchaseReturnLine]
    ├── Id (Guid - PK)
    ├── PurchaseReturnId (Guid - FK to PurchaseReturn)
    ├── PurchaseOrderId (Guid - FK to PurchaseOrder)
    ├── PurchaseOrderLineId (Guid - FK to PurchaseOrderLine)
    ├── ProductId (Guid - FK to Product)
    ├── UnitId (Guid - FK to Unit)
    ├── ConversionFactor (Int)
    ├── Quantity / BaseQuantity (Decimal)
    ├── OriginalUnitPrice (Decimal - Giá mua gốc)
    ├── DepreciationRate (Decimal - Tỷ lệ khấu hao)
    ├── ReturnUnitPrice (Decimal - Đơn giá trả sau khấu hao)
    ├── TaxRate / TaxAmount / TotalPrice / FinalPrice (Decimal)
    ```

### 1.3. Ràng buộc nghiệp vụ ở Tầng Domain (`PurchaseReturnManager.cs`)
*   **Ràng buộc Khấu hao theo Loại trả hàng:**
    *   Nếu `ReturnType` = `Defective` $\rightarrow$ Tỷ lệ khấu hao `DepreciationRate` bắt buộc phải truyền vào là `0`. Nếu truyền khác `0` $\rightarrow$ Báo lỗi `BusinessException`: *"Hàng lỗi bể vỡ do nhà cung cấp không được phép tính khấu hao!"*.
    *   Nếu `ReturnType` = `Commercial` $\rightarrow$ Cho phép truyền `DepreciationRate` tự do (từ 0% đến 100%) thỏa thuận.
*   **Ràng buộc số lượng (Cumulative Limit Check):**
    *   Tổng số lượng xuất trả (`Quantity`) của dòng PO gốc này (`PurchaseOrderLineId`) trên toàn hệ thống không được vượt quá số lượng đã nhận kho thực tế (`ReceivedQuantity`) của dòng PO đó.
*   **Hạch toán cấn trừ công nợ (Event Handler):**
    *   Khi phiếu chuyển trạng thái `Completed`, hệ thống sẽ duyệt qua từng dòng hàng chi tiết `PurchaseReturnLine` để lấy `PurchaseOrderId` và số tiền `FinalPrice` tương ứng để **giảm trừ công nợ chính xác cho đúng đơn PO đó**.

---

## 2. PHẦN II: THIẾT KẾ CHI TIẾT GIAO DIỆN ANGULAR (FRONTEND)

### 2.1. Cấu Trúc Thư Mục & Đăng Ký Định Tuyến (Routing)
Cấu trúc thư mục độc lập dưới `angular/src/app/orders`:
```text
angular/src/app/orders/
├── purchase-returns/
│   ├── purchase-returns.component.ts         # Danh sách phiếu trả hàng mua
│   ├── purchase-returns.component.html
│   └── purchase-return-details/
│       ├── purchase-return-details.component.ts   # Chi tiết phiếu trả + Drawer Bottom chọn PO Line
│       └── purchase-return-details.component.html
└── sales-recalls/
    ├── sales-recalls.component.ts             # Danh sách quyết định thu hồi
    └── sales-recall-details/
        ├── sales-recall-details.component.ts      # Chi tiết quyết định + Drawer Bottom truy vết
        └── sales-recall-details.component.html
```

---

## 3. PHÂN HỆ PURCHASERETURNS (TRẢ HÀNG MUA - THƯƠNG MẠI & BỂ VỠ)

### 3.1. Màn hình danh sách (`PurchaseReturnsComponent`)
*   **Bảng dữ liệu:** Mã phiếu (link click), Nhà cung cấp, Kho xuất trả, Loại trả hàng (Tag xanh dương cho Thương mại, Tag đỏ cho Bể vỡ), Ngày trả, Tổng tiền, Trạng thái.
*   **Nút "Tạo phiếu" (Drawer bên phải - `<app-drawer position="right" width="md">`):**
    *   **Thứ tự các trường:**
        1.  Chọn **Kho xuất trả (`WarehouseId`)** $\rightarrow$ Bắt buộc chọn đầu tiên (Kho thủ kho chịu trách nhiệm).
        2.  Chọn **Nhà cung cấp (`SupplierId`)** $\rightarrow$ Bắt buộc chọn tiếp theo.
        3.  Chọn **Loại trả hàng (`ReturnType`)** $\rightarrow$ Dropdown chọn `Bể vỡ / Lỗi do Nhà cung cấp (Defective)` hoặc `Trả hàng thương mại (Commercial)`.
        4.  Ngày trả (`ReturnDate`), Ghi chú (`Note`).
    *   Bấm Lưu $\rightarrow$ Hệ thống gọi API `CreateAsync` ở Backend tạo phiếu nháp, đóng Drawer và chuyển hướng sang trang Chi tiết Phiếu vừa tạo.

### 3.2. Màn hình chi tiết (`PurchaseReturnDetailsComponent`)
*   **Master Info Card:** Hiển thị Kho, Nhà cung cấp, Loại trả hàng, Ngày trả, Tổng tiền.
*   **Bảng dòng chi tiết hàng trả (`Lines`):** hiển thị danh sách sản phẩm, số lô, số lượng trả, đơn giá trả, thành tiền và **Số PO gốc tham chiếu của từng dòng**.
*   **Chứng từ liên quan Widget:** Hiển thị danh sách các phiếu xuất kho (`InventoryTicket`) tự động sinh ra khi thực thi.

### 3.3. Drawer Bottom chọn dòng hàng (`app-drawer position="bottom" height="lg"`)
*   Khi người dùng bấm **"Chọn dòng hàng mua"**:
    *   Hệ thống gọi API load tất cả các dòng hàng đã nhận (`PurchaseOrderLine`) của **Nhà cung cấp** tại **Kho xuất** được chọn (Lọc song song theo 2 điều kiện này).
    *   **Bảng hiển thị:**
        *   Cột Checkbox chọn dòng.
        *   **Mã đơn mua hàng gốc (`PurchaseOrderCode`)** $\rightarrow$ Hiển thị rõ để thủ kho biết dòng hàng này thuộc PO nào.
        *   Sản phẩm & Số lô.
        *   Số lượng đã nhận gốc (`ReceivedQuantity`).
        *   Đơn giá mua gốc (`UnitPrice`).
        *   **Số lượng xuất trả lần này (Input):** Người dùng nhập số lượng trả.
        *   **Tỷ lệ khấu hao (`DepreciationRate` - Input):**
            *   *Nếu `ReturnType` của phiếu là `Defective` (Bể vỡ):* Ô nhập này sẽ bị **Khóa cứng (Disabled)** và tự động set bằng `0`. Người dùng không thể sửa đổi, đảm bảo tính chặt chẽ.
            *   *Nếu `ReturnType` của phiếu là `Commercial` (Thương mại):* Ô nhập này được **Mở khóa (Enabled)** để người dùng tự do nhập tỷ lệ khấu hao thỏa thuận thực tế.
    *   Bấm Lưu $\rightarrow$ Gọi API `addLine` song song qua `forkJoin` để lưu tất cả dòng được chọn, reload lại trang chi tiết.

---

## 4. PHÂN HỆ SALESRECALLS (THU HỒI HÀNG BÁN - LUẬT DƯỢC)

### 4.1. Màn hình danh sách (`SalesRecallsComponent`)
*   **Bảng dữ liệu:** Mã phiếu (link), Số quyết định, Sản phẩm & Số lô, Mức độ thu hồi (Level 1: Đỏ, Level 2: Cam, Level 3: Vàng), Hạn chót tuân thủ (`Deadline`), Cảnh báo trễ hạn (`IsOverdue` nhấp nháy đỏ rực), Trạng thái.
*   **Nút "Tạo quyết định" (Drawer phải):** Nhập thông tin Quyết định thu hồi (Sản phẩm, Số lô, Số quyết định, Mức độ thu hồi, Kho nhận).

### 4.2. Màn hình chi tiết (`SalesRecallDetailsComponent`)
*   **Master Info Card & Cảnh báo:** Hiển thị viền màu theo mức độ khẩn cấp, đếm ngược hạn chót và cảnh báo trễ hạn.
*   **Realtime Progress Bar:** Hiển thị tỷ lệ % phần trăm thu hồi thực tế: `Đã thu hồi / Tổng số lượng thuốc lỗi đã phát hành ra thị trường`.
*   **Drawer Bottom Truy vết:** Gọi API `TraceCustomersByBatchAsync` load tất cả các đơn bán hàng lịch sử đã bán lô thuốc lỗi này.
    *   Hiển thị danh sách khách hàng, mã đơn bán, số lượng đã giao gốc.
    *   Thủ kho tick chọn khách hàng, nhập số lượng thực tế đã thu hồi thành công $\rightarrow$ Lưu hàng loạt để đưa vào phiếu thu hồi và cập nhật thanh tiến độ thực tế.

---

## 5. ĐỒNG BỘ HÓA STYLE (GLOBAL DESIGN SYSTEM)

Sử dụng trực tiếp các class trong [styles.scss](file:///D:/ProjectOwner/SupplyCoreERP/angular/src/styles.scss) để đồng bộ 100% giao diện:
*   Tiêu đề trang: `.ph-page-title`
*   Nhãn trường: `.ph-label`, `.ph-label--required` (sao đỏ)
*   Nút bấm: `.ph-btn-primary`, `.ph-btn-outline`, `.ph-btn-danger`
*   Xem chi tiết: `.ph-field-meta`, `.ph-field-value`
*   Màu chữ: `.ph-text-danger` (Cảnh báo quá hạn), `.ph-text-primary` (Tổng tiền)
