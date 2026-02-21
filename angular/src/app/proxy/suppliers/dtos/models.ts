import type { Gender } from '../../enums/partner/gender.enum';
import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateSupplierDto {
  code: string;
  name: string;
  taxCode?: string;
  phoneNumber?: string;
  email?: string;
  representativeName?: string;
  gender?: Gender;
  note?: string;
  address?: string;
  countryId?: string;
  cityId?: string;
  areaId?: string;
  debtLimit?: number;
  paymentTermDays?: number;
  isActive?: boolean;
}

export interface GetSupplierListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  isActive?: boolean;
}

export interface SupplierDetailDto extends SupplierDto {
  taxCode?: string;
  representativeName?: string;
  note?: string;
  address?: string;
  countryId?: string;
  countryName?: string;
  cityId?: string;
  areaId?: string;
  areaName?: string;
  debtLimit?: number;
  paymentTermDays?: number;
  gender?: Gender;
}

export interface SupplierDto extends FullAuditedEntityDto<string> {
  code?: string;
  name?: string;
  phoneNumber?: string;
  email?: string;
  cityName?: string;
  currentDebt?: number;
  isActive?: boolean;
}
