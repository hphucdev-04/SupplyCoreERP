# Database Schema

> **Tổng số bảng:** 49 &nbsp;|&nbsp; **Tổng số cột:** 687

---

## Danh sách các bảng

| # | Tên bảng | Số cột |
|---|----------|--------|
| 1 | `AppActiveIngredients` | 12 |
| 2 | `AppAgentMessages` | 8 |
| 3 | `AppAgentSessions` | 6 |
| 4 | `AppAgentTasks` | 8 |
| 5 | `AppAreas` | 13 |
| 6 | `AppBaseUnits` | 12 |
| 7 | `AppBins` | 20 |
| 8 | `AppCategories` | 11 |
| 9 | `AppCities` | 12 |
| 10 | `AppContinents` | 11 |
| 11 | `AppCountries` | 13 |
| 12 | `AppCustomers` | 28 |
| 13 | `AppDocumentSequences` | 6 |
| 14 | `AppDosageForms` | 12 |
| 15 | `AppInventoryBalances` | 12 |
| 16 | `AppInventoryBinBalances` | 5 |
| 17 | `AppInventoryReservations` | 15 |
| 18 | `AppInventoryTicketDetails` | 12 |
| 19 | `AppInventoryTicketLines` | 11 |
| 20 | `AppInventoryTickets` | 17 |
| 21 | `AppInventoryTransactions` | 20 |
| 22 | `AppManufacturers` | 14 |
| 23 | `AppMedicineIngredients` | 3 |
| 24 | `AppMedicineRegistrations` | 14 |
| 25 | `AppMedicines` | 7 |
| 26 | `AppNotifications` | 10 |
| 27 | `AppPriceLists` | 15 |
| 28 | `AppProductBatches` | 18 |
| 29 | `AppProductPrices` | 13 |
| 30 | `AppProductUnits` | 10 |
| 31 | `AppProducts` | 17 |
| 32 | `AppPurchaseOrderLines` | 13 |
| 33 | `AppPurchaseOrders` | 22 |
| 34 | `AppPurchaseRequisitionLines` | 11 |
| 35 | `AppPurchaseRequisitions` | 16 |
| 36 | `AppPurchaseReturnLines` | 14 |
| 37 | `AppPurchaseReturnRequestLines` | 20 |
| 38 | `AppPurchaseReturnRequests` | 20 |
| 39 | `AppPurchaseReturns` | 21 |
| 40 | `AppSalesOrderLines` | 14 |
| 41 | `AppSalesOrders` | 22 |
| 42 | `AppSalesRecallLines` | 14 |
| 43 | `AppSalesRecalls` | 21 |
| 44 | `AppSupplierProductConditions` | 9 |
| 45 | `AppSupplierProducts` | 12 |
| 46 | `AppSuppliers` | 26 |
| 47 | `AppUserNotifications` | 8 |
| 48 | `AppWarehouses` | 20 |
| 49 | `AppZones` | 19 |

---

## Chi tiết từng bảng

### 1. AppActiveIngredients

**Số cột:** 12

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Code` | `text` | ❌ | — |
| 3 | `Name` | `text` | ❌ | — |
| 4 | `ExtraProperties` | `text` | ❌ | — |
| 5 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 6 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 7 | `CreatorId` | `uuid` | ✅ | — |
| 8 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 9 | `LastModifierId` | `uuid` | ✅ | — |
| 10 | `IsDeleted` | `boolean` | ❌ | `false` |
| 11 | `DeleterId` | `uuid` | ✅ | — |
| 12 | `DeletionTime` | `timestamp without time zone` | ✅ | — |

### 2. AppAgentMessages

**Số cột:** 8

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `SessionId` | `uuid` | ❌ | — |
| 3 | `Role` | `character varying(50)` | ❌ | — |
| 4 | `Text` | `text` | ✅ | — |
| 5 | `ToolCallsJson` | `jsonb` | ✅ | — |
| 6 | `ToolResponsesJson` | `jsonb` | ✅ | — |
| 7 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 8 | `CreatorId` | `uuid` | ✅ | — |

### 3. AppAgentSessions

**Số cột:** 6

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `UserId` | `uuid` | ❌ | — |
| 3 | `ExtraProperties` | `text` | ❌ | — |
| 4 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 5 | `CreatorId` | `uuid` | ✅ | — |
| 6 | `ConcurrencyStamp` | `character varying(40)` | ❌ | `''::character varying` |

### 4. AppAgentTasks

**Số cột:** 8

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `SessionId` | `uuid` | ❌ | — |
| 3 | `TaskType` | `integer` | ❌ | — |
| 4 | `Status` | `integer` | ❌ | `1` |
| 5 | `FormJson` | `text` | ✅ | — |
| 6 | `SuspendedDataJson` | `text` | ✅ | — |
| 7 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 8 | `CreatorId` | `uuid` | ✅ | — |

### 5. AppAreas

**Số cột:** 13

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `CityId` | `uuid` | ❌ | — |
| 3 | `ZipCode` | `text` | ❌ | — |
| 4 | `Name` | `text` | ❌ | — |
| 5 | `ExtraProperties` | `text` | ❌ | — |
| 6 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 7 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 8 | `CreatorId` | `uuid` | ✅ | — |
| 9 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 10 | `LastModifierId` | `uuid` | ✅ | — |
| 11 | `IsDeleted` | `boolean` | ❌ | `false` |
| 12 | `DeleterId` | `uuid` | ✅ | — |
| 13 | `DeletionTime` | `timestamp without time zone` | ✅ | — |

### 6. AppBaseUnits

**Số cột:** 12

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Code` | `text` | ❌ | — |
| 3 | `Name` | `text` | ❌ | — |
| 4 | `ExtraProperties` | `text` | ❌ | — |
| 5 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 6 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 7 | `CreatorId` | `uuid` | ✅ | — |
| 8 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 9 | `LastModifierId` | `uuid` | ✅ | — |
| 10 | `IsDeleted` | `boolean` | ❌ | `false` |
| 11 | `DeleterId` | `uuid` | ✅ | — |
| 12 | `DeletionTime` | `timestamp without time zone` | ✅ | — |

