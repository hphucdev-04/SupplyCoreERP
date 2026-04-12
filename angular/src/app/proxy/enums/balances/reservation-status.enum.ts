import { mapEnumToOptions } from '@abp/ng.core';

export enum ReservationStatus {
  Active = 1,
  Completed = 2,
  Cancelled = 3,
}

export const reservationStatusOptions = mapEnumToOptions(ReservationStatus);
