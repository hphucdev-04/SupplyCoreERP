import { mapEnumToOptions } from '@abp/ng.core';

export enum CustomerType {
  Individual = 1,
  Organization = 2,
}

export const customerTypeOptions = mapEnumToOptions(CustomerType);