### 7. AppBins

**Số cột:** 20

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `WarehouseId` | `uuid` | ❌ | — |
| 3 | `ZoneId` | `uuid` | ❌ | — |
| 4 | `Code` | `character varying(50)` | ❌ | — |
| 5 | `PositionX` | `integer` | ❌ | — |
| 6 | `PositionY` | `integer` | ❌ | — |
| 7 | `Width` | `integer` | ❌ | — |
| 8 | `Length` | `integer` | ❌ | — |
| 9 | `Rotation` | `real` | ❌ | — |
| 10 | `IsBlocked` | `boolean` | ❌ | — |
| 11 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 12 | `CreatorId` | `uuid` | ✅ | — |
| 13 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 14 | `LastModifierId` | `uuid` | ✅ | — |
| 15 | `IsDeleted` | `boolean` | ❌ | `false` |
| 16 | `DeleterId` | `uuid` | ✅ | — |
| 17 | `DeletionTime` | `timestamp without time zone` | ✅ | — |
| 18 | `MaxSKU` | `integer` | ❌ | `0` |
| 19 | `MaxVolume` | `numeric` | ❌ | `0.0` |
| 20 | `Height` | `integer` | ❌ | `0` |

### 8. AppCategories

**Số cột:** 11

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Name` | `text` | ❌ | — |
| 3 | `ExtraProperties` | `text` | ❌ | — |
| 4 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 5 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 6 | `CreatorId` | `uuid` | ✅ | — |
| 7 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 8 | `LastModifierId` | `uuid` | ✅ | — |
| 9 | `IsDeleted` | `boolean` | ❌ | `false` |
| 10 | `DeleterId` | `uuid` | ✅ | — |
| 11 | `DeletionTime` | `timestamp without time zone` | ✅ | — |

### 9. AppCities

**Số cột:** 12

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `CountryId` | `uuid` | ❌ | — |
| 3 | `Name` | `text` | ❌ | — |
| 4 | `ExtraProperties` | `text` | ❌ | — |
| 5 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 6 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 7 | `CreatorId` | `uuid` | ✅ | — |
| 8 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 9 | `LastModifierId` | `uuid` | ✅ | — |
| 10 | `IsDeleted` | `boolean` | ❌ | `false` |
| 11 | `DeleterId` | `uuid` | ✅ | — |
| 12 | `DeletionTime` | `timestamp without time zone` | ✅ | — |

### 10. AppContinents

**Số cột:** 11

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Name` | `text` | ❌ | — |
| 3 | `ExtraProperties` | `text` | ❌ | — |
| 4 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 5 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 6 | `CreatorId` | `uuid` | ✅ | — |
| 7 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 8 | `LastModifierId` | `uuid` | ✅ | — |
| 9 | `IsDeleted` | `boolean` | ❌ | `false` |
| 10 | `DeleterId` | `uuid` | ✅ | — |
| 11 | `DeletionTime` | `timestamp without time zone` | ✅ | — |

### 11. AppCountries

**Số cột:** 13

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `ContinentId` | `uuid` | ❌ | — |
| 3 | `ISO` | `text` | ❌ | — |
| 4 | `Name` | `text` | ❌ | — |
| 5 | `ExtraProperties` | `text` | ❌ | — |
| 6 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 7 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 8 | `CreatorId` | `uuid` | ✅ | — |
| 9 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 10 | `LastModifierId` | `uuid` | ✅ | — |
| 11 | `IsDeleted` | `boolean` | ❌ | `false` |
| 12 | `DeleterId` | `uuid` | ✅ | — |
| 13 | `DeletionTime` | `timestamp without time zone` | ✅ | — |

### 12. AppCustomers

