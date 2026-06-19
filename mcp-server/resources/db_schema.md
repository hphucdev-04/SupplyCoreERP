# Database Schema — App Tables

> Tổng cộng **48 bảng**, **672 cột**

---

## Danh sách bảng

- [AppActiveIngredients](#appactiveingredients)
- [AppAgentMessages](#appagentmessages)
- [AppAgentSessions](#appagentsessions)
- [AppAgentTasks](#appagenttasks)
- [AppAreas](#appareas)
- [AppBaseUnits](#appbaseunits)
- [AppBins](#appbins)
- [AppCategories](#appcategories)
- [AppCities](#appcities)
- [AppContinents](#appcontinents)
- [AppCountries](#appcountries)
- [AppCustomers](#appcustomers)
- [AppDocumentSequences](#appdocumentsequences)
- [AppDosageForms](#appdosageforms)
- [AppInventoryBalances](#appinventorybalances)
- [AppInventoryReservations](#appinventoryreservations)
- [AppInventoryTicketDetails](#appinventoryticketdetails)
- [AppInventoryTicketLines](#appinventoryticketlines)
- [AppInventoryTickets](#appinventorytickets)
- [AppInventoryTransactions](#appinventorytransactions)
- [AppManufacturers](#appmanufacturers)
- [AppMedicineIngredients](#appmedicineingredients)
- [AppMedicineRegistrations](#appmedicineregistrations)
- [AppMedicines](#appmedicines)
- [AppNotifications](#appnotifications)
- [AppPriceLists](#apppricelists)
- [AppProductBatches](#appproductbatches)
- [AppProductPrices](#appproductprices)
- [AppProductUnits](#appproductunits)
- [AppProducts](#appproducts)
- [AppPurchaseOrderLines](#apppurchaseorderlines)
- [AppPurchaseOrders](#apppurchaseorders)
- [AppPurchaseRequisitionLines](#apppurchaserequisitionlines)
- [AppPurchaseRequisitions](#apppurchaserequisitions)
- [AppPurchaseReturnLines](#apppurchasereturnlines)
- [AppPurchaseReturnRequestLines](#apppurchasereturnrequestlines)
- [AppPurchaseReturnRequests](#apppurchasereturnrequests)
- [AppPurchaseReturns](#apppurchasereturns)
- [AppSalesOrderLines](#appsalesorderlines)
- [AppSalesOrders](#appsalesorders)
- [AppSalesRecallLines](#appsalesrecalllines)
- [AppSalesRecalls](#appsalesrecalls)
- [AppSupplierProductConditions](#appsupplierproductconditions)
- [AppSupplierProducts](#appsupplierproducts)
- [AppSuppliers](#appsuppliers)
- [AppUserNotifications](#appusernotifications)
- [AppWarehouses](#appwarehouses)
- [AppZones](#appzones)

---

## AppActiveIngredients

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Code` | `text` | ✗ |  |  |
| 3 | `Name` | `text` | ✗ |  |  |
| 4 | `ExtraProperties` | `text` | ✗ |  |  |
| 5 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 6 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 7 | `CreatorId` | `uuid` | ✓ |  |  |
| 8 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 9 | `LastModifierId` | `uuid` | ✓ |  |  |
| 10 | `IsDeleted` | `boolean` | ✗ | false |  |
| 11 | `DeleterId` | `uuid` | ✓ |  |  |
| 12 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |

## AppAgentMessages

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `SessionId` | `uuid` | ✗ |  |  |
| 3 | `Role` | `character varying(50)` | ✗ |  |  |
| 4 | `Text` | `text` | ✓ |  |  |
| 5 | `ToolCallsJson` | `jsonb` | ✓ |  |  |
| 6 | `ToolResponsesJson` | `jsonb` | ✓ |  |  |
| 7 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 8 | `CreatorId` | `uuid` | ✓ |  |  |

## AppAgentSessions

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `UserId` | `uuid` | ✗ |  |  |
| 3 | `ExtraProperties` | `text` | ✗ |  |  |
| 6 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 7 | `CreatorId` | `uuid` | ✓ |  |  |
| 11 | `ConcurrencyStamp` | `character varying(40)` | ✗ | ''::character varying |  |

## AppAgentTasks

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `SessionId` | `uuid` | ✗ |  |  |
| 3 | `TaskType` | `integer(32,0)` | ✗ |  |  |
| 4 | `Status` | `integer(32,0)` | ✗ | 1 |  |
| 5 | `FormJson` | `text` | ✓ |  |  |
| 6 | `SuspendedDataJson` | `text` | ✓ |  |  |
| 7 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 8 | `CreatorId` | `uuid` | ✓ |  |  |

## AppAreas

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `CityId` | `uuid` | ✗ |  |  |
| 3 | `ZipCode` | `text` | ✗ |  |  |
| 4 | `Name` | `text` | ✗ |  |  |
| 5 | `ExtraProperties` | `text` | ✗ |  |  |
| 6 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 7 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 8 | `CreatorId` | `uuid` | ✓ |  |  |
| 9 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 10 | `LastModifierId` | `uuid` | ✓ |  |  |
| 11 | `IsDeleted` | `boolean` | ✗ | false |  |
| 12 | `DeleterId` | `uuid` | ✓ |  |  |
| 13 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |

## AppBaseUnits

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Code` | `text` | ✗ |  |  |
| 3 | `Name` | `text` | ✗ |  |  |
| 4 | `ExtraProperties` | `text` | ✗ |  |  |
| 5 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 6 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 7 | `CreatorId` | `uuid` | ✓ |  |  |
| 8 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 9 | `LastModifierId` | `uuid` | ✓ |  |  |
| 10 | `IsDeleted` | `boolean` | ✗ | false |  |
| 11 | `DeleterId` | `uuid` | ✓ |  |  |
| 12 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |

## AppBins

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `WarehouseId` | `uuid` | ✗ |  |  |
| 3 | `ZoneId` | `uuid` | ✗ |  |  |
| 4 | `Code` | `character varying(50)` | ✗ |  |  |
| 5 | `PositionX` | `integer(32,0)` | ✗ |  |  |
| 6 | `PositionY` | `integer(32,0)` | ✗ |  |  |
| 7 | `Width` | `integer(32,0)` | ✗ |  |  |
| 8 | `Length` | `integer(32,0)` | ✗ |  |  |
| 9 | `Rotation` | `real` | ✗ |  |  |
| 11 | `IsBlocked` | `boolean` | ✗ |  |  |
| 14 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 15 | `CreatorId` | `uuid` | ✓ |  |  |
| 16 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 17 | `LastModifierId` | `uuid` | ✓ |  |  |
| 18 | `IsDeleted` | `boolean` | ✗ | false |  |
| 19 | `DeleterId` | `uuid` | ✓ |  |  |
| 20 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |
| 21 | `MaxSKU` | `integer(32,0)` | ✗ | 0 |  |

## AppCategories

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Name` | `text` | ✗ |  |  |
| 3 | `ExtraProperties` | `text` | ✗ |  |  |
| 4 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 5 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 6 | `CreatorId` | `uuid` | ✓ |  |  |
| 7 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 8 | `LastModifierId` | `uuid` | ✓ |  |  |
| 9 | `IsDeleted` | `boolean` | ✗ | false |  |
| 10 | `DeleterId` | `uuid` | ✓ |  |  |
| 11 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |

## AppCities

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `CountryId` | `uuid` | ✗ |  |  |
| 3 | `Name` | `text` | ✗ |  |  |
| 4 | `ExtraProperties` | `text` | ✗ |  |  |
| 5 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 6 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 7 | `CreatorId` | `uuid` | ✓ |  |  |
| 8 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 9 | `LastModifierId` | `uuid` | ✓ |  |  |
| 10 | `IsDeleted` | `boolean` | ✗ | false |  |
| 11 | `DeleterId` | `uuid` | ✓ |  |  |
| 12 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |

## AppContinents

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Name` | `text` | ✗ |  |  |
| 3 | `ExtraProperties` | `text` | ✗ |  |  |
| 4 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 5 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 6 | `CreatorId` | `uuid` | ✓ |  |  |
| 7 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 8 | `LastModifierId` | `uuid` | ✓ |  |  |
| 9 | `IsDeleted` | `boolean` | ✗ | false |  |
| 10 | `DeleterId` | `uuid` | ✓ |  |  |
| 11 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |

## AppCountries

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `ContinentId` | `uuid` | ✗ |  |  |
| 3 | `ISO` | `text` | ✗ |  |  |
| 4 | `Name` | `text` | ✗ |  |  |
| 5 | `ExtraProperties` | `text` | ✗ |  |  |
| 6 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 7 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 8 | `CreatorId` | `uuid` | ✓ |  |  |
| 9 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 10 | `LastModifierId` | `uuid` | ✓ |  |  |
| 11 | `IsDeleted` | `boolean` | ✗ | false |  |
| 12 | `DeleterId` | `uuid` | ✓ |  |  |
| 13 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |

## AppCustomers

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Code` | `text` | ✗ |  |  |
| 3 | `Name` | `text` | ✗ |  |  |
| 4 | `PhoneNumber` | `text` | ✓ |  |  |
| 5 | `Email` | `text` | ✓ |  |  |
| 7 | `Gender` | `integer(32,0)` | ✓ |  |  |
| 8 | `Type` | `integer(32,0)` | ✗ |  |  |
| 9 | `TaxCode` | `text` | ✓ |  |  |
| 10 | `IsActive` | `boolean` | ✗ |  |  |
| 11 | `Address` | `text` | ✓ |  |  |
| 12 | `CountryId` | `uuid` | ✓ |  |  |
| 13 | `CityId` | `uuid` | ✓ |  |  |
| 14 | `AreaId` | `uuid` | ✓ |  |  |
| 15 | `DebtLimit` | `numeric` | ✗ |  |  |
| 16 | `PaymentTermDays` | `integer(32,0)` | ✗ |  |  |
| 17 | `CurrentDebt` | `numeric` | ✗ |  |  |
| 18 | `ExtraProperties` | `text` | ✗ |  |  |
| 19 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 20 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 21 | `CreatorId` | `uuid` | ✓ |  |  |
| 22 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 23 | `LastModifierId` | `uuid` | ✓ |  |  |
| 24 | `IsDeleted` | `boolean` | ✗ | false |  |
| 25 | `DeleterId` | `uuid` | ✓ |  |  |
| 26 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |
| 27 | `Note` | `text` | ✓ |  |  |
| 28 | `RepresentativeName` | `text` | ✓ |  |  |
| 29 | `PriceListId` | `uuid` | ✓ |  |  |

## AppDocumentSequences

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `DocumentType` | `character varying(10)` | ✗ |  |  |
| 3 | `PrefixDate` | `character varying(6)` | ✗ |  |  |
| 4 | `LastValue` | `integer(32,0)` | ✗ |  |  |
| 5 | `ExtraProperties` | `text` | ✗ |  |  |
| 6 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |

## AppDosageForms

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Code` | `text` | ✗ |  |  |
| 3 | `Name` | `text` | ✗ |  |  |
| 4 | `ExtraProperties` | `text` | ✗ |  |  |
| 5 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 6 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 7 | `CreatorId` | `uuid` | ✓ |  |  |
| 8 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 9 | `LastModifierId` | `uuid` | ✓ |  |  |
| 10 | `IsDeleted` | `boolean` | ✗ | false |  |
| 11 | `DeleterId` | `uuid` | ✓ |  |  |
| 12 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |

## AppInventoryBalances

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `WarehouseId` | `uuid` | ✗ |  |  |
| 3 | `BinId` | `uuid` | ✗ |  |  |
| 4 | `ProductId` | `uuid` | ✗ |  |  |
| 5 | `ProductBatchId` | `uuid` | ✗ |  |  |
| 6 | `Quantity` | `numeric(18,2)` | ✗ |  |  |
| 7 | `LockedQuantity` | `numeric(18,2)` | ✗ |  |  |
| 8 | `ExtraProperties` | `text` | ✗ |  |  |
| 9 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 10 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 11 | `CreatorId` | `uuid` | ✓ |  |  |
| 12 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 13 | `LastModifierId` | `uuid` | ✓ |  |  |
| 14 | `IsDeleted` | `boolean` | ✗ | false |  |
| 15 | `DeleterId` | `uuid` | ✓ |  |  |
| 16 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |

## AppInventoryReservations

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `ReferenceDocumentId` | `uuid` | ✗ |  |  |
| 3 | `ReferenceDocumentNumber` | `character varying(50)` | ✗ |  |  |
| 4 | `WarehouseId` | `uuid` | ✗ |  |  |
| 5 | `BinId` | `uuid` | ✗ |  |  |
| 6 | `ProductId` | `uuid` | ✗ |  |  |
| 7 | `ProductBatchId` | `uuid` | ✗ |  |  |
| 8 | `ReservedQuantity` | `numeric(18,4)` | ✗ |  |  |
| 9 | `Status` | `integer(32,0)` | ✗ |  |  |
| 10 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 11 | `CreatorId` | `uuid` | ✓ |  |  |

## AppInventoryTicketDetails

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `TicketLineId` | `uuid` | ✗ |  |  |
| 3 | `ProductId` | `uuid` | ✗ |  |  |
| 4 | `ProductBatchId` | `uuid` | ✗ |  |  |
| 5 | `BinId` | `uuid` | ✗ |  |  |
| 6 | `Quantity` | `numeric(18,2)` | ✗ |  |  |
| 7 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 8 | `CreatorId` | `uuid` | ✓ |  |  |
| 9 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 10 | `LastModifierId` | `uuid` | ✓ |  |  |
| 14 | `ConversionFactor` | `integer(32,0)` | ✗ | 0 |  |
| 15 | `UnitId` | `uuid` | ✗ | '00000000-0000-0000-0000-000000000000'::... |  |

## AppInventoryTicketLines

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `TicketId` | `uuid` | ✗ |  |  |
| 3 | `ProductId` | `uuid` | ✗ |  |  |
| 5 | `Quantity` | `numeric(18,2)` | ✗ |  |  |
| 6 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 7 | `CreatorId` | `uuid` | ✓ |  |  |
| 8 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 9 | `LastModifierId` | `uuid` | ✓ |  |  |
| 13 | `ConversionFactor` | `integer(32,0)` | ✗ | 0 |  |
| 14 | `UnitId` | `uuid` | ✗ | '00000000-0000-0000-0000-000000000000'::... |  |
| 17 | `ReferenceDocumentLineId` | `uuid` | ✗ | '00000000-0000-0000-0000-000000000000'::... |  |

## AppInventoryTickets

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `TicketNumber` | `character varying(50)` | ✗ |  |  |
| 3 | `Type` | `integer(32,0)` | ✗ |  |  |
| 4 | `Status` | `integer(32,0)` | ✗ |  |  |
| 5 | `WarehouseId` | `uuid` | ✗ |  |  |
| 6 | `ReferenceDocumentId` | `uuid` | ✗ | '00000000-0000-0000-0000-000000000000'::... |  |
| 7 | `Note` | `character varying(1000)` | ✓ |  |  |
| 8 | `ExtraProperties` | `text` | ✗ |  |  |
| 9 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 10 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 11 | `CreatorId` | `uuid` | ✓ |  |  |
| 12 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 13 | `LastModifierId` | `uuid` | ✓ |  |  |
| 14 | `IsDeleted` | `boolean` | ✗ | false |  |
| 15 | `DeleterId` | `uuid` | ✓ |  |  |
| 16 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |
| 17 | `ReferenceDocumentNumber` | `text` | ✓ |  |  |

## AppInventoryTransactions

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `WarehouseId` | `uuid` | ✗ |  |  |
| 3 | `BinId` | `uuid` | ✗ |  |  |
| 4 | `ProductId` | `uuid` | ✗ |  |  |
| 5 | `ProductBatchId` | `uuid` | ✗ |  |  |
| 6 | `TransactionType` | `integer(32,0)` | ✗ |  |  |
| 7 | `QuantityChanged` | `numeric(18,2)` | ✗ |  |  |
| 8 | `BalanceAfterTransaction` | `numeric(18,2)` | ✗ |  |  |
| 9 | `ReferenceDocumentId` | `uuid` | ✓ |  |  |
| 10 | `Note` | `character varying(1000)` | ✓ |  |  |
| 11 | `ExtraProperties` | `text` | ✗ |  |  |
| 12 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 13 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 14 | `CreatorId` | `uuid` | ✓ |  |  |
| 16 | `ReferenceDocumentNumber` | `text` | ✓ |  |  |

## AppManufacturers

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Name` | `text` | ✗ |  |  |
| 4 | `ContinentId` | `uuid` | ✗ |  |  |
| 5 | `CountryId` | `uuid` | ✗ |  |  |
| 8 | `ExtraProperties` | `text` | ✗ |  |  |
| 9 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 10 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 11 | `CreatorId` | `uuid` | ✓ |  |  |
| 12 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 13 | `LastModifierId` | `uuid` | ✓ |  |  |
| 14 | `IsDeleted` | `boolean` | ✗ | false |  |
| 15 | `DeleterId` | `uuid` | ✓ |  |  |
| 16 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |
| 17 | `Code` | `text` | ✗ | ''::text |  |

## AppMedicineIngredients

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `MedicineId` | `uuid` | ✗ |  |  |
| 3 | `ActiveIngredientId` | `uuid` | ✗ |  |  |

## AppMedicineRegistrations

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `MedicineId` | `uuid` | ✗ |  |  |
| 3 | `RegistrationNumber` | `character varying(100)` | ✗ |  |  |
| 4 | `ValidFrom` | `timestamp without time zone` | ✓ |  |  |
| 5 | `ValidTo` | `timestamp without time zone` | ✓ |  |  |
| 6 | `IsActive` | `boolean` | ✗ |  |  |
| 7 | `Note` | `text` | ✓ |  |  |
| 8 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 9 | `CreatorId` | `uuid` | ✓ |  |  |
| 10 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 11 | `LastModifierId` | `uuid` | ✓ |  |  |
| 12 | `IsDeleted` | `boolean` | ✗ | false |  |
| 13 | `DeleterId` | `uuid` | ✓ |  |  |
| 14 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |

## AppMedicines

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `DosageFormId` | `uuid` | ✗ |  |  |
| 4 | `UsageRoute` | `integer(32,0)` | ✗ |  |  |
| 5 | `StorageCondition` | `integer(32,0)` | ✗ |  |  |
| 6 | `IsPrescriptionDrug` | `boolean` | ✗ |  |  |
| 8 | `IsActive` | `boolean` | ✗ | false |  |
| 9 | `Status` | `integer(32,0)` | ✗ | 0 |  |

## AppNotifications

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Title` | `character varying(255)` | ✗ |  |  |
| 3 | `Content` | `character varying(2048)` | ✗ |  |  |
| 4 | `Severity` | `integer(32,0)` | ✗ |  |  |
| 5 | `Level` | `integer(32,0)` | ✗ |  |  |
| 7 | `ExtraProperties` | `text` | ✗ |  |  |
| 8 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 9 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 10 | `CreatorId` | `uuid` | ✓ |  |  |
| 11 | `TargetPermissions` | `ARRAY` | ✗ |  |  |

## AppPriceLists

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Code` | `character varying(20)` | ✗ |  |  |
| 3 | `Name` | `character varying(100)` | ✗ |  |  |
| 4 | `Currency` | `integer(32,0)` | ✗ |  |  |
| 5 | `IsBase` | `boolean` | ✗ |  |  |
| 6 | `IsActive` | `boolean` | ✗ |  |  |
| 7 | `ExtraProperties` | `text` | ✗ |  |  |
| 8 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 9 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 10 | `CreatorId` | `uuid` | ✓ |  |  |
| 11 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 12 | `LastModifierId` | `uuid` | ✓ |  |  |
| 13 | `IsDeleted` | `boolean` | ✗ | false |  |
| 14 | `DeleterId` | `uuid` | ✓ |  |  |
| 15 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |

## AppProductBatches

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `ProductId` | `uuid` | ✗ |  |  |
| 3 | `BatchNumber` | `character varying(100)` | ✗ |  |  |
| 4 | `ManufacturingDate` | `timestamp without time zone` | ✗ |  |  |
| 5 | `ExpiryDate` | `timestamp without time zone` | ✗ |  |  |
| 6 | `SupplierId` | `uuid` | ✓ |  |  |
| 7 | `Status` | `integer(32,0)` | ✗ |  |  |
| 8 | `ExtraProperties` | `text` | ✗ |  |  |
| 9 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 10 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 11 | `CreatorId` | `uuid` | ✓ |  |  |
| 12 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 13 | `LastModifierId` | `uuid` | ✓ |  |  |
| 14 | `IsDeleted` | `boolean` | ✗ | false |  |
| 15 | `DeleterId` | `uuid` | ✓ |  |  |
| 16 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |
| 17 | `Code` | `text` | ✗ | ''::text |  |
| 18 | `MedicineRegistrationId` | `uuid` | ✓ |  |  |

## AppProductPrices

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `PriceListId` | `uuid` | ✗ |  |  |
| 3 | `ProductId` | `uuid` | ✗ |  |  |
| 5 | `UnitId` | `uuid` | ✗ |  |  |
| 6 | `Price` | `numeric(18,2)` | ✗ |  |  |
| 7 | `MinQuantity` | `integer(32,0)` | ✗ |  |  |
| 8 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 9 | `CreatorId` | `uuid` | ✓ |  |  |
| 10 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 11 | `LastModifierId` | `uuid` | ✓ |  |  |
| 12 | `IsDeleted` | `boolean` | ✗ | false |  |
| 13 | `DeleterId` | `uuid` | ✓ |  |  |
| 14 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |

## AppProductUnits

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `ProductId` | `uuid` | ✗ |  |  |
| 3 | `UnitId` | `uuid` | ✗ |  |  |
| 4 | `ConversionFactor` | `integer(32,0)` | ✗ |  |  |
| 5 | `Level` | `integer(32,0)` | ✗ |  |  |
| 7 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 8 | `CreatorId` | `uuid` | ✓ |  |  |
| 9 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 10 | `LastModifierId` | `uuid` | ✓ |  |  |

## AppProducts

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `CategoryId` | `uuid` | ✗ |  |  |
| 3 | `ManufacturerId` | `uuid` | ✗ |  |  |
| 4 | `Code` | `text` | ✗ |  |  |
| 5 | `Name` | `text` | ✗ |  |  |
| 6 | `BaseUnitId` | `uuid` | ✗ |  |  |
| 7 | `ProductType` | `integer(32,0)` | ✗ |  |  |
| 8 | `ExtraProperties` | `text` | ✗ |  |  |
| 9 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 10 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 11 | `CreatorId` | `uuid` | ✓ |  |  |
| 12 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 13 | `LastModifierId` | `uuid` | ✓ |  |  |
| 14 | `IsDeleted` | `boolean` | ✗ | false |  |
| 15 | `DeleterId` | `uuid` | ✓ |  |  |
| 16 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |

## AppPurchaseOrderLines

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `PurchaseOrderId` | `uuid` | ✗ |  |  |
| 3 | `ProductId` | `uuid` | ✗ |  |  |
| 4 | `UnitId` | `uuid` | ✗ |  |  |
| 5 | `ConversionFactor` | `integer(32,0)` | ✗ |  |  |
| 6 | `Quantity` | `numeric(18,4)` | ✗ |  |  |
| 7 | `UnitPrice` | `numeric(18,4)` | ✗ |  |  |
| 8 | `TaxRate` | `numeric(5,2)` | ✗ |  |  |
| 9 | `ReceivedQuantity` | `numeric(18,4)` | ✗ |  |  |
| 10 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 11 | `CreatorId` | `uuid` | ✓ |  |  |
| 12 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 13 | `LastModifierId` | `uuid` | ✓ |  |  |

## AppPurchaseOrders

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Code` | `character varying(50)` | ✗ |  |  |
| 3 | `SupplierId` | `uuid` | ✗ |  |  |
| 4 | `OrderDate` | `timestamp without time zone` | ✗ |  |  |
| 5 | `ExpectedDeliveryDate` | `timestamp without time zone` | ✓ |  |  |
| 6 | `Status` | `integer(32,0)` | ✗ |  |  |
| 7 | `SubTotal` | `numeric(18,4)` | ✗ |  |  |
| 8 | `TaxAmount` | `numeric(18,4)` | ✗ |  |  |
| 9 | `TotalAmount` | `numeric(18,4)` | ✗ |  |  |
| 10 | `Note` | `character varying(1000)` | ✓ |  |  |
| 11 | `ExtraProperties` | `text` | ✗ |  |  |
| 12 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 13 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 14 | `CreatorId` | `uuid` | ✓ |  |  |
| 15 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 16 | `LastModifierId` | `uuid` | ✓ |  |  |
| 17 | `IsDeleted` | `boolean` | ✗ | false |  |
| 18 | `DeleterId` | `uuid` | ✓ |  |  |
| 19 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |
| 20 | `DueDate` | `timestamp without time zone` | ✓ |  |  |
| 21 | `WarehouseId` | `uuid` | ✗ | '00000000-0000-0000-0000-000000000000'::... |  |
| 22 | `PurchaseRequisitionId` | `uuid` | ✓ |  |  |

## AppPurchaseRequisitionLines

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `PurchaseRequisitionId` | `uuid` | ✗ |  |  |
| 3 | `ProductId` | `uuid` | ✗ |  |  |
| 4 | `UnitId` | `uuid` | ✗ |  |  |
| 5 | `Quantity` | `numeric(18,4)` | ✗ |  |  |
| 6 | `OrderedQuantity` | `numeric(18,4)` | ✗ |  |  |
| 7 | `Note` | `character varying(500)` | ✓ |  |  |
| 8 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 9 | `CreatorId` | `uuid` | ✓ |  |  |
| 10 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 11 | `LastModifierId` | `uuid` | ✓ |  |  |

## AppPurchaseRequisitions

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Code` | `character varying(50)` | ✗ |  |  |
| 3 | `RequestedDate` | `timestamp without time zone` | ✗ |  |  |
| 4 | `RequiredDate` | `timestamp without time zone` | ✓ |  |  |
| 5 | `Status` | `integer(32,0)` | ✗ |  |  |
| 6 | `Note` | `character varying(1000)` | ✓ |  |  |
| 7 | `ExtraProperties` | `text` | ✗ |  |  |
| 8 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 9 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 10 | `CreatorId` | `uuid` | ✓ |  |  |
| 11 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 12 | `LastModifierId` | `uuid` | ✓ |  |  |
| 13 | `IsDeleted` | `boolean` | ✗ | false |  |
| 14 | `DeleterId` | `uuid` | ✓ |  |  |
| 15 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |
| 16 | `WarehouseId` | `uuid` | ✗ | '00000000-0000-0000-0000-000000000000'::... |  |

## AppPurchaseReturnLines

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `PurchaseReturnId` | `uuid` | ✗ |  |  |
| 3 | `PurchaseOrderLineId` | `uuid` | ✗ |  |  |
| 4 | `ProductId` | `uuid` | ✗ |  |  |
| 5 | `UnitId` | `uuid` | ✗ |  |  |
| 6 | `ConversionFactor` | `integer(32,0)` | ✗ |  |  |
| 7 | `Quantity` | `numeric(18,4)` | ✗ |  |  |
| 8 | `OriginalUnitPrice` | `numeric(18,4)` | ✗ |  |  |
| 9 | `DepreciationRate` | `numeric(5,2)` | ✗ |  |  |
| 10 | `TaxRate` | `numeric(5,2)` | ✗ |  |  |
| 11 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 12 | `CreatorId` | `uuid` | ✓ |  |  |
| 13 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 14 | `LastModifierId` | `uuid` | ✓ |  |  |

## AppPurchaseReturnRequestLines

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `PurchaseReturnRequestId` | `uuid` | ✗ |  |  |
| 4 | `ProductId` | `uuid` | ✗ |  |  |
| 6 | `UnitId` | `uuid` | ✗ |  |  |
| 8 | `ConversionFactor` | `integer(32,0)` | ✗ |  |  |
| 9 | `PurchaseOrderId` | `uuid` | ✗ |  |  |
| 10 | `PurchaseOrderLineId` | `uuid` | ✗ |  |  |
| 11 | `Quantity` | `numeric(18,4)` | ✗ |  |  |
| 12 | `BaseQuantity` | `numeric(18,4)` | ✗ |  |  |
| 13 | `OriginalUnitPrice` | `numeric(18,4)` | ✗ |  |  |
| 14 | `DepreciationRate` | `numeric(5,2)` | ✗ |  |  |
| 15 | `ReturnUnitPrice` | `numeric(18,4)` | ✗ |  |  |
| 16 | `TaxRate` | `numeric(5,2)` | ✗ |  |  |
| 17 | `TotalPrice` | `numeric(18,4)` | ✗ |  |  |
| 18 | `TaxAmount` | `numeric(18,4)` | ✗ |  |  |
| 19 | `FinalPrice` | `numeric(18,4)` | ✗ |  |  |
| 20 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 21 | `CreatorId` | `uuid` | ✓ |  |  |
| 22 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 23 | `LastModifierId` | `uuid` | ✓ |  |  |

## AppPurchaseReturnRequests

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Code` | `character varying(50)` | ✗ |  |  |
| 3 | `SupplierId` | `uuid` | ✗ |  |  |
| 4 | `WarehouseId` | `uuid` | ✗ |  |  |
| 5 | `ReturnType` | `integer(32,0)` | ✗ |  |  |
| 6 | `RequestDate` | `timestamp without time zone` | ✗ |  |  |
| 7 | `Status` | `integer(32,0)` | ✗ |  |  |
| 8 | `SubTotal` | `numeric(18,4)` | ✗ |  |  |
| 9 | `TaxAmount` | `numeric(18,4)` | ✗ |  |  |
| 10 | `TotalAmount` | `numeric(18,4)` | ✗ |  |  |
| 11 | `Note` | `character varying(1000)` | ✓ |  |  |
| 12 | `ExtraProperties` | `text` | ✗ |  |  |
| 13 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 14 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 15 | `CreatorId` | `uuid` | ✓ |  |  |
| 16 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 17 | `LastModifierId` | `uuid` | ✓ |  |  |
| 18 | `IsDeleted` | `boolean` | ✗ | false |  |
| 19 | `DeleterId` | `uuid` | ✓ |  |  |
| 20 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |

## AppPurchaseReturns

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Code` | `character varying(50)` | ✗ |  |  |
| 3 | `PurchaseOrderId` | `uuid` | ✗ |  |  |
| 4 | `SupplierId` | `uuid` | ✗ |  |  |
| 5 | `WarehouseId` | `uuid` | ✗ |  |  |
| 6 | `ReturnDate` | `timestamp without time zone` | ✗ |  |  |
| 7 | `Status` | `integer(32,0)` | ✗ |  |  |
| 8 | `SubTotal` | `numeric(18,4)` | ✗ |  |  |
| 9 | `TaxAmount` | `numeric(18,4)` | ✗ |  |  |
| 10 | `TotalAmount` | `numeric(18,4)` | ✗ |  |  |
| 11 | `Note` | `character varying(1000)` | ✓ |  |  |
| 12 | `ExtraProperties` | `text` | ✗ |  |  |
| 13 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 14 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 15 | `CreatorId` | `uuid` | ✓ |  |  |
| 16 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 17 | `LastModifierId` | `uuid` | ✓ |  |  |
| 18 | `IsDeleted` | `boolean` | ✗ | false |  |
| 19 | `DeleterId` | `uuid` | ✓ |  |  |
| 20 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |
| 21 | `PurchaseReturnRequestId` | `uuid` | ✓ |  |  |

## AppSalesOrderLines

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `SalesOrderId` | `uuid` | ✗ |  |  |
| 3 | `ProductId` | `uuid` | ✗ |  |  |
| 4 | `UnitId` | `uuid` | ✗ |  |  |
| 5 | `ConversionFactor` | `integer(32,0)` | ✗ |  |  |
| 6 | `Quantity` | `numeric(18,4)` | ✗ |  |  |
| 7 | `DeliveredQuantity` | `numeric(18,4)` | ✗ |  |  |
| 8 | `UnitPrice` | `numeric(18,4)` | ✗ |  |  |
| 9 | `DiscountRate` | `numeric(5,2)` | ✗ |  |  |
| 10 | `TaxRate` | `numeric(5,2)` | ✗ |  |  |
| 11 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 12 | `CreatorId` | `uuid` | ✓ |  |  |
| 13 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 14 | `LastModifierId` | `uuid` | ✓ |  |  |

## AppSalesOrders

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Code` | `character varying(50)` | ✗ |  |  |
| 3 | `CustomerId` | `uuid` | ✗ |  |  |
| 4 | `OrderDate` | `timestamp without time zone` | ✗ |  |  |
| 5 | `ExpectedDeliveryDate` | `timestamp without time zone` | ✓ |  |  |
| 6 | `DueDate` | `timestamp without time zone` | ✓ |  |  |
| 7 | `Status` | `integer(32,0)` | ✗ |  |  |
| 8 | `SubTotal` | `numeric(18,4)` | ✗ |  |  |
| 9 | `DiscountAmount` | `numeric(18,4)` | ✗ |  |  |
| 10 | `TaxAmount` | `numeric(18,4)` | ✗ |  |  |
| 11 | `TotalAmount` | `numeric(18,4)` | ✗ |  |  |
| 12 | `Note` | `character varying(1000)` | ✓ |  |  |
| 13 | `WarehouseId` | `uuid` | ✗ |  |  |
| 14 | `ExtraProperties` | `text` | ✗ |  |  |
| 15 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 16 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 17 | `CreatorId` | `uuid` | ✓ |  |  |
| 18 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 19 | `LastModifierId` | `uuid` | ✓ |  |  |
| 20 | `IsDeleted` | `boolean` | ✗ | false |  |
| 21 | `DeleterId` | `uuid` | ✓ |  |  |
| 22 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |

## AppSalesRecallLines

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `SalesRecallId` | `uuid` | ✗ |  |  |
| 3 | `CustomerId` | `uuid` | ✗ |  |  |
| 4 | `SalesOrderId` | `uuid` | ✗ |  |  |
| 5 | `UnitId` | `uuid` | ✗ |  |  |
| 6 | `ConversionFactor` | `integer(32,0)` | ✗ |  |  |
| 7 | `Quantity` | `numeric(18,4)` | ✗ |  |  |
| 8 | `OriginalUnitPrice` | `numeric(18,4)` | ✗ |  |  |
| 9 | `TaxRate` | `numeric(5,2)` | ✗ |  |  |
| 10 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 11 | `CreatorId` | `uuid` | ✓ |  |  |
| 12 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 13 | `LastModifierId` | `uuid` | ✓ |  |  |

## AppSalesRecalls

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Code` | `character varying(50)` | ✗ |  |  |
| 3 | `RecallDecisionNumber` | `character varying(256)` | ✗ |  |  |
| 4 | `ProductId` | `uuid` | ✗ |  |  |
| 5 | `ProductBatchId` | `uuid` | ✓ |  |  |
| 6 | `WarehouseId` | `uuid` | ✗ |  |  |
| 7 | `RecallDate` | `timestamp without time zone` | ✗ |  |  |
| 8 | `Level` | `integer(32,0)` | ✗ |  |  |
| 9 | `Deadline` | `timestamp without time zone` | ✗ |  |  |
| 10 | `Status` | `integer(32,0)` | ✗ |  |  |
| 11 | `TotalAmount` | `numeric(18,4)` | ✗ |  |  |
| 12 | `Note` | `character varying(1000)` | ✓ |  |  |
| 13 | `ExtraProperties` | `text` | ✗ |  |  |
| 14 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 15 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 16 | `CreatorId` | `uuid` | ✓ |  |  |
| 17 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 18 | `LastModifierId` | `uuid` | ✓ |  |  |
| 19 | `IsDeleted` | `boolean` | ✗ | false |  |
| 20 | `DeleterId` | `uuid` | ✓ |  |  |
| 21 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |

## AppSupplierProductConditions

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `SupplierProductId` | `uuid` | ✗ |  |  |
| 3 | `UnitId` | `uuid` | ✗ |  |  |
| 4 | `ConversionFactor` | `integer(32,0)` | ✗ |  |  |
| 5 | `StandardPrice` | `numeric` | ✗ |  |  |
| 6 | `LastPurchasePrice` | `numeric` | ✗ |  |  |
| 7 | `MinOrderQuantity` | `numeric` | ✗ |  |  |
| 8 | `OverDeliveryTolerancePct` | `numeric` | ✗ |  |  |
| 9 | `UnderDeliveryTolerancePct` | `numeric` | ✗ |  |  |

## AppSupplierProducts

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `SupplierId` | `uuid` | ✗ |  |  |
| 3 | `ProductId` | `uuid` | ✗ |  |  |
| 4 | `DefaultUnitId` | `uuid` | ✗ |  |  |
| 8 | `LeadTimeDays` | `integer(32,0)` | ✗ |  |  |
| 12 | `IsPreferred` | `boolean` | ✗ |  |  |
| 13 | `IsActive` | `boolean` | ✗ |  |  |
| 14 | `Note` | `text` | ✓ |  |  |
| 15 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 16 | `CreatorId` | `uuid` | ✓ |  |  |
| 17 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 18 | `LastModifierId` | `uuid` | ✓ |  |  |

## AppSuppliers

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Code` | `text` | ✗ |  |  |
| 3 | `Name` | `text` | ✗ |  |  |
| 4 | `TaxCode` | `text` | ✓ |  |  |
| 5 | `PhoneNumber` | `text` | ✓ |  |  |
| 6 | `Email` | `text` | ✓ |  |  |
| 7 | `RepresentativeName` | `text` | ✓ |  |  |
| 8 | `Note` | `text` | ✓ |  |  |
| 9 | `IsActive` | `boolean` | ✗ |  |  |
| 10 | `Address` | `text` | ✓ |  |  |
| 11 | `DebtLimit` | `numeric` | ✗ |  |  |
| 12 | `PaymentTermDays` | `integer(32,0)` | ✗ |  |  |
| 13 | `CurrentDebt` | `numeric` | ✗ |  |  |
| 14 | `CountryId` | `uuid` | ✓ |  |  |
| 15 | `CityId` | `uuid` | ✓ |  |  |
| 16 | `AreaId` | `uuid` | ✓ |  |  |
| 17 | `ExtraProperties` | `text` | ✗ |  |  |
| 18 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 19 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 20 | `CreatorId` | `uuid` | ✓ |  |  |
| 21 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 22 | `LastModifierId` | `uuid` | ✓ |  |  |
| 23 | `IsDeleted` | `boolean` | ✗ | false |  |
| 24 | `DeleterId` | `uuid` | ✓ |  |  |
| 25 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |
| 26 | `Gender` | `integer(32,0)` | ✓ |  |  |

## AppUserNotifications

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `NotificationId` | `uuid` | ✗ |  |  |
| 3 | `UserId` | `uuid` | ✗ |  |  |
| 4 | `IsRead` | `boolean` | ✗ |  |  |
| 5 | `ReadAt` | `timestamp without time zone` | ✓ |  |  |
| 6 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 7 | `CreatorId` | `uuid` | ✓ |  |  |
| 8 | `IsDelete` | `boolean` | ✗ | false |  |

## AppWarehouses

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `Code` | `character varying(50)` | ✗ |  |  |
| 3 | `Name` | `character varying(255)` | ✗ |  |  |
| 4 | `Address` | `character varying(500)` | ✓ |  |  |
| 5 | `CityId` | `uuid` | ✓ |  |  |
| 6 | `AreaId` | `uuid` | ✓ |  |  |
| 7 | `MapWidth` | `integer(32,0)` | ✗ |  |  |
| 8 | `MapLength` | `integer(32,0)` | ✗ |  |  |
| 9 | `Status` | `integer(32,0)` | ✗ |  |  |
| 10 | `IsActive` | `boolean` | ✗ |  |  |
| 11 | `ExtraProperties` | `text` | ✗ |  |  |
| 12 | `ConcurrencyStamp` | `character varying(40)` | ✗ |  |  |
| 13 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 14 | `CreatorId` | `uuid` | ✓ |  |  |
| 15 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 16 | `LastModifierId` | `uuid` | ✓ |  |  |
| 17 | `IsDeleted` | `boolean` | ✗ | false |  |
| 18 | `DeleterId` | `uuid` | ✓ |  |  |
| 19 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |
| 20 | `CountryId` | `uuid` | ✓ |  |  |

## AppZones

| # | Column | Type | Nullable | Default | Comment |
|---|--------|------|----------|---------|---------|
| 1 | `Id` | `uuid` | ✗ |  |  |
| 2 | `WarehouseId` | `uuid` | ✗ |  |  |
| 3 | `Code` | `character varying(50)` | ✗ |  |  |
| 4 | `Name` | `character varying(255)` | ✗ |  |  |
| 5 | `Type` | `integer(32,0)` | ✗ |  |  |
| 6 | `StorageCondition` | `integer(32,0)` | ✗ |  |  |
| 7 | `Color` | `character varying(20)` | ✗ |  |  |
| 8 | `PositionX` | `integer(32,0)` | ✗ |  |  |
| 9 | `PositionY` | `integer(32,0)` | ✗ |  |  |
| 10 | `Width` | `integer(32,0)` | ✗ |  |  |
| 11 | `Length` | `integer(32,0)` | ✗ |  |  |
| 12 | `Rotation` | `real` | ✗ |  |  |
| 15 | `CreationTime` | `timestamp without time zone` | ✗ |  |  |
| 16 | `CreatorId` | `uuid` | ✓ |  |  |
| 17 | `LastModificationTime` | `timestamp without time zone` | ✓ |  |  |
| 18 | `LastModifierId` | `uuid` | ✓ |  |  |
| 19 | `IsDeleted` | `boolean` | ✗ | false |  |
| 20 | `DeleterId` | `uuid` | ✓ |  |  |
| 21 | `DeletionTime` | `timestamp without time zone` | ✓ |  |  |