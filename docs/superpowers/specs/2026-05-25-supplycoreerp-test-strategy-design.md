# Chiến lược và Kế hoạch Thiết kế Kiểm thử (Test Strategy & Design Spec)

- **Dự án:** SupplyCoreERP (.NET 10.0 / ABP Framework)
- **Ngày thiết lập:** 25-05-2026
- **Trạng thái:** Đã phê duyệt (Approved)
- **Độ bao phủ mục tiêu:** 100% Method Coverage trên cả 3 tầng (Entity, Domain Service, Application Service) cho 9 thực thể cốt lõi.

---

## 1. Bản đồ cấu trúc thư mục kiểm thử vật lý

Bộ khung kiểm thử được tổ chức chặt chẽ theo cấu trúc phân lớp ABP Framework và Bounded Contexts:

### 1.1. Dự án `SupplyCoreERP.Domain.Tests`
Chứa Unit Test cho Entity, Unit Test cho Domain Manager, kịch bản abstract cho Integration Test của Domain Manager và Data Seed Contributors.

```text
test/SupplyCoreERP.Domain.Tests/
├── Catalog/
│   ├── Medicines/
│   │   ├── Medicine_Unit_Tests.cs
│   │   ├── MedicineManager_Unit_Tests.cs
│   │   └── MedicineManager_Integration_Tests.cs  (abstract)
│   └── Products/
│       ├── Product_Unit_Tests.cs
│       ├── ProductManager_Unit_Tests.cs
│       └── ProductManager_Integration_Tests.cs   (abstract)
├── Partner/
│   ├── Suppliers/
│   │   ├── Supplier_Unit_Tests.cs
│   │   ├── SupplierManager_Unit_Tests.cs
│   │   └── SupplierManager_Integration_Tests.cs  (abstract)
│   └── Customers/
│       ├── Customer_Unit_Tests.cs
│       ├── CustomerManager_Unit_Tests.cs
│       └── CustomerManager_Integration_Tests.cs  (abstract)
├── Procurement/
│   ├── PurchaseOrders/
│   │   ├── PurchaseOrder_Unit_Tests.cs
│   │   ├── PurchaseOrderManager_Unit_Tests.cs
│   │   └── PurchaseOrderManager_Integration_Tests.cs (abstract)
│   └── PurchaseRequisitions/
│       ├── PurchaseRequisition_Unit_Tests.cs
│       ├── PurchaseRequisitionManager_Unit_Tests.cs
│       └── PurchaseRequisitionManager_Integration_Tests.cs (abstract)
├── Sales/
│   └── SalesOrders/
│       ├── SalesOrder_Unit_Tests.cs
│       ├── SalesOrderManager_Unit_Tests.cs
│       └── SalesOrderManager_Integration_Tests.cs (abstract)
├── Inventory/
│   ├── Tickets/
│   │   ├── InventoryTicket_Unit_Tests.cs
│   │   ├── TicketManager_Unit_Tests.cs            
│   │   └── TicketManager_Integration_Tests.cs     (abstract) 
│   └── Balances/
│       ├── InventoryBalance_Unit_Tests.cs
│       ├── InventoryBalanceManager_Unit_Tests.cs
│       └── InventoryBalanceManager_Integration_Tests.cs (abstract)
└── SeedData/
    ├── MedicineTestDataSeedContributor.cs
    ├── ProductTestDataSeedContributor.cs
    ├── SupplierTestDataSeedContributor.cs
    ├── CustomerTestDataSeedContributor.cs
    ├── PurchaseOrderTestDataSeedContributor.cs
    ├── PurchaseRequisitionTestDataSeedContributor.cs
    ├── SalesOrderTestDataSeedContributor.cs
    ├── TicketTestDataSeedContributor.cs           
    └── InventoryBalanceTestDataSeedContributor.cs
```

### 1.2. Dự án `SupplyCoreERP.Application.Tests`
Chứa các lớp kịch bản tích hợp abstract kiểm thử cho các Application Service.

```text
test/SupplyCoreERP.Application.Tests/
├── Catalog/
│   └── Medicines/
│       └── MedicineAppService_Integration_Tests.cs (abstract)
├── Partner/
│   ├── Suppliers/
│   │   └── SupplierAppService_Integration_Tests.cs (abstract)
│   └── Customers/
│       └── CustomerAppService_Integration_Tests.cs (abstract)
├── Procurement/
│   ├── PurchaseOrders/
│   │   └── PurchaseOrderAppService_Integration_Tests.cs (abstract)
│   └── PurchaseRequisitions/
│       └── PurchaseRequisitionAppService_Integration_Tests.cs (abstract)
├── Sales/
│   └── SalesOrders/
│       └── SalesOrderAppService_Integration_Tests.cs (abstract)
└── Inventory/
    ├── Tickets/
    │   └── InventoryTicketAppService_Integration_Tests.cs (abstract)
    └── Balances/
        └── InventoryBalanceAppService_Integration_Tests.cs (abstract)
```