**Số cột:** 28

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Code` | `text` | ❌ | — |
| 3 | `Name` | `text` | ❌ | — |
| 4 | `PhoneNumber` | `text` | ✅ | — |
| 5 | `Email` | `text` | ✅ | — |
| 6 | `Gender` | `integer` | ✅ | — |
| 7 | `Type` | `integer` | ❌ | — |
| 8 | `TaxCode` | `text` | ✅ | — |
| 9 | `IsActive` | `boolean` | ❌ | — |
| 10 | `Address` | `text` | ✅ | — |
| 11 | `CountryId` | `uuid` | ✅ | — |
| 12 | `CityId` | `uuid` | ✅ | — |
| 13 | `AreaId` | `uuid` | ✅ | — |
| 14 | `DebtLimit` | `numeric` | ❌ | — |
| 15 | `PaymentTermDays` | `integer` | ❌ | — |
| 16 | `CurrentDebt` | `numeric` | ❌ | — |
| 17 | `ExtraProperties` | `text` | ❌ | — |
| 18 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 19 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 20 | `CreatorId` | `uuid` | ✅ | — |
| 21 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 22 | `LastModifierId` | `uuid` | ✅ | — |
| 23 | `IsDeleted` | `boolean` | ❌ | `false` |
| 24 | `DeleterId` | `uuid` | ✅ | — |
| 25 | `DeletionTime` | `timestamp without time zone` | ✅ | — |
| 26 | `Note` | `text` | ✅ | — |
| 27 | `RepresentativeName` | `text` | ✅ | — |
| 28 | `PriceListId` | `uuid` | ✅ | — |

### 13. AppDocumentSequences

**Số cột:** 6

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `DocumentType` | `character varying(10)` | ❌ | — |
| 3 | `PrefixDate` | `character varying(6)` | ❌ | — |
| 4 | `LastValue` | `integer` | ❌ | — |
| 5 | `ExtraProperties` | `text` | ❌ | — |
| 6 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |

### 14. AppDosageForms

**Số cột:** 12

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Code` | `text` | ❌ | — |
| 3 | `Name` | `text` | ❌ | — |
| 4 | `ExtraProperties` | `text` | ❌ | — |
| 5 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 6 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 7 | `CreatorId` | `uuid` | ✅ | — |
| 8 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 9 | `LastModifierId` | `uuid` | ✅ | — |
| 10 | `IsDeleted` | `boolean` | ❌ | `false` |
| 11 | `DeleterId` | `uuid` | ✅ | — |
| 12 | `DeletionTime` | `timestamp without time zone` | ✅ | — |

### 15. AppInventoryBalances

**Số cột:** 12

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `WarehouseId` | `uuid` | ❌ | — |
| 3 | `ProductId` | `uuid` | ❌ | — |
| 4 | `ProductBatchId` | `uuid` | ❌ | — |
| 5 | `Quantity` | `numeric` | ❌ | — |
| 6 | `LockedQuantity` | `numeric` | ❌ | — |
| 7 | `ExtraProperties` | `text` | ❌ | — |
| 8 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 9 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 10 | `CreatorId` | `uuid` | ✅ | — |
| 11 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 12 | `LastModifierId` | `uuid` | ✅ | — |

### 16. AppInventoryBinBalances

**Số cột:** 5

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `InventoryBalanceId` | `uuid` | ❌ | — |
| 3 | `BinId` | `uuid` | ❌ | — |
| 4 | `Quantity` | `numeric` | ❌ | — |
| 5 | `LockedQuantity` | `numeric` | ❌ | — |

### 17. AppInventoryReservations

**Số cột:** 15

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `ReferenceDocumentId` | `uuid` | ❌ | — |
| 3 | `ReferenceDocumentNumber` | `character varying(50)` | ❌ | — |
| 4 | `WarehouseId` | `uuid` | ❌ | — |
| 5 | `BinId` | `uuid` | ❌ | — |
| 6 | `ProductId` | `uuid` | ❌ | — |
| 7 | `ProductBatchId` | `uuid` | ❌ | — |
| 8 | `ReservedQuantity` | `numeric` | ❌ | — |
| 9 | `Status` | `integer` | ❌ | — |
| 10 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 11 | `CreatorId` | `uuid` | ✅ | — |
| 12 | `PartnerId` | `uuid` | ✅ | — |
| 13 | `PartnerName` | `character varying(250)` | ✅ | — |
| 14 | `SourceDocumentId` | `uuid` | ✅ | — |
| 15 | `SourceDocumentNumber` | `character varying(50)` | ✅ | — |

### 18. AppInventoryTicketDetails

**Số cột:** 12

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `TicketLineId` | `uuid` | ❌ | — |
| 3 | `ProductId` | `uuid` | ❌ | — |
| 4 | `ProductBatchId` | `uuid` | ❌ | — |
| 5 | `BinId` | `uuid` | ❌ | — |
| 6 | `Quantity` | `numeric` | ❌ | — |
| 7 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 8 | `CreatorId` | `uuid` | ✅ | — |
| 9 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 10 | `LastModifierId` | `uuid` | ✅ | — |
| 11 | `ConversionFactor` | `integer` | ❌ | `0` |
| 12 | `UnitId` | `uuid` | ❌ | `'00000000-0000-0000-0000-000000000000'::uuid` |

### 19. AppInventoryTicketLines

**Số cột:** 11

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `TicketId` | `uuid` | ❌ | — |
| 3 | `ProductId` | `uuid` | ❌ | — |
| 4 | `Quantity` | `numeric` | ❌ | — |
| 5 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 6 | `CreatorId` | `uuid` | ✅ | — |
| 7 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 8 | `LastModifierId` | `uuid` | ✅ | — |
| 9 | `ConversionFactor` | `integer` | ❌ | `0` |
| 10 | `UnitId` | `uuid` | ❌ | `'00000000-0000-0000-0000-000000000000'::uuid` |
| 11 | `ReferenceDocumentLineId` | `uuid` | ❌ | `'00000000-0000-0000-0000-000000000000'::uuid` |

### 20. AppInventoryTickets

**Số cột:** 17

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `TicketNumber` | `character varying(50)` | ❌ | — |
| 3 | `Type` | `integer` | ❌ | — |
| 4 | `Status` | `integer` | ❌ | — |
| 5 | `WarehouseId` | `uuid` | ❌ | — |
| 6 | `ReferenceDocumentId` | `uuid` | ❌ | `'00000000-0000-0000-0000-000000000000'::uuid` |
| 7 | `Note` | `character varying(1000)` | ✅ | — |
| 8 | `ExtraProperties` | `text` | ❌ | — |
| 9 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 10 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 11 | `CreatorId` | `uuid` | ✅ | — |
| 12 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 13 | `LastModifierId` | `uuid` | ✅ | — |
| 14 | `IsDeleted` | `boolean` | ❌ | `false` |
| 15 | `DeleterId` | `uuid` | ✅ | — |
| 16 | `DeletionTime` | `timestamp without time zone` | ✅ | — |
| 17 | `ReferenceDocumentNumber` | `text` | ✅ | — |

