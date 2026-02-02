import { mapEnumToOptions } from '@abp/ng.core';

export enum UsageRoute {
  Oral = 1,
  Injection = 2,
  External = 3,
  Other = 4,
}

export const usageRouteOptions = mapEnumToOptions(UsageRoute);
