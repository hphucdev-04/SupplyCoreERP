import { mapEnumToOptions } from '@abp/ng.core';

export enum TicketType {
  GoodsReceipt = 0,
  GoodsIssue = 1,
  ReturnInward = 2,
  ReturnOutward = 3,
  RecallReceipt = 4,
  DisposalIssue = 5,
}

export const ticketTypeOptions = mapEnumToOptions(TicketType);