### 21. AppInventoryTransactions

**Số cột:** 20

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `WarehouseId` | `uuid` | ❌ | — |
| 3 | `BinId` | `uuid` | ❌ | — |
| 4 | `ProductId` | `uuid` | ❌ | — |
| 5 | `ProductBatchId` | `uuid` | ❌ | — |
| 6 | `TransactionType` | `integer` | ❌ | — |
| 7 | `QuantityChanged` | `numeric` | ❌ | — |
| 8 | `BalanceAfterTransaction` | `numeric` | ❌ | — |
| 9 | `ReferenceDocumentId` | `uuid` | ✅ | — |
| 10 | `Note` | `character varying(1000)` | ✅ | — |
| 11 | `ExtraProperties` | `text` | ❌ | — |
| 12 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 13 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 14 | `CreatorId` | `uuid` | ✅ | — |
| 15 | `ReferenceDocumentNumber` | `character varying(50)` | ✅ | — |
| 16 | `PartnerId` | `uuid` | ✅ | — |
| 17 | `PartnerName` | `character varying(250)` | ✅ | — |
| 18 | `SourceDocumentId` | `uuid` | ✅ | — |
| 19 | `SourceDocumentNumber` | `character varying(50)` | ✅ | — |
| 20 | `CorrelationId` | `uuid` | ✅ | — |

### 22. AppManufacturers

**Số cột:** 14

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Name` | `text` | ❌ | — |
| 3 | `ContinentId` | `uuid` | ❌ | — |
| 4 | `CountryId` | `uuid` | ❌ | — |
| 5 | `ExtraProperties` | `text` | ❌ | — |
| 6 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 7 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 8 | `CreatorId` | `uuid` | ✅ | — |
| 9 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 10 | `LastModifierId` | `uuid` | ✅ | — |
| 11 | `IsDeleted` | `boolean` | ❌ | `false` |
| 12 | `DeleterId` | `uuid` | ✅ | — |
| 13 | `DeletionTime` | `timestamp without time zone` | ✅ | — |
| 14 | `Code` | `text` | ❌ | `''::text` |

### 23. AppMedicineIngredients

**Số cột:** 3

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `MedicineId` | `uuid` | ❌ | — |
| 3 | `ActiveIngredientId` | `uuid` | ❌ | — |

### 24. AppMedicineRegistrations

**Số cột:** 14

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `MedicineId` | `uuid` | ❌ | — |
| 3 | `RegistrationNumber` | `character varying(100)` | ❌ | — |
| 4 | `ValidFrom` | `timestamp without time zone` | ✅ | — |
| 5 | `ValidTo` | `timestamp without time zone` | ✅ | — |
| 6 | `IsActive` | `boolean` | ❌ | — |
| 7 | `Note` | `text` | ✅ | — |
| 8 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 9 | `CreatorId` | `uuid` | ✅ | — |
| 10 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 11 | `LastModifierId` | `uuid` | ✅ | — |
| 12 | `IsDeleted` | `boolean` | ❌ | `false` |
| 13 | `DeleterId` | `uuid` | ✅ | — |
| 14 | `DeletionTime` | `timestamp without time zone` | ✅ | — |

### 25. AppMedicines

**Số cột:** 7

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `DosageFormId` | `uuid` | ❌ | — |
| 3 | `UsageRoute` | `integer` | ❌ | — |
| 4 | `StorageCondition` | `integer` | ❌ | — |
| 5 | `IsPrescriptionDrug` | `boolean` | ❌ | — |
| 6 | `IsActive` | `boolean` | ❌ | `false` |
| 7 | `Status` | `integer` | ❌ | `0` |

### 26. AppNotifications

**Số cột:** 10

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Title` | `character varying(255)` | ❌ | — |
| 3 | `Content` | `character varying(2048)` | ❌ | — |
| 4 | `Severity` | `integer` | ❌ | — |
| 5 | `Level` | `integer` | ❌ | — |
| 6 | `ExtraProperties` | `text` | ❌ | — |
| 7 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 8 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 9 | `CreatorId` | `uuid` | ✅ | — |
| 10 | `TargetPermissions` | `ARRAY` | ❌ | — |

### 27. AppPriceLists

