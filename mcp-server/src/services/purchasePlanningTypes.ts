export interface PurchasePlanningInput {
  targetYear: number;
  targetMonth: number;
  warehouseIds?: string[];
  productIds?: string[];
  forecastMethod: "avg_delivered_last_3_months";
  demandSource: "sales_orders";
  safetyStockDays: number;
  includeRequisitionBacklog: boolean;
  preferredSupplierOnly: boolean;
}

export interface PlanningLine {
  productId: string;
  productCode: string;
  productName: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  forecastDemand: number;
  avgDailyDemand: number;
  safetyStockQuantity: number;
  onHandQuantity: number;
  lockedQuantity: number;
  availableQuantity: number;
  incomingQuantity: number;
  requisitionBacklogQuantity: number;
  netRequiredQuantity: number;
  suggestedOrderQuantity: number;
  recommendedSupplierId: string | null;
  recommendedSupplierName: string | null;
  recommendedUnitId: string | null;
  recommendedUnitName: string | null;
  conversionFactor: number | null;
  leadTimeDays: number | null;
  estimatedUnitPrice: number | null;
  estimatedAmount: number | null;
  reason: string;
  warnings: string[];
}

export interface PurchasePlanningResult {
  summary: {
    targetYear: number;
    targetMonth: number;
    forecastMethod: "avg_delivered_last_3_months";
    demandSource: "sales_orders";
    totalProductsAnalyzed: number;
    totalSuggestedLines: number;
  };
  items: PlanningLine[];
}
