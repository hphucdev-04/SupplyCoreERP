import type { Gender } from '../../enums/partner/gender.enum';
import type { CustomerType } from '../../enums/partner/customer-type.enum';
import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateCustomerDto {
  code: string;
  name: string;
  phoneNumber?: string;
  email?: string;
  representativeName?: string;
  gender?: Gender;
  type?: CustomerType;
  taxCode?: string;
  address?: string;
  countryId?: string;
  cityId?: string;
  areaId?: string;
  note?: string;
  debtLimit?: number;
  paymentTermDays?: number;
  isActive?: boolean;
}

export interface CustomerDetailDto extends CustomerDto {
  email?: string;
  representativeName?: string;
  gender?: Gender;
  taxCode?: string;
  note?: string;
  address?: string;
  countryId?: string;
  countryName?: string;
  cityId?: string;
  areaId?: string;
  areaName?: string;
  debtLimit?: number;
  paymentTermDays?: number;
}

export interface CustomerDto extends FullAuditedEntityDto<string> {
  code?: string;
  name?: string;
  phoneNumber?: string;
  type?: CustomerType;
  cityName?: string;
  currentDebt?: number;
  isActive?: boolean;
}

export interface GetCustomerListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  isActive?: boolean;
}
