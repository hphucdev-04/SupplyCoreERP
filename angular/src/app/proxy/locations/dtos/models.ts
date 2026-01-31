import type { EntityDto } from '@abp/ng.core';

export interface AreaDto extends EntityDto<string> {
  cityId?: string;
  zipCode?: string;
  name?: string;
}

export interface CityDto extends EntityDto<string> {
  countryId?: string;
  name?: string;
}

export interface ContinentDto extends EntityDto<string> {
  name?: string;
}

export interface CountryDto extends EntityDto<string> {
  continentId?: string;
  iso?: string;
  name?: string;
}
