import type { FullAuditedEntityDto } from '@abp/ng.core';

export interface BaseUnitDto extends FullAuditedEntityDto<string> {
  code?: string;
  name?: string;
}

export interface CreateUpdateBaseUnitDto {
  code: string;
  name: string;
}

export interface GetBaseUnitListDto {
  filter?: string;
}