### 1.3. Dự án `SupplyCoreERP.EntityFrameworkCore.Tests`
Chứa các lớp concrete chạy thực tế kế thừa từ các lớp abstract để khởi tạo DbContext (SQLite In-Memory).

```text
test/SupplyCoreERP.EntityFrameworkCore.Tests/EntityFrameworkCore/
├── Domains/
│   ├── MedicineManager_Integration_Tests.cs
│   ├── ProductManager_Integration_Tests.cs
│   ├── SupplierManager_Integration_Tests.cs
│   ├── CustomerManager_Integration_Tests.cs
│   ├── PurchaseOrderManager_Integration_Tests.cs
│   ├── PurchaseRequisitionManager_Integration_Tests.cs
│   ├── SalesOrderManager_Integration_Tests.cs
│   ├── TicketManager_Integration_Tests.cs         
│   └── InventoryBalanceManager_Integration_Tests.cs
└── Applications/
    ├── MedicineAppService_Integration_Tests.cs
    ├── SupplierAppService_Integration_Tests.cs
    ├── CustomerAppService_Integration_Tests.cs
    ├── PurchaseOrderAppService_Integration_Tests.cs
    ├── PurchaseRequisitionAppService_Integration_Tests.cs
    ├── SalesOrderAppService_Integration_Tests.cs
    ├── InventoryTicketAppService_Integration_Tests.cs
    └── InventoryBalanceAppService_Integration_Tests.cs
```

---

## 2. Đặc tả chi tiết các Test Cases tầng Domain

### 2.1. Bounded Context: `Catalog` (Medicine, Product)

#### A. `Medicine_Unit_Tests.cs` (Unit Test cho Entity)
* `Should_Create_Medicine_With_Valid_Parameters()`
* `Should_Update_Medicine_Info()`
* `Should_Add_Registration_When_RegNumber_Changes()`
* `Should_Update_PharmaInfo_With_Valid_Parameters()`
* `Should_Add_Ingredient_To_Medicine()`
* `Should_Remove_Ingredient_From_Medicine()`
* `Should_Add_Unit_With_Conversion_Factor()`
* `Should_Update_Unit_Conversion_Factor()`
* `Should_Remove_Unit_From_Medicine()`
* `Should_Test_Product_Abstract_Behavior_Through_Medicine()`: Kiểm thử hành vi thừa kế của lớp cha `Product`.

#### B. `MedicineManager`
* **`MedicineManager_Unit_Tests.cs` (Unit Test thuần):**
  * `Should_Throw_BusinessException_When_Foreign_Keys_Are_Invalid()`
  * `Should_Throw_BusinessException_When_Add_NonExistent_Ingredient()`
* **`MedicineManager_Integration_Tests.cs` (Integration Test):**
  * `Should_Create_Medicine_When_Parameters_Are_Valid()`
  * `Should_Update_Medicine_Successfully()`

#### C. `ProductManager`
* **`ProductManager_Unit_Tests.cs` (Unit Test thuần):**
  * `Should_Throw_BusinessException_When_CheckCode_Duplicate()`
* **`ProductManager_Integration_Tests.cs` (Integration Test):**
  * `Should_Return_True_For_HasTransactions_When_Balance_Exists()`
  * `Should_Return_True_For_HasTransactions_When_TicketLine_Exists()`
  * `Should_Return_True_For_HasTransactions_When_PurchaseOrderLine_Exists()`
  * `Should_Return_True_For_HasTransactions_When_SalesOrderLine_Exists()`
  * `Should_Return_True_For_HasTransactions_When_PurchaseRequisitionLine_Exists()`
  * `Should_Throw_BusinessException_When_BaseUnit_Changed_With_Transactions()`
  * `Should_Throw_BusinessException_When_Unit_Changed_With_Transactions()`

---

### 2.2. Bounded Context: `Partner` (Supplier, Customer)

#### A. `Supplier_Unit_Tests.cs` (Unit Test cho Entity)
* `Should_Create_Supplier_With_Valid_Parameters()`
* `Should_Update_Supplier_Info()`
* `Should_Set_Location_Successfully()`
* `Should_Set_DebtInfo_Successfully()`
* `Should_Add_Product_To_Supplier()`
* `Should_Update_SupplierProduct_Successfully()`
* `Should_Remove_SupplierProduct()`
* `Should_Toggle_Product_Active()`

