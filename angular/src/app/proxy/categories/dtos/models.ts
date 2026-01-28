import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CategoryDto extends FullAuditedEntityDto<string> {
  name?: string;
  productCount?: number;
}

export interface CreateUpdateCategoryDto {
  name: string;
}

export interface GetCategoryListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
}
