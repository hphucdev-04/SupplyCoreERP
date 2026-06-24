import type { Gender } from '../../enums/partner/gender.enum';
import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateSupplierDto {
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

export interface CreateUpdateSupplierProductConditionDto {
  id?: string;
  unitId: string;
  conversionFactor?: number;
  standardPrice?: number;
  minOrderQuantity?: number;
}

export interface CreateUpdateSupplierProductDto {
  productId: string;
  defaultUnitId: string;
  leadTimeDays?: number;
  isPreferred?: boolean;
  note?: string;
  conditions?: CreateUpdateSupplierProductConditionDto[];
}

export interface GetSupplierListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  isActive?: boolean;
}

export interface GetSupplierMedicineListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
}

export interface GetSupplierProductListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  isPreferred?: boolean;
  isActive?: boolean;
  minPrice?: number;
  maxPrice?: number;
}

export interface SourcingSuggestionDto {
  productId?: string;
  supplierId?: string;
  supplierName?: string;
  score?: number;
}

export interface SupplierDetailDto extends SupplierDto {
  taxCode?: string;
  representativeName?: string;
  gender?: Gender;
  note?: string;
  address?: string;
  areaId?: string;
  areaName?: string;
  debtLimit?: number;
  paymentTermDays?: number;
}

export interface SupplierDto extends FullAuditedEntityDto<string> {
  code?: string;
  name?: string;
  phoneNumber?: string;
  email?: string;
  countryId?: string;
  countryName?: string;
  cityId?: string;
  cityName?: string;
  currentDebt?: number;
  isActive?: boolean;
}

export interface SupplierMedicineDto {
  supplierId?: string;
  supplierCode?: string;
  supplierName?: string;
  countryId?: string;
  countryName?: string;
  standardPrice?: number;
  leadTimeDays?: number;
  minOrderQuantity?: number;
  defaultUnitName?: string;
  isPreferred?: boolean;
}

export interface SupplierProductConditionDto extends EntityDto<string> {
  supplierProductId?: string;
  unitId?: string;
  unitName?: string;
  conversionFactor?: number;
  standardPrice?: number;
  lastPurchasePrice?: number;
  minOrderQuantity?: number;
}

export interface SupplierProductDto extends EntityDto<string> {
  supplierId?: string;
  productId?: string;
  productName?: string;
  productCode?: string;
  defaultUnitId?: string;
  defaultUnitName?: string;
  leadTimeDays?: number;
  isPreferred?: boolean;
  isActive?: boolean;
  note?: string;
  conditions?: SupplierProductConditionDto[];
}
