import { mapEnumToOptions } from '@abp/ng.core';

export enum MedicineStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3,
}

export const medicineStatusOptions = mapEnumToOptions(MedicineStatus);