#### B. `SupplierManager`
* **`SupplierManager_Unit_Tests.cs` (Unit Test thuần):**
  * `Should_Throw_BusinessException_When_Delete_Supplier_With_Outstanding_Debt()`
  * `Should_Throw_BusinessException_When_Location_Country_City_Mismatch()`
  * `Should_Throw_BusinessException_When_Location_City_Area_Mismatch()`
* **`SupplierManager_Integration_Tests.cs` (Integration Test):**
  * `Should_Create_Supplier_And_Generate_Supplier_Code()`
  * `Should_Throw_BusinessException_When_Code_Or_Name_Exists()`
  * `Should_AddProduct_Throw_Exception_When_Product_Not_Available()`
  * `Should_AddProduct_Throw_Exception_When_Unit_Not_Found()`

#### C. `Customer_Unit_Tests.cs` (Unit Test cho Entity)
* `Should_Create_Customer_With_Valid_Parameters()`
* `Should_Update_Customer_Info()`
* `Should_Set_Location_Successfully()`
* `Should_Set_DebtInfo_Successfully()`
* `Should_Set_PriceList_Successfully()`

#### D. `CustomerManager`
* **`CustomerManager_Unit_Tests.cs` (Unit Test thuần):**
  * `Should_Throw_BusinessException_When_Delete_Customer_With_Outstanding_Debt()`
* **`CustomerManager_Integration_Tests.cs` (Integration Test):**
  * `Should_Create_Customer_And_Generate_Customer_Code()`
  * `Should_Throw_BusinessException_When_Phone_Number_Already_Exists()`
  * `Should_Throw_BusinessException_When_PriceList_Not_Found()`

---

### 2.3. Bounded Context: `Procurement` (PurchaseOrder, PurchaseRequisition)

#### A. `PurchaseOrder_Unit_Tests.cs` (Unit Test cho Entity)
* `Should_Create_PurchaseOrder_With_Valid_Parameters()`
* `Should_AddLine_To_PurchaseOrder()`
* `Should_UpdateLine_In_PurchaseOrder()`
* `Should_RemoveLine_From_PurchaseOrder()`
* `Should_Update_Status_Workflow_Correctly()`

#### B. `PurchaseOrderManager`
* **`PurchaseOrderManager_Unit_Tests.cs` (Unit Test thuần):**
  * `Should_Throw_BusinessException_When_Delete_NonDraft_PurchaseOrder()`
* **`PurchaseOrderManager_Integration_Tests.cs` (Integration Test):**
  * `Should_CreateOrder_With_Valid_Supplier_And_Warehouse()`
  * `Should_CreateOrdersFromRequisition_With_Auto_Price_MOQ_Allocation()`
  * `Should_Throw_BusinessException_When_MOQ_Price_Not_Configured()`
  * `Should_Throw_BusinessException_When_Allocation_Exceeds_Remaining_Quantity()`
  * `Should_Approve_PurchaseOrder_And_Generate_GoodsReceipt_Ticket()`
  * `Should_Throw_BusinessException_When_Approve_Exceeds_Supplier_Debt_Limit()`
  * `Should_Throw_BusinessException_When_Supplier_Has_Overdue_Orders()`
  * `Should_Complete_PurchaseOrder_And_Update_Supplier_Debt()`
  * `Should_Throw_BusinessException_When_Complete_Before_GoodsReceipt_Ticket_Approved()`

#### C. `PurchaseRequisition_Unit_Tests.cs` (Unit Test cho Entity)
* `Should_Create_PurchaseRequisition_With_Valid_Parameters()`
* `Should_AddLine_To_PurchaseRequisition()`
* `Should_UpdateLine_In_PurchaseRequisition()`
* `Should_RemoveLine_From_PurchaseRequisition()`
* `Should_Update_OrderingStatus_Correctly()`

#### D. `PurchaseRequisitionManager`
* **`PurchaseRequisitionManager_Unit_Tests.cs` (Unit Test thuần):**
  * `Should_Throw_BusinessException_When_Warehouse_Is_Inactive()`
  * `Should_Throw_BusinessException_When_RequestedDate_Is_Future()`
* **`PurchaseRequisitionManager_Integration_Tests.cs` (Integration Test):**
  * `Should_Create_Requisition_With_Auto_Code_Generation()`
  * `Should_AddLine_With_Available_For_Inventory_Product()`
  * `Should_Throw_BusinessException_When_AddLine_With_Unavailable_Product()`
  * `Should_Workflow_SendToApprove_Approve_Reject_Correctly()`

