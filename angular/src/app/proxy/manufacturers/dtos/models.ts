import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateManufacturerDto {
  name: string;
  continentId: string;
  countryId: string;
}

export interface GetManufacturerListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
}

export interface ManufacturerDto extends FullAuditedEntityDto<string> {
  name?: string;
  continentId?: string;
  continentName?: string;
  countryId?: string;
  countryName?: string;
}
