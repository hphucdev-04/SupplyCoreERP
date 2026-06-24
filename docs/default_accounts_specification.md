# Đặc tả Tài khoản và Phân quyền mặc định (Default Accounts & Roles Specification)

Tài liệu này xác định các vai trò (Roles), danh sách quyền hạn (Permissions) tương ứng và thông tin tài khoản mẫu (Default Accounts) để đưa vào hệ thống **SupplyCoreERP**.

Để đảm bảo nguyên tắc **Bất kiêm nhiệm (Segregation of Duties - SoD)** trong kiểm soát nội bộ ERP, hệ thống phân tách rõ ràng giữa nhóm **Nhân viên thực hiện (Maker)** và nhóm **Quản lý phê duyệt (Checker)** ở từng phân hệ.

---

## 1. Phân tách Vai trò và Quyền hạn chi tiết (Roles & Permissions Mapping)

### Phân hệ: Quản trị Hệ thống (System Admin)

*   **Role: Admin (Quản trị viên hệ thống)**
    *   **Mô tả:** Có toàn bộ quyền hạn trong hệ thống để quản trị kỹ thuật, phân quyền và cấu hình hệ thống.
    *   **Quyền hạn:** Toàn bộ quyền hệ thống (`*` hoặc tất cả các nhóm quyền dưới đây).

---

### Phân hệ: Quản lý Danh mục (Catalog)

*   **Role: CatalogStaff (Nhân viên Danh mục - Maker)**
    *   **Mô tả:** Nhập liệu và cập nhật thông tin sản phẩm, hoạt chất, quy cách đóng gói, nhà sản xuất.
    *   **Quyền hạn được gán:**
        *   `Catalog.Category` (View, Create, Update, Delete)
        *   `Catalog.BaseUnit` (View, Create, Update, Delete)
        *   `Catalog.DosageForm` (View, Create, Update, Delete)
        *   `Catalog.ActiveIngredient` (View, Create, Update, Delete)
        *   `Catalog.Manufacturer` (View, Create, Update, Delete)
        *   `Catalog.Medicine` (View, Create, Update, Delete) *(Không có quyền phê duyệt)*

*   **Role: CatalogManager (Quản lý Danh mục / Trưởng nhóm Danh mục - Checker)**
    *   **Mô tả:** Kiểm tra chất lượng thông tin, duyệt sản phẩm mới để đưa vào lưu thông hoặc sử dụng.
    *   **Quyền hạn được gán:**
        *   `Catalog.Medicine` (View, **Approve**, **Reject**)
        *   Quyền xem các danh mục phụ trợ khác (`View` của Category, BaseUnit, DosageForm, ActiveIngredient, Manufacturer).

---

### Phân hệ: Mua hàng và Cung ứng (Procurement)

*   **Role: PurchasingStaff (Nhân viên Mua hàng - Maker)**
    *   **Mô tả:** Tìm kiếm và cập nhật nhà cung cấp, lập yêu cầu mua hàng (PR), tạo đơn mua hàng (PO), tạo yêu cầu xuất trả hàng lỗi (PRR) hoặc chứng từ xuất trả (Purchase Return).
    *   **Quyền hạn được gán:**
        *   `Partner.Supplier` (View, Create, Update, Delete)
        *   `Order.PurchaseRequisition` (View, Create, Update, Delete) *(Không có quyền duyệt)*
        *   `Order.PurchaseOrder` (View, Create, Update, Delete) *(Không có quyền duyệt)*
        *   `Order.PurchaseReturnRequest` (View, Create, Update, Delete) *(Không có quyền duyệt)*
        *   `Order.PurchaseReturn` (View, Create, Update, Delete) *(Không có quyền duyệt)*

*   **Role: PurchasingManager (Trưởng phòng Mua hàng - Checker)**
    *   **Mô tả:** Phê duyệt đơn mua hàng PO, duyệt yêu cầu mua hàng PR và duyệt phiếu xuất trả hàng nhà cung cấp.
    *   **Quyền hạn được gán:**
        *   `Order.PurchaseRequisition` (View, **Approve**, **Reject**)
        *   `Order.PurchaseOrder` (View, **Approve**, **Reject**)
        *   `Order.PurchaseReturnRequest` (View, **Approve**, **Reject**)
        *   `Order.PurchaseReturn` (View, **Approve**, **Reject**)
        *   `Partner.Supplier` (View)

---

### Phân hệ: Kho vận (Inventory)

