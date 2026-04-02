import { mapEnumToOptions } from '@abp/ng.core';

export enum InventoryTransactionType {
  PurchaseReceipt = 0,
  SaleDelivery = 1,
  ReturnInward = 2,
  ReturnOutward = 3,
  RecallReceipt = 4,
  Disposal = 5,
  AdjustmentIn = 6,
  AdjustmentOut = 7,
  TransferIn = 8,
  TransferOut = 9,
}

export const inventoryTransactionTypeOptions = mapEnumToOptions(InventoryTransactionType);
