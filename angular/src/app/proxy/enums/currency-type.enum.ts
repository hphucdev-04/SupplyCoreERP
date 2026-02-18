import { mapEnumToOptions } from '@abp/ng.core';

export enum CurrencyType {
  VND = 0,
  USD = 1,
  EUR = 2,
}

export const currencyTypeOptions = mapEnumToOptions(CurrencyType);