**Số cột:** 15

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Code` | `character varying(20)` | ❌ | — |
| 3 | `Name` | `character varying(100)` | ❌ | — |
| 4 | `Currency` | `integer` | ❌ | — |
| 5 | `IsBase` | `boolean` | ❌ | — |
| 6 | `IsActive` | `boolean` | ❌ | — |
| 7 | `ExtraProperties` | `text` | ❌ | — |
| 8 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 9 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 10 | `CreatorId` | `uuid` | ✅ | — |
| 11 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 12 | `LastModifierId` | `uuid` | ✅ | — |
| 13 | `IsDeleted` | `boolean` | ❌ | `false` |
| 14 | `DeleterId` | `uuid` | ✅ | — |
| 15 | `DeletionTime` | `timestamp without time zone` | ✅ | — |

### 28. AppProductBatches

**Số cột:** 18

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `ProductId` | `uuid` | ❌ | — |
| 3 | `BatchNumber` | `character varying(100)` | ❌ | — |
| 4 | `ManufacturingDate` | `timestamp without time zone` | ❌ | — |
| 5 | `ExpiryDate` | `timestamp without time zone` | ❌ | — |
| 6 | `SupplierId` | `uuid` | ✅ | — |
| 7 | `Status` | `integer` | ❌ | — |
| 8 | `ExtraProperties` | `text` | ❌ | — |
| 9 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 10 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 11 | `CreatorId` | `uuid` | ✅ | — |
| 12 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 13 | `LastModifierId` | `uuid` | ✅ | — |
| 14 | `IsDeleted` | `boolean` | ❌ | `false` |
| 15 | `DeleterId` | `uuid` | ✅ | — |
| 16 | `DeletionTime` | `timestamp without time zone` | ✅ | — |
| 17 | `Code` | `text` | ❌ | `''::text` |
| 18 | `MedicineRegistrationId` | `uuid` | ✅ | — |

### 29. AppProductPrices

**Số cột:** 13

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `PriceListId` | `uuid` | ❌ | — |
| 3 | `ProductId` | `uuid` | ❌ | — |
| 4 | `UnitId` | `uuid` | ❌ | — |
| 5 | `Price` | `numeric` | ❌ | — |
| 6 | `MinQuantity` | `integer` | ❌ | — |
| 7 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 8 | `CreatorId` | `uuid` | ✅ | — |
| 9 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 10 | `LastModifierId` | `uuid` | ✅ | — |
| 11 | `IsDeleted` | `boolean` | ❌ | `false` |
| 12 | `DeleterId` | `uuid` | ✅ | — |
| 13 | `DeletionTime` | `timestamp without time zone` | ✅ | — |

### 30. AppProductUnits

**Số cột:** 10

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `ProductId` | `uuid` | ❌ | — |
| 3 | `UnitId` | `uuid` | ❌ | — |
| 4 | `ConversionFactor` | `integer` | ❌ | — |
| 5 | `Level` | `integer` | ❌ | — |
| 6 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 7 | `CreatorId` | `uuid` | ✅ | — |
| 8 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 9 | `LastModifierId` | `uuid` | ✅ | — |
| 10 | `Volume` | `numeric` | ❌ | `0.0` |

### 31. AppProducts

**Số cột:** 17

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `CategoryId` | `uuid` | ❌ | — |
| 3 | `ManufacturerId` | `uuid` | ❌ | — |
| 4 | `Code` | `text` | ❌ | — |
| 5 | `Name` | `text` | ❌ | — |
| 6 | `BaseUnitId` | `uuid` | ❌ | — |
| 7 | `ProductType` | `integer` | ❌ | — |
| 8 | `ExtraProperties` | `text` | ❌ | — |
| 9 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 10 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 11 | `CreatorId` | `uuid` | ✅ | — |
| 12 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 13 | `LastModifierId` | `uuid` | ✅ | — |
| 14 | `IsDeleted` | `boolean` | ❌ | `false` |
| 15 | `DeleterId` | `uuid` | ✅ | — |
| 16 | `DeletionTime` | `timestamp without time zone` | ✅ | — |
| 17 | `BaseUnitVolume` | `numeric` | ❌ | `0.0` |

### 32. AppPurchaseOrderLines

**Số cột:** 13

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `PurchaseOrderId` | `uuid` | ❌ | — |
| 3 | `ProductId` | `uuid` | ❌ | — |
| 4 | `UnitId` | `uuid` | ❌ | — |
| 5 | `ConversionFactor` | `integer` | ❌ | — |
| 6 | `Quantity` | `numeric` | ❌ | — |
| 7 | `UnitPrice` | `numeric` | ❌ | — |
| 8 | `TaxRate` | `numeric` | ❌ | — |
| 9 | `ReceivedQuantity` | `numeric` | ❌ | — |
| 10 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 11 | `CreatorId` | `uuid` | ✅ | — |
| 12 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 13 | `LastModifierId` | `uuid` | ✅ | — |

### 33. AppPurchaseOrders

**Số cột:** 22

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Code` | `character varying(50)` | ❌ | — |
| 3 | `SupplierId` | `uuid` | ❌ | — |
| 4 | `OrderDate` | `timestamp without time zone` | ❌ | — |
| 5 | `ExpectedDeliveryDate` | `timestamp without time zone` | ✅ | — |
| 6 | `Status` | `integer` | ❌ | — |
| 7 | `SubTotal` | `numeric` | ❌ | — |
| 8 | `TaxAmount` | `numeric` | ❌ | — |
| 9 | `TotalAmount` | `numeric` | ❌ | — |
| 10 | `Note` | `character varying(1000)` | ✅ | — |
| 11 | `ExtraProperties` | `text` | ❌ | — |
| 12 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 13 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 14 | `CreatorId` | `uuid` | ✅ | — |
| 15 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 16 | `LastModifierId` | `uuid` | ✅ | — |
| 17 | `IsDeleted` | `boolean` | ❌ | `false` |
| 18 | `DeleterId` | `uuid` | ✅ | — |
| 19 | `DeletionTime` | `timestamp without time zone` | ✅ | — |
| 20 | `DueDate` | `timestamp without time zone` | ✅ | — |
| 21 | `WarehouseId` | `uuid` | ❌ | `'00000000-0000-0000-0000-000000000000'::uuid` |
| 22 | `PurchaseRequisitionId` | `uuid` | ✅ | — |

### 34. AppPurchaseRequisitionLines

