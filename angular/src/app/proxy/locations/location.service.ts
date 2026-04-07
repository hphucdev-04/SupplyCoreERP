import type { AreaDto, CityDto, ContinentDto, CountryDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LocationService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  getAllCities = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<CityDto>>({
      method: 'GET',
      url: '/api/app/location/cities',
    },
    { apiName: this.apiName,...config });
  

  getAllCountries = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<CountryDto>>({
      method: 'GET',
      url: '/api/app/location/countries',
    },
    { apiName: this.apiName,...config });
  

  getAreasByCity = (cityId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<AreaDto>>({
      method: 'GET',
      url: `/api/app/location/areas-by-city/${cityId}`,
    },
    { apiName: this.apiName,...config });
  

  getCitiesByCountry = (countryId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<CityDto>>({
      method: 'GET',
      url: `/api/app/location/cities-by-country/${countryId}`,
    },
    { apiName: this.apiName,...config });
  

  getContinents = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ContinentDto>>({
      method: 'GET',
      url: '/api/app/location/continents',
    },
    { apiName: this.apiName,...config });
  

  getCountriesByContinent = (continentId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<CountryDto>>({
      method: 'GET',
      url: `/api/app/location/countries-by-continent/${continentId}`,
    },
    { apiName: this.apiName,...config });
}