---

### 2.4. Bounded Context: `Sales` (SalesOrder)

#### A. `SalesOrder_Unit_Tests.cs` (Unit Test cho Entity)
* `Should_Create_SalesOrder_With_Valid_Parameters()`
* `Should_AddLine_To_SalesOrder()`
* `Should_UpdateLine_In_SalesOrder()`
* `Should_RemoveLine_From_SalesOrder()`
* `Should_Update_Status_Workflow_Correctly()`

#### B. `SalesOrderManager`
* **`SalesOrderManager_Unit_Tests.cs` (Unit Test thuần):**
  * `Should_Throw_BusinessException_When_Delete_NonDraft_SalesOrder()`
* **`SalesOrderManager_Integration_Tests.cs` (Integration Test):**
  * `Should_CreateOrder_With_Valid_Customer_And_Warehouse()`
  * `Should_AddLine_Throw_Exception_When_Available_Inventory_Is_Insufficient()`
  * `Should_Approve_SalesOrder_And_Generate_GoodsIssue_Ticket()`
  * `Should_Throw_BusinessException_When_Approve_Exceeds_Customer_Debt_Limit()`
  * `Should_Throw_BusinessException_When_Customer_Has_Overdue_Orders()`
  * `Should_Complete_SalesOrder_And_Update_Customer_Debt()`
  * `Should_Throw_BusinessException_When_Complete_Before_GoodsIssue_Ticket_Approved()`

---

### 2.5. Bounded Context: `Inventory` (InventoryTicket, InventoryBalance)

#### A. `InventoryTicket_Unit_Tests.cs` (Unit Test cho Entity)
* `Should_Create_InventoryTicket_With_Valid_Parameters()`
* `Should_UpdateNote_In_InventoryTicket()`
* `Should_AddLine_To_InventoryTicket()`
* `Should_UpdateLine_Quantity()`
* `Should_RemoveLine_From_InventoryTicket()`

#### B. `TicketManager`
* **`TicketManager_Unit_Tests.cs` (Unit Test thuần):**
  * `Should_Throw_BusinessException_When_CreateTicket_InactiveWarehouse()`
  * `Should_Throw_BusinessException_When_CreateTicket_TooManyDrafts()`
  * `Should_Throw_BusinessException_When_UpdateTicket_Already_Approved()`
  * `Should_Throw_BusinessException_When_Delete_Approved_Ticket()`
* **`TicketManager_Integration_Tests.cs` (Integration Test):**
  * `Should_CreateTicket_With_Document_Sequence_Number()`
  * `Should_CreateTicketLine_With_Validations()`
  * `Should_AllocateFEFOForLine_Successfully()`
  * `Should_Throw_Exception_When_FEFO_Stock_Is_Insufficient()`
  * `Should_CreateTicketDetail_With_Bin_StorageCondition_QAStatus_Checks()`
  * `Should_SendToApprove_With_Validation_Of_Line_And_Detail_Quantities()`
  * `Should_RejectTicket_And_Release_LockedStock()`
  * `Should_ExecuteTicket_And_ExecuteStockMovement()`
  * `Should_SyncPurchaseOrderProgress_When_GoodsReceipt_Executed()`
  * `Should_SyncSalesOrderProgress_When_GoodsIssue_Executed()`
  * `Should_AllocateFEFO_Manually_For_Direct_FEFO_Requests()`

#### C. `InventoryBalance_Unit_Tests.cs` (Unit Test cho Entity)
* `Should_Create_InventoryBalance_With_Valid_Parameters()`
* `Should_LockStock_Correctly()`
* `Should_UnlockStock_Correctly()`
* `Should_AddStock_Correctly()`
* `Should_RemoveStock_Correctly()`

#### D. `InventoryBalanceManager`
* **`InventoryBalanceManager_Unit_Tests.cs` (Unit Test thuần):**
  * `Should_Throw_Exception_When_LockStock_With_Insufficient_Balance()`
  * `Should_Throw_Exception_When_AdjustLock_With_OutOfStock()`
* **`InventoryBalanceManager_Integration_Tests.cs` (Integration Test):**
  * `Should_LockStock_And_Create_Active_Reservations()`
  * `Should_UnlockStock_And_Cancel_Reservations()`
  * `Should_AdjustLock_Quantity_Correctly()`
  * `Should_ExecuteStockMovement_For_GoodsReceipt_And_GoodsIssue_Correctly()`

---

## 3. Đặc tả chi tiết các Test Cases tầng Application