*   **Role: WarehouseStaff (Nhân viên Kho / Thủ kho - Maker)**
    *   **Mô tả:** Thực hiện nhận hàng, soạn hàng, dán nhãn lô sản phẩm, lập phiếu nhập kho/xuất kho vật lý trên phần mềm.
    *   **Quyền hạn được gán:**
        *   `Inventory.Batch` (View, Create, Update, Delete)
        *   `Inventory.Ticket` (View, Create, Update, Delete) *(Không có quyền duyệt)*
        *   `Inventory.Warehouse` (View)

*   **Role: WarehouseManager (Quản lý Kho / Trưởng kho - Checker)**
    *   **Mô tả:** Phê duyệt chứng từ nhập/xuất/kiểm kê kho thực tế để cập nhật số dư tồn kho, quản lý phân vùng kho và thực hiện điều chuyển vùng lưu trữ.
    *   **Quyền hạn được gán:**
        *   `Inventory.Ticket` (View, **Approve**, **Reject**)
        *   `Inventory.Warehouse` (View, Create, Update, Delete, **Approve**, **Reject**, **ZoneTransfer**)
        *   `Inventory.Batch` (View)

---

### Phân hệ: Bán hàng (Sales)

*   **Role: SalesStaff (Nhân viên Bán hàng - Maker)**
    *   **Mô tả:** Chăm sóc khách hàng, tiếp nhận đơn đặt hàng từ khách, lập đơn bán hàng (SO), lập yêu cầu thu hồi hàng bán do lỗi.
    *   **Quyền hạn được gán:**
        *   `Partner.Customer` (View, Create, Update, Delete)
        *   `Order.SaleOrder` (View, Create, Update, Delete) *(Không có quyền duyệt)*
        *   `Order.SalesRecall` (View, Create, Update, Delete) *(Không có quyền duyệt)*

*   **Role: SalesManager (Trưởng phòng Kinh doanh / Trưởng nhóm Bán hàng - Checker)**
    *   **Mô tả:** Duyệt đơn đặt hàng bán của nhân viên để chuyển kho xuất hàng, phê duyệt hồ sơ khách hàng và duyệt yêu cầu thu hồi hàng bán từ khách hàng.
    *   **Quyền hạn được gán:**
        *   `Order.SaleOrder` (View, **Approve**, **Reject**)
        *   `Order.SalesRecall` (View, **Approve**, **Reject**)
        *   `Partner.Customer` (View)

---

## 2. Danh sách Tài khoản mẫu (Default Accounts Seed)

Các tài khoản mẫu sau sẽ được khởi tạo tự động trong hệ thống với mật khẩu mặc định (ví dụ: `P@ssword123`) phục vụ quá trình vận hành và kiểm thử các luồng nghiệp vụ:

| Username | Email | Vai trò (Role) | Ý nghĩa kiểm thử luồng |
| :--- | :--- | :--- | :--- |
| `admin` | `admin@supplycore.com` | `Admin` (Quản trị viên hệ thống) | Quản trị toàn hệ thống |
| `catalog.staff` | `catalog.staff@supplycore.com` | `CatalogStaff` (Nhân viên Danh mục) | Lập thông tin thuốc mới |
| `catalog.manager` | `catalog.manager@supplycore.com` | `CatalogManager` (Quản lý Danh mục) | Phê duyệt thông tin thuốc |
| `purchasing.staff` | `purchasing.staff@supplycore.com` | `PurchasingStaff` (Nhân viên Mua hàng) | Lập PR, PO, PRR, PurchaseReturn |
| `purchasing.manager` | `purchasing.manager@supplycore.com` | `PurchasingManager` (Trưởng phòng Mua hàng) | Phê duyệt PR, PO, PRR, PurchaseReturn |
| `warehouse.staff` | `warehouse.staff@supplycore.com` | `WarehouseStaff` (Nhân viên Kho) | Nhập lô hàng, lập phiếu nhập/xuất kho |
| `warehouse.manager` | `warehouse.manager@supplycore.com` | `WarehouseManager` (Quản lý Kho) | Duyệt phiếu kho, điều chuyển vùng kho |
| `sales.staff` | `sales.staff@supplycore.com` | `SalesStaff` (Nhân viên Bán hàng) | Lập đơn bán SO, yêu cầu SalesRecall |
| `sales.manager` | `sales.manager@supplycore.com` | `SalesManager` (Trưởng phòng Kinh doanh) | Phê duyệt SO, yêu cầu SalesRecall |