**Số cột:** 11

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `PurchaseRequisitionId` | `uuid` | ❌ | — |
| 3 | `ProductId` | `uuid` | ❌ | — |
| 4 | `UnitId` | `uuid` | ❌ | — |
| 5 | `Quantity` | `numeric` | ❌ | — |
| 6 | `OrderedQuantity` | `numeric` | ❌ | — |
| 7 | `Note` | `character varying(500)` | ✅ | — |
| 8 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 9 | `CreatorId` | `uuid` | ✅ | — |
| 10 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 11 | `LastModifierId` | `uuid` | ✅ | — |

### 35. AppPurchaseRequisitions

**Số cột:** 16

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Code` | `character varying(50)` | ❌ | — |
| 3 | `RequestedDate` | `timestamp without time zone` | ❌ | — |
| 4 | `RequiredDate` | `timestamp without time zone` | ✅ | — |
| 5 | `Status` | `integer` | ❌ | — |
| 6 | `Note` | `character varying(1000)` | ✅ | — |
| 7 | `ExtraProperties` | `text` | ❌ | — |
| 8 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 9 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 10 | `CreatorId` | `uuid` | ✅ | — |
| 11 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 12 | `LastModifierId` | `uuid` | ✅ | — |
| 13 | `IsDeleted` | `boolean` | ❌ | `false` |
| 14 | `DeleterId` | `uuid` | ✅ | — |
| 15 | `DeletionTime` | `timestamp without time zone` | ✅ | — |
| 16 | `WarehouseId` | `uuid` | ❌ | `'00000000-0000-0000-0000-000000000000'::uuid` |

### 36. AppPurchaseReturnLines

**Số cột:** 14

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `PurchaseReturnId` | `uuid` | ❌ | — |
| 3 | `PurchaseOrderLineId` | `uuid` | ❌ | — |
| 4 | `ProductId` | `uuid` | ❌ | — |
| 5 | `UnitId` | `uuid` | ❌ | — |
| 6 | `ConversionFactor` | `integer` | ❌ | — |
| 7 | `Quantity` | `numeric` | ❌ | — |
| 8 | `OriginalUnitPrice` | `numeric` | ❌ | — |
| 9 | `DepreciationRate` | `numeric` | ❌ | — |
| 10 | `TaxRate` | `numeric` | ❌ | — |
| 11 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 12 | `CreatorId` | `uuid` | ✅ | — |
| 13 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 14 | `LastModifierId` | `uuid` | ✅ | — |

### 37. AppPurchaseReturnRequestLines

**Số cột:** 20

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `PurchaseReturnRequestId` | `uuid` | ❌ | — |
| 3 | `ProductId` | `uuid` | ❌ | — |
| 4 | `UnitId` | `uuid` | ❌ | — |
| 5 | `ConversionFactor` | `integer` | ❌ | — |
| 6 | `PurchaseOrderId` | `uuid` | ❌ | — |
| 7 | `PurchaseOrderLineId` | `uuid` | ❌ | — |
| 8 | `Quantity` | `numeric` | ❌ | — |
| 9 | `BaseQuantity` | `numeric` | ❌ | — |
| 10 | `OriginalUnitPrice` | `numeric` | ❌ | — |
| 11 | `DepreciationRate` | `numeric` | ❌ | — |
| 12 | `ReturnUnitPrice` | `numeric` | ❌ | — |
| 13 | `TaxRate` | `numeric` | ❌ | — |
| 14 | `TotalPrice` | `numeric` | ❌ | — |
| 15 | `TaxAmount` | `numeric` | ❌ | — |
| 16 | `FinalPrice` | `numeric` | ❌ | — |
| 17 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 18 | `CreatorId` | `uuid` | ✅ | — |
| 19 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 20 | `LastModifierId` | `uuid` | ✅ | — |

### 38. AppPurchaseReturnRequests

**Số cột:** 20

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Code` | `character varying(50)` | ❌ | — |
| 3 | `SupplierId` | `uuid` | ❌ | — |
| 4 | `WarehouseId` | `uuid` | ❌ | — |
| 5 | `ReturnType` | `integer` | ❌ | — |
| 6 | `RequestDate` | `timestamp without time zone` | ❌ | — |
| 7 | `Status` | `integer` | ❌ | — |
| 8 | `SubTotal` | `numeric` | ❌ | — |
| 9 | `TaxAmount` | `numeric` | ❌ | — |
| 10 | `TotalAmount` | `numeric` | ❌ | — |
| 11 | `Note` | `character varying(1000)` | ✅ | — |
| 12 | `ExtraProperties` | `text` | ❌ | — |
| 13 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 14 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 15 | `CreatorId` | `uuid` | ✅ | — |
| 16 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 17 | `LastModifierId` | `uuid` | ✅ | — |
| 18 | `IsDeleted` | `boolean` | ❌ | `false` |
| 19 | `DeleterId` | `uuid` | ✅ | — |
| 20 | `DeletionTime` | `timestamp without time zone` | ✅ | — |

### 39. AppPurchaseReturns

