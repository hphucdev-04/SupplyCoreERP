import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { StorageCondition } from '../../enums/medicines/storage-condition.enum';
import type { ZoneType } from '../../enums/warehouses/zone-type.enum';
import type { ApprovalStatus } from '../../enums/warehouses/approval-status.enum';

export interface BinDto extends EntityDto<string> {
  warehouseId?: string;
  zoneId?: string;
  zoneName?: string;
  zoneStorageCondition?: StorageCondition;
  code?: string;
  positionX?: number;
  positionY?: number;
  width?: number;
  length?: number;
  rotation?: number;
  maxWeight?: number;
  isBlocked?: boolean;
}

export interface CreateUpdateBinDto {
  warehouseId: string;
  zoneId: string;
  code: string;
  positionX?: number;
  positionY?: number;
  width?: number;
  length?: number;
  rotation?: number;
  maxSKU?: number;
  isBlocked?: boolean;
}

export interface CreateUpdateWarehouseDto {
  code: string;
  name: string;
  address?: string;
  cityId?: string;
  areaId?: string;
  mapWidth?: number;
  mapLength?: number;
}

export interface CreateUpdateZoneDto {
  warehouseId: string;
  code: string;
  name: string;
  type: ZoneType;
  storageCondition: StorageCondition;
  color?: string;
  positionX?: number;
  positionY?: number;
  width?: number;
  length?: number;
  rotation?: number;
}

export interface GetWarehouseListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  status?: ApprovalStatus;
  isActive?: boolean;
}

export interface WarehouseDto extends FullAuditedEntityDto<string> {
  code?: string;
  name?: string;
  address?: string;
  cityId?: string;
  cityName?: string;
  areaId?: string;
  areaName?: string;
  mapWidth?: number;
  mapLength?: number;
  status?: ApprovalStatus;
  isActive?: boolean;
}

export interface ZoneDto extends EntityDto<string> {
  warehouseId?: string;
  code?: string;
  name?: string;
  type?: ZoneType;
  storageCondition?: StorageCondition;
  color?: string;
  positionX?: number;
  positionY?: number;
  width?: number;
  length?: number;
  rotation?: number;
}