Mỗi lớp `_Integration_Tests.cs` tại `SupplyCoreERP.Application.Tests` chứa đặc tả các ca kiểm thử tích hợp đầu cuối (E2E Integration) qua API DTOs:

* **`MedicineAppService_Integration_Tests.cs`**:
  * `Should_Get_List_Of_Medicines()`
  * `Should_Get_Medicine_By_Id()`
  * `Should_Create_Medicine_When_Input_Is_Valid()`
  * `Should_Update_Medicine_When_Input_Is_Valid()`
  * `Should_Throw_AbpValidationException_When_Input_Is_Invalid()`
  * `Should_Add_Ingredient_To_Medicine()`
  * `Should_Add_Unit_To_Medicine()`

* **`SupplierAppService_Integration_Tests.cs`**:
  * `Should_Get_List_Of_Suppliers()`
  * `Should_Create_Supplier_With_Valid_DTO()`
  * `Should_Update_Supplier_Successfully()`
  * `Should_Delete_Supplier_When_No_Debt()`
  * `Should_Add_Product_To_Supplier_Catalog()`

* **`CustomerAppService_Integration_Tests.cs`**:
  * `Should_Get_List_Of_Customers()`
  * `Should_Create_Customer_With_Valid_DTO()`
  * `Should_Update_Customer_Successfully()`
  * `Should_Delete_Customer_When_No_Debt()`

* **`PurchaseOrderAppService_Integration_Tests.cs`**:
  * `Should_Create_PurchaseOrder_Successfully()`
  * `Should_Create_Orders_From_Requisition_DTO()`
  * `Should_Send_PurchaseOrder_To_Approve()`
  * `Should_Approve_PurchaseOrder_And_Generate_Receipt_Ticket()`
  * `Should_Complete_PurchaseOrder_And_Increase_Supplier_Debt()`

* **`SalesOrderAppService_Integration_Tests.cs`**:
  * `Should_Create_SalesOrder_Successfully()`
  * `Should_Send_SalesOrder_To_Approve()`
  * `Should_Approve_SalesOrder_And_Generate_Issue_Ticket()`
  * `Should_Complete_SalesOrder_And_Increase_Customer_Debt()`

* **`PurchaseRequisitionAppService_Integration_Tests.cs`**:
  * `Should_Create_PurchaseRequisition_Successfully()`
  * `Should_Send_Requisition_To_Approve()`
  * `Should_Approve_Requisition()`
  * `Should_Reject_Requisition()`

* **`InventoryTicketAppService_Integration_Tests.cs`**:
  * `Should_Create_InventoryTicket_With_Lines_And_Details()`
  * `Should_Allocate_FEFO_For_TicketLine()`
  * `Should_Send_Ticket_To_Approve()`
  * `Should_Execute_Ticket_And_Deduct_Or_Add_Stock()`

* **`InventoryBalanceAppService_Integration_Tests.cs`**:
  * `Should_Get_List_Of_Balances()`
  * `Should_Get_Stock_Details_By_Product()`

---

## 4. Thiết kế các lớp nạp dữ liệu hạt giống (TestDataSeedContributor)

Tất cả các dữ liệu hạt giống sẽ được khai báo thông qua `TestDataConsts` tĩnh đặt tại dự án `SupplyCoreERP.Domain.Tests` để tránh trùng lặp dữ liệu và phân tách trách nhiệm nạp hạt giống rõ ràng:

1. **`MedicineTestDataSeedContributor`**: Khởi tạo cấu trúc Catalog cốt lõi bao gồm Categories, Manufacturers, BaseUnits, DosageForms, ActiveIngredients, và thực thể thuốc Paracetamol 500mg đã được phê duyệt.
2. **`SupplierTestDataSeedContributor` & `CustomerTestDataSeedContributor`**: Khởi tạo thông tin địa lý Việt Nam/HCM/Q1, Bảng giá chuẩn, Nhà cung cấp A, Khách hàng A và liên kết sản phẩm Paracetamol vào danh mục cung cấp của NCC A.
3. **`InventoryBalanceTestDataSeedContributor`**: Thiết lập cấu trúc sơ đồ kho gồm Kho tổng, Vùng lạnh, vị trí kệ Bin, Lô hàng QA Approved (`BATCH-001`) và nạp số dư ban đầu `1000` đơn vị tồn kho Paracetamol tại vị trí kệ này.
4. **`PurchaseOrderTestDataSeedContributor` & `SalesOrderTestDataSeedContributor`**: Nạp sẵn một số đơn hàng PO/SO nháp để tăng tốc độ kiểm thử tích hợp đầu cuối.
