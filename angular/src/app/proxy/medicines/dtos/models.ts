import type { UsageRoute } from '../../enums/medicines/usage-route.enum';
import type { StorageCondition } from '../../enums/medicines/storage-condition.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { MedicineStatus } from '../../enums/medicines/medicine-status.enum';

export interface CreateUpdateMedicineDto {
  code: string;
  name: string;
  categoryId: string;
  manufacturerId: string;
  baseUnitId: string;
  dosageFormId: string;
  registrationNumber?: string;
  usageRoute?: UsageRoute;
  storageCondition?: StorageCondition;
  isPrescriptionDrug?: boolean;
  isActive?: boolean;
}

export interface CreateUpdateMedicineIngredientDto {
  activeIngredientId: string;
}

export interface CreateUpdateMedicineUnitDto {
  unitId: string;
  conversionFactor?: number;
  level: number;
}

export interface GetMedicineListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  categoryId?: string;
  manufacturerId?: string;
  status?: MedicineStatus;
  isActive?: boolean;
}

export interface MedicineDetailDto extends MedicineDto {
  categoryId?: string;
  manufacturerId?: string;
  baseUnitId?: string;
  dosageFormId?: string;
  originCountryId?: string;
  registrationNumber?: string;
  usageRoute?: UsageRoute;
  storageCondition?: StorageCondition;
  isPrescriptionDrug?: boolean;
  ingredients?: MedicineIngredientDto[];
  units?: MedicineUnitDto[];
}

export interface MedicineDto extends EntityDto<string> {
  code?: string;
  name?: string;
  categoryName?: string;
  manufacturerName?: string;
  baseUnitName?: string;
  dosageFormName?: string;
  originCountryName?: string;
  status?: MedicineStatus;
  isActive?: boolean;
  creationTime?: string;
}

export interface MedicineIngredientDto {
  activeIngredientId?: string;
  activeIngredientName?: string;
  activeIngredientCode?: string;
}

export interface MedicineUnitDto {
  unitId?: string;
  unitName?: string;
  conversionFactor?: number;
  level?: number;
}
