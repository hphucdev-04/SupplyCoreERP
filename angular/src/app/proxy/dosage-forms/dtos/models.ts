import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateDosageFormDto {
  code: string;
  name: string;
}

export interface DosageFormDto extends FullAuditedEntityDto<string> {
  code?: string;
  name?: string;
}

export interface GetDosageFormListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
}