**Số cột:** 21

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Code` | `character varying(50)` | ❌ | — |
| 3 | `PurchaseOrderId` | `uuid` | ❌ | — |
| 4 | `SupplierId` | `uuid` | ❌ | — |
| 5 | `WarehouseId` | `uuid` | ❌ | — |
| 6 | `ReturnDate` | `timestamp without time zone` | ❌ | — |
| 7 | `Status` | `integer` | ❌ | — |
| 8 | `SubTotal` | `numeric` | ❌ | — |
| 9 | `TaxAmount` | `numeric` | ❌ | — |
| 10 | `TotalAmount` | `numeric` | ❌ | — |
| 11 | `Note` | `character varying(1000)` | ✅ | — |
| 12 | `ExtraProperties` | `text` | ❌ | — |
| 13 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 14 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 15 | `CreatorId` | `uuid` | ✅ | — |
| 16 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 17 | `LastModifierId` | `uuid` | ✅ | — |
| 18 | `IsDeleted` | `boolean` | ❌ | `false` |
| 19 | `DeleterId` | `uuid` | ✅ | — |
| 20 | `DeletionTime` | `timestamp without time zone` | ✅ | — |
| 21 | `PurchaseReturnRequestId` | `uuid` | ✅ | — |

### 40. AppSalesOrderLines

**Số cột:** 14

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `SalesOrderId` | `uuid` | ❌ | — |
| 3 | `ProductId` | `uuid` | ❌ | — |
| 4 | `UnitId` | `uuid` | ❌ | — |
| 5 | `ConversionFactor` | `integer` | ❌ | — |
| 6 | `Quantity` | `numeric` | ❌ | — |
| 7 | `DeliveredQuantity` | `numeric` | ❌ | — |
| 8 | `UnitPrice` | `numeric` | ❌ | — |
| 9 | `DiscountRate` | `numeric` | ❌ | — |
| 10 | `TaxRate` | `numeric` | ❌ | — |
| 11 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 12 | `CreatorId` | `uuid` | ✅ | — |
| 13 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 14 | `LastModifierId` | `uuid` | ✅ | — |

### 41. AppSalesOrders

**Số cột:** 22

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Code` | `character varying(50)` | ❌ | — |
| 3 | `CustomerId` | `uuid` | ❌ | — |
| 4 | `OrderDate` | `timestamp without time zone` | ❌ | — |
| 5 | `ExpectedDeliveryDate` | `timestamp without time zone` | ✅ | — |
| 6 | `DueDate` | `timestamp without time zone` | ✅ | — |
| 7 | `Status` | `integer` | ❌ | — |
| 8 | `SubTotal` | `numeric` | ❌ | — |
| 9 | `DiscountAmount` | `numeric` | ❌ | — |
| 10 | `TaxAmount` | `numeric` | ❌ | — |
| 11 | `TotalAmount` | `numeric` | ❌ | — |
| 12 | `Note` | `character varying(1000)` | ✅ | — |
| 13 | `WarehouseId` | `uuid` | ❌ | — |
| 14 | `ExtraProperties` | `text` | ❌ | — |
| 15 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 16 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 17 | `CreatorId` | `uuid` | ✅ | — |
| 18 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 19 | `LastModifierId` | `uuid` | ✅ | — |
| 20 | `IsDeleted` | `boolean` | ❌ | `false` |
| 21 | `DeleterId` | `uuid` | ✅ | — |
| 22 | `DeletionTime` | `timestamp without time zone` | ✅ | — |

### 42. AppSalesRecallLines

**Số cột:** 14

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `SalesRecallId` | `uuid` | ❌ | — |
| 3 | `CustomerId` | `uuid` | ❌ | — |
| 4 | `SalesOrderId` | `uuid` | ❌ | — |
| 5 | `UnitId` | `uuid` | ❌ | — |
| 6 | `ConversionFactor` | `integer` | ❌ | — |
| 7 | `Quantity` | `numeric` | ❌ | — |
| 8 | `OriginalUnitPrice` | `numeric` | ❌ | — |
| 9 | `TaxRate` | `numeric` | ❌ | — |
| 10 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 11 | `CreatorId` | `uuid` | ✅ | — |
| 12 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 13 | `LastModifierId` | `uuid` | ✅ | — |
| 14 | `RecalledQuantity` | `numeric` | ❌ | `0.0` |

### 43. AppSalesRecalls

