
export interface DashboardBatchLookupDto {
  id?: string;
  batchNumber?: string;
  medicineName?: string;
}

export interface DashboardBatchQAStatusDto {
  statusName?: string;
  count?: number;
  percentage?: number;
}

export interface DashboardBatchTraceBalanceDto {
  warehouseName?: string;
  binCode?: string;
  quantity?: number;
}

export interface DashboardBatchTraceDeliveryDto {
  customerName?: string;
  ticketNumber?: string;
  soNumber?: string;
  date?: string;
  quantity?: number;
}

export interface DashboardBatchTraceDto {
  batchId?: string;
  batchNumber?: string;
  medicineCode?: string;
  medicineName?: string;
  manufacturingDate?: string;
  expiryDate?: string;
  status?: string;
  supplierName?: string;
  totalOnHand?: number;
  totalReserved?: number;
  balances?: DashboardBatchTraceBalanceDto[];
  receipts?: DashboardBatchTraceReceiptDto[];
  deliveries?: DashboardBatchTraceDeliveryDto[];
  otherTransactions?: DashboardBatchTraceOtherDto[];
}

export interface DashboardBatchTraceOtherDto {
  transactionType?: string;
  ticketNumber?: string;
  date?: string;
  quantity?: number;
  note?: string;
}

export interface DashboardBatchTraceReceiptDto {
  supplierName?: string;
  ticketNumber?: string;
  poNumber?: string;
  date?: string;
  quantity?: number;
}

export interface DashboardCategoryDistributionDto {
  categoryName?: string;
  totalQuantity?: number;
  percentage?: number;
}

export interface DashboardDebtOverviewDto {
  totalReceivableDebt?: number;
  totalPayableDebt?: number;
  totalCustomers?: number;
  totalSuppliers?: number;
}

export interface DashboardExpiredBatchDto {
  medicineName?: string;
  batchNumber?: string;
  warehouseName?: string;
  quantity?: number;
  expiryDate?: string;
  daysRemaining?: number;
}

export interface DashboardFilterInput {
  warehouseId?: string;
  days?: number;
  categoryId?: string;
}

export interface DashboardFinancialTrendDto {
  date?: string;
  salesAmount?: number;
  procurementAmount?: number;
}

export interface DashboardInventoryTicketStatusDto {
  statusName?: string;
  count?: number;
  percentage?: number;
}

export interface DashboardInventoryTransactionDto {
  transactionTypeName?: string;
  count?: number;
  percentage?: number;
}

export interface DashboardOverviewDto {
  totalWarehouses?: number;
  totalMedicines?: number;
  averageCapacityPercent?: number;
  expiredAlertCount?: number;
  totalRevenue?: number;
  totalProcurement?: number;
  totalSalesRecall?: number;
  totalPurchaseReturn?: number;
  totalReservedVolume?: number;
  totalAvailableVolume?: number;
}

export interface DashboardPartnerDebtDto {
  partnerCode?: string;
  partnerName?: string;
  currentDebt?: number;
}

export interface DashboardPhysicalMovementTrendDto {
  date?: string;
  inboundVolume?: number;
  outboundVolume?: number;
}

export interface DashboardProcurementStatusDto {
  statusName?: string;
  count?: number;
  percentage?: number;
}

export interface DashboardSalesStatusDto {
  statusName?: string;
  count?: number;
  percentage?: number;
}

export interface DashboardWarehouseCapacityDto {
  warehouseId?: string;
  warehouseName?: string;
  occupiedVolume?: number;
  reservedVolume?: number;
  availableVolume?: number;
  safeMaxVolume?: number;
  capacityPercent?: number;
}
