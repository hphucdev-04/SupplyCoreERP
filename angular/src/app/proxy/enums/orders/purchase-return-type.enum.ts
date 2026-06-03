import { mapEnumToOptions } from '@abp/ng.core';

export enum PurchaseReturnType {
  Defective = 1,
  Commercial = 2,
}

export const purchaseReturnTypeOptions = mapEnumToOptions(PurchaseReturnType);