**Số cột:** 21

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Code` | `character varying(50)` | ❌ | — |
| 3 | `RecallDecisionNumber` | `character varying(256)` | ❌ | — |
| 4 | `ProductId` | `uuid` | ❌ | — |
| 5 | `ProductBatchId` | `uuid` | ✅ | — |
| 6 | `WarehouseId` | `uuid` | ❌ | — |
| 7 | `RecallDate` | `timestamp without time zone` | ❌ | — |
| 8 | `Level` | `integer` | ❌ | — |
| 9 | `Deadline` | `timestamp without time zone` | ❌ | — |
| 10 | `Status` | `integer` | ❌ | — |
| 11 | `TotalAmount` | `numeric` | ❌ | — |
| 12 | `Note` | `character varying(1000)` | ✅ | — |
| 13 | `ExtraProperties` | `text` | ❌ | — |
| 14 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 15 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 16 | `CreatorId` | `uuid` | ✅ | — |
| 17 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 18 | `LastModifierId` | `uuid` | ✅ | — |
| 19 | `IsDeleted` | `boolean` | ❌ | `false` |
| 20 | `DeleterId` | `uuid` | ✅ | — |
| 21 | `DeletionTime` | `timestamp without time zone` | ✅ | — |

### 44. AppSupplierProductConditions

**Số cột:** 9

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `SupplierProductId` | `uuid` | ❌ | — |
| 3 | `UnitId` | `uuid` | ❌ | — |
| 4 | `ConversionFactor` | `integer` | ❌ | — |
| 5 | `StandardPrice` | `numeric` | ❌ | — |
| 6 | `LastPurchasePrice` | `numeric` | ❌ | — |
| 7 | `MinOrderQuantity` | `numeric` | ❌ | — |
| 8 | `OverDeliveryTolerancePct` | `numeric` | ❌ | — |
| 9 | `UnderDeliveryTolerancePct` | `numeric` | ❌ | — |

### 45. AppSupplierProducts

**Số cột:** 12

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `SupplierId` | `uuid` | ❌ | — |
| 3 | `ProductId` | `uuid` | ❌ | — |
| 4 | `DefaultUnitId` | `uuid` | ❌ | — |
| 5 | `LeadTimeDays` | `integer` | ❌ | — |
| 6 | `IsPreferred` | `boolean` | ❌ | — |
| 7 | `IsActive` | `boolean` | ❌ | — |
| 8 | `Note` | `text` | ✅ | — |
| 9 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 10 | `CreatorId` | `uuid` | ✅ | — |
| 11 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 12 | `LastModifierId` | `uuid` | ✅ | — |

### 46. AppSuppliers

**Số cột:** 26

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Code` | `text` | ❌ | — |
| 3 | `Name` | `text` | ❌ | — |
| 4 | `TaxCode` | `text` | ✅ | — |
| 5 | `PhoneNumber` | `text` | ✅ | — |
| 6 | `Email` | `text` | ✅ | — |
| 7 | `RepresentativeName` | `text` | ✅ | — |
| 8 | `Note` | `text` | ✅ | — |
| 9 | `IsActive` | `boolean` | ❌ | — |
| 10 | `Address` | `text` | ✅ | — |
| 11 | `DebtLimit` | `numeric` | ❌ | — |
| 12 | `PaymentTermDays` | `integer` | ❌ | — |
| 13 | `CurrentDebt` | `numeric` | ❌ | — |
| 14 | `CountryId` | `uuid` | ✅ | — |
| 15 | `CityId` | `uuid` | ✅ | — |
| 16 | `AreaId` | `uuid` | ✅ | — |
| 17 | `ExtraProperties` | `text` | ❌ | — |
| 18 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 19 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 20 | `CreatorId` | `uuid` | ✅ | — |
| 21 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 22 | `LastModifierId` | `uuid` | ✅ | — |
| 23 | `IsDeleted` | `boolean` | ❌ | `false` |
| 24 | `DeleterId` | `uuid` | ✅ | — |
| 25 | `DeletionTime` | `timestamp without time zone` | ✅ | — |
| 26 | `Gender` | `integer` | ✅ | — |

### 47. AppUserNotifications

**Số cột:** 8

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `NotificationId` | `uuid` | ❌ | — |
| 3 | `UserId` | `uuid` | ❌ | — |
| 4 | `IsRead` | `boolean` | ❌ | — |
| 5 | `ReadAt` | `timestamp without time zone` | ✅ | — |
| 6 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 7 | `CreatorId` | `uuid` | ✅ | — |
| 8 | `IsDelete` | `boolean` | ❌ | `false` |

### 48. AppWarehouses

**Số cột:** 20

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `Code` | `character varying(50)` | ❌ | — |
| 3 | `Name` | `character varying(255)` | ❌ | — |
| 4 | `Address` | `character varying(500)` | ✅ | — |
| 5 | `CityId` | `uuid` | ✅ | — |
| 6 | `AreaId` | `uuid` | ✅ | — |
| 7 | `MapWidth` | `integer` | ❌ | — |
| 8 | `MapLength` | `integer` | ❌ | — |
| 9 | `Status` | `integer` | ❌ | — |
| 10 | `IsActive` | `boolean` | ❌ | — |
| 11 | `ExtraProperties` | `text` | ❌ | — |
| 12 | `ConcurrencyStamp` | `character varying(40)` | ❌ | — |
| 13 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 14 | `CreatorId` | `uuid` | ✅ | — |
| 15 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 16 | `LastModifierId` | `uuid` | ✅ | — |
| 17 | `IsDeleted` | `boolean` | ❌ | `false` |
| 18 | `DeleterId` | `uuid` | ✅ | — |
| 19 | `DeletionTime` | `timestamp without time zone` | ✅ | — |
| 20 | `CountryId` | `uuid` | ✅ | — |

### 49. AppZones

**Số cột:** 19

| # | Cột | Kiểu dữ liệu | Nullable | Default |
|---|-----|--------------|----------|---------|
| 1 | `Id` | `uuid` | ❌ | — |
| 2 | `WarehouseId` | `uuid` | ❌ | — |
| 3 | `Code` | `character varying(50)` | ❌ | — |
| 4 | `Name` | `character varying(255)` | ❌ | — |
| 5 | `Type` | `integer` | ❌ | — |
| 6 | `StorageCondition` | `integer` | ❌ | — |
| 7 | `Color` | `character varying(20)` | ❌ | — |
| 8 | `PositionX` | `integer` | ❌ | — |
| 9 | `PositionY` | `integer` | ❌ | — |
| 10 | `Width` | `integer` | ❌ | — |
| 11 | `Length` | `integer` | ❌ | — |
| 12 | `Rotation` | `real` | ❌ | — |
| 13 | `CreationTime` | `timestamp without time zone` | ❌ | — |
| 14 | `CreatorId` | `uuid` | ✅ | — |
| 15 | `LastModificationTime` | `timestamp without time zone` | ✅ | — |
| 16 | `LastModifierId` | `uuid` | ✅ | — |
| 17 | `IsDeleted` | `boolean` | ❌ | `false` |
| 18 | `DeleterId` | `uuid` | ✅ | — |
| 19 | `DeletionTime` | `timestamp without time zone` | ✅ | — |