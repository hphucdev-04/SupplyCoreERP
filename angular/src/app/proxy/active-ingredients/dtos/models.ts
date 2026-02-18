import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface ActiveIngredientDto extends FullAuditedEntityDto<string> {
  code?: string;
  name?: string;
}

export interface CreateUpdateActiveIngredientDto {
  code: string;
  name: string;
}

export interface GetActiveIngredientListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
}
