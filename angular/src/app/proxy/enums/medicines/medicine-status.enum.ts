import { mapEnumToOptions } from '@abp/ng.core';

export enum MedicineStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
}

export const medicineStatusOptions = mapEnumToOptions(MedicineStatus);
