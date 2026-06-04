# SupplyCoreERP Database Schema

Below is the database schema for the main tables in the system. This schema helps you understand how data is linked when writing queries.

## 1. Table: AppProducts (Products / Medicines)
- Id: UUID (Primary Key)
- Code: VARCHAR (Product code, e.g., MD2605260001, SP001)
- Name: VARCHAR (Product name, e.g., Panadol)
- BaseUnitId: UUID (Foreign key linking to AppBaseUnits.Id)
- IsDeleted: BOOLEAN (Deletion status, default false)

## 2. Table: AppWarehouses (Warehouses)
- Id: UUID (Primary Key)
- Code: VARCHAR (Warehouse code, e.g., KHO_HCM)
- Name: VARCHAR (Warehouse name)
- Address: VARCHAR (Warehouse address)
- IsDeleted: BOOLEAN

## 3. Table: AppInventoryBalances (Physical Stock Inventory)
- Id: UUID (Primary Key)
- ProductId: UUID (Foreign key linking to AppProducts.Id)
- WarehouseId: UUID (Foreign key linking to AppWarehouses.Id)
- Quantity: NUMERIC (Physical stock quantity)
- IsDeleted: BOOLEAN

## 4. Table: AppSuppliers (Suppliers)
- Id: UUID (Primary Key)
- Code: VARCHAR (Supplier code)
- Name: VARCHAR (Supplier name)
- PhoneNumber: VARCHAR (Phone number)
- Email: VARCHAR (Email address)
- IsDeleted: BOOLEAN

## 5. Table: AppCustomers (Customers)
- Id: UUID (Primary Key)
- Code: VARCHAR (Customer code)
- Name: VARCHAR (Customer name)
- PhoneNumber: VARCHAR (Phone number)
- IsDeleted: BOOLEAN

## 6. Table: AppProductBatches (Product Batches / Lots)
- Id: UUID (Primary Key)
- Code: VARCHAR (Batch management code)
- BatchNumber: VARCHAR (Product batch number, e.g., LOT123)
- ExpiryDate: TIMESTAMP (Expiration date)
- Status: VARCHAR (Batch status)
- IsDeleted: BOOLEAN

## 7. Table: AppBaseUnits (Base Units of Measure)
- Id: UUID (Primary Key)
- Code: VARCHAR (Unit code)
- Name: VARCHAR (Unit name, e.g., Box, Bottle, Tablet)
- IsDeleted: BOOLEAN

---
## Foreign Key Relationships
- AppProducts.BaseUnitId -> AppBaseUnits.Id (Each product has a base unit of measure)
- AppInventoryBalances.ProductId -> AppProducts.Id (Links inventory balance to its product)
- AppInventoryBalances.WarehouseId -> AppWarehouses.Id (Links inventory balance to its warehouse)
