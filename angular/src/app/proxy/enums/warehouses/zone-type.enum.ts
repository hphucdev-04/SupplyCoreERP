import { mapEnumToOptions } from '@abp/ng.core';

export enum ZoneType {
  Storage = 0,
  Inbound = 1,
  Outbound = 2,
  Staging = 3,
  Quarantine = 4,
  ForkliftParking = 5,
  Office = 6,
  QA = 7,
}

export const zoneTypeOptions = mapEnumToOptions(ZoneType);
