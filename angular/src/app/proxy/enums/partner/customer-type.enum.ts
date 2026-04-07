import { mapEnumToOptions } from '@abp/ng.core';

export enum CustomerType {
  Individual = 0,
  Organization = 1,
}

export const customerTypeOptions = mapEnumToOptions(CustomerType);
