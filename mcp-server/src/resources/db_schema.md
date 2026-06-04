# Sơ đồ Cấu trúc Cơ sở dữ liệu (Database Schema) của SupplyCoreERP

Dưới đây là sơ đồ cấu trúc của các bảng chính trong cơ sở dữ liệu giúp bạn hiểu cách liên kết dữ liệu khi viết các câu truy vấn.

## 1. Bảng: AppProducts (Sản phẩm / Thuốc)
- Id: UUID (Khóa chính)
- Code: VARCHAR (Mã sản phẩm, ví dụ: MD2605260001, SP001)
- Name: VARCHAR (Tên sản phẩm, ví dụ: Panadol)
- BaseUnitId: UUID (Liên kết với AppBaseUnits.Id)
- IsDeleted: BOOLEAN (Trạng thái xóa, mặc định false)

## 2. Bảng: AppWarehouses (Kho hàng)
- Id: UUID (Khóa chính)
- Code: VARCHAR (Mã kho, ví dụ: KHO_HCM)
- Name: VARCHAR (Tên kho)
- Address: VARCHAR (Địa chỉ kho)
- IsDeleted: BOOLEAN

## 3. Bảng: AppInventoryBalances (Tồn kho thực tế)
- Id: UUID (Khóa chính)
- ProductId: UUID (Khóa ngoại liên kết AppProducts.Id)
- WarehouseId: UUID (Khóa ngoại liên kết AppWarehouses.Id)
- Quantity: NUMERIC (Số lượng tồn kho thực tế)
- IsDeleted: BOOLEAN

## 4. Bảng: AppSuppliers (Nhà cung cấp)
- Id: UUID (Khóa chính)
- Code: VARCHAR (Mã nhà cung cấp)
- Name: VARCHAR (Tên nhà cung cấp)
- PhoneNumber: VARCHAR (Số điện thoại)
- Email: VARCHAR (Địa chỉ email)
- IsDeleted: BOOLEAN

## 5. Bảng: AppCustomers (Khách hàng)
- Id: UUID (Khóa chính)
- Code: VARCHAR (Mã khách hàng)
- Name: VARCHAR (Tên khách hàng)
- PhoneNumber: VARCHAR (Số điện thoại)
- IsDeleted: BOOLEAN

## 6. Bảng: AppProductBatches (Lô hàng sản phẩm)
- Id: UUID (Khóa chính)
- Code: VARCHAR (Mã quản lý lô hàng)
- BatchNumber: VARCHAR (Số lô sản phẩm, ví dụ: LOT123)
- ExpiryDate: TIMESTAMP (Hạn sử dụng)
- Status: VARCHAR (Trạng thái lô hàng)
- IsDeleted: BOOLEAN

## 7. Bảng: AppBaseUnits (Đơn vị tính)
- Id: UUID (Khóa chính)
- Code: VARCHAR (Mã đơn vị tính)
- Name: VARCHAR (Tên đơn vị tính, ví dụ: Hộp, Chai, Viên)
- IsDeleted: BOOLEAN

---
## Mối quan hệ khóa ngoại (Foreign Key Relationships)
- AppProducts.BaseUnitId -> AppBaseUnits.Id (Mỗi sản phẩm có 1 đơn vị tính gốc)
- AppInventoryBalances.ProductId -> AppProducts.Id (Liên kết số lượng tồn kho với sản phẩm)
- AppInventoryBalances.WarehouseId -> AppWarehouses.Id (Liên kết số lượng tồn kho với kho chứa tương ứng)